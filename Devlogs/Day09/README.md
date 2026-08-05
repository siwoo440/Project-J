# Project J

3D 3인칭 온라인 수직 점프 경쟁 파티 게임 **Project J**의 개발 저장소입니다.

---

# 개발 환경

| 항목 | 내용 |
|---|---|
| 게임 엔진 | Unity 6 |
| Unity 버전 | 6000.3.21f1 |
| 프로젝트 템플릿 | Universal 3D |
| 렌더 파이프라인 | URP |
| 대상 플랫폼 | Steam PC |
| 개발 인원 | 1인 개발 |
| 기본 온라인 인원 | 4~8인 |
| 입력 시스템 | Unity Input System 1.20.0 |
| 테스트 프레임워크 | Unity Test Framework 1.6.0 |
| 저장소 | siwoo440/Project-J |

---

# 9일차 : 테스트 프레임워크 구성

## 개발 목표

기존 EditMode 테스트 기반을 확장하여 PlayMode 테스트, 전용 Tests 씬, 공통 로그 규칙과 명령행 반복 테스트 환경을 구성했습니다.

9일차에서는 실제 게임 기능을 새로 구현하지 않고, 이후 개발 과정에서 기능이 추가되거나 변경될 때 기존 기능의 손상을 빠르게 확인할 수 있는 자동 검증 기반을 마련했습니다.

주요 목표는 다음과 같습니다.

- 기존 EditMode 테스트 구조 유지
- PlayMode 테스트 전용 Assembly Definition 추가
- Tests 씬 전용 Runtime 마커 추가
- Tests 씬의 Build Settings 등록 상태 자동 보장
- Project J 공통 로그 접두사와 Category 규칙 정의
- 선택적 오류 코드 정규화 규칙 정의
- 예상 오류 로그를 검사하는 EditMode 테스트 추가
- 실제 씬 로드와 프레임 진행을 검사하는 PlayMode 테스트 추가
- Unity Test Runner에서 EditMode·PlayMode 테스트 분리 실행
- PowerShell을 이용한 EditMode·PlayMode 반복 실행
- XML 테스트 결과와 로그 파일 자동 저장

---

# 검토 기준 커밋

| 항목 | 내용 |
|---|---|
| 검토 기준 커밋 | `527c36a9e0aba50b6a17092f971acafd238f0df9` |
| 커밋 제목 | `9일차 : 테스트 프레임워크 구성` |
| 브랜치 | `main` |
| 이전 커밋 | `f88df27691b084f2aa6edc7836614a08eaa2c1df` |
| 검토 시각 | 2026-08-05 |
| 상태 | 자동 테스트 스크립트 경로 정정 후 완료 |

경로 정정과 본 개발 일지를 기존 커밋에 amend하면 최종 커밋 SHA는 변경됩니다.

---

# 최신 커밋 검토 결과

최신 커밋에서 다음 항목을 확인했습니다.

- `9일차 : 테스트 프레임워크 구성` 커밋 제목 정상
- `Tests.unity`에 `ProjectJ_TestSceneRoot` 추가
- `ProjectTestSceneMarker` 연결과 Framework Version 1 적용
- Runtime 공통 로그 Category 추가
- Runtime 공통 로그 출력 형식 추가
- Runtime 테스트 프레임워크 상수 추가
- Runtime Tests 씬 마커 추가
- Day 09 Editor 자동 구성·검증 메뉴 추가
- EditMode 공통 로그 테스트 4개 추가
- PlayMode 테스트 Assembly Definition 추가
- PlayMode Tests 씬 Smoke 테스트 4개 추가
- 각 Unity 에셋과 스크립트의 `.meta` 파일 등록
- PowerShell 자동 테스트 스크립트 내용 확인

저장소에서 확인 가능한 C#·asmdef·씬 참조 구조에서는 치명적인 문제가 발견되지 않았습니다.

다만 PowerShell 파일은 최초 적용 과정에서 다음 위치에 들어갔습니다.

```text
Assets/_ProjectJ/Tests/Run-ProjectJTests.ps1
```

스크립트는 `$PSScriptRoot\..\..`를 프로젝트 루트로 계산하므로 이 위치에서는 실제로 `Assets` 폴더가 계산됩니다.

```text
현재 스크립트 폴더:
<ProjectRoot>/Assets/_ProjectJ/Tests

현재 계산 결과:
<ProjectRoot>/Assets
```

이후 스크립트가 `<ProjectRoot>/Assets/Assets` 존재 여부를 검사하므로 기본 실행 시 다음 오류가 발생할 수 있습니다.

```text
올바른 Unity 프로젝트 경로가 아닙니다.
```

최종 위치는 다음으로 정정합니다.

```text
Tools/Tests/Run-ProjectJTests.ps1
```

이 위치에서는 `$PSScriptRoot\..\..` 계산 결과가 정확히 Unity 프로젝트 루트가 됩니다.

```text
스크립트 폴더:
<ProjectRoot>/Tools/Tests

계산 결과:
<ProjectRoot>
```

---

# 최종 파일 배치

## Runtime 로그 구조

```text
Assets/_ProjectJ/Scripts/Runtime/Common/Diagnostics
├─ ProjectLogCategory.cs
└─ ProjectLog.cs
```

## Runtime 테스트 구조

```text
Assets/_ProjectJ/Scripts/Runtime/Common/Testing
├─ ProjectTestFramework.cs
└─ ProjectTestSceneMarker.cs
```

## Editor 구성 도구

```text
Assets/_ProjectJ/Scripts/Editor
└─ Day09TestFrameworkSetupTool.cs
```

## EditMode 테스트

```text
Assets/_ProjectJ/Tests/EditMode
└─ ProjectLogTests.cs
```

## PlayMode 테스트

```text
Assets/_ProjectJ/Tests/PlayMode
├─ ProjectJ.Tests.PlayMode.asmdef
└─ TestScenePlayModeTests.cs
```

## 명령행 테스트 도구

```text
Tools/Tests
└─ Run-ProjectJTests.ps1
```

PowerShell 파일은 Unity 에셋이 아니므로 `Assets` 밖에서 관리합니다. 따라서 해당 파일에는 Unity `.meta` 파일이 필요하지 않습니다.

---

# 공통 로그 규칙

## 1. 기본 로그 형식

코드가 없는 일반 로그는 다음 형식을 사용합니다.

```text
[ProjectJ][Category] Message
```

예시:

```text
[ProjectJ][Core] Initialization complete.
```

오류 코드 또는 이벤트 코드가 있는 로그는 다음 형식을 사용합니다.

```text
[ProjectJ][Category][CODE] Message
```

예시:

```text
[ProjectJ][Test][TEST_FRAMEWORK_READY] EditMode·PlayMode 테스트 프레임워크 구성을 완료했습니다.
```

---

## 2. 로그 Category

| Category | 용도 |
|---|---|
| Core | 공통 초기화와 핵심 서비스 |
| Scene | 씬 전환과 씬 상태 |
| Input | 입력 시스템 |
| Data | 데이터 로드와 검증 |
| Physics | 물리와 충돌 |
| Gameplay | 게임 플레이 |
| UI | 화면과 UI |
| Audio | 오디오 |
| Network | 네트워크 |
| Test | 자동 테스트와 Tests 씬 |

---

## 3. 로그 코드 규칙

로그 코드는 다음 기준을 사용합니다.

```text
영문 대문자
단어 사이 밑줄
기능 또는 시스템 이름 포함
```

예시:

```text
TEST_SCENE_READY
TEST_FRAMEWORK_VALID
TEST_FRAMEWORK_INVALID
TEST_PLAYMODE_LOG
```

`ProjectLog`에 공백이나 소문자가 포함된 코드를 전달하면 앞뒤 공백을 제거하고 대문자 밑줄 형식으로 변환합니다.

```text
입력: " test smoke "
결과: TEST_SMOKE
```

---

# ProjectLog

## 4. 공통 로그 출력

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Common/Diagnostics/ProjectLog.cs
```

제공 기능:

```text
Format
Info
Warning
Error
```

각 출력 메서드는 선택적으로 관련 Unity Object를 문맥으로 전달할 수 있습니다.

예시:

```csharp
ProjectLog.Info(
    ProjectLogCategory.Test,
    "Test framework validation passed.",
    "TEST_FRAMEWORK_VALID");
```

---

## 5. 빈 메시지 처리

공백 또는 null 메시지가 전달되면 다음 문구를 사용합니다.

```text
(no message)
```

예상 결과:

```text
[ProjectJ][Data][DATA_EMPTY] (no message)
```

---

# 테스트 프레임워크 공통 상수

## 6. ProjectTestFramework

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Common/Testing/ProjectTestFramework.cs
```

주요 값:

```text
Framework Version: 1
Scene Root Name: ProjectJ_TestSceneRoot
Smoke Category: Smoke
Scene Category: Scene
Logging Category: Logging
```

테스트 씬과 테스트 코드가 동일한 이름과 버전을 사용하도록 공통 상수로 관리합니다.

---

# Tests 씬

## 7. ProjectJ_TestSceneRoot

수정된 씬:

```text
Assets/_ProjectJ/Scenes/Game/Tests.unity
```

추가 구조:

```text
ProjectJ_TestSceneRoot
└─ ProjectTestSceneMarker
```

기존 Tests 씬의 다음 오브젝트는 유지했습니다.

```text
Main Camera
Directional Light
ProjectJ_SceneFlowDebug
ProjectJ_InputDebug
```

---

## 8. ProjectTestSceneMarker

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Common/Testing/ProjectTestSceneMarker.cs
```

Tests 씬 로드 시 `Awake`에서 다음 값을 기록합니다.

```text
Loaded Scene Name
Initialization Frame
Is Initialized
Framework Version
```

초기화 완료 로그:

```text
[ProjectJ][Test][TEST_SCENE_READY] Test scene marker initialized.
```

PlayMode 테스트는 이 값을 확인하여 단순한 씬 파일 존재 여부가 아니라 실제 Runtime 초기화 여부를 검증합니다.

---

# Editor 자동 구성

## 9. Day09TestFrameworkSetupTool

파일 위치:

```text
Assets/_ProjectJ/Scripts/Editor/Day09TestFrameworkSetupTool.cs
```

Unity 상단 메뉴:

```text
Project J
└─ Day 09
   ├─ Configure Test Framework
   └─ Validate Test Framework
```

Play Mode 진입 중이거나 실행 중일 때는 메뉴를 비활성화합니다.

---

## 10. Configure Test Framework

다음 작업을 자동으로 수행합니다.

```text
EditMode asmdef 확인
→ PlayMode asmdef 확인
→ Tests 씬 확인
→ Build Settings 등록 확인
→ Tests 씬 단독 열기
→ ProjectJ_TestSceneRoot 확인
→ ProjectTestSceneMarker 추가 또는 재사용
→ Framework Version 적용
→ Tests 씬 저장
→ 구성 전체 재검증
```

정상 로그:

```text
[ProjectJ][Test][TEST_FRAMEWORK_READY] EditMode·PlayMode 테스트 프레임워크 구성을 완료했습니다.
```

---

## 11. Validate Test Framework

다음 항목을 검사합니다.

```text
EditMode 테스트 Assembly Definition 존재
PlayMode 테스트 Assembly Definition 존재
Tests 씬 존재
Tests 씬 Build Settings 활성 등록
ProjectJ_TestSceneRoot 정확히 1개
ProjectTestSceneMarker 존재
Framework Version 일치
```

정상 로그:

```text
[ProjectJ][Test][TEST_FRAMEWORK_VALID] 테스트 프레임워크 검증을 통과했습니다.
```

---

# EditMode 테스트

## 12. ProjectLogTests

파일 위치:

```text
Assets/_ProjectJ/Tests/EditMode/ProjectLogTests.cs
```

추가 테스트 수:

```text
4개
```

### FormatIncludesProjectPrefixCategoryAndMessage

기본 로그 문자열을 검사합니다.

```text
[ProjectJ][Core] Initialization complete.
```

### FormatNormalizesOptionalCode

로그 코드의 공백 제거와 대문자 밑줄 변환을 검사합니다.

```text
 test smoke
→ TEST_SMOKE
```

### EmptyMessageUsesFallbackText

빈 메시지가 `(no message)`로 변환되는지 검사합니다.

### ExpectedErrorLogDoesNotFailTest

의도적으로 발생시키는 Error 로그를 `LogAssert.Expect`로 먼저 등록하면 테스트가 실패하지 않는지 검사합니다.

---

# PlayMode 테스트

## 13. ProjectJ.Tests.PlayMode.asmdef

파일 위치:

```text
Assets/_ProjectJ/Tests/PlayMode/ProjectJ.Tests.PlayMode.asmdef
```

주요 설정:

```text
Name: ProjectJ.Tests.PlayMode
Root Namespace: ProjectJ.Tests.PlayMode
Reference: ProjectJ.Runtime
Optional Unity Reference: TestAssemblies
Platform 제한: 없음
```

PlayMode 테스트가 Runtime 코드와 Tests 씬 마커를 참조할 수 있도록 구성했습니다.

---

## 14. TestScenePlayModeTests

파일 위치:

```text
Assets/_ProjectJ/Tests/PlayMode/TestScenePlayModeTests.cs
```

추가 테스트 수:

```text
4개
```

각 테스트 전에 Tests 씬을 `LoadSceneMode.Single`로 비동기 로드하고 한 프레임을 기다립니다.

### TestsSceneLoadsAsActiveScene

Tests 씬이 활성 씬으로 정상 로드되는지 검사합니다.

### TestSceneMarkerInitializesSuccessfully

다음 상태를 검사합니다.

```text
ProjectTestSceneMarker 존재
IsInitialized = true
Framework Version = 1
Loaded Scene Name = Tests
```

### RuntimeAdvancesAcrossFrames

한 프레임 대기 후 `Time.frameCount`가 증가하는지 검사합니다.

### ProjectLogWritesExpectedMessageInPlayMode

PlayMode에서 공통 로그가 예상 형식으로 출력되는지 검사합니다.

---

# 테스트 수

기존 EditMode 테스트:

```text
2일차: 2개
3일차: 3개
4일차: 4개
5일차: 6개
6일차: 8개
7일차: 8개
8일차: 8개
합계: 39개
```

9일차 신규 테스트:

```text
EditMode: 4개
PlayMode: 4개
```

예상 최종 결과:

```text
EditMode Passed: 43
PlayMode Passed: 4
전체 Passed: 47
Failed: 0
Ignored: 0
```

GitHub Actions 상태 검사는 아직 등록되지 않았으므로 실제 테스트 통과 여부는 로컬 Unity Test Runner 또는 PowerShell 스크립트 실행으로 확인해야 합니다.

---

# PowerShell 자동 테스트

## 15. 최종 스크립트 위치

```text
Tools/Tests/Run-ProjectJTests.ps1
```

기본 실행:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\Tests\Run-ProjectJTests.ps1
```

Unity 설치 경로가 다른 경우:

```powershell
powershell -ExecutionPolicy Bypass `
    -File .\Tools\Tests\Run-ProjectJTests.ps1 `
    -UnityPath "D:\Unity\Hub\Editor\6000.3.21f1\Editor\Unity.exe"
```

---

## 16. 자동 실행 순서

```text
Unity 실행 파일 확인
→ 프로젝트 루트 확인
→ 결과 폴더 생성
→ EditMode 테스트 실행
→ PlayMode 테스트 실행
→ 두 종료 코드 확인
→ 성공 또는 실패 반환
```

Unity 에디터에서 같은 프로젝트가 열려 있으면 명령행 실행 전에 종료합니다.

---

## 17. 테스트 결과 파일

결과 위치:

```text
Library/ProjectJTestResults
```

생성 파일:

```text
EditModeResults.xml
PlayModeResults.xml
EditMode.log
PlayMode.log
```

`Library` 폴더는 일반 Unity `.gitignore`에서 제외되므로 테스트 결과를 저장소에 커밋하지 않습니다.

---

# 파일 위치 검증 결과

| 파일 | 적용 위치 | 결과 |
|---|---|:---:|
| `ProjectLogCategory.cs` | `Scripts/Runtime/Common/Diagnostics` | 정상 |
| `ProjectLog.cs` | `Scripts/Runtime/Common/Diagnostics` | 정상 |
| `ProjectTestFramework.cs` | `Scripts/Runtime/Common/Testing` | 정상 |
| `ProjectTestSceneMarker.cs` | `Scripts/Runtime/Common/Testing` | 정상 |
| `Day09TestFrameworkSetupTool.cs` | `Scripts/Editor` | 정상 |
| `ProjectLogTests.cs` | `Tests/EditMode` | 정상 |
| `ProjectJ.Tests.PlayMode.asmdef` | `Tests/PlayMode` | 정상 |
| `TestScenePlayModeTests.cs` | `Tests/PlayMode` | 정상 |
| `ProjectJ_TestSceneRoot` | `Scenes/Game/Tests.unity` | 정상 |
| `Run-ProjectJTests.ps1` | `Assets/_ProjectJ/Tests` | 이동 필요 |
| `Run-ProjectJTests.ps1` 최종 위치 | `Tools/Tests` | 정상 |

---

# 로컬 최종 확인 항목

```text
Unity Console Error: 0개
Project J → Day 09 → Validate Test Framework 통과
EditMode Passed: 43개
PlayMode Passed: 4개
Failed: 0개
PowerShell EditMode 종료 코드: 0
PowerShell PlayMode 종료 코드: 0
```

저장소 코드만으로는 Unity Console과 실제 테스트 실행 결과를 확인할 수 없으므로 위 결과는 로컬 Unity에서 검증해야 합니다.

---

# 최종 프로젝트 구조

```text
Assets/_ProjectJ
├─ Scenes
│  └─ Game
│     └─ Tests.unity
├─ Scripts
│  ├─ Runtime
│  │  └─ Common
│  │     ├─ Diagnostics
│  │     │  ├─ ProjectLogCategory.cs
│  │     │  └─ ProjectLog.cs
│  │     └─ Testing
│  │        ├─ ProjectTestFramework.cs
│  │        └─ ProjectTestSceneMarker.cs
│  └─ Editor
│     └─ Day09TestFrameworkSetupTool.cs
└─ Tests
   ├─ EditMode
   │  └─ ProjectLogTests.cs
   └─ PlayMode
      ├─ ProjectJ.Tests.PlayMode.asmdef
      └─ TestScenePlayModeTests.cs

Tools
└─ Tests
   └─ Run-ProjectJTests.ps1

Devlogs
└─ Day09
   └─ README.md
```

---

# 다음 개발 방향

## 10일차 : 개발 빌드 프로필

다음 일차에는 개발 클라이언트와 향후 서버 빌드를 위한 Build Profile을 구성합니다.

주요 작업:

```text
Development Build 프로필 생성
개발 클라이언트 씬 목록 연결
로그 저장 위치 확인
개발 빌드 실행
향후 Dedicated Server 프로필 확장 기반 준비
```

완료 기준:

```text
Development Build가 실행되고 로그가 저장된다.
```

---

# 커밋 정보

```text
9일차 : 테스트 프레임워크 구성
```

경로 정정과 개발 일지를 기존 커밋에 포함할 때:

```bash
mkdir Tools\Tests
git mv Assets/_ProjectJ/Tests/Run-ProjectJTests.ps1 Tools/Tests/Run-ProjectJTests.ps1
git rm Assets/_ProjectJ/Tests/Run-ProjectJTests.ps1.meta
git add Devlogs/Day09/README.md
git status
git commit --amend --no-edit
git push --force-with-lease origin main
git status
```
