# 19일차 개발일지 - 경사면·단차 이동 처리 구현

## 개발 목표

18일차까지 구현한 기본 이동, Sprint, Crouch, Jump, Air Control 위에 실제 맵에서 자주 발생하는 경사면과 작은 단차 이동을 추가한다.

이번 일차의 핵심 목표는 다음과 같다.

- 완만한 경사면을 표면을 따라 자연스럽게 이동
- 최대 경사각을 넘어가는 가파른 면은 걷기 가능한 Ground로 취급하지 않음
- 내리막에서 플레이어가 지면에서 순간적으로 뜨는 현상 완화
- 작은 단차를 점프 없이 통과할 수 있도록 Step Assist 추가
- Jump 중에는 Ground Snap과 Step Assist가 개입하지 않도록 제한
- 통합 테스트 맵의 Zone 07을 Slope / Step 검증 구역으로 확장

---

## 주요 구현 내용

### 1. 경사면 감지

플레이어 아래쪽으로 Ground Surface를 검사해 현재 밟고 있는 표면의 Normal과 지면까지의 거리를 얻도록 했다.

검사 결과는 다음 용도로 사용한다.

```text
Surface Normal
→ 경사각 계산
→ 걷기 가능한 경사인지 판정

Ground Gap
→ Ground Snap 필요 여부 판정
```

---

### 2. 최대 경사각 제한

경사면을 무조건 Ground로 취급하지 않고 `Max Slope Angle`을 기준으로 걷기 가능 여부를 판단한다.

기본값:

```text
Max Slope Angle = 45°
```

따라서:

```text
45° 이하
→ Walkable Slope

45° 초과
→ 걷기 가능한 Ground로 취급하지 않음
```

가파른 벽이나 급경사를 평지처럼 걸어 올라가는 것을 방지한다.

---

### 3. 경사면 방향 투영

평지에서 사용하던 이동 방향을 그대로 경사면에 적용하지 않고, 현재 Surface Normal을 기준으로 이동 방향을 표면 위에 투영한다.

```text
카메라 기준 이동 입력
↓
Slope Surface Normal
↓
ProjectDirectionOnSlope
↓
경사면을 따라가는 이동 방향
```

이를 통해 오르막과 내리막에서 플레이어가 표면을 따라 이동하도록 구성했다.

---

### 4. 경사면 이동 속도 계산

새로운 Surface Velocity 계산을 추가했다.

경사면에서도 기존 이동 시스템의 속도 규칙을 유지한다.

```text
Walk Speed = 6
Sprint Speed = 9
Crouch Speed = 3.5
```

경사 방향으로 이동하더라도 설정된 최대 이동 속도를 초과하지 않는다.

---

### 5. Ground Probe 추가

기존 Ground Check 외에 조금 더 아래쪽의 지면을 확인하는 Ground Probe를 추가했다.

기본값:

```text
Ground Probe Distance = 0.6
```

이를 통해 내리막이나 경사 전환 구간에서 CapsuleCollider가 지면에서 아주 조금 떨어졌을 때도 가까운 지면의 존재를 확인할 수 있다.

---

### 6. Ground Snap 구현

내리막을 이동할 때 Rigidbody가 관성 때문에 지면에서 순간적으로 뜨는 현상을 줄이기 위해 Ground Snap을 추가했다.

기본값:

```text
Ground Snap Distance = 0.25
Ground Snap Speed = 4
```

조건을 만족하면 플레이어를 지면 방향으로 붙여준다.

단 다음 상황에서는 Ground Snap을 사용하지 않는다.

```text
Jump 실행 중
상승 중
걷기 불가능한 급경사
지면이 너무 멀리 떨어진 경우
```

따라서 Jump가 Ground Snap 때문에 강제로 취소되지 않도록 구성했다.

---

### 7. 작은 단차 Step Assist 구현

플레이어 전방의 낮은 위치와 높은 위치를 각각 검사한다.

```text
낮은 Probe
→ 앞에 장애물 있음

높은 Probe
→ 공간이 비어 있음

지상 상태
→ Step Assist 가능
```

기본 설정:

```text
Max Step Height = 0.4
Step Check Distance = 0.6
Step Up Speed = 3
```

작은 턱은 위쪽 이동 보조를 적용해 점프 없이 통과하도록 했다.

---

### 8. Step Assist 제한

Step Assist는 다음 상황에서는 사용하지 않는다.

```text
공중 상태
Jump 실행 중
상단 Probe가 막혀 있음
전방 낮은 Probe에 장애물이 없음
상승 중
```

따라서 일반 Jump나 공중 이동에 Step Assist가 잘못 개입하지 않도록 했다.

---

## Zone 07 테스트 구역 확장

통합 테스트 맵의 다음 구역을 사용한다.

```text
Zone_07_RampAndStairs
```

여기에 19일차 전용 테스트 오브젝트를 추가했다.

### Step Course

연속 단차 높이는 다음과 같이 구성했다.

```text
0.2
0.2
0.3
0.4
0.6
```

앞쪽 네 개는 최대 0.4 이하의 통과 가능한 단차 테스트용이며, 마지막 0.6 단차는 자동 Step Assist로 통과하지 않아야 하는 기준용이다.

### Slope Markers

```text
WalkableSlope_30deg
→ 통과 가능한 경사

BlockedSlope_55deg
→ Max Slope Angle을 넘는 급경사
```

으로 구성했다.

---

## 자동 테스트 추가

새 테스트 파일을 추가했다.

```text
Assets/ProjectJ/Tests/EditMode/
└─ PlayerSlopeStepTests.cs
```

주요 테스트 항목은 다음과 같다.

- 30° 경사가 Walkable인지 확인
- 60° 경사가 Walkable이 아닌지 확인
- 경사 투영 이동 방향이 Surface Normal과 평행 조건을 만족하는지 확인
- 경사 이동 속도가 Move Speed를 초과하지 않는지 확인
- Ground Gap 계산 검증
- 가까운 지면에서 Ground Snap 가능 여부 검증
- Jump 중 Ground Snap 차단
- 상승 중 Ground Snap 차단
- 작은 단차 Step Assist 허용
- 상단이 막힌 Step 차단
- Jump 중 Step Assist 차단
- 공중 상태 Step Assist 차단

---

## 변경 파일

### 수정

```text
Assets/ProjectJ/Runtime/Player/
└─ PlayerCameraRelativeMovement.cs

Assets/ProjectJ/Tests/Manual/Day11/
└─ Day11_MovementTest.unity
```

### 생성

```text
Assets/ProjectJ/Editor/
└─ Day19SlopeStepMapSetup.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlayerSlopeStepTests.cs
```

### 삭제

```text
없음
```

---

## 주요 설정값

```text
Max Slope Angle        = 45
Ground Probe Distance  = 0.6
Ground Snap Distance   = 0.25
Ground Snap Speed      = 4
Max Step Height        = 0.4
Step Check Distance    = 0.6
Step Up Speed          = 3
```

기존 이동값은 유지한다.

```text
Move Speed    = 6
Sprint Speed  = 9
Crouch Speed  = 3.5
Jump Velocity = 8
Gravity       = -22
```

---

## 수동 테스트 항목

### 경사면

```text
WalkableSlope_30deg
```

에서 다음을 확인한다.

- [ ] 일반 이동으로 올라갈 수 있음
- [ ] Sprint 상태에서도 자연스럽게 이동
- [ ] 경사면에서 속도가 비정상적으로 증가하지 않음
- [ ] 내리막에서 불필요하게 지면에서 뜨지 않음

### 급경사

```text
BlockedSlope_55deg
```

에서 확인한다.

- [ ] 일반 Ground처럼 처리되지 않음
- [ ] 평지처럼 그대로 걸어 올라가지 않음

### 단차

Step Course에서 확인한다.

- [ ] 0.2 단차 통과
- [ ] 0.3 단차 통과
- [ ] 0.4 단차 통과
- [ ] 0.6 단차 자동 통과 불가

### Jump 회귀

- [ ] 경사면에서 Jump 가능
- [ ] Jump 중 Ground Snap이 적용되지 않음
- [ ] Jump 중 Step Assist가 적용되지 않음
- [ ] 경사 끝에서 Coyote Time 정상
- [ ] Jump Buffer 정상

### 기존 이동 회귀

- [ ] Walk 정상
- [ ] Sprint / Stamina 정상
- [ ] Crouch 정상
- [ ] Standing Space Check 정상
- [ ] Air Control 정상

---

## 테스트 체크리스트

- [ ] 기존 EditMode 테스트 전체 Green
- [ ] PlayerSlopeStepTests 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

GitHub 저장소에는 자동 CI 결과가 등록되어 있지 않으므로 위 항목은 Unity 로컬 Test Runner에서 직접 확인한다.

---

## 개발 결과

19일차에서는 평지 중심이었던 플레이어 이동을 실제 맵 지형에 대응할 수 있도록 확장했다.

플레이어는 이제 걷기 가능한 경사면의 Normal을 기준으로 표면을 따라 이동하며, 내리막에서는 Ground Snap을 통해 지면 접촉을 보조한다.

또한 작은 단차를 점프 없이 통과할 수 있는 Step Assist를 추가하고, 최대 경사각과 최대 단차 높이를 기준으로 자동 이동이 허용되는 범위를 제한했다.

다음 20일차에서는 플레이어 앞쪽의 벽과 상단 공간을 검사해 **Ledge Detect**를 구현한다.
