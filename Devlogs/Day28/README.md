# 프로젝트 J 28일차 개발 일지

## 개발 목표

부활 직후 플레이어가 다른 플레이어의 충돌과 밀치기로 인해 다시 추락하는 문제를 방지하기 위해 3초간의 부활 보호 기능을 구현했다.

## 구현 내용

### 1. 부활 보호 시스템

- 부활 위치 이동이 끝난 시점부터 3초간 보호 상태 적용
- 보호 시작 시 `Player` 레이어를 `RespawnProtection` 레이어로 변경
- 보호 종료 시 기존 `Player` 레이어로 자동 복구
- 보호 남은 시간과 활성 상태를 외부에서 확인할 수 있도록 구성
- 연속 부활 시 기존 보호 상태를 정리하고 보호 시간을 3초로 재시작
- 경기 종료 시 남아 있는 보호 상태 즉시 해제

### 2. 부활 중 충돌 처리

- 부활 암전과 위치 이동 중 `CharacterController` 비활성화
- 마지막 체크포인트의 `RespawnPoint` 위치와 회전 적용
- 이동 완료 후 `CharacterController` 재활성화
- 부활 과정에서 플레이어가 다른 충돌체에 걸리거나 밀리는 현상 방지

### 3. 보호 중 플레이어 조작

- WASD 이동 허용
- Shift 달리기 허용
- Ctrl 앉기 허용
- Space 점프 허용
- 경사·계단·모서리 이동 유지
- 끝자락 올라오기 유지
- 체크포인트와 상호작용 기능 유지

### 4. 플레이어 충돌과 밀치기 차단

- 보호 중인 플레이어를 밀치기 후보에서 제외
- 보호 대상에게 플레이어 밀치기 외부 힘이 적용되지 않도록 처리
- 보호 중 대상 외곽선 표시 차단
- 다른 플레이어의 몸체를 통과하도록 3D Physics 충돌 행렬 수정
- 보호 종료 후 기존 밀치기와 연속 피격 면역 기능 복구

### 5. 장애물과 이동 발판 유지

- 보호 중에도 바닥과 벽 충돌 유지
- 이동 발판 전달 속도 유지
- 장애물에서 발생하는 외부 힘 허용
- 체크포인트 Trigger와 아이템 상호작용 유지
- 플레이어가 발생시킨 밀치기만 선택적으로 차단

### 6. 3D Physics 충돌 행렬

`Edit → Project Settings → Physics → Layer Collision Matrix`에서 다음 충돌을 비활성화했다.

| 레이어 조합 | 설정 |
|---|---|
| Player ↔ Player | 비활성화 |
| Player ↔ RespawnProtection | 비활성화 |
| PushHitbox ↔ RespawnProtection | 비활성화 |
| RespawnProtection ↔ RespawnProtection | 비활성화 |

`RespawnProtection`과 바닥·벽·장애물·체크포인트 사이의 충돌은 활성 상태로 유지했다.

## 추가 및 수정 파일

### 신규 파일

- `Assets/_ProjectJ/Scripts/Runtime/Player/Respawn/PlayerRespawnProtectionController.cs`
- `Assets/_ProjectJ/Scripts/Runtime/Player/Respawn/RespawnProtectionRules.cs`
- `Assets/_ProjectJ/Tests/EditMode/RespawnProtectionRulesTests.cs`

### 수정 파일

- `Assets/_ProjectJ/Scripts/Runtime/Player/Respawn/PlayerRespawnController.cs`
- `Assets/_ProjectJ/Scripts/Runtime/Player/Forces/ExternalForceReceiver.cs`
- `Assets/_ProjectJ/Scripts/Runtime/Player/Forces/PlayerExternalForceController.cs`
- `Assets/_ProjectJ/Scripts/Runtime/Player/Interaction/PlayerPushController.cs`
- `Assets/_ProjectJ/Scenes/Game/Game.unity`
- `ProjectSettings/DynamicsManager.asset`

## 테스트 내용

- 부활 완료 후 3초간 보호 상태 유지 확인
- 보호 시작과 종료 시 레이어 자동 전환 확인
- 보호 중 이동·달리기·앉기·점프 확인
- 보호 중 다른 플레이어 몸체 통과 확인
- 보호 중 플레이어 밀치기 무효 확인
- 보호 종료 후 밀치기 정상 복구 확인
- 장애물 외부 힘과 이동 발판 속도 유지 확인
- 연속 부활 시 보호 시간 정상 초기화 확인
- 경기 종료 시 보호 상태 정리 확인
- 기존 체크포인트·추락 부활·ESC 직접 부활 기능 회귀 확인

## EditMode 테스트

`RespawnProtectionRulesTests`에 다음 6개 테스트를 추가했다.

- `NegativeDurationClampsToZero`
- `ThreeSecondDurationRemainsAtStart`
- `ElapsedTimeReducesRemainingDuration`
- `RemainingDurationNeverBecomesNegative`
- `PositiveRemainingTimeMeansProtected`
- `ZeroRemainingTimeMeansUnprotected`

## 완료 결과

- 부활 이동 중 충돌 비활성화 완료
- 부활 완료 후 3초 보호 적용 완료
- 보호 중 플레이어 조작 허용 완료
- 보호 중 다른 플레이어 충돌과 밀치기 차단 완료
- 보호 중 장애물과 이동 발판 기능 유지 완료
- 보호 종료 후 정상 레이어와 밀치기 기능 복구 완료
- 3D Physics 충돌 행렬 설정 완료
- 체크포인트 좌표와 정상 지점 좌표 기존 설정 유지

## 커밋 제목

```text
28일차 : 부활 후 3초 보호 및 플레이어 충돌·밀치기 차단
```
