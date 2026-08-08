# Project J 개발 일지

## 51일차 : 설정 메뉴 UI 및 4개 탭 기본 기능 구현

### 개발 목표

50일차에 정리한 `SettingsManager`와 사용자 설정 데이터 구조를 실제 설정 메뉴 UI에 연결한다.

이번 일차에서는 설정 화면을 다음 4개 탭으로 구성하고, 실제 저장값을 바로 수정하지 않는 작업 복사본 기반 편집 흐름을 완성한다.

- 화면
- 사운드
- 조작
- 카메라

또한 설정 화면 하단에 다음 공통 기능을 구현한다.

- 기본값
- 취소
- 적용

---

## 기준 커밋

### 이전 기준

```text
345f2ebef6fc058bc2cea03da9ee8f9b6b1cfebb
50일차 : SettingsManager 및 설정 데이터 기반 통합
```

### 51일차 완료 커밋

```text
8881ee320044dba3e02d3cb8ef164d984387e6c6
51일차 : 설정 메뉴 UI 및 4개 탭 기본 기능 구현
```

---

## 1. 설정 메뉴 UI 기본 구조 구현

`MainMenu.unity`에 설정 화면을 추가하였다.

기본 구조는 다음과 같다.

```text
MainMenu
├─ EventSystem
└─ SettingsMenuCanvas
   ├─ MainMenuRoot
   │  ├─ GameTitle
   │  ├─ DevelopmentLabel
   │  └─ OpenSettingsButton
   │
   └─ SettingsPanel
      └─ SettingsCard
         ├─ Title
         ├─ TabRoot
         │  ├─ ScreenTabButton
         │  ├─ SoundTabButton
         │  ├─ ControlsTabButton
         │  └─ CameraTabButton
         │
         ├─ ContentRoot
         │  ├─ ScreenTabPanel
         │  ├─ SoundTabPanel
         │  ├─ ControlsTabPanel
         │  └─ CameraTabPanel
         │
         ├─ DefaultsButton
         ├─ CancelButton
         ├─ ApplyButton
         └─ StatusText
```

`SettingsMenuCanvas`에는 `SettingsMenuController`를 연결하여 설정 데이터와 UI를 관리하도록 구성하였다.

---

## 2. 화면 탭 구현

화면 탭에 다음 기본 설정 UI를 추가하였다.

- 해상도
- 화면 모드
- 최대 FPS
- VSync

### 해상도

현재 실행 장치에서 사용할 수 있는 `Screen.resolutions`를 읽어 목록을 구성한다.

같은 가로·세로 크기에서 주사율만 다른 경우 하나의 해상도 항목으로 정리한다.

예시:

```text
1280 × 720
1600 × 900
1920 × 1080
2560 × 1440
```

해상도 선택은 좌우 버튼 방식으로 구현하였다.

### 화면 모드

다음 세 종류를 선택할 수 있도록 구성하였다.

```text
전체 화면 창
창 모드
독점 전체 화면
```

### 최대 FPS

다음 값을 순환 선택할 수 있도록 구성하였다.

```text
제한 없음
30 FPS
60 FPS
90 FPS
120 FPS
144 FPS
165 FPS
240 FPS
360 FPS
```

### VSync

Toggle을 통해 VSync 사용 여부를 작업 복사본에 저장한다.

---

## 3. 사운드 탭 구현

사운드 탭에 다음 설정을 연결하였다.

- 마스터 음량
- BGM 음량
- SFX 음량
- 전체 음소거

각 음량은 Slider로 조작하며 현재 값을 백분율로 표시한다.

예시:

```text
Master : 100%
BGM    : 80%
SFX    : 70%
```

51일차에서는 UI와 설정 데이터 연결을 우선 구현하였다.

BGM·SFX의 실제 개별 오디오 채널 적용은 다음 설정 완성 단계에서 확장하도록 남겨두었다.

---

## 4. 조작 탭 기본 구조 구현

51일차의 조작 탭은 실제 키 재지정 기능을 구현하기 전의 기본 구조를 담당한다.

현재 상태에 따라 다음 정보를 표시한다.

```text
기본 키 사용 중
```

또는

```text
저장된 키 재지정 있음
```

실제 Keyboard&Mouse 키 재지정 UI와 중복 키 검사는 다음 설정 완성 단계에서 연결하도록 구성하였다.

---

## 5. 카메라 탭 구현

기존 사용자 설정 데이터에 존재하던 카메라 설정을 UI에 연결하였다.

- 마우스 감도
- 게임패드 시점 속도
- Y축 반전

### 마우스 감도

```text
0.01 ~ 2.00
```

범위의 Slider로 구성하였다.

### 게임패드 시점 속도

```text
30°/s ~ 720°/s
```

범위로 구성하였다.

### Y축 반전

Toggle을 통해 설정하도록 구성하였다.

기존 `ThirdPersonCameraController`의 설정 적용 구조를 재사용하므로 새로운 카메라 시스템을 별도로 만들지 않았다.

---

## 6. 작업 복사본 기반 편집 구조

50일차에 구현한 `SettingsManager`의 작업 복사본 기능을 설정 메뉴에 실제로 연결하였다.

설정 화면을 열면 다음 흐름으로 동작한다.

```text
설정 화면 열기
↓
SettingsManager.CreateWorkingCopy()
↓
ProjectUserSettings 작업 복사본 생성
↓
UI에서는 작업 복사본만 수정
```

이 구조를 통해 사용자가 값을 움직이는 즉시 실제 저장 데이터가 변경되는 문제를 방지하였다.

---

## 7. 적용 버튼

`적용` 버튼을 누르면 작업 복사본을 실제 사용자 설정으로 확정한다.

```text
UI 작업 복사본
↓
SettingsManager.Apply()
↓
SettingsService
↓
설정 검증
↓
런타임 적용
↓
user-settings.json 저장
```

적용 성공 후 실제 저장된 설정을 다시 작업 복사본으로 가져와 UI를 동기화하도록 구성하였다.

---

## 8. 취소 버튼

`취소` 버튼은 현재 작업 복사본을 저장하지 않고 폐기한다.

```text
작업 복사본 변경
↓
취소
↓
작업 복사본 폐기
↓
기존 실제 설정 유지
```

따라서 설정 화면에서 값을 변경했더라도 적용하지 않았다면 다음에 설정 화면을 열 때 기존 저장값이 다시 표시된다.

---

## 9. 기본값 버튼

`기본값` 버튼은 실제 설정을 즉시 초기화하지 않는다.

```text
기본값
↓
SettingsManager.CreateDefaultWorkingCopy()
↓
작업 화면에 기본값 표시
```

이 상태에서 `취소`를 누르면 실제 저장 설정은 유지된다.

기본값을 실제 설정으로 저장하려면 반드시 `적용` 버튼을 눌러야 한다.

---

## 10. Editor 자동 구성 도구 추가

다음 Editor Tool을 추가하였다.

```text
Assets/_ProjectJ/Scripts/Editor/UI/Day51SettingsMenuSetupTool.cs
```

Unity 상단 메뉴에서 실행하여 MainMenu의 설정 UI를 자동 구성할 수 있도록 하였다.

자동 구성 과정에서 다음을 처리한다.

- `SettingsMenuCanvas` 생성
- `EventSystem` 생성
- Input System UI Module 구성
- 4개 설정 탭 생성
- 버튼·Slider·Toggle 생성
- `SettingsMenuController` 추가
- Inspector 참조 자동 연결

이를 통해 설정 UI를 수동으로 하나씩 연결하면서 발생할 수 있는 참조 실수를 줄였다.

---

## 11. EditMode 테스트 추가

다음 테스트 파일을 추가하였다.

```text
Assets/_ProjectJ/Tests/EditMode/UI/SettingsMenuWorkingCopyTests.cs
```

테스트에서는 다음 내용을 확인하도록 구성하였다.

- 작업 복사본을 수정해도 원본 설정이 변경되지 않는지 확인
- 기본 설정 작업 복사본이 유효 범위를 유지하는지 확인

51일차 테스트는 설정 UI 전체를 직접 자동 조작하기보다는 작업 복사본 기반의 핵심 데이터 안전성을 우선 검증한다.

---

## 12. 변경 파일

### 신규

```text
Assets/_ProjectJ/Scripts/Editor/UI/Day51SettingsMenuSetupTool.cs
Assets/_ProjectJ/Scripts/Editor/UI/Day51SettingsMenuSetupTool.cs.meta

Assets/_ProjectJ/Scripts/Runtime/UI/Menu/SettingsMenuController.cs
Assets/_ProjectJ/Scripts/Runtime/UI/Menu/SettingsMenuController.cs.meta

Assets/_ProjectJ/Tests/EditMode/UI/SettingsMenuWorkingCopyTests.cs
Assets/_ProjectJ/Tests/EditMode/UI/SettingsMenuWorkingCopyTests.cs.meta
```

### 수정

```text
Assets/_ProjectJ/Scenes/Game/MainMenu.unity
```

### 함께 추가된 개발일지

```text
Devlogs/Day50/README.md
```

51일차 커밋에는 이전에 작성한 50일차 개발일지도 함께 추가되었다.

---

## 13. 51일차 완료 결과

이번 일차를 통해 설정 시스템은 다음 단계까지 연결되었다.

```text
50일차
설정 데이터 / SettingsManager
↓
51일차
실제 설정 UI
↓
화면·사운드·조작·카메라 4개 탭
↓
작업 복사본
↓
적용 / 취소 / 기본값
```

51일차에서는 UI 구조와 편집 흐름을 완성하였고, 화면 밝기·UI 음량·실제 키 재지정·오디오 채널별 적용 등은 다음 설정 완성 단계로 넘겼다.

---

## 로컬 검증 항목

GitHub 커밋 자체에서는 Unity Editor 실행 결과를 확인할 수 없으므로 다음 항목은 로컬 Unity에서 확인하는 기준으로 사용한다.

```text
[ ] Unity Console Error 0
[ ] EditMode Failed 0
[ ] Bootstrap → MainMenu 정상
[ ] 설정 화면 정상 표시
[ ] 화면·사운드·조작·카메라 탭 정상
[ ] 취소 시 미적용 값 폐기
[ ] 기본값은 적용 전까지 미저장
[ ] 적용 시 설정 저장
[ ] 재실행 후 적용값 복원
```

---

## 커밋

```text
51일차 : 설정 메뉴 UI 및 4개 탭 기본 기능 구현
```

```text
8881ee320044dba3e02d3cb8ef164d984387e6c6
```
