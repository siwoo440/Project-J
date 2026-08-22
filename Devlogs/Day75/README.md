# Project J - 75일차 개발일지

## 1. 개발 목표

Phase 6에서 구현한 네트워크 기능들을 실제 멀티플레이 환경에서 통합 점검할 수 있도록 테스트 흐름을 정리한다.

이번 일차의 핵심 목표는 다음과 같다.

- 방 생성·참가 후 `Game` 씬 대신 `Day49_AllSystemsTest` 씬으로 바로 이동
- `Day49_AllSystemsTest`를 Fusion 경기 씬으로 사용할 수 있도록 연결
- Phase 6 네트워크 상태를 한 화면에서 확인할 수 있는 Gate 디버그 UI 추가
- Rigidbody를 사용하지 않는 Network Player가 이동·회전 플랫폼의 움직임을 따라가도록 보완
- Phase 6 통합 테스트용 Build Settings 구성을 자동화

---

## 2. 구현 내용

### 2.1 Day49_AllSystemsTest 직접 진입

기존 74일차의 Lobby → MatchLoading → Game 흐름을 유지하면서,
Phase 6 통합 테스트 중에는 Lobby를 건너뛰고 `Day49_AllSystemsTest` 씬으로 직접 진입하도록 테스트 모드를 추가했다.

현재 테스트 흐름은 다음과 같다.

```text
Host 방 생성
↓
Photon Fusion Session 생성
↓
Host Scene Authority가 Day49_AllSystemsTest 로드
↓
Client 방 코드로 참가
↓
Fusion Scene 동기화
↓
Network Player 준비
↓
Countdown
↓
멀티플레이 통합 테스트
```

`ProjectJNetworkLobbyFlow`에서 `DirectGameTestMode`를 사용하며,
Scene Authority를 가진 Host만 실제 씬 로드를 요청한다.

Client는 각자 Unity `SceneManager.LoadScene`을 호출하지 않고
Fusion의 Scene 동기화를 따라가도록 유지했다.

---

### 2.2 Day49_AllSystemsTest Build Settings 자동 등록

`ProjectJDay49BuildSceneInstaller`를 추가했다.

Unity Editor에서 스크립트 컴파일 후 프로젝트 내에서
`Day49_AllSystemsTest` 씬을 자동 검색한다.

Build Settings에 씬이 없는 경우 자동으로 추가하고,
등록되어 있으나 비활성 상태라면 활성화한다.

대상 씬은 현재 다음 경로에 있다.

```text
Assets/ProjectJ/Tests/Manual/Day49/Day49_AllSystemsTest.unity
```

이를 통해 테스트 빌드를 만들기 전에
Build Settings에 테스트 씬을 수동으로 추가하는 과정을 줄였다.

---

### 2.3 Day49 테스트 씬을 경기 씬으로 인식

기존 `ProjectJNetworkExternalGameplay`는
`Game` 씬에서만 경기 로직을 허용했다.

이번 일차에는 다음 두 씬을 경기용 씬으로 인정하도록 수정했다.

```text
Game
Day49_AllSystemsTest
```

따라서 `Day49_AllSystemsTest`로 이동한 후에도 다음 기능을 계속 사용할 수 있다.

- Game Player 준비
- 3초 Countdown
- 이동
- 점프
- 달리기
- 앉기
- Push
- Checkpoint
- Respawn
- 아이템
- 경기 상태
- 결과 처리

---

### 2.4 Phase 6 Gate 디버그 화면 추가

`ProjectJPhase6GateDebugView`를 추가했다.

Editor 또는 Development Build에서 `F3` 키를 사용해
Phase 6 통합 상태를 확인할 수 있다.

현재 확인할 수 있는 주요 항목은 다음과 같다.

- NetworkRunner 개수
- 참가 인원과 PlayerObject 개수
- Local Input Authority Player 개수
- Spawned Player 수
- State Authority Player 수
- Lobby Flow 상태
- Match 상태
- Network Dynamic Platform 개수
- 플랫폼 승객 이동 누적 횟수
- 현재 플랫폼 승객 이동 인원

최종 Phase 6 Gate 확인 기준은 다음과 같이 표시한다.

```text
Console Error 0 + 2PC 전체 경기
```

`ProjectJFusionBootstrapRuntimeInstaller`에서
Bootstrap 오브젝트에 Gate 디버그 컴포넌트가 자동 설치되도록 연결했다.

---

### 2.5 Network Dynamic Platform 승객 이동 보완

기존 이동 플랫폼은 Rigidbody 기반 승객 이동을 중심으로 구현되어 있었기 때문에,
현재 Fusion Network Player처럼 Rigidbody 없이 직접 위치를 계산하는 캐릭터는
플랫폼 위에서 제대로 따라가지 못할 수 있었다.

이번 일차에는 `ProjectJNetworkDynamicPlatform`이
State Authority에서 플랫폼 위 Network Player를 직접 탐색하도록 보완했다.

주요 처리 흐름은 다음과 같다.

```text
플랫폼 이전 위치·회전 저장
↓
현재 위치·회전 계산
↓
플랫폼 상단의 Network Player 탐색
↓
플랫폼 위치·회전 변화량 계산
↓
승객의 목표 위치 계산
↓
Host State Authority에서 Player 위치 적용
↓
NetworkTransform을 통해 다른 Client에 동기화
```

승객 탐색에는 `Physics.OverlapBoxNonAlloc`을 사용해
매 Tick 불필요한 배열 생성을 줄였다.

또한 동일 Player가 여러 Collider로 탐지되어도
한 Tick에 중복 이동되지 않도록 처리했다.

---

### 2.6 Network Player 플랫폼 이동 API 추가

`ProjectJNetworkPlayer`에
State Authority 전용 플랫폼 승객 위치 적용 함수를 추가했다.

다음 조건을 만족할 때만 플랫폼의 이동량을 적용한다.

- NetworkObject가 유효함
- State Authority를 가지고 있음
- 경기 입력이 허용된 상태
- Player가 Grounded 상태

점프 또는 낙하 중인 Player는
플랫폼에 강제로 끌려가지 않도록 했다.

---

## 3. 변경 파일

### 신규 파일

```text
Assets/ProjectJ/Editor/
├─ ProjectJDay49BuildSceneInstaller.cs
└─ ProjectJDay49BuildSceneInstaller.cs.meta

Assets/ProjectJ/Network/Fusion/Bootstrap/
├─ ProjectJPhase6GateDebugView.cs
└─ ProjectJPhase6GateDebugView.cs.meta
```

### 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
└─ ProjectJFusionBootstrapRuntimeInstaller.cs

Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkExternalGameplay.cs
└─ ProjectJNetworkPlayer.cs

Assets/ProjectJ/Network/Fusion/Session/
└─ ProjectJNetworkLobbyFlow.cs

Assets/ProjectJ/Network/Fusion/World/
└─ ProjectJNetworkDynamicPlatform.cs

ProjectSettings/
└─ EditorBuildSettings.asset
```

---

## 4. 75일차 테스트 항목

### Scene 진입

- Host가 방을 생성할 수 있는지 확인
- Client가 6자리 방 코드로 참가할 수 있는지 확인
- 두 PC 모두 `Day49_AllSystemsTest`로 이동하는지 확인
- NetworkRunner가 하나만 유지되는지 확인

### Player

- 참가 인원 수와 PlayerObject 수가 일치하는지 확인
- 각 PC에서 Local Input Authority가 정확히 한 명인지 확인
- 두 Player가 서로 정상적으로 보이는지 확인
- 이동·점프·달리기·앉기가 동기화되는지 확인

### 경기

- Player 준비 후 Countdown이 시작되는지 확인
- Countdown 전에는 경기 입력이 잠기는지 확인
- Countdown 종료 후 입력이 허용되는지 확인
- Push가 Host 권한 기준으로 처리되는지 확인
- Checkpoint와 Respawn이 동기화되는지 확인
- 3초 Respawn Protection이 적용되는지 확인
- 높이와 순위가 양쪽에서 동일한지 확인

### 아이템

- Item Box 획득이 동기화되는지 확인
- 2슬롯 Inventory가 동기화되는지 확인
- 대표 아이템 5종의 사용과 소비가 정상인지 확인

### Dynamic Platform

- Moving Platform 위에서 Player가 플랫폼을 따라가는지 확인
- Rotating Platform에서 회전에 맞춰 위치가 이동하는지 확인
- 점프 시 플랫폼 강제 이동이 해제되는지 확인
- F3 Gate 화면의 Passenger Carry 값이 증가하는지 확인

### 종료

- FINISH 도달 순서가 동기화되는지 확인
- 제한 시간 종료 시 최종 순위가 동일한지 확인
- 경기 종료 후 결과 상태가 양쪽에서 동일한지 확인
- Console Error가 발생하지 않는지 확인

---

## 5. 검토 결과

GitHub 최신 커밋 기준으로 변경 내용을 정적 검토했다.

현재 확인된 범위에서는 즉시 수정이 필요한 구조적 또는 명백한 코드 오류는 발견하지 못했다.

특히 다음 연결은 정상적으로 구성되어 있다.

- `Day49_AllSystemsTest` Build Settings 등록
- Fusion Scene Authority 기반 테스트 씬 로드
- `Day49_AllSystemsTest` 경기 씬 판정
- Phase 6 Gate 자동 설치
- Network Player 플랫폼 승객 이동 처리

다만 현재 저장소에는 해당 커밋을 자동으로 Unity 컴파일하거나
2PC 실행 테스트하는 GitHub CI 상태 검사가 등록되어 있지 않다.

따라서 최종 완료 판정은 실제 Windows Host + Client 환경에서
위 테스트 항목을 모두 확인하는 방식으로 진행한다.

---

## 6. 75일차 완료 기준

다음 조건을 만족하면 Phase 6 통합 Gate를 완료한 것으로 본다.

- Host와 Client가 동일한 `Day49_AllSystemsTest`에 진입
- NetworkRunner 중복 없음
- PlayerObject 수 정상
- Local Input Authority 정상
- 이동·점프·Sprint·Crouch 정상
- Push 정상
- Checkpoint·Respawn·Protection 정상
- 높이·순위 동기화 정상
- Dynamic Platform 승객 이동 정상
- Item Box·Inventory·대표 아이템 5종 정상
- Countdown·Timer·FINISH·Result 정상
- Console Error 0
- 2PC 전체 경기 1회 이상 정상 완료

---

## 7. 다음 개발 방향

75일차 Phase 6 통합 Gate를 실제 2PC 환경에서 통과한 뒤
다음 Phase 개발로 진행한다.

통합 테스트 중 발견되는 문제는 새로운 기능 추가보다
Phase 6의 네트워크 권한·동기화·Scene 전환 문제를 우선 수정한다.
