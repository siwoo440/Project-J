# 10일차 개발일지 - 기반 시스템 전체 회귀 검증

## 오늘의 목표

1~9일차에서 구축한 Project J의 기초 개발 환경이
서로 충돌하지 않고 정상적으로 유지되는지 전체 회귀 검증을 진행한다.

이번 일차에서는 새로운 게임 기능을 구현하지 않는다.

Scene 흐름, Input System, Physics Layer, 공통 데이터,
자동 테스트, Windows Development Build Profile,
Legacy 격리 구조를 한 번에 다시 확인하고
11일차 플레이어 이동 구현에 들어가기 전 기준선을 확정한다.

## 현재 기준 커밋

현재 확인한 최신 원격 커밋:

```text
c3ee13ed35e45ecbe12302667ae2d87b299bbbd8
9일차 : Legacy 시스템 격리 구조 구축
```

9일차 이후 별도의 새 커밋은 없는 상태다.

## 1. Unity Console 확인

Unity 프로젝트를 열고 컴파일이 완전히 끝날 때까지 기다린다.

확인 항목:

- [ ] Console Error 0
- [ ] 빈 Assembly 관련 경고 없음
- [ ] Missing Script 관련 오류 없음
- [ ] Assembly Definition 참조 오류 없음

## 2. Scene 흐름 회귀 확인

다음 Scene 흐름을 PlayMode에서 확인한다.

```text
Bootstrap
↓
MainMenu
↓
START
↓
Game
↓
BACK TO MENU
↓
MainMenu
```

확인 항목:

- [ ] Bootstrap에서 정상 시작
- [ ] MainMenu 자동 이동 정상
- [ ] START 버튼 정상 작동
- [ ] Game Scene 정상 로드
- [ ] BACK TO MENU 정상 작동
- [ ] MainMenu 복귀 정상
- [ ] Scene 전환 중 Console Error 없음

## 3. Input System 확인

`Assets/InputSystem_Actions.inputactions`를 열어
Player Action Map을 확인한다.

확인할 Action:

```text
Move
Look
Jump
Sprint
Crouch
Push
UseItem
ItemSlotLeft
ItemSlotRight
Interact
Scoreboard
Menu
Emote
```

확인 항목:

- [ ] Player Action Map 존재
- [ ] 총 13개 Action 존재
- [ ] Keyboard&Mouse Control Scheme 존재
- [ ] Gamepad Control Scheme 존재
- [ ] WASD Move Binding 정상
- [ ] Mouse Look Binding 정상
- [ ] Space Jump Binding 정상
- [ ] Shift Sprint Binding 정상
- [ ] Ctrl Crouch Binding 정상
- [ ] Escape Menu Binding 정상

이번 일차에서는 실제 플레이어를 움직이지 않는다.
입력 정의가 유지되고 있는지만 확인한다.

## 4. Physics Layer 확인

`Edit → Project Settings → Tags and Layers`에서
다음 Layer가 존재하는지 확인한다.

```text
Player
World
Obstacle
GameplayTrigger
Item
```

확인 항목:

- [ ] Player Layer 존재
- [ ] World Layer 존재
- [ ] Obstacle Layer 존재
- [ ] GameplayTrigger Layer 존재
- [ ] Item Layer 존재

다음으로:

```text
Edit
→ Project Settings
→ Physics
→ Layer Collision Matrix
```

에서 확인한다.

- [ ] Player ↔ Player OFF
- [ ] GameplayTrigger ↔ GameplayTrigger OFF
- [ ] Player ↔ World ON
- [ ] Player ↔ Obstacle ON
- [ ] Player ↔ GameplayTrigger ON
- [ ] Player ↔ Item ON

## 5. 공통 데이터 구조 확인

다음 파일이 정상적으로 유지되는지 확인한다.

```text
Assets/ProjectJ/Runtime/Data/GameData.cs
Assets/ProjectJ/Runtime/Data/GameDataId.cs
```

확인 항목:

- [ ] GameDataId 존재
- [ ] GameData 존재
- [ ] GameData 생성 메뉴 사용 가능
- [ ] 새 GameData 생성 시 ID 자동 생성
- [ ] ID가 32자리 GUID 형식
- [ ] 잘못된 ID 입력 시 자동 재생성
- [ ] Display Name 입력 가능
- [ ] Description 입력 가능

검증용 Asset을 새로 만들었다면 테스트 후 삭제한다.

## 6. EditMode 자동 테스트

Unity 메뉴:

```text
Window
→ General
→ Test Runner
→ EditMode
```

에서 `Run All`을 실행한다.

현재 주요 테스트:

```text
GameDataIdTests
```

확인 항목:

- [ ] Create_ReturnsValidId 통과
- [ ] Create_ReturnsDifferentIds 통과
- [ ] 잘못된 ID 입력 테스트 통과
- [ ] EditMode 전체 Green

## 7. PlayMode 자동 테스트

Test Runner에서:

```text
PlayMode
→ Run All
```

을 실행한다.

현재 주요 테스트:

```text
ProjectSmokeTests
```

확인 항목:

- [ ] PlayMode 테스트 정상 시작
- [ ] 1 Frame 진행 정상
- [ ] Application.isPlaying 검사 통과
- [ ] PlayMode 전체 Green

## 8. Windows Development Build Profile 확인

다음 Build Profile을 확인한다.

```text
ProjectJ_Windows_Development
```

확인 항목:

- [ ] Windows Build Profile 존재
- [ ] Active Profile 설정
- [ ] Development Build ON
- [ ] Autoconnect Profiler OFF
- [ ] Deep Profiling OFF
- [ ] Script Debugging OFF
- [ ] Compression Method LZ4
- [ ] Global Scene List 사용

Global Scene List:

```text
Bootstrap
MainMenu
Game
```

## 9. Windows Development Build 실행 확인

Windows Development Build를 생성한다.

권장 출력 위치:

```text
Builds/Windows_Development/
```

확인 항목:

- [ ] Windows Build 성공
- [ ] 실행 파일 정상 실행
- [ ] Bootstrap 정상
- [ ] MainMenu 정상
- [ ] START → Game 정상
- [ ] Game → MainMenu 정상
- [ ] 실행 중 치명적 오류 없음

`Builds` 폴더의 실제 실행 파일은 Git에 포함하지 않는다.

## 10. Legacy 격리 구조 확인

다음 구조가 유지되는지 확인한다.

```text
Assets/ProjectJ/Legacy/
├─ ProjectJ.Legacy.asmdef
├─ Networking/
└─ Generation/
```

확인 항목:

- [ ] Legacy 폴더 존재
- [ ] Networking 폴더 존재
- [ ] Generation 폴더 존재
- [ ] ProjectJ.Legacy Assembly 존재
- [ ] Auto Referenced OFF
- [ ] Define Constraint에 PROJECTJ_LEGACY 존재
- [ ] 실제 Scripting Define Symbols에는 PROJECTJ_LEGACY 없음
- [ ] ProjectJ.Runtime에서 Legacy 참조 없음
- [ ] Legacy Assembly가 기본 게임 빌드에 영향을 주지 않음

## 11. 프로젝트 구조 최종 확인

기본 구조:

```text
Assets/ProjectJ/
├─ BuildProfiles/
├─ Data/
├─ Legacy/
├─ Runtime/
├─ Scenes/
└─ Tests/
```

확인 항목:

- [ ] 현재 Runtime 코드 정상
- [ ] Tests 구조 정상
- [ ] Legacy 구조 정상
- [ ] 불필요한 임시 테스트 Asset 없음
- [ ] 사용하지 않는 Editor Assembly 없음
- [ ] Missing Meta 또는 Missing Script 없음

## 10일차 최종 완료 기준

다음 조건을 모두 만족하면 10일차 완료로 판단한다.

```text
Console Error 0

EditMode 전체 Green
PlayMode 전체 Green

Bootstrap
→ MainMenu
→ Game
→ MainMenu 정상

Input System 13개 Action 유지

Physics Layer 및 Collision Matrix 정상

GameData / GameDataId 정상

ProjectJ_Windows_Development 정상
Windows Development Build 성공
실행 파일 Scene 흐름 정상

ProjectJ.Legacy 기본 비활성
Runtime → Legacy 의존성 없음
```

## 결과

10일차에서는 새로운 게임 기능을 추가하지 않고
1~9일차에서 구축한 기반 시스템 전체를 회귀 검증한다.

이 검증을 모두 통과하면 Project J의 초기 개발 기반을 확정하고,
11일차부터 실제 플레이어 시스템 구현 단계로 넘어간다.

다음 개발 단계는 카메라 방향을 기준으로
WASD 이동 방향을 계산하는
Camera Relative Movement 구현이다.
