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
| 개발 빌드 대상 | Windows 64-bit |
| 테스트 프레임워크 | Unity Test Framework 1.6.0 |

---

# 10일차 : 개발 빌드 프로필 구성

## 개발 목표

Windows 개발 클라이언트를 반복적으로 생성하고 실행할 수 있도록 Unity Build Profile과 자동 빌드 도구를 구성했습니다.

글로벌 빌드 씬 목록에는 자동 테스트 전용 `Tests.unity`가 포함되어 있으므로, 개발 Build Profile에서는 전용 씬 목록을 사용하여 실제 게임용 씬 5개만 포함하도록 분리했습니다.

이번 일차의 주요 목표는 다음과 같습니다.

- Windows 개발 Build Profile 생성
- Build Profile 에셋 버전 관리
- 게임용 씬 5개만 빌드
- Tests 씬 제외
- `PROJECTJ_DEVELOPMENT` 스크립팅 정의 추가
- Development Build와 Script Debugging 활성화
- Autoconnect Profiler와 Deep Profiling 비활성화
- LZ4 압축 개발 빌드
- 에디터 빌드·빌드 후 실행 메뉴 추가
- 빌드 결과 요약 로그 저장
- 플레이어 실행 시 실제 `Player.log` 경로 출력
- PowerShell 명령행 빌드 지원
- Build Profile 설정 EditMode 테스트 추가

---

# 검토 기준 커밋

| 항목 | 내용 |
|---|---|
| 커밋 제목 | `10일차 : 개발 빌드 프로필 구성` |
| 커밋 SHA | `a9b748ffe662750e55c0b80434eaf36dfa21f278` |
| 브랜치 | `main` |
| 이전 커밋 | `d294e4ad92fa56888cc1597b5be9d6e014dd7ee5` |
| 검토 결과 | 치명적인 구조 문제 없음 |

---

# 최신 커밋 검토 결과

최신 커밋에서 다음 항목을 확인했습니다.

- Windows 개발 Build Profile 에셋 추가
- Build Profile의 글로벌 씬 목록 오버라이드 활성화
- Bootstrap부터 Game까지 게임용 씬 5개 등록
- Tests 씬 제외
- `PROJECTJ_DEVELOPMENT` 정의 추가
- Runtime 빌드 설정 상수 추가
- Runtime 빌드 정보와 Player 로그 경로 출력 기능 추가
- Build Profile 검증기 추가
- 에디터 개발 빌드 자동화 메뉴 추가
- 빌드 결과 요약 로그 저장 기능 추가
- Build Profile EditMode 테스트 8개 추가
- PowerShell Windows 개발 빌드 도구 추가

저장소에서 확인 가능한 코드와 에셋 구조에서는 10일차 진행을 막는 치명적인 문제가 발견되지 않았습니다.

GitHub Actions나 Unity 자동 빌드 상태 검사는 아직 등록되어 있지 않으므로 실제 컴파일, Test Runner와 Windows 플레이어 빌드 성공 여부는 로컬 Unity에서 최종 확인해야 합니다.

---

# Unity가 함께 저장한 설정 파일

Build Profile 생성과 활성화 과정에서 다음 기존 설정 파일도 변경되었습니다.

```text
Assets/_ProjectJ/Settings/Rendering/DefaultVolumeProfile.asset
Assets/_ProjectJ/Settings/Rendering/PC_RPAsset.asset
Assets/_ProjectJ/Settings/Rendering/UniversalRenderPipelineGlobalSettings.asset
ProjectSettings/ProjectSettings.asset
ProjectSettings/UnityConnectSettings.asset
```

렌더링 설정 파일에는 현재 Unity 버전의 직렬화 필드가 추가되거나 순서가 다시 저장됐습니다.

이 파일들은 10일차 핵심 구현 파일은 아니므로 다음 항목을 로컬에서 확인합니다.

```text
Game 씬 화면이 이전과 동일함
URP 렌더링 오류 없음
Volume 관련 Console 경고 없음
Unity Services 활성 상태가 의도와 일치함
```

이상이 없다면 현재 변경을 유지합니다.

---

# Windows 개발 Build Profile

## Build Profile 에셋

```text
Assets/_ProjectJ/Settings/BuildProfiles/ProjectJ_Windows_Development.asset
```

에셋 이름:

```text
ProjectJ_Windows_Development
```

Build Profile은 Windows 개발 클라이언트의 씬 목록과 빌드별 스크립팅 정의를 저장합니다.

---

# 개발 클라이언트 씬 목록

## 글로벌 씬 목록

현재 글로벌 씬 목록:

```text
Bootstrap
MainMenu
Lobby
MatchLoading
Game
Tests
```

## 개발 Profile 전용 목록

```text
0. Bootstrap
1. MainMenu
2. Lobby
3. MatchLoading
4. Game
```

제외:

```text
Tests
```

`Bootstrap`은 빌드 인덱스 0으로 유지하여 게임 실행 직후 공통 서비스 초기화 흐름이 시작되도록 구성했습니다.

---

# 개발 전용 정의

Build Profile에 다음 스크립팅 정의를 추가했습니다.

```text
PROJECTJ_DEVELOPMENT
```

향후 다음 기능을 개발 Profile에서만 활성화할 때 사용할 수 있습니다.

```text
개발자용 화면
디버그 패널
네트워크 상태 표시
추가 진단 로그
테스트 명령
```

Unity가 제공하는 `DEVELOPMENT_BUILD`와 프로젝트가 추가한 `PROJECTJ_DEVELOPMENT`는 서로 다른 용도로 사용합니다.

---

# Runtime 빌드 설정

## ProjectBuildConfiguration

파일:

```text
Assets/_ProjectJ/Scripts/Runtime/Common/Build/ProjectBuildConfiguration.cs
```

관리 항목:

```text
Build Profile 이름과 경로
개발 전용 스크립팅 정의
개발 실행 파일 출력 경로
빌드 로그 경로
빌드 요약 로그 경로
게임용 씬 목록
Tests 씬 경로
```

개발 실행 파일:

```text
Builds/Windows/Development/ProjectJ_Development.exe
```

Unity 명령행 빌드 로그:

```text
Logs/Builds/Windows/DevelopmentBuild.log
```

빌드 결과 요약:

```text
Logs/Builds/Windows/DevelopmentBuildSummary.log
```

---

# Runtime 빌드 정보 로그

## ProjectBuildRuntimeReporter

파일:

```text
Assets/_ProjectJ/Scripts/Runtime/Common/Build/ProjectBuildRuntimeReporter.cs
```

첫 씬 로드 전에 다음 정보를 출력합니다.

```text
Build Type
Unity Version
Application Version
Build GUID
Player.log 경로
```

로그 코드:

```text
BUILD_RUNTIME_INFO
BUILD_LOG_PATH
```

실행 환경 표시:

| 실행 환경 | 값 |
|---|---|
| Unity Editor | Editor |
| Development Build | Development |
| 일반 Player | Release |

---

# Build Profile 검증기

## ProjectDevelopmentBuildValidator

파일:

```text
Assets/_ProjectJ/Scripts/Editor/Build/ProjectDevelopmentBuildValidator.cs
```

검사 항목:

```text
Build Profile 에셋 존재
에셋 경로와 이름
Override Global Scene List
게임 씬 목록과 순서
Tests 씬 제외
PROJECTJ_DEVELOPMENT 정의
현재 활성 Profile
Windows 64-bit 빌드 대상
Development Build
Script Debugging
Autoconnect Profiler 비활성화
Deep Profiling 비활성화
Wait for Managed Debugger 비활성화
```

잘못된 설정은 `BUILD_PROFILE_INVALID` 오류 코드로 출력합니다.

---

# Editor 개발 빌드 메뉴

## Day10DevelopmentBuildTool

파일:

```text
Assets/_ProjectJ/Scripts/Editor/Day10DevelopmentBuildTool.cs
```

Unity 메뉴:

```text
Project J
└─ Day 10
   ├─ Configure Development Profile
   ├─ Validate Development Profile
   ├─ Build Development Client
   ├─ Build and Run Development Client
   └─ Open Latest Build Summary
```

## Configure Development Profile

자동 적용:

```text
프로필 전용 씬 목록 활성화
게임 씬 5개 등록
Tests 씬 제외
PROJECTJ_DEVELOPMENT 추가
프로필 활성화
Development Build 활성화
Script Debugging 활성화
Autoconnect Profiler 비활성화
Deep Profiling 비활성화
Wait for Managed Debugger 비활성화
```

성공 로그:

```text
[ProjectJ][Core][BUILD_PROFILE_READY]
```

## Validate Development Profile

성공 로그:

```text
[ProjectJ][Core][BUILD_PROFILE_VALID]
```

## Build Development Client

적용 빌드 옵션:

```text
Development
AllowDebugging
CompressWithLz4
StrictMode
DetailedBuildReport
```

## Build and Run Development Client

위 옵션에 다음을 추가합니다.

```text
AutoRunPlayer
```

---

# 빌드 요약 로그

빌드 후 다음 파일을 생성합니다.

```text
Logs/Builds/Windows/DevelopmentBuildSummary.log
```

저장 내용:

```text
빌드 시작·완료 시각
Unity 버전
Build Profile 경로
빌드 결과
빌드 대상
빌드 옵션
출력 경로
빌드 시간
빌드 크기
자동 실행 요청 여부
Development Build 상태
Script Debugging 상태
Profiler 관련 설정
빌드 씬 목록
```

Unity 메뉴에서 열기:

```text
Project J
→ Day 10
→ Open Latest Build Summary
```

---

# EditMode 테스트

## DevelopmentBuildProfileTests

파일:

```text
Assets/_ProjectJ/Tests/EditMode/DevelopmentBuildProfileTests.cs
```

추가 테스트 8개:

1. 개발 빌드 출력·로그 경로 검사
2. 게임 씬 5개 순서 검사
3. Tests 씬 제외 검사
4. 필수 게임 씬 에셋 존재 검사
5. Build Profile 에셋 경로 검사
6. 글로벌 씬 목록 오버라이드 검사
7. 실제 Profile 씬 순서 검사
8. `PROJECTJ_DEVELOPMENT` 포함 검사

예상 테스트 결과:

```text
EditMode Passed: 51
PlayMode Passed: 4
전체 Passed: 55
Failed: 0
Ignored: 0
```

---

# PowerShell 개발 빌드

## Build-ProjectJDevelopment.ps1

파일:

```text
Tools/Builds/Build-ProjectJDevelopment.ps1
```

검사 항목:

```text
Unity.exe 존재
Unity 프로젝트 경로
Build Profile 에셋 존재
로그 폴더 생성
Unity 종료 코드
개발 실행 파일 생성
```

기본 실행:

```powershell
powershell -ExecutionPolicy Bypass `
    -File .\Tools\Builds\Build-ProjectJDevelopment.ps1
```

호출 Editor 메서드:

```text
ProjectJ.Editor.Day10DevelopmentBuildTool.BuildDevelopmentClientFromCommandLine
```

정상 결과:

```text
Unity 종료 코드: 0
Windows 개발 빌드 성공
```

---

# 생성·수정된 주요 파일

## 새 Runtime 파일

```text
Assets/_ProjectJ/Scripts/Runtime/Common/Build
├─ ProjectBuildConfiguration.cs
└─ ProjectBuildRuntimeReporter.cs
```

## 새 Editor 파일

```text
Assets/_ProjectJ/Scripts/Editor
├─ Day10DevelopmentBuildTool.cs
└─ Build
   └─ ProjectDevelopmentBuildValidator.cs
```

## 새 Build Profile

```text
Assets/_ProjectJ/Settings/BuildProfiles
└─ ProjectJ_Windows_Development.asset
```

## 새 테스트

```text
Assets/_ProjectJ/Tests/EditMode
└─ DevelopmentBuildProfileTests.cs
```

## 새 명령행 도구

```text
Tools/Builds
└─ Build-ProjectJDevelopment.ps1
```

---

# 로컬 최종 검증 절차

## Build Profile

```text
File
→ Build Profiles
→ ProjectJ_Windows_Development
```

확인:

```text
Active Profile
Windows 64-bit
Override Global Scene List 활성화
게임용 씬 5개
Tests 씬 제외
PROJECTJ_DEVELOPMENT
Development Build
Script Debugging
Autoconnect Profiler 비활성화
Deep Profiling 비활성화
Wait for Managed Debugger 비활성화
```

## Profile 검증

```text
Project J
→ Day 10
→ Validate Development Profile
```

## Test Runner

```text
EditMode Passed: 51
PlayMode Passed: 4
Failed: 0
```

## 개발 빌드

```text
Project J
→ Day 10
→ Build Development Client
```

생성 파일:

```text
Builds/Windows/Development/ProjectJ_Development.exe
```

## 플레이어 로그

검색할 코드:

```text
BUILD_RUNTIME_INFO
BUILD_LOG_PATH
```

## 명령행 빌드

```powershell
powershell -ExecutionPolicy Bypass `
    -File .\Tools\Builds\Build-ProjectJDevelopment.ps1
```

---

# 검증 결과

| 검증 항목 | 저장소 확인 |
|---|:---:|
| 최신 커밋 제목 정상 | 완료 |
| Windows Build Profile 추가 | 완료 |
| Build Profile 고정 경로 | 완료 |
| 전용 씬 목록 오버라이드 | 완료 |
| 게임용 씬 5개 등록 | 완료 |
| Tests 씬 제외 | 완료 |
| PROJECTJ_DEVELOPMENT 추가 | 완료 |
| Runtime 빌드 설정 | 완료 |
| Runtime 빌드 정보 로그 | 완료 |
| Build Profile 검증기 | 완료 |
| Editor 빌드 메뉴 | 완료 |
| 빌드 요약 로그 | 완료 |
| EditMode 테스트 8개 | 완료 |
| PowerShell 빌드 도구 | 완료 |
| GitHub CI 상태 검사 | 미구성 |

로컬 최종 확인:

```text
Console Error: 0개
Build Profile 검증 통과
EditMode 51개 통과
PlayMode 4개 통과
Windows 개발 빌드 성공
실행 파일 정상 실행
Player.log 정보 출력
PowerShell 종료 코드 0
```

---

# 다음 개발 방향

## 11일차 : 설정 저장 구조

다음 일차에는 다음 사용자 설정 모델과 저장 구조를 구성합니다.

```text
그래픽
마스터 음량
BGM 음량
SFX 음량
입력
접근성
지역과 언어
카메라 감도
카메라 반전
```

완료 기준:

```text
설정을 변경하고 게임을 다시 실행해도 저장된 값이 복원된다.
```

---

# 커밋 정보

```text
10일차 : 개발 빌드 프로필 구성
```

```text
https://github.com/siwoo440/Project-J/commit/a9b748ffe662750e55c0b80434eaf36dfa21f278
```
