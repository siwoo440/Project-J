# Project J - 59일차 개발 일지

## 개발 목표

기존 59일차의 Photon Fusion 비공개 Session 생성·참가 구조와 이후 진행한 60일차의 6자리 방 코드 시스템을 하나의 일차로 통합한다.

이번 59일차에서는 58일차에서 구축한 `NetworkRunner` Bootstrap 위에 실제 비공개 방 생성·참가·이탈 흐름을 만들고, 사용자가 긴 Session 이름 대신 짧은 방 코드만으로 같은 방에 참가할 수 있도록 정리한다.

최종 목표는 다음과 같다.

```text
비공개 Session 생성
↓
6자리 방 코드 자동 생성
↓
Host가 방 코드 공유
↓
Client가 방 코드 입력
↓
같은 Fusion Session 참가
↓
참가 인원 확인
↓
Client 이탈
↓
Host Session 유지
↓
Client 재참가
```

---

## 주요 개발 내용

### 1. 비공개 Fusion Session 생성 구조 구현

58일차에서 만든 `ProjectJFusionBootstrap`을 확장해 Host가 비공개 Session을 생성할 수 있도록 했다.

Host 생성 설정:

```text
GameMode.Host
IsVisible = false
IsOpen = true
PlayerCount = 8
```

각 설정의 의미:

```text
IsVisible = false
→ 공개 Session 목록에 노출하지 않음

IsOpen = true
→ Session 이름을 알고 있는 Client는 참가 가능

PlayerCount = 8
→ 최대 8명까지 참가 가능
```

이를 통해 Project J의 비공개 멀티플레이 방 기반을 구축했다.

---

### 2. Client 비공개 방 참가 구조 구현

Client가 이미 생성된 Host Session에 참가할 수 있도록 `RequestJoinPrivateRoom()` 흐름을 추가했다.

Client에서는 다음 옵션을 사용한다.

```text
GameMode.Client
EnableClientSessionCreation = false
```

따라서 Client가 존재하지 않는 Session을 입력해도 새로운 방을 자동 생성하지 않는다.

동작:

```text
Client 참가 요청
↓
기존 Session 검색
↓
존재
→ 참가

존재하지 않음
→ 참가 실패
→ 새 Session 생성 안 함
```

---

### 3. 방 나가기 및 재접속 구조 구현

기존 Runner 종료 기능을 비공개 방 흐름에 맞춰 `RequestLeaveRoom()`으로 정리했다.

```text
방 나가기
↓
NetworkRunner.Shutdown()
↓
Runner 오브젝트 제거
↓
State = Idle
↓
다시 방 생성 / 참가 가능
```

기존 호환 메서드도 유지했다.

```text
RequestStartHost()
RequestStartClient()
RequestShutdown()
```

---

### 4. 참가 인원 표시 추가

현재 `NetworkRunner.ActivePlayers`를 기준으로 실제 참가 인원을 계산한다.

예:

```text
Host만 접속
→ 1 / 8

Host + Client
→ 2 / 8
```

이를 F2 네트워크 디버그 창에서 바로 확인할 수 있도록 했다.

---

### 5. Session 상태 정보 확장

현재 연결 상태를 확인하기 위해 `ProjectJFusionBootstrap`에서 다음 정보를 노출하도록 했다.

```text
ParticipantCount
ConnectedSessionName
ConnectedRegion
IsSessionVisible
IsSessionOpen
LastConnectionResult
```

이 정보는 이후 Lobby와 Player 관리 시스템에서도 활용할 수 있는 기초 데이터가 된다.

---

### 6. Session 이름 검증 구조 추가

초기 59일차 작업에서는 직접 Session 이름을 입력하는 구조를 사용했기 때문에 `ProjectJFusionSessionNameValidator`를 추가했다.

검증 규칙:

```text
최소 3자
최대 24자

허용:
영문
숫자
-
_
```

이후 방 코드 시스템으로 사용자 입력 방식은 변경되었지만, Session 입력 검증용 구조는 네트워크 유틸리티로 유지한다.

---

### 7. NetworkRunner Root 구조 정리

Fusion의 `NetworkRunner`는 Runtime에서 Root GameObject로 생성하도록 수정했다.

Hierarchy 예:

```text
=== Project J Fusion Bootstrap ===

=== Fusion NetworkRunner ===
```

Runner를 Bootstrap의 자식으로 넣지 않도록 해 `DontDestroyOnLoad` 관련 경고가 발생할 수 있는 구조를 제거했다.

---

### 8. 현재 Scene 재등록 제거

Session 연결만 검증하는 단계에서는 현재 Scene을 Fusion Scene으로 다시 등록할 필요가 없으므로 기존 `NetworkSceneInfo` 재등록 코드를 제거했다.

기존:

```text
현재 Scene 검색
↓
NetworkSceneInfo 생성
↓
LoadSceneMode.Single
↓
StartGame()
```

수정:

```text
NetworkRunner 생성
↓
NetworkSceneManagerDefault 추가
↓
Session 설정
↓
StartGame()
```

이를 통해 비공개 Session 생성 단계에서 불필요한 Scene 로딩 흐름을 제거했다.

---

## 6자리 방 코드 시스템

### 9. ProjectJFusionRoomCode 추가

사용자가 긴 Fusion Session 이름을 직접 입력하지 않아도 되도록 방 코드 전용 유틸리티를 추가했다.

신규 파일:

```text
Assets/ProjectJ/Network/Fusion/Session/
└─ ProjectJFusionRoomCode.cs
```

방 코드는 정확히 6자리로 구성한다.

사용 문자:

```text
ABCDEFGHJKLMNPQRSTUVWXYZ
23456789
```

헷갈리기 쉬운 문자는 제외했다.

```text
I
O
0
1
```

예:

```text
AB4K7P
K9W3FT
7HPM2X
```

---

### 10. Host 방 코드 자동 생성

Host는 더 이상 Session 이름을 직접 입력하지 않는다.

동작:

```text
비공개 방 생성
↓
6자리 코드 자동 생성
↓
예: AB4K7P
↓
실제 Fusion Session 이름 생성
↓
ProjectJ-AB4K7P
↓
Host Session 생성
```

사용자에게는 짧은 코드만 노출하고 실제 Session 이름은 내부에서 관리한다.

---

### 11. 사용자 방 코드와 내부 Session 이름 분리

사용자 입력:

```text
AB4K7P
```

내부 Fusion Session:

```text
ProjectJ-AB4K7P
```

형태로 분리했다.

이 구조를 통해 이후 실제 Lobby UI에서도 Session 내부 규칙을 사용자에게 노출하지 않고 방 코드만 표시할 수 있다.

---

### 12. Client 방 코드 참가

Client는 Host에게 받은 6자리 코드만 입력한다.

```text
AB4K7P
↓
ProjectJFusionRoomCode.TryNormalize()
↓
ProjectJ-AB4K7P
↓
Fusion Session 참가
```

소문자로 입력해도 자동으로 대문자로 정규화한다.

예:

```text
ab4k7p
↓
AB4K7P
```

---

### 13. 잘못된 방 코드 차단

Fusion 접속을 시작하기 전에 방 코드 자체를 검증한다.

정상:

```text
AB4K7P
```

실패 예:

```text
ABC
→ 길이 부족

ABCDEFG
→ 길이 초과

AB 123
→ 공백 포함

AB@123
→ 허용되지 않은 문자

AB10OP
→ 제외 문자 포함
```

잘못된 코드는 Runner 연결 시도 전에 차단한다.

---

### 14. Session 이름에서 방 코드 추출

현재 연결된 Fusion Session 이름이:

```text
ProjectJ-AB4K7P
```

인 경우:

```text
AB4K7P
```

만 다시 추출할 수 있도록 처리했다.

이를 이용해 Host와 Client 모두 F2 디버그 창에서 현재 참가 중인 방 코드를 동일하게 확인할 수 있다.

---

## F2 네트워크 디버그 UI 개선

### 15. F2 창 크기 확대

기존 네트워크 디버그 창:

```text
470 × 360
```

을 다음 크기로 확대했다.

```text
650 × 510
```

화면 왼쪽 위 위치는 유지하면서 정보가 잘리지 않고 넓게 표시되도록 정리했다.

---

### 16. 방 코드 중심 UI로 변경

기존 Session 이름 입력 UI를 방 코드 중심으로 변경했다.

표시 항목:

```text
방 코드
상태
역할
현재 방 코드
내부 Session
연결 Session
참가 인원
공개 여부
Region
상태 메시지
마지막 결과
```

버튼:

```text
비공개 방 생성
방 코드로 참가
방 나가기
```

---

### 17. 방 코드 표시 강화

방 코드 입력 영역은 일반 정보보다 크게 보이도록 다음과 같이 변경했다.

```text
큰 글자
굵은 글자
가운데 정렬
```

Host가 생성한 코드를 다른 Client에게 전달하기 쉽게 확인할 수 있도록 했다.

---

### 18. 개발용 입력 유지

기존 네트워크 디버그 조작은 그대로 유지한다.

```text
F2
→ 네트워크 테스트 창 표시 / 숨김

ALT 누르고 있음
→ 마우스 커서 활성화
```

따라서 게임 플레이용 커서 잠금 구조를 유지하면서 개발용 UI 버튼을 조작할 수 있다.

---

## 전체 동작 구조

### Host

```text
F2
↓
비공개 방 생성
↓
6자리 코드 생성
↓
AB4K7P
↓
ProjectJ-AB4K7P 변환
↓
Fusion Host 시작
↓
비공개 Session 생성
↓
참가 인원 1 / 8
```

### Client

```text
Development Build 실행
↓
F2
↓
Host 방 코드 입력
↓
AB4K7P
↓
방 코드로 참가
↓
ProjectJ-AB4K7P 변환
↓
기존 Session 참가
↓
참가 인원 2 / 8
```

### 이탈 및 재접속

```text
Client 방 나가기
↓
Host Session 유지
↓
Host 참가 인원 1 / 8
↓
Client 같은 코드 재입력
↓
재참가
↓
2 / 8
```

---

## 생성 파일

```text
Assets/ProjectJ/Network/Fusion/Session/
├─ ProjectJFusionSessionNameValidator.cs
└─ ProjectJFusionRoomCode.cs
```

Unity가 생성한 `.meta` 파일 및 `Session.meta`가 함께 추가된다.

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
├─ ProjectJFusionBootstrap.cs
└─ ProjectJFusionBootstrapDebugView.cs
```

---

## 삭제 파일

```text
없음
```

---

## Development Build 테스트

Editor를 Host로 사용하고 별도로 빌드한 Development Build를 Client로 사용한다.

```text
Unity Editor
→ Host

Development Build .exe
→ Client
```

테스트 흐름:

```text
Editor
→ F2
→ 비공개 방 생성
→ 방 코드 확인

Development Build
→ F2
→ 동일 방 코드 입력
→ 방 코드로 참가

양쪽
→ 참가 인원 2 / 8 확인
```

---

## 테스트 항목

```text
[Host]

비공개 방 생성
→ 6자리 코드 자동 생성

실제 Session
→ ProjectJ-XXXXXX 형식

공개 여부
→ 비공개

참가 인원
→ 1 / 8
```

```text
[Client]

Host 코드 입력
→ 동일 Session 참가

소문자 코드 입력
→ 대문자로 정규화

존재하지 않는 코드
→ 참가 실패
→ 새 Session 생성 안 됨
```

```text
[이탈 / 재접속]

Client 이탈
→ Host 유지
→ 1 / 8

Client 재참가
→ 2 / 8
```

```text
[UI]

F2
→ 확대된 네트워크 창 표시

ALT
→ 커서 활성화

방 코드
→ 큰 글자로 확인 가능
```

---

## 59일차 완료 기준

```text
비공개 Fusion Session 생성 가능
↓
최대 8인 Session 설정
↓
6자리 방 코드 자동 생성
↓
사용자 코드와 내부 Session 이름 분리
↓
Client 코드 입력 참가 가능
↓
잘못된 방 코드 차단
↓
Client 자동 Session 생성 방지
↓
참가 인원 확인 가능
↓
Client 이탈 가능
↓
Host Session 유지
↓
Client 재참가 가능
↓
F2 디버그 UI 확대
↓
Console Error 없음
```

위 기준을 통과하면 기존 59일차와 60일차 작업을 통합한 새로운 59일차를 완료한다.

---

## 다음 개발 방향

다음 일차부터는 비공개 방 접속 기반 위에 실제 네트워크 플레이어를 생성한다.

주요 목표:

```text
Client Session 참가
↓
Network Player Spawn
↓
PlayerRef 연결
↓
State Authority 지정
↓
Input Authority 지정
↓
Host와 Client가 각자의 네트워크 Player 보유
```

아직 이동 동기화까지 진행하지 않고, 플레이어 생성과 Authority 구분부터 단계적으로 연결한다.
