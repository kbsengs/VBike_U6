# [Plan] Unity6 마이그레이션

**Feature**: Unity6마이그레이션
**Created**: 2026-03-17
**Phase**: Plan
**Level**: Enterprise (167 scripts, 82 scenes, 778 prefabs)

---

## Executive Summary

| 항목 | 내용 |
|------|------|
| Feature | Unity6 마이그레이션 |
| 시작일 | 2026-03-17 |
| 목표 | Unity 3.5 자전거 시뮬레이터를 Unity 6에서 완전 동작 |

### Value Delivered (4-Perspective)

| 관점 | 내용 |
|------|------|
| **Problem** | Unity 3.5로 작성된 아케이드 자전거 시뮬레이터가 Unity 6에서 동작하지 않음. 개발자 부재로 유지보수 불가. 수백 개의 deprecated API 오류와 완전히 제거된 레거시 네트워킹 시스템이 주요 장애물. |
| **Solution** | Unity 6 신규 프로젝트를 생성하고 단계적으로 마이그레이션. 우선 키보드 폴백(TEST_MODE=true)으로 단일플레이어를 동작시키고, 이후 멀티플레이어 순서로 진행. API 교체 → 렌더링 수정 → 물리 검증 → 네트워크 재구현 순으로 체계적 접근. |
| **Function UX Effect** | 동일한 게임 경험(Training/BMX/MTB 모드, 자전거 물리, AI 경쟁자, 미니맵, 사운드)을 Unity 6에서 유지. 향후 업그레이드 요청사항을 안전하게 추가할 수 있는 안정적 기반 확보. |
| **Core Value** | 개발자 없이도 유지보수 가능한 최신 Unity 버전 기반으로 전환. 레거시 시스템 의존성 제거로 장기 운영 가능성 확보 및 향후 기능 확장 토대 마련. |

---

## 1. 현황 분석

### 1.1 프로젝트 규모
- **C# 스크립트**: 167개
- **씬**: 82개 (주요 플레이어블 씬 8개)
- **프리팹**: 778개
- **네이티브 DLL**: 4개 (BikeSerial.dll, ZoLock.dll, EasyPOD.dll, io.dll)

### 1.2 소스 Unity 버전: 3.5 → 타겟: Unity 6

### 1.3 테스트 환경
- 하드웨어 없음 (키보드 폴백, `GameData.TEST_MODE = true`)
- Windows 10 x64

---

## 2. Breaking Changes 분류

### 2.1 즉시 컴파일 오류 (Critical — 전체 빌드 차단)

| 구분 | 구API | 신API | 영향 범위 |
|------|-------|-------|-----------|
| 컴포넌트 접근 | `rigidbody` | `GetComponent<Rigidbody>()` | 다수 스크립트 |
| 컴포넌트 접근 | `renderer` | `GetComponent<Renderer>()` | Cycle_Control, Cycle_Impact 등 |
| 씬 로딩 | `Application.LoadLevel(name)` | `SceneManager.LoadScene(name)` | Loading.cs, 각 State 스크립트 |
| 커서 | `Screen.showCursor` | `Cursor.visible` | StateControl.cs |
| DNS | `Dns.GetHostByName()` | `Dns.GetHostEntry()` | StateControl.cs |

### 2.2 레거시 네트워킹 완전 제거 (Blocking — Unity 5.1에서 삭제)

Unity 내장 레거시 네트워킹 전체가 제거됨. 영향 파일 목록:

| 파일 | 제거된 API |
|------|-----------|
| `Cycle_Control.cs` | `Network.peerType`, `networkView.RPC()`, `[RPC]`, `BitStream`, `OnSerializeNetworkView`, `OnNetworkInstantiate` |
| `UDPConnection.cs` | `Network.InitializeServer()`, `Network.Connect()`, `networkView.RPC()`, `RPCMode`, `NetworkPlayer` |
| `NetworkRigidbody.cs` | 전체 클래스 (완전 재작성 필요) |
| `BMX_Server_*.cs` (5개) | `[RPC]`, `networkView.*`, `Network.*` |
| `BMX_Client_*.cs` (4개) | `[RPC]`, `networkView.*`, `Network.*` |
| `BMX_Multi_*.cs` (4개) | `[RPC]`, `networkView.*`, `Network.*` |
| `MTB_Client_*.cs` (4개) | `[RPC]`, `networkView.*`, `Network.*` |
| `MTB_LobbyServer.cs` | `[RPC]`, `networkView.*`, `Network.*` |

**대응 전략**: Phase 1에서는 모든 `Network.*` / `networkView.*` 호출을 `#if UNITY_LEGACY_NETWORK` 가드로 비활성화. Phase 3에서 커스텀 UDP 기반 메시지 시스템으로 재구현.

### 2.3 렌더링/쉐이더 변경 (Medium)

| 항목 | 영향 |
|------|------|
| 레거시 쉐이더 (`Legacy Shaders/`) | 빌트인 파이프라인은 유지되나 일부 쉐이더 경로 변경 |
| `renderQueue.cs`, `renderQueueAll.cs` | `renderer` 접근 방식 수정 필요 |
| 파티클 시스템 | 구버전 파티클은 Unity 6에서 자동 업그레이드 시도하나 수동 확인 필요 |
| `material.SetTexture()` | API 동일하나 쉐이더 프로퍼티명 확인 필요 |

### 2.4 Standard Assets 제거 (Low)

Unity Standard Assets은 Unity 6에서 공식 지원 종료. `Assets/Standard Assets/Character Controllers/` 등을 프로젝트 내 직접 포함하거나 대체 필요.

### 2.5 WheelCollider (Low — 확인 필요)

`WheelFrictionCurve`의 `stiffness` 프로퍼티 적용 방식이 Unity 버전별로 미묘하게 다를 수 있음. 물리 튜닝 필요할 수 있음.

---

## 3. 마이그레이션 전략

### 전체 방침
> **신규 Unity 6 프로젝트 생성 후 Assets 복사** (기존 프로젝트 직접 업그레이드 아님)

이유: Unity 3.5 → Unity 6는 버전 차이가 너무 커 직접 업그레이드 시 씬/프리팹 손상 위험이 높음.

---

## 4. 단계별 구현 계획

### Phase 1: 신규 프로젝트 셋업 & 컴파일 오류 전량 수정
**목표**: Unity 6에서 에러 없이 빌드 성공

**작업 항목**:
1. Unity 6 신규 3D 프로젝트 생성 (Built-in Render Pipeline 선택)
2. `Assets/` 폴더 전체 복사 (씬 제외, 스크립트/프리팹/텍스처/사운드/3D)
3. `using UnityEngine.SceneManagement;` 누락 파일 전체 추가
4. `rigidbody` → `GetComponent<Rigidbody>()` 전체 치환 (캐싱 패턴으로)
5. `renderer` → `GetComponent<Renderer>()` 전체 치환
6. `Screen.showCursor` → `Cursor.visible`
7. `Application.LoadLevel` → `SceneManager.LoadScene`
8. `Dns.GetHostByName` → `Dns.GetHostEntry`
9. 레거시 네트워킹 API 전체를 `#if false` 또는 빈 스텁으로 임시 처리
10. `OnGUI()` 기반 UI 동작 확인 (Unity 6에서 여전히 동작)
11. 네이티브 DLL 플랫폼 타겟 `x86_64`로 설정
12. 컴파일 에러 0 달성

**완료 기준**: Unity 6 콘솔에서 컴파일 에러 0개

---

### Phase 2: 핵심 게임 동작 검증 (단일플레이어)
**목표**: Training / BMX 단일플레이 / MTB 단일플레이 정상 동작

**작업 항목**:
1. 씬 파일 임포트 및 씬 리스트 빌드 설정에 추가
2. Menu.unity → Training → Map 씬 로딩 플로우 테스트
3. `StateControl` 싱글톤 유지 (`DontDestroyOnLoad`) 확인
4. `GameMng.SetState()` 상태 전환 정상 동작 확인
5. `Cycle_Control` + 물리 컴포넌트 동적 추가(`AddComponent`) 동작 확인
6. `WheelCollider` 물리 (전진/조향/점프/착지) 동작 확인
7. 키보드 입력 (`Input.GetAxis`) → 자전거 조종 테스트
8. AI 자전거 (`Cycle_AI`) 웨이포인트 추종 동작 확인
9. 충돌/라그돌 (`Cycle_Impact`, `RagdollAct`) 동작 확인
10. 미니맵 렌더링 확인
11. 사운드 (`AudioCtr`) 동작 확인
12. BMX/MTB 싱글플레이 결과 화면까지 전체 플로우 완주

**완료 기준**: Training, BMX 싱글, MTB 싱글 각 1 라운드 완주 가능

---

### Phase 3: 렌더링 및 쉐이더 수정
**목표**: 시각적 품질 원본과 동등 수준

**작업 항목**:
1. 임포트 후 핑크/마젠타 쉐이더 오류 확인 및 수정
2. `renderQueue.cs` / `renderQueueAll.cs` — `renderer` 접근 수정
3. Standard Assets 대체 또는 직접 포함
4. EasyRoads3D Unity 6 호환성 확인 (도로 메시 정상 렌더링)
5. 파티클 효과 (`Cycle_Smoke`) 동작 확인
6. 미니맵 카메라 렌더 텍스처 정상 확인

**완료 기준**: 핑크 머티리얼 없음, 파티클 동작, EasyRoads 도로 정상 표시

---

### Phase 4: 멀티플레이어 재구현
**목표**: BMX/MTB 멀티플레이어 동작 (UDP 기반 커스텀 RPC)

**작업 항목**:
1. 커스텀 RPC 메시지 시스템 설계 (기존 `UDPConnection.cs` UDP 레이어 위에 구현)
2. `NetworkRigidbody.cs` 완전 재작성
3. `Cycle_Control.cs` — `OnSerializeNetworkView` 로직을 커스텀 직렬화로 포팅
4. `BMX_Server_*` / `BMX_Client_*` / `BMX_Multi_*` — `[RPC]` 호출을 커스텀 메시지로 교체
5. `MTB_Client_*` / `MTB_LobbyServer` 동일 처리
6. 2대 PC 멀티플레이 기능 테스트
7. 최대 10인 플레이어 스트레스 테스트

**완료 기준**: 2인 이상 BMX/MTB 멀티플레이 정상 레이스 완주 가능

---

## 5. 리스크 분석

| 리스크 | 심각도 | 대응 |
|--------|--------|------|
| 씬 파일(.unity) 손상 | High | Phase 1에서 씬 없이 스크립트만 먼저 임포트; 씬은 새로 구성하거나 텍스트 에디터로 수작업 수정 |
| WheelCollider 물리 특성 변화 | Medium | 기존 파라미터 유지 후 플레이테스트로 튜닝 |
| EasyRoads3D 비호환 | Medium | Unity 6 지원 버전 확인; 미지원 시 도로 메시를 일반 메시로 베이크 |
| BikeSerial.dll x86 전용 | Low | TEST_MODE=true로 DLL 없이 개발; 배포 시 x64 DLL 재컴파일 또는 교체 |
| 씬 내 레거시 컴포넌트 참조 손상 | Medium | 씬 YAML 수작업 점검 필요 |

---

## 6. 구현 우선순위

```
[Phase 1] 컴파일 0 에러  ──▶  [Phase 2] 단일플레이 동작  ──▶  [Phase 3] 렌더링 수정  ──▶  [Phase 4] 멀티플레이
    (필수)                         (필수)                         (필수)                       (선택적)
```

---

## 7. 파일별 수정 우선순위

### 즉시 수정 필요 (Phase 1)
- `StateControl.cs` — Screen.showCursor, Dns.GetHostByName, 레거시 네트워크 스텁
- `Cycle_Control.cs` — rigidbody, renderer, 레거시 RPC 전체
- `UDPConnection.cs` — Network.* 전체 스텁
- `NetworkRigidbody.cs` — 임시 빈 클래스로 대체
- `Loading.cs` — Application.LoadLevel
- 모든 State/*.cs, SinglePlay/**/*.cs — Application.LoadLevel 치환

### Phase 2 수정
- `Cycle_Move.cs` — rigidbody 캐싱
- `Cycle_Impact.cs` — rigidbody, renderer
- `Cycle_AI.cs` — rigidbody
- `MainMenu.cs` — GUI 동작 확인

### Phase 3 수정
- `renderQueue.cs`, `renderQueueAll.cs`
- `MinimapSetTexture.cs`, `MinimapTexture.cs`

### Phase 4 수정
- `Network/BMXMode/**` (13 스크립트)
- `Network/MTBMode/**` (5 스크립트)
- `NetworkRigidbody.cs` (재작성)

---

## 8. 성공 기준

| 마일스톤 | 기준 |
|---------|------|
| M1. 컴파일 성공 | Unity 6 Console 에러 0개 |
| M2. 단일플레이 동작 | Training/BMX/MTB 각 1회 완주 |
| M3. 렌더링 완성 | 핑크 머티리얼 0개, 파티클/미니맵 정상 |
| M4. 멀티플레이 (선택) | 2인 BMX 레이스 완주 |
