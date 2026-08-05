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
| 저장소 | siwoo440/Project-J |

---

# 4일차 : 공통 서비스 초기화

## 개발 목표

게임의 여러 씬에서 공통으로 사용하는 설정, 저장, 오디오와 데이터 검증 기능을 서비스로 분리하고, `Bootstrap` 씬에서 정해진 순서대로 한 번만 생성·초기화하는 기반을 구성했습니다.

공통 서비스 초기화가 성공한 경우에만 `MainMenu`로 이동하도록 변경하여, 이후 설정이나 데이터 초기화에 실패했을 때 게임이 불완전한 상태로 진행되지 않도록 했습니다.

이번 일차의 핵심 목표는 다음과 같습니다.

- 공통 서비스 인터페이스와 상태 규칙 정의
- 서비스 형식별 단일 인스턴스 등록
- 초기화 순서 관리
- 중복 등록과 중복 초기화 방지
- 설정·저장·오디오·데이터 검증 서비스 생성
- 초기화 실패 시 MainMenu 전환 중단
- Bootstrap 공통 서비스 구성 자동화
- EditMode 자동 테스트 추가

---

## 최신 커밋

| 항목 | 내용 |
|---|---|
| 커밋 제목 | `4일차 : 공통 서비스 초기화` |
| 커밋 SHA | `f6d98e979a59bf456824b5bd96022130de234374` |
| 브랜치 | `main` |
| 이전 커밋 | `e921e8ac49c680afb1778ab0b48ccafb4c5168e6` |
| 커밋 링크 | https://github.com/siwoo440/Project-J/commit/f6d98e979a59bf456824b5bd96022130de234374 |

---

# 최신 커밋 검토 결과

최신 커밋을 기준으로 다음 항목을 확인했습니다.

- 커밋 제목이 `4일차 : 공통 서비스 초기화`로 정상 등록
- `Bootstrap` 씬의 `ProjectJ_Bootstrap` 오브젝트에 `CommonServiceInitializer` 추가
- `BootstrapEntryPoint`에서 공통 서비스 초기화 후 MainMenu 전환
- 공통 서비스 상태 enum 추가
- 공통 서비스 인터페이스와 기본 클래스 추가
- 서비스 등록·조회·초기화 순서 관리용 Registry 추가
- 설정, 저장, 오디오와 데이터 검증 서비스 추가
- Bootstrap 공통 서비스 구성용 Editor 도구 추가
- 서비스 중복 등록과 초기화 순서를 검증하는 EditMode 테스트 추가
- 관련 `.meta` 파일과 Unity 씬 참조 정상 반영

저장소에서 확인 가능한 범위에서는 수정이 필요한 치명적인 구조 오류를 발견하지 못했습니다.

GitHub Actions와 자동 CI 상태 검사가 아직 구성되지 않았으므로 다음 항목은 로컬 Unity 에디터에서 최종 확인해야 합니다.

```text
Console Error: 0개
EditMode Passed: 9개
EditMode Failed: 0개
Bootstrap → MainMenu 전환 정상
공통 서비스 등록 수: 4개
```

---

# 구현 내용

## 1. 공통 서비스 상태 정의

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Services/GameServiceState.cs
```

모든 공통 서비스가 다음 네 가지 상태 중 하나를 가지도록 구성했습니다.

```text
NotInitialized
Initializing
Initialized
Failed
```

| 상태 | 의미 |
|---|---|
| NotInitialized | 아직 초기화를 시작하지 않은 상태 |
| Initializing | 현재 초기화가 진행 중인 상태 |
| Initialized | 초기화가 정상적으로 완료된 상태 |
| Failed | 초기화 과정에서 예외가 발생한 상태 |

각 서비스의 초기화 상태를 명확하게 추적할 수 있어, 준비되지 않은 서비스를 사용하는 문제와 초기화 실패 원인을 구분할 수 있습니다.

---

## 2. IGameService 인터페이스 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Services/IGameService.cs
```

모든 공통 서비스가 동일한 규칙을 따르도록 다음 항목을 정의했습니다.

```text
ServiceName
InitializationOrder
State
Initialize()
```

| 항목 | 역할 |
|---|---|
| ServiceName | 로그와 진단에 사용할 서비스 이름 |
| InitializationOrder | 서비스 초기화 순서 |
| State | 현재 초기화 상태 |
| Initialize | 서비스 초기화 실행 |

앞으로 새로운 공통 서비스가 추가되더라도 이 인터페이스를 기준으로 Registry에 등록할 수 있습니다.

---

## 3. GameServiceBase 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Services/GameServiceBase.cs
```

각 서비스에 반복되는 상태 전환과 예외 처리를 공통 기본 클래스에서 처리하도록 구성했습니다.

초기화 흐름:

```text
Initialize 호출
→ 이미 Initialized 상태인지 확인
→ Initializing 상태로 변경
→ 파생 서비스의 OnInitialize 실행
→ 성공 시 Initialized
→ 실패 시 Failed
```

같은 서비스에서 `Initialize()`를 여러 번 호출하더라도 실제 초기화 로직은 한 번만 실행됩니다.

초기화 도중 다시 같은 서비스를 초기화하려는 경우에는 재진입 오류로 판단하여 예외를 발생시킵니다.

---

## 4. GameServiceRegistry 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Services/GameServiceRegistry.cs
```

프로젝트의 공통 서비스를 형식별로 한 개씩 등록하고 조회하는 Registry를 구현했습니다.

주요 기능:

```text
Contains<T>()
Register<T>()
Get<T>()
TryGet<T>()
InitializeAll()
```

### 서비스 단일 등록

같은 서비스 형식은 한 번만 등록할 수 있습니다.

```text
첫 번째 SettingsService 등록 → 성공
두 번째 SettingsService 등록 → 거부
```

### 서비스 조회

등록된 서비스를 다음과 같이 조회할 수 있습니다.

```csharp
SettingsService settingsService =
    GameServiceRegistry.Get<SettingsService>();
```

서비스가 등록되어 있는지 안전하게 확인할 때는 `TryGet`을 사용합니다.

```csharp
if (GameServiceRegistry.TryGet(out SettingsService settingsService))
{
}
```

### 초기화 순서 정렬

서비스 등록 순서와 관계없이 `InitializationOrder` 값을 기준으로 정렬한 뒤 초기화합니다.

같은 순서 값을 가진 서비스가 여러 개라면 서비스 이름을 기준으로 정렬합니다.

### 런타임 재시작 처리

Unity 런타임이 시작될 때 정적 Registry 상태를 초기화하도록 구성했습니다.

```text
RuntimeInitializeLoadType.SubsystemRegistration
```

Enter Play Mode 설정에서 Domain Reload가 비활성화된 경우에도 이전 Play Mode 실행에서 남은 정적 서비스 상태를 제거할 수 있습니다.

---

## 5. CommonServiceInitializer 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Services/CommonServiceInitializer.cs
```

Bootstrap에서 사용할 필수 공통 서비스의 생성과 초기화를 담당합니다.

등록되는 서비스:

```text
SettingsService
SaveService
AudioService
DataValidationService
```

초기화 흐름:

```text
기존 초기화 완료 여부 확인
→ 없는 서비스만 생성
→ 서비스 Registry 등록
→ 초기화 순서대로 실행
→ 성공 결과 반환
```

Registry가 이미 초기화된 경우에는 새로운 서비스를 만들지 않고 기존 인스턴스를 사용합니다.

초기화 과정에서 예외가 발생하면 오류 로그를 출력하고 `false`를 반환합니다.

```text
[Services] 공통 서비스 초기화에 실패하여 MainMenu 전환을 중단합니다.
```

---

# 공통 서비스 구성

## 6. SettingsService 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Services/SettingsService.cs
```

초기화 순서:

```text
100
```

4일차에서는 다음 기본값을 준비합니다.

```text
MasterVolume: 1
Language: Application.systemLanguage
```

실제 그래픽, 음량, 입력, 접근성, 지역과 카메라 설정 모델은 11일차 설정 저장 구조에서 확장할 예정입니다.

---

## 7. SaveService 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Services/SaveService.cs
```

초기화 순서:

```text
200
```

Unity의 플랫폼별 영구 저장 경로를 기준으로 `Saves` 폴더를 생성합니다.

```text
Application.persistentDataPath
└─ Saves
```

초기화 과정:

```text
persistentDataPath 조회
→ 경로 유효성 확인
→ Saves 경로 생성
→ Directory.CreateDirectory 실행
```

4일차에서는 저장 폴더만 준비하며 실제 저장 파일 작성과 불러오기는 이후 일정에서 구현합니다.

---

## 8. AudioService 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Audio/AudioService.cs
```

초기화 순서:

```text
300
```

초기 상태:

```text
MasterVolume: 1
IsMuted: false
```

제공 기능:

```text
SetMasterVolume
SetMuted
```

마스터 음량은 `Mathf.Clamp01`을 사용해 0부터 1 사이로 제한합니다.

실제 출력 음량은 다음 규칙으로 적용됩니다.

```text
IsMuted == true  → AudioListener.volume = 0
IsMuted == false → AudioListener.volume = MasterVolume
```

---

## 9. DataValidationService 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/DataValidationService.cs
```

초기화 순서:

```text
400
```

데이터 검증 서비스는 자신보다 먼저 초기화되어야 하는 다음 서비스를 확인합니다.

```text
SettingsService
SaveService
AudioService
```

각 서비스가 Registry에 등록되어 있고 `Initialized` 상태인지 검사합니다.

검증 성공 시:

```text
LastValidationSucceeded: true
```

필수 서비스가 초기화되지 않은 상태라면 예외를 발생시켜 Bootstrap 진행을 중단합니다.

12일차 Google Sheets 데이터 가져오기와 데이터 누락·중복 검사가 구현되면 이 서비스에 실제 데이터 검증 절차를 추가할 예정입니다.

---

# 서비스 초기화 순서

최종 초기화 순서는 다음과 같습니다.

| 순서 | 서비스 |
|---:|---|
| 100 | Settings |
| 200 | Save |
| 300 | Audio |
| 400 | DataValidation |

등록 순서가 달라도 실제 초기화는 항상 위 순서를 따릅니다.

예상 로그:

```text
[Services] 100: Settings 초기화 완료
[Services] 200: Save 초기화 완료
[Services] 300: Audio 초기화 완료
[Services] 400: DataValidation 초기화 완료
[Services] 공통 서비스 4개 초기화를 완료했습니다.
```

---

# Bootstrap 흐름 변경

## 10. BootstrapEntryPoint 수정

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/SceneFlow/BootstrapEntryPoint.cs
```

기존 흐름:

```text
Bootstrap 실행
→ SceneFlowManager 준비
→ MainMenu 이동
```

변경된 흐름:

```text
Bootstrap 실행
→ SceneFlowManager 준비
→ CommonServiceInitializer 준비
→ 공통 서비스 4개 초기화
→ 초기화 성공 확인
→ MainMenu 이동
```

`CommonServiceInitializer`가 같은 게임 오브젝트에 없다면 `Awake()`에서 자동으로 추가합니다.

공통 서비스 초기화가 실패한 경우에는 `MainMenu` 씬 전환을 실행하지 않습니다.

```text
초기화 실패
→ Bootstrap 씬 유지
→ 예외와 오류 로그 출력
→ MainMenu 이동 중단
```

13일차 치명 오류 UI 구현 시 이 실패 결과를 공통 오류 화면과 연결할 예정입니다.

---

## 11. Bootstrap 씬 변경

다음 오브젝트에 `CommonServiceInitializer`를 추가했습니다.

```text
ProjectJ_Bootstrap
```

최종 컴포넌트:

```text
SceneFlowManager
CommonServiceInitializer
BootstrapEntryPoint
```

`SceneFlowManager`가 해당 루트 게임 오브젝트에 `DontDestroyOnLoad`를 적용하므로 MainMenu로 이동한 뒤에도 공통 초기화 오브젝트가 유지됩니다.

---

# Editor 자동화

## 12. Day04ServiceSetupTool 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Editor/Day04ServiceSetupTool.cs
```

Unity 상단 메뉴:

```text
Project J
→ Day 04
→ Configure Common Services
```

도구 역할:

- Bootstrap 씬 존재 여부 확인
- Bootstrap 씬 열기
- `ProjectJ_Bootstrap` 오브젝트 확인 및 생성
- `SceneFlowManager` 존재 상태 보장
- `CommonServiceInitializer` 존재 상태 보장
- `BootstrapEntryPoint` 존재 상태 보장
- Bootstrap 씬 저장
- Play Mode 시작 씬을 Bootstrap으로 유지

Play Mode 진입 중이거나 실행 중일 때는 메뉴가 비활성화됩니다.

---

# 자동 테스트

## 13. GameServiceRegistryTests 생성

파일 위치:

```text
Assets/_ProjectJ/Tests/EditMode/GameServiceRegistryTests.cs
```

다음 네 가지 테스트를 추가했습니다.

### SameServiceTypeCanBeRegisteredOnlyOnce

같은 서비스 형식이 Registry에 중복 등록되지 않는지 검사합니다.

검증 항목:

```text
첫 번째 등록 성공
두 번째 등록 실패
최초 인스턴스 유지
등록 수 1개
```

### ServicesInitializeInConfiguredOrder

서비스를 초기화 순서와 반대로 등록해도 `InitializationOrder`에 따라 초기화되는지 검사합니다.

```text
등록 순서: B → A
초기화 순서: A → B
```

### InitializeAllDoesNotInitializeServiceTwice

`InitializeAll()`을 두 번 호출해도 서비스의 실제 초기화 횟수가 한 번인지 검사합니다.

```text
InitializeCount: 1
```

### CommonServiceInitializerCreatesFourServicesOnlyOnce

`CommonServiceInitializer.InitializeServices()`를 두 번 실행해도 다음 네 서비스만 존재하는지 검사합니다.

```text
SettingsService
SaveService
AudioService
DataValidationService
```

검증 결과:

```text
RegisteredServiceCount: 4
SettingsService State: Initialized
Registry State: Initialized
```

---

# 전체 테스트 구성

기존 테스트:

```text
ProjectStructureTests: 2개
GameSceneCatalogTests: 3개
```

4일차 신규 테스트:

```text
GameServiceRegistryTests: 4개
```

예상 전체 결과:

```text
Passed: 9
Failed: 0
Ignored: 0
```

---

# 생성·수정된 주요 파일

## 수정된 파일

```text
Assets/_ProjectJ/Scenes/Game/Bootstrap.unity
Assets/_ProjectJ/Scripts/Runtime/Core/SceneFlow/BootstrapEntryPoint.cs
```

## 새로 생성된 파일

```text
Assets/_ProjectJ/Scripts/Editor/Day04ServiceSetupTool.cs

Assets/_ProjectJ/Scripts/Runtime/Audio/AudioService.cs

Assets/_ProjectJ/Scripts/Runtime/Core/Services/GameServiceState.cs
Assets/_ProjectJ/Scripts/Runtime/Core/Services/IGameService.cs
Assets/_ProjectJ/Scripts/Runtime/Core/Services/GameServiceBase.cs
Assets/_ProjectJ/Scripts/Runtime/Core/Services/GameServiceRegistry.cs
Assets/_ProjectJ/Scripts/Runtime/Core/Services/CommonServiceInitializer.cs
Assets/_ProjectJ/Scripts/Runtime/Core/Services/SettingsService.cs
Assets/_ProjectJ/Scripts/Runtime/Core/Services/SaveService.cs

Assets/_ProjectJ/Scripts/Runtime/Data/DataValidationService.cs

Assets/_ProjectJ/Tests/EditMode/GameServiceRegistryTests.cs
```

각 폴더와 스크립트의 `.meta` 파일도 함께 Git에 등록했습니다.

---

# 주요 프로젝트 구조

```text
Assets/_ProjectJ
├─ Scenes
│  └─ Game
│     └─ Bootstrap.unity
├─ Scripts
│  ├─ Runtime
│  │  ├─ Audio
│  │  │  └─ AudioService.cs
│  │  ├─ Core
│  │  │  ├─ SceneFlow
│  │  │  │  └─ BootstrapEntryPoint.cs
│  │  │  └─ Services
│  │  │     ├─ GameServiceState.cs
│  │  │     ├─ IGameService.cs
│  │  │     ├─ GameServiceBase.cs
│  │  │     ├─ GameServiceRegistry.cs
│  │  │     ├─ CommonServiceInitializer.cs
│  │  │     ├─ SettingsService.cs
│  │  │     └─ SaveService.cs
│  │  └─ Data
│  │     └─ DataValidationService.cs
│  └─ Editor
│     └─ Day04ServiceSetupTool.cs
└─ Tests
   └─ EditMode
      └─ GameServiceRegistryTests.cs
```

---

# 검증 결과

| 검증 항목 | 저장소 확인 |
|---|:---:|
| 최신 커밋 제목 정상 | 완료 |
| Bootstrap 서비스 초기화 컴포넌트 추가 | 완료 |
| 서비스 상태 enum 생성 | 완료 |
| 서비스 공통 인터페이스 생성 | 완료 |
| 중복 초기화 방지 기본 클래스 생성 | 완료 |
| 서비스 Registry 생성 | 완료 |
| 같은 형식 중복 등록 방지 | 완료 |
| 초기화 순서 정렬 | 완료 |
| SettingsService 생성 | 완료 |
| SaveService 생성 | 완료 |
| AudioService 생성 | 완료 |
| DataValidationService 생성 | 완료 |
| Bootstrap 초기화 성공 후 MainMenu 전환 | 완료 |
| 초기화 실패 시 씬 전환 중단 | 완료 |
| Editor 자동 구성 도구 생성 | 완료 |
| EditMode 테스트 4개 작성 | 완료 |
| GitHub Actions 자동 검사 | 미구성 |

로컬 Unity 에디터 최종 확인 항목:

```text
Console Error: 0개
EditMode Passed: 9개
EditMode Failed: 0개
공통 서비스 등록 수: 4개
Bootstrap → MainMenu 전환 정상
```

---

# 다음 개발 방향

## 5일차 : Input System 액션 맵

다음 일차에는 키보드·마우스와 게임패드 입력을 프로젝트 전용 Input Actions로 정리합니다.

예정 작업:

- Gameplay 액션 맵 생성
- UI 액션 맵 생성
- WASD 이동 입력
- 마우스와 오른쪽 스틱 시점 입력
- Space 점프
- Shift 달리기
- Ctrl 앉기
- 좌클릭 밀치기
- 우클릭 아이템 사용
- Q·E 아이템 슬롯 선택
- R 아이템 보여주기
- G 아이템 버리기
- F 상호작용
- Tab 순위표
- ESC 일시정지
- 키보드·마우스와 게임패드 Control Scheme 분리
- 입력 액션 검증 테스트

---

# 커밋 정보

```text
4일차 : 공통 서비스 초기화
```

```text
https://github.com/siwoo440/Project-J/commit/f6d98e979a59bf456824b5bd96022130de234374
```
