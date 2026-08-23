# 프로젝트 J 84일차 개발 일지

## 개발 목표
84일차는 PHASE 7의 마지막 Gate 일차다. 신규 기능을 추가하기보다 76~83일차의 Host Mode 온라인 기능을 한 번에 점검하고 다음 페이즈로 넘어갈 수 있는지 판단하는 것이 목적이었다.

## 점검 범위
- 다중 창 Host Mode
- 2인·4인·8인 경기
- RTT·Jitter·Packet Loss
- Steam 인증
- Project Account ID
- Steam 친구 초대
- Rich Presence
- 비공개 Room 자동 참가
- 전체 Scene Flow
- 재접속
- 온라인 오류 처리
- 재시도
- Lobby·Game·MainMenu 복귀

## 실제 진행 상태
현재 확인 가능한 기록에서는 PHASE 7 전체 End-to-End Gate가 완료되었다고 판단할 근거가 없다.

특히 다음 항목은 완료 판정을 보류한다.
- 실제 Steam App ID를 사용한 인증
- 서로 다른 Steam 계정 2개 인증
- 친구 초대 수락 후 실제 Fusion Room 참가
- 실제 4인 전체 경기
- 실제 8인 전체 경기
- Steam + Scene Flow + 재접속 통합 검증

따라서 84일차는 완료된 신규 기능 개발일이 아니라 **PHASE 7 Gate 점검 및 후속 개발 일정 재정리 일차**로 기록한다.

## PHASE 7에서 확보한 기반

### 다중 창·경기 테스트
Editor Host와 여러 Windows Build Client를 동시에 실행할 수 있는 구조와 최대 8인 Spawn / GAME START 기반을 만들었다.

### 4인·8인 진단
4인 Greybox 경기와 8인 성능/Prediction Gate 구조를 준비했다.

### 네트워크 품질
RTT, Jitter, Packet Loss 조건을 인위적으로 적용할 수 있는 진단 구조를 만들었다.

### Steam 인증
Steamworks.NET, SteamID, Project Account ID와 Fusion 사용자 연결 기반을 만들었다. 실제 App ID 다계정 최종 검증은 남아 있다.

### Steam 친구 초대
친구 목록, Rich Presence, `InviteUserToGame`, Connect String, 초대 수락 후 Room 참가 기반을 만들었다.

### Scene Flow
82일차에 Bootstrap → MainMenu → Lobby → MatchLoading → Game과 Game → Lobby, Session 종료 → MainMenu 복귀를 연결했다.

### Recovery
83일차에 연결 실패 감지, 마지막 Room Code, 자동 재접속 1회, 수동 RECONNECT, Steam Retry, 오류 초기화, MainMenu 복귀를 구현했다.

## Gate 결과
- PHASE 7 기능 구현 기반: 완료
- 82일차 Scene Flow: 구현 완료
- 83일차 Recovery: 구현 및 컴파일 오류 수정
- 실제 Steam 런타임 검증: 미확인
- 실제 4인·8인 전체 경기 Gate: 미확인
- PHASE 7 최종 Gate 완료 판정: 보류

## 후속 일정 재구성
Dedicated Server 전환 전에 Scene 구성과 전체 Flow를 별도로 검증하기 위해 새 PHASE 8을 추가했다.

### PHASE 8. Scene 구성·전체 Flow 검증
- 85일차: Bootstrap Scene
- 86일차: MainMenu
- 87일차: 온라인 방 UI
- 88일차: Lobby
- 89일차: Lobby Ready / Player UI
- 90일차: Game 경기장
- 91일차: Game HUD / Countdown / Result
- 92일차: 전체 Scene 연결
- 93일차: 2인 실전 Flow 검증
- 94일차: Integration Gate

기존 Dedicated Server 이후 일정은 10일씩 뒤로 이동했다.

## 결과
84일차에는 PHASE 7을 무리하게 완료 처리하지 않고 아직 실제 검증되지 않은 Steam 다계정, 4·8인 경기, 재접속과 전체 Scene Flow를 남은 Gate 항목으로 분리했다. 그리고 Dedicated Server 전환 전에 Scene 구조와 전체 Flow를 별도 페이즈에서 검증하도록 개발 일정을 재구성했다.
