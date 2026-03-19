# [Design] Unity6 마이그레이션

**Feature**: Unity6마이그레이션
**Created**: 2026-03-17
**Phase**: Design
**Reference**: [Plan 문서](../../01-plan/features/Unity6마이그레이션.plan.md)

---

## 1. 설계 원칙

1. **원본 동작 보존**: 게임 로직/물리/AI를 변경하지 않고 API만 교체
2. **최소 변경**: 각 파일을 동작시키는 데 필요한 최소한의 수정만 수행
3. **단계적 검증**: Phase별로 실행 가능한 상태를 유지하며 진행
4. **네트워크 격리**: 레거시 네트워킹은 전용 조건부 컴파일로 격리

---

## 2. Phase 1 — 컴파일 에러 전량 수정

### 2.1 신규 Unity 6 프로젝트 생성 절차

```
1. Unity Hub → New Project → 3D (Built-in Render Pipeline) → Unity 6
2. 프로젝트명: VBike_Unity6
3. Assets/ 전체 복사 (스크립트/프리팹/텍스처/사운드/3D모델)
4. 씬 파일은 나중에 별도 처리 (YAML 손상 가능성)
5. Edit > Project Settings > Player:
   - Other Settings > Scripting Runtime Version: .NET Standard 2.1
   - Allow unsafe Code: 체크
6. Build Settings에 플랫폼: Windows x86_64 설정
7. Assets/plugins/ DLL 각각 선택 → Inspector:
   - Include Platforms: Editor, Standalone (Windows)
   - CPU: x86_64
```

### 2.2 전역 API 치환 목록 (Find & Replace)

아래 치환은 **전체 프로젝트** 에 일괄 적용. 파일별 상세 수정 전 먼저 실행.

| 검색 패턴 | 치환 패턴 | 비고 |
|-----------|-----------|------|
| `Application.LoadLevel(` | `UnityEngine.SceneManagement.SceneManager.LoadScene(` | using 추가 필요 |
| `Application.LoadLevelAsync(` | `UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(` | |
| `Screen.showCursor` | `Cursor.visible` | |
| `Dns.GetHostByName(` | `Dns.GetHostEntry(` | 반환 타입 변경 주의 |

### 2.3 파일별 수정 상세 설계

---

#### `StateControl.cs`

**변경 1 — Screen.showCursor**
```csharp
// Before
Screen.showCursor = false;

// After
Cursor.visible = false;
```

**변경 2 — Dns.GetHostByName**
```csharp
// Before
IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
GameData.MY_IP = IPHost.AddressList[0].ToString();

// After
IPHostEntry IPHost = Dns.GetHostEntry(Dns.GetHostName());
// IPv4만 필터링 (AddressList에 IPv6 포함될 수 있음)
System.Net.IPAddress ipv4 = System.Array.Find(
    IPHost.AddressList,
    ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
);
GameData.MY_IP = (ipv4 != null) ? ipv4.ToString() : "127.0.0.1";
```

**변경 3 — 레거시 네트워킹 스텁 (임시)**
```csharp
// StateControl.Start() 에서 Select_Network.Server 분기의
// CBikeSerial.Init() 이후 코드는 그대로 유지.
// Select_Network.Client 분기도 그대로 유지.
// Network.* 호출이 없으므로 이 파일은 추가 스텁 불필요.
```

**추가 — using 선언**
```csharp
using UnityEngine.SceneManagement;
using System.Net;
```

---

#### `Cycle_Control.cs`

**변경 1 — rigidbody 프로퍼티**
```csharp
// 클래스 상단에 필드 추가
private Rigidbody _rb;

// Start() 최상단에 추가
_rb = GetComponent<Rigidbody>();

// Before (모든 rigidbody 참조)
rigidbody.isKinematic = true;
rigidbody.velocity = ...

// After
_rb.isKinematic = true;
_rb.linearVelocity = ...   // Unity 6에서 velocity → linearVelocity
```

> **주의**: Unity 6에서 `Rigidbody.velocity`가 `linearVelocity`로 변경됨

**변경 2 — renderer 프로퍼티 (SetArrowColor)**
```csharp
// Before
minimapArrow.arrow.renderer.material.SetTexture(...)

// After
minimapArrow.arrow.GetComponent<Renderer>().material.SetTexture(...)
```

**변경 3 — Network/networkView 전체 비활성화**
```csharp
// Update() 내 Network.peerType 블록
#if LEGACY_NETWORK
if (Network.peerType == NetworkPeerType.Server)
{
    networkView.RPC("SyncRank_RPC", RPCMode.All, rank);
}
#endif

// FixedUpdate() 내 gameFinish && Network 블록
#if LEGACY_NETWORK
if (gameFinish && Network.peerType == NetworkPeerType.Server)
{
    networkView.RPC("SendGameFinish", RPCMode.Others, gameFinish);
}
#endif

// OnNetworkInstantiate, OnSerializeNetworkView, [RPC] 메서드 전체
#if LEGACY_NETWORK
void OnNetworkInstantiate(NetworkMessageInfo info) { ... }
[RPC] void SendGameFinish(bool a) { ... }
// ... 모든 네트워크 관련 메서드
#endif
```

**변경 4 — animation 접근 (Ready() 코루틴 내)**
```csharp
// BMX_S_InGame.cs 에서 호출되는 애니메이션
// Before
GameObject.Find("start" + GameData.BMXMap + "/start_plane1").animation.Play();

// After (Animation 컴포넌트 명시적 접근)
var animObj = GameObject.Find("start" + GameData.BMXMap + "/start_plane1");
if (animObj != null)
{
    var anim = animObj.GetComponent<Animation>();
    if (anim != null) anim.Play();
}
```

---

#### `Cycle_Impact.cs`

**변경 1 — rigidbody**
```csharp
private Rigidbody _rb;

void Start()
{
    _rb = GetComponent<Rigidbody>();
    // ...
}

// Before: rigidbody.isKinematic = true/false
// After:  _rb.isKinematic = true/false
```

---

#### `Cycle_AI.cs`

`rigidbody` 직접 사용 없음. `MoveModule` 베이스 클래스에서 처리됨.
`Cycle_AI` 자체 수정 불필요 (MoveModule 수정으로 해결).

---

#### `Cycle_Move.cs`

`MoveModule` 베이스 클래스에서 WheelCollider/rigidbody 처리.
`Serial()` 메서드의 `rigidbody.velocity.magnitude` 참조:

```csharp
// CBikeSerial.FrameBike 호출 시 speed 파라미터
// Before
CBikeSerial.FrameBike(Time.fixedDeltaTime, heightAngle, maxValue.MaxSteer,
    (int)(_control.environment), rigidbody.velocity.magnitude);

// After (MoveModule에 _rb 캐시 후 사용)
CBikeSerial.FrameBike(Time.fixedDeltaTime, heightAngle, maxValue.MaxSteer,
    (int)(_control.environment), _rb.linearVelocity.magnitude);
```

---

#### `MoveModule.cs` (베이스 클래스 — 찾아서 수정 필요)

WheelCollider 물리를 담당하는 핵심 베이스 클래스. 다음 패턴으로 수정:

```csharp
// 클래스 상단
protected Rigidbody _rb;

// Init() 또는 Awake()에서
_rb = GetComponent<Rigidbody>();

// WheelFrictionCurve 설정 (값 타입이므로 copy-modify-assign 패턴 필수)
// Before (가능한 구버전 패턴)
frontTire.GetComponent<WheelCollider>().forwardFriction.extremumSlip = value;

// After (Unity 5+에서 필수 패턴)
WheelCollider wc = frontTire.GetComponent<WheelCollider>();
WheelFrictionCurve ffc = wc.forwardFriction;
ffc.extremumSlip = value;
wc.forwardFriction = ffc;
```

---

#### `BMX_S_InGame.cs` (및 모든 *_S_InGame.cs, *_InGame.cs)

**변경 1 — Application.LoadLevelAsync**
```csharp
// Before
IEnumerator Activate()
{
    AsyncOperation async = Application.LoadLevelAsync(GameData.map[3]);
    yield return async;
    ...
}

// After
using UnityEngine.SceneManagement;
...
IEnumerator Activate()
{
    AsyncOperation async = SceneManager.LoadSceneAsync(GameData.map[3]);
    yield return async;
    ...
}
```

**변경 2 — rigidbody 직접 접근**
```csharp
// Before
data.MyCharacter.rigidbody.isKinematic = false;
foreach (Cycle_Control pos in data.ai)
    pos.rigidbody.isKinematic = false;

// After — Cycle_Control에 public 래퍼 속성 추가
// Cycle_Control.cs에 추가:
public Rigidbody Rb => _rb;

// BMX_S_InGame.cs
data.MyCharacter.Rb.isKinematic = false;
foreach (Cycle_Control pos in data.ai)
    pos.Rb.isKinematic = false;
```

---

#### `Loading.cs`

`Application.LoadLevel` 없음. `OnGUI()` 기반 렌더링은 Unity 6에서도 동작.
`Graphics.DrawTexture()`는 Unity 6에서도 지원.
**수정 불필요** (단, 씬 해상도 하드코딩 1920×1080 확인 필요).

---

#### `NetworkRigidbody.cs` — 임시 빈 클래스로 대체

```csharp
// Phase 1 임시 버전 (Phase 4에서 재작성)
using UnityEngine;

public class NetworkRigidbody : MonoBehaviour
{
    // TODO: Phase 4 - 커스텀 UDP 기반 물리 동기화로 재구현
}
```

---

#### `UDPConnection.cs` — 레거시 Network.* 스텁 처리

```csharp
// Network.player.ipAddress → 커스텀 IP 취득으로 교체
// Before (Start)
myip = Network.player.ipAddress.ToString();

// After
var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
var ip = System.Array.Find(host.AddressList,
    a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
myip = (ip != null) ? ip.ToString() : "127.0.0.1";

// NewStartServer() 내 레거시 Network 코드 비활성화
#if LEGACY_NETWORK
Network.incomingPassword = "MTB";
Network.InitializeServer(32, listenPort, false);
#endif

// StartNetwork() 내 Network.Connect 비활성화
#if LEGACY_NETWORK
Network.Connect(server_name, listenPort, "MTB");
#endif

// OnPlayerConnected, OnPlayerDisconnected, OnServerInitialized,
// OnConnectedToServer, OnDisconnectedFromServer, SetMyInfo([RPC]) 등
// 전부 #if LEGACY_NETWORK 로 감싸기

// OnGUI() — 유지 (단순 디버그 레이블)
```

---

#### `Network/BMXMode/*.cs` 및 `Network/MTBMode/*.cs` (Phase 1 스텁)

이 파일들은 `[RPC]`, `networkView`, `Network.*` 을 대량 사용.
**Phase 1 목표: 컴파일 통과** (기능 동작 불필요)

전략:
```csharp
// 각 파일 상단에 추가
#define LEGACY_NETWORK_DISABLED

// 각 메서드 내 Network.* / networkView.* / [RPC] 라인을
// #if !LEGACY_NETWORK_DISABLED ... #endif 로 감싸기

// [RPC] 어트리뷰트는 삭제 또는 주석 처리
// RPCMode, NetworkPlayer, BitStream 타입 참조 제거
```

---

### 2.4 `using` 선언 추가 대상 파일

`LoadLevelAsync` / `LoadLevel` 사용 파일 전체에 추가:
```csharp
using UnityEngine.SceneManagement;
```

대상 파일 (예상):
- `BMX_S_InGame.cs`
- `MTB_S_InGame.cs`
- `Training_InGame.cs`
- `BMX_Client_InGame.cs`
- `MTB_Client_InGame.cs`
- `BMX_Server_InGame.cs`
- `BMX_Multi_InGame.cs`
- 기타 `Application.LoadLevel` 사용 파일

---

## 3. Phase 2 — 단일플레이어 동작 검증

### 3.1 씬 임포트 전략

Unity 3.5의 `.unity` 씬 파일은 YAML 텍스트 형식이며, Unity 6에서 직접 열면 컴포넌트 GUID 불일치로 손상될 수 있음.

**추천 절차**:
```
1. 먼저 Menu.unity 하나만 임포트 시도
2. Unity 6에서 열어 오류 확인
3. 오류가 많으면 씬을 새로 구성 (빈 씬에 프리팹 재배치)
4. Map 씬들은 3D 오브젝트(지형, 트랙)는 프리팹으로 존재하므로
   씬 손상 시 빈 씬에 프리팹 드래그로 재구성 가능
```

**Build Settings 씬 등록**:
```
0: Menu
1: Map1 (MTB001)
2: Map2_01 (MTB002)
3: Map2_02 (MTB003)
4: Map5
5: BMXServer
6: Client
7: MTBLobbyServer
```
`GameData.map[]`의 인덱스 순서와 일치시킬 것.

### 3.2 StateControl 초기화 흐름 확인

```
[App Start]
  └─ StateControl.Awake() → DontDestroyOnLoad(this)
  └─ StateControl.Start()
       ├─ Cursor.visible = false
       ├─ GameMng 획득
       ├─ IP 획득 (GetHostEntry)
       ├─ TEST_MODE → GameData.m_bLock = true
       └─ select_Network == Client
            ├─ CBikeSerial.Init() [하드웨어 없으면 false 반환, 계속 진행]
            ├─ gameObject.AddComponent<Menu_SelectGame>()
            └─ m_GameMng.SetState(typeof(Menu_SelectGame))
```

**검증 포인트**: CBikeSerial.Init()이 DLL 없어도 예외 없이 false 반환하는지 확인.
BikeSerial.dll이 없을 경우 DllNotFoundException 발생 → try/catch로 감쌀 것.

```csharp
// CBikeSerial.Init() 수정 (TEST_MODE 시 DLL 호출 skip)
public static bool Init()
{
    if (GameData.TEST_MODE)
    {
        start = 1;
        m_nJump = 0; m_nTong = 0; m_nBreak = 0; m_nCrash = 0;
        return false; // 하드웨어 없음을 의미하는 false, 게임은 계속
    }
    // 기존 DLL 초기화 코드 ...
}
```

### 3.3 WheelCollider 설계

`MoveModule`의 `FrictionSetting()` 에서 WheelFrictionCurve를 WheelCollider에 적용.
Unity 6에서 `WheelCollider.forwardFriction` 등은 값 타입이므로 아래 패턴 준수:

```csharp
// WheelCollider에 forwardFriction 적용 패턴
void ApplyFriction(WheelCollider wc, WheelFrictionCurve ffc, WheelFrictionCurve sfc)
{
    // forwardFriction
    WheelFrictionCurve ff = wc.forwardFriction;
    ff.extremumSlip = ffc.extremumSlip;
    ff.extremumValue = ffc.extremumValue;
    ff.asymptoteSlip = ffc.asymptoteSlip;
    ff.asymptoteValue = ffc.asymptoteValue;
    ff.stiffness = ffc.stiffness;
    wc.forwardFriction = ff;

    // sidewaysFriction
    WheelFrictionCurve sf = wc.sidewaysFriction;
    sf.extremumSlip = sfc.extremumSlip;
    sf.extremumValue = sfc.extremumValue;
    sf.asymptoteSlip = sfc.asymptoteSlip;
    sf.asymptoteValue = sfc.asymptoteValue;
    sf.stiffness = sfc.stiffness;
    wc.sidewaysFriction = sf;
}
```

> **주의**: Unity 6에서 `WheelCollider`의 물리 특성이 미묘하게 다를 수 있음.
> 기존 파라미터로 시작하고 플레이테스트 후 stiffness 값 튜닝.

### 3.4 GameData.map[] 인덱스와 씬 이름 매핑

```csharp
// GameData.cs (현재 정의)
public static string[] map = new string[] { "MTB001", "MTB002", "MTB003", "Map5" };
// 인덱스: 0=Training MTB1, 1=Training MTB2, 2=Training MTB3, 3=BMX(Map5)

public static string[] bmxWay  = new string[] { "_Waypoint", "_Waypoint1", "_Waypoint2", "_Waypoint3" };
public static string[] bmxStart = new string[] { "_Waypoint", "_Startpoint1", "_Startpoint2", "_Startpoint3" };
// BMXMap 0~3에 대응
```

씬 파일명이 GameData.map[] 값과 **정확히 일치**해야 함.
Unity 6 프로젝트 Build Settings에 등록된 씬 이름 = GameData.map[] 값.

### 3.5 Rigidbody.velocity → linearVelocity 대응

Unity 6에서 `Rigidbody.velocity`는 deprecated, `linearVelocity` 사용 권장.

```csharp
// 전체 검색: rigidbody.velocity  / _rb.velocity
// 치환: _rb.linearVelocity

// 단, speed 계산 시
float speed = _rb.linearVelocity.magnitude; // ✅
```

---

## 4. Phase 3 — 렌더링 수정

### 4.1 레거시 쉐이더 대응

Unity 6 Built-in RP는 레거시 쉐이더 대부분을 지원하나 일부 경로 변경:

| 구버전 쉐이더 경로 | Unity 6 대응 |
|--------------------|--------------|
| `Legacy Shaders/Diffuse` | `Legacy Shaders/Diffuse` (유지) |
| `Mobile/Diffuse` | `Mobile/Diffuse` (유지) |
| `Particles/Additive` | `Particles/Standard Unlit` 또는 `Universal Render Pipeline/Particles/Unlit` |

**핑크 머티리얼 수정 절차**:
1. 씬에서 핑크로 표시되는 오브젝트 선택
2. Inspector → Material → Shader 확인
3. 없어진 쉐이더는 `Standard`로 교체 후 텍스처 재연결

### 4.2 `renderQueue.cs` / `renderQueueAll.cs`

```csharp
// renderQueue.cs — renderer 프로퍼티 접근
// Before
renderer.material.renderQueue = renderQueue;

// After
GetComponent<Renderer>().material.renderQueue = renderQueue;
```

### 4.3 `MinimapSetTexture.cs` / `MinimapTexture.cs`

```csharp
// renderer 접근 패턴 동일하게 수정
// Render Texture 기반 미니맵은 Unity 6에서 동일하게 동작 예상
```

### 4.4 `Cycle_Control.SetArrowColor()` / `ArrowPosition()`

```csharp
// minimapArrow.arrow.renderer.material →
minimapArrow.arrow.GetComponent<Renderer>().material...
```

---

## 5. Phase 4 — 멀티플레이어 재구현 설계

### 5.1 커스텀 RPC 메시지 시스템 설계

레거시 `[RPC]` 대체를 위한 경량 UDP 메시지 프레임워크:

```csharp
// 메시지 포맷
[System.Serializable]
public struct NetMessage
{
    public byte type;      // 메시지 타입 ID
    public int sender;     // 플레이어 번호
    public byte[] payload; // 직렬화된 데이터
}

// 메시지 타입 정의
public enum NetMsgType : byte
{
    SyncBikeState   = 1,  // Cycle_Control 동기화
    SyncRank        = 2,  // 순위 동기화
    GameFinish      = 3,  // 게임 종료 알림
    SetReady        = 4,  // 준비 상태
    StartGame       = 5,  // 게임 시작
    // ...
}
```

### 5.2 `NetworkRigidbody.cs` 재구현 설계

```csharp
public class NetworkRigidbody : MonoBehaviour
{
    private Rigidbody _rb;
    private bool _isMine;

    // 보간을 위한 버퍼
    private Vector3 _targetPos;
    private Quaternion _targetRot;

    void Update()
    {
        if (!_isMine)
        {
            // 수신된 위치로 보간
            transform.position = Vector3.Lerp(
                transform.position, _targetPos, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, _targetRot, Time.deltaTime * 10f);
        }
    }

    public void ApplyNetworkState(Vector3 pos, Quaternion rot)
    {
        _targetPos = pos;
        _targetRot = rot;
    }
}
```

### 5.3 Cycle_Control 네트워크 동기화 대체

`OnSerializeNetworkView` 로직을 커스텀 송수신으로 교체:

```csharp
// 기존 OnSerializeNetworkView 내용을 두 메서드로 분리
public byte[] SerializeBikeState()
{
    // steer, handle, pedalSpeed, realSpeed, groundF/R,
    // heightAngle, lrAngle, deadState, gameFinish, drift 등 직렬화
    using (var ms = new System.IO.MemoryStream())
    using (var bw = new System.IO.BinaryWriter(ms))
    {
        bw.Write(moveValue.steer);
        bw.Write(moveValue.handle);
        // ...
        return ms.ToArray();
    }
}

public void DeserializeBikeState(byte[] data)
{
    using (var ms = new System.IO.MemoryStream(data))
    using (var br = new System.IO.BinaryReader(ms))
    {
        moveValue.steer    = br.ReadSingle();
        moveValue.handle   = br.ReadSingle();
        // ...
    }
}
```

---

## 6. 수정 파일 전체 목록 (우선순위 순)

### Phase 1 필수 수정 파일

| 파일 | 수정 내용 | 난이도 |
|------|-----------|--------|
| `StateControl.cs` | showCursor, Dns, using 추가 | 낮음 |
| `Cycle_Control.cs` | rigidbody→_rb, renderer, Network 비활성화 | 높음 |
| `Cycle_Impact.cs` | rigidbody→_rb | 낮음 |
| `Cycle_Move.cs` | rigidbody.velocity→linearVelocity | 낮음 |
| `MoveModule.cs` | rigidbody→_rb 캐시, WheelCollider copy-assign | 중간 |
| `CBikeSerial.cs` | TEST_MODE 시 DLL skip 처리 | 낮음 |
| `NetworkRigidbody.cs` | 빈 클래스로 교체 | 낮음 |
| `UDPConnection.cs` | Network.* 비활성화, IP 취득 교체 | 중간 |
| `BMX_S_InGame.cs` | LoadLevelAsync, rigidbody, animation | 중간 |
| `MTB_S_InGame.cs` | LoadLevelAsync, rigidbody | 중간 |
| `Training_InGame.cs` | LoadLevelAsync | 낮음 |
| `BMX_Server_*.cs` (5개) | [RPC]/Network 비활성화 | 중간 |
| `BMX_Client_*.cs` (4개) | [RPC]/Network 비활성화 | 중간 |
| `BMX_Multi_*.cs` (4개) | [RPC]/Network 비활성화 | 중간 |
| `MTB_Client_*.cs` (4개) | [RPC]/Network 비활성화 | 중간 |
| `MTB_LobbyServer.cs` | [RPC]/Network 비활성화 | 중간 |
| `Cycle_Control.cs`의 `animation` 접근 | GetComponent<Animation>() | 낮음 |
| `minimapArrow.arrow.renderer` | GetComponent<Renderer>() | 낮음 |

### Phase 3 수정 파일

| 파일 | 수정 내용 |
|------|-----------|
| `renderQueue.cs` | renderer→GetComponent<Renderer>() |
| `renderQueueAll.cs` | renderer→GetComponent<Renderer>() |
| `MinimapSetTexture.cs` | renderer 접근 수정 |
| `MinimapTexture.cs` | renderer 접근 수정 |

### Phase 4 수정 파일 (재구현)

| 파일 | 수정 내용 |
|------|-----------|
| `NetworkRigidbody.cs` | 전체 재작성 |
| `UDPConnection.cs` | 커스텀 RPC 레이어 추가 |
| `Cycle_Control.cs` | OnSerializeNetworkView → Serialize/Deserialize |
| `BMX_Server_*.cs` / `BMX_Client_*.cs` | [RPC] → 커스텀 메시지로 교체 |
| `MTB_Client_*.cs` / `MTB_LobbyServer.cs` | 동일 |

---

## 7. 테스트 체크리스트

### Phase 1 완료 기준
- [ ] Unity 6 Console 탭에 컴파일 에러 0개
- [ ] 경고(Warning)는 허용

### Phase 2 완료 기준
- [ ] Menu 씬 진입 (MainMenu 표시)
- [ ] Training 모드 선택 → Map 씬 로딩 성공
- [ ] 자전거 생성 및 키보드 조종 (WASD/방향키)
- [ ] AI 자전거 웨이포인트 추종 확인
- [ ] 결승선 통과 → Result 화면 전환
- [ ] BMX 싱글플레이 완주
- [ ] MTB 싱글플레이 완주
- [ ] 충돌 → 라그돌 → 리스폰 동작

### Phase 3 완료 기준
- [ ] 핑크/마젠타 머티리얼 없음
- [ ] 파티클 효과 (먼지, 스모크) 표시
- [ ] 미니맵 자전거 위치 표시
- [ ] 로딩 화면 텍스처 표시

---

## 8. 구현 순서 (Do Phase 가이드)

```
Step 1: Unity 6 프로젝트 생성 & Assets 복사
Step 2: 전역 텍스트 치환 (Application.LoadLevel 등)
Step 3: NetworkRigidbody.cs → 빈 클래스 교체
Step 4: Network/BMXMode, MTBMode 파일 → #if LEGACY_NETWORK 처리
Step 5: UDPConnection.cs 수정
Step 6: Cycle_Control.cs 수정 (rigidbody, renderer, Network)
Step 7: StateControl.cs 수정
Step 8: CBikeSerial.cs TEST_MODE 처리
Step 9: MoveModule.cs 수정 (WheelCollider copy-assign)
Step 10: Cycle_Impact.cs, Cycle_Move.cs 수정
Step 11: BMX_S_InGame.cs, MTB_S_InGame.cs 수정
Step 12: 컴파일 에러 확인 및 잔여 오류 수정
Step 13: 씬 임포트 및 플레이테스트
Step 14: renderQueue, Renderer 관련 수정
```
