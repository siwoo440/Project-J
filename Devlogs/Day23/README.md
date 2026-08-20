# 23일차 개발일지 - 카메라 휠 줌 및 Sprint FOV 구현

## 개발 목표

22일차에서 구현한 3인칭 카메라 회전과 벽 가림 방지 시스템을 확장한다.

이번 일차에서는 플레이어가 마우스 휠을 사용해 원하는 카메라 거리를 직접 조절할 수 있도록 하고, 실제 Sprint 상태에 따라 FOV가 부드럽게 변화하도록 구현한다.

기존 Camera Collision은 유지하면서 사용자가 선택한 줌 거리와 실제 Camera Distance를 분리해, 벽이 사라진 뒤 고정 거리 7.5가 아니라 플레이어가 마지막으로 선택한 거리로 복귀하도록 개선한다.

핵심 목표는 다음과 같다.

- 마우스 휠 카메라 줌
- 최소 / 최대 Camera Distance 제한
- 사용자 선택 거리와 실제 Camera Distance 분리
- Camera Collision과 줌 시스템 연동
- 벽 해제 후 사용자 선택 거리로 복귀
- 실제 Sprint 상태 기반 FOV 변경
- Sprint 종료 시 Normal FOV 복귀
- 기존 카메라 회전과 이동 시스템 유지

---

## 주요 구현 내용

### 1. Camera Zoom 거리 범위 추가

기존에는 카메라 거리가 다음 값으로 고정되어 있었다.

```text
Camera Distance = 7.5
```

23일차부터 사용자가 직접 조절할 수 있도록 최소·최대 범위를 추가했다.

```text
Minimum Camera Distance = 3.5
Maximum Camera Distance = 10
Default Camera Distance = 7.5
Zoom Step = 0.75
```

---

## 2. 마우스 휠 줌

`Mouse.current.scroll` 입력을 사용해 카메라 거리를 변경한다.

동작 방향:

```text
마우스 휠 위
→ Camera 가까워짐

마우스 휠 아래
→ Camera 멀어짐
```

한 번의 입력마다 다음 값만큼 이동한다.

```text
Zoom Step = 0.75
```

예:

```text
7.5
→ 6.75
→ 6.0
→ 5.25
```

또는:

```text
7.5
→ 8.25
→ 9.0
→ 9.75
→ 10
```

---

## 3. 줌 거리 Clamp

사용자가 휠을 계속 돌려도 다음 범위를 벗어나지 않는다.

```text
3.5 ≤ Camera Distance ≤ 10
```

따라서 캐릭터에 지나치게 가까이 붙거나 맵을 과도하게 멀리 보는 것을 방지한다.

---

## 4. Desired Distance와 Actual Distance 분리

카메라에는 두 종류의 거리가 존재하도록 구성했다.

```text
Desired Camera Distance
→ 플레이어가 휠로 선택한 거리

Current Camera Distance
→ 현재 화면에 실제 적용되고 있는 거리
```

예:

```text
Desired = 10

벽 때문에 실제 Camera가 4 근처까지 접근
→ Current ≈ 3.85
```

Camera Collision 때문에 실제 카메라 위치가 바뀌어도 플레이어가 선택한 줌 값은 유지한다.

---

## 5. Camera Collision과 줌 연동

22일차의 SphereCast 충돌 처리를 유지한다.

```text
CameraPivot
→ 원하는 Camera 위치 방향으로 SphereCast
→ World / Obstacle 검사
```

사용자가 최대 거리 10을 선택했더라도 중간에 벽이 있으면 해당 벽보다 앞쪽으로 Camera를 이동시킨다.

예:

```text
Desired Distance = 10
Hit Distance = 4
Collision Padding = 0.15

Actual Distance
≈ 3.85
```

---

## 6. 벽 해제 후 선택 거리 복귀

22일차에서는 Camera Collision이 해제되면 기본 거리로 복귀했다.

23일차에서는 사용자가 마지막으로 선택한 거리로 돌아가도록 변경했다.

예:

```text
사용자가 휠로 Distance 10 선택
↓
벽 접근
↓
Camera Distance 약 3.85
↓
벽 제거
↓
Camera가 다시 10까지 복귀
```

이 과정은 기존 `Camera Return Speed`를 사용해 부드럽게 처리한다.

```text
Camera Return Speed = 12
```

---

## 7. Sprint FOV 추가

일반 이동과 Sprint 상태의 FOV를 분리했다.

기본값:

```text
Normal FOV = 60
Sprint FOV = 68
```

일반 상태:

```text
FOV 60
```

실제 Sprint 상태:

```text
FOV 60
→ 부드럽게 68
```

Sprint 종료:

```text
FOV 68
→ 부드럽게 60
```

---

## 8. 실제 IsSprinting 상태 사용

Sprint FOV는 단순한 Shift 입력을 기준으로 하지 않는다.

기존 `PlayerCameraRelativeMovement.IsSprinting`을 읽어 실제 Sprint가 활성화된 경우에만 FOV를 변경한다.

따라서 다음 상황에서는 Normal FOV를 유지한다.

```text
Shift만 누른 상태
이동 입력 없음
Stamina 부족
Crouch 상태
Sprint 불가 상태
```

---

## 9. FOV 전환 속도

FOV가 즉시 변경되면 화면이 갑자기 튀는 느낌이 생길 수 있으므로 `Mathf.MoveTowards`를 사용한다.

기본값:

```text
FOV Change Speed = 8
```

현재 FOV에서 Target FOV까지 프레임마다 부드럽게 접근한다.

---

## 10. 기존 카메라 기능 유지

23일차의 기능은 22일차 카메라 구조 위에 추가된다.

기존 기능:

```text
Mouse Look
Yaw 회전
Pitch 회전
Pitch Clamp
Player 추적
Cursor Lock
Camera Collision
벽 접근 시 Camera 당김
벽 해제 후 부드러운 복귀
```

새 기능:

```text
Mouse Wheel Zoom
Min / Max Distance
Sprint FOV
```

---

## 11. 기존 플레이어 이동 시스템 연동

`PlayerCameraRelativeMovement`는 수정하지 않았다.

카메라 쪽에서 기존 Player의 `IsSprinting` 값을 읽기 때문에 Walk, Sprint, Crouch, Jump 등 기존 이동 로직을 그대로 유지한다.

최종 구조:

```text
PlayerCameraRelativeMovement
→ 이동과 Sprint 상태 담당

PlayerThirdPersonCamera
→ 회전
→ 줌
→ 벽 충돌
→ FOV
```

---

## 주요 설정값

```text
Mouse Sensitivity       = 0.15
Min Pitch               = -45
Max Pitch               = 70
Target Height           = 1.5

Camera Distance         = 7.5
Minimum Camera Distance = 3.5
Maximum Camera Distance = 10
Zoom Step               = 0.75

Collision Radius        = 0.25
Collision Padding       = 0.15
Camera Return Speed     = 12
Collision Layers        = World | Obstacle

Normal FOV              = 60
Sprint FOV              = 68
FOV Change Speed        = 8
```

---

## Editor Setup 도구

23일차 설정을 기존 Camera Rig에 적용하기 위한 Editor 메뉴를 추가했다.

```text
ProjectJ
→ Day23
→ Apply Camera Zoom And Sprint FOV
```

이 메뉴는 기존:

```text
=== Day22 Camera Rig ===
```

를 찾아 다음 값을 자동 적용한다.

```text
Camera Distance         7.5
Minimum Distance        3.5
Maximum Distance        10
Zoom Step               0.75

Collision Radius        0.25
Collision Padding       0.15
Camera Return Speed     12

Normal FOV              60
Sprint FOV              68
FOV Change Speed        8

Collision Layers
World | Obstacle
```

---

## 자동 테스트 수정

테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/
└─ PlayerThirdPersonCameraTests.cs
```

23일차에서 다음 테스트를 추가·정리했다.

- Look 입력에 따른 Yaw 변화
- Look 입력에 따른 Pitch 변화
- 최소 Pitch Clamp
- 최대 Pitch Clamp
- 휠 위 입력 시 Camera Distance 감소
- 휠 아래 입력 시 Camera Distance 증가
- Minimum Distance 이하로 내려가지 않음
- Maximum Distance 이상으로 올라가지 않음
- 벽이 없으면 Desired Distance 유지
- 벽이 있으면 Zoom Distance보다 Camera Collision 우선
- 매우 가까운 벽에서도 Camera Distance 음수 방지
- 일반 상태에서 Normal FOV 선택
- Sprint 상태에서 Sprint FOV 선택
- FOV가 Target 값으로 점진적으로 이동
- Player 위치 기반 Camera Rig 위치 계산

---

## 변경 파일

### 생성

```text
Assets/ProjectJ/Editor/
└─ Day23CameraPolishSetup.cs
```

### 수정

```text
Assets/ProjectJ/Runtime/Camera/
└─ PlayerThirdPersonCamera.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlayerThirdPersonCameraTests.cs

Assets/ProjectJ/Tests/Manual/Day11/
└─ Day11_MovementTest.unity
```

### 삭제

```text
없음
```

---

## 수동 테스트 항목

### Mouse Wheel Zoom

- [ ] 휠 위로 Camera가 가까워짐
- [ ] 휠 아래로 Camera가 멀어짐
- [ ] Minimum Distance 3.5 이하로 내려가지 않음
- [ ] Maximum Distance 10 이상으로 올라가지 않음
- [ ] Zoom Step 0.75 적용 확인

### Zoom + Camera Collision

- [ ] Distance 10 상태에서 벽 접근 시 Camera가 자동으로 앞으로 이동
- [ ] Camera가 벽을 뚫지 않음
- [ ] 벽이 사라지면 사용자가 선택한 Distance로 복귀
- [ ] 벽 근처에서 휠 조작 가능
- [ ] 좁은 공간에서 심한 Camera 떨림 없음

### Sprint FOV

- [ ] 일반 상태에서 FOV 60
- [ ] 실제 Sprint 시작 시 FOV가 68로 이동
- [ ] Sprint 종료 시 FOV가 60으로 복귀
- [ ] Shift만 누르고 이동하지 않으면 FOV 60 유지
- [ ] Stamina가 없으면 Sprint FOV 미적용
- [ ] Crouch 중 Sprint FOV 미적용
- [ ] FOV 전환이 갑자기 튀지 않고 부드러움

### 기존 기능 회귀

- [ ] 카메라 Yaw 정상
- [ ] 카메라 Pitch 정상
- [ ] Pitch Clamp 정상
- [ ] Camera Collision 정상
- [ ] Camera 기준 WASD 정상
- [ ] Walk 정상
- [ ] Sprint / Stamina 정상
- [ ] Crouch 정상
- [ ] Jump 정상
- [ ] Slope / Step 정상
- [ ] Ledge Detect 정상
- [ ] Ledge Climb 정상

---

## 테스트 체크리스트

- [ ] 기존 EditMode 테스트 전체 Green
- [ ] PlayerThirdPersonCameraTests 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

GitHub 저장소에는 자동 CI 결과가 등록되어 있지 않으므로 위 항목은 Unity 로컬 Test Runner에서 직접 확인한다.

---

## 개발 결과

23일차에서는 22일차의 3인칭 카메라를 실제 플레이에 더 적합하게 확장했다.

플레이어는 마우스 휠로 `3.5 ~ 10` 사이에서 원하는 Camera Distance를 선택할 수 있고, 중간에 벽이 끼면 기존 Camera Collision이 실제 Camera Distance를 자동으로 줄인다.

벽이 사라진 뒤에는 고정 거리로 돌아가는 것이 아니라 플레이어가 마지막으로 선택한 줌 거리까지 다시 부드럽게 복귀한다.

또한 기존 Player의 실제 `IsSprinting` 상태와 연동하여 Sprint 중에는 FOV를 60에서 68로 확대하고, Sprint 종료 후 다시 60으로 복귀하도록 구현했다.

현재 카메라 기능은 다음 단계까지 연결되었다.

```text
Mouse Look
→ Yaw / Pitch
→ Pitch Clamp
→ Camera Wall Collision
→ Mouse Wheel Zoom
→ Sprint FOV
```

다음 24일차에서는 11~23일차에 구현한 플레이어 이동과 카메라 기능 전체를 한 번에 점검하는 **Player Control Regression** 단계로 진행한다.
