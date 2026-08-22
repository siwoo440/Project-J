# Project J - 76일차 개발일지

## 1. 개발 목표

Phase 7의 4인·8인 멀티플레이 테스트를 진행하기 전에
한 PC에서 여러 게임 창을 동시에 실행하고,
실제 `Game` Scene에서 Host가 원하는 시점에 경기를 시작할 수 있는
멀티플레이 테스트 환경을 구축한다.

이번 일차의 핵심 목표는 다음과 같다.

- 한 PC에서 Unity Editor Host와 여러 Windows Build Client 동시 실행
- 비활성 게임 창도 계속 네트워크 Simulation 유지
- 실제 `Assets/ProjectJ/Scenes/Game.unity`를 멀티플레이 경기 Scene으로 사용
- 최대 8명의 Player 시작 위치 구성
- Host에게만 `GAME START` 버튼 제공
- Client 참가만으로 경기가 자동 시작되지 않도록 변경
- Host 시작 버튼 입력 후 Player 초기화 → Spawn 배치 → 3초 Countdown → Playing 진행
- 잘못 생성했던 Day76 테스트 Scene 대신 실제 Game Scene을 사용하도록 수정

---

## 2. 구현 내용

### 2.1 한 PC 다중 창 테스트 환경

Windows Player 설정을 멀티플레이 로컬 테스트에 맞게 조정했다.

현재 테스트 기준은 다음과 같다.

```text
Window Mode
→ Windowed

Default Resolution
→ 960 × 540

Run In Background
→ ON

Force Single Instance
→ OFF

Resizable Window
→ ON
```

이를 통해 한 PC에서 다음과 같은 형태로 여러 Player를 동시에 실행할 수 있다.

```text
Unity Editor
└─ Host

ProjectJ.exe
├─ Client 1
├─ Client 2
└─ Client 3
```

76일차에서는 2~4개 인스턴스의 접속과 시작 흐름을 우선 확인하고,
다음 일차부터 실제 4인·8인 전체 경기 검증을 진행한다.

---

### 2.2 Day76 Runtime Test Flow 추가

`ProjectJDay76TestFlow`를 추가했다.

이 Flow는 기존 75일차의 임시 `Day49_AllSystemsTest` 직접 진입 Flow를
76일차 테스트 중 비활성화하고,
실제 경기 Scene을 직접 사용하도록 한다.

현재 대상 Scene은 다음과 같다.

```text
Assets/ProjectJ/Scenes/Game.unity
```

Host의 Scene Authority가 실제 `Game` Scene 로드를 요청하고,
Client는 Photon Fusion Scene 동기화를 통해 동일한 Scene으로 이동한다.

```text
Host 방 생성
↓
Fusion Session 생성
↓
실제 Game Scene 로드
↓
Client 방 코드 참가
↓
Fusion Scene 동기화
↓
Host GAME START 대기
```

---

### 2.3 기존 테스트 Scene 진입 문제 수정

76일차 초기 구현에서는 다음 Scene을 사용했다.

```text
Assets/ProjectJ/Tests/Manual/Day76/Game.unity
```

해당 Scene은 `Day49_AllSystemsTest`를 복사해 만든 테스트 맵이었기 때문에,
파일 이름은 `Game`이지만 실제 게임의 `Game` Scene과 다른 맵이 표시되는 문제가 있었다.

이를 수정해 최종 76일차에서는 다음 실제 Scene만 사용한다.

```text
Assets/ProjectJ/Scenes/Game.unity
```

Editor 설치 도구도 더 이상 Day49 테스트 Scene을 복사하지 않는다.

이전에 잘못 생성된 Day76 테스트 Scene이 존재하는 경우 제거하고,
Build Settings에서도 해당 테스트 Scene 등록을 제외하도록 정리했다.

---

## 3. Player Spawn Point

### 3.1 최대 8개 Spawn Point

실제 `Game` Scene에 다음 테스트 구조를 추가했다.

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

각 Spawn Point에는 `ProjectJNetworkSpawnPoint`가 연결되어 있으며
0~7의 Slot Index를 가진다.

현재 기본 위치는 기존 네트워크 Game Scene 준비 규칙과 동일한 형태를 사용한다.

```text
Spawn_00 = (0, 2, 4)
Spawn_01 = (3, 2, 4)
Spawn_02 = (6, 2, 4)
Spawn_03 = (9, 2, 4)
Spawn_04 = (12, 2, 4)
Spawn_05 = (15, 2, 4)
Spawn_06 = (18, 2, 4)
Spawn_07 = (21, 2, 4)
```

향후 실제 맵의 출발 지점에 맞추고 싶을 경우
코드를 수정하지 않고 Scene의 `Spawn_00 ~ Spawn_07` Transform만 이동하면 된다.

---

### 3.2 Player Slot 배정

Host는 현재 Session의 Active Player 목록을
Player Index 순서로 정렬한 뒤 Spawn Slot을 배정한다.

```text
첫 번째 Player
→ Spawn_00

두 번째 Player
→ Spawn_01

세 번째 Player
→ Spawn_02

...

여덟 번째 Player
→ Spawn_07
```

동일한 Player가 여러 위치에 배정되지 않도록
한 Player당 하나의 Spawn Point만 사용한다.

---

## 4. Host 전용 GAME START

### 4.1 자동 경기 시작 제거

75일차까지의 임시 통합 Flow에서는
Player 준비가 완료되면 Countdown으로 자동 진행하는 구조가 있었다.

76일차에서는 여러 Client 창을 모두 실행한 후
Host가 원하는 시점에 경기를 시작할 수 있어야 하므로
자동 시작을 사용하지 않는다.

현재 흐름은 다음과 같다.

```text
Host 방 생성
↓
Game Scene 진입
↓
Client 참가
↓
Player 대기
↓
Host GAME START
↓
Player 초기화
↓
Spawn Point 재배치
↓
3초 Countdown
↓
Playing
```

---

### 4.2 Host UI

실제 Game Scene에 접속하면 테스트용 Runtime UI가 표시된다.

주요 표시 정보는 다음과 같다.

```text
DAY 76 - GAME SCENE MULTIPLAYER TEST

ROOM : 방 코드
PLAYERS : 현재 인원 / 8
MATCH : 현재 경기 상태
현재 Flow 상태
```

Host에게는 조건이 충족되면 다음 버튼이 표시된다.

```text
[ GAME START ]
```

Client에서는 직접 시작할 수 없으며 다음과 같이 Host를 기다린다.

```text
Waiting for Host...
```

---

### 4.3 경기 시작 최소 인원

현재 기존 네트워크 경기 규칙에 맞춰
최소 2명의 Player가 존재해야 GAME START가 가능하다.

```text
Host 1명
→ 시작 불가

Host + Client
→ 시작 가능
```

최대 테스트 인원은 8명이다.

---

## 5. GAME START 처리

Host가 `GAME START`를 누르면
모든 참가 Player의 PlayerObject와 Spawn Point 상태를 확인한다.

정상적으로 준비되면 기존 `PrepareForGameSceneAuthority()`를 재사용해
각 Player를 경기 시작 상태로 초기화한다.

주요 초기화 대상은 다음과 같다.

- 외부 이동 힘
- Push Cooldown
- Respawn Protection
- Checkpoint
- Respawn 위치
- Lobby Ready
- Inventory
- Player 이동 상태
- 현재 높이
- 최고 높이

이후 모든 Player를 각자의 Spawn Point로 Network Teleport한다.

---

## 6. 기존 Network Countdown 재사용

Player 준비가 끝나면
기존 `TryBeginCountdownFromLobbyFlowAuthority()`를 통해
Network Match Countdown을 시작한다.

```text
GAME START
↓
Preparing
↓
3초 Countdown
↓
Playing
↓
10분 Match Timer
```

따라서 별도의 로컬 Countdown을 만들지 않고
Host State Authority가 확정한 기존 네트워크 경기 상태를 그대로 사용한다.

경기 시작이 확정되면 Session을 닫아
경기 도중 새로운 Player가 늦게 참가하지 못하도록 한다.

---

## 7. 기존 Lobby Flow 충돌 방지

75일차의 `ProjectJNetworkLobbyFlow`에는
Phase 6 테스트를 위해 `Day49_AllSystemsTest`로 직접 이동하는 임시 Flow가 남아 있다.

76일차에서는 `ProjectJDay76TestFlow`가 활성화되면
해당 기존 Lobby Flow를 비활성화한다.

이를 통해 다음 두 Scene Load 요청이 동시에 발생하는 것을 방지한다.

```text
75일차 Flow
→ Day49_AllSystemsTest

76일차 Flow
→ 실제 Game
```

76일차 테스트 중에는 실제 Game Scene Flow만 사용한다.

---

## 8. Build Settings 정리

`ProjectJDay76TestSceneInstaller`는
실제 Game Scene이 Build Settings에 등록되어 있는지 확인한다.

대상 Scene:

```text
Assets/ProjectJ/Scenes/Game.unity
```

등록되어 있지 않다면 추가하고,
비활성 상태라면 활성화한다.

잘못 등록된 이전 테스트 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day76/Game.unity
```

은 Build Settings 대상에서 제외한다.

---

## 9. 변경 파일

### 신규 파일

```text
Assets/ProjectJ/Editor/
├─ ProjectJDay76TestSceneInstaller.cs
└─ ProjectJDay76TestSceneInstaller.cs.meta

Assets/ProjectJ/Network/Fusion/Test/
├─ ProjectJDay76RuntimeInstaller.cs
├─ ProjectJDay76RuntimeInstaller.cs.meta
├─ ProjectJDay76TestFlow.cs
├─ ProjectJDay76TestFlow.cs.meta
├─ ProjectJNetworkSpawnPoint.cs
└─ ProjectJNetworkSpawnPoint.cs.meta
```

### 수정 파일

```text
Assets/ProjectJ/Scenes/
└─ Game.unity

ProjectSettings/
└─ ProjectSettings.asset
```

---

## 10. 테스트 방법

### 10.1 기본 접속

```text
Unity Editor
→ Host

Windows Build 1
→ Client 1
```

같은 방 코드로 접속한다.

확인 사항:

- Host와 Client 모두 실제 `Game` Scene 진입
- Day49 테스트 맵으로 이동하지 않음
- 현재 참가 인원이 정상 표시됨
- Host에게 GAME START 버튼 표시
- Client에는 GAME START 버튼이 표시되지 않음

---

### 10.2 경기 시작

Host에서 `GAME START`를 누른다.

확인 사항:

- 각 Player가 서로 다른 Spawn Point로 이동
- 3초 Countdown 실행
- Countdown 전 이동 제한
- Countdown 종료 후 동시에 조작 가능
- Match Timer 시작

---

### 10.3 다중 창

Windows Build를 추가로 실행한다.

```text
Editor Host
Client 1
Client 2
Client 3
```

확인 사항:

- 같은 PC에서 여러 Build 동시 실행
- 다른 창을 선택해도 비활성 Client 연결 유지
- Player 수 정상 표시
- 각 PlayerObject가 중복 생성되지 않음
- 각 Player가 다른 Spawn Point 사용

---

## 11. 76일차 완료 기준

다음 조건을 만족하면 76일차를 완료한 것으로 본다.

- 실제 `Assets/ProjectJ/Scenes/Game.unity` 사용
- Day49 또는 Day76 복사 테스트 Scene으로 이동하지 않음
- Game Scene에 Spawn Point 8개 구성
- 한 PC에서 여러 Windows Build 실행 가능
- Run In Background 활성화
- 최대 8인 Spawn Slot 구조 준비
- Host에게만 GAME START 버튼 표시
- Client 참가만으로 자동 시작되지 않음
- 최소 2인부터 Host가 경기 시작 가능
- GAME START 후 모든 Player 초기화
- Player별 Spawn Point 배치
- 기존 3초 Network Countdown 정상 실행
- Countdown 종료 후 Playing 진입
- 경기 시작 후 Session 신규 참가 차단
- 기존 네트워크 Player·Checkpoint·Inventory·Rank 시스템과 연결 유지
- Console Error 없음

---

## 12. 검토 결과

최신 GitHub 커밋 기준으로 75일차와 76일차 변경을 비교해 정적 검토했다.

현재 확인된 범위에서는 즉시 수정해야 할 명백한 구조 오류는 발견하지 못했다.

특히 다음 항목은 의도한 구조로 반영되어 있다.

- 실제 `Assets/ProjectJ/Scenes/Game.unity`를 Runtime 이동 대상으로 사용
- 실제 Game Scene에 Day76 Spawn Point 구조 추가
- Host 전용 GAME START Flow
- 기존 Match 준비·Countdown API 재사용
- 다중 창 실행용 Player Settings 적용
- 기존 Day49 직접 진입 Flow 비활성화
- 이전 잘못된 Day76 복사 테스트 Scene 사용 제거

다만 현재 저장소에는 Unity Compile 또는 Windows 멀티 인스턴스 실행을
자동 검증하는 CI 상태 검사가 등록되어 있지 않다.

따라서 최종 완료 여부는 Unity Console과 실제 Host + Client 실행을 통해 확인한다.

---

## 13. 다음 개발 방향

77일차에는 이번에 만든 다중 창 환경을 사용해
실제 4인 전체 경기를 검증한다.

중점 확인 대상은 다음과 같다.

- 4인 Player Spawn
- 4인 이동·점프·Sprint·Crouch
- 여러 Player 동시 Push
- Checkpoint·Respawn
- 실시간 높이와 순위
- Dynamic Platform
- Item Box와 Inventory
- 대표 아이템
- FINISH 순서
- 경기 종료 결과
- Host 부하와 Network 안정성

76일차 환경 구축 이후에는 새로운 테스트 맵을 추가하기보다
실제 Game Scene을 기준으로 멀티플레이 동작을 계속 검증한다.
