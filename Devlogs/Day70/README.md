# Project J - 70일차 개발 일지

## 개발 기준

69일차 완료 커밋:

```text
edbf6731d66b78674223000a510f958569894f4b
69일차 : External Force·State Authority Push·Checkpoint 네트워크화 및 타입 충돌 수정
```

70일차 작업 커밋:

```text
16962c8dce30ea4f6c19968419088d442a547f27
70
```

이번 일차에서는 69일차에 저장한 Checkpoint와 External Force 상태를 실제 부활 시스템과 경쟁 상태에 연결했다.

```text
State Authority Respawn
+
3초 Respawn Protection
+
현재 높이 동기화
+
실시간 순위 동기화
```

---

## 70일차 목표

플레이어의 부활과 경쟁 진행 상태를 Client가 직접 확정하지 않고 State Authority가 판정하도록 구성한다.

전체 흐름:

```text
낙하 또는 직접 부활 요청
↓
State Authority 판정
↓
최고 Checkpoint Respawn 위치로 순간이동
↓
Vertical / External Velocity 초기화
↓
3초 Respawn Protection 시작
↓
현재 발 높이 계산
↓
실시간 순위 계산
↓
Networked 상태로 동기화
```

---

## 1. Network Respawn 상태 추가

다음 Networked 상태를 추가했다.

```text
NetworkRespawnProtectionTimer
NetworkRespawnCount
NetworkLastRespawnReason
```

부활 원인은 다음과 같이 구분한다.

```text
None
Fall
Manual
```

이를 통해 낙하 부활과 직접 부활을 동일한 State Authority 처리 흐름으로 관리할 수 있다.

---

## 2. State Authority 기반 낙하 부활

기존 Checkpoint별 낙하 한계 데이터인 `CheckpointFallLimitSet`을 재사용한다.

현재 Player의 최고 Checkpoint를 기준으로 낙하 한계를 조회하고, Player의 World Y가 해당 값보다 낮아지면 State Authority가 부활을 실행한다.

```text
현재 Checkpoint
↓
CheckpointFallLimitSet
↓
Fall Limit Y 조회
↓
Player Y < Fall Limit Y
↓
State Authority Respawn
```

Client가 자신의 위치를 직접 되돌리지 않는다.

---

## 3. 직접 부활 요청 구현

70일차 테스트용 직접 부활 입력으로 `R` 키를 추가했다.

Host 자신의 Player는 State Authority에서 즉시 부활하고, Client는 RPC를 통해 State Authority에 부활을 요청한다.

```text
Input Authority
↓
RPC_RequestManualRespawn()
↓
State Authority
↓
PerformRespawn()
```

실제 게임의 ESC 메뉴 부활 버튼은 이후 `RequestManualRespawn()` 호출로 연결할 수 있다.

---

## 4. NetworkTransform Teleport 부활

부활 시 단순 Transform 이동이 아니라 `NetworkTransform.Teleport()`를 사용한다.

```text
NetworkRespawnPosition
NetworkRespawnRotation
↓
NetworkTransform.Teleport()
```

이를 통해 부활 위치 이동이 일반 이동 보간처럼 보이지 않고 네트워크 순간이동으로 처리된다.

NetworkTransform이 존재하지 않는 예외 상황에는 `SetPositionAndRotation()`으로 보정한다.

---

## 5. 부활 시 이동 상태 초기화

`ProjectJNetworkPlayer`에 다음 부활 전용 초기화 기능을 추가했다.

```text
ResetMotionForRespawn()
```

초기화 대상:

```text
NetworkVerticalVelocity = 0
NetworkGrounded = false
Prediction 기준 위치 갱신
Simulation 기준 위치 갱신
```

또한 External Gameplay에서는 부활 시:

```text
NetworkExternalVelocity = Vector3.zero
NetworkLastExternalForceSource = None
```

으로 이전 Push나 장애물 외력이 부활 이후까지 남지 않도록 처리한다.

---

## 6. 3초 Respawn Protection 구현

부활 직후 Fusion `TickTimer`를 사용하여 3초 보호를 시작한다.

```text
Respawn
↓
TickTimer.CreateFromSeconds(
    Runner,
    3f
)
↓
3초 보호
```

보호 상태는 모든 Peer가 동일한 Networked 상태를 확인할 수 있다.

확인 값:

```text
IsRespawnProtected
RespawnProtectionRemaining
```

---

## 7. 적대적 External Force 차단

부활 보호 중에는 모든 외력을 막는 것이 아니라 적대적 외력만 차단한다.

차단:

```text
Push
Item
```

허용:

```text
AirBag
Obstacle
```

따라서 부활 직후 Player 간 방해는 막지만 맵 장애물의 기본 물리 규칙은 유지한다.

---

## 8. Push 보호 결과 추가

Push 결과 상태에 다음 값을 추가했다.

```text
Protected
```

보호 중인 Player를 Push 대상으로 선택하면:

```text
Target 선택
↓
Respawn Protection 확인
↓
Protected
↓
External Force 적용 안 함
```

으로 처리한다.

기존 Push 결과:

```text
None
Success
Miss
Cooldown
Invalid
Protected
```

---

## 9. 현재 발 높이 Networked 처리

현재 경쟁 높이를 다음 Networked 값으로 저장한다.

```text
NetworkRaceHeight
```

기준은 Player 루트 Transform의 World Y이며 현재 Network Player 구조에서 루트가 발 위치 기준이다.

높이는 소수점 둘째 자리까지만 남기고 이후 값은 버린다.

예:

```text
125.678
↓
125.67
```

계산 방식:

```text
Floor(Y × 100) / 100
```

플레이어가 아래로 이동하면 높이 값도 다시 감소한다.

---

## 10. 실시간 공동 순위 구현

현재 경쟁 순위를 다음 Networked 값으로 저장한다.

```text
NetworkRaceRank
```

순위 계산 방식:

```text
자신보다 높은 Player 수 + 1
```

예:

```text
100.00 → 1위
90.00  → 2위
90.00  → 2위
80.00  → 4위
```

동일한 0.00 단위 높이는 같은 순위가 된다.

---

## 11. Active Player Registry 재사용

69일차 Push Target 판정에 사용한 `ActivePlayers` Registry를 70일차 순위 계산에도 재사용한다.

이를 통해 별도 Ranking Manager를 추가하지 않고 현재 단계에서 연결된 Network Player들의 높이를 비교할 수 있도록 했다.

---

## 12. 70일차 디버그 표시

기존 69일차 디버그 표시를 70일차 상태에 맞게 확장했다.

확인 가능한 주요 값:

```text
External Velocity
External Force Source

Push Result
Push Target
Push Cooldown

Checkpoint
Respawn Position

Respawn Count
Respawn Reason
Protection Remaining

Race Height
Race Rank
```

이를 통해 Host와 Client에서 부활과 경쟁 상태를 빠르게 확인할 수 있다.

---

## 기존 기능 유지 확인

`ProjectJNetworkPlayer` 수정 이후에도 기존 네트워크 이동 기능을 유지한다.

```text
WASD 이동
Jump
Gravity
Ground 판정
Sprint
Stamina
Crouch
Standing Clearance
CapsuleCollider 변경
Prediction
Resimulation
Remote Interpolation 진단
Local Presentation 연결
```

70일차에서는 기존 기능을 제거하지 않고 `ResetMotionForRespawn()`만 추가하여 부활과 연결했다.

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJExternalForceSource.cs
├─ ProjectJNetworkExternalGameplay.cs
└─ ProjectJNetworkPlayer.cs
```

---

## 생성 파일

```text
없음
```

---

## 삭제 파일

```text
없음
```

---

## 테스트 항목

```text
Host + Client 접속

CP1 접촉
↓
CP1 기준 Fall Limit 아래로 낙하
↓
CP1 Respawn Position으로 순간이동

부활 직후
Vertical Velocity = 0
External Velocity = 0

부활 후 3초
Push 차단
Push Result = Protected

3초 종료 후
Push 정상 적용

부활 보호 중
Item Force 차단
AirBag Force 허용
Obstacle Force 허용

R 직접 부활
Host 정상
Client RPC 정상

높이 125.678
→ 125.67

높이가 내려가면
Race Height 감소

100 / 90 / 90 / 80
→ Rank 1 / 2 / 2 / 4

Host / Client
Respawn / Protection / Height / Rank 동일

기존 Jump / Sprint / Crouch 정상
Prediction / Resimulation 유지
Console Error 0
```

---

## 70일차 완료 내용

```text
Checkpoint Fall Limit
↓
State Authority Fall Respawn
↓
Manual Respawn RPC
↓
NetworkTransform Teleport
↓
Vertical / External Velocity 초기화
↓
3초 Respawn Protection
↓
Push / Item Force 차단
↓
Obstacle Force 유지
↓
발 기준 Height 0.00 단위 동기화
↓
공동 Rank 계산
↓
Host / Client 상태 동기화
```

---

## 다음 개발 방향

71일차에서는 경기 진행 자체를 State Authority 기준으로 확장한다.

```text
3초 경기 시작 Countdown
↓
Networked Match Timer
↓
FINISH Trigger
↓
도착 순서 확정
↓
개인 최종 순위 고정
↓
Result 동기화
```

70일차까지는 실시간 높이 경쟁 상태를 관리하고, 71일차부터는 FINISH한 Player를 실시간 높이 경쟁에서 제외하고 도착 순서 기반 최종 순위를 확정하는 단계로 넘어간다.
