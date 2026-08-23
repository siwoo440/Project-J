# 프로젝트 J 94일차 개발 일지

## 개발 목표

94일차는 Bootstrap부터 MainMenu, Lobby, Game, Result, Lobby 복귀와 MainMenu 나가기까지 이어지는 실제 Scene Flow를 정리하고, Scene 전환 중 중복 입력과 Runtime 객체 중복 생성을 점검할 수 있는 안전 장치를 추가하는 작업이다.

## 주요 개발 내용

### 1. 전체 Scene Flow 연결 점검

기존 Day82 Scene Flow를 기준으로 다음 흐름이 실제 UI 흐름으로 이어지도록 정리했다.

```text
Bootstrap
→ MainMenu
→ HOME
→ PLAY
→ Private Match
→ Host / Join
→ Lobby
→ Ready
→ Game
→ Countdown
→ Playing
→ Finished
→ Lobby
```

Lobby에서 LEAVE를 선택하면 Fusion Session을 종료한 뒤 MainMenu로 복귀하는 기존 흐름을 그대로 사용한다.

### 2. Day94 Scene Flow Guard 추가

새로운 `ProjectJDay94SceneFlowGuard`를 추가했다.

Scene 전환 상태를 감지해 전환 중 Root Canvas의 입력을 잠그고, 버튼 연속 클릭으로 Host 생성·Join·Ready·Leave·Scene Load 요청이 중복 발생하지 않도록 구성했다.

전환이 끝나면 기존 Canvas 상태를 다시 복원한다.

### 3. Bootstrap Runtime Installer 연결

`ProjectJFusionBootstrapRuntimeInstaller`에 Day94 Scene Flow Guard 자동 설치를 추가했다.

새 Bootstrap이 만들어지는 경우와 기존 Bootstrap이 존재하는 경우 모두 `ProjectJDay94SceneFlowGuard`가 누락되지 않도록 처리했다.

따라서 별도 Scene Installer 실행이나 Inspector 수동 연결 없이 Runtime에서 자동 적용된다.

### 4. Runtime 객체 중복 Audit 추가

Scene 변경 후 다음 Runtime 요소를 자동 점검하도록 구성했다.

- ProjectJFusionBootstrap
- NetworkRunner
- Network Player
- Main Camera
- AudioListener
- MainMenu 복귀 후 남아 있는 Network Player

정상 상태에서는 Console에 다음 형식의 로그가 출력된다.

```text
[Project J/Day94] Scene Flow Audit OK / Scene: ...
```

중복 객체나 비정상 잔존 상태가 발견되면 Day94 태그가 붙은 Error 또는 Warning을 출력한다.

### 5. Scene 전환 중 UI 입력 잠금

MainMenu, Lobby, Game의 전환 상태에 따라 UI 입력을 잠그도록 했다.

주요 잠금 대상 상태는 다음과 같다.

- MainMenu에서 Host / Join 연결 중
- Lobby 진입 중
- Lobby → Game MatchLoading
- GamePreparing
- Countdown
- Game → Lobby 복귀 중
- Lobby → MainMenu 나가기 중

### 6. Fusion obsolete Warning 정리

`ProjectJNetworkConnectionRecovery.cs`의 `OnUserSimulationMessage`는 `INetworkRunnerCallbacks` 구현을 위해 유지해야 하지만, Fusion의 `SimulationMessagePtr`가 obsolete 처리되어 CS0618 Warning이 발생했다.

콜백 자체는 유지하고 해당 메서드 구간에만 경고 억제를 적용했다.

```text
#pragma warning disable CS0618
#pragma warning restore CS0618
```

프로젝트 전체의 obsolete Warning을 끄지 않고 필요한 부분에만 제한적으로 적용했다.

### 7. 사용되지 않는 Lobby 변수 제거

`ProjectJNetworkLobbyFlow.cs`의 `returnToLobbyRequested`는 값만 대입되고 실제 조건 판단에는 사용되지 않아 CS0414 Warning이 발생했다.

Lobby 복귀는 기존의 `lobbyLoadRequested`, `ReturningToLobby` Phase와 NetworkRunner Scene Load가 담당하므로 사용되지 않는 변수와 관련 대입문을 제거했다.

## 변경 파일

### 생성

```text
Assets/ProjectJ/Network/Fusion/Session/ProjectJDay94SceneFlowGuard.cs
Assets/ProjectJ/Network/Fusion/Session/ProjectJDay94SceneFlowGuard.cs.meta
```

### 수정

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/ProjectJFusionBootstrapRuntimeInstaller.cs
Assets/ProjectJ/Network/Fusion/Session/ProjectJNetworkConnectionRecovery.cs
Assets/ProjectJ/Network/Fusion/Session/ProjectJNetworkLobbyFlow.cs
```

### 삭제

없음.

## 테스트 항목

94일차의 최종 테스트 흐름은 다음과 같다.

```text
Bootstrap
→ MainMenu HOME
→ PLAY
→ Private Match
→ Host / Join
→ Lobby
→ Ready
→ Game
→ Countdown
→ Playing
→ Finished
→ Result
→ Lobby
```

추가로 다음 흐름을 확인한다.

```text
Lobby
→ Leave
→ MainMenu
```

Scene 전환 중 버튼을 연속 클릭했을 때 중복 요청이 발생하지 않는지 확인한다.

Lobby → Game → Lobby 흐름을 두 번 이상 반복해 NetworkRunner, Network Player, Camera, AudioListener가 중복 생성되지 않는지 확인한다.

MainMenu 복귀 후 Session이 종료된 상태에서는 Network Player가 남아 있지 않아야 한다.

## 검증 기준

다음 조건을 모두 만족하면 94일차 완료로 판단한다.

- 실제 UI만 사용해 Bootstrap → MainMenu → Lobby → Game → Result → Lobby 흐름 진행 가능
- Lobby → MainMenu 복귀 정상
- Scene 전환 중 UI 중복 입력 차단
- NetworkRunner 중복 없음
- Network Player 비정상 잔존 없음
- Main Camera 중복 없음
- AudioListener 중복 없음
- CS0618 Warning 정리
- CS0414 Warning 정리
- Missing Reference 없음
- Console Error 0건

## 현재 GitHub 검토

검토 기준 최신 커밋:

```text
eaa59cf4d179500be352836ffcebb5b41b450a04
```

현재 커밋 메시지는 임시값 `a`이다.

커밋 변경 내용에는 Day94 Scene Flow Guard, Bootstrap 자동 설치, Fusion obsolete Warning 정리, 사용되지 않는 Lobby 변수 제거가 포함되어 있으며 정적 코드 검토에서 94일차 진행을 막는 구조적인 문제는 확인되지 않았다.

GitHub에 연결된 자동 CI 상태 검사는 없으므로 실제 PlayMode 및 Host/Client 실행 결과는 Unity에서 최종 확인한다.

## 결과

94일차에서는 기존 Scene Flow를 새로 재작성하지 않고 이미 구현된 Host/Join, Lobby Ready, Game 진입, Result, Lobby/MainMenu 복귀 흐름을 유지하면서 Scene 전환 안정성을 보강했다.

특히 전환 중 UI 중복 입력 차단과 Runtime 객체 Audit을 추가하여 이후 95일차의 Host 1명 + Client 1명 전체 Flow 실전 검증을 진행할 수 있는 기준을 마련했다.
