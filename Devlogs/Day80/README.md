# Project J - 80일차 개발일지

## 1. 개발 목표

79일차까지 Photon Fusion Host Mode의 8인 경기와
RTT·Jitter·Packet Loss 환경 검증을 진행했다.

80일차에서는 온라인 Session에 참가한 Player를
단순한 `PlayerRef`가 아니라 실제 Steam 사용자와 연결하기 위해
Steamworks.NET 기반 인증 계층을 추가하고,
SteamID를 Project J 내부 Account ID와 Fusion UserId에 연결하는 구조를 구현했다.

이번 일차의 주요 목표는 다음과 같다.

- Steamworks.NET 패키지 추가
- Steam 초기화 및 로그인 상태 확인
- SteamID64 획득
- Steam Persona Name 획득
- Project Account ID 생성
- Steam Web API Ticket 발급 준비
- Fusion AuthenticationValues.UserId 연결
- Host에서 PlayerRef와 Account ID 연결 확인
- 동일 Account ID 중복 참가 차단
- Development Build용 steam_appid.txt 자동 복사
- F7 Steam Identity Debug View 추가

실제 Project J Steam App ID를 사용한 최종 인증 검증은
PHASE 7 최종 Gate인 84일차에 진행한다.

---

## 2. Steamworks.NET 패키지 추가

`Packages/manifest.json`에 Steamworks.NET 패키지를 추가했다.

```text
com.rlabrecque.steamworks.net
```

사용 버전:

```text
2025.164.0
```

Git 기반 UPM 패키지로 연결했다.

```text
https://github.com/rlabrecque/Steamworks.NET.git
?path=/com.rlabrecque.steamworks.net
#2025.164.0
```

Unity가 프로젝트를 열면 Package Manager가
Steamworks.NET을 자동으로 가져오는 구조다.

`Packages/packages-lock.json`에도
해당 패키지 정보가 함께 반영되었다.

---

## 3. Steam Identity Service 추가

신규 파일:

```text
Assets/ProjectJ/Steam/Runtime/
└─ ProjectJSteamIdentityService.cs
```

를 추가했다.

게임 실행 시 자동으로 생성되는 Runtime Service이며
Scene이 변경되어도 유지된다.

Runtime Object:

```text
=== Project J Steam Identity ===
```

---

## 4. Steam 인증 상태 정의

Steam 인증 흐름을 상태값으로 구분했다.

```text
Uninitialized
Initializing
WaitingForWebApiTicket
Authenticated
SteamUnavailable
LoginRequired
TicketFailed
PackageMissing
```

이를 통해 단순 성공/실패가 아니라
어느 단계에서 Steam 인증이 멈췄는지 확인할 수 있다.

---

## 5. Steam Client 실행 여부 확인

Steam 초기화 전 먼저:

```text
SteamAPI.IsSteamRunning()
```

을 확인한다.

Steam Client가 실행되어 있지 않으면:

```text
SteamUnavailable
```

상태로 전환한다.

이 상태에서는 Fusion Host 또는 Client 연결을 시작하지 않는다.

---

## 6. SteamAPI 초기화

Steam Client가 실행 중이면:

```text
SteamAPI.Init()
```

을 호출한다.

초기화 실패 시:

```text
SteamUnavailable
```

상태로 처리하고
`steam_appid.txt`와 Steam 실행 상태를 확인하도록 메시지를 표시한다.

---

## 7. Steam 로그인 확인

SteamAPI 초기화 후:

```text
SteamUser.BLoggedOn()
```

으로 로그인 여부를 확인한다.

로그인되어 있지 않으면:

```text
LoginRequired
```

상태가 된다.

따라서 Project J의 Fusion 온라인 연결은
Steam 인증 준비가 끝난 사용자만 진행할 수 있도록 구성했다.

---

## 8. SteamID64 획득

로그인된 사용자는:

```text
SteamUser.GetSteamID()
```

를 사용해 SteamID를 가져온다.

내부에서는 이를 문자열 형태의:

```text
SteamId64
```

로 보관한다.

예:

```text
7656119xxxxxxxxxx
```

---

## 9. Steam Persona Name 획득

Steam 표시 이름은:

```text
SteamFriends.GetPersonaName()
```

으로 가져온다.

이를 통해 Debug 화면에서
SteamID뿐 아니라 현재 Steam 사용자 이름도 확인할 수 있다.

---

## 10. Project Account ID

SteamID64를 Project J 내부 Account ID로 변환한다.

형식:

```text
pj-steam-{SteamID64}
```

예:

```text
pj-steam-7656119xxxxxxxxxx
```

이 값은 이후 게임 내부에서
SteamID 자체를 직접 사용하지 않고
Project J 사용자 식별자로 사용할 수 있는 기반이 된다.

---

## 11. Steam Web API Ticket 준비

Steam 로그인 및 사용자 ID 확인 후:

```text
SteamUser.GetAuthTicketForWebApi()
```

를 호출한다.

Identity 문자열:

```text
projectj-fusion-auth-v1
```

을 사용한다.

Callback:

```text
GetTicketForWebApiResponse_t
```

을 통해 Ticket 응답을 받는다.

성공하면 Ticket Binary를 Hex 문자열로 변환하고
Ticket Byte Length를 기록한다.

---

## 12. Web API Ticket 상태

Ticket 발급 전:

```text
WaitingForWebApiTicket
```

Ticket 발급 성공:

```text
Authenticated
```

Ticket 발급 실패:

```text
TicketFailed
```

로 구분한다.

현재 80일차에서는
Steam Ticket을 정상적으로 발급받을 수 있는 구조까지만 구현했다.

실제 보안 서버에서의 Steam Ticket 검증은
향후 Dedicated Server 또는 인증 서버 계층에서 연결한다.

---

## 13. Steam Callback 처리

Steamworks.NET Callback 처리를 위해
Steam Service의 Update에서:

```text
SteamAPI.RunCallbacks()
```

를 호출한다.

따라서 Web API Ticket Callback을 포함한
Steam 이벤트를 정상적으로 받을 수 있도록 구성했다.

---

## 14. Steam 종료 처리

게임 종료 또는 Steam Identity Service 제거 시:

```text
SteamUser.CancelAuthTicket()
SteamAPI.Shutdown()
```

을 호출한다.

기존 Ticket Handle이 남아 있지 않도록 정리한다.

---

## 15. Fusion 연결 전 Steam 인증 Gate

기존:

```text
ProjectJFusionBootstrap
```

에 Steam 인증 조건을 추가했다.

이제 Host 또는 Client 연결 시작 전에:

```text
ProjectJSteamIdentityService.TryGetAuthenticated()
```

를 검사한다.

인증이 준비되지 않은 경우:

```text
Fusion 연결 시작
→ 차단
```

된다.

Bootstrap 상태:

```text
Failed
```

연결 결과:

```text
Steam 인증 실패
```

로 표시된다.

---

## 16. Fusion AuthenticationValues 연결

Steam 인증 성공 후
Project Account ID를 Photon AuthenticationValues에 넣는다.

사용 타입:

```text
Photon.Realtime.AuthenticationValues
```

구조:

```text
SteamID64
↓
Project Account ID
↓
AuthenticationValues.UserId
↓
StartGameArgs.AuthValues
↓
Fusion Session
```

예:

```text
pj-steam-7656119xxxxxxxxxx
```

가 Photon/Fusion UserId로 전달된다.

---

## 17. 컴파일 오류 수정

초기 80일차 구현 후 다음 오류가 발생했다.

```text
AuthenticationValues
type or namespace not found
```

원인은 Project J에 포함된 Photon Realtime SDK의
실제 namespace가:

```text
Photon.Realtime
```

인데 다른 namespace를 사용했기 때문이었다.

최종 구현에서는:

```text
using Photon.Realtime;
```

으로 수정했다.

---

## 18. F7 GUI Button 오류 수정

초기 F7 Debug View에서:

```text
GUILayout.Button(Rect, ...)
```

형태를 사용해 Unity GUI Overload 오류가 발생했다.

최종 구현에서는 Rect 기반 버튼에 맞게:

```text
GUI.Button(Rect, ...)
```

을 사용하도록 수정했다.

현재 최신 커밋에는 이 수정이 반영되어 있다.

---

## 19. Fusion PlayerRef와 Project Account ID 연결

Host에서는:

```text
Runner.GetPlayerUserId(PlayerRef)
```

를 통해 각 Player의 UserId를 확인한다.

따라서 다음과 같은 매핑이 가능하다.

```text
P0
→ pj-steam-AAAA

P1
→ pj-steam-BBBB
```

이 구조는 이후:

- 친구 초대
- 재접속
- Rank 기록
- FINISH 기록
- 통계
- 저장 데이터
- Dedicated Server

에서 동일 사용자를 식별하는 기반이 된다.

---

## 20. 중복 Account ID 검사

`ProjectJNetworkPlayerSpawner`에서
Player Spawn 전에 Account ID를 검사한다.

확인 과정:

```text
Joining Player
↓
Runner.GetPlayerUserId()
↓
기존 ActivePlayers UserId 비교
```

이미 같은 Account ID가 존재하면:

```text
중복 Project Account ID
↓
Player Spawn 차단
↓
Runner.Disconnect()
```

를 수행한다.

이를 통해 동일 Steam 계정의 중복 참가를
Host에서 차단할 수 있는 기반을 추가했다.

---

## 21. Account ID 없는 Player 차단

PlayerRef에서 UserId를 얻지 못하거나
빈 문자열인 경우도 Spawn하지 않는다.

```text
Project Account ID 없음
↓
Player 연결 거부
```

따라서 Steam 인증 없이
정상 NetworkPlayer로 진입하는 것을 방지한다.

---

## 22. F7 Steam Identity Debug View

신규 파일:

```text
Assets/ProjectJ/Network/Fusion/Test/
└─ ProjectJDay80SteamIdentityDebugView.cs
```

를 추가했다.

Editor 또는 Development Build에서:

```text
F7
```

로 표시/숨김을 전환한다.

---

## 23. F7 표시 정보

Steam 연결 전:

```text
Steam State
Steam Status Message
SteamID64
Persona
Project Account ID
Web API Ticket
```

을 표시한다.

Fusion 연결 후에는:

```text
Fusion State
Local PlayerRef
```

를 추가로 확인한다.

Host에서는 모든 Player의 Account ID도 표시한다.

예:

```text
HOST ACCOUNT MAP

P0 -> pj-steam-AAAA
P1 -> pj-steam-BBBB
```

---

## 24. Steam 인증 다시 시도

F7 화면에:

```text
Steam 인증 다시 시도
```

버튼을 추가했다.

Steam Client 실행 상태 변경 등의 이유로
초기 인증이 실패했을 때
게임을 다시 실행하지 않고 인증 초기화를 다시 시도할 수 있다.

---

## 25. 기존 Debug View와 분리

현재 Debug 단축키 구조:

```text
F4
→ 77일차 4인 Gate

F5
→ 78일차 8인 Gate

F6
→ 79일차 Network Condition Gate

F7
→ 80일차 Steam Identity Gate
```

F7 Debug View가 켜진 동안에는
79일차 F6 Network Condition 화면을 자동으로 숨겨
화면이 겹치지 않도록 구성했다.

---

## 26. steam_appid.txt 추가

프로젝트 루트에:

```text
steam_appid.txt
```

를 추가했다.

현재 저장된 값:

```text
0
```

이다.

이는 실제 Steam App ID를 아직 연결하지 않은 상태를 의미한다.

80일차에서는 Steam 연동 코드 구현을 완료하고
실제 Project J Steam App ID를 통한 인증 검증은
84일차 PHASE 7 통합 Gate로 이월한다.

---

## 27. Development Build App ID 자동 복사

신규 Editor Build Processor:

```text
Assets/ProjectJ/Editor/
└─ ProjectJDay80SteamAppIdBuildProcessor.cs
```

를 추가했다.

Development Build 완료 후
프로젝트 루트의:

```text
steam_appid.txt
```

를 Build 실행 파일과 같은 폴더로 복사한다.

예:

```text
Build/
├─ ProjectJ.exe
└─ steam_appid.txt
```

---

## 28. App ID 유효성 검사

Build Processor에서는
`steam_appid.txt`를 읽어 다음을 확인한다.

```text
숫자인가?
0보다 큰가?
```

현재처럼:

```text
0
```

이면 실제 파일 복사는 하지 않고 Warning을 출력한다.

따라서 잘못된 App ID를 사용해
Development Build를 실행하는 것을 방지한다.

---

## 29. Release Build 처리

비-Development Build에서는
`steam_appid.txt`를 자동으로 배포하지 않는다.

이미 Build 폴더에 존재하는 경우 제거한다.

이는 `steam_appid.txt`가
개발 환경 테스트용 파일이라는 점을 고려한 처리다.

---

## 30. 변경 파일

79일차 완료 커밋과 비교한 80일차 변경 파일은 다음과 같다.

### 신규

```text
Assets/ProjectJ/Editor/
├─ ProjectJDay80SteamAppIdBuildProcessor.cs
└─ ProjectJDay80SteamAppIdBuildProcessor.cs.meta

Assets/ProjectJ/Network/Fusion/Test/
├─ ProjectJDay80SteamIdentityDebugView.cs
└─ ProjectJDay80SteamIdentityDebugView.cs.meta

Assets/ProjectJ/Steam/
├─ Runtime/
│  ├─ ProjectJSteamIdentityService.cs
│  └─ ProjectJSteamIdentityService.cs.meta
├─ Steam.meta
└─ Runtime.meta

steam_appid.txt
```

### 수정

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
└─ ProjectJFusionBootstrap.cs

Assets/ProjectJ/Network/Fusion/Player/
└─ ProjectJNetworkPlayerSpawner.cs

Packages/
├─ manifest.json
└─ packages-lock.json

ProjectSettings/
└─ ProjectSettings.asset
```

### 삭제

```text
없음
```

---

## 31. 최신 커밋 확인

현재 최신 GitHub 커밋:

```text
516ed117f236b08e3916b6bb667d8d4b196686d6
```

현재 커밋 제목:

```text
a
```

79일차 완료 커밋:

```text
b4fa626e5f26d862140c7aae474078f23b090f44
```

대비:

```text
1 commit ahead
0 commit behind
```

상태다.

---

## 32. 현재 검증 상태

최신 커밋 기준으로 다음 구현이 확인되었다.

- Steamworks.NET UPM 의존성 추가
- Steam Identity Service 추가
- Steam 초기화 코드
- Steam 로그인 확인
- SteamID64 획득 구조
- Persona Name 획득 구조
- Project Account ID 생성
- Web API Ticket 요청 구조
- Fusion AuthValues 연결
- `Photon.Realtime.AuthenticationValues` 컴파일 수정 반영
- PlayerRef → Project Account ID 확인 구조
- 중복 Account ID 차단
- F7 Steam Identity Debug View
- `GUI.Button` 컴파일 수정 반영
- Development Build App ID 복사 도구
- steam_appid.txt 추가

GitHub CI 상태 검사는 현재 등록되어 있지 않다.

따라서 Unity 실제 Compile/Runtime 및
Steam 인증 성공 여부는 GitHub에서 자동 확인되지 않는다.

---

## 33. 80일차 완료 판정

80일차는 다음 기준으로 처리한다.

```text
구현
→ 완료

실제 Project J Steam App ID 인증 검증
→ 84일차로 이월
```

즉 이번 일차에서는 Steam 계정 식별과
Fusion 사용자 연결에 필요한 코드 기반을 완성했다.

다음 항목은 PHASE 7 최종 Gate에서 검증한다.

- 실제 Project J Steam App ID 설정
- SteamAPI.Init 성공
- Steam 로그인 확인
- SteamID64 확인
- Persona 확인
- Web API Ticket READY
- 서로 다른 두 Steam 계정 Host/Client 연결
- PlayerRef와 Project Account ID 일치
- 동일 Account ID 중복 참가 차단

---

## 34. 다음 개발 방향

다음 81일차에서는
80일차에서 만든 Steam 사용자 식별 계층을 기반으로
Steam 친구 초대와 비공개 방 참가 흐름을 연결한다.

예정 흐름:

```text
Host
↓
비공개 Fusion Session 생성
↓
Steam 친구 초대
↓
친구가 Invite 수락
↓
Project J 실행 또는 활성화
↓
초대 정보 확인
↓
Host의 비공개 방 참가
```

81일차에서도 실제 Project J Steam App ID 최종 검증은
84일차 Gate에서 함께 진행한다.
