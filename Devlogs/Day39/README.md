# 39일차 개발일지 - 플레이어 상호 통과 및 길막 방지

## 1. 개발 목표

39일차의 목표는 여러 플레이어가 같은 고정 코스를 동시에 진행할 때 서로의 몸체 충돌 때문에 길을 막지 않도록 Player 간 물리 충돌 규칙을 정리하는 것이다.

현재 기준 커밋:

```text
211c0e33e5cecd1f6e846d75f53bbc1c03456d62
```

현재 커밋 메시지:

```text
39
```

이번 일차의 핵심 규칙은 다음과 같다.

```text
Player ↔ Player
→ 물리 충돌 무시

Player ↔ World
→ 기존 충돌 유지

Player ↔ Obstacle
→ 기존 충돌 유지

Player ↔ GameplayTrigger
→ 기존 판정 유지
```

Player Collider 자체는 삭제하지 않고 유지한다.

이유는 이후 밀치기 Target 탐색과 각종 Physics Query에서 Player Collider를 계속 사용해야 하기 때문이다.

---

## 2. PlayerCollisionRules 추가

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Player/PlayerCollisionRules.cs
```

역할:

```text
Player Layer 탐색
Player ↔ Player 충돌 무시 적용
현재 Player 상호 충돌 무시 상태 확인
```

Player Layer 이름은 다음 상수로 관리한다.

```text
Player
```

게임 실행 시 별도의 Scene 설정 없이 자동 적용되도록:

```text
RuntimeInitializeOnLoadMethod
BeforeSceneLoad
```

를 사용한다.

따라서 Scene이 로드되기 전에 Player 간 충돌 무시 규칙이 적용된다.

---

## 3. Player 간 물리 충돌 비활성화

핵심 처리:

```text
Physics.IgnoreLayerCollision(
    Player Layer,
    Player Layer,
    true
)
```

즉 모든 Player Layer 오브젝트끼리는 일반적인 Rigidbody / Collider 물리 충돌을 만들지 않는다.

결과적으로:

```text
Player A → Player B 방향으로 이동
↓
서로 몸체가 닿음
↓
밀려나거나 멈추지 않음
↓
서로 통과
```

하도록 한다.

---

## 4. Collider는 유지

이번 구현에서는 다음 요소를 제거하지 않았다.

```text
CapsuleCollider
Rigidbody
Player Layer
```

Player 간 물리 충돌만 무시한다.

따라서 다음과 같은 Physics Query는 계속 사용할 수 있다.

```text
Physics.OverlapSphere
Physics.OverlapCapsule
Physics.Raycast
기타 LayerMask 기반 Player 탐색
```

이는 다음 단계의 밀치기 Target 선택에서 중요하다.

---

## 5. World 및 Obstacle 충돌 유지

Player 간 충돌 무시를 적용하더라도:

```text
Player ↔ World
Player ↔ Obstacle
```

관계에는 직접 변경을 가하지 않는다.

따라서 기존의:

```text
바닥 착지
벽 충돌
경사면 이동
단차 처리
장애물 충돌
```

은 이전과 동일하게 동작해야 한다.

---

## 6. GameplayTrigger 판정 유지

체크포인트와 FINISH 등 Trigger 시스템에 영향을 주지 않도록:

```text
Player ↔ GameplayTrigger
```

충돌 규칙도 변경하지 않는다.

따라서 기존 Phase 3의:

```text
Checkpoint 활성화
FINISH Trigger
기타 Gameplay Trigger
```

판정은 그대로 유지한다.

---

## 7. 길막 방지 목적

이번 시스템의 목적은 경쟁 플레이에서 다른 플레이어가 물리적으로 길을 막는 현상을 방지하는 것이다.

예:

```text
좁은 통로

Player A →
          ← Player B
```

기존 물리 충돌이 활성화되어 있다면 두 Player가 서로 밀어내며 통로를 봉쇄할 수 있다.

39일차 이후에는:

```text
Player A ↔ Player B
→ 서로 통과
```

하므로 좁은 통로나 입구에서 영구적인 길막이 발생하지 않는다.

---

## 8. 향후 밀치기와의 관계

일반 Player 충돌을 제거해도 밀치기 시스템은 별도로 구현한다.

향후 구조:

```text
평상시 Player 접촉
→ 서로 통과

밀치기 입력
↓
Physics Query
↓
전방 Player Collider 검색
↓
Target 선택
↓
별도 Push Force 적용
```

즉:

```text
상시 몸체 충돌
```

과:

```text
게임플레이 밀치기 판정
```

을 분리한다.

이 구조를 통해 평상시 길막은 방지하면서 의도적인 경쟁 상호작용은 유지할 수 있다.

---

## 9. EditMode 테스트 추가

새 테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/PlayerCollisionRulesTests.cs
```

다음 내용을 검증한다.

### Player 간 충돌 무시

```text
PlayerCollisionRules.Apply()
↓
Player ↔ Player Ignore = true
```

인지 확인한다.

### World / Obstacle 규칙 유지

적용 전과 적용 후의:

```text
Player ↔ World
Player ↔ Obstacle
```

Ignore 상태가 동일한지 확인한다.

### GameplayTrigger 규칙 유지

적용 전과 적용 후의:

```text
Player ↔ GameplayTrigger
```

Ignore 상태가 동일한지 확인한다.

### Player Collider Query 유지

Player ↔ Player 충돌 무시 후에도:

```text
Physics.OverlapSphere
```

로 Player Collider를 탐색할 수 있는지 확인한다.

이 테스트는 다음 밀치기 Target 선택 구현을 위한 기반 검증이기도 하다.

---

## 10. 생성 요소

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Player/PlayerCollisionRules.cs
Assets/ProjectJ/Runtime/Player/PlayerCollisionRules.cs.meta
```

새 EditMode 테스트:

```text
Assets/ProjectJ/Tests/EditMode/PlayerCollisionRulesTests.cs
Assets/ProjectJ/Tests/EditMode/PlayerCollisionRulesTests.cs.meta
```

수정 파일:

```text
없음
```

삭제 파일:

```text
없음
```

---

## 11. 수동 테스트 기준

두 개 이상의 Player를 같은 Scene에 배치하고 다음을 확인한다.

```text
두 Player가 정면으로 접근
→ 서로 멈추지 않고 통과

좁은 통로에서 서로 반대 방향으로 이동
→ 영구 길막 없음

같은 위치에서 Player가 겹침
→ 서로 강제로 튕겨나가지 않음

다른 Player와 겹친 상태에서 점프
→ Player 몸체 충돌 때문에 이동이 방해되지 않음

Player가 벽에 접근
→ 벽 충돌 정상

Player가 바닥에 착지
→ 바닥 충돌 정상

Player가 Obstacle에 접근
→ 장애물 충돌 정상

Checkpoint 접촉
→ 정상 활성화

FINISH 접촉
→ 정상 완주 처리
```

---

## 12. 자동 테스트 기준

Unity에서:

```text
Window
→ General
→ Test Runner
→ EditMode
→ Run All
```

을 실행한다.

39일차 테스트에서 확인할 내용:

```text
Player ↔ Player 충돌 무시
Player ↔ World 기존 규칙 유지
Player ↔ Obstacle 기존 규칙 유지
Player ↔ GameplayTrigger 기존 규칙 유지
Player Collider Physics Query 탐색 가능
```

---

## 13. 이번 일차에서 구현하지 않은 기능

39일차에서는 Player 길막 방지까지만 구현한다.

아직 구현하지 않은 기능:

```text
밀치기 Target 선택
밀치기 Force
밀치기 Cooldown
밀치기 서버 판정
다중 Target 선택
외부 힘 누적 규칙
밀치기 연출
```

이후 개발 단계에서 Player Collider를 Physics Query 대상으로 사용해 밀치기 시스템을 연결한다.

---

## 14. 개발 결과

39일차에서는 Player 몸체끼리의 상시 물리 충돌을 제거해 여러 참가자가 좁은 코스에서도 서로 영구적으로 길을 막지 않도록 했다.

최종 구조:

```text
Player ↔ Player
→ 서로 통과

Player ↔ World
→ 충돌

Player ↔ Obstacle
→ 충돌

Player ↔ GameplayTrigger
→ 기존 판정 유지

Player Collider
→ Physics Query에서 계속 사용 가능
```

이를 통해 이후 경쟁 시스템에서 일반 이동과 의도적인 밀치기 판정을 서로 독립적으로 구현할 수 있는 기반을 마련했다.

---

## 15. 검증 체크리스트

- [ ] Unity Console Error 0
- [ ] Player ↔ Player 서로 통과
- [ ] 정면 충돌 시 길막 없음
- [ ] 좁은 통로 동시 통과 가능
- [ ] Player ↔ World 충돌 정상
- [ ] Player ↔ Obstacle 충돌 정상
- [ ] Checkpoint Trigger 정상
- [ ] FINISH Trigger 정상
- [ ] Player Collider Physics Query 탐색 가능
- [ ] EditMode 전체 Green
- [ ] PlayMode 기존 테스트 Green
- [ ] Phase 3 기능 회귀 오류 없음

---

## 16. 저장소 검토 메모

GitHub 최신 커밋에는 39일차의 PlayerCollisionRules와 PlayerCollisionRulesTests가 정상 포함되어 있다.

정적 코드 검토 기준으로 Player 간 충돌 무시 외의 Layer 관계를 직접 변경하지 않고, Collider를 유지한 채 Physics Query 가능 여부까지 테스트하고 있어 39일차 목표와 충돌하는 문제는 확인되지 않았다.

해당 GitHub 커밋에는 별도의 CI 상태가 등록되어 있지 않으므로 최종 완료 판정은 로컬 Unity에서 EditMode / PlayMode 테스트와 Console Error 0을 확인한 결과를 기준으로 한다.
