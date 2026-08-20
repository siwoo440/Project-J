# 17일차 개발일지 - 앉기 충돌체·이동 구현

## 개발 목표

플레이어가 앉기 입력을 사용해 충돌체 높이를 낮추고, 낮아진 자세에 맞는 이동 속도로 움직일 수 있도록 앉기 시스템을 구현한다.

이번 일차에서는 다음 기능을 목표로 진행했다.

- Crouch 입력 연결
- CapsuleCollider 기반 앉기 충돌체 처리
- 앉기 중 이동 속도 감소
- 앉기 중 Sprint 차단
- 앉기 전후 캐릭터 발 위치 유지
- 기존 이동·점프·공중 제어·Sprint·Stamina 시스템과의 연동
- EditMode 테스트 보강

---

## 수정 파일

- `Assets/ProjectJ/Runtime/Player/PlayerCameraRelativeMovement.cs`
- `Assets/ProjectJ/Tests/EditMode/PlayerCameraRelativeMovementTests.cs`

---

## 구현 내용

### 1. CapsuleCollider 기반 앉기 처리

플레이어 이동 컴포넌트가 `CapsuleCollider`를 필수로 사용하도록 구성했다.

게임 시작 시 서 있는 상태의 Collider 높이와 중심 위치를 저장하고, 앉기 상태에서는 Collider 높이를 줄이도록 구현했다.

기본 앉기 높이는 다음과 같다.

- Standing Height: 기존 CapsuleCollider 값 사용
- Crouch Height: `1.2`
- 최소 높이: CapsuleCollider 반지름의 2배 이상

이를 통해 Collider가 비정상적으로 작아지는 상황을 방지한다.

---

### 2. 앉기 입력 연결

Input System의 `Crouch` 액션을 플레이어 이동 스크립트에 연결했다.

입력이 유지되는 동안 앉기 상태가 활성화되고, 입력을 해제하면 다시 서 있는 상태로 돌아간다.

현재 단계에서는 단순히 입력 상태에 따라 앉기/서기를 전환한다.

머리 위에 장애물이 있을 때 일어서기를 제한하는 기능은 다음 일차에서 구현한다.

---

### 3. 발 위치를 유지하는 Collider 중심 보정

Collider의 높이만 줄이면 캐릭터의 바닥 위치가 함께 변할 수 있기 때문에, 높이 차이만큼 Collider 중심을 아래쪽으로 이동시키도록 보정했다.

`CalculateCrouchCenterY()`를 통해 다음 관계가 유지되도록 구현했다.

```text
서 있을 때 Collider 바닥 위치
=
앉았을 때 Collider 바닥 위치
```

따라서 앉기 전환 과정에서 캐릭터가 공중으로 뜨거나 바닥 아래로 내려가지 않도록 했다.

---

### 4. 앉기 이동 속도

앉은 상태에서 지상 이동 시 별도의 이동 속도를 사용하도록 했다.

기본 수치는 다음과 같다.

- 기본 이동 속도: `6`
- 달리기 속도: `9`
- 앉기 이동 속도: `3.5`

이동 속도 선택 우선순위는 다음과 같다.

```text
앉기 상태
→ Crouch Speed

달리기 상태
→ Sprint Speed

그 외
→ Normal Move Speed
```

공중에서는 앉기 이동 속도를 별도로 강제하지 않고 기존 공중 이동 제어를 유지한다.

---

### 5. 앉기 중 Sprint 차단

앉은 상태에서는 Sprint가 시작되지 않도록 `CanSprint()` 조건에 앉기 상태를 추가했다.

Sprint 조건은 다음과 같다.

- Sprint 입력 중
- 이동 입력 존재
- 지상 상태
- Stamina가 0보다 큼
- Exhausted 상태가 아님
- 앉기 상태가 아님

따라서 Ctrl을 누른 채 이동해도 달리기로 전환되지 않는다.

---

## 테스트 보강

EditMode 테스트에 앉기 관련 검증을 추가했다.

주요 검증 항목은 다음과 같다.

- 앉기 상태에서 Sprint가 시작되지 않는지 확인
- 앉기 상태에서 Crouch Speed가 선택되는지 확인
- Collider 높이가 줄어도 바닥 위치가 유지되는지 확인
- 높이가 변하지 않을 경우 Collider 중심도 변하지 않는지 확인
- 대각선 앉기 이동이 설정된 Crouch Speed를 초과하지 않는지 확인

기존 Sprint 관련 테스트도 변경된 메서드 인자에 맞춰 유지했다.

---

## 수동 확인 항목

Unity PlayMode에서 다음 항목을 확인한다.

1. Ctrl을 누르면 플레이어가 앉는다.
2. Ctrl을 놓으면 다시 선다.
3. 앉을 때 CapsuleCollider 높이가 줄어든다.
4. 앉기 전후 플레이어의 발 위치가 움직이지 않는다.
5. 앉은 상태의 이동 속도가 일반 이동보다 느리다.
6. 앉은 상태에서는 Shift를 눌러도 Sprint가 발동하지 않는다.
7. 앉은 상태에서도 점프 입력이 정상적으로 처리된다.
8. 점프 후 착지했을 때 기존 이동·Stamina 시스템이 정상적으로 유지된다.
9. EditMode Test Runner가 통과한다.
10. PlayMode Test Runner가 통과한다.
11. Console Error가 0건인지 확인한다.

---

## 현재 제한 사항

이번 일차에서는 머리 위 공간을 검사하지 않는다.

따라서 낮은 천장 아래에서 Crouch 입력을 해제하면 Collider가 즉시 원래 높이로 복구된다.

이 문제는 다음 일차인 **18일차 - 일어서기 공간 검사**에서 처리한다.

---

## 개발 결과

17일차 기준으로 다음 플레이어 조작 기반이 연결되었다.

```text
카메라 기준 이동
→ 지상 가속·감속
→ 점프·중력
→ 코요테 타임·점프 버퍼
→ 공중 제어
→ Sprint·Stamina
→ Crouch
```

다음 일차에서는 낮은 천장과 장애물 아래에서 플레이어가 강제로 일어서며 충돌하지 않도록 **일어서기 공간 검사**를 구현한다.
