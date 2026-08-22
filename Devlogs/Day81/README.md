# Project J - 81일차 개발일지

## 1. 개발 목표

80일차에 구축한 Steam 사용자 식별 및 Fusion UserId 연결 구조를 기반으로,
Steam 친구 초대 기능을 기존 Project J의 비공개 Fusion Room Code 시스템에 연결한다.

이번 일차에서는 새로운 매치메이킹 시스템이나 Steam Lobby를 만들지 않고,
현재 사용 중인 6자리 Fusion Room Code를 Steam 친구 초대의 Connect String으로 전달해
초대를 수락한 친구가 기존 `RequestJoinPrivateRoom()` 흐름을 통해
같은 Fusion Session에 참가하도록 구성했다.

주요 목표:

- Steam 친구 목록 조회
- Host의 비공개 Fusion Room을 Steam Rich Presence에 게시
- 친구에게 Room Code가 포함된 Steam Invite 전송
- `GameRichPresenceJoinRequested_t` 수신
- Steam Connect String에서 Room Code 추출
- Room Code 유효성 검사
- 기존 Fusion Room 참가 흐름 재사용
- 다른 Room 참가 중 초대 수락 시 기존 Room 종료 후 이동
- Steam을 통해 게임이 실행된 경우 Launch Command 처리
- Host Room 종료/비공개 Session Close 시 Rich Presence 정리
- F8 Steam Invite Debug View 추가
- 실제 Steam End-to-End 테스트는 84일차 PHASE 7 Gate로 이월

---

## 2. 현재 기준 커밋

81일차 작업 기준 최신 커밋:

```text
a05b5fd3aa9e75f211c64e2a73bea2af69984e1a
```

현재 커밋 제목:

```text
a
```

이전 80일차 완료 커밋:

```text
b098875ca2164a6648598ba09b4cac5c8a3aa055
80일차 : Steam 인증·Project Account ID 및 Fusion 사용자 연결 구현
```

81일차 최신 커밋은 80일차보다:

```text
1 commit ahead
0 commit behind
```

상태다.

---

## 3. 변경 파일

### 신규 파일

```text
Assets/ProjectJ/Steam/Runtime/
├─ ProjectJSteamInviteService.cs
└─ ProjectJSteamInviteService.cs.meta

Assets/ProjectJ/Network/Fusion/Test/
├─ ProjectJDay81SteamInviteDebugView.cs
└─ ProjectJDay81SteamInviteDebugView.cs.meta
```

### 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Test/
└─ ProjectJDay80SteamIdentityDebugView.cs
```

변경 내용:

```text
F7 Day80 Steam Identity Debug View
기본 표시 true
↓
기본 표시 false
```

### 삭제 파일

```text
없음
```

### 수정하지 않은 주요 파일

```text
ProjectJFusionBootstrap.cs
ProjectJFusionRoomCode.cs
Game.unity
```

81일차에서는 기존 Fusion 연결 로직을 그대로 재사용한다.

---

## 4. 기존 Fusion Room Code 재사용

Project J의 Room Code는 6자리다.

사용 문자:

```text
ABCDEFGHJKLMNPQRSTUVWXYZ23456789
```

예:

```text
R7K2QM
9PG4TW
2MFK8H
```

숫자 전용이 아니라 영문 대문자와 숫자를 함께 사용한다.

기존:

```text
ProjectJFusionRoomCode.TryNormalize()
```

를 그대로 재사용해
Steam 초대에서 받은 Room Code도 동일한 규칙으로 검증한다.

---

## 5. Steam Invite Service 추가

신규 Runtime Service:

```text
ProjectJSteamInviteService
```

를 추가했다.

게임 실행 시 자동 생성되고 Scene 전환 후에도 유지된다.

Runtime Object:

```text
=== Project J Steam Invite ===
```

주요 역할:

```text
Steam 친구 목록 조회
Steam 친구 초대 전송
Steam Rich Presence 관리
초대 수락 Callback 처리
Launch Command 처리
Pending Invite 관리
기존 Fusion Room 참가 호출
```

---

## 6. Steam Invite 상태

81일차 초대 흐름을 다음 상태로 구분했다.

```text
WaitingForSteam
Ready
HostRoomReady
InviteSent
InviteReceived
LeavingCurrentRoom
Joining
Joined
InvalidInvite
Failed
```

이를 통해 F8 화면에서
현재 Steam Invite 흐름이 어느 단계에 있는지 확인할 수 있다.

---

## 7. Steam 인증 의존

Steam Invite 기능은 80일차의:

```text
ProjectJSteamIdentityService
```

가 인증된 상태에서만 동작한다.

조건:

```text
steamIdentity != null
steamIdentity.IsAuthenticated == true
```

인증 전에는:

```text
WaitingForSteam
```

상태를 유지한다.

80일차와 동일하게 실제 Project J Steam App ID 검증은
84일차 PHASE 7 최종 Gate에서 수행한다.

---

## 8. Steam 친구 목록 조회

Steam 친구 목록은:

```text
SteamFriends.GetFriendCount()
SteamFriends.GetFriendByIndex()
```

를 사용한다.

친구 범위:

```text
EFriendFlags.k_EFriendFlagImmediate
```

즉 현재 Steam 친구 목록의 일반 친구를 대상으로 한다.

각 친구에 대해 다음 정보를 수집한다.

```text
SteamID64
Persona Name
Persona State
Online 여부
In Game 여부
```

---

## 9. 친구 상태 조회

각 친구의 상태는:

```text
SteamFriends.GetFriendPersonaState()
```

로 확인한다.

게임 플레이 여부는:

```text
SteamFriends.GetFriendGamePlayed()
```

를 사용한다.

F8 Debug View에서는 예를 들어:

```text
Friend A / Online
Friend B / Offline
Friend C / Online / IN GAME
```

형태로 확인할 수 있다.

---

## 10. 친구 목록 자동 갱신

친구 목록은 일정 주기로 갱신한다.

현재 기준:

```text
2초
```

마다 새로 읽는다.

F8 화면에는 수동:

```text
친구 목록 새로고침
```

버튼도 추가했다.

---

## 11. Steam Connect String

Steam Invite에는 전체 Session 데이터를 넣지 않는다.

현재 Fusion Room Code만 전달한다.

형식:

```text
+projectj_room {RoomCode}
```

예:

```text
+projectj_room R7K2QM
```

이 문자열이 Steam Invite와 Rich Presence의 연결 정보가 된다.

---

## 12. Connect String 생성

Service 내부에서:

```text
BuildConnectString()
```

을 통해 Room Code를 Connect String으로 변환한다.

변환 전 반드시:

```text
ProjectJFusionRoomCode.TryNormalize()
```

를 사용한다.

잘못된 Room Code는 Connect String으로 만들지 않는다.

---

## 13. Connect String 파싱

Steam에서 전달받은 연결 문자열은:

```text
TryParseConnectString()
```

으로 처리한다.

다음 두 형식을 처리한다.

```text
+projectj_room R7K2QM
```

또는

```text
+projectj_room=R7K2QM
```

파싱된 값 역시 기존 Room Code Validator를 통과해야 한다.

---

## 14. 잘못된 초대 방어

다음과 같은 문자열은 거부한다.

```text
+projectj_room ABC
+projectj_room !!!!!!
+wrong_room R7K2QM
```

유효하지 않으면:

```text
InvalidInvite
```

상태로 변경한다.

잘못된 Steam Connect String을
Fusion 참가 요청에 그대로 넘기지 않는다.

---

## 15. Host Room 조건

Steam 초대는 Host가 현재 정상적인 비공개 Fusion Room을 열고 있을 때만 가능하다.

조건:

```text
Steam 인증 완료
Fusion State = Running
ActiveMode = Host
Session Open = true
유효한 Connected Room Code 존재
```

조건을 만족하면:

```text
CanInvite = true
```

가 된다.

경기가 시작되어 기존 시스템이 Session을 닫으면
새 초대 전송도 막힌다.

---

## 16. Steam Rich Presence 등록

Host가 OPEN 상태의 Fusion Room을 만들면:

```text
SteamFriends.SetRichPresence()
```

를 사용한다.

등록 키:

```text
connect
status
```

Connect 값:

```text
+projectj_room R7K2QM
```

Status:

```text
Project J Private Match
```

---

## 17. Steam Join Game 지원

Rich Presence의:

```text
connect
```

값을 등록했기 때문에
Steam 친구 목록의 Join Game 흐름에서도
동일한 Connect String을 전달할 수 있는 기반을 마련했다.

즉 Host는:

```text
Fusion Room 생성
↓
Rich Presence 등록
↓
Steam에서 Join Game 가능한 상태
```

가 된다.

---

## 18. Host Room 종료 시 Rich Presence 정리

다음 상황에서는 Rich Presence를 제거한다.

```text
Host Room 종료
Session Close
Steam 인증 해제
Application Quit
Invite Service Destroy
```

삭제 대상:

```text
connect
status
```

이를 통해 이미 끝난 Room의 Join Game 정보가
Steam에 계속 남아 있는 것을 방지한다.

---

## 19. Steam 친구 초대 전송

Host가 F8에서 친구의:

```text
INVITE
```

버튼을 누르면:

```text
SteamFriends.InviteUserToGame()
```

을 호출한다.

전달 값:

```text
Friend SteamID
Connect String
```

예:

```text
Friend
→ 7656119xxxxxxxxxx

Connect
→ +projectj_room R7K2QM
```

성공 시:

```text
InviteSent
```

상태로 변경한다.

---

## 20. 초대 수락 Callback

친구가 Steam 초대를 수락하면:

```text
GameRichPresenceJoinRequested_t
```

Callback을 받는다.

Callback에서:

```text
m_rgchConnect
m_steamIDFriend
```

를 읽는다.

`m_rgchConnect`에서 Room Code를 추출하고
`m_steamIDFriend`를 마지막 초대 발신 SteamID로 기록한다.

---

## 21. Pending Invite

초대를 받으면 바로 모든 로직을 강제로 실행하지 않고:

```text
PendingInviteRoomCode
```

에 먼저 저장한다.

예:

```text
Pending Invite
→ R7K2QM
```

이후 현재 Fusion Bootstrap 상태를 확인하면서
안전하게 참가 절차를 진행한다.

---

## 22. Fusion 연결 전 상태

현재 Fusion Session이 없다면:

```text
bootstrap.CanStart == true
```

인 상태다.

이 경우:

```text
bootstrap.RoomCode = PendingInviteRoomCode
↓
bootstrap.RequestJoinPrivateRoom()
```

을 호출한다.

즉 81일차에서 새로운 Fusion Client 연결 코드를 만들지 않았다.

---

## 23. 기존 Room 참가 중 초대 수락

이미 다른 Fusion Room에 접속해 있는 상태에서
Steam 초대를 수락할 수 있다.

이 경우:

```text
현재 Fusion Room
↓
RequestLeaveRoom()
↓
Bootstrap Idle
↓
Pending Invite Room Code 입력
↓
RequestJoinPrivateRoom()
```

순서로 처리한다.

Runner가 종료되기 전에
새 Runner를 동시에 시작하지 않도록 구성했다.

---

## 24. 동일 Room 초대

이미 초대받은 Room에 접속해 있다면
다시 Leave/Join하지 않는다.

```text
Connected Room
=
Pending Invite Room
```

이면:

```text
Joined
```

상태로 정리한다.

---

## 25. Steam을 통해 게임이 실행된 경우

게임이 꺼져 있는 상태에서
Steam Invite나 Join Game을 통해 실행될 가능성도 고려했다.

초기 실행 시:

```text
SteamApps.GetLaunchCommandLine()
```

을 확인한다.

Launch Command에:

```text
+projectj_room R7K2QM
```

이 포함되어 있으면
일반 Steam Invite와 동일하게 Pending Invite로 처리한다.

---

## 26. 실행 중 새로운 Launch 요청

이미 게임이 실행 중인데
Steam에서 새로운 실행/Join 요청이 발생하는 경우:

```text
NewUrlLaunchParameters_t
```

Callback을 등록했다.

Callback 수신 후 다시:

```text
SteamApps.GetLaunchCommandLine()
```

을 읽어 Room Code를 확인한다.

---

## 27. 기존 Fusion Bootstrap 재사용

81일차에서:

```text
ProjectJFusionBootstrap.cs
```

는 수정하지 않았다.

이미 기존 Bootstrap에 다음 기능이 있기 때문이다.

```text
RoomCode
CanStart
CanShutdown
RequestLeaveRoom()
RequestJoinPrivateRoom()
ConnectedRoomCode
HasValidSessionInfo
```

Steam Invite Service가 이 API를 사용하도록 구성했다.

이렇게 해서 Steam 시스템과 Fusion 핵심 연결 로직의 결합을 최소화했다.

---

## 28. F8 Steam Friend Invite Debug View

신규 파일:

```text
ProjectJDay81SteamInviteDebugView.cs
```

를 추가했다.

Editor 또는 Development Build에서:

```text
F8
```

로 표시/숨김을 전환한다.

기본 상태:

```text
표시
```

---

## 29. F8 표시 정보

F8에서는 다음을 확인할 수 있다.

```text
Steam Auth
Steam Persona

Invite State
Invite Status Message

Fusion State
Host / Client
Room Code
Session Open / Closed

Published Room
Pending Invite
Last Accepted Room
Last Invite From

Steam Friend Count
Friend SteamID
Friend Persona
Friend Online State
Friend In Game
INVITE 버튼
```

---

## 30. F7 기본 숨김

80일차 F7 Steam Identity 화면은 삭제하지 않았다.

81일차부터 기본 화면이 F8이므로:

```text
F7
기본 표시
↓
기본 숨김
```

으로 변경했다.

필요한 경우 F7을 눌러
80일차 Steam 인증 상태를 다시 확인할 수 있다.

---

## 31. 현재 Debug 단축키

```text
F4
→ 77일차 4 Player Gate

F5
→ 78일차 8 Player Gate

F6
→ 79일차 Network Condition Gate

F7
→ 80일차 Steam Identity Gate

F8
→ 81일차 Steam Friend Invite
```

---

## 32. 실제 Steamworks.NET API 확인

81일차 코드에서 사용한 핵심 Steamworks.NET API:

```text
SteamFriends.GetFriendCount()
SteamFriends.GetFriendByIndex()
SteamFriends.GetFriendPersonaState()
SteamFriends.GetFriendPersonaName()
SteamFriends.GetFriendGamePlayed()

SteamFriends.InviteUserToGame()
SteamFriends.SetRichPresence()

GameRichPresenceJoinRequested_t
NewUrlLaunchParameters_t

SteamApps.GetLaunchCommandLine()
```

현재 프로젝트에 설치된 Steamworks.NET 패키지의
실제 API 형태에 맞춰 구현했다.

---

## 33. Room Code 기준 수정

이전 설명 과정에서 Room Code를
'6자리 숫자'라고 표현한 적이 있었지만,
실제 Project J 코드 기준 Room Code는 숫자 전용이 아니다.

정확한 기준:

```text
길이
→ 6

문자
→ ABCDEFGHJKLMNPQRSTUVWXYZ23456789
```

81일차 구현은 실제 코드 규칙을 기준으로 작성했다.

---

## 34. 최신 커밋 정적 검토

80일차 완료 커밋과 81일차 최신 커밋을 비교한 결과
변경 파일은 정확히 다음 5개다.

```text
수정 1개
신규 4개
삭제 0개
```

수정:

```text
ProjectJDay80SteamIdentityDebugView.cs
```

신규:

```text
ProjectJDay81SteamInviteDebugView.cs
ProjectJDay81SteamInviteDebugView.cs.meta
ProjectJSteamInviteService.cs
ProjectJSteamInviteService.cs.meta
```

기존:

```text
ProjectJFusionBootstrap.cs
Game.unity
```

등은 변경하지 않았다.

정적 코드 구조 및
현재 설치된 Steamworks.NET API 연결 기준으로
즉시 수정해야 할 명백한 문제는 확인되지 않았다.

---

## 35. CI 상태

최신 81일차 커밋에는
GitHub CI Status Check가 등록되어 있지 않다.

따라서 GitHub에서 자동으로:

```text
Unity Compile
PlayMode
Windows Build
Steam Invite
Fusion Host/Client
```

를 검증한 상태는 아니다.

최종 Runtime 검증은 Unity와 Steam Client에서 진행한다.

---

## 36. 81일차 완료 판정

81일차는 다음 기준으로 처리한다.

```text
Steam 친구 초대 코드 구현
→ 완료

Steam Rich Presence 연결
→ 완료

초대 수락 → Fusion Room 자동 참가 구조
→ 완료

실제 Steam App ID + 두 Steam 계정 End-to-End 검증
→ 84일차 PHASE 7 Gate로 이월
```

따라서 이번 일차에서는
Steam Friend Invite와 기존 Fusion Private Room을 연결하는
코드 기반을 완성한 것으로 기록한다.

---

## 37. 84일차 이월 검증 항목

PHASE 7 최종 Gate에서 다음을 실제로 확인한다.

```text
Project J 실제 Steam App ID
Steam Account A
Steam Account B

Host Fusion Room 생성
↓
Steam Rich Presence 게시
↓
Friend Invite 전송
↓
Account B 초대 수락
↓
Room Code 정상 전달
↓
같은 Fusion Session 참가
↓
PlayerRef / Project Account ID 정상
```

추가 확인:

```text
잘못된 Connect String 거부
다른 Room 참가 중 Invite 처리
Host 종료 시 Rich Presence 제거
Session CLOSED 후 Invite 비활성
게임 종료 상태에서 Invite 수락
```

---

## 38. 다음 개발 방향

다음 82일차에서는
현재 개별적으로 동작하는 Bootstrap, Lobby, Steam Invite, Game Scene 흐름을
하나의 전체 Scene Flow로 연결한다.

예정 흐름:

```text
Bootstrap
↓
Steam 인증
↓
Lobby
↓
Host 생성 / Room Code 참가 / Steam Invite 참가
↓
Match Loading
↓
Game
↓
Result
↓
Lobby 또는 종료
```

81일차에서 만든 `PendingInviteRoomCode`도
82일차 Scene Flow와 연결해
초대를 통해 게임이 실행되더라도
올바른 Lobby/Match 흐름으로 진입할 수 있게 확장한다.
