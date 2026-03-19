# [Plan] Phase 2 — 단일플레이어 동작 검증

**Feature**: Phase2-SinglePlayer
**Created**: 2026-03-17
**Phase**: Plan
**Prerequisites**: Phase 1 완료 (컴파일 에러 0개) ✅
**Reference**: [Unity6마이그레이션 Plan](./Unity6마이그레이션.plan.md)

---

## Executive Summary

| 항목 | 내용 |
|------|------|
| Feature | Phase 2 — Unity 6 단일플레이어 동작 검증 |
| 시작일 | 2026-03-17 |
| 목표 | Unity 6 프로젝트에서 Training / BMX / MTB 단일플레이를 처음부터 끝까지 플레이 가능한 상태 달성 |

### Value Delivered (4-Perspective)

| 관점 | 내용 |
|------|------|
| **Problem** | Phase 1에서 소스코드 컴파일 오류는 전량 수정했지만, 아직 Unity 6 프로젝트가 존재하지 않음. 씬 임포트, 씬 이름 불일치, WheelCollider 물리 차이, GUITexture→RawImage 전환 등 실제 동작까지 추가 작업이 필요. |
| **Solution** | Unity 6 신규 프로젝트를 생성하고 Assets 복사, 씬 이름 불일치 수정(`GameData.map[]`), 씬 임포트 후 발생하는 런타임 오류를 체계적으로 수정. TEST_MODE=true 키보드 폴백으로 하드웨어 없이 전체 게임 플로우 테스트. |
| **Function & UX Effect** | Training/BMX/MTB 모드가 Unity 6에서 정상 동작. 메뉴 → 게임 → 결과 화면 전체 플로우 플레이 가능. 자전거 물리(주행/조향/점프/충돌)가 원본과 동등하게 동작. |
| **Core Value** | 실제 동작하는 Unity 6 빌드 달성으로 이후 기능 추가 및 렌더링 수정(Phase 3)의 안정적 기반 확보. |

---

## 1. 현황 분석

### 1.1 Phase 1 완료 상태

- 소스 코드 컴파일 에러: **0개** ✅
- 수정된 파일: ~34개
- 아직 수행되지 않은 것: Unity 6 프로젝트 생성, 씬 임포트, 실제 게임 플레이 테스트

### 1.2 Phase 2 진입 전 알려진 문제점

| 문제 | 심각도 | 세부 내용 |
|------|--------|-----------|
| **씬 이름 불일치** | Critical | `GameData.map[]`=`{"MTB001","MTB002","MTB003","Map5"}` 지만 실제 씬 파일은 `Map1.unity`, `Map2_01.unity`, `Map2_02.unity`, `Map5.unity` |
| **Training 씬 경로** | Medium | `Training_InGame.cs`가 `"Demo02"`, `"Demo03"`, `"Tracking03"`을 로드. 실제 파일: `Assets/Scene/Demo02.unity` 등 — 빌드 세팅 등록 필요 |
| **GUITexture 컴포넌트** | Medium | 코드는 `RawImage`로 수정했으나 씬의 `GUITexture` 컴포넌트는 Unity 6에서 없어진 컴포넌트이므로 씬 재구성 필요 |
| **Unity 3.5 씬 YAML** | High | Unity 3.5 `.unity` 파일을 Unity 6에서 직접 열면 컴포넌트 GUID 불일치로 손상 가능 |
| **WheelCollider 물리** | Low | Unity 버전 차이로 자전거 물리 튜닝 필요할 수 있음 |
| **EasyRoads3D** | Medium | Unity 6 호환 여부 불명 — 도로 렌더링 확인 필요 |

---

## 2. 씬 이름 불일치 수정 계획

**가장 중요한 Phase 2 작업** — 이것이 해결되지 않으면 어떤 게임 모드도 로드 불가.

### 2.1 현재 상태

```
GameData.map[] = {"MTB001", "MTB002", "MTB003", "Map5"}
                    ↓         ↓         ↓         ↓
실제 씬 파일:    Map1.unity  Map2_01  Map2_02  Map5.unity ← Map5만 일치!
```

### 2.2 수정 옵션

**옵션 A (권장): GameData.map[] 값을 실제 씬 이름에 맞게 수정**

```csharp
// GameData.cs 수정
public static string[] map = new string[] { "Map1", "Map2_01", "Map2_02", "Map5" };
```

- 장점: 씬 파일 이름 변경 불필요, 코드 한 곳만 수정
- 주의: `GameData.MTBMap` 인덱스(0,1,2)와 BGM 인덱스(`snd_bgm[GameData.MTBMap+1]`) 확인 필요

**옵션 B: 씬 파일을 GameData 이름에 맞게 복사/재명명**

- Unity에서 씬 파일 이름 변경 시 GUID 유지 가능하나 작업 복잡
- 비권장

### 2.3 Training 씬 등록

`Training_InGame.cs`가 로드하는 씬들을 Build Settings에 추가:
```
Assets/Scene/Demo02.unity    → "Demo02"
Assets/Scene/Demo03.unity    → "Demo03"
Assets/Scene/Tracking03.unity → "Tracking03"
```

---

## 3. Unity 6 프로젝트 생성 절차

### 3.1 신규 프로젝트 생성

```
1. Unity Hub → New Project
2. Template: 3D (Built-in Render Pipeline)  ← Universal RP가 아닌 Built-in RP 선택 필수
3. Unity Version: 6.x.x (최신 LTS 권장)
4. Project Name: VBike_Unity6
5. Location: (원하는 위치)
```

### 3.2 Assets 복사

```
VBike/ (기존 Unity 3.5 소스)
├── Assets/               ← 이 폴더 전체를 VBike_Unity6/Assets/ 에 복사
│   ├── Bike Assets/
│   ├── Scene/            ← Training 씬 포함
│   ├── _Program/
│   ├── EasyRoads3D/
│   ├── Standard Assets/
│   ├── TB v2 Assets/
│   ├── WaypointScript/
│   └── plugins/          ← DLL 포함
```

> **주의**: ProjectSettings 폴더는 복사하지 말 것. Unity 6이 자체 생성한 것을 사용.

### 3.3 프로젝트 설정

```
Edit > Project Settings > Player
├── Other Settings
│   ├── Scripting Runtime Version: .NET Standard 2.1
│   ├── API Compatibility Level: .NET Standard 2.1
│   ├── Allow unsafe Code: ✓
│   └── Architecture: x86_64
└── Resolution and Presentation
    └── Default Is Full Screen: ✓ (아케이드 모드)
```

### 3.4 Plugin 설정

각 DLL 선택 → Inspector:
```
BikeSerial.dll:
  ├── Include Platforms: Editor, Standalone
  └── CPU: x86_64

ZoLock.dll:
  ├── Include Platforms: Editor, Standalone
  └── CPU: x86_64

EasyPOD.dll, io.dll: 미사용 확인 시 Exclude 처리
```

### 3.5 Build Settings 씬 등록

```
File > Build Settings > Add Open Scenes (또는 드래그)

등록 순서 및 이름:
Index 0: Menu         (Assets/Bike Assets/Program/Park/Scene/Menu.unity)
Index 1: Map1         (Assets/Bike Assets/Program/Park/Scene/Map1.unity)   ← MTBMap=0
Index 2: Map2_01      (Assets/Bike Assets/Program/Park/Scene/Map2_01.unity) ← MTBMap=1
Index 3: Map2_02      (Assets/Bike Assets/Program/Park/Scene/Map2_02.unity) ← MTBMap=2
Index 4: Map5         (Assets/Bike Assets/Program/Park/Scene/Map5.unity)   ← BMX
Index 5: Demo02       (Assets/Scene/Demo02.unity)                           ← Training 0
Index 6: Demo03       (Assets/Scene/Demo03.unity)                           ← Training 1
Index 7: Tracking03   (Assets/Scene/Tracking03.unity)                      ← Training 2
Index 8: BMXServer    (multiplayer — 나중에 추가)
Index 9: Client       (multiplayer — 나중에 추가)
Index 10: MTBLobbyServer (multiplayer — 나중에 추가)
```

> 씬 Build Index는 `SceneManager.LoadScene(name)` 사용 시 이름 기반이므로
> 순서보다 **이름 일치**가 중요.

---

## 4. GameData.cs 수정 (씬 이름 불일치 수정)

```csharp
// Assets/Bike Assets/Program/Park/Script/Control/GameData.cs
// Before
public static string[] map = new string[] { "MTB001", "MTB002", "MTB003", "Map5" };

// After (실제 씬 파일명과 일치)
public static string[] map = new string[] { "Map1", "Map2_01", "Map2_02", "Map5" };
```

---

## 5. 씬 임포트 전략

### 5.1 Unity 3.5 씬 YAML 호환성

Unity 3.5의 `.unity` 파일은 구형 YAML 포맷. Unity 6에서 열 때:
- **최선의 경우**: 경고는 나지만 열림 (오브젝트 손상 없음)
- **최악의 경우**: 참조 누락, 컴포넌트 GUID 불일치로 빈 씬

### 5.2 씬 임포트 우선순위

```
1단계: Menu.unity — 가장 단순한 씬, 먼저 열어 Console 확인
2단계: Demo02.unity — Training 씬 (지형만 있는 단순 씬)
3단계: Map1.unity — MTB 레이싱 씬
4단계: Map5.unity — BMX 씬
```

### 5.3 씬 손상 시 대응

씬 열기 실패 또는 치명적 오류 시:
```
1. 빈 씬(New Scene)에서 시작
2. 해당 씬의 핵심 프리팹을 씬에 드래그
   - _Manager.prefab (StateControl, GameMng 포함)
   - 지형 메시 오브젝트
   - _Waypoint.prefab, _Startpoint.prefab
3. Lighting 설정: Window > Rendering > Lighting > Generate Lighting
4. 씬 저장
```

---

## 6. GUITexture 씬 재작업 계획

코드는 `RawImage`로 수정했으나 씬 내 `GUITexture` 컴포넌트는 Unity 6에서 null 참조.

### 6.1 영향 범위

| 씬 | GUITexture 사용 위치 |
|----|---------------------|
| Menu.unity | 크레딧 표시 (insertcoin 오브젝트) |
| Map1~Map5 | BMX_Champ, MTB_Champ의 카운트다운 UI |

### 6.2 수정 방법

각 GUITexture 컴포넌트가 있던 GameObject:
```
1. GameObject 선택
2. Add Component → UI > Raw Image (RawImage)
3. 기존 GUITexture에 설정된 Texture 값을 Raw Image의 Texture 필드에 복사
4. GUITexture 컴포넌트 Remove
5. 씬 저장
```

> **주의**: GUITexture는 스크린 공간에 직접 렌더링됐으나 RawImage는 Canvas가 필요.
> 기존 오브젝트에 Canvas 컴포넌트 추가 후 Screen Space - Overlay 설정 필요.

---

## 7. 게임 플로우 검증 체크리스트

### 7.1 StateControl 싱글톤 검증

```
□ Menu.unity에서 Play
□ Console에 "UDPConnection Start" 로그 확인
□ Console에 "myip=..." 로그 확인 (StateControl IP 취득)
□ Console에 TEST_MODE 관련 로그 확인
□ Menu 화면 표시 (MainMenu 오브젝트 활성화)
```

### 7.2 Training 모드 검증

```
□ Menu에서 Training 선택
□ Training 씬 로드 성공 (Console에 SceneManager 관련 에러 없음)
□ 자전거 오브젝트 생성 확인
□ W/S 키로 페달링 (속도 변화)
□ A/D 키 또는 방향키로 조향
□ 장애물 통과 (Mission 오브젝트)
□ 타이머 카운트다운 확인
□ 시간 종료 → Result 화면 전환
```

### 7.3 BMX 싱글플레이 검증

```
□ Menu에서 BMX > Single 선택
□ Map5.unity 로드 성공
□ 시작 카운트다운 (3-2-1-GO) 표시
□ 4대 자전거 생성 (플레이어 1 + AI 3)
□ WheelCollider 물리: 점프대 점프/착지
□ 체크포인트 통과
□ 결승선 통과 → 결과 화면
□ AI 순위 경쟁 동작 확인
```

### 7.4 MTB 싱글플레이 검증

```
□ Menu에서 MTB > Single 선택
□ Map1.unity 로드 성공 (GameData.map[0]="Map1")
□ 동일 플로우 확인
□ 장애물(롤링 스톤) 동작 확인 (CRollingStone)
□ 지형 오르막/내리막 물리 확인
□ 결승선 → 결과 화면
```

### 7.5 공통 확인 항목

```
□ BGM 재생 (AudioCtr.Play)
□ 효과음 (브레이크, 충돌)
□ 미니맵 렌더링 (카메라 렌더 텍스처)
□ 속도/칼로리/거리 UI 표시
□ 충돌 → 라그돌 → 리스폰 동작
□ Menu 씬으로 돌아가기 (결과 화면 후)
```

---

## 8. 예상 오류 및 해결 방법

| 오류 유형 | 원인 | 해결 방법 |
|-----------|------|-----------|
| `NullReferenceException: GameData.map[...]` | 씬 이름 불일치 | Section 4의 GameData.cs 수정 |
| `Scene 'MTB001' couldn't be loaded` | BuildSettings 미등록 or 이름 불일치 | Build Settings에 씬 추가, 이름 확인 |
| `DllNotFoundException: BikeSerial` | DLL 플랫폼 타겟 불일치 | TEST_MODE=true 확인, DLL Inspector 확인 |
| `NullReferenceException` in GUI scripts | GUITexture 컴포넌트 없음 | Section 6의 RawImage 교체 |
| `MissingReferenceException` in 씬 오브젝트 | 씬 YAML 손상으로 컴포넌트 참조 누락 | 해당 씬 재구성 (Section 5.3) |
| WheelCollider 자전거가 튕기거나 비정상 | Physics 파라미터 차이 | stiffness 값 낮추기 (1.0→0.5 시도) |
| EasyRoads3D 도로 없음/오류 | 버전 비호환 | EasyRoads 메시 베이크 버전 확인 |
| `SetActiveRecursively` 오류 없음 | Phase 1에서 이미 수정됨 | — |

---

## 9. 리스크 분석

| 리스크 | 가능성 | 영향 | 대응 |
|--------|--------|------|------|
| 씬 YAML 전면 손상 | Medium | High | 씬 재구성 (프리팹 기반으로 1-2일 추가 작업) |
| EasyRoads3D 비호환 | Medium | Medium | Map1/2 도로 임시 제거 후 물리 테스트 먼저 |
| GUITexture UI 전면 재작업 | High | Medium | RawImage + Canvas 구조로 순차 교체 |
| WheelCollider 물리 대폭 변경 | Low | High | MoveModule 파라미터 조정으로 대응 가능 |
| `Standard Assets` 컴파일 오류 | Medium | Low | 사용 중인 클래스만 남기고 나머지 삭제 |

---

## 10. 완료 기준 (Definition of Done)

| 항목 | 기준 |
|------|------|
| Training 완주 | Demo02 씬에서 타이머 종료까지 키보드로 자전거 조종 가능 |
| BMX 싱글 완주 | Map5에서 결승선 통과, 결과 화면 표시 |
| MTB 싱글 완주 | Map1에서 결승선 통과, 결과 화면 표시 |
| 물리 동작 | 점프/착지/충돌/라그돌 모두 런타임 에러 없이 동작 |
| 씬 전환 | 메뉴 → 게임 → 결과 → 메뉴 전체 루프 무한 반복 가능 |
| Console | 런타임 Exception 0개 (Warning은 허용) |

---

## 11. 구현 순서 (Do Phase 가이드)

```
Step 1: Unity 6 프로젝트 생성 + Assets 복사
Step 2: GameData.map[] 씬 이름 수정 (MTB001→Map1 등)
Step 3: Build Settings에 전체 씬 등록
Step 4: Plugin DLL Inspector 설정 (x86_64)
Step 5: Menu.unity 열어 Console 확인 → 1차 오류 수집
Step 6: 씬별 임포트 오류 수정 (YAML 손상 대응)
Step 7: GUITexture → RawImage + Canvas 변환
Step 8: Training 모드 플레이테스트 + 오류 수정
Step 9: BMX 싱글플레이 테스트 + 오류 수정
Step 10: MTB 싱글플레이 테스트 + 오류 수정
Step 11: 전체 플로우 (메뉴 루프) 검증
```
