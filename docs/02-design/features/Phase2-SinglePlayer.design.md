# [Design] Phase 2 — 단일플레이어 동작 검증

**Feature**: Phase2-SinglePlayer
**Created**: 2026-03-17
**Phase**: Design
**Reference Plan**: [Phase2-SinglePlayer.plan.md](../../01-plan/features/Phase2-SinglePlayer.plan.md)

---

## 1. 범위 및 목표

Phase 1에서 소스 코드 컴파일 에러를 전량 수정했다. Phase 2의 목표는:

1. Unity 6 신규 프로젝트에서 소스 코드가 컴파일되는지 확인
2. 씬 이름 불일치 코드 수정 (`GameData.map[]`)
3. Build Settings에 모든 씬 등록
4. 씬 내 `GUITexture` 컴포넌트를 `Canvas + RawImage`로 교체
5. Training / BMX Single / MTB Single 모드 플레이 가능 상태 달성

---

## 2. 코드 수정 설계

### 2.1 GameData.cs — 씬 이름 불일치 수정 (Priority: Critical)

**파일**: `Assets/Bike Assets/Program/Park/Script/Control/GameData.cs:14`

**현재 상태**:
```csharp
public static string[] map = new string[] { "MTB001", "MTB002", "MTB003", "Map5" };
```

**수정 후**:
```csharp
public static string[] map = new string[] { "Map1", "Map2_01", "Map2_02", "Map5" };
```

**영향 분석**:

| 사용 위치 | 코드 | 사용 인덱스 | 현재→수정 후 |
|-----------|------|-------------|-------------|
| `MTB_S_InGame.cs:85` | `SceneManager.LoadSceneAsync(GameData.map[GameData.MTBMap])` | 0, 1, 2 | "MTB001"→"Map1", "MTB002"→"Map2_01", "MTB003"→"Map2_02" |
| `BMX_S_InGame.cs:84` | `SceneManager.LoadSceneAsync(GameData.map[3])` | 3 (고정) | "Map5" (변경 없음 ✓) |
| `RankData.cs` | `GameData.map[...]` 사용 여부 확인 | 확인 필요 | — |

**BGM 인덱스 영향 없음**:
- MTB: `AudioCtr.snd_bgm[GameData.MTBMap + 1]` — MTBMap=0,1,2 → snd_bgm[1,2,3] (map[] 변경과 무관)
- BMX: `AudioCtr.snd_bgm[GameData.BMXMap]` — BMXMap=1,2,3 설정됨 (map[] 변경과 무관)

### 2.2 그 외 코드 수정

Phase 1에서 모든 컴파일 에러를 수정했으므로 추가 코드 수정은 **없음**.

단, Unity 6에서 실제 임포트 후 런타임 오류 발생 시 그 자리에서 수정:
- `Standard Assets` 컴파일 오류 → 사용 중인 파일만 유지, 나머지 제거
- `EasyRoads3D` 버전 비호환 오류 → 도구 메뉴에서 재빌드 시도 또는 도로 메시 베이크 결과물만 유지

---

## 3. Unity 6 프로젝트 설정 설계

### 3.1 프로젝트 생성 사양

```
Unity Hub → New Project
├── Template: 3D (Core) — Built-in Render Pipeline  ← URP/HDRP 아님!
├── Unity Version: 6000.x.x LTS 최신
├── Project Name: VBike_Unity6
└── Location: 원하는 경로
```

> Built-in RP 선택 이유: 기존 셰이더가 모두 Built-in용 (`Diffuse`, `Transparent/Cutout/Diffuse` 등). URP로 만들면 핑크 머티리얼 전면 발생.

### 3.2 Assets 복사 절차

```
[원본] VBike/Assets/  →  [대상] VBike_Unity6/Assets/
```

복사 대상 (전체 폴더):
```
Assets/
├── Bike Assets/         ← 게임 코어 (씬, 스크립트, 3D, 텍스처 포함)
├── Scene/               ← Training 씬 (Demo02, Demo03, Tracking03)
├── _Program/            ← 유틸리티 스크립트, 에디터 도구
├── EasyRoads3D/         ← 도로 메시 생성 도구
├── Standard Assets/     ← 레거시 (CharacterController 등)
├── TB v2 Assets/        ← 지형 브러시
├── WaypointScript/      ← 웨이포인트 시스템
├── plugins/             ← DLL (BikeSerial.dll 등)
└── StreamingAssets/     ← 있는 경우 복사
```

**복사하지 않을 것**:
```
ProjectSettings/    ← Unity 6가 새로 생성한 것 사용
Library/            ← Unity가 자동 재생성
Temp/               ← 빌드 임시 파일
```

### 3.3 Project Settings 구성

**Edit > Project Settings > Player**:

| 설정 항목 | 값 |
|-----------|-----|
| Scripting Backend | Mono (IL2CPP는 Phase 3에서 고려) |
| API Compatibility Level | .NET Standard 2.1 |
| Allow Unsafe Code | ✓ (BikeSerial P/Invoke 필요) |
| Architecture | x86_64 |
| Default Screen Width | 1280 |
| Default Screen Height | 1024 |
| Default Is Full Screen | ✓ |
| Run In Background | ✓ (멀티플레이어 Phase 4 대비) |

**Edit > Project Settings > Physics**:

| 설정 항목 | 값 | 이유 |
|-----------|-----|------|
| Default Solver Iterations | 6 (기본) | WheelCollider 안정성 |
| Default Solver Velocity Iterations | 1 (기본) | — |
| Bounce Threshold | 2 | 자전거 통통 튀는 현상 방지 |

### 3.4 Plugin DLL Inspector 설정

각 DLL 파일 선택 → Inspector에서:

**BikeSerial.dll**:
```
Include Platforms: ✓ Editor, ✓ Standalone
CPU: x86_64
```

**ZoLock.dll**:
```
Include Platforms: ✓ Editor, ✓ Standalone
CPU: x86_64
```
> `ZoLock.dll` — 라이선스 체크용. `GameData.TEST_MODE = true` 상태에서는 `CheckLock()` 반환값을 무시하므로 개발 중 문제 없음.

**EasyPOD.dll, io.dll**:
```
Include Platforms: 모두 체크 해제 (미사용 확인 후)
```
> Grep으로 `EasyPOD`, `io.dll` 임포트 여부 확인 후 결정.

---

## 4. Build Settings 씬 등록 설계

**File > Build Settings** 등록 목록:

| Index | 씬 이름 (SceneManager에서 사용하는 이름) | 파일 경로 | 용도 |
|-------|----------------------------------------|-----------|------|
| 0 | `Menu` | `Assets/Bike Assets/Program/Park/Scene/Menu.unity` | 메인 메뉴 |
| 1 | `Map1` | `Assets/Bike Assets/Program/Park/Scene/Map1.unity` | MTB Map0 |
| 2 | `Map2_01` | `Assets/Bike Assets/Program/Park/Scene/Map2_01.unity` | MTB Map1 |
| 3 | `Map2_02` | `Assets/Bike Assets/Program/Park/Scene/Map2_02.unity` | MTB Map2 |
| 4 | `Map5` | `Assets/Bike Assets/Program/Park/Scene/Map5.unity` | BMX |
| 5 | `Demo02` | `Assets/Scene/Demo02.unity` | Training Map0 |
| 6 | `Demo03` | `Assets/Scene/Demo03.unity` | Training Map1 |
| 7 | `Tracking03` | `Assets/Scene/Tracking03.unity` | Training Map2 |

> `SceneManager.LoadScene(name)` 는 Build Index가 아닌 **씬 이름** 기준이므로, 순서보다 이름이 중요. 단, Index 0 = 시작 씬이므로 Menu가 0번이어야 함.

**멀티플레이어 씬** (Phase 4 전까지 Disabled 상태로만 등록):
- `BMXServer` — `Assets/Bike Assets/Program/Park/Scene/BMXServer.unity`
- `Client` — `Assets/Bike Assets/Program/Park/Scene/Client.unity`
- `MTBLobbyServer` — `Assets/Bike Assets/Program/Park/Scene/MTBLobbyServer.unity`

---

## 5. GUITexture → Canvas + RawImage 씬 재작업 설계

### 5.1 왜 씬 재작업이 필요한가

- Phase 1에서 **스크립트의 `GUITexture` 타입**은 `RawImage`로 수정 완료
- 그러나 Unity 씬(.unity) 내부의 **GUITexture 컴포넌트**는 Unity 6에서 인식 불가 → `NullReferenceException`

코드의 현재 상태 예시 (이미 수정됨):
```csharp
// MainMenu.cs:93 — 이미 RawImage 사용
RawImage[] allGUI = GetComponentsInChildren<RawImage>();

// BMX_Champ.cs:95 — 이미 RawImage 사용
countdownAni[0].GetComponent<RawImage>().texture = ...;
```

씬 파일 내부 (수동 수정 필요):
```
GUITexture 컴포넌트가 붙은 GameObject
  ↓ 이것을 Canvas + RawImage 구조로 교체
Canvas (Screen Space - Overlay)
  └── GameObject (RawImage 컴포넌트)
```

### 5.2 영향 받는 씬 및 오브젝트

| 씬 | 오브젝트명 | GUITexture 용도 | 스크립트 참조 |
|----|-----------|----------------|--------------|
| `Menu.unity` | `insertcoin` 하위 오브젝트들 | 크레딧 표시 (숫자 텍스처) | `MainMenu.cs` — `credit000[]` |
| `Map5.unity` (BMX) | `countdownAni[0]` 참조 오브젝트 | 카운트다운 텍스처 | `BMX_Champ.cs` |
| `Map1.unity` (MTB) | `countdownAni[0]` 참조 오브젝트 | 카운트다운 텍스처 | `MTB_Champ.cs` |
| `Map2_01.unity` (MTB) | 동일 | 동일 | `MTB_Champ.cs` |
| `Map2_02.unity` (MTB) | 동일 | 동일 | `MTB_Champ.cs` |

### 5.3 교체 절차 (씬별 동일)

각 씬에서 GUITexture 컴포넌트가 있는 GameObject마다:

```
Step A: GameObject 선택
Step B: Inspector에서 GUITexture 컴포넌트 확인 → 설정된 Texture 값 메모
Step C: [Add Component] → UI > Canvas (Screen Space - Overlay 모드로 설정)
         (이미 Canvas가 씬에 있는 경우 해당 Canvas 하위로 이동)
Step D: [Add Component] → UI > Raw Image
Step E: Raw Image의 Texture 필드에 Step B에서 메모한 텍스처 할당
Step F: GUITexture 컴포넌트 우클릭 → Remove Component
Step G: 씬 저장
```

> **주의**: `GetComponentsInChildren<RawImage>()` 패턴 (MainMenu.cs, MTB_Champ.cs)은 해당 GameObject의 자식에서 RawImage를 찾는다. Canvas가 부모가 되면 계층이 바뀌므로, **원래 부모-자식 관계를 유지**해야 한다.

### 5.4 Canvas 없이 단순화하는 대안

GUITexture는 원래 월드 공간이 아닌 스크린 픽셀 좌표에 직접 그렸다. Unity 6에서 가장 유사한 방식:

```
방법 1 (권장): Canvas (Screen Space - Overlay) + RawImage
  - 장점: 픽셀 좌표 그대로 사용 가능
  - 단점: Canvas 계층 설정 필요

방법 2: 3D Quad + MeshRenderer + 카메라 고정
  - 단점: 복잡, 비권장

→ 방법 1 (Canvas + RawImage) 적용
```

---

## 6. 씬 임포트 전략 설계

### 6.1 임포트 순서 (위험도 낮은 것부터)

```
1번: Menu.unity       — 가장 단순, 3D 씬 없음. GUI 오류 확인
2번: Demo02.unity     — Training 씬, 지형만 있음
3번: Map1.unity       — MTB 레이싱 (가장 복잡)
4번: Map5.unity       — BMX 씬
5번: Map2_01, Map2_02 — MTB 추가 맵
```

### 6.2 씬 열기 후 확인 사항

각 씬 열기 후 Console에서 확인:
```
□ 경고/에러 수 확인 (에러 0개 목표, 경고는 허용)
□ Missing Script 경고: Phase 4 멀티 스크립트 참조 (무시 가능)
□ Missing Reference: 프리팹/텍스처 참조 누락 → 수동 재연결 필요
□ EasyRoads3D 관련 오류: Version 비호환 → 섹션 6.3 참고
```

### 6.3 EasyRoads3D 대응

Unity 6와 호환되는 EasyRoads3D 버전이 있으면 업데이트. 없으면:
```
옵션 A: EasyRoads3D 메시가 이미 베이크된 경우 → 스크립트만 제거해도 비주얼 유지
옵션 B: 도로 메시 재생성 필요 시 → EasyRoads3D Unity 6 버전 구매/업그레이드
옵션 C 임시 우회: EasyRoads3D 폴더 전체를 Plugins/ 밖으로 이동 → 런타임 오류 방지,
         도로 비주얼은 손실되지만 물리 테스트 가능
```

### 6.4 씬 YAML 손상 시 비상 대응

씬이 열리지 않거나 핵심 오브젝트 누락 시:

```csharp
// 최소 실행 가능 씬 재구성 순서
1. 빈 씬 (New Scene) 생성
2. Resources.Load로 로드되는 프리팹들 확인:
   - "Main_menu"      (Menu_SelectGame.cs:112)
   - "Prefeb/Loading_MTB" (Menu_SelectGame.cs:334)
   - "Prefeb/Loading_BMX" (Menu_SelectGame.cs:344)
   - "Prefeb/Cycle X" (BMX_S_Data.cs, MTB_S_Data.cs)
   - "Game"           (BMX_S_Data.cs:69, MTB_S_Data.cs:62)
3. StateControl 프리팹 배치 (DontDestroyOnLoad 오브젝트)
4. 지형 메시는 FBX에서 재임포트
```

---

## 7. 게임 흐름 및 씬 로드 맵핑

```
[초기 씬: Menu.unity]
     │
     ▼
Menu_SelectGame.OnActivate()
  → SceneManager.LoadSceneAsync("Menu")   [이미 Menu.unity 안에 있어도 재로드]
  → Resources.Load("Main_menu") 프리팹 인스턴스화
     │
     ├── Training 선택 → Training_InGame.Activate()
     │     → SceneManager.LoadSceneAsync("Demo02" / "Demo03" / "Tracking03")
     │
     ├── MTB Single 선택 → MTB_S_InGame.Activate()
     │     → SceneManager.LoadSceneAsync(GameData.map[GameData.MTBMap])
     │           map[0]="Map1", map[1]="Map2_01", map[2]="Map2_02"  ← 이것이 수정 후
     │
     └── BMX Single 선택 → BMX_S_InGame.Activate()
           → SceneManager.LoadSceneAsync(GameData.map[3])
                 map[3]="Map5"  ← 변경 없음, 이미 정상
```

**결과 후 복귀**:
```
Training_InGame → Menu_SelectGame (resultTime 경과 후)
BMX_S_Result   → Menu_SelectGame
MTB_S_Result   → Menu_SelectGame
```

---

## 8. 테스트 설계

### 8.1 컴파일 검증

Unity 6에서 Assets 복사 후 콘솔 확인:
```
목표: 0 Errors (Warnings 허용)
확인 패턴:
  □ 컴파일 에러 없음
  □ CS0618 (deprecated) 경고 — Phase 1에서 처리함, 있으면 누락 파일
  □ CS0246 (type not found) — 플러그인 문제
```

### 8.2 Training 모드 테스트 절차

```
1. Play Mode 진입 (Menu.unity 열린 상태)
2. Console 에러 없는지 확인
3. Space 키 → Training 선택 → Map 선택 → Space 확인
4. Demo02 씬 로드 확인
5. W키 페달 → 자전거 전진 확인
6. A/D키 조향 확인
7. 타이머 작동 확인
8. 타이머 만료 → 결과 화면 전환 확인
9. 결과 화면 → Menu 복귀 확인
```

### 8.3 BMX Single 테스트 절차

```
1. Menu에서 BMX → Single → Space
2. Map5 로드 확인
3. 카운트다운 (countdown_0, countdown_1...) 텍스처 표시 확인
4. W키 전진, 점프대 점프 확인
5. 체크포인트 통과 순위 UI 확인
6. 결승선 → BMX_S_Result 화면 확인
7. 결과 후 Menu 복귀 확인
```

### 8.4 MTB Single 테스트 절차

```
1. Menu에서 MTB → Map1 → Space
2. Map1 로드 확인 (GameData.map[0]="Map1")
3. 자전거 물리 동작 확인 (오르막/내리막)
4. CRollingStone 장애물 동작 확인
5. 결승선 → MTB_S_Result 화면 확인
6. Map2_01, Map2_02도 동일하게 테스트
```

### 8.5 공통 런타임 에러 체크

| 에러 패턴 | 원인 | 대응 |
|-----------|------|------|
| `Scene 'MTB001' couldn't be loaded` | GameData.map[] 미수정 또는 Build Settings 미등록 | Section 2.1 수정 + Section 4 등록 |
| `NullReferenceException` in GUI | GUITexture 컴포넌트 없음 | Section 5 씬 재작업 |
| `MissingReferenceException: _Startpoint` | 씬 YAML 손상으로 StartPoint 없음 | 프리팹 재배치 |
| `DllNotFoundException: BikeSerial` | DLL Inspector 미설정 | Section 3.4, TEST_MODE=true 확인 |
| `Object reference not set` in `CBikeSerial` | TEST_MODE=false | `GameData.cs:42` → `TEST_MODE = true` 확인 |
| WheelCollider 자전거 튕김 | Unity6 WheelCollider Physics 파라미터 변화 | `MoveModule.cs` stiffness 값 0.8→0.5 시도 |

---

## 9. 구현 순서 (Do Phase)

```
[Step 1] GameData.cs 씬 이름 수정 (코드 1줄)
         map[] = {"Map1", "Map2_01", "Map2_02", "Map5"}

[Step 2] Unity Hub → Unity 6 프로젝트 생성 (Built-in RP)

[Step 3] Assets 폴더 복사 (ProjectSettings 제외)

[Step 4] Unity 6에서 프로젝트 열기
         → 콘솔 에러 수 확인 및 기록

[Step 5] Project Settings 구성 (Section 3.3)

[Step 6] Plugin DLL Inspector 설정 (Section 3.4)

[Step 7] Build Settings 씬 등록 (Section 4 목록 순서대로)

[Step 8] Menu.unity 열기 → Play → 콘솔 에러 수정
         → 1차 오류 수집 및 수정

[Step 9] GUITexture → Canvas + RawImage 씬별 교체 (Section 5)
         순서: Menu.unity → Map5.unity → Map1.unity → Map2_01 → Map2_02

[Step 10] Training 모드 플레이테스트 (Section 8.2)
          → 오류 수정

[Step 11] BMX Single 플레이테스트 (Section 8.3)
          → 오류 수정

[Step 12] MTB Single 플레이테스트 (Section 8.4)
          → 오류 수정

[Step 13] 전체 루프 (Menu → Game → Result → Menu) 반복 확인
```

---

## 10. 완료 기준 (Definition of Done)

| 항목 | 기준 |
|------|------|
| 컴파일 | Unity 6 Console: Error 0개 |
| Training | Demo02에서 타이머 종료, 결과 화면, Menu 복귀 |
| BMX Single | Map5에서 결승선 통과, 결과 화면, Menu 복귀 |
| MTB Single | Map1에서 결승선 통과, 결과 화면, Menu 복귀 |
| 물리 | WheelCollider 점프/착지/충돌/라그돌 Exception 없음 |
| UI | 카운트다운, 속도/칼로리/거리 HUD 표시 |
| 씬 전환 | 전체 루프 3회 연속 Exception 없음 |
