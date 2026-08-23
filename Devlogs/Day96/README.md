# Project J - 96일차 개발일지

## 오늘의 목표

PHASE 9의 시작으로 기존 Fusion Host Mode와 분리된 Server Mode 실행 기반을 만든다.

96일차에서는 아직 실제 Dedicated Build나 전체 경기 권한 이전까지 진행하지 않고, 다음 조건을 만족하는 Server Mode Bootstrap 기준선을 만드는 것을 목표로 한다.

```text
Fusion GameMode.Server 실행
→ Local Input 없음
→ Server 자체 Player 없음
→ Client 접속 대기
→ 접속 Client Player만 Spawn
→ Client 종료 후 Server 유지
```

---

## 작업 내용

### 1. Fusion Server Mode 전용 Bootstrap 추가

기존 `ProjectJFusionBootstrap`은 Host / Client 비공개 방 흐름에 계속 사용하고, Dedicated Server 테스트용으로 별도의 `ProjectJDay96ServerModeBootstrap`을 추가했다.

Server Runner는 다음 기준으로 시작하도록 구성했다.

```text
GameMode = Server
ProvideInput = false
Maximum Players = 8
Room Code = 960001
```

서버는 자신의 입력을 제공하지 않으며 `ProjectJFusionInputProvider`도 생성하지 않는다.

---

### 2. Client 전용 Network Player Spawn 구조 유지

기존 `ProjectJNetworkPlayerSpawner`를 Server Runner에도 연결했다.

Spawner는 `Runner.IsServer`인 Runner에서 접속한 `PlayerRef`에 대해서만 Network Player를 생성한다.

따라서 Server Mode 자체에는 Local Player가 존재하지 않고, 실제 Client가 접속했을 때 해당 Client의 Network Player만 생성되는 구조를 사용한다.

---

### 3. 일반 Host / Client Bootstrap 자동 설치 차단

`ProjectJFusionBootstrapRuntimeInstaller`를 수정했다.

`ProjectJDay96ServerModeBootstrap`이 존재하는 Scene에서는 기존 Host / Client용 Bootstrap과 Lobby / Scene Flow 관련 Runtime Component를 자동 생성하지 않도록 했다.

이를 통해 Server Mode 테스트 Scene에서 다음 요소가 불필요하게 생성되는 것을 방지한다.

```text
일반 ProjectJFusionBootstrap
Host / Client Lobby Flow
일반 Scene Flow
Host / Client Debug Bootstrap
```

---

### 4. Server Mode 전용 테스트 Scene 구성

96일차 전용 Scene을 추가했다.

```text
Assets/ProjectJ/Scenes/Day96_ServerModeTest.unity
```

Scene의 핵심 구성은 다음과 같다.

```text
Day96_ServerModeTest
└─ === Day96 Server Mode ===
   └─ ProjectJDay96ServerModeBootstrap
```

Camera, Canvas, Local Input용 오브젝트를 기본 구성에 포함하지 않는다.

---

### 5. Editor 설치 메뉴 추가

Server Mode 테스트 Scene을 다시 구성할 수 있도록 Editor 메뉴를 추가했다.

```text
Project J
→ Scene
→ 96일차 Server Mode Test Scene 구성
```

실행하면 `Day96_ServerModeTest.unity`를 새로 만들고 `ProjectJDay96ServerModeBootstrap`을 배치한다.

---

## 주요 변경 파일

```text
Assets/ProjectJ/Editor/
├─ ProjectJDay96ServerModeInstaller.cs
└─ ProjectJDay96ServerModeInstaller.cs.meta

Assets/ProjectJ/Network/Fusion/Bootstrap/
├─ ProjectJDay96ServerModeBootstrap.cs
├─ ProjectJDay96ServerModeBootstrap.cs.meta
└─ ProjectJFusionBootstrapRuntimeInstaller.cs

Assets/ProjectJ/Scenes/
├─ Day96_ServerModeTest.unity
└─ Day96_ServerModeTest.unity.meta
```

---

## Server Mode 기준

Server Runner가 정상 실행되면 다음 상태를 목표로 한다.

```text
GameMode       : Server
ProvideInput   : False
InputProvider  : 없음
Local Player   : 없음
Participant    : Client 접속 전 0
Spawned Player : Client 접속 전 0
```

Client가 Room Code `960001`로 접속하면 Server에서 Client Player가 생성되는 구조다.

---

## 검토 결과

최신 GitHub 커밋의 변경 내용을 기준으로 다음을 확인했다.

- Server Mode Bootstrap이 기존 Host / Client Bootstrap과 분리되어 있음
- `GameMode.Server`를 사용하도록 구성되어 있음
- Server의 `ProvideInput`이 `false`로 설정되어 있음
- Server Runner에 `ProjectJFusionInputProvider`를 추가하지 않음
- 기존 `ProjectJNetworkPlayerSpawner`를 재사용해 접속 Client Player를 생성함
- Server Test Scene에서는 일반 Host / Client Runtime Bootstrap 자동 설치를 차단함
- Server Test Scene과 Script GUID 연결이 정상적으로 커밋되어 있음
- 현재 커밋에 등록된 CI / 자동 빌드 검증 결과는 없음

소스 변경 내용에서 즉시 확인되는 명백한 구조 문제는 발견하지 못했다.

다만 GitHub 커밋만으로 Unity PlayMode의 실제 Server 실행 성공, Client 접속 성공, Client 종료 후 Server 유지 여부까지 증명할 수는 없으므로 해당 항목은 실제 실행 테스트 결과를 기준으로 최종 판단한다.

---

## 최신 커밋

```text
SHA
9e33eb7090068ebdd71eda20091935081c9d63ad

현재 커밋 메시지
96
```

---

## 96일차 결과

96일차에서는 PHASE 9 Dedicated Server 전환의 첫 단계로 Fusion Server Mode 전용 Bootstrap과 독립 테스트 Scene을 구성했다.

기존 Host / Client 흐름을 유지하면서 Server 전용 Runner가 별도 경로로 시작될 수 있는 기준선을 마련했으며, 다음 97일차에서는 이 구조를 실제 Headless Dedicated Build Profile로 확장한다.
