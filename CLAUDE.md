# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Language

모든 응답과 결과는 **한국어**로 작성한다. 코드, 파일 경로, 기술 용어(API 이름, Unity 클래스명 등)는 영어 원문을 유지하되, 설명문·요약·안내 메시지는 모두 한글로 번역하여 출력한다.

## Project Overview

VBike는 Unity 3.5 아케이드 자전거 시뮬레이터를 **Unity 6 (6000.3.11f1)** 으로 마이그레이션한 프로젝트다. Training, BMX, MTB 세 가지 게임 모드와 단일 플레이/UDP 멀티플레이를 지원한다. 실제 아케이드 하드웨어(운동 자전거)는 COM2 시리얼로 통신하지만, `GameData.cs`의 `TEST_MODE = true`(기본값)로 키보드 폴백 가능 — 개발 시 하드웨어 불필요.

## Current Migration Status

**Phase 1 (단일 플레이) — 완료됨:**
- ✅ 레거시 API 컴파일 에러 전부 수정
- ✅ `StateControl.cs`: `Screen.showCursor` → `Cursor.visible`, `Dns.GetHostByName` → `Dns.GetHostEntry`
- ✅ `MoveModule.cs` / `Cycle_Move.cs`: `rigidbody` → `GetComponent<Rigidbody>()` (캐시)
- ✅ `UDPConnection.cs`: `Network.*` 및 `[RPC]` 전부 제거, 커스텀 UDP만 유지
- ✅ `NetworkRigidbody.cs`: 빈 스텁 (Phase 4 재구현 대기)
- ✅ TEST_MODE 키보드 입력 및 물리 안정화 구현
- ✅ `Cycle_Move.cs`: WheelCollider 제거, Kinematic Raycast 이동 시스템 구현
- ✅ 지면 밀착 수정: `_groundOffset` 자동 계산, Pitch 보정, RaycastAll 자기 필터링

**Phase 4 (멀티플레이) — 미착수:**
- ⏳ `Network/BMXMode/`, `Network/MTBMode/` 스크립트: `[RPC]` 대체 메시지 시스템 재구현
- ⏳ `NetworkRigidbody.cs` 완전 재작성
- ⏳ 64-bit `BikeSerial.dll` 포팅 필요 (현재 x86 전용)

## Unity Version Migration Context

**Source:** Unity 3.5 | **Target:** Unity 6 (6000.3.11f1)

그린필드 마이그레이션 — 기존 프로젝트를 Unity 6에서 직접 열지 말고, 새 Unity 6 프로젝트에 에셋을 복사/포팅한다.

### Critical Breaking API Changes

| Old (Unity 3.5) | New (Unity 6) | Notes |
|---|---|---|
| `rigidbody` | `GetComponent<Rigidbody>()` | `Awake`/`Start`에서 캐시 |
| `renderer` | `GetComponent<Renderer>()` | `Awake`/`Start`에서 캐시 |
| `Screen.showCursor = false` | `Cursor.visible = false` | `StateControl.cs` |
| `Application.LoadLevel(name)` | `SceneManager.LoadScene(name)` | `using UnityEngine.SceneManagement` 추가 |
| `Dns.GetHostByName()` | `Dns.GetHostEntry()` + IPv4 필터 | `StateControl.cs`, `UDPConnection.cs` |
| `Network.*` (전체 API) | **제거됨** — Unity 코어에 대체 없음 | 아래 네트워크 섹션 참조 |
| `networkView` | **제거됨** | |
| `[RPC]` attribute | **제거됨** | |
| `RPCMode`, `BitStream`, `NetworkPeerType`, `NetworkPlayer` | **제거됨** | |
| `OnSerializeNetworkView()`, `OnNetworkInstantiate()` | **제거됨** | |

### Legacy Networking Replacement Strategy

Unity 5.1에서 레거시 네트워킹 전체(`Network`, `networkView`, `[RPC]`)가 제거됐다. `UDPConnection.cs`의 커스텀 UDP 브로드캐스트가 서버 탐색 대체층이다. 인게임 RPC 호출(`BMXMode/`, `MTBMode/` 스크립트)은 Phase 4에서 이 UDP 레이어 위의 커스텀 메시지 시스템으로 재구현해야 한다.

**단일 플레이(Phase 1)에서는** 모든 `Network.*` / `networkView.*` 호출을 스텁 또는 `#if` 가드로 처리한다.

## Architecture

### Game State Machine

`StateControl` (싱글톤, `DontDestroyOnLoad`)이 씬 전반에 걸쳐 유지되며 `GameMng` 컴포넌트로 `GameState` 서브클래스 간 전환을 관리한다:

```
StateControl (GameObject, 씬 전환 시 유지)
  └─ GameMng.SetState(typeof(SomeState))
       ├─ OnDeactivate() — 이전 상태 종료
       └─ OnActivate()   — 새 상태 시작
```

`GameMng.m_StartUpdate`가 `false`인 동안은 `Update`/`FixedUpdate` 호출이 차단된다 — 상태 전환 중 안전장치.

`GameState` 서브클래스는 `Script/State/`, `Script/SinglePlay/`, `Script/Training/`, `Script/Network/` 아래에 위치한다.

`GameData.cs`는 순수 정적 필드만 갖는 `MonoBehaviour`로 전역 설정/상태 저장소다 (`TEST_MODE`, `GameState` (0=BMX/1=MTB), `inGame`, 하드웨어 포트 등).

**TEST_MODE에서의 초기화 흐름:**
`StateControl.Start()`가 `TEST_MODE = true`를 감지하면 `return`으로 일반 초기화를 건너뛴다. `TestBootstrap.cs` 컴포넌트가 대신 초기 상태를 설정한다.

### Bike Physics

각 자전거 `GameObject`는 `Cycle_Control`을 루트 컴포넌트로 사용한다. `Start()`에서 나머지 컴포넌트를 프로그래밍 방식으로 추가한다:

```
Cycle_Control (루트 — 상태 플래그 및 웨이포인트 추적)
  ├─ Cycle_Move    (플레이어 물리: 시리얼/키보드 입력, Raycast 기반 이동)
  ├─ Cycle_AI      (AI 물리: 웨이포인트 추종)
  ├─ Cycle_Impact  (충돌 감지, 래그돌 트리거)
  ├─ Cycle_Animation
  └─ Cycle_Smoke
```

`Cycle_Move`와 `Cycle_AI` 모두 `MoveModule`을 상속한다 (`Assets/Bike Assets/Program/Park/Module/MoveModule.cs`). `MoveModule`이 WheelCollider 초기화와 마찰/서스펜션 설정을 담당하지만, `Cycle_Move`에서는 **WheelCollider를 완전히 비활성화**하고 Raycast 기반으로 이동을 대체한다 (Unity 6 PhysX 5.x WheelCollider + MeshCollider 충돌 폭발력 문제).

`Cycle_Control.moveValue` (`MoveValue` 타입)가 컴포넌트 간 공유 데이터 버스다 — 매 프레임 이동값을 쓰고 다른 컴포넌트가 읽는다.

### Cycle_Move Kinematic Raycast 이동 시스템

`Cycle_Move`는 완전 Kinematic Rigidbody (`isKinematic=true`, `useGravity=false`)로 동작한다. `transform.position`을 코드로 직접 제어한다.

**스폰 후 대기:** `START_DELAY(1초)` 동안 kinematic 유지 + `SnapToGround()` 실행 → 이후 정상 주행 (`_ready=true`).

**FixedUpdate 주행 루프 순서:**
```
UpdateAngles() → ApplyKeyboard()/Serial() → CheckJump()
→ RaycastGround() → ApplyMovement() → StabilizeLean() → UpdateSpeed()
```

**RaycastGround():** `RaycastAll` + 자기 transform 필터링으로 앞뒤 지형 감지.
- `upOffset=2.0m`, `rayLen=4.0m` — 급경사 오르막에서도 레이가 지형 위에서 시작
- 자기 Collider 제외 필수: `upOffset` 증가로 레이가 바이크 프레임/안장을 먼저 맞힐 수 있음

**ApplyMovement():** 지면 위치 = `_groundY + _groundOffset * cos(slopeRad)`.
- `_groundOffset` — `Start()`에서 WheelCollider 반경·위치 기반 자동 계산 (`radius - pivotToWheelCenterY`)
- Pitch 보정: 앞뒤 hit 높이 차 → `Atan2` → `euler.x = -slopeDeg` (오르막에서 앞이 올라감)
- 수직: 접지 시 직접 스냅, 공중 시 수동 중력 (`_verticalVel -= 9.8f * dt`)
- Y < -30f 이면 자동 리스폰 (`deadState=2`)

**장애물 처리:**
- `ObstacleCheck()`: 전방 SphereCast (반경 0.4m), `normal.y < 0.5` = 벽으로 판정, 슬라이드 이동
- `PushOutFromWalls()`: OverlapSphere (반경 0.5m) + ClosestPoint 기반 겹침 탈출, `pushDir.y=0` (수직 밀어냄 제외)

**`_pedalLevel`** (범위 `-20` ~ `+20`)이 속도 단계를 결정한다. 실제 속도 = `_pedalLevel × 13f` (최대 ≈ 130 내부 단위 = 100 km/h).

### TEST_MODE Keyboard Controls

| 키 | 동작 |
|---|---|
| ↑ / W | 페달 가속 (+1 단계) |
| ↓ / S | 페달 감속 (-1 단계) |
| ← / A | 좌회전 |
| → / D | 우회전 |
| Q | 점프 (접지 상태) |
| C | 동전 추가 (테스트) |

### Hardware Abstraction

`CBikeSerial.cs`가 `BikeSerial.dll` (x86)을 P/Invoke로 래핑한다. `TEST_MODE = true`이면 DLL 호출을 건너뛰고 `Input.GetAxis` 키보드 입력을 사용한다.

주요 `CBikeSerial` 정적 멤버:
- `FrameBike(frame, heightAngle, maxHandle, env, speed)` — `FixedUpdate`마다 호출
- `FrameLock(deltaTime)` — `Update`마다 호출 (하드웨어 잠금 타이밍)
- `m_fPedalSpeed`, `m_fSteer`, `b1`, `b2` — 입력 상태 읽기

`ZoLock.dll`의 `CheckLock()`은 라이센스 체크다 — 마이그레이션 환경에서 우회 권장.

### Scene Structure

기본 씬 위치: `Assets/Bike Assets/Program/Park/Scene/`
- `Menu.unity` — 메인 메뉴 진입점
- `Map1.unity`, `Map2_01.unity`, `Map2_02.unity`, `Map5.unity` — 플레이 가능 트랙
- `BMXServer.unity`, `Client.unity`, `MTBLobbyServer.unity` — 멀티플레이 (Phase 4)

트랙 3D 메시: `Assets/Bike Assets/3D/Map01/`, `Map02_01/`, `Map02_02/`, `Map04/`

### Cycle_Impact 충돌/크래시 시스템

`deadState`: 0=정상, 1=크래시, 2=리스폰 중. `Cycle_Move`는 `deadState != 0`이면 이동을 완전 중단한다.

**크래시 유예:** 스폰/리스폰 후 `CRASH_GRACE_SEC=15초` 동안 충돌 감지 차단 (`_crashGraceTimer`).

**CapsuleCast 크래시 감지 주의사항:**
- `TerrainCollider`는 항상 크래시 제외 (`is TerrainCollider` 체크)
- 법선 `normal.y > 0.5f` 인 경사면 MeshCollider도 크래시 제외 (지형 오탐 방지)

### WheelCollider Friction (Cycle_AI 전용)

`Cycle_AI`는 `MoveModule.Move()`를 통해 WheelCollider 기반 물리를 사용한다. `WheelFrictionCurve` API는 값 타입(struct)이므로 반드시 복사 → 수정 → 재대입해야 한다. `Cycle_Move`에서는 WheelCollider가 비활성화되므로 해당 없음.

### Waypoint System

`WaypointDefine`이 트랙 경로를 `allways[]` (Transform 배열)로 저장하며 최대 5개의 분기 교차로(`crossways_1` ~ `crossways_5`)를 지원한다. `Cycle_Control.FindNextStep()`과 `FindClosePoint()`가 이 시스템을 탐색한다. 웨이포인트 GameObject 이름은 `Cycle_Control.wayName` (기본값 `"_Waypoint"`)에 저장된다.

## Migration Priority Order

1. ✅ **컴파일 에러 수정** — 위 API 대체 완료
2. ✅ **단일 플레이 Training 모드** — 네트워킹 없는 가장 단순한 경로
3. ✅ **단일 플레이 BMX/MTB** — 순위/결과 로직 포함
4. ⏳ **멀티플레이** — 기존 UDP 위의 커스텀 RPC 대체 필요

## Key Files

| 파일 | 마이그레이션 난이도 | 비고 |
|---|---|---|
| `Cycle_Control.cs` | High | `rigidbody`, `renderer`, 레거시 Network/RPC 사용 |
| `StateControl.cs` | **완료** | `Cursor.visible`, `Dns.GetHostEntry` 적용 완료 |
| `UDPConnection.cs` | **완료** | `Network.*` 제거, 커스텀 UDP만 유지 |
| `NetworkRigidbody.cs` | **스텁** | Phase 4 재작성 대기 |
| `MoveModule.cs` | **완료** | `_rb = GetComponent<Rigidbody>()` 캐시 적용 |
| `Cycle_Move.cs` | **완료** | Kinematic Raycast 이동, WheelCollider 비활성, Pitch 보정, 지면 스냅 |
| `Network/BMXMode/` 스크립트 | High | `[RPC]` 다수 사용 — Phase 4 |
| `Network/MTBMode/` 스크립트 | High | `[RPC]` 다수 사용 — Phase 4 |
| `GameData.cs` | **없음** | 순수 데이터 클래스, 레거시 API 없음 |
| `GameMng.cs` | **없음** | 레거시 API 없음 |
| `Training_*.cs`, `BMX_S_*.cs`, `MTB_S_*.cs` | Low | 네트워킹 없음 또는 최소 |

## Native Plugins

`Assets/plugins/` — Windows x86 DLL (32-bit):
- `BikeSerial.dll` — 하드웨어 자전거 인터페이스 (TEST_MODE에서 안전하게 스킵)
- `ZoLock.dll` — 라이센스 체크 (`CheckLock()`) — 마이그레이션 시 우회 권장
- `EasyPOD.dll`, `io.dll` — 현재 활발히 사용되지 않는 것으로 보임

Unity 6 64-bit 빌드 시 Inspector에서 플러그인 플랫폼 타겟을 `x86_64`로 설정해야 한다.

### BikeSerial.dll 64-bit 빌드

`D:\자전거SW\자전거 소스\HNS\BikeSerial.zip`에 C++ 소스 프로젝트가 있다 (Visual Studio 2017, `.vcxproj`). 이 프로젝트를 x64 Release로 빌드하면 64-bit `BikeSerial.dll`을 생성할 수 있다. zip 내 `Release/BikeSerial.dll` (19,456 bytes)가 이미 빌드된 버전으로 현재 플러그인(17,408 bytes)과 다르다 — x64 또는 최신 빌드일 수 있으므로 테스트 후 교체 고려.

### BikeSerial DLL API (전체 exported 함수)

`CBikeSerial.cs`에서 P/Invoke로 호출하는 함수 전체 목록 (소스 기준):

| 함수 | 반환형 | 설명 |
|---|---|---|
| `InitBikeSerial()` | void | 시리얼 초기화 |
| `OpenBikeSerial(char* port)` | bool | COM 포트 열기 |
| `EndBikeSerial()` | void | 포트 닫기 |
| `GetTilt()` | int | 기울기 센서값 |
| `GetButton(int i)` | int | 버튼 상태 (인덱스 0~3) |
| `GetBreak(int i)` | int | 브레이크 상태 (인덱스 0~1) |
| `GetSpeed()` | float | 페달 속도 |
| `GetHandle()` | int | 핸들 방향값 |
| `GetBill()` | int | 동전 투입 여부 |
| `Send(char c)` | void | 하드웨어로 명령 전송 |
| `GetPos()` | int | 위치 |
| `GetJoystick1(int i)` / `GetJoystick2(int i)` | int | 조이스틱 (인덱스 0~5) |
| `GetSensorUDM()` / `GetSensorLRM()` / `GetSensorFRM()` | int | 거리 센서 |
| `GetSensorRot()` / `GetSensorLR()` | int | 회전/좌우 센서 |

시리얼 프로토콜: 헤더 바이트 + 데이터 길이 기반 패킷. 하드웨어로 보내는 명령은 단일 char (`'D'`=기울임다운, `'U'`=기울임업, `'C'`=중립, `'A'`=각도전송, `'B'`=브레이크, `'E'`=속도, `'R'`=리셋).

## Config.ini

`Assets/StreamingAssets/Config.ini`에서 런타임 설정을 읽는다 (`INIFileParser.dll` 사용). `D:\자전거SW\자전거 소스\HNS\바이크 설정.txt`가 각 항목의 원본 설명이다.

```ini
[Option]
3D = 0              ; 0=2D, 1=3D
FreeMode = 1        ; 0=유료모드, 1=무료모드
Total_Coin = 0      ; 현재 코인 수
OneGameCoin = 1     ; 게임당 소비 코인
Difficult = 4       ; 난이도
Training_Time = 10  ; 훈련모드 시간(분)
AutoMode = 1        ; 0=데모모드 OFF, 1=ON
AutoModeSpeed = 5   ; 데모모드 속도

[Hardware]
Port = COM2         ; 시리얼 포트
Speed1 = 40         ; 기울임 모터 속도1
Speed2 = 80         ; 기울임 모터 속도2
MotorSpeed = 0      ; 저항모터 (클수록 페달링 변화 속도 감소)
RFID = 0            ; RFID 기능 0/1
Tong = 2.0          ; 통 시간
JButton = B         ; 점프 버튼
JSpeed = 2.0        ; 점프 속도
handleLeft = 20     ; 핸들 최솟값
handleRight = 28    ; 핸들 최댓값
handleGap = 1       ; 핸들 이동 간격
handleIgnore = 0    ; 무시 각도 (1이면 2~4도 무시)
heightDir = -1      ; 높이 방향 (1=올라, -1=반대)
pedalspeed = 1      ; 페달 속도 배수

[Network]
BMX_Server_Ip = 192.168.0.110
BMX_Server_WaitTime = 60  ; 서버 대기 시간(초)
Use_Server = 0      ; 서버 사용 여부 0/1
Use_Network = 0     ; 네트워크 사용 여부 0/1
ServerNoTime = 0    ; 서버 없을 때 바로 게임 여부 0/1
```

## Third-Party Assets

- **EasyRoads3D** — 도로 메시 생성; Unity 6 호환 버전 확인 필요
- **Standard Assets** (`Character Controllers`) — 레거시; Unity 내장 대체 또는 제거 권장
- `INIFileParser.dll` (`Assets/_Program/`) — `.ini` 파일 읽기 (config 스크립트에서 사용)

## Documentation

`docs/` 디렉토리에 마이그레이션 계획·설계·분석·보고 문서가 있다:
- `docs/01-plan/features/` — 기능별 계획
- `docs/02-design/features/` — 기능별 설계
- `docs/03-analysis/` — 분석 문서
- `docs/04-report/features/` — 보고 문서
