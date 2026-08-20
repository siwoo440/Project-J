# 3일차 개발일지 - Canvas 기반 최소 Scene 흐름 구축

## 오늘의 목표

게임 실행부터 메인 메뉴와 게임 화면까지 이어지는
최소 Scene 흐름을 구축한다.

이후 실제 UI 디자인으로 확장할 수 있도록
디버그 UI가 아닌 Unity Canvas 기반으로 구성한다.

## 구현 내용

### 1. Scene 전환 구조 구성

다음 기본 실행 흐름을 구성했다.

Bootstrap
→ MainMenu
→ Game
→ MainMenu

Bootstrap Scene에서 게임 실행에 필요한 초기 진입 후
MainMenu Scene으로 자동 전환되도록 구성했다.

### 2. Scene 이름 관리

SceneNames를 추가해 Scene 이름을 여러 스크립트에서
직접 문자열로 작성하지 않도록 정리했다.

### 3. SceneNavigator 구현

MainMenu와 Game 사이의 Scene 전환을 담당하는
SceneNavigator를 구현했다.

MainMenu의 START 버튼을 통해 Game Scene으로 이동하고,
Game의 BACK TO MENU 버튼을 통해 MainMenu로 돌아갈 수 있다.

### 4. Canvas 기반 MainMenu UI 구성

MainMenu에 실제 Canvas 기반 UI 구조를 구성했다.

- UI_MainMenu
- Background
- Title
- StartButton
- EventSystem

Canvas Scaler를 사용해 1920×1080을 기준 해상도로 설정했다.

### 5. Canvas 기반 Game UI 구성

Game Scene에 UI_Game Canvas를 구성하고
메인 메뉴로 돌아가기 위한 버튼을 배치했다.

임시 디자인이지만 이후 실제 게임 UI로 확장할 수 있는
Canvas 구조를 사용했다.

### 6. Input System UI 연결

EventSystem에 InputSystemUIInputModule을 사용해
현재 프로젝트의 Input System과 UI 입력 구조를 연결했다.

### 7. Build Settings 구성

Scene 실행 순서를 다음과 같이 설정했다.

0. Bootstrap
1. MainMenu
2. Game

### 8. Scene 자동 구성 도구 사용

3일차 작업을 위해 Editor 전용 Scene 자동 구성 도구를 제작했다.

자동 구성 과정에서 발생할 수 있는 Hierarchy 갱신 문제를 줄이기 위해
씬을 단계적으로 처리하도록 수정했다.

해당 도구는 Scene 구성이 완료된 후에는 필요하지 않으므로
다음 개발 일차에서 제거한다.

## 테스트

- Bootstrap 실행 확인
- MainMenu 자동 진입 확인
- START 버튼으로 Game 이동 확인
- BACK TO MENU 버튼으로 MainMenu 복귀 확인
- Scene 전환 반복 확인
- Canvas UI 버튼 입력 확인
- Console Error 0 확인

## 결과

Project J의 기본 Scene 흐름을 구축했다.

이후 플레이어 입력, 게임 시스템, 고정맵 구현을
Game Scene을 기준으로 단계적으로 추가할 수 있는 상태가 되었다.