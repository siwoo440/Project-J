# 프로젝트 J 82일차 개발 일지

## 개발 목표
82일차는 Bootstrap, MainMenu, Lobby, MatchLoading, Game을 실제 온라인 Scene Flow로 연결하고 경기 후 Lobby 복귀, Session 종료 후 MainMenu 복귀를 구현하는 작업이다.

## GitHub 기준
- Commit: `9f3e8d816308d9c69b46a89147e208bf236a0fb3`
- Title: `82일차 : 전체 Scene 흐름 연결 및 Lobby·Game 복귀 구조 구현`

## 주요 작업

### Scene Flow Coordinator
`ProjectJDay82SceneFlowCoordinator`를 추가하고 다음 상태를 관리하도록 했다.
- Bootstrap
- MainMenu
- Connecting
- Lobby
- MatchLoading
- Game
- Finished
- ReturningToMainMenu

### Bootstrap → MainMenu
Steam Identity 초기화 상태가 결정되기 전에는 Bootstrap에서 대기하고, 초기화가 해결된 뒤 MainMenu를 로드하도록 연결했다.

### 비공개 Host Room 생성과 참가
기존 `ProjectJFusionBootstrap`을 재사용해 Host Room 생성과 Room Code 참가를 Scene Flow에서 요청하도록 했다. Room Code는 `ProjectJFusionRoomCode.TryNormalize()`를 통해 검증한다.

### Game → Lobby 복귀
`ProjectJNetworkLobbyFlow.RequestReturnToLobby()` 구조를 연결해 현재 Session을 유지한 채 Game에서 Lobby로 돌아갈 수 있도록 했다.

### Session 종료 → MainMenu 복귀
MainMenu로 나갈 때 Fusion Runner가 실행 중이면 먼저 `RequestLeaveRoom()`을 요청하고 Runner 종료를 확인한 뒤 MainMenu를 로드하도록 했다.

### Runtime Installer 보강
기존 Bootstrap 오브젝트에도 다음 컴포넌트가 빠지지 않도록 자동 설치 구조를 정리했다.
- `ProjectJNetworkLobbyFlow`
- `ProjectJPhase6GateDebugView`
- `ProjectJDay82SceneFlowCoordinator`
- `ProjectJDay82SceneFlowDebugView`

### F9 Scene Flow Debug
`ProjectJDay82SceneFlowDebugView`를 추가했다. F9를 통해 현재 Scene, Fusion 상태, Scene Flow 상태와 주요 전환 요청을 확인할 수 있다.

## 변경 파일

### 수정
- `Assets/ProjectJ/Network/Fusion/Bootstrap/ProjectJFusionBootstrapRuntimeInstaller.cs`
- `Assets/ProjectJ/Network/Fusion/Session/ProjectJNetworkLobbyFlow.cs`
- `Assets/ProjectJ/Network/Fusion/Test/ProjectJDay76RuntimeInstaller.cs`

### 추가
- `Assets/ProjectJ/Network/Fusion/Session/ProjectJDay82SceneFlowCoordinator.cs`
- `Assets/ProjectJ/Network/Fusion/Session/ProjectJDay82SceneFlowCoordinator.cs.meta`
- `Assets/ProjectJ/Network/Fusion/Test/ProjectJDay82SceneFlowDebugView.cs`
- `Assets/ProjectJ/Network/Fusion/Test/ProjectJDay82SceneFlowDebugView.cs.meta`

## 확인 흐름
1. Bootstrap
2. Steam 초기화 상태 처리
3. MainMenu
4. Host Room 생성 또는 Room Code 참가
5. Lobby
6. MatchLoading
7. Game
8. 경기 종료
9. Game → Lobby
10. Session 종료
11. MainMenu 복귀

## 결과
기존에 흩어져 있던 Steam 초기화, Fusion Bootstrap, Lobby, Game 이동 구조를 하나의 Scene Flow로 연결했다. 이로써 Game Scene 직접 실행 중심의 테스트 구조에서 실제 온라인 게임 흐름으로 확장할 기반을 만들었다.
