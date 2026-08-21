# 42일차 개발일지 - 동시 밀치기 힘 합산 및 외부 힘 감속

## 1. 개발 목표

42일차의 목표는 여러 Player에게서 거의 동시에 들어오는 밀치기 힘을 하나의 외부 힘 상태로 합산하고, 밀치기 이후 캐릭터가 얼음 위처럼 계속 미끄러지는 현상을 방지하는 것이다.

현재 기준 커밋:

```text
2a6cbc0343a808f83e95e45f499013439aa7a4d4
```

현재 커밋 메시지:

```text
42
```

이번 일차의 핵심 구조는 다음과 같다.

```text
여러 Push 요청
↓
PlayerExternalForceAccumulator
↓
수평 외부 속도 합산
↓
Rigidbody에 적용
↓
시간에 따라 외부 속도 감속
↓
정지
```

---

## 2. PlayerExternalForceAccumulator 추가

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Push/
└─ PlayerExternalForceAccumulator.cs
```

주요 역할:

```text
외부 수평 속도 누적
여러 밀치기 힘 합산
외부 속도 감속
정지 임계값 처리
기존 Y 속도 보존
외부 속도 초기화
```

기본 설정값:

```text
Horizontal Decay = 12
Stop Threshold = 0.05
```

`DefaultExecutionOrder(100)`을 사용해 일반적인 Player 이동 처리 이후 외부 힘 감속이 적용될 수 있도록 구성했다.

---

## 3. 동시·연속 밀치기 힘 합산

각 밀치기 요청은 다음처럼 외부 수평 속도에 더해진다.

예:

```text
Push A = (4, 0, 0)
Push B = (0, 0, 3)
```

결과:

```text
External Velocity = (4, 0, 3)
```

반대 방향 힘이 들어오면 서로 상쇄될 수 있다.

```text
(6, 0, 0)
+
(-6, 0, 0)
=
(0, 0, 0)
```

이를 통해 여러 Player가 같은 Target을 밀칠 때 마지막 힘 하나만 남는 것이 아니라 각 방향의 힘이 함께 반영된다.

---

## 4. 수직 속도 제외

밀치기 외부 힘은 X/Z 평면에서만 처리한다.

```text
밀치기
→ X / Z 변경

Y
→ 기존 Rigidbody 속도 유지
```

따라서 이전에 발생했던 캐릭터가 하늘로 튀어 오르는 문제를 다시 만들지 않도록 했다.

점프 중인 Player의 경우:

```text
기존 Y 속도
→ 유지

밀치기 수평 속도
→ 별도로 추가
```

된다.

---

## 5. 밀치기 미끄러짐 수정

기존 문제:

```text
밀치기
↓
수평 속도 증가
↓
Rigidbody에 속도가 오래 남음
↓
얼음 위처럼 계속 미끄러짐
```

42일차에서는 외부 힘으로 추가된 수평 속도를 별도로 추적하고 `Horizontal Decay` 값에 따라 감속한다.

기본값:

```text
Horizontal Decay = 12
```

결과:

```text
밀치기
→ 강하게 이동
→ 빠르게 감속
→ 정지
```

전체 Rigidbody의 Linear Damping을 올리는 방식은 사용하지 않았다.

이유는 전체 Damping을 높이면 일반 이동, 점프, 공중 제어까지 영향을 받을 수 있기 때문이다.

대신 밀치기에서 발생한 외부 속도만 따로 감속한다.

---

## 6. 충돌 후 역방향 튕김 방지

외부 힘 감속 과정에서 Target이 벽 등에 부딪혀 이미 속도가 0이 되었을 때 감속 보정값 때문에 반대 방향으로 튕겨나가지 않도록 처리했다.

```text
밀치기
↓
벽 충돌
↓
현재 수평 속도 0
↓
감속 보정
↓
역방향 속도 생성 금지
```

현재 Rigidbody 속도와 외부 힘 방향의 내적을 이용해 실제로 제거할 수 있는 만큼만 감속한다.

---

## 7. PlayerPushReceiver 수정

수정 파일:

```text
Assets/ProjectJ/Runtime/Push/
└─ PlayerPushReceiver.cs
```

기존에는 Push Receiver가 Rigidbody 속도를 직접 변경했다.

42일차 이후:

```text
PlayerPushReceiver
↓
PlayerExternalForceAccumulator
↓
AddVelocityChange()
```

구조로 변경했다.

또 `PlayerExternalForceAccumulator`를 필수 컴포넌트로 지정해 Receiver와 외부 힘 처리기가 함께 존재하도록 했다.

---

## 8. 밀치기 힘 2배 조정

41일차 기본 밀치기 힘:

```text
Horizontal Velocity Change = 6
```

42일차에서 테스트 결과를 바탕으로 약 2배인:

```text
Horizontal Velocity Change = 12
```

로 조정했다.

수직 밀치기 값:

```text
Upward Velocity Change = 0
```

은 유지한다.

현재 기본 밀치기 설정:

```text
Horizontal Velocity Change = 12
Upward Velocity Change = 0
Cooldown Duration = 1.5
```

---

## 9. Day42 Setup Tool 추가

새 Editor 파일:

```text
Assets/ProjectJ/Editor/
└─ Day42ExternalForceSetup.cs
```

Unity 메뉴:

```text
ProjectJ
→ Day42
→ Setup External Force Accumulator
```

실행 시:

```text
Player.prefab
↓
PlayerExternalForceAccumulator 추가 또는 재사용
↓
Horizontal Decay = 12
Stop Threshold = 0.05
↓
PlayerPushController
Horizontal Velocity Change = 12
Upward Velocity Change = 0
↓
Prefab 저장
```

을 자동으로 처리한다.

---

## 10. Player Prefab 수정

수정 파일:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
```

추가:

```text
PlayerExternalForceAccumulator
```

현재 설정:

```text
Horizontal Decay = 12
Stop Threshold = 0.05
```

또한 Push Controller 설정을 다음과 같이 정리했다.

```text
Horizontal Velocity Change = 12
Upward Velocity Change = 0
Cooldown Duration = 1.5
```

---

## 11. 자동 테스트 추가

새 테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/
└─ PlayerExternalForceAccumulatorTests.cs
```

검증 항목:

```text
여러 외부 속도 합산
반대 방향 힘 상쇄
Y 성분 무시
감속 후 외부 속도 감소
충분한 시간이 지나면 완전 정지
벽 충돌처럼 Body 속도가 이미 0인 경우 역방향 튕김 없음
외부 힘 초기화 시 기존 Y 속도 유지
```

기존 `PlayerPushControllerTests`도 밀치기 힘을 `12` 기준으로 수정했다.

---

## 12. 생성·수정·삭제 요소

### 생성

```text
Assets/ProjectJ/Runtime/Push/
└─ PlayerExternalForceAccumulator.cs

Assets/ProjectJ/Editor/
└─ Day42ExternalForceSetup.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlayerExternalForceAccumulatorTests.cs
```

각 `.meta` 파일 포함.

### 수정

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab

Assets/ProjectJ/Runtime/Push/
├─ PlayerPushController.cs
└─ PlayerPushReceiver.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlayerPushControllerTests.cs
```

### 삭제

```text
없음
```

---

## 13. 현재 Push 처리 흐름

42일차 종료 기준:

```text
Push Input
↓
PlayerPushController
↓
Cooldown 검사
↓
PlayerPushTargetSelector
↓
최근접 유효 Player 선택
↓
PlayerPushReceiver
↓
Finish / Respawn Protection 검사
↓
PlayerExternalForceAccumulator
↓
수평 외부 속도 합산
↓
Rigidbody에 적용
↓
외부 속도 감속
↓
정지
```

---

## 14. Phase 4 테스트맵 검증

Phase 4 전용 Scene:

```text
Assets/ProjectJ/Tests/Manual/Phase4/
└─ Phase4_InteractionTest.unity
```

주요 확인 장소:

```text
PUSH / COOLDOWN
PUSH EDGE / DROP
RESPAWN PROTECTED
```

수동 확인 내용:

```text
밀치기 힘이 이전보다 강하게 적용되는지
Target이 하늘로 뜨지 않는지
Target이 밀린 뒤 자연스럽게 멈추는지
낭떠러지 근처에서 수평으로 밀리는지
Respawn Protection 중 밀치기가 거부되는지
여러 밀치기 방향이 합산되는지
```

---

## 15. 이번 일차에서 구현하지 않은 기능

42일차에서는 Player Push 기반 외부 힘 합산과 감속까지만 구현한다.

아직 구현하지 않은 기능:

```text
네트워크 서버 권한 기반 외부 힘 판정
외부 힘 RPC / Snapshot 동기화
장애물 Force 통합
아이템 Force 통합
폭발 Force 통합
밀치기 애니메이션
밀치기 효과음
밀치기 기록 통계
```

이후 장애물이나 아이템에서도 동일한 외부 힘 구조를 사용할 수 있지만 현재 단계에서는 미리 연결하지 않는다.

---

## 16. 수동 테스트 체크리스트

- [ ] Unity Console Error 0
- [ ] 밀치기 Horizontal Velocity Change = 12
- [ ] Upward Velocity Change = 0
- [ ] 한 번 밀치면 Target이 충분히 밀림
- [ ] Target이 하늘로 솟아오르지 않음
- [ ] 밀린 후 얼음 위처럼 계속 미끄러지지 않음
- [ ] 밀치기 후 자연스럽게 감속 및 정지
- [ ] 벽 충돌 후 반대 방향으로 튕기지 않음
- [ ] 점프 중 기존 Y 속도 유지
- [ ] 같은 방향 Push가 합산됨
- [ ] 반대 방향 Push가 상쇄됨
- [ ] Respawn Protection Target은 Push 거부
- [ ] 기존 1.5초 Push Cooldown 정상
- [ ] Phase 4 테스트맵 회귀 오류 없음

---

## 17. 자동 테스트 체크리스트

Unity:

```text
Window
→ General
→ Test Runner
→ EditMode
→ Run All
```

확인:

- [ ] `PlayerExternalForceAccumulatorTests` 전체 Green
- [ ] `PlayerPushControllerTests` 전체 Green
- [ ] `PlayerPushTargetSelectorTests` 전체 Green
- [ ] `PlayerCollisionRulesTests` 전체 Green
- [ ] 기존 EditMode 테스트 전체 Green
- [ ] 기존 PlayMode 테스트 Green

---

## 18. 개발 결과

42일차에서는 밀치기 물리 구조를 단순한 Rigidbody 속도 직접 변경 방식에서 외부 힘을 별도로 추적하는 구조로 확장했다.

최종 결과:

```text
여러 밀치기
→ 수평 힘 합산

수직 힘
→ 추가하지 않음

밀치기 강도
→ 6에서 12로 증가

밀치기 후 속도
→ Horizontal Decay 12로 감속

작은 잔여 속도
→ Stop Threshold 0.05 이하에서 정지
```

이를 통해 강한 경쟁 상호작용은 유지하면서 캐릭터가 과도하게 떠오르거나 바닥에서 계속 미끄러지는 현상을 줄였다.

---

## 19. 저장소 검토 메모

GitHub 최신 커밋에는 42일차의 `PlayerExternalForceAccumulator`, Setup Tool, Player Prefab 변경, Push Receiver 연결, 밀치기 힘 12 조정 및 EditMode 테스트가 포함되어 있다.

정적 코드 검토 기준으로 현재 42일차 목표 진행을 막는 문제는 확인되지 않았다.

다만 GitHub 최신 커밋에는 별도의 CI 상태가 등록되어 있지 않다.

따라서 최종 완료 판정은 로컬 Unity에서:

```text
Console Error 0
EditMode 전체 통과
PlayMode 회귀 테스트 통과
Phase 4 수동 밀치기 테스트 통과
```

를 확인한 결과를 기준으로 한다.
