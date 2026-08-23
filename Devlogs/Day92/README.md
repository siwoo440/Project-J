# Project J 개발일지 — 92일차

## 오늘의 목표

기존 `Game.unity`의 고정 테스트 경기장 Hierarchy를 정리하고, Game Scene 직접 실행용 Day76 Test Flow가 Bootstrap에서 시작하는 정상 Scene Flow와 서로 간섭하지 않도록 분리한다.

## 구현 내용

### 1. Game Scene 테스트 경기장 Hierarchy 정리

기존 경기장 오브젝트의 위치와 Gameplay Component는 유지하면서 주요 진행 구조의 Hierarchy 순서를 정리했다.

정리 기준:

```text
=== DAY77 4 PLAYER TEST MAP ===
├─ === SYSTEM ===
├─ === START ===
├─ === SECTION 01 / CP1 ===
├─ === SECTION 02 / CP2 ===
├─ === SECTION 03 / CP3 ===
├─ === SECTION 04 / CP4 ===
└─ === FINISH ===
```

기존 맵을 새로 생성하지 않고 현재 고정 테스트 경기장을 그대로 재사용하도록 구성했다.

### 2. Spawn Point 구조 정리

Day76 멀티플레이 테스트용 Spawn Point를 기존 위치와 Slot 설정을 유지한 상태에서 `Spawn_00`부터 `Spawn_07` 순서로 정리하도록 했다.

```text
=== DAY76 MULTIPLAYER TEST ===
└─ SpawnPoints
   ├─ Spawn_00
   ├─ Spawn_01
   ├─ Spawn_02
   ├─ Spawn_03
   ├─ Spawn_04
   ├─ Spawn_05
   ├─ Spawn_06
   └─ Spawn_07
```

### 3. Day76 Test Flow와 정상 Scene Flow 분리

기존 Day76 Runtime Installer는 `Game.unity`가 로드될 때마다 Test Flow를 생성할 수 있는 구조였다.

이를 수정해 Play 시작 시 최초 Scene을 기록하고, 최초 실행 Scene이 `Game.unity`인 경우에만 `ProjectJDay76TestFlow`를 생성하도록 변경했다.

이에 따라 다음 두 실행 흐름을 분리했다.

```text
Game.unity 직접 실행
→ Day76 Test Flow 사용

Bootstrap
→ MainMenu
→ Lobby
→ Game
→ Day76 Test Flow 생성 안 함
```

정상 온라인 Scene Flow에서 Day76 Test Flow가 기존 Lobby Flow를 비활성화하는 문제를 방지한다.

### 4. Game Scene 정리용 Editor Installer 추가

다음 메뉴를 추가했다.

```text
Project J
→ Scene
→ 92일차 Game Scene 테스트 경기장 정리
```

실행 시 다음 작업을 자동으로 처리한다.

- 실제 `Game.unity` 열기
- 경기장 주요 Group 순서 정리
- Spawn Point 순서 정리
- 필수 Group 중복 검사
- Spawn Point 누락 검사
- 활성 Camera 중복 검사
- 활성 AudioListener 중복 검사
- Scene 저장

Camera나 AudioListener가 중복된 경우 자동 삭제하지 않고 Warning만 출력하도록 구성했다.

## 생성·수정 파일

### 생성

```text
Assets/ProjectJ/Editor/ProjectJDay92GameSceneOrganizer.cs
Assets/ProjectJ/Editor/ProjectJDay92GameSceneOrganizer.cs.meta
```

### 수정

```text
Assets/ProjectJ/Network/Fusion/Test/ProjectJDay76RuntimeInstaller.cs
Assets/ProjectJ/Scenes/Game.unity
```

### 삭제

없음.

## 확인 결과

GitHub 최신 커밋 기준으로 91일차 이후 정확히 1개의 커밋이 추가되었으며, 92일차에 필요한 네 파일이 모두 반영되어 있다.

`Game.unity`에도 실제 Hierarchy 순서 변경이 포함되어 있어 Editor Installer 실행 결과가 저장된 것을 확인했다.

GitHub Actions 또는 별도 CI 상태는 등록되어 있지 않아 자동 PlayMode 테스트 결과는 GitHub에서 확인할 수 없다.

## 92일차 결과

- Game Scene 고정 테스트 경기장 구조 정리
- START → SECTION01~04 → FINISH 순서 정리
- Spawn_00~07 구조 유지
- Day76 직접 실행 테스트 Flow 유지
- 정상 Bootstrap Scene Flow와 Day76 Test Flow 분리
- Camera·AudioListener 중복 검사 추가
- 기존 경기장 Gameplay 구조와 위치 유지
