# Project J 개발 일지

## 50일차 : SettingsManager 및 설정 데이터 기반 통합

### 개발 목표

49일차까지 프로젝트 구조 정리와 전체 검증을 완료한 뒤, 50일차부터 다시 기능 개발 단계로 전환했습니다.

이번 일차에서는 기존 16일차에서 구현했던 사용자 설정 저장·복원 기반을 다시 만드는 대신, 현재 프로젝트 구조에 맞게 설정 시스템을 정리하고 이후 설정 UI에서 안전하게 사용할 수 있는 통합 진입점을 마련하는 것을 목표로 진행했습니다.

핵심 목표는 다음과 같습니다.

- 기존 `SettingsService` 기능 유지
- 사용자 설정 데이터 복사와 비교 기능 추가
- JSON 직렬화와 역직렬화 기능 분리
- `SettingsManager` 통합 접근 계층 추가
- 설정 작업 복사본 기반 마련
- 설정 적용·저장·불러오기·초기화 기능 연결
- 51일차 설정 UI 구현을 위한 공개 API 준비
- 설정 데이터 회귀 테스트 추가

---

### 기준 커밋

50일차 작업은 다음 49일차 완료 커밋을 기준으로 진행했습니다.

```text
c8406099ea8a17a0c169ca65291b8192fefdbf2f
```

기준 커밋 제목:

```text
49일차 : 컴파일·참조·메뉴·테스트·빌드 최종 검증
```

50일차 완료 커밋:

```text
15ee71c29935e8cb4906c6ca981202e0c96f34e6
```

커밋 제목:

```text
50일차 : SettingsManager 및 설정 데이터 기반 통합
```

---

### 기존 설정 시스템 확인

프로젝트에는 이미 16일차에서 다음 기반이 구현되어 있었습니다.

```text
SaveService
SettingsService
ProjectUserSettings
AudioService
PlayerInputReader
ThirdPersonCameraController
```

기존 설정 시스템에서는 다음 항목을 저장할 수 있었습니다.

| 분류 | 항목 |
|---|---|
| 그래픽 | 품질 단계, 해상도, 화면 모드, VSync, 목표 FPS |
| 오디오 | Master, BGM, SFX, 전체 음소거 |
| 조작 | 마우스 감도, 게임패드 시점 속도, Y축 반전 |
| 입력 | Input System 바인딩 재지정 JSON |
| 로그 | 최소 로그 등급 |
| 호환성 | 설정 파일 버전 |

설정 파일은 기존과 동일하게 다음 위치를 사용합니다.

```text
Application.persistentDataPath/
└─ Settings/
   └─ user-settings.json
```

따라서 50일차에서는 저장 형식을 새로 만들지 않고 기존 데이터를 유지하면서 관리 구조를 정리했습니다.

---

### 최종 설정 관리 구조

50일차 이후 설정 데이터 흐름은 다음과 같이 정리했습니다.

```text
설정 UI
   ↓
SettingsManager
   ↓
SettingsService
   ↓
SettingsJsonSerializer
   ↓
SaveService
   ↓
user-settings.json
```

각 클래스의 역할을 분리하여 설정 UI가 파일 저장이나 서비스 레지스트리를 직접 처리하지 않도록 구성했습니다.

---

### ProjectUserSettings 확장

기존 `ProjectUserSettings`에 설정 UI와 데이터 관리를 위한 기능을 추가했습니다.

추가한 주요 메서드는 다음과 같습니다.

```text
Clone()
CopyFrom()
ContentEquals()
```

---

### Clone 구현

`Clone()`은 현재 설정의 독립적인 복사본을 생성합니다.

예:

```text
현재 저장 설정
MasterVolume = 1.0

        ↓ Clone

UI 작업 복사본
MasterVolume = 1.0
```

UI에서 작업 복사본을 다음과 같이 변경해도:

```text
MasterVolume = 0.5
```

실제 저장 설정은 즉시 변경되지 않습니다.

이 구조는 이후 설정 화면에서 다음 흐름을 구현하기 위한 기반입니다.

```text
설정 화면 열기
↓
현재 설정 복사
↓
UI에서 값 수정
↓
적용
또는
취소
```

---

### CopyFrom 구현

`CopyFrom()`은 다른 `ProjectUserSettings`의 전체 값을 현재 객체로 복사합니다.

복사 대상에는 다음 값들이 포함됩니다.

```text
Version

GraphicsQualityName
ResolutionWidth
ResolutionHeight
FullScreenModeValue
VSyncCount
TargetFrameRate

MasterVolume
MusicVolume
SfxVolume
IsMuted

MouseSensitivity
GamepadLookDegreesPerSecond
InvertLookY

InputBindingOverridesJson
MinimumLogLevelValue
```

이를 통해 이후 설정 적용·취소·초기화 과정에서 전체 설정을 일괄 처리할 수 있게 했습니다.

---

### ContentEquals 구현

`ContentEquals()`는 두 설정 객체의 실제 저장값이 모두 같은지 비교합니다.

이를 통해 이후 설정 UI에서:

```text
현재 설정 == 작업 설정
```

이면 변경 사항 없음,

```text
현재 설정 != 작업 설정
```

이면 변경 사항 있음으로 판단할 수 있습니다.

특히 적용 버튼 활성화나 저장되지 않은 설정 변경 확인 기능에 활용할 수 있습니다.

---

### 설정 값 검증 유지

기존 `Validate()`의 설정 범위 검증 기능은 유지했습니다.

주요 제한값은 다음과 같습니다.

| 설정 | 유효 범위 |
|---|---|
| 최소 가로 해상도 | 640 이상 |
| 최소 세로 해상도 | 360 이상 |
| VSync | 0~4 |
| 목표 FPS | -1~360 |
| Master | 0~1 |
| Music | 0~1 |
| SFX | 0~1 |
| 마우스 감도 | 0.01~2 |
| 게임패드 시점 속도 | 30~720 |
| 로그 등급 | 0~4 |

잘못된 전체 화면 모드가 저장되어 있으면 `FullScreenWindow`로 복구합니다.

---

### SettingsJsonSerializer 추가

새로운 파일을 추가했습니다.

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Services/
SettingsJsonSerializer.cs
```

기존에는 `SettingsService`가 JSON 변환까지 직접 담당하고 있었습니다.

50일차부터는 역할을 다음처럼 분리했습니다.

```text
SettingsService
→ 설정 생명주기와 런타임 적용

SettingsJsonSerializer
→ JSON 변환과 데이터 검증

SaveService
→ 파일 읽기와 쓰기
```

---

### Serialize 구현

다음 흐름으로 설정을 JSON으로 변환합니다.

```text
ProjectUserSettings
↓
Clone
↓
Validate
↓
JsonUtility.ToJson
↓
JSON 문자열
```

직렬화 과정에서 원본 설정 객체를 직접 수정하지 않고 복사본을 검증한 뒤 JSON을 생성하도록 했습니다.

---

### TryDeserialize 구현

JSON을 읽을 때 다음 항목을 검사합니다.

```text
JSON 문자열 존재 여부

JSON 변환 성공 여부

변환 결과 null 여부

설정 Version 일치 여부

설정 값 범위
```

정상적인 경우:

```text
JSON
↓
ProjectUserSettings
↓
Validate
↓
사용 가능 설정 반환
```

잘못된 경우:

```text
손상된 JSON
지원하지 않는 Version
빈 JSON
```

을 안전하게 거부하고 실패 원인을 반환하도록 구성했습니다.

---

### SettingsService 정리

기존 `SettingsService`를 설정 시스템의 실제 처리 서비스로 유지했습니다.

기존 공개 기능도 그대로 유지했습니다.

```text
SetGraphics()
SetAudio()
SetControls()
SetInputBindingOverrides()
SetMinimumLogLevel()
```

따라서 기존 `AudioService`, `ThirdPersonCameraController`, `PlayerInputReader`가 사용하는 설정 연결 구조를 변경하지 않았습니다.

---

### CreateSnapshot 추가

현재 설정을 직접 전달하지 않고 독립된 복사본으로 반환합니다.

```text
SettingsService.Current
↓
Clone
↓
작업 복사본
```

설정 UI가 실제 저장 데이터 객체를 직접 수정하는 문제를 방지합니다.

---

### ApplySettings 추가

작업 복사본을 실제 설정으로 확정하는 통합 기능을 추가했습니다.

처리 흐름:

```text
작업 복사본
↓
Clone
↓
Validate
↓
그래픽·로그 설정 적용
↓
JSON 저장
↓
SettingsChanged 이벤트
```

이 기능은 51일차 설정 UI의 `적용` 버튼에서 사용할 예정입니다.

---

### ReloadFromDisk 추가

현재 메모리 설정을 다시 저장 파일 기준으로 복원할 수 있도록 했습니다.

```text
user-settings.json
↓
JSON 읽기
↓
데이터 검증
↓
현재 설정 교체
↓
런타임 적용
↓
SettingsChanged
```

손상된 파일이나 지원하지 않는 Version은 기존 정책대로 기본값으로 복구합니다.

---

### ResetToDefaults 정리

설정 전체 초기화 기능을 반환값이 있는 구조로 정리했습니다.

처리 순서:

```text
현재 실행 환경 기준 기본값 생성
↓
Validate
↓
런타임 설정 적용
↓
JSON 저장
↓
SettingsChanged
```

이 기능은 이후 설정 UI의 `기본값` 기능과 연결할 수 있습니다.

---

### SaveCurrent 정리

JSON 생성 코드를 `SettingsJsonSerializer`로 이동하여 `SettingsService`는 다음 흐름만 관리하도록 역할을 단순화했습니다.

```text
Current.Validate()
↓
SettingsJsonSerializer.Serialize()
↓
SaveService.SaveSettingsText()
```

---

### SettingsManager 추가

50일차의 핵심 신규 파일입니다.

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Services/
SettingsManager.cs
```

`SettingsManager`는 새로운 MonoBehaviour나 새로운 Singleton GameObject가 아닙니다.

기존에 존재하는:

```text
GameServiceRegistry
SettingsService
```

를 설정 UI나 다른 시스템에서 간단하고 안전하게 사용할 수 있도록 만드는 정적 접근 계층입니다.

---

### SettingsManager를 별도 GameObject로 만들지 않은 이유

현재 Bootstrap에는 이미 공통 서비스 초기화 구조가 존재합니다.

```text
Bootstrap
↓
CommonServiceInitializer
↓
GameServiceRegistry
↓
SettingsService
```

여기에 별도의:

```text
SettingsManager GameObject
DontDestroyOnLoad
Singleton Instance
```

를 추가하면 동일한 설정 데이터를 관리하는 경로가 두 개가 될 수 있습니다.

따라서 50일차에서는 기존 서비스 구조를 그대로 유지하고 `SettingsManager`는 단순한 접근 파사드 역할만 담당하도록 구성했습니다.

---

### SettingsManager 공개 기능

다음 API를 추가했습니다.

```text
IsReady
Current

CreateWorkingCopy()
CreateDefaultWorkingCopy()
TryCreateWorkingCopy()

Apply()
Save()
Reload()
ResetToDefaults()
```

---

### IsReady

현재 `SettingsService`가 등록되어 있고 초기화까지 완료되었는지 확인합니다.

```text
SettingsManager.IsReady
```

Bootstrap 초기화 완료 여부를 설정 UI에서 쉽게 확인할 수 있습니다.

---

### Current

현재 설정 원본을 그대로 노출하지 않고 안전한 복사본을 반환합니다.

```text
SettingsManager.Current
↓
ProjectUserSettings Clone
```

외부 코드가 실수로 `SettingsService.Current`를 직접 수정하는 문제를 줄였습니다.

---

### CreateWorkingCopy

설정 UI용 작업 데이터를 생성합니다.

```text
현재 설정
↓
CreateWorkingCopy()
↓
UI 편집용 설정
```

51일차 설정 화면을 열 때 사용할 핵심 기능입니다.

---

### CreateDefaultWorkingCopy

현재 저장 설정을 즉시 초기화하지 않고 기본값 상태만 미리 생성합니다.

따라서 UI에서 기본값 버튼을 누른 뒤 바로 저장하지 않고 화면상에서 먼저 확인하는 방식도 구현할 수 있습니다.

---

### Apply

작업 복사본을 실제 설정에 반영하고 저장합니다.

```text
SettingsManager.Apply(workingCopy)
```

내부에서는 `SettingsService.ApplySettings()`를 호출합니다.

---

### Save

현재 설정을 명시적으로 다시 저장합니다.

```text
SettingsManager.Save()
```

---

### Reload

저장된 `user-settings.json`을 다시 읽고 런타임에 재적용합니다.

```text
SettingsManager.Reload()
```

---

### ResetToDefaults

현재 설정을 기본값으로 복원하고 저장합니다.

```text
SettingsManager.ResetToDefaults()
```

---

### SettingsManagerTests 추가

다음 EditMode 테스트 파일을 추가했습니다.

```text
Assets/_ProjectJ/Tests/EditMode/ProjectSettings/
SettingsManagerTests.cs
```

설정 시스템의 핵심 데이터 처리 기능을 실제 설정 파일을 변경하지 않고 메모리에서 검사합니다.

---

### 테스트 항목

#### DefaultSettingsUseCurrentVersionAndValidRanges

기본 설정이 현재 Version을 사용하고 유효한 범위로 생성되는지 검사합니다.

#### CloneCreatesIndependentWorkingCopy

작업 복사본을 수정해도 원본 설정이 변경되지 않는지 검사합니다.

#### ValidateClampsUnsafeValues

비정상적인 설정값을 `Validate()`가 안전 범위로 복구하는지 검사합니다.

#### JsonSerializerRoundTripsAllSettings

설정을 JSON으로 저장하고 다시 읽었을 때 모든 값이 동일하게 복원되는지 검사합니다.

```text
ProjectUserSettings
↓
JSON
↓
ProjectUserSettings
```

왕복 결과를 검증합니다.

#### JsonSerializerRejectsUnsupportedVersion

지원하지 않는 설정 Version을 정상적으로 거부하는지 확인합니다.

#### SettingsManagerIsNotReadyWithoutRegisteredService

Bootstrap 서비스 초기화 전에는 `SettingsManager`가 준비되지 않은 상태를 안전하게 반환하는지 검사합니다.

---

### Day50SettingsValidationTool 추가

설정 기반을 Unity Editor에서도 빠르게 검사할 수 있도록 다음 도구를 추가했습니다.

```text
Assets/_ProjectJ/Scripts/Editor/ProjectSettings/Services/
Day50SettingsValidationTool.cs
```

Unity 메뉴:

```text
Project J
→ 01. 프로젝트 설정
→ 서비스
→ 50일차 설정 기반 검증
```

---

### Edit Mode 검증

Edit Mode에서 메뉴를 실행하면 다음 항목을 검사합니다.

```text
기본 설정 생성

작업 복사본 생성

원본과 복사본 동일성

JSON Serialize

JSON Deserialize

저장 전후 설정 동일성
```

실제 `user-settings.json`은 변경하지 않습니다.

---

### Play Mode 검증

Bootstrap을 통해 공통 서비스가 초기화된 Play Mode에서는 추가로:

```text
SettingsManager.IsReady
SettingsManager.CreateWorkingCopy()
```

가 정상 작동하는지 검사합니다.

이를 통해 실제 게임 실행 환경에서도 SettingsManager가 기존 공통 서비스 구조와 정상적으로 연결되는지 확인할 수 있습니다.

---

### 수정한 파일

| 파일 | 변경 내용 |
|---|---|
| `ProjectUserSettings.cs` | Clone, CopyFrom, ContentEquals 추가와 기본 설정 검증 정리 |
| `SettingsService.cs` | Snapshot, Apply, Reload, Reset, Serializer 연결 및 저장 흐름 정리 |

---

### 생성한 파일

| 파일 | 역할 |
|---|---|
| `SettingsManager.cs` | 설정 시스템 통합 접근 계층 |
| `SettingsJsonSerializer.cs` | 사용자 설정 JSON 직렬화·역직렬화 |
| `SettingsManagerTests.cs` | 설정 데이터와 관리자 기반 EditMode 회귀 테스트 |
| `Day50SettingsValidationTool.cs` | Edit Mode·Play Mode 설정 기반 검증 도구 |

신규 `.cs` 파일의 Unity `.meta` 파일도 함께 생성하여 저장소에 반영했습니다.

---

### 기존 시스템 호환 유지

50일차에서는 다음 기존 시스템을 수정하지 않았습니다.

```text
AudioService
ThirdPersonCameraController
PlayerInputReader
SaveService
GameServiceRegistry
CommonServiceInitializer
BootstrapEntryPoint
```

기존 `SettingsService.SettingsChanged` 이벤트와 기능별 변경 API를 유지했기 때문에 기존 오디오·카메라·입력 시스템의 설정 연결도 그대로 사용할 수 있습니다.

---

### Scene·Prefab 변경 없음

50일차는 설정 데이터와 코드 기반 정리 작업이므로 Scene과 Prefab은 수정하지 않았습니다.

커밋 변경 범위는 설정 관련 스크립트와 테스트, Editor 검증 도구로 제한했습니다.

```text
Scene 변경 없음
Prefab 변경 없음
Packages 변경 없음
ProjectSettings 변경 없음
기타 Runtime 기능 변경 없음
```

---

### 설정 UI를 위한 작업 흐름 완성

51일차 이후 설정 화면은 다음 방식으로 구현할 수 있습니다.

```text
설정 메뉴 열기
↓
SettingsManager.CreateWorkingCopy()
↓
UI 각 탭에 값 표시
↓
사용자가 값 수정
```

적용:

```text
작업 설정
↓
SettingsManager.Apply()
↓
런타임 적용
↓
JSON 저장
```

취소:

```text
작업 설정 폐기
↓
원본 설정 유지
```

기본값 미리보기:

```text
SettingsManager.CreateDefaultWorkingCopy()
↓
UI에 기본값 표시
↓
적용 전까지 실제 설정 유지
```

즉 51일차의 `적용·취소·기본값` 버튼을 구현할 수 있는 데이터 기반이 준비되었습니다.

---

### 50일차 완료 결과

- 기존 SettingsService 구조 유지
- 사용자 설정 작업 복사본 기능 추가
- 설정 전체 값 복사 기능 추가
- 설정 변경 여부 비교 기능 추가
- JSON 직렬화 로직 서비스에서 분리
- 손상 JSON과 지원하지 않는 Version 검증 유지
- SettingsManager 통합 접근 계층 추가
- 설정 적용 기능 통합
- 설정 명시적 저장 기능 연결
- 설정 다시 불러오기 기능 연결
- 설정 기본값 초기화 기능 연결
- 설정 UI용 작업 데이터 구조 완성
- EditMode 설정 회귀 테스트 추가
- Unity Editor 설정 기반 검증 도구 추가
- 기존 Audio·Camera·Input 설정 연결 유지
- Scene·Prefab 비의도 변경 없음

---

### 다음 개발 방향

51일차에서는 50일차에서 완성한 `SettingsManager`와 작업 복사본 구조를 실제 설정 메뉴 UI에 연결합니다.

주요 구현 대상은 다음과 같습니다.

```text
설정 메뉴 화면

화면 탭
사운드 탭
조작 탭
카메라 탭

적용 버튼
취소 버튼
기본값 버튼
```

각 UI 컨트롤은 실제 `SettingsService.Current`를 직접 수정하지 않고 `SettingsManager.CreateWorkingCopy()`로 생성한 작업 복사본을 수정하도록 구성합니다.

사용자가 적용을 누를 때만 `SettingsManager.Apply()`를 호출하여 설정을 실제 게임에 반영하고 저장하는 구조로 구현합니다.

---

## 50일차 커밋

```text
50일차 : SettingsManager 및 설정 데이터 기반 통합
```

커밋 SHA:

```text
15ee71c29935e8cb4906c6ca981202e0c96f34e6
```
