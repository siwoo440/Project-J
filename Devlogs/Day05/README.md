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
| 입력 시스템 | Unity Input System 1.20.0 |
| 저장소 | siwoo440/Project-J |

---

# 5일차 : Input System 액션 맵 구성

## 개발 목표

Unity 기본 템플릿의 입력 구성을 Project J 전용 입력 구조로 교체하고, 키보드·마우스와 게임패드 입력을 `Gameplay` 및 `UI` 액션 맵으로 분리했습니다.

아직 실제 캐릭터 이동이나 UI 조작 기능은 구현하지 않고, 이후 플레이어·카메라·아이템·UI 시스템에서 공통으로 사용할 입력 이름, 바인딩, Control Scheme과 검증 환경을 먼저 확립했습니다.

이번 일차의 핵심 목표는 다음과 같습니다.

- Unity 기본 `Player` 액션 맵을 Project J 전용 `Gameplay` 맵으로 교체
- 게임 플레이용 필수 액션 14개 정의
- UI용 필수 액션 6개 정의
- 키보드·마우스 Control Scheme 구성
- 게임패드 Control Scheme 구성
- 불필요한 XR·Touch·Joystick 입력 제거
- 입력 이름과 에셋 경로 상수화
- Tests 씬 입력 검증 도구 구성
- 필수 액션과 바인딩 자동 테스트 추가
- Play Mode 시작 씬을 Tests와 Bootstrap 사이에서 전환하는 Editor 메뉴 추가

---

## 최신 커밋

| 항목 | 내용 |
|---|---|
| 커밋 제목 | `5일차 : Input System 액션 맵 구성` |
| 커밋 SHA | `1fa21427b94b8c5d4bda76610a344758050c0d74` |
| 브랜치 | `main` |
| 이전 커밋 | `013c02b8b78ec20501cbcde47fb9b90235861045` |
| 커밋 링크 | https://github.com/siwoo440/Project-J/commit/1fa21427b94b8c5d4bda76610a344758050c0d74 |

---

# 최신 커밋 검토 결과

최신 커밋을 기준으로 다음 항목을 확인했습니다.

- 커밋 제목이 `5일차 : Input System 액션 맵 구성`으로 정상 등록
- 기존 입력 에셋의 `.meta` GUID 유지
- `Gameplay` 액션 맵 생성
- `UI` 액션 맵 재구성
- 키보드·마우스와 게임패드 Control Scheme 구성
- 기존 XR·Touch·Joystick Control Scheme 제거
- Runtime 어셈블리의 `Unity.InputSystem` 참조 추가
- Editor 어셈블리의 `Unity.InputSystem` 참조 추가
- EditMode Tests 어셈블리의 `Unity.InputSystem` 참조 추가
- 입력 이름 상수 관리 코드 추가
- 입력 액션 디버그 모니터 추가
- Tests 씬에 `ProjectJ_InputDebug` 오브젝트 추가
- 입력 구성과 Play Mode 시작 씬 전환용 Editor 도구 추가
- 필수 액션, Control Scheme과 바인딩을 검증하는 EditMode 테스트 추가

저장소에서 확인 가능한 범위에서는 수정이 필요한 치명적인 구조 오류를 발견하지 못했습니다.

GitHub Actions와 자동 상태 검사가 아직 구성되지 않았으므로 다음 항목은 로컬 Unity 에디터에서 최종 확인해야 합니다.

```text
Console Error: 0개
EditMode Passed: 15개
EditMode Failed: 0개
키보드·마우스 입력 정상
게임패드 입력 정상
Play Mode 시작 씬 Bootstrap 복원 완료
```

---

# 구현 내용

## 1. Input Actions 에셋 재구성

파일 위치:

```text
Assets/_ProjectJ/Settings/Input/InputSystem_Actions.inputactions
```

Unity 기본 템플릿의 `Player` 액션 맵을 삭제하고 Project J에서 사용할 두 액션 맵으로 재구성했습니다.

```text
InputSystem_Actions
├─ Gameplay
└─ UI
```

입력 에셋의 기존 `.meta` 파일은 유지했습니다.

```text
Assets/_ProjectJ/Settings/Input/InputSystem_Actions.inputactions.meta
```

유지된 GUID:

```text
052faaac586de48259a63d0c4782560b
```

이를 통해 기존 Unity 에셋 참조가 끊기지 않도록 했습니다.

---

# Gameplay 액션 맵

## 2. Gameplay 액션 구성

`Gameplay` 액션 맵에는 다음 14개 액션을 정의했습니다.

| 액션 | 타입 | 역할 |
|---|---|---|
| Move | Value / Vector2 | 캐릭터 이동 방향 |
| Look | Value / Vector2 | 카메라 시점 입력 |
| Jump | Button | 점프 |
| Sprint | Button | 달리기 |
| Crouch | Button | 앉기 |
| Push | Button | 다른 플레이어 밀치기 |
| UseItem | Button | 현재 아이템 사용 |
| SelectPreviousItem | Button | 이전 아이템 선택 |
| SelectNextItem | Button | 다음 아이템 선택 |
| ShowItem | Button | 보유 아이템 보여주기 |
| DropItem | Button | 아이템 버리기 |
| Interact | Button | 오브젝트 상호작용 |
| Scoreboard | Button | 경기 순위표 표시 |
| Pause | Button | 일시정지 메뉴 |

`Move`와 `Look`은 연속적인 방향값이 필요하므로 `Vector2` 타입으로 구성했습니다.

나머지 액션은 누름 여부를 사용하는 `Button` 타입입니다.

---

## 3. 키보드·마우스 입력 구성

| 액션 | 키보드·마우스 입력 |
|---|---|
| Move | W·A·S·D |
| Look | 마우스 이동 |
| Jump | Space |
| Sprint | 왼쪽 Shift |
| Crouch | 왼쪽 Ctrl |
| Push | 마우스 왼쪽 버튼 |
| UseItem | 마우스 오른쪽 버튼 |
| SelectPreviousItem | Q |
| SelectNextItem | E |
| ShowItem | R |
| DropItem | G |
| Interact | F |
| Scoreboard | Tab |
| Pause | ESC |

### Move 바인딩

WASD는 `2D Vector Composite`로 구성했습니다.

```text
Up    → W
Down  → S
Left  → A
Right → D
```

입력 예시:

```text
W     → (0, 1)
S     → (0, -1)
A     → (-1, 0)
D     → (1, 0)
W + D → (1, 1)
```

### Look 바인딩

마우스 포인터의 절대 위치가 아니라 프레임 간 이동량을 사용합니다.

```text
<Mouse>/delta
```

실제 마우스 감도와 카메라 회전 처리는 이후 카메라 시스템에서 구현합니다.

---

## 4. 게임패드 입력 구성

| 액션 | 게임패드 입력 |
|---|---|
| Move | 왼쪽 스틱 |
| Look | 오른쪽 스틱 |
| Jump | South 버튼 |
| Sprint | 왼쪽 스틱 클릭 |
| Crouch | East 버튼 |
| Push | 오른쪽 숄더 |
| UseItem | 오른쪽 트리거 |
| SelectPreviousItem | D-pad 왼쪽 |
| SelectNextItem | D-pad 오른쪽 |
| ShowItem | West 버튼 |
| DropItem | D-pad 아래 |
| Interact | North 버튼 |
| Scoreboard | Select 버튼 |
| Pause | Start 버튼 |

게임패드 버튼은 특정 제조사의 문자 이름 대신 Unity의 공통 위치 이름을 사용했습니다.

```text
buttonSouth
buttonEast
buttonWest
buttonNorth
```

따라서 Xbox, PlayStation과 기타 호환 게임패드에서 물리적 위치를 기준으로 동일한 역할을 유지할 수 있습니다.

---

# UI 액션 맵

## 5. UI 액션 구성

`UI` 액션 맵에는 다음 6개 액션을 정의했습니다.

| 액션 | 타입 | 역할 |
|---|---|---|
| Navigate | PassThrough / Vector2 | 메뉴 선택 이동 |
| Submit | Button | 선택 항목 확인 |
| Cancel | Button | 취소 또는 뒤로 가기 |
| Point | PassThrough / Vector2 | 마우스 포인터 위치 |
| Click | PassThrough / Button | UI 클릭 |
| ScrollWheel | PassThrough / Vector2 | 스크롤 입력 |

### Navigate 입력

```text
W·A·S·D
방향키
게임패드 왼쪽 스틱
게임패드 D-pad
```

### Submit 입력

```text
Enter
Space
게임패드 South 버튼
```

### Cancel 입력

```text
ESC
게임패드 East 버튼
```

### 마우스 UI 입력

```text
Point       → Mouse Position
Click       → Mouse Left Button
ScrollWheel → Mouse Scroll
```

5일차에서는 입력만 정의했으며 실제 EventSystem과 메뉴 UI 연결은 이후 UI 구현 일정에서 진행합니다.

---

# Control Scheme

## 6. Keyboard&Mouse 구성

Control Scheme 이름:

```text
Keyboard&Mouse
```

필수 장치:

```text
Keyboard
Mouse
```

키보드와 마우스 바인딩은 모두 `Keyboard&Mouse` 그룹에 등록했습니다.

---

## 7. Gamepad 구성

Control Scheme 이름:

```text
Gamepad
```

필수 장치:

```text
Gamepad
```

게임패드 바인딩은 모두 `Gamepad` 그룹에 등록했습니다.

기존 Unity 템플릿에 포함된 다음 Control Scheme은 제거했습니다.

```text
Touch
Joystick
XR
```

Project J의 초기 출시 플랫폼은 Steam PC이며, 5일차에서는 키보드·마우스와 일반 게임패드만 지원 대상으로 구성했습니다.

---

# Assembly Definition 변경

## 8. ProjectJ.Runtime 수정

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/ProjectJ.Runtime.asmdef
```

추가된 참조:

```text
Unity.InputSystem
```

Runtime 코드에서 다음 형식을 사용할 수 있게 됐습니다.

```text
InputActionAsset
InputActionMap
InputAction
InputActionPhase
```

---

## 9. ProjectJ.Editor 수정

파일 위치:

```text
Assets/_ProjectJ/Scripts/Editor/ProjectJ.Editor.asmdef
```

최종 참조:

```text
ProjectJ.Runtime
Unity.InputSystem
```

Editor 도구에서 Input Actions 에셋을 불러오고 검증할 수 있도록 구성했습니다.

---

## 10. ProjectJ.Tests.EditMode 수정

파일 위치:

```text
Assets/_ProjectJ/Tests/EditMode/ProjectJ.Tests.EditMode.asmdef
```

최종 참조:

```text
ProjectJ.Runtime
Unity.InputSystem
TestAssemblies
```

EditMode 테스트에서 `InputActionAsset`, `InputActionMap`과 바인딩 정보를 검사할 수 있도록 구성했습니다.

---

# 입력 코드 구조

## 11. ProjectInputNames 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Input/ProjectInputNames.cs
```

입력 에셋 경로, Control Scheme 이름과 액션 이름을 한 곳에서 관리합니다.

주요 상수:

```text
AssetPath
KeyboardMouseScheme
GamepadScheme
```

Gameplay 액션 상수:

```text
Move
Look
Jump
Sprint
Crouch
Push
UseItem
SelectPreviousItem
SelectNextItem
ShowItem
DropItem
Interact
Scoreboard
Pause
```

UI 액션 상수:

```text
Navigate
Submit
Cancel
Point
Click
ScrollWheel
```

앞으로 문자열을 코드 여러 위치에 직접 반복하지 않고 다음과 같이 사용합니다.

```csharp
ProjectInputNames.Gameplay.Move
ProjectInputNames.Gameplay.Interact
ProjectInputNames.UI.Navigate
```

입력 액션 이름 변경 시 수정 범위를 줄일 수 있습니다.

---

## 12. InputDebugMap 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Input/InputDebugMap.cs
```

개발용 입력 검증 대상 맵을 enum으로 관리합니다.

```text
Gameplay
UI
```

`InputActionDebugMonitor`의 Inspector에서 검증할 액션 맵을 선택할 때 사용합니다.

---

## 13. InputActionDebugMonitor 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Input/InputActionDebugMonitor.cs
```

Tests 씬에서 실제 입력 액션이 정상 작동하는지 확인하기 위한 개발용 컴포넌트입니다.

주요 기능:

- InputActionAsset 연결 확인
- 원본 입력 에셋의 런타임 복제본 생성
- 선택한 액션 맵 활성화
- 모든 액션의 `Performed` 이벤트 기록
- 최근 수행 액션과 실제 컨트롤 이름 표시
- Move와 Look Vector2 값 표시
- UI Navigate, Point와 ScrollWheel 값 표시
- Editor와 Development Build에서만 디버그 창 표시
- 비활성화 시 액션 이벤트와 복제 에셋 정리

원본 Input Actions 에셋을 직접 활성화하지 않고 런타임 복제본을 사용하여 다른 시스템의 상태에 영향을 주지 않도록 구성했습니다.

---

# Tests 씬 구성

## 14. ProjectJ_InputDebug 오브젝트 추가

수정된 씬:

```text
Assets/_ProjectJ/Scenes/Game/Tests.unity
```

추가된 게임 오브젝트:

```text
ProjectJ_InputDebug
```

연결된 컴포넌트:

```text
InputActionDebugMonitor
```

Inspector 설정:

```text
Input Actions: InputSystem_Actions
Map To Test: Gameplay
Log Performed Actions: 활성화
```

입력 에셋 참조에는 기존 `.meta` GUID가 사용됐습니다.

```text
052faaac586de48259a63d0c4782560b
```

---

# Editor 자동화

## 15. Day05InputSetupTool 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Editor/Day05InputSetupTool.cs
```

Unity 상단 메뉴에 다음 기능을 추가했습니다.

```text
Project J
└─ Day 05
   ├─ Configure Input Debug
   ├─ Use Tests As Play Mode Start Scene
   └─ Restore Bootstrap As Play Mode Start Scene
```

### Configure Input Debug

다음 작업을 자동으로 수행합니다.

```text
입력 에셋 경로 확인
→ Gameplay 맵 존재 확인
→ Gameplay 필수 액션 확인
→ UI 맵 존재 확인
→ UI 필수 액션 확인
→ Tests 씬 열기
→ ProjectJ_InputDebug 생성 또는 조회
→ InputActionDebugMonitor 추가
→ Input Actions 에셋 연결
→ Tests 씬 저장
```

### Use Tests As Play Mode Start Scene

입력 수동 검증 중 Play 버튼을 눌렀을 때 Bootstrap이 아닌 Tests 씬에서 시작하도록 설정합니다.

```text
Play Mode Start Scene: Tests
```

### Restore Bootstrap As Play Mode Start Scene

입력 검증이 끝난 뒤 실제 게임 시작 흐름으로 복원합니다.

```text
Play Mode Start Scene: Bootstrap
```

Play Mode 실행 중이거나 진입 중일 때는 세 메뉴를 사용할 수 없도록 구성했습니다.

---

# 자동 테스트

## 16. InputActionAssetTests 생성

파일 위치:

```text
Assets/_ProjectJ/Tests/EditMode/InputActionAssetTests.cs
```

다음 6개의 EditMode 테스트를 추가했습니다.

### InputAssetContainsExpectedActionMaps

다음 액션 맵이 존재하는지 검사합니다.

```text
Gameplay
UI
```

### GameplayMapContainsExpectedActions

Gameplay 필수 액션 14개가 모두 존재하는지 검사합니다.

### UIMapContainsExpectedActions

UI 필수 액션 6개가 모두 존재하는지 검사합니다.

### KeyboardMouseAndGamepadControlSchemesExist

다음 두 Control Scheme이 존재하고 불필요한 Scheme이 추가되지 않았는지 검사합니다.

```text
Keyboard&Mouse
Gamepad
```

### RequiredKeyboardMouseBindingsExist

다음 필수 바인딩을 검사합니다.

```text
W·A·S·D
Mouse Delta
Space
Left Shift
Left Ctrl
Left Click
Right Click
Q
E
R
G
F
Tab
Escape
```

### RequiredGamepadBindingsExist

다음 필수 바인딩을 검사합니다.

```text
Left Stick
Right Stick
South
Left Stick Press
East
Right Shoulder
Right Trigger
D-pad Left
D-pad Right
West
D-pad Down
North
Select
Start
```

---

# 전체 테스트 구성

기존 테스트:

```text
ProjectStructureTests: 2개
GameSceneCatalogTests: 3개
GameServiceRegistryTests: 4개
```

5일차 신규 테스트:

```text
InputActionAssetTests: 6개
```

예상 전체 결과:

```text
Passed: 15
Failed: 0
Ignored: 0
```

---

# 생성·수정된 주요 파일

## 수정된 파일

```text
Assets/_ProjectJ/Scenes/Game/Tests.unity
Assets/_ProjectJ/Settings/Input/InputSystem_Actions.inputactions
Assets/_ProjectJ/Scripts/Runtime/ProjectJ.Runtime.asmdef
Assets/_ProjectJ/Scripts/Editor/ProjectJ.Editor.asmdef
Assets/_ProjectJ/Tests/EditMode/ProjectJ.Tests.EditMode.asmdef
```

## 새로 생성된 파일

```text
Assets/_ProjectJ/Scripts/Runtime/Input/ProjectInputNames.cs
Assets/_ProjectJ/Scripts/Runtime/Input/InputDebugMap.cs
Assets/_ProjectJ/Scripts/Runtime/Input/InputActionDebugMonitor.cs
Assets/_ProjectJ/Scripts/Editor/Day05InputSetupTool.cs
Assets/_ProjectJ/Tests/EditMode/InputActionAssetTests.cs
```

각 폴더와 스크립트의 `.meta` 파일도 함께 Git에 등록했습니다.

---

# 주요 프로젝트 구조

```text
Assets/_ProjectJ
├─ Scenes
│  └─ Game
│     └─ Tests.unity
├─ Settings
│  └─ Input
│     ├─ InputSystem_Actions.inputactions
│     └─ InputSystem_Actions.inputactions.meta
├─ Scripts
│  ├─ Runtime
│  │  ├─ ProjectJ.Runtime.asmdef
│  │  └─ Input
│  │     ├─ ProjectInputNames.cs
│  │     ├─ InputDebugMap.cs
│  │     └─ InputActionDebugMonitor.cs
│  └─ Editor
│     ├─ ProjectJ.Editor.asmdef
│     └─ Day05InputSetupTool.cs
└─ Tests
   └─ EditMode
      ├─ ProjectJ.Tests.EditMode.asmdef
      └─ InputActionAssetTests.cs
```

---

# 수동 검증 절차

## 17. Gameplay 입력 검증

Unity 메뉴:

```text
Project J
→ Day 05
→ Use Tests As Play Mode Start Scene
```

Play Mode를 실행한 뒤 다음 입력을 확인합니다.

```text
W·A·S·D
마우스 이동
Space
왼쪽 Shift
왼쪽 Ctrl
마우스 왼쪽 버튼
마우스 오른쪽 버튼
Q
E
R
G
F
Tab
ESC
```

디버그 창에서 다음 정보가 표시되어야 합니다.

```text
Action Map: Gameplay
Last Action
Move
Look
```

Console에는 다음 형식의 로그가 표시됩니다.

```text
[Input] Gameplay / Jump / Space
[Input] Gameplay / Push / Left Button
[Input] Gameplay / Interact / F
```

---

## 18. 게임패드 입력 검증

Play Mode 실행 전에 게임패드를 연결합니다.

다음 입력을 확인합니다.

```text
왼쪽 스틱
오른쪽 스틱
South 버튼
왼쪽 스틱 클릭
East 버튼
오른쪽 숄더
오른쪽 트리거
D-pad 왼쪽
D-pad 오른쪽
West 버튼
D-pad 아래
North 버튼
Select 버튼
Start 버튼
```

각 입력에 대응하는 액션 이름이 디버그 창 또는 Console에 표시되어야 합니다.

---

## 19. UI 입력 검증

Play Mode를 종료하고 `ProjectJ_InputDebug`를 선택합니다.

Inspector 변경:

```text
Map To Test: UI
```

다시 Play Mode를 실행한 뒤 다음 입력을 확인합니다.

```text
WASD 또는 방향키
게임패드 왼쪽 스틱 또는 D-pad
Enter 또는 Space
ESC
마우스 이동
마우스 왼쪽 클릭
마우스 휠
```

실제 UI는 아직 연결하지 않았으므로 메뉴 선택이 이동하지는 않습니다. 디버그 창과 Console에서 액션 발생 여부만 확인합니다.

---

## 20. Play Mode 시작 씬 복원

입력 검증이 끝나면 다음 메뉴를 실행합니다.

```text
Project J
→ Day 05
→ Restore Bootstrap As Play Mode Start Scene
```

정상 게임 실행 흐름:

```text
Bootstrap
→ 공통 서비스 초기화
→ MainMenu
```

---

# 검증 결과

| 검증 항목 | 저장소 확인 |
|---|:---:|
| 최신 커밋 제목 정상 | 완료 |
| 기존 입력 에셋 GUID 유지 | 완료 |
| Gameplay 액션 맵 생성 | 완료 |
| Gameplay 액션 14개 구성 | 완료 |
| UI 액션 맵 구성 | 완료 |
| UI 액션 6개 구성 | 완료 |
| Keyboard&Mouse Control Scheme 구성 | 완료 |
| Gamepad Control Scheme 구성 | 완료 |
| XR·Touch·Joystick Scheme 제거 | 완료 |
| Runtime Input System 참조 | 완료 |
| Editor Input System 참조 | 완료 |
| Tests Input System 참조 | 완료 |
| 입력 이름 상수화 | 완료 |
| Tests 씬 디버그 오브젝트 추가 | 완료 |
| 입력 구성 Editor 도구 추가 | 완료 |
| EditMode 테스트 6개 작성 | 완료 |
| GitHub Actions 자동 검사 | 미구성 |

로컬 Unity 에디터 최종 확인 항목:

```text
Console Error: 0개
EditMode Passed: 15개
EditMode Failed: 0개
Keyboard&Mouse 입력 정상
Gamepad 입력 정상
UI 액션 입력 정상
Play Mode 시작 씬 Bootstrap 복원
```

---

# 이후 확장 방향

5일차에서 정의한 입력은 이후 시스템에서 다음과 같이 사용합니다.

| 입력 | 이후 연결 대상 |
|---|---|
| Move | 플레이어 이동 |
| Look | 3인칭 카메라 |
| Jump | 플레이어 점프 |
| Sprint | 달리기와 스태미나 |
| Crouch | 앉기와 낮은 통로 |
| Push | 플레이어 밀치기 |
| UseItem | 아이템 사용 |
| Item Select | 아이템 슬롯 |
| Interact | 오브젝트 상호작용 |
| Scoreboard | 경기 순위표 |
| Pause | 일시정지 메뉴 |
| UI Actions | 메뉴와 HUD |

실제 게임 기능이 구현될 때 입력 키를 직접 읽지 않고 이번 일차에서 구성한 Input Actions를 기준으로 연결합니다.

---

# 커밋 정보

```text
5일차 : Input System 액션 맵 구성
```

```text
https://github.com/siwoo440/Project-J/commit/1fa21427b94b8c5d4bda76610a344758050c0d74
```
