# Project J - 67일차 개발 일지

## 개발 목표

66일차까지 구현한 Networked 이동, 점프·중력, Sprint·Stamina 구조를 유지하면서 Crouch 상태를 Fusion Tick Simulation에 연결하고, 실제 Network Player의 CapsuleCollider 높이와 Visual 높이를 함께 변경한다.

이번 일차의 핵심 흐름은 다음과 같다.

```text
Crouch Input
↓
Networked Crouch 상태
↓
CapsuleCollider 높이 변경
↓
Visual 높이 변경
↓
낮은 공간 이동
↓
Crouch 해제
↓
Standing 공간 검사
↓
공간이 있으면 일어서기
공간이 없으면 Crouch 유지
```

기존 Local Prediction, Resimulation, Sprint·Stamina, 점프·중력, Remote NetworkTransform Interpolation 구조는 그대로 유지한다.

---

## 주요 개발 내용

### 1. Network Player CapsuleCollider 추가

기존 Network Player Prefab Root에는 실제 몸통 Collider가 없었기 때문에 67일차에서 Root에 `CapsuleCollider`를 추가했다.

Standing 기본값:

```text
Height
2.0

Radius
0.4

Center Y
1.0
```

Player Root는 발 위치 기준을 유지한다.

---

### 2. Prefab 자동 검증 구조 확장

`ProjectJNetworkPlayerPrefabBuilder`가 기존 Prefab을 삭제하고 다시 만드는 방식 대신, 현재 Prefab을 검사하고 필요한 설정만 보정하도록 변경했다.

메뉴:

```text
Tools
→ Project J
→ Fusion
→ 67일차 Network Player Prefab 검증
```

기존 Prefab에 CapsuleCollider가 없다면 자동으로 추가하고 다음 값을 맞춘다.

```text
Direction = Y
Radius = 0.4
Height = 2.0
Center = (0, 1, 0)
Is Trigger = FALSE
```

Visual의 기존 Collider는 제거된 상태를 유지한다.

---

## Networked Crouch 상태

### 3. Crouch 상태 추가

Crouch 상태를 Fusion Tick 상태로 관리하기 위해 다음 값을 추가했다.

```text
NetworkIsCrouching
```

외부 진단용 속성:

```text
IsCrouching
```

기존 NetworkInput에서 이미 전달하고 있던 Crouch 입력을 실제 Player 상태에 연결했다.

---

### 4. Crouch Collider 높이 변경

Standing 상태:

```text
Height = 2.0
Center Y = 1.0
```

Crouch 상태:

```text
Height = 1.0
Center Y = 0.5
```

발 위치 기준 Root는 그대로 유지하고 머리 쪽 높이만 줄어드는 구조다.

사용 메서드:

```text
ApplyColliderPosture()
```

---

## Crouch Visual 처리

### 5. Visual Capsule 높이 변경

실제 Collider뿐 아니라 화면에 보이는 Capsule도 Crouch 상태에 맞춰 변경한다.

Standing:

```text
Visual Position Y = 1.0
Visual Scale Y = 1.0
```

Crouch:

```text
Visual Position Y = 0.5
Visual Scale Y = 0.5
```

사용 메서드:

```text
ApplyCrouchPresentation()
```

Remote Player 역시 Networked Crouch 상태를 기준으로 동일하게 표시된다.

---

## Standing 공간 검사

### 6. 천장 충돌 검사 추가

Crouch 입력을 해제했다고 바로 Standing 상태로 복귀하지 않고, Standing 높이로 돌아갈 공간이 있는지 먼저 검사한다.

사용 메서드:

```text
HasStandingClearance()
```

처리 흐름:

```text
Crouch Input 해제
↓
Standing 공간 검사
↓
장애물 없음
→ Standing

장애물 있음
→ Crouch 유지
```

낮은 통로 안에서 Collider가 천장과 겹친 상태로 강제로 커지는 것을 방지한다.

---

### 7. Can Stand 진단 추가

외부에서 현재 일어설 수 있는지 확인할 수 있도록 다음 속성을 추가했다.

```text
CanStandUp
```

예:

```text
낮은 천장 아래
→ FALSE

낮은 공간 밖
→ TRUE
```

---

## Ground 판정 보정

### 8. 자기 Collider Ground 판정 제외

67일차부터 Root CapsuleCollider가 추가되었기 때문에 기존 Ground Raycast가 자기 자신의 Collider를 Ground로 판단하지 않도록 수정했다.

기존 단일 Raycast 대신 여러 Hit를 검사한 뒤 자신의 Collider를 제외한다.

사용 구조:

```text
RaycastNonAlloc
↓
모든 Hit 확인
↓
자기 Root Collider 제외
↓
자식 Collider 제외
↓
가장 가까운 외부 Ground 사용
```

사용 메서드:

```text
TryFindGroundHit()
IsOwnCollider()
```

---

### 9. 착지 Ground 검사도 동일한 방식으로 변경

기존 낙하 구간 Ground 검사 역시 동일한 자기 Collider 제외 구조를 사용한다.

```text
Current Y
↓
Next Y
↓
RaycastNonAlloc
↓
자기 Collider 제외
↓
외부 Ground 발견
↓
Landing Height 적용
```

점프·중력 구조 자체는 65일차 방식 그대로 유지한다.

---

## Crouch와 Sprint 연결

### 10. Crouch 중 Sprint 차단

Sprint 조건에 다음 조건을 추가했다.

```text
IsCrouching = FALSE
```

따라서:

```text
W + Shift
→ Sprint

Crouch Input
→ Sprint 종료
→ Walk Speed
→ Stamina 회복
```

낮은 통로 때문에 Crouch가 강제로 유지되는 동안에도 Sprint는 활성화되지 않는다.

---

## Crouch와 Jump 연결

### 11. Crouch 중 Jump 차단

점프 조건에 Crouch 상태가 아닌 경우를 추가했다.

```text
Jump Input
+
Grounded
+
Crouching = FALSE
```

따라서 Crouch 상태에서 Space를 눌러도 점프하지 않는다.

Standing으로 정상 복귀한 뒤에는 기존 점프를 다시 사용할 수 있다.

---

## Prediction / Resimulation 유지

### 12. Local Crouch Prediction

Crouch 상태는 `FixedUpdateNetwork()` 안에서 계산한다.

```text
Client Crouch Input
↓
NetworkInput
↓
FixedUpdateNetwork()
↓
Local Prediction
↓
즉시 Crouch
↓
Host 상태 수신
↓
필요 시 Resimulation
```

따라서 Client 자신의 Crouch가 Host 응답을 기다린 뒤 늦게 반응하는 구조가 아니다.

---

## Remote Player 표시 유지

### 13. Remote Crouch 표시

Remote Player도 동기화된 Crouch 상태를 기준으로 Collider와 Visual을 갱신한다.

```text
State Authority Crouch 계산
↓
Networked Crouch 상태
↓
Remote Player 수신
↓
Collider / Visual Crouch 적용
```

기존 NetworkTransform 이동 보간 구조는 그대로 유지한다.

---

## F2 네트워크 디버그 UI 확장

### 14. 67일차 화면 표시

F2 디버그 창 제목을 다음과 같이 변경했다.

```text
Project J - Fusion 67일차
```

진단 섹션:

```text
Crouch · Collider · Sprint / Jump
```

---

### 15. Crouch 진단 정보 추가

Player별 표시 항목:

```text
Player
Role
Crouch
Collider H
Can Stand
Sprint
Stamina
Speed
Ground
Vertical V
Interpolation
```

Standing 상태 예시:

```text
Crouch
-

Collider H
2.00

Can Stand
TRUE
```

Crouch 상태:

```text
Crouch
TRUE

Collider H
1.00
```

낮은 천장 아래:

```text
Crouch
TRUE

Can Stand
-
```

---

### 16. 디버그 Player 표시 8인 복구

이전 디버그 UI에서 최대 6명까지만 보이던 제한을 다시 8명으로 변경했다.

```text
최대 Player 표시
6 → 8
```

행 간격을 줄여 기존 F2 영역 안에서 8명의 상태를 확인할 수 있도록 했다.

---

## 테스트 흐름

### Host 단독 테스트

```text
Standing
→ Collider Height 2.0

Crouch Input
→ Collider Height 1.0
→ Visual 낮아짐

Crouch 해제
→ 공간이 있으면 Standing
```

---

### 낮은 공간 테스트

낮은 천장용 Collider 아래에서 다음 순서로 확인한다.

```text
Crouch
↓
낮은 공간 진입
↓
Crouch Input 해제
↓
Can Stand = FALSE
↓
Crouch 유지
↓
낮은 공간 밖으로 이동
↓
Can Stand = TRUE
↓
Standing 복귀
```

---

### Sprint 결합 테스트

```text
W + Shift
→ Sprint

Crouch Input
→ Sprint 종료
→ Speed 5
→ Stamina 회복
```

Crouch 상태에서 Shift를 계속 눌러도 Sprint가 다시 켜지지 않는지 확인한다.

---

### Jump 결합 테스트

```text
Standing + Space
→ Jump

Crouch + Space
→ Jump 차단
```

Standing으로 복귀한 뒤에는 기존 점프가 정상 동작해야 한다.

---

### Host / Client 테스트

```text
Unity Editor
→ Host

Development Build
→ Client
```

Client에서 Crouch 입력:

```text
Client Local Player
→ 즉시 Crouch

Host 화면
→ Client Crouch 상태 확인
```

반대로 Host가 Crouch할 때 Client 화면에서도 Remote Host Player의 Visual 높이가 변경되는지 확인한다.

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
└─ ProjectJFusionBootstrapDebugView.cs

Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkPlayer.cs
├─ Editor/
│  └─ ProjectJNetworkPlayerPrefabBuilder.cs
└─ Resources/
   └─ ProjectJNetworkPlayer.prefab
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

## 67일차 완료 기준

```text
Crouch Input 실제 상태 연결
↓
Networked Crouch 상태 구현
↓
Root CapsuleCollider 추가
↓
Standing / Crouch Collider 높이 전환
↓
Visual 높이 전환
↓
Standing 공간 검사
↓
낮은 공간에서 Crouch 유지
↓
공간 확보 후 Standing 복귀
↓
자기 Collider Ground 판정 제외
↓
Crouch 중 Sprint 차단
↓
Crouch 중 Jump 차단
↓
Local Prediction 유지
↓
Remote Crouch 표시 유지
↓
F2 최대 8인 진단 복구
```

---

## 최신 커밋 확인

README 작성 시점 최신 `main` 커밋:

```text
7ab86cb0baeeef42d78c1dafb962ab85182055c7
67
```

이 커밋은 66일차 커밋 `748356b68af89467de359aa8d4aabb158fd5fd5a` 바로 다음에 이어지는 1개 커밋이다.

66일차 대비 변경 파일은 다음 네 개다.

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/ProjectJFusionBootstrapDebugView.cs
Assets/ProjectJ/Network/Fusion/Player/Editor/ProjectJNetworkPlayerPrefabBuilder.cs
Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkPlayer.cs
Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkPlayer.prefab
```

Prefab에도 실제 CapsuleCollider 설정이 저장되어 있다.

```text
Radius = 0.4
Height = 2
Direction = Y
Center = (0, 1, 0)
Is Trigger = FALSE
```

GitHub에는 해당 커밋에 대한 CI 상태가 등록되어 있지 않으므로 Unity Compile, Test Runner, Host / Client Runtime 결과는 로컬 실행을 최종 기준으로 확인한다.

---

## 다음 개발 방향

다음 68일차에서는 Network Player 중 Local Input Authority를 가진 Player만 Gameplay Camera와 Local UI를 사용하도록 분리한다.

예상 흐름:

```text
Network Player Spawn
↓
Input Authority 확인
↓
Local Player
→ Gameplay Camera 활성화
→ Local UI 연결

Remote Player
→ Camera 비활성화
→ Local UI 미연결
```

기존 이동, 점프, Sprint, Stamina, Crouch, Prediction, Resimulation, Remote Interpolation 구조를 유지한 채 소유자 전용 표현 계층을 분리한다.
