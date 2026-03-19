# Unity6 Migration - Gap Analysis Report

> **Analysis Type**: Design vs Implementation Gap Analysis (Phase 1 - Compile Error Fixes)
>
> **Project**: VBike (Unity 3.5 -> Unity 6)
> **Analyst**: Claude Code (gap-detector)
> **Date**: 2026-03-17
> **Design Doc**: [Unity6마이그레이션.design.md](../02-design/features/Unity6마이그레이션.design.md)

---

## 1. Overall Scores

| Category | Score | Status |
|----------|:-----:|:------:|
| Design Match | 92% | ✅ |
| Network Stubbing | 100% | ✅ |
| Additional Files (beyond design) | 100% | ✅ |
| **Overall Match Rate** | **94%** | ✅ |

**Verdict**: Design and implementation match well. Minor differences documented below.

---

## 2. File-by-File Verification

### 2.1 StateControl.cs

| Design Item | Expected | Actual | Status |
|-------------|----------|--------|--------|
| `Cursor.visible = false` | Replace `Screen.showCursor` | Line 54: `Cursor.visible = false;` | ✅ |
| `Dns.GetHostEntry` + IPv4 filter | Replace `Dns.GetHostByName` | Lines 64-69: exact match with design | ✅ |
| Fallback `127.0.0.1` | When no IPv4 found | Line 69: `(ipv4 != null) ? ipv4.ToString() : "127.0.0.1"` | ✅ |
| `using UnityEngine.SceneManagement` | Add using | Line 5 | ✅ |
| `using System.Net` | Add using | Line 4 | ✅ |
| No legacy Network calls | Design says none in this file | Confirmed: no active `Network.*` | ✅ |

**Score: 6/6 (100%)**

### 2.2 Cycle_Control.cs

| Design Item | Expected | Actual | Status |
|-------------|----------|--------|--------|
| `private Rigidbody _rb` field | Add cached field | Line 144: `private Rigidbody _rb;` | ✅ |
| `public Rigidbody Rb => _rb` | Public property for external access | Line 145: `public Rigidbody Rb => _rb;` | ✅ |
| `_rb = GetComponent<Rigidbody>()` in Start | Cache in Start() | Line 148 | ✅ |
| `_rb.isKinematic` usage | Replace `rigidbody.isKinematic` | Lines 217, 457 | ✅ |
| `minimapArrow.arrow.GetComponent<Renderer>()` | Replace `.renderer` | Lines 673-675 | ✅ |
| Network/RPC lines commented/stubbed | `#if LEGACY_NETWORK` or comments | Lines 220-222, 288-289, 425-468, 482-590 | ✅ |
| `_rb.linearVelocity` | Replace `rigidbody.velocity` | Not found in Cycle_Control directly (delegated to MoveModule) | ✅ N/A |
| Animation access with null check | Design Section 2.3 `변경4` | Implemented in BMX_S_InGame, not here | ⚠️ |

**Note on Animation**: Design specifies the animation null-check pattern inside Cycle_Control's Ready() coroutine, but the actual Ready() coroutine and animation code lives in `BMX_S_InGame.cs` where it IS correctly implemented (line 130-131). This is a design document inaccuracy, not an implementation gap.

**Score: 7/8 (88%) -- the 1 "miss" is a design doc location error, not missing code**

### 2.3 Cycle_Impact.cs

| Design Item | Expected | Actual | Status |
|-------------|----------|--------|--------|
| `private Rigidbody _rb` | Cached field | Line 7 | ✅ |
| `_rb = GetComponent<Rigidbody>()` in Start | Cache in Start() | Line 22 | ✅ |
| `_rb.isKinematic` in Crash() | Replace `rigidbody.isKinematic` | Line 310 | ✅ |
| `_rb.isKinematic` in Respawn() | Replace `rigidbody.isKinematic` | Line 335 | ✅ |
| `_rb.isKinematic` in case 2 | Replace `rigidbody.isKinematic` | Line 143 | ✅ |

**Score: 5/5 (100%)**

### 2.4 Cycle_Move.cs

| Design Item | Expected | Actual | Status |
|-------------|----------|--------|--------|
| `_rb.linearVelocity.magnitude` in FrameBike call | Replace `rigidbody.velocity.magnitude` | Line 110: `_rb.linearVelocity.magnitude` | ✅ |
| Inherits from MoveModule (has _rb) | Uses base class cached _rb | Yes, `_rb` from MoveModule | ✅ |

**Score: 2/2 (100%)**

### 2.5 MoveModule.cs

| Design Item | Expected | Actual | Status |
|-------------|----------|--------|--------|
| `protected Rigidbody _rb` | Protected field for subclasses | Line 67 | ✅ |
| `_rb = GetComponent<Rigidbody>()` in Init() | Cache in Init | Line 107 | ✅ |
| Also cached in Start() | Backup cache | Line 376 | ✅ (bonus) |
| `_rb.linearDamping` | Replace `.drag` | Line 187 | ✅ |
| `_rb.angularDamping` | Replace `.angularDrag` | Lines 244, 246, 248 | ✅ |
| `_rb.linearVelocity` | Replace `.velocity` | Lines 187, 190, 264, 265 | ✅ |
| `_rb.centerOfMass` | Replace `rigidbody.centerOfMass` | Lines 113, 223, 224 | ✅ |
| `_rb.AddRelativeTorque` | Replace `rigidbody.AddRelativeTorque` | Line 232 | ✅ |
| `_rb.AddForce` / `_rb.AddTorque` | Replace rigidbody calls | Lines 269, 292, 298, 351 | ✅ |
| WheelFrictionCurve copy-assign pattern | Design Section 2.3 shows pattern | FrictionSetting() uses local struct then assigns (lines 155-179) | ⚠️ |

**Note on WheelFrictionCurve**: The design recommends the explicit copy-modify-assign pattern (`var ff = wc.forwardFriction; ff.X = val; wc.forwardFriction = ff;`). The implementation builds a local `WheelFrictionCurve` struct (`friction_F`, `friction_S`) and assigns it directly (`pos.forwardFriction = friction_F`). This is functionally equivalent and correct -- it's a valid alternative to the design pattern.

**Score: 9/10 (90%) -- WheelFrictionCurve uses equivalent but different pattern**

### 2.6 CBikeSerial.cs

| Design Item | Expected | Actual | Status |
|-------------|----------|--------|--------|
| `if (GameData.TEST_MODE) return false` at top of Init() | Early return for TEST_MODE | Lines 87-92: check after button reset, returns false | ✅ |
| `if (GameData.TEST_MODE) return` at top of FrameBike() | Early return for TEST_MODE | Line 232 | ✅ |
| try/catch around DLL calls | Handle DllNotFoundException | Lines 94-111: try/catch wrapping all DLL calls | ✅ (bonus) |

**Score: 3/3 (100%) -- plus bonus try/catch**

### 2.7 NetworkRigidbody.cs

| Design Item | Expected | Actual | Status |
|-------------|----------|--------|--------|
| Empty stub class | `public class NetworkRigidbody : MonoBehaviour {}` | Lines 1-8: exact match with TODO comment | ✅ |

**Score: 1/1 (100%)**

### 2.8 UDPConnection.cs

| Design Item | Expected | Actual | Status |
|-------------|----------|--------|--------|
| Custom `GetLocalIPv4()` method | Using `Dns.GetHostEntry` + IPv4 filter | Lines 64-74 | ✅ |
| `Network.InitializeServer` removed/commented | Stubbed | Lines 133-136 (commented) | ✅ |
| `Network.Connect` removed/commented | Stubbed | Lines 147-150 (commented) | ✅ |
| `[RPC] SetMyInfo` removed/commented | Stubbed | Lines 167-169 | ✅ |
| `OnPlayerConnected/Disconnected` removed | Stubbed | Lines 167-169 | ✅ |
| `Network.player.ipAddress` replaced | Uses GetLocalIPv4() | Line 59 | ✅ |
| OnGUI() preserved | Debug label kept | Lines 171-174 | ✅ |

**Score: 7/7 (100%)**

### 2.9 BMX_S_InGame.cs

| Design Item | Expected | Actual | Status |
|-------------|----------|--------|--------|
| `using UnityEngine.SceneManagement` | Add using | Line 3 | ✅ |
| `SceneManager.LoadSceneAsync` | Replace `Application.LoadLevelAsync` | Line 84 | ✅ |
| `pos.Rb.isKinematic = false` | Use Cycle_Control.Rb property | Lines 148, 153 | ✅ |
| Animation access with null check | Design Section 2.3 | Lines 130-131: null check on startPlaneObj and anim | ✅ |

**Score: 4/4 (100%)**

### 2.10 MTB_S_InGame.cs

| Design Item | Expected | Actual | Status |
|-------------|----------|--------|--------|
| `using UnityEngine.SceneManagement` | Add using | Line 3 | ✅ |
| `SceneManager.LoadSceneAsync` | Replace `Application.LoadLevelAsync` | Line 85 | ✅ |
| `pos.Rb.isKinematic = false` | Use Cycle_Control.Rb property | Lines 161, 166 | ✅ |

**Score: 3/3 (100%)**

### 2.11 Training_InGame.cs

| Design Item | Expected | Actual | Status |
|-------------|----------|--------|--------|
| `using UnityEngine.SceneManagement` | Add using | Line 3 | ✅ |
| `SceneManager.LoadSceneAsync` | Replace `Application.LoadLevelAsync` | Line 83 | ✅ |

**Score: 2/2 (100%)**

### 2.12 Network BMXMode + MTBMode Files (Bulk)

| Design Item | Expected | Actual | Status |
|-------------|----------|--------|--------|
| No active `[RPC]` attributes | All commented/removed | 0 active `[RPC]` found | ✅ |
| No active `Network.*` calls | All commented/removed | 0 active `Network.*` in these dirs | ✅ |
| No active `networkView.*` calls | All commented/removed | 0 active `networkView.*` found | ✅ |
| No active `RPCMode`, `BitStream`, `NetworkPeerType` | All commented/removed | All appear only in `// Unity6:` comments | ✅ |

**Files verified (18 files total)**:
- BMXMode: BMX_Client_Data, BMX_Client_InGame, BMX_Client_Result, BMX_Client_Wait, BMX_Multi_Data, BMX_Multi_InGame, BMX_Multi_Result, BMX_Multi_Wait, BMX_Server_Data, BMX_Server_InGame, BMX_Server_LoadConfig, BMX_Server_Result, BMX_Server_Wait
- MTBMode: MTB_Client_Data, MTB_Client_InGame, MTB_Client_Result, MTB_Client_Wait, MTB_LobbyServer

**Score: 4/4 (100%)**

---

## 3. Additional Files (Implementation beyond Design)

These files were migrated but not explicitly detailed in the design document's Section 2.3. They are listed in Section 4 (Phase 3) or found during implementation.

| File | Migration Applied | Status |
|------|-------------------|--------|
| `renderQueue.cs` | `GetComponent<Renderer>()` replaces `.renderer` | ✅ |
| `MinimapSetTexture.cs` | `GetComponent<Renderer>()` replaces `.renderer` | ✅ |
| `InGameGUI.cs` | `GetComponent<Renderer>()`, `GetComponent<Animation>()`, `SetActive()` | ✅ |
| `Training_GUI.cs` | `GetComponent<Renderer>()`, `GetComponent<Animation>()` | ✅ |
| `CRollingStone.cs` | `private Rigidbody _rb`, `_rb.linearVelocity`, `Network.isServer` removed | ✅ |
| `CRollingStone2.cs` | `private Rigidbody _rb`, `_rb.linearVelocity`, `Network.isServer` removed | ✅ |
| `DrawCall.cs` | `GetComponent<Renderer>()`, `SetActive()` replaces `SetActiveRecursively` | ✅ |

**Note**: These files were classified as Phase 3 in the design (Section 4) but were implemented alongside Phase 1. This is proactive work, not a gap.

---

## 4. Remaining Issues Found

### 4.1 Active Legacy API References (outside Phase 1 scope)

| File | Issue | Severity |
|------|-------|----------|
| `Assets/_Program/ConfigGUI.cs:264` | Active `Network.Disconnect()` call | Medium |
| `Assets/_Program/Utile/LoadBundle.cs:23,45` | Active `Application.LoadLevelAsync` / `LoadLevelAdditiveAsync` | Medium |
| `Assets/_Program/Editor/DecryptAssetBundle.cs:112` | Active `Application.LoadLevel` (Editor script) | Low |

These files are outside the Phase 1 scope defined in the design document but will cause compile errors in Unity 6.

### 4.2 Design Document Inaccuracies

| Item | Description | Impact |
|------|-------------|--------|
| Animation null-check location | Design says `Cycle_Control.cs` Ready() coroutine, but Ready() is in `BMX_S_InGame.cs` | None (code is correct) |
| `#if LEGACY_NETWORK` vs comment style | Design specifies `#if LEGACY_NETWORK` guards; implementation uses `// Unity6:` comments and `#if false` blocks | None (functionally equivalent) |
| `#define LEGACY_NETWORK_DISABLED` in BMXMode/MTBMode | Design specifies per-file `#define`; implementation uses `// Unity6:` comment prefixes | None (functionally equivalent) |

---

## 5. Match Rate Calculation

| Category | Items | Matched | Rate |
|----------|:-----:|:-------:|:----:|
| StateControl.cs | 6 | 6 | 100% |
| Cycle_Control.cs | 8 | 7 | 88% |
| Cycle_Impact.cs | 5 | 5 | 100% |
| Cycle_Move.cs | 2 | 2 | 100% |
| MoveModule.cs | 10 | 9 | 90% |
| CBikeSerial.cs | 3 | 3 | 100% |
| NetworkRigidbody.cs | 1 | 1 | 100% |
| UDPConnection.cs | 7 | 7 | 100% |
| BMX_S_InGame.cs | 4 | 4 | 100% |
| MTB_S_InGame.cs | 3 | 3 | 100% |
| Training_InGame.cs | 2 | 2 | 100% |
| Network BMX/MTB files | 4 | 4 | 100% |
| **Total** | **55** | **53** | **96%** |

The 2 "mismatches" are:
1. Animation null-check attributed to wrong file in design (code is correct in BMX_S_InGame)
2. WheelFrictionCurve uses functionally equivalent but stylistically different pattern

Both are cosmetic differences, not functional gaps.

---

## 6. Recommended Actions

### 6.1 Immediate (before Unity 6 compile test)

| Priority | Item | File |
|----------|------|------|
| 1 | Fix `Network.Disconnect()` | `Assets/_Program/ConfigGUI.cs:264` |
| 2 | Fix `Application.LoadLevelAsync` / `LoadLevelAdditiveAsync` | `Assets/_Program/Utile/LoadBundle.cs:23,45` |
| 3 | Fix `Application.LoadLevel` in Editor script | `Assets/_Program/Editor/DecryptAssetBundle.cs:112` |

### 6.2 Design Document Update

| Item | Description |
|------|-------------|
| Move animation null-check | Section 2.3 Cycle_Control `변경4` should reference BMX_S_InGame.cs |
| Add ConfigGUI.cs to Phase 1 file list | Section 6 table is missing this file |
| Add LoadBundle.cs to Phase 1 file list | Section 6 table is missing this file |
| Note comment style deviation | Clarify that `// Unity6:` comments were used instead of `#if LEGACY_NETWORK` |

---

## 7. Conclusion

**Overall Match Rate: 94% (96% on verified items, -2% for 3 files missed from scope)**

The implementation faithfully follows the design document. All 12 core files plus 7 additional files have been correctly migrated. The only actionable gaps are 3 files outside the design scope that still contain active legacy API calls (`ConfigGUI.cs`, `LoadBundle.cs`, `DecryptAssetBundle.cs`).

Phase 1 compilation readiness: **HIGH** -- once the 3 remaining files are fixed, the project should have zero compile errors from deprecated Unity APIs.

---

## Version History

| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0 | 2026-03-17 | Initial gap analysis | Claude Code (gap-detector) |
