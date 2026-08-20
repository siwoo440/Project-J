# 4일차 개발일지 - Input System 기본 액션 확정

## 오늘의 목표

Project J에서 사용할 플레이어 입력 체계를 확정하고,
키보드·마우스와 게임패드가 동일한 Player Action Map을 사용하도록 정리한다.

이번 일차에서는 실제 플레이어 이동 로직을 구현하지 않고,
이후 조작 시스템에서 공통으로 사용할 입력 기준만 구축한다.

## 구현 내용

### 1. Input Actions 정리

기존 Unity 템플릿의 `InputSystem_Actions.inputactions`를
Project J에서 실제 사용할 입력만 남도록 정리했다.

불필요한 XR, Touch, 일반 Joystick 입력과
템플릿 기본 액션을 제거했다.

### 2. Player Action Map 구성

`Player` Action Map에 다음 13개 액션을 구성했다.

- Move
- Look
- Jump
- Sprint
- Crouch
- Push
- UseItem
- ItemSlotLeft
- ItemSlotRight
- Interact
- Scoreboard
- Menu
- Emote

### 3. 키보드·마우스 입력 구성

다음 기본 입력을 설정했다.

| Action | 입력 |
| --- | --- |
| Move | WASD |
| Look | Mouse Delta |
| Jump | Space |
| Sprint | Left Shift |
| Crouch | Left Ctrl |
| Push | Left Mouse Button |
| UseItem | Right Mouse Button |
| ItemSlotLeft | Q |
| ItemSlotRight | E |
| Interact | F |
| Scoreboard | Tab |
| Menu | Escape |
| Emote | G |

### 4. 게임패드 입력 구성

동일한 Player Action Map에서 게임패드 입력을 사용할 수 있도록 구성했다.

| Action | 입력 |
| --- | --- |
| Move | Left Stick |
| Look | Right Stick |
| Jump | South Button |
| Sprint | Left Stick Press |
| Crouch | East Button |
| Push | Right Trigger |
| UseItem | Left Trigger |
| ItemSlotLeft | D-Pad Left |
| ItemSlotRight | D-Pad Right |
| Interact | West Button |
| Scoreboard | Left Shoulder |
| Menu | Start |
| Emote | D-Pad Up |

### 5. Control Scheme 정리

입력 장치 구성을 다음 두 가지로 단순화했다.

- Keyboard&Mouse
- Gamepad

현재 개발 단계에서 필요하지 않은 Touch, XR, Joystick Scheme은 사용하지 않는다.

### 6. 이전 일차 임시 Editor 코드 정리

3일차 Scene 자동 구성에 사용했던 일회성 Editor 스크립트를 제거했다.

삭제한 파일:

- `Assets/ProjectJ/Editor/Day3SceneSetup.cs`
- `Assets/ProjectJ/Editor/Day3SceneSetup.cs.meta`

Scene 구성 완료 후 더 이상 필요하지 않은 자동화 코드를 제거해
일차별 임시 개발 스크립트가 프로젝트에 누적되지 않도록 정리했다.

### 7. Editor Assembly 참조 정리

3일차 Scene 자동 구성 도구를 위해 추가했던
`Unity.InputSystem`, `UnityEngine.UI` 참조를 제거했다.

`ProjectJ.Editor`는 다시 `ProjectJ.Runtime`만 참조하도록 정리했다.

## 테스트

- `InputSystem_Actions`에서 Player Action Map 확인
- 13개 입력 Action 존재 확인
- Keyboard&Mouse Control Scheme 확인
- Gamepad Control Scheme 확인
- WASD와 마우스 입력 인식 확인
- 게임패드 연결 시 스틱과 버튼 입력 인식 확인
- Input Debugger에서 장치 입력 변화 확인
- Unity Console Error 0 확인

## 결과

Project J에서 사용할 기본 입력 체계를 확정했다.

키보드·마우스와 게임패드가 같은 Player Action Map을 사용하도록 구성했으며,
이후 플레이어 이동, 점프, 달리기, 앉기, 경쟁 행동, 아이템 등의 시스템에서
동일한 입력 자산을 기준으로 기능을 연결할 수 있는 상태가 되었다.

또한 이전 일차에 사용한 일회성 Editor 구성 코드를 제거해
개발 보조 스크립트가 프로젝트에 계속 누적되지 않도록 정리했다.
