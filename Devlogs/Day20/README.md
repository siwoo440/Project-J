# 20일차 개발일지 - Ledge Detect 구현

## 개발 목표

19일차까지 구현한 평지·경사·단차 이동 위에, 플레이어가 앞으로 접근한 구조물이 실제로 올라갈 수 있는 턱인지 판별하는 Ledge Detect 기반을 추가한다.

이번 일차에서는 실제로 턱 위로 올라가는 Climb 동작은 구현하지 않고, 다음 21일차에서 사용할 Ledge 정보를 안정적으로 검출하고 저장하는 단계까지만 진행한다.

핵심 목표는 다음과 같다.

- 플레이어 전방의 벽 감지
- 벽 위쪽 공간 확인
- 턱 윗면 탐색
- 허용 가능한 Ledge 높이 검사
- 턱 윗면 경사 검사
- 턱 위 Standing 공간 검사
- 유효한 Ledge의 위치·Normal·높이 정보 저장
- 기존 이동 시스템과 분리된 독립 Detector 구성

---

## 주요 구현 내용

### 1. PlayerLedgeDetector 분리

기존 `PlayerCameraRelativeMovement`에 Ledge 기능을 직접 추가하지 않고 별도 컴포넌트로 분리했다.

```text
PlayerCameraRelativeMovement
→ 기존 이동 담당

PlayerLedgeDetector
→ 주변 Ledge 감지 담당
```

이를 통해 20일차에서는 감지만 처리하고, 21일차 Ledge Climb에서 Detector의 결과를 재사용할 수 있도록 했다.

---

### 2. Ledge 높이 범위

기본 허용 범위는 다음과 같다.

```text
Min Ledge Height = 0.45
Max Ledge Height = 1.4
```

따라서:

```text
0.45 미만
→ Step Assist 영역
→ Ledge 아님

0.45 ~ 1.4
→ Ledge 후보

1.4 초과
→ 너무 높은 구조물
→ Ledge 아님
```

19일차의 `Max Step Height = 0.4`와 겹치지 않도록 구간을 분리했다.

---

### 3. 전방 벽 검사

플레이어가 바라보는 방향으로 Raycast를 사용해 전방에 실제 구조물이 있는지 검사한다.

기본 검사 거리:

```text
Wall Check Distance = 0.8
```

전방 벽이 없으면 즉시 Ledge 후보에서 제외한다.

---

### 4. 상단 공간 검사

전방에 벽이 있더라도 구조물이 너무 높다면 Ledge로 사용할 수 없다.

플레이어의 `Max Ledge Height`보다 높은 위치에서 다시 Forward Raycast를 수행해 위쪽 공간이 막혀 있는지 확인한다.

```text
하단 검사
→ 벽 있음

상단 검사
→ 공간 비어 있음

조건 만족
→ Top Surface 검사 진행
```

상단까지 벽이 이어져 있으면 올라갈 수 없는 높은 벽으로 판단한다.

---

### 5. Top Surface 탐색

전방 벽이 확인되면 벽 위쪽 위치에서 아래 방향으로 Raycast를 수행해 실제 턱 윗면을 찾는다.

기본 설정:

```text
Top Probe Forward Offset = 0.2
Top Probe Extra Height   = 0.2
```

검출된 Top Surface를 기준으로 실제 Ledge 높이를 계산한다.

---

### 6. Top Surface 경사 검사

턱의 윗면이 너무 가파르면 플레이어가 올라선 뒤 안정적으로 설 수 없으므로 Ledge로 인정하지 않는다.

기본값:

```text
Top Surface Max Angle = 45°
```

따라서:

```text
30° Top
→ 허용

60° Top
→ 거부
```

하도록 구성했다.

---

### 7. Landing Point 계산

Top Surface를 찾은 뒤 벽 바로 위가 아니라 조금 안쪽 위치를 실제 도착 후보 지점으로 계산한다.

기본값:

```text
Landing Forward Offset = 0.35
```

이 위치는 다음 21일차 Ledge Climb에서 최종 이동 지점 계산의 기준으로 사용한다.

---

### 8. 턱 위 Standing 공간 검사

Top Surface가 존재해도 위쪽 공간이 낮은 천장으로 막혀 있다면 안전하게 올라설 수 없다.

따라서 Landing Point 위치에 Standing Capsule이 들어갈 공간이 있는지 `Physics.CheckCapsule`로 검사한다.

기본 여유값:

```text
Landing Clearance Padding = 0.03
```

검사 과정에서는 Player 자신의 CapsuleCollider를 잠시 제외해 자기 자신을 충돌 대상으로 인식하지 않도록 했다.

---

### 9. Ledge 결과 저장

모든 조건을 통과하면 다음 값을 저장한다.

```text
HasLedge
LedgeWallPoint
LedgeTopPoint
LedgeWallNormal
LedgeTopNormal
LedgeHeight
```

이번 일차에서는 이 값만 계산하며 플레이어 위치나 속도는 변경하지 않는다.

---

## Ledge 유효 조건

최종 Ledge 판정은 다음 조건을 모두 만족해야 한다.

```text
전방 벽 있음
상단 공간 비어 있음
Top Surface 존재
높이 범위 정상
Top Surface 경사 정상
Landing 공간 충분
```

하나라도 실패하면:

```text
HasLedge = false
```

로 유지한다.

---

## Gizmo 디버그 표시

Player를 선택하면 Scene View에서 검사 방향을 확인할 수 있도록 Gizmo를 추가했다.

```text
빨간 선
→ 전방 벽 검사

노란 선
→ 상단 공간 검사

초록 점
→ 유효한 Ledge Top Point
```

20일차에서는 실제 Climb보다 검출 정확도가 중요하므로 Scene View에서 감지 위치를 쉽게 확인할 수 있도록 구성했다.

---

## Player Prefab 적용

Player Prefab에 다음 컴포넌트를 추가했다.

```text
PlayerLedgeDetector
```

기본 설정값:

```text
Min Ledge Height           = 0.45
Max Ledge Height           = 1.4
Wall Check Distance        = 0.8
Top Probe Forward Offset   = 0.2
Top Probe Extra Height     = 0.2
Top Surface Max Angle      = 45
Landing Forward Offset     = 0.35
Landing Clearance Padding  = 0.03
```

`Ledge Layers`는 런타임에서 값이 비어 있을 경우 다음으로 자동 보정된다.

```text
World
Obstacle
```

Prefab Inspector에서도 해당 두 Layer를 직접 지정해 저장해두는 것을 권장한다.

---

## Zone 08 테스트 구역 확장

통합 테스트 맵의 다음 구역을 활용했다.

```text
Zone_08_JumpBlocks
```

여기에 20일차 전용 테스트 구조를 추가했다.

### 높이별 Ledge

```text
Ledge_0.3
→ 너무 낮음
→ Ledge 불가

Ledge_0.7
→ Ledge 가능

Ledge_1.0
→ Ledge 가능

Ledge_1.4
→ 최대 허용 높이

Ledge_1.7
→ 너무 높음
→ Ledge 불가
```

### Blocked Headroom

```text
Ledge_1.0
+ Low Ceiling
```

구조를 배치해 높이는 유효하지만 위쪽 Standing 공간이 부족한 경우를 테스트한다.

결과:

```text
HasLedge = false
```

이어야 한다.

### Steep Top

```text
SteepTop_60deg
```

구조를 추가해 Top Surface가 너무 가파른 경우 Ledge로 인정하지 않는지 확인한다.

---

## 자동 테스트 추가

새 테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/
└─ PlayerLedgeDetectorTests.cs
```

주요 테스트 항목:

- 0.3 높이 Ledge 거부
- 0.8 높이 Ledge 허용
- 1.4 높이 경계값 허용
- 1.7 높이 Ledge 거부
- 30° Top Surface 허용
- 60° Top Surface 거부
- 모든 조건 정상 시 Ledge Candidate 허용
- 상단 막힘 시 거부
- Top Surface 없음 시 거부
- 잘못된 높이 시 거부
- 급경사 Top 시 거부
- Landing 공간 부족 시 거부

---

## 변경 파일

### 생성

```text
Assets/ProjectJ/Runtime/Player/
└─ PlayerLedgeDetector.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlayerLedgeDetectorTests.cs

Assets/ProjectJ/Editor/
└─ Day20LedgeMapSetup.cs
```

### 수정

```text
Assets/ProjectJ/Prefabs/Player/
└─ Player.prefab

Assets/ProjectJ/Tests/Manual/Day11/
└─ Day11_MovementTest.unity
```

### 삭제

```text
없음
```

---

## 수동 테스트 항목

### 높이 테스트

- [ ] 0.3 구조물에서 `HasLedge = false`
- [ ] 0.7 구조물에서 `HasLedge = true`
- [ ] 1.0 구조물에서 `HasLedge = true`
- [ ] 1.4 구조물에서 `HasLedge = true`
- [ ] 1.7 구조물에서 `HasLedge = false`

### 공간 테스트

- [ ] BlockedHeadroom에서 Ledge 검출 안 됨
- [ ] 일반 Ledge에서 Landing Point 계산됨

### 경사 테스트

- [ ] 평평한 Top Surface 정상 검출
- [ ] 60° Top Surface 거부

### 기존 시스템 회귀

- [ ] 기본 이동 정상
- [ ] Sprint / Stamina 정상
- [ ] Crouch 정상
- [ ] Standing Space Check 정상
- [ ] Slope 이동 정상
- [ ] Step Assist 정상
- [ ] Jump 정상
- [ ] Coyote Time 정상
- [ ] Jump Buffer 정상
- [ ] Air Control 정상

---

## 테스트 체크리스트

- [ ] 기존 EditMode 테스트 전체 Green
- [ ] PlayerLedgeDetectorTests 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

GitHub 저장소에는 자동 CI 결과가 등록되어 있지 않으므로 위 항목은 Unity 로컬 Test Runner에서 직접 확인한다.

---

## 개발 결과

20일차에서는 플레이어의 이동을 직접 변경하지 않으면서 주변 구조물이 Ledge로 사용할 수 있는지 판별하는 독립 감지 시스템을 구축했다.

이제 플레이어는 전방 벽, 상단 공간, Top Surface, 높이 범위, 표면 경사, Landing 공간을 순서대로 검사하고 유효한 경우 다음 Climb 단계에서 사용할 위치와 방향 정보를 보관한다.

다음 21일차에서는 `PlayerLedgeDetector`가 제공하는 정보를 사용해 **실제 Ledge Climb 동작**을 구현한다.
