# 18일차 개발일지 - 일어서기 공간 검사 및 통합 테스트 맵 리뉴얼

## 개발 목표

17일차에 구현한 Crouch 시스템을 확장해, 낮은 천장 아래에서 플레이어가 강제로 일어서며 Collider가 장애물과 겹치는 문제를 방지한다.

또한 기존에 떨어져 있던 테스트 구역들을 하나의 맵 안에 모아, 이동·Sprint·Crouch·점프·충돌·경사·단차 등 현재까지 구현한 플레이어 기능을 한 공간에서 연속적으로 테스트할 수 있도록 테스트 맵을 리뉴얼한다.

---

## 주요 구현 내용

### 1. Standing Space Check 구현

플레이어가 Crouch 입력을 해제했을 때 즉시 Standing 상태로 복귀하지 않고, 먼저 서 있을 수 있는 공간이 있는지 검사하도록 수정했다.

동작 흐름은 다음과 같다.

```text
Ctrl 유지
→ Crouch 유지

Ctrl 해제
→ Standing 공간 검사

공간 충분
→ Standing

공간 부족
→ Crouch 유지

이후 공간 확보
→ 자동 Standing
```

이를 통해 낮은 천장이나 장애물 아래에서 Collider가 강제로 커지는 문제를 방지했다.

---

### 2. 실제 자세 상태와 입력 상태 분리

기존에는 Crouch 입력 상태가 곧 실제 자세 상태였다.

이번 일차부터는 다음 두 상태를 구분한다.

```text
Crouch Input
실제 IsCrouching 상태
```

따라서 Ctrl을 이미 놓았더라도 머리 위 공간이 부족하다면 실제 플레이어는 계속 Crouch 상태를 유지할 수 있다.

이 구조는 이후 이동 상태 관리와 네트워크 동기화에도 활용할 수 있다.

---

### 3. Standing Capsule 검사

현재 CapsuleCollider의 Standing 크기를 기준으로 서 있는 상태가 차지할 공간을 계산한다.

검사에는 Standing Collider의 월드 좌표와 반지름을 계산하는 로직을 사용하며, 자신의 CapsuleCollider가 검사에 포함되지 않도록 검사 순간 자기 Collider를 제외한다.

Trigger는 Standing을 막는 대상으로 사용하지 않는다.

검사 대상 Layer는 기존 Ground Layer 설정을 사용한다.

```text
World
Obstacle
```

---

### 4. Standing Space Check Padding

Standing 검사 시 충돌 경계에 너무 민감하게 반응하지 않도록 작은 Padding 값을 추가했다.

기본값:

```text
Standing Space Check Padding = 0.02
```

이를 통해 Collider 경계가 아주 미세하게 닿는 상황에서 불필요한 Standing 차단을 줄인다.

---

### 5. Crouch와 Sprint 연동 유지

머리 위 공간이 막혀 강제로 Crouch 상태가 유지되는 동안에도 기존 규칙을 그대로 적용한다.

```text
IsCrouching = true
→ Sprint 불가
→ Sprint Stamina 소비 없음
```

Standing 공간이 확보되어 자동으로 일어서면 다시 정상적인 Sprint 상태로 전환할 수 있다.

---

## 통합 테스트 맵 리뉴얼

기존 Zone들이 서로 멀리 떨어져 있던 테스트 구조를 정리하고, Zone 01~10을 하나의 통합 테스트 맵 안에 배치했다.

전체 맵은 약 60×60 규모의 테스트 공간으로 구성되어 있으며 중앙 건물을 기준으로 각 기능 테스트 구역을 주변에 배치했다.

### Zone 01 - Open Plaza

자유 이동과 기본 조작을 확인하는 시작 광장.

주요 테스트:

```text
기본 이동
회전
가속·감속
```

---

### Zone 02 - Sprint Lane

직선 이동 구역.

주요 테스트:

```text
Sprint
Stamina 소모
Stamina 회복
최대 이동 속도
```

---

### Zone 03 - Diagonal Slalom

여러 Marker 사이를 대각선으로 이동하는 구역.

주요 테스트:

```text
대각선 이동
카메라 기준 이동
방향 전환
```

---

### Zone 04 - Collision Corridor

좁은 복도와 내부 장애물을 배치한 충돌 테스트 구역.

주요 테스트:

```text
벽 충돌
좁은 통로 이동
방향 전환
```

---

### Zone 05 - Crouch Tunnel

낮은 천장이 있는 Crouch 전용 통로.

주요 테스트:

```text
Crouch
Crouch Speed
낮은 통로 통과
```

---

### Zone 06 - Standing Space Lab

낮은 천장과 높은 천장 구역을 연결해 이번 일차 Standing Space Check를 집중적으로 테스트하는 공간.

주요 테스트:

```text
Ctrl 해제
공간 부족 시 Crouch 유지
공간 확보 후 자동 Standing
```

---

### Zone 07 - Ramp And Stairs

경사로와 계단 형태를 배치한 구역.

현재는 이동 테스트와 향후 19일차 Slope / Step 개발을 위한 준비 공간으로 사용한다.

---

### Zone 08 - Jump Blocks

높이가 다른 여러 플랫폼을 배치한 점프 테스트 공간.

주요 테스트:

```text
Jump
Gravity
Coyote Time
Jump Buffer
Air Control
```

---

### Zone 09 - Central Building

맵 중심에 큰 건축물 형태의 구조를 배치했다.

건물 내부와 외부를 이동하면서 여러 조작을 자연스럽게 연결해 시험할 수 있도록 구성했다.

---

### Zone 10 - Upper Deck

중앙 건물 상부의 높은 플랫폼 구역.

상층 접근과 높이 차이가 있는 이동을 테스트하는 공간으로 구성했다.

---

## 맵 연결 구조

Zone 01~10은 서로 떨어진 개별 테스트 공간이 아니라 하나의 맵 안에서 걸어서 이동할 수 있도록 재배치했다.

중앙에는 십자형 연결 통로를 두고, 외곽 Zone과 중앙 건물을 연결했다.

```text
Zone07        Zone06        Zone05
Ramp/Stairs   Headroom      Crouch

Zone08        Zone09/10     Zone04
Jump          Central       Corridor

Zone01        Zone02        Zone03
Plaza         Sprint        Slalom
```

이를 통해 한 Scene에서 플레이어의 현재 이동 기능 전체를 연속적으로 검증할 수 있다.

---

## 생성된 맵 전용 머티리얼

통합 테스트 맵 구분을 위해 전용 머티리얼을 추가했다.

주요 용도:

```text
Floor
Wall
Light
Accent
Route
```

Floor와 Route는 이동 가능한 공간을 구분하고, Accent 오브젝트는 주요 Marker와 테스트 목표 지점을 시각적으로 구분하는 용도로 사용한다.

---

## 자동 테스트 보강

`PlayerCameraRelativeMovementTests.cs`에 Standing Space Check 상태 판정과 Capsule 월드 좌표 계산 관련 테스트를 추가했다.

주요 테스트 항목:

```text
Crouch 입력 유지
→ Crouch 유지

Ctrl 해제 + 공간 부족
→ Crouch 유지

Ctrl 해제 + 공간 충분
→ Standing

Standing 상태 + 입력 없음
→ Standing 유지

Standing Capsule 월드 좌표 계산
→ Height / Radius 계산 검증
```

기존 이동, Sprint, Stamina, Jump, Coyote Time, Jump Buffer, Air Control, Crouch 테스트도 유지한다.

---

## 주요 변경 파일

### 수정

```text
Assets/ProjectJ/Runtime/Player/
└─ PlayerCameraRelativeMovement.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlayerCameraRelativeMovementTests.cs

Assets/ProjectJ/Tests/Manual/Day11/
└─ Day11_MovementTest.unity
```

### 생성

```text
Assets/ProjectJ/Editor/
└─ Day18TestMapEnhancer.cs

Assets/ProjectJ/Art/Generated/
├─ Day18_Generated_Accent.mat
├─ Day18_Generated_Floor.mat
├─ Day18_Generated_Light.mat
├─ Day18_Generated_Route.mat
├─ Day18_Generated_Wall.mat
└─ 테스트 맵 생성 과정에서 만들어진 기타 Day18 머티리얼
```

---

## 수동 테스트 항목

### Standing Space Check

```text
Ctrl
→ 낮은 천장 아래 진입
→ Ctrl 해제
```

확인:

- 낮은 천장 아래에서 Crouch 유지
- Collider가 강제로 커지지 않음
- 플레이어가 천장 때문에 밀려나지 않음

그 상태에서 통로 밖으로 이동한다.

확인:

- Ctrl을 다시 누르지 않아도 자동 Standing
- Collider가 Standing 크기로 정상 복구

---

### 기존 이동 회귀

다음 기능도 함께 확인한다.

```text
기본 이동
Sprint
Stamina
Crouch
Jump
Gravity
Coyote Time
Jump Buffer
Air Control
```

---

### 통합 맵

Zone 01부터 Zone 10까지 직접 이동하면서 다음을 확인한다.

- 구역 간 이동이 끊기지 않음
- 중앙 건물 접근 가능
- Crouch Tunnel 통과 가능
- Standing Space Lab 정상
- Jump Blocks 접근 가능
- Ramp / Stairs 구역 접근 가능
- 맵 Collider에 비정상적인 틈이나 끼임이 없음

---

## 검증 체크리스트

- [ ] Ctrl 입력 중 Crouch 유지
- [ ] Ctrl 해제 + 공간 부족 시 Crouch 유지
- [ ] 공간 확보 후 자동 Standing
- [ ] Standing 시 Collider 정상 복구
- [ ] Crouch 중 Sprint 차단 유지
- [ ] Ground Check 정상
- [ ] Jump 정상
- [ ] Coyote Time 정상
- [ ] Jump Buffer 정상
- [ ] Air Control 정상
- [ ] Sprint / Stamina 정상
- [ ] Zone 01~10 전체 접근 가능
- [ ] EditMode 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

## 개발 결과

18일차에서는 Crouch 시스템의 핵심 예외 상황인 낮은 천장 아래 Standing 문제를 처리했다.

플레이어는 이제 Crouch 입력을 해제하더라도 머리 위 공간이 부족하면 자동으로 앉은 상태를 유지하며, 공간이 확보되는 순간 자연스럽게 Standing 상태로 돌아간다.

또한 기존에 분산되어 있던 테스트 구역을 하나의 통합 맵으로 리뉴얼해, 현재까지 구현한 플레이어 이동 기능을 한 Scene에서 연속적으로 검증할 수 있는 테스트 환경을 마련했다.

다음 19일차에서는 통합 맵의 Ramp와 Stair 구역을 활용해 **Slope / Step 처리**를 구현한다.
