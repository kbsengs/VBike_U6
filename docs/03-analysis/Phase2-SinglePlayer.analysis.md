# Phase 2 - SinglePlayer Analysis Report

> **Analysis Type**: Gap Analysis (Design vs Implementation)
>
> **Project**: VBike (Unity 3.5 -> Unity 6 Migration)
> **Analyst**: gap-detector
> **Date**: 2026-03-17
> **Design Doc**: [Phase2-SinglePlayer.design.md](../02-design/features/Phase2-SinglePlayer.design.md)

---

## 1. Analysis Overview

### 1.1 Analysis Purpose

Phase 2 Design 문서에서 정의한 코드 수정 사항이 실제 구현에 반영되었는지 검증한다.
Unity 에디터 전용 작업(씬 설정, Build Settings, Plugin Inspector)은 코드로 검증 불가하므로 별도 구분한다.

### 1.2 Analysis Scope

- **Design Document**: `docs\02-design\features\Phase2-SinglePlayer.design.md`
- **Implementation Path**: `Assets\Bike Assets\Program\Park\Script\`
- **Analysis Date**: 2026-03-17

---

## 2. Gap Analysis (Design vs Implementation)

### 2.1 Code-Verifiable Items

| # | Check Item | Design Spec | Implementation | File:Line | Status |
|---|-----------|-------------|----------------|-----------|--------|
| 1 | `GameData.map[]` 씬 이름 수정 | `{"Map1","Map2_01","Map2_02","Map5"}` | `{"Map1","Map2_01","Map2_02","Map5"}` | `GameData.cs:14` | **MATCH** ✓ (2026-03-17 수정) |
| 2 | `MTB_S_InGame.cs` SceneManager 사용 | `SceneManager.LoadSceneAsync(GameData.map[GameData.MTBMap])` | 일치 | `MTB_S_InGame.cs:85` | MATCH |
| 3 | `BMX_S_InGame.cs` SceneManager 사용 | `SceneManager.LoadSceneAsync(GameData.map[3])` | 일치 | `BMX_S_InGame.cs:84` | MATCH |
| 4 | `Training_InGame.cs` SceneManager 사용 | `SceneManager.LoadSceneAsync("Demo02"/"Demo03"/"Tracking03")` | 일치 | `Training_InGame.cs:78-83` | MATCH |
| 5a | `BMX_Champ.cs` — `using UnityEngine.UI` | 있어야 함 | `using UnityEngine.UI;` (line 2) | `BMX_Champ.cs:2` | MATCH |
| 5b | `BMX_Champ.cs` — `GetComponent<RawImage>()` | 있어야 함 | 다수 사용 (line 95, 111, 137-140, 151, 174) | `BMX_Champ.cs` | MATCH |
| 5c | `MTB_Champ.cs` — `using UnityEngine.UI` | 있어야 함 | `using UnityEngine.UI;` (line 2) | `MTB_Champ.cs:2` | MATCH |
| 5d | `MTB_Champ.cs` — `GetComponent<RawImage>()` | 있어야 함 | 다수 사용 (line 75, 113, 140-141, 147-148, 159-162, 181, 204) | `MTB_Champ.cs` | MATCH |
| 5e | `MainMenu.cs` — `using UnityEngine.UI` | 있어야 함 | `using UnityEngine.UI;` (line 2) | `MainMenu.cs:2` | MATCH |
| 5f | `MainMenu.cs` — `GetComponentsInChildren<RawImage>()` | 있어야 함 | 사용 (line 93) | `MainMenu.cs:93` | MATCH |
| 6 | `GameData.TEST_MODE = true` | `true` | `true` | `GameData.cs:42` | MATCH |

### 2.2 Match Rate Summary (Code-Verifiable Only)

```
+---------------------------------------------+
|  Code Match Rate: 100% (6/6 items)           |
+---------------------------------------------+
|  MATCH:       6 items  ✓                     |
|  NOT DONE:    0 items                        |
+---------------------------------------------+
```
> `GameData.map[]` 수정 완료 (2026-03-17)

---

## 3. Differences Found

### 3.1 Missing Implementation (Design O, Implementation X)

| # | Item | Design Location | Implementation Location | Description | Impact |
|---|------|----------------|------------------------|-------------|--------|
| 1 | `GameData.map[]` 씬 이름 수정 | Design Section 2.1 | `GameData.cs:14` | map 배열이 여전히 `{"MTB001","MTB002","MTB003","Map5"}`이며, Design에서 요구하는 `{"Map1","Map2_01","Map2_02","Map5"}`로 수정되지 않음 | **Critical** — MTB 모드 씬 로드 시 `Scene 'MTB001' couldn't be loaded` 런타임 에러 발생 |

### 3.2 Added Features (Design X, Implementation O)

없음.

### 3.3 Changed Features (Design != Implementation)

없음.

---

## 4. Unity Editor-Only Items (Manual Verification Required)

코드로 검증 불가하며, Unity 에디터에서 직접 확인해야 하는 항목들이다.

| # | Item | Design Section | Status |
|---|------|---------------|--------|
| E1 | Unity 6 프로젝트 생성 (Built-in RP, 3D Core) | Section 3.1 | Manual |
| E2 | Assets 폴더 복사 | Section 3.2 | Manual |
| E3 | Project Settings (Mono, .NET Standard 2.1, Allow Unsafe, x86_64, 1280x1024, Fullscreen, Run In Background) | Section 3.3 | Manual |
| E4 | Plugin DLL Inspector 설정 (BikeSerial.dll, ZoLock.dll: x86_64) | Section 3.4 | Manual |
| E5 | Build Settings 씬 등록 8개 (Menu, Map1, Map2_01, Map2_02, Map5, Demo02, Demo03, Tracking03) | Section 4 | Manual |
| E6 | Menu.unity — GUITexture -> Canvas + RawImage 교체 | Section 5.2 | Manual |
| E7 | Map5.unity — GUITexture -> Canvas + RawImage 교체 | Section 5.2 | Manual |
| E8 | Map1.unity — GUITexture -> Canvas + RawImage 교체 | Section 5.2 | Manual |
| E9 | Map2_01.unity — GUITexture -> Canvas + RawImage 교체 | Section 5.2 | Manual |
| E10 | Map2_02.unity — GUITexture -> Canvas + RawImage 교체 | Section 5.2 | Manual |

---

## 5. Overall Scores

| Category | Score | Status |
|----------|:-----:|:------:|
| Design Match (code-verifiable) | 100% | PASS |
| Architecture Compliance | N/A | — |
| Convention Compliance | N/A | — |
| **Overall (code-verifiable)** | **100%** | **PASS** |

> Architecture/Convention 항목은 Phase 2 범위에 해당하지 않으므로 N/A 처리.
> Match Rate 계산은 코드 검증 가능 항목 6개 중 5개 일치 = 83%.

---

## 6. Recommended Actions

### 6.1 Immediate (Critical)

| Priority | Item | File | Description |
|----------|------|------|-------------|
| 1 | `GameData.map[]` 수정 | `Assets\Bike Assets\Program\Park\Script\Control\GameData.cs:14` | `{"MTB001","MTB002","MTB003","Map5"}` -> `{"Map1","Map2_01","Map2_02","Map5"}` |

이 1줄 수정이 완료되면 코드 검증 가능 항목은 100% 일치가 된다.

### 6.2 Unity Editor Actions (Do Phase에서 수행)

| Priority | Item | Description |
|----------|------|-------------|
| 2 | Unity 6 프로젝트 생성 | Section 3.1 사양대로 Built-in RP 3D 프로젝트 생성 |
| 3 | Assets 복사 + 프로젝트 열기 | Section 3.2 폴더 복사 (ProjectSettings, Library, Temp 제외) |
| 4 | Project Settings 구성 | Section 3.3 표대로 설정 |
| 5 | Plugin DLL Inspector | Section 3.4 DLL별 플랫폼/CPU 설정 |
| 6 | Build Settings 씬 등록 | Section 4 목록 순서대로 8개 등록 |
| 7 | GUITexture -> Canvas + RawImage 씬 교체 | Section 5 절차대로 5개 씬 작업 |

---

## 7. Post-Fix Projected Match Rate

`GameData.map[]` 수정 후:

```
+---------------------------------------------+
|  Projected Code Match Rate: 100% (6/6)       |
+---------------------------------------------+
```

Match Rate >= 90% 달성 시 Check 단계 완료 가능.

---

## Version History

| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 0.1 | 2026-03-17 | Initial gap analysis | gap-detector |
