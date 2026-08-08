# Project J 개발 일지

## 52일차 : 화면·사운드·조작·카메라 설정 통합 및 저장·복원 완성

### 개발 목표

기존에 분리되어 있던 52일차와 53일차 설정 작업을 하나로 통합하여 설정 시스템을 실사용 가능한 단계까지 완성한다.

51일차에서 구현한 설정 UI를 기반으로 다음 기능을 연결한다.

- 화면 설정 실제 적용
- 밝기 설정
- Master·BGM·SFX·UI 음량 구조
- 전체 음소거
- Keyboard&Mouse 키 재지정
- 중복 키 방지
- 재지정 취소
- 기본 키 복원
- 마우스 감도
- 게임패드 시점 속도
- Y축 반전
- 설정 파일 v1 → v2 마이그레이션
- 저장·재실행 복원

---

## 기준 커밋

### 이전 기준

```text
8881ee320044dba3e02d3cb8ef164d984387e6c6
51일차 : 설정 메뉴 UI 및 4개 탭 기본 기능 구현
```

### 52일차 완료 커밋

```text
f2f4129e272f0b371a4cfd354cce821153a3fada
52일차 : 화면·사운드·조작·카메라 설정 통합 및 저장·복원 완성
```

---

## 1. 사용자 설정 Version 2 적용

`ProjectUserSettings`를 Version 2로 확장하였다.

새롭게 추가한 설정값은 다음과 같다.

```text
Brightness
UiVolume
```

### Brightness

화면 밝기 설정값이다.

```text
0.5 ~ 1.5
```

범위로 관리한다.

UI에서는 다음과 같이 표시한다.

```text
50% ~ 150%
```

### UI Volume

메뉴와 HUD 등 UI 효과음 전용 음량 설정값이다.

```text
0.0 ~ 1.0
```

범위로 관리한다.

---

## 2. 기존 Version 1 설정 마이그레이션

51일차까지 생성된 사용자 설정 파일은 Version 1 구조를 사용하였다.

52일차에서는 기존 사용자 설정을 삭제하지 않고 Version 2로 자동 변환하도록 `SettingsJsonSerializer`를 확장하였다.

Version 1 설정을 불러오면 기존 값은 유지하고 새 필드만 기본값으로 채운다.

```text
기존 Resolution
기존 Volume
기존 Camera 설정
기존 Input Binding Override
        ↓
그대로 유지

Brightness = 1.0
UiVolume   = 1.0
        ↓
Version 2 저장
```

이를 통해 기존 설정 파일을 계속 사용할 수 있도록 하였다.

---

## 3. 화면 설정 실제 적용 유지 및 확장

기존 `SettingsService`에서 사용하던 그래픽 적용 구조를 유지하였다.

다음 설정은 기존과 동일하게 Unity 런타임에 적용된다.

```text
해상도
→ Screen.SetResolution()

화면 모드
→ FullScreenMode

VSync
→ QualitySettings.vSyncCount

최대 FPS
→ Application.targetFrameRate
```

52일차에서는 여기에 밝기 적용을 추가하였다.

---

## 4. URP 밝기 적용 기능 추가

다음 파일을 새로 추가하였다.

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Services/DisplayBrightnessApplier.cs
```

URP의 전역 `Volume`과 `Color Adjustments`의 `Post Exposure`를 이용해 실제 3D 화면 밝기를 조절한다.

런타임에는 다음 구조의 전역 Volume이 생성된다.

```text
ProjectJ_DisplayBrightness
└─ Volume
   └─ Runtime VolumeProfile
      └─ Color Adjustments
         └─ Post Exposure
```

밝기 변환 기준은 다음과 같다.

```text
50%  → Post Exposure -1
100% → Post Exposure  0
150% → Post Exposure +1
```

Scene이 변경된 뒤에도 새로운 Camera가 사용자 밝기 설정을 사용할 수 있도록 후처리 설정을 다시 확인하도록 구성하였다.

---

## 5. Runtime Assembly Definition 확장

밝기 적용 기능에서 URP 타입을 직접 사용하므로 다음 Runtime asmdef 참조를 추가하였다.

```text
Unity.RenderPipelines.Core.Runtime
Unity.RenderPipelines.Universal.Runtime
```

기존 참조는 그대로 유지하였다.

```text
Unity.InputSystem
Unity.TextMeshPro
UnityEngine.UI
```

---

## 6. 오디오 설정 확장

`AudioService`를 다음 구조로 확장하였다.

```text
Master
BGM
SFX
UI
Mute
```

### Master

전체 게임 출력 크기를 관리한다.

### BGM

배경 음악 전용 사용자 음량을 관리한다.

### SFX

게임 효과음 전용 사용자 음량을 관리한다.

### UI

메뉴와 HUD 효과음 전용 사용자 음량을 관리한다.

### Mute

전체 게임 음소거 상태를 관리한다.

Master와 Mute는 기존 방식대로 `AudioListener.volume`을 통해 전체 출력에 반영한다.

---

## 7. 오디오 채널 구분 추가

다음 열거형을 추가하였다.

```text
Assets/_ProjectJ/Scripts/Runtime/Audio/ProjectAudioChannel.cs
```

채널 종류는 다음과 같다.

```text
Music
Sfx
UI
```

AudioSource가 어떤 사용자 음량 설정을 사용할지 구분하기 위한 값이다.

---

## 8. AudioChannelVolumeController 추가

다음 파일을 새로 추가하였다.

```text
Assets/_ProjectJ/Scripts/Runtime/Audio/AudioChannelVolumeController.cs
```

개별 `AudioSource`에 추가하여 다음 방식으로 사용한다.

```text
BGM AudioSource
└─ AudioChannelVolumeController
   └─ Channel = Music

게임 효과음 AudioSource
└─ AudioChannelVolumeController
   └─ Channel = Sfx

UI 효과음 AudioSource
└─ AudioChannelVolumeController
   └─ Channel = UI
```

기존 `AudioSource.volume`을 원본 볼륨으로 보존하고 사용자 설정 채널 음량을 곱하여 최종 출력 크기를 결정한다.

```text
최종 AudioSource Volume
=
Base Volume
×
사용자 Channel Volume
```

현재 정식 BGM·SFX·UI AudioSource가 존재하는 경우 이 컴포넌트를 연결하여 개별 음량 설정을 사용할 수 있는 기반을 마련하였다.

---

## 9. 조작 탭 실제 키 재지정 구현

51일차에서는 상태 안내만 표시하던 조작 탭을 실제 Keyboard&Mouse 재지정 화면으로 확장하였다.

기본 재지정 대상은 다음 15개이다.

```text
앞으로 이동
뒤로 이동
왼쪽 이동
오른쪽 이동
점프
달리기
앉기
밀치기
아이템 사용
이전 아이템
다음 아이템
아이템 보여주기
아이템 버리기
상호작용
순위표
```

`Pause`의 Escape 키는 재지정 취소키와 충돌하지 않도록 기본 재지정 목록에서 제외하였다.

---

## 10. Interactive Rebinding 구현

Input System의 Interactive Rebinding을 이용해 사용자가 직접 새 키를 입력하도록 구성하였다.

기본 흐름은 다음과 같다.

```text
변경 버튼
↓
입력 대기...
↓
새 Keyboard 키 또는 Mouse Button 입력
↓
Binding Override 작업 복사본에 반영
```

재지정 중에는 다른 설정 UI를 잠시 비활성화하여 잘못된 UI 입력이 새 키로 인식되는 상황을 줄였다.

---

## 11. 재지정 취소 기능

키 입력 대기 중 `Esc`를 누르면 현재 재지정을 취소하도록 구성하였다.

```text
변경 버튼
↓
입력 대기
↓
Esc
↓
재지정 취소
↓
기존 키 유지
```

---

## 12. 중복 키 검사

다음 파일을 추가하였다.

```text
Assets/_ProjectJ/Scripts/Runtime/Input/InputBindingConflictRules.cs
```

같은 Gameplay Action Map 안에서 이미 다른 조작이 사용하고 있는 Keyboard&Mouse 경로를 검사한다.

예:

```text
Jump = Space
Crouch = Left Ctrl
```

상태에서 Crouch를 Space로 변경하려는 경우:

```text
Jump   = Space
Crouch = Space
```

가 되지 않도록 변경을 거부한다.

현재 정책은 자동 교환이 아니라 중복 입력 거부 방식이다.

---

## 13. 기본 키 미리보기

조작 탭에 `기본 키` 버튼을 추가하였다.

버튼을 눌러도 즉시 실제 저장 파일을 변경하지 않는다.

```text
사용자 키 재지정
↓
기본 키 버튼
↓
작업 복사본의 Override 제거
↓
UI에서 기본 키 표시
```

실제로 저장하려면 하단의 `적용` 버튼을 눌러야 한다.

---

## 14. Binding Override JSON 저장

작업 복사본에서 변경한 키는 다음 흐름으로 저장한다.

```text
InputAction Binding Override
↓
SaveBindingOverridesAsJson()
↓
ProjectUserSettings.InputBindingOverridesJson
↓
SettingsManager.Apply()
↓
user-settings.json
```

이를 통해 게임 종료 후 다시 실행해도 사용자 키 설정을 복원할 수 있는 구조를 유지하였다.

---

## 15. PlayerInputReader 실행 중 재적용

기존 `PlayerInputReader`는 시작 시 저장된 Binding Override JSON을 읽어 적용하는 기반을 가지고 있었다.

52일차에서는 `SettingsService.SettingsChanged`를 구독하도록 확장하였다.

따라서 설정 화면에서 키를 변경하고 `적용`하면 현재 실행 중인 플레이어 입력 복제본에도 새 Binding Override를 다시 적용할 수 있다.

흐름은 다음과 같다.

```text
설정 Apply
↓
SettingsChanged
↓
PlayerInputReader
↓
새 Binding Override JSON 확인
↓
기존 Override 제거
↓
새 Override 적용
```

---

## 16. 카메라 설정 통합

다음 설정은 기존 `ThirdPersonCameraController` 구조를 그대로 사용한다.

```text
MouseSensitivity
GamepadLookDegreesPerSecond
InvertLookY
```

카메라 컨트롤러가 이미 `SettingsChanged`를 구독하고 있으므로 52일차에서는 중복 카메라 시스템을 만들지 않았다.

설정 메뉴의 카메라 탭에서 작업 복사본을 수정한 뒤 적용하면 기존 카메라 시스템에서 값을 사용하는 구조를 유지한다.

---

## 17. 52일차 Editor 자동 구성 도구

다음 파일을 새로 추가하였다.

```text
Assets/_ProjectJ/Scripts/Editor/UI/Day52SettingsCompletionSetupTool.cs
```

52일차 자동 구성 도구는 기존 51일차 설정 화면을 기반으로 다음 UI를 추가한다.

```text
화면 탭
└─ 밝기 Slider

사운드 탭
└─ UI Volume Slider

조작 탭
├─ 기본 키 Button
└─ Keyboard&Mouse Rebind Scroll View
   └─ 15개 조작 행
```

각 조작 행은 다음 구조를 가진다.

```text
조작 이름 | 현재 키 | 변경
```

자동 구성 후 `SettingsMenuController`의 추가 직렬화 참조도 연결한다.

---

## 18. 설정 메뉴 Controller 확장

`SettingsMenuController`를 크게 확장하여 다음 기능을 하나의 설정 화면 흐름으로 통합하였다.

- 밝기 작업 복사본
- UI Volume 작업 복사본
- 재지정용 InputActionAsset 복제본
- 현재 Binding 표시
- 재지정 입력 대기
- Esc 취소
- 중복 키 거부
- 기본 키 미리보기
- 전체 기본값
- 취소
- 적용
- 적용 후 UI 재동기화

실제 플레이어 입력을 설정 편집 중 바로 변경하지 않고, 설정 메뉴 전용 `InputActionAsset` 복제본에서 먼저 변경하는 구조를 사용하였다.

---

## 19. 테스트 확장

### SettingsManagerTests

기존 설정 테스트를 Version 2에 맞게 확장하였다.

추가 검증 항목:

```text
Brightness 기본 범위
UI Volume 기본 범위
Brightness Validate
UI Volume Validate
Version 2 JSON 왕복
Version 1 → Version 2 마이그레이션
```

### Day52SettingsIntegrationTests

다음 신규 테스트 파일을 추가하였다.

```text
Assets/_ProjectJ/Tests/EditMode/UI/Day52SettingsIntegrationTests.cs
```

검증 내용:

```text
50% Brightness  → Post Exposure -1
100% Brightness → Post Exposure 0
150% Brightness → Post Exposure +1

중복 Keyboard Binding 검출
비중복 Keyboard Binding 허용
```

---

## 20. MainMenu Scene 확장

`MainMenu.unity`의 설정 Canvas를 52일차 구조로 다시 구성하였다.

주요 추가 요소:

```text
Brightness Slider
UI Volume Slider
Reset Bindings Button
Rebind Scroll View
15개 Rebind Button
15개 현재 Binding 표시 Text
InputSystem_Actions 참조
```

52일차 Editor Tool이 51일차 설정 Canvas를 다시 생성한 뒤 새로운 UI를 추가하기 때문에 Scene 변경량이 크게 발생하였다.

---

## 21. TMP Font Asset 변경

52일차 커밋에서는 다음 Font Asset도 변경되었다.

```text
Assets/_ProjectJ/Fonts/Source/NotoSansKR-Bold SDF.asset
Assets/_ProjectJ/Fonts/Source/NotoSansKR-Regular SDF.asset
```

새롭게 추가된 한글 설정 UI 문자열과 키 재지정 항목을 사용하면서 TMP Font Asset의 문자·Glyph·Atlas 데이터가 확장된 것으로 판단된다.

현재 한글 UI 표시를 위해 사용되는 Font Asset이므로 52일차 커밋에 함께 포함된 상태이다.

---

## 22. 변경 파일

### 신규

```text
Assets/_ProjectJ/Scripts/Editor/UI/Day52SettingsCompletionSetupTool.cs
Assets/_ProjectJ/Scripts/Editor/UI/Day52SettingsCompletionSetupTool.cs.meta

Assets/_ProjectJ/Scripts/Runtime/Audio/AudioChannelVolumeController.cs
Assets/_ProjectJ/Scripts/Runtime/Audio/AudioChannelVolumeController.cs.meta

Assets/_ProjectJ/Scripts/Runtime/Audio/ProjectAudioChannel.cs
Assets/_ProjectJ/Scripts/Runtime/Audio/ProjectAudioChannel.cs.meta

Assets/_ProjectJ/Scripts/Runtime/Core/Services/DisplayBrightnessApplier.cs
Assets/_ProjectJ/Scripts/Runtime/Core/Services/DisplayBrightnessApplier.cs.meta

Assets/_ProjectJ/Scripts/Runtime/Input/InputBindingConflictRules.cs
Assets/_ProjectJ/Scripts/Runtime/Input/InputBindingConflictRules.cs.meta

Assets/_ProjectJ/Tests/EditMode/UI/Day52SettingsIntegrationTests.cs
Assets/_ProjectJ/Tests/EditMode/UI/Day52SettingsIntegrationTests.cs.meta
```

### 수정

```text
Assets/_ProjectJ/Fonts/Source/NotoSansKR-Bold SDF.asset
Assets/_ProjectJ/Fonts/Source/NotoSansKR-Regular SDF.asset

Assets/_ProjectJ/Scenes/Game/MainMenu.unity

Assets/_ProjectJ/Scripts/Runtime/Audio/AudioService.cs
Assets/_ProjectJ/Scripts/Runtime/Core/Services/ProjectUserSettings.cs
Assets/_ProjectJ/Scripts/Runtime/Core/Services/SettingsJsonSerializer.cs
Assets/_ProjectJ/Scripts/Runtime/Core/Services/SettingsService.cs
Assets/_ProjectJ/Scripts/Runtime/Player/Input/PlayerInputReader.cs
Assets/_ProjectJ/Scripts/Runtime/ProjectJ.Runtime.asmdef
Assets/_ProjectJ/Scripts/Runtime/UI/Menu/SettingsMenuController.cs

Assets/_ProjectJ/Tests/EditMode/ProjectSettings/SettingsManagerTests.cs
```

---

## 23. 52일차 완료 결과

이번 일차를 통해 설정 시스템은 다음 구조까지 확장되었다.

```text
ProjectUserSettings Version 2
↓
SettingsManager
↓
SettingsService
├─ 그래픽 적용
├─ 밝기 적용
├─ 설정 저장
└─ SettingsChanged
     ├─ AudioService
     ├─ ThirdPersonCameraController
     └─ PlayerInputReader
```

UI 흐름은 다음과 같다.

```text
설정 화면
↓
작업 복사본
├─ 화면
├─ 사운드
├─ 조작
└─ 카메라
↓
적용
↓
런타임 반영
↓
JSON 저장
↓
게임 재실행 시 복원
```

기존에 별도 53일차로 예정했던 기본 키 재지정과 카메라 설정 저장·복원 작업을 52일차에 함께 통합하였다.

따라서 다음 일차부터는 설정 시스템을 벗어나 수직 맵 진행 구조 개발로 넘어갈 수 있다.

---

## 로컬 검증 항목

GitHub에서는 커밋 내용과 변경 파일까지 확인할 수 있으며 Unity Editor의 실제 실행 결과는 로컬 환경에서 확인한다.

```text
[ ] Unity Console Error 0

[ ] SettingsManagerTests Failed 0
[ ] Day52SettingsIntegrationTests Failed 0
[ ] EditMode Run All Failed 0
[ ] PlayMode Run All Failed 0

[ ] Bootstrap → MainMenu 정상

[ ] 화면 탭 Brightness 정상
[ ] 사운드 탭 UI Volume 정상
[ ] 조작 탭 15개 키 표시
[ ] 키 재지정 정상
[ ] Esc 재지정 취소
[ ] 중복 키 거부
[ ] 기본 키 미리보기

[ ] 취소 시 미적용 값 폐기
[ ] 전체 기본값 미리보기
[ ] 적용 시 실제 저장

[ ] 해상도 적용
[ ] 화면 모드 적용
[ ] FPS 적용
[ ] VSync 적용
[ ] 밝기 적용

[ ] Master/Mute 적용
[ ] 연결된 AudioSource의 BGM/SFX/UI 채널 적용

[ ] 마우스 감도 적용
[ ] 게임패드 감도 적용
[ ] Y축 반전 적용
[ ] 재지정 키 실제 플레이 적용

[ ] 게임 재실행 후 전체 설정 복원
```

---

## 다음 개발 방향

설정 시스템을 52일차에 통합 완료하였으므로 다음 일차에서는 실제 경기 진행 공간을 수직 높이 기준으로 확정한다.

다음 목표:

```text
Y = 0       시작 구간
Y = 200     체크포인트 1
Y = 400     체크포인트 2
Y = 600     체크포인트 3
Y = 800     체크포인트 4
Y = 1000    정상 Goal
```

이를 통해 전체 맵을 5개의 수직 진행 구간으로 나누고 체크포인트와 최종 정상 도착 구조를 완성하는 방향으로 진행한다.

---

## 커밋

```text
52일차 : 화면·사운드·조작·카메라 설정 통합 및 저장·복원 완성
```

```text
f2f4129e272f0b371a4cfd354cce821153a3fada
```
