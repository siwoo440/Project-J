# 프로젝트 J 83일차 개발 일지

## 개발 목표
83일차는 온라인 연결 실패와 연결 종료를 감지하고, 자동 재접속·수동 재접속·Steam 재시도·안전한 MainMenu 복귀가 가능한 Recovery 구조를 만드는 작업이다.

## GitHub 반영 상태
83일차 작업 시점에 원격 GitHub의 최신 커밋은 82일차였다. 따라서 아래 내용은 83일차 작업 대화와 작성 코드 기준이며 아직 원격 커밋으로 검증되지는 않았다.

## 주요 작업

### Network Connection Recovery
`ProjectJNetworkConnectionRecovery.cs`를 추가했다.

다음 Fusion 콜백을 통해 연결 문제를 수집하도록 했다.
- `OnConnectFailed`
- `OnDisconnectedFromServer`
- `OnShutdown`

오류가 발생하면 단순 로그로 끝내지 않고 Recovery 상태와 원인을 저장한 뒤 재접속 또는 복귀 흐름으로 이어지게 했다.

### 오류 분류
대표적으로 다음 상황을 구분할 수 있도록 기반을 만들었다.
- 잘못된 Room Code
- 존재하지 않는 Session
- Host 종료
- 연결 중단
- Steam 인증 문제
- 재접속 실패
- Session 종료

### 마지막 Room Code 보존
연결이 끊긴 뒤에도 마지막 Room Code를 보존해 동일한 비공개 Session으로 다시 참가할 수 있도록 했다.

### 자동 재접속
복구 가능한 연결 종료라면 약 1.5초 후 자동 재접속을 1회 수행하도록 했다. 무한 재시도를 하지 않고, 실패하면 수동 RECONNECT 단계로 넘긴다.

### 수동 RECONNECT
자동 재접속 실패 후 사용자가 직접 같은 Room으로 다시 연결할 수 있는 흐름을 만들었다.

### Steam Retry
Steam 인증 실패 또는 초기화 문제 발생 시 Steam 인증 자체를 다시 시도할 수 있는 경로를 추가했다.

### 오류 초기화와 MainMenu 복귀
이전 오류 상태를 초기화하거나 현재 Session 복구를 포기하고 MainMenu로 안전하게 돌아갈 수 있게 했다.

### F12 Recovery Debug
`ProjectJDay83ConnectionRecoveryDebugView.cs`를 추가했다.

F12 Recovery 화면에서 다음 항목을 확인한다.
- 현재 Recovery 상태
- 마지막 오류
- 마지막 Room Code
- 자동 재접속 상태
- 수동 RECONNECT
- 오류 초기화
- Steam Retry
- MainMenu 복귀

## 확인된 핵심 파일
- `Assets/ProjectJ/Network/Fusion/Session/ProjectJNetworkConnectionRecovery.cs`
- `Assets/ProjectJ/Network/Fusion/Test/ProjectJDay83ConnectionRecoveryDebugView.cs`

원격 커밋이 아직 없기 때문에 확인되지 않은 추가 파일명은 임의로 기록하지 않았다.

## Fusion 2.1 컴파일 수정
Reliable Data 수신 콜백의 인자 형식이 현재 Fusion 버전과 맞지 않아 컴파일 오류가 발생했다.

기존:
`ArraySegment<byte>`

수정:
`ReadOnlySpan<byte>`

`ProjectJNetworkConnectionRecovery.cs`의 `OnReliableDataReceived(...)` 시그니처를 Fusion 2.1 기준으로 수정했고, 이후 사용자가 컴파일 오류가 사라졌다고 확인했다.

## 테스트 기준

### 기본
1. Bootstrap → MainMenu
2. F12 Recovery 화면
3. 잘못된 Room Code
4. 오류 초기화
5. Steam Retry

### 실제 네트워크
1. Host / Client 연결
2. Client 연결 종료
3. 자동 재접속 1회
4. 실패 후 수동 RECONNECT
5. Host 종료
6. 존재하지 않는 Session
7. MainMenu 복귀
8. 경기 중 Session 종료 시 `SessionClosed` 계열 처리

실제 Steam 계정과 Fusion Session을 이용한 전체 End-to-End 테스트 완료 기록은 아직 확인되지 않았다.

## 결과
연결 실패를 단순 오류 로그로 끝내지 않고 마지막 Room Code 보존, 자동 재접속 1회, 수동 RECONNECT, Steam Retry, 오류 초기화, MainMenu 복귀를 하나의 Recovery 구조로 묶었다. Fusion 2.1 콜백 시그니처 문제도 수정해 컴파일 오류를 제거했다.
