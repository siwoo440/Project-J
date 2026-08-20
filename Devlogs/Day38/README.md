# PHASE 3 개발일지 - 모듈 조립형 고정맵 기반 1차 게임 구현

## 1. Phase 3 개요

Phase 3는 25일차부터 38일차까지 진행하는 **모듈 조립형 고정맵 기반 1차 게임 구현 단계**다.

기획서의 Phase 3 완료 기준은 다음과 같다.

```text
동일 규격의 정육면체 Module Prefab을 Socket 기준으로 수동 조립한 고정 5구간 맵에서
START부터 1000m FINISH까지 플레이하며
카운트다운·순위·체크포인트·부활·결과가 한 판 안에서 완전히 동작한다.
```

현재 GitHub 기준 Phase 3 관련 구현은 25일차부터 37일차까지 반영되어 있다.

현재 최신 커밋:

```text
91ceeff3e7c2b8a5abc0e92a1b894dcef0e57ea8
```

현재 최신 커밋 메시지:

```text
37일차 : 기본 관전 전환 및 FINISH 플레이어 퇴장 처리 구현
```

38일차는 Phase 3의 최종 통합 테스트 단계이며, 별도 구현 커밋은 아직 없다.

---

## 2. Phase 3의 전체 목표

이번 Phase에서는 플레이어 조작 기반을 실제 경기 흐름으로 연결했다.

전체 구조는 다음과 같다.

```text
정육면체 Module 기반 고정맵
↓
발 기준 높이 계산
↓
실시간 경쟁 순위
↓
Ready 기반 경기 준비
↓
3·2·1 카운트다운
↓
경기 타이머
↓
체크포인트 저장
↓
구간별 추락 판정
↓
최고 체크포인트 부활
↓
3초 부활 보호
↓
FINISH 도착 순서 확정
↓
개인 결과 Snapshot 생성
↓
완주 Player 퇴장
↓
기본 관전 전환
```

즉, Phase 2까지 만든 플레이어 조작을 **실제 한 판의 경기 구조**로 연결하는 것이 핵심이었다.

---

# 3. 일차별 개발 내용

## 25일차 - 정육면체 Module 규격 및 고정맵 기반 구축

커밋:

```text
0a57f6a1ed449562f28df8dcfb357c8f033497f9
```

핵심 목표:

```text
동일 규격의 1:1:1 정육면체 Module
6면 분리
6방향 Socket
Entrance / Exit / Drop 구조
고정맵 수동 조립
```

플레이어 키 2를 기준으로 Module 규격을 통일했다.

Module의 기본 구조:

```text
Module
├─ Floor
├─ Ceiling
├─ North
├─ South
├─ East
├─ West
└─ Socket
   ├─ North
   ├─ South
   ├─ East
   ├─ West
   ├─ Up
   └─ Down
```

고정맵도 하나의 거대한 Greybox가 아니라 이후 절차 생성에서 사용할 동일 Module Prefab을 수동으로 조립하는 방식으로 구성했다.

진행 기본 규칙:

```text
현재 Module Exit
↓
다음 Module Entrance
```

Drop은 진행 경로가 아니라 낙하 위험 공간으로 분리했다.

---

## 26일차 - 발 기준 플레이어 높이 계산

커밋:

```text
78d8cd553d6a50efd074f7dd69b6b03ba1bdefd0
```

플레이어의 높이를 캐릭터 중심이 아니라 **발 위치 기준**으로 계산하는 시스템을 구축했다.

기본 기준:

```text
캐릭터 Local Y = 0
→ Player Foot
```

높이는 소수점 둘째 자리까지 사용하고 이후 값은 버리는 방식으로 관리한다.

주요 데이터:

```text
CurrentHeight
CurrentHeightCentimeters
HighestHeight
HighestHeightCentimeters
```

이 값은 이후 실시간 순위와 개인 결과 데이터의 기반이 되었다.

---

## 27일차 - 실시간 높이 기반 공동 순위

커밋:

```text
87c3ebe9377db416a2642edb08f0070b16c7a68b
```

플레이어의 현재 높이를 비교해 실시간 순위를 계산하는 구조를 구현했다.

순위는 경쟁 순위 방식을 사용한다.

예:

```text
1위
2위
공동 2위
4위
```

현재 높이가 내려가면 순위도 다시 내려간다.

체크포인트나 최고 높이는 순위 기준으로 사용하지 않고 현재 실제 높이만 사용한다.

---

## 28일차 - Ready 기반 경기 상태 및 카운트다운

커밋:

```text
205e6bf8d14ad6f2f36f8699fa60f07c3969ab56
```

경기의 기본 상태 흐름을 구성했다.

```text
Preparing
↓
Countdown
↓
Playing
↓
Finished
```

오프라인 개발 단계에서는 모든 플레이어가 준비되었다는 상태를 자동으로 시뮬레이션한다.

카운트다운:

```text
3
2
1
시작!
```

카운트다운 완료 시점에 플레이어 조작을 해금하도록 구성했다.

향후 Fusion 적용 시 서버가 모든 참가자의 준비 상태를 확인한 뒤 동일 흐름을 시작하도록 확장할 수 있는 형태로 설계했다.

---

## 29일차 - 15분 경기 타이머 및 경고

커밋:

```text
ab7cf88f974b3f713556d8e23beaa6f12ca1272b
```

현재 구현 기준 경기 시간은 15분으로 설정했다.

경고 시점:

```text
1분
30초
10초
```

경기 카운트다운 중에는 제한 시간이 감소하지 않고 실제 Playing 상태부터 타이머가 감소하도록 구성했다.

시간 종료 Event는 이후 개인 결과 생성과 경기 종료 처리에서 사용할 수 있도록 연결했다.

---

## 30일차 - 체크포인트 기본 활성화

커밋:

```text
08ce33f81787638b3e71abca943181be36540264
```

고정맵의 주요 높이에 체크포인트를 배치하고 플레이어별 활성화 상태를 저장하기 시작했다.

체크포인트:

```text
START
CP1
CP2
CP3
CP4
```

기본 저장 데이터:

```text
CurrentCheckpointId
RespawnPosition
RespawnRotation
```

---

## 31일차 - 체크포인트 건너뛰기 및 최고값 유지

커밋:

```text
0c593015513e524ced4bf0033ec15fb78473c544
```

체크포인트는 순서대로 밟아야만 하는 구조가 아니라, 더 높은 체크포인트를 먼저 밟아도 정상 활성화되도록 변경했다.

예:

```text
START
↓
CP3
```

이면 CP3를 최고 체크포인트로 저장한다.

이후:

```text
CP1
CP2
```

를 밟아도 저장값은 CP3 아래로 내려가지 않는다.

---

## 32일차 - 구간별 Fall Limit

커밋:

```text
cef826b134afc761d23f1e965b4dc55199ba504f
```

현재 최고 체크포인트 기준으로 플레이어가 지나치게 아래로 떨어졌는지 감지하는 구조를 구현했다.

흐름:

```text
현재 최고 Checkpoint
↓
해당 Checkpoint의 Fall Limit 조회
↓
Player Y 비교
↓
Fall Limit 아래
↓
Fallen 상태
```

32일차에서는 실제 이동이나 부활보다 **추락 상태 감지**에 책임을 한정했다.

---

## 33일차 - 체크포인트 부활 및 물리 초기화

커밋:

```text
4a62efc10cf1f0bd0d70401b4d82bf0454511014
```

추락 또는 직접 부활 요청 시 최고 체크포인트 위치로 이동하는 시스템을 추가했다.

부활 시:

```text
위치 초기화
회전 초기화
linearVelocity = 0
angularVelocity = 0
Fall 상태 초기화
```

를 수행한다.

체크포인트가 없으면 START 위치를 사용한다.

---

## 34일차 - 3초 부활 보호

커밋:

```text
966a5a5ba83c6e97455a94a09f9ba784f04ab406
```

부활 직후 다시 즉시 밀치기나 방해를 받아 연속으로 추락하는 상황을 막기 위한 보호 기반을 추가했다.

기본 보호 시간:

```text
3초
```

보호 중에는:

```text
적대 효과
→ 차단

이동
점프
→ 허용
```

구조로 설계했다.

반복 부활 시 기존 시간에 누적하는 것이 아니라 새로운 3초 보호 시간을 다시 시작한다.

---

## 35일차 - 정상 도달 및 도착 순서 확정

커밋:

```text
d122cdf42409646448ae5093a32d25675974c75f
```

FINISH Trigger에 접촉한 순간 정상 도달을 확정하도록 구현했다.

저장 정보:

```text
IsFinished
FinishOrder
FinishTime
```

첫 완주:

```text
FinishOrder = 1
```

두 번째 완주:

```text
FinishOrder = 2
```

형태로 고정한다.

완주 Player는:

```text
HeightRankingEligible = false
```

로 전환해 더 이상 실시간 높이 경쟁에 포함하지 않는다.

완주 순위는 이후 높이가 변해도 다시 계산하지 않는다.

---

## 36일차 - 개인 경기 결과 Snapshot

커밋:

```text
eddf83b34895f67764b996bd85c749c0106ed8e9
```

경기 중 여러 시스템에 분산된 Player 기록을 하나의 개인 결과 데이터로 묶었다.

PlayerMatchResult:

```text
PlayerId
FinalRank
IsFinished
FinishOrder
FinishTime
HighestHeight
HighestHeightCentimeters
HighestCheckpoint
```

정상 도달한 Player:

```text
FinalRank = FinishOrder
```

시간 종료 시 미완주 Player:

```text
FinalRank = CurrentRank
```

결과는 생성 순간 값을 복사하는 Snapshot 방식이다.

따라서 결과 생성 후:

```text
Player 위치 변경
Rank 변경
Checkpoint 변경
```

이 발생해도 이미 생성된 결과 데이터는 변경되지 않는다.

---

## 37일차 - 기본 관전 전환 및 FINISH Player 퇴장

커밋:

```text
91ceeff3e7c2b8a5abc0e92a1b894dcef0e57ea8
```

완주 이후 다른 경기 중 Player를 관전하는 최소 구조를 구현했다.

관전 대상:

```text
자기 자신이 아님
IsFinished == false
활성 상태
```

관전 기능:

```text
관전 시작
다음 대상
이전 대상
현재 Target 자동 갱신
관전 종료
```

카메라 Target은 다른 Player로 변경하지만 입력은 Local PlayerInput을 유지한다.

따라서:

```text
관전 카메라 Look
→ Local 입력 사용

관전 대상 이동
→ 권한 변경 없음
```

구조를 유지한다.

---

# 4. FINISH Player 퇴장 처리 보강

37일차 테스트 과정에서 FINISH에 접촉한 뒤에도 이동 입력과 기존 Rigidbody 속도가 남아 Player가 계속 달려가는 문제가 확인되었다.

이를 수정해 FINISH 확정 후:

```text
PlayerCameraRelativeMovement 비활성화
PlayerLedgeClimber 비활성화
PlayerLedgeDetector 비활성화
linearVelocity = 0
angularVelocity = 0
detectCollisions = false
isKinematic = true
Collider 비활성화
Animator 비활성화
Renderer 비활성화
```

를 적용한다.

최종 흐름:

```text
FINISH
↓
도착 기록 확정
↓
개인 결과 생성
↓
이동과 물리 정지
↓
캐릭터 경기장에서 사라짐
↓
관전 전환
```

Player GameObject 자체는 Destroy하지 않기 때문에 경기 결과와 Player 식별 데이터는 유지된다.

---

# 5. Phase 3에서 구축된 핵심 시스템

## 맵

```text
1:1:1 Cube Module
6 Faces
6 Direction Sockets
Entrance
Exit
Drop
고정맵 수동 조립
```

## 경기

```text
Preparing
Countdown
Playing
Finished
```

## 순위

```text
발 기준 현재 높이
소수점 둘째 자리 기준
공동 순위
정상 도달 순위 고정
```

## 체크포인트

```text
START
CP1
CP2
CP3
CP4
최고 Checkpoint 유지
Checkpoint Skip 허용
```

## 추락·부활

```text
Section Fall Limit
최고 Checkpoint Respawn
위치 / 회전 / 속도 초기화
3초 Respawn Protection
```

## 정상 도달

```text
FINISH Trigger
FinishOrder
FinishTime
중복 Finish 차단
실시간 높이 순위 제외
```

## 결과

```text
PlayerMatchResult Snapshot
FinalRank
Finish 정보
HighestHeight
HighestCheckpoint
```

## 관전

```text
경기 중 Player Target
Previous / Next
Local PlayerInput 유지
완주 Player Gameplay 차단
```

---

# 6. Phase 3의 전체 경기 흐름

현재 구현 구조를 한 판 기준으로 연결하면 다음과 같다.

```text
Scene 진입
↓
Player / Map 준비
↓
Ready 확인
↓
3 - 2 - 1 - 시작!
↓
Playing
↓
실시간 높이 / 순위 계산
↓
CP1
↓
낙하 시 CP1 부활
↓
CP2
↓
CP3
↓
CP4
↓
1000m FINISH
↓
FinishOrder / FinishTime
↓
높이 경쟁 제외
↓
PlayerMatchResult
↓
완주 Player 물리 / 이동 종료
↓
캐릭터 모델 제거 표현
↓
다른 미완주 Player 관전
```

---

# 7. 시스템 간 책임 분리

Phase 3에서는 각 기능을 한 Script에 집중시키지 않고 역할별로 분리했다.

대표 구조:

```text
PlayerHeightTracker
→ 높이

PlayerRankingParticipant
PlayerRankingManager
→ 순위

MatchFlowController
→ 경기 상태 / 카운트다운

MatchTimer
→ 경기 시간

PlayerCheckpointTracker
→ 최고 Checkpoint

PlayerFallTracker
→ 추락 감지

PlayerRespawnController
→ 부활

PlayerRespawnProtection
→ 부활 보호

PlayerFinishState
FinishOrderManager
→ 정상 도달

PlayerMatchResultCollector
→ 개인 결과

SpectatorController
→ 관전
```

이를 통해 이후 네트워크 권한 구조를 적용할 때 각 기능의 판정 책임을 서버 Authority 쪽으로 옮기기 쉽게 만들었다.

---

# 8. 테스트 구조

Phase 3에서는 기능마다 EditMode 테스트와 Manual Scene을 함께 추가했다.

주요 검증 대상:

```text
Height 계산
Competition Ranking
Countdown
Match Timer
Checkpoint
Checkpoint Skip
Fall Limit
Respawn
Respawn Protection
FINISH
Result Snapshot
Spectator
FINISH Departure
```

또한 Editor Setup Tool을 사용해 필요한 테스트 Scene과 컴포넌트를 반복 생성할 수 있도록 구성했다.

---

# 9. 개발 중 해결한 주요 문제

## Checkpoint namespace 충돌

Checkpoint namespace와 Component 이름 충돌로:

```text
CS0118
'Checkpoint' is a namespace but is used like a type
```

오류가 발생했다.

Component alias를 사용해 해결했다.

---

## EditMode Input System 참조 누락

SpectatorController 테스트가:

```text
PlayerInput
UnityEngine.InputSystem
```

을 직접 사용하면서 Test asmdef의 참조가 부족해 컴파일 오류가 발생했다.

수정:

```text
ProjectJ.Tests.EditMode.asmdef

references:
ProjectJ.Runtime
Unity.InputSystem
```

---

## Day37 Camera Rig 부재

Day36 Scene에 PlayerThirdPersonCamera가 존재한다고 가정한 Setup 코드 때문에 Day37 Scene 생성이 실패했다.

수정 후:

```text
기존 Camera Rig가 있으면 재사용
없으면 자동 생성
```

하도록 변경했다.

---

## FINISH 후 계속 이동하는 문제

FINISH 확정만 하고 Player 이동과 Rigidbody를 중단하지 않아 캐릭터가 계속 앞으로 이동했다.

FINISH Departure 처리 추가 후:

```text
이동 기능 종료
Rigidbody 속도 제거
충돌 제거
Renderer 제거
```

하도록 수정했다.

---

# 10. Phase 3 현재 상태

GitHub 기준 25~37일차 구현 커밋이 모두 존재한다.

현재 최신:

```text
37일차 : 기본 관전 전환 및 FINISH 플레이어 퇴장 처리 구현
```

까지 반영되어 있다.

다만 Phase 3 기획상의 마지막 단계인:

```text
38일차
고정 코스 한 판 통합 테스트
```

는 아직 별도 완료 커밋이 없다.

따라서 현재 상태는:

```text
Phase 3 기능 구현
→ 25~37일차 완료

Phase 3 최종 통합 검증
→ 38일차 예정
```

으로 정리한다.

---

# 11. 38일차 최종 완료 기준

수동 조립 Module 고정맵에서 다음 흐름을 개발 빌드로 검증한다.

```text
START
↓
Countdown
↓
CP1
↓
CP2
↓
CP3
↓
CP4
↓
Fall / Respawn
↓
1000m FINISH
↓
Personal Result
↓
Finish Departure
↓
Spectator
```

필수 조건:

```text
Module Exit → Entrance 연결 끊김 없음
진행 불가능 구간 없음
Checkpoint 정상
Respawn 정상
FINISH 정상
Result 정상
Console Error 0
```

그리고 위 한 판을:

```text
최소 5회 연속
```

오류 없이 완료해야 Phase 3를 최종 완료 상태로 판단한다.

---

# 12. Phase 3 결과 요약

Phase 3를 통해 Project J는 단순한 이동 테스트 프로젝트에서 **실제 경기 한 판의 핵심 흐름을 실행할 수 있는 구조**로 확장되었다.

Phase 3 이전:

```text
Player 이동
점프
달리기
앉기
Ledge
Camera
```

Phase 3 이후:

```text
Module 고정맵
↓
경기 준비
↓
카운트다운
↓
높이 경쟁
↓
실시간 순위
↓
Checkpoint
↓
Fall / Respawn
↓
Respawn Protection
↓
FINISH
↓
개인 결과
↓
Player 퇴장
↓
관전
```

다음 Phase에서는 이 기반 위에 **밀치기와 실제 경쟁 장애물**을 추가해 단순 완주 테스트에서 다른 Player와 상호작용하는 경쟁 게임으로 확장한다.
