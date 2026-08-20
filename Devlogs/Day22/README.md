# 22일차 개발일지 - 3인칭 카메라 회전 및 벽 가림 방지 구현

## 개발 목표

21일차까지 구현한 플레이어 이동 시스템을 실제 3인칭 플레이 형태로 사용할 수 있도록 카메라 회전 시스템을 추가한다.

또한 초기 카메라 거리 5가 너무 가까웠고, Player와 Camera 사이에 벽이 들어오면 캐릭터가 가려지는 문제가 있어 카메라 거리를 확대하고 벽 가림 방지 기능까지 함께 적용한다.

이번 일차의 핵심 목표는 다음과 같다.

- 마우스 Look 입력으로 카메라 좌우 회전
- 카메라 상하 Pitch 회전
- Pitch 최소·최대 각도 제한
- Player와 Camera 회전 분리
- 카메라 기준 WASD 이동 유지
- Cursor Lock / Hide 처리
- 기본 Camera Distance 확대
- Player와 Camera 사이 장애물 감지
- 벽이 끼면 Camera를 Player 쪽으로 자동 이동
- 장애물이 사라지면 원래 거리까지 부드럽게 복귀

---

## 주요 구현 내용

### 1. PlayerThirdPersonCamera 추가

새로운 카메라 전용 런타임 스크립트를 추가했다.

```text
PlayerThirdPersonCamera
```

이 스크립트는 Player 이동과 분리되어 다음 기능을 담당한다.

```text
Look 입력
Yaw 회전
Pitch 회전
Pitch Clamp
Player 위치 추적
Camera Distance
Camera Collision
Cursor Lock
```

Player 자체의 Transform에 카메라 회전을 직접 적용하지 않도록 구성했다.

---

## 2. 독립적인 Camera Rig 구조

Scene에 다음 구조를 사용한다.

```text
=== Day22 Camera Rig ===
└─ CameraPivot
   └─ Main Camera
```

역할은 다음과 같다.

```text
Camera Rig
→ Player 위치 추적
→ Yaw 회전

CameraPivot
→ Pitch 회전

Main Camera
→ 실제 화면 출력
→ 거리 조절
```

Camera Rig를 Player의 자식으로 두지 않아 카메라만 돌릴 때 Player가 함께 회전하지 않도록 했다.

---

## 3. Look 입력 연결

기존 Input System의 `Look` Action을 그대로 사용한다.

새 Input Action은 추가하지 않았다.

```text
Mouse X
→ Yaw

Mouse Y
→ Pitch
```

기본 감도:

```text
Mouse Sensitivity = 0.15
```

---

## 4. Yaw 회전

마우스를 좌우로 움직이면 Camera Rig가 Player 주변을 자유롭게 회전한다.

Yaw는 Signed Angle 범위로 정규화한다.

```text
-180° ~ 180°
```

따라서 360° 이상 계속 회전해도 값이 계속 커지지 않도록 했다.

---

## 5. Pitch 회전 및 제한

CameraPivot을 이용해 상하 시점을 회전한다.

기본값:

```text
Min Pitch = -45°
Max Pitch = 70°
```

이를 통해 카메라가 Player 위를 넘어 뒤집히는 것을 방지한다.

---

## 6. Player 추적 높이

카메라 Rig는 Player Transform 위치를 그대로 따라가지 않고 약간 높은 위치를 기준으로 추적한다.

기본값:

```text
Target Height = 1.5
```

따라서 Player의 몸 중심보다 조금 높은 위치를 카메라 회전 기준점으로 사용한다.

---

## 7. Camera Distance 확대

초기 22일차 카메라 거리는 다음과 같았다.

```text
Camera Distance = 5
```

실제 플레이에서 캐릭터와 주변 지형이 너무 가깝게 보여 다음으로 수정했다.

```text
Camera Distance = 7.5
```

이를 통해 플레이어 주변의 장애물과 이동 경로를 더 넓게 볼 수 있도록 했다.

---

## 8. 카메라 벽 가림 방지

Player와 Camera 사이에 벽이 존재하면 기존 방식에서는 카메라가 벽 뒤에 그대로 남아 캐릭터가 보이지 않는 문제가 있었다.

이를 해결하기 위해 CameraPivot에서 원하는 Camera 위치까지 `Physics.SphereCast`를 수행한다.

```text
CameraPivot
↓
원하는 Camera 방향으로 SphereCast
↓
World / Obstacle 검사
```

장애물이 없으면:

```text
Camera Distance = 7.5
```

를 유지한다.

장애물이 있으면:

```text
Hit Distance - Collision Padding
```

위치까지 Camera를 앞으로 이동한다.

이를 통해 Camera가 벽 뒤에 남지 않고 Player와 벽 사이로 이동하여 캐릭터를 계속 볼 수 있게 했다.

---

## 9. SphereCast 설정

기본값:

```text
Collision Radius    = 0.25
Collision Padding   = 0.15
```

Raycast가 아니라 SphereCast를 사용해 얇은 모서리나 벽 가장자리에서도 카메라 충돌을 조금 더 안정적으로 감지하도록 했다.

Collision Layer는 다음을 사용한다.

```text
World
Obstacle
```

Trigger는 무시한다.

---

## 10. 장애물 접근 시 즉시 Camera 이동

카메라와 Player 사이에 장애물이 들어오면 Camera Distance를 즉시 줄인다.

```text
정상 상태
Player -------- Camera

벽 접근
Player -- Camera | Wall
```

카메라가 벽 내부를 지나가는 시간을 최소화하기 위해 앞으로 이동하는 경우에는 보간 없이 즉시 허용 거리로 변경한다.

---

## 11. 장애물 제거 후 부드러운 복귀

벽에서 벗어난 뒤 Camera가 즉시 7.5 거리로 튀어 나가면 화면이 불안정하게 보일 수 있다.

따라서 Camera가 뒤로 복귀할 때는 `Mathf.MoveTowards`를 사용한다.

기본값:

```text
Camera Return Speed = 12
```

동작 흐름:

```text
벽 접근
→ Camera 즉시 앞으로 이동

벽 해제
→ 원래 Camera Distance까지 부드럽게 복귀
```

---

## 12. 기존 이동 시스템과 연동

기존 `PlayerCameraRelativeMovement`는 Main Camera 방향을 기준으로 이동 방향을 계산하는 구조를 유지한다.

따라서 카메라가 회전하면 별도의 이동 코드 변경 없이 다음과 같이 동작한다.

```text
Camera Forward
↓
W 입력 방향
```

예:

```text
Camera가 북쪽을 바라봄
+ W
→ 북쪽 이동

Camera가 동쪽으로 회전
+ W
→ 동쪽 이동
```

Player는 카메라 회전만으로는 회전하지 않고 실제 이동 방향이 발생할 때 기존 방식으로 방향을 변경한다.

---

## 13. Cursor Lock

Play 시작 시 마우스가 Game View 밖으로 빠져나가지 않도록 다음 상태로 변경한다.

```text
Cursor.lockState = Locked
Cursor.visible = false
```

카메라 컴포넌트가 비활성화되면 Cursor 상태를 다시 복구한다.

---

## Editor Setup 도구

카메라 구조를 빠르게 구성하기 위한 Editor 메뉴를 추가했다.

```text
ProjectJ
→ Day22
→ Setup Third Person Camera
```

이 메뉴는 다음 작업을 수행한다.

```text
Player 검색
PlayerInput 검색
Main Camera 검색
Camera Rig 생성 또는 재사용
CameraPivot 생성 또는 재사용
Main Camera를 Pivot 자식으로 이동
Player / Input / Camera 자동 연결
Camera Distance = 7.5
Collision Radius = 0.25
Collision Padding = 0.15
Camera Return Speed = 12
Collision Layers = World | Obstacle
```

재실행해도 기존 Camera Rig를 찾아 설정을 갱신하도록 구성했다.

---

## 자동 테스트 추가

새 테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/
└─ PlayerThirdPersonCameraTests.cs
```

주요 테스트 항목:

- 오른쪽 Look 입력 시 Yaw 증가
- 위쪽 Look 입력 시 Pitch 변화
- 최소 Pitch Clamp
- 최대 Pitch Clamp
- Yaw Signed Angle Wrap
- Camera Rig 높이 계산
- 장애물이 없을 때 기본 거리 유지
- 벽 감지 시 Camera Distance 감소
- 매우 가까운 벽에서도 거리 음수 방지
- Hit Distance가 기본 거리보다 멀면 최대 Camera Distance로 제한

---

## 변경 파일

### 생성

```text
Assets/ProjectJ/Runtime/Camera/
└─ PlayerThirdPersonCamera.cs

Assets/ProjectJ/Editor/
└─ Day22CameraSetup.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlayerThirdPersonCameraTests.cs
```

### 수정

```text
Assets/ProjectJ/Tests/Manual/Day11/
└─ Day11_MovementTest.unity
```

### 삭제

```text
없음
```

---

## 주요 설정값

```text
Mouse Sensitivity   = 0.15
Min Pitch           = -45
Max Pitch           = 70
Target Height       = 1.5
Camera Distance     = 7.5
Collision Radius    = 0.25
Collision Padding   = 0.15
Camera Return Speed = 12
Collision Layers    = World | Obstacle
Lock Cursor On Play = true
```

---

## 수동 테스트 항목

### 기본 카메라

- [ ] 마우스 좌우 회전 정상
- [ ] 360° Yaw 회전 정상
- [ ] 마우스 상하 회전 정상
- [ ] Pitch -45° 제한 정상
- [ ] Pitch 70° 제한 정상
- [ ] Player 정지 상태에서 Camera만 회전
- [ ] Camera Distance 7.5 적용

### 카메라 기준 이동

- [ ] Camera를 돌린 뒤 W가 Camera Forward 기준으로 이동
- [ ] A / S / D 방향 정상
- [ ] Player는 실제 이동 시에만 방향 회전

### 벽 가림

- [ ] Player와 Camera 사이에 벽이 들어오면 Camera가 앞으로 이동
- [ ] Camera가 벽 뒤에 남지 않음
- [ ] Player가 계속 화면에 보임
- [ ] 벽 가장자리에서도 충돌 반응
- [ ] 벽에서 벗어나면 7.5 거리로 부드럽게 복귀
- [ ] Camera가 벽 내부에서 심하게 흔들리지 않음

### 기존 기능 회귀

- [ ] Walk 정상
- [ ] Sprint / Stamina 정상
- [ ] Crouch 정상
- [ ] Jump 정상
- [ ] Slope 정상
- [ ] Step Assist 정상
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

22일차에서는 프로젝트의 기본 3인칭 카메라 시스템을 구축했다.

카메라는 Player와 독립적인 Rig를 사용해 마우스 입력으로 Yaw / Pitch 회전하며, 기존 이동 시스템은 Main Camera 방향을 기준으로 계속 동작한다.

초기 Camera Distance를 5에서 7.5로 확대해 주변 시야를 넓혔으며, Player와 Camera 사이에 World 또는 Obstacle이 들어오면 SphereCast로 이를 감지해 Camera가 자동으로 Player 쪽으로 이동하도록 개선했다.

장애물이 사라지면 Camera는 원래 거리까지 부드럽게 복귀한다.

현재 기본 이동 계열과 카메라 계열은 다음 단계까지 연결되었다.

```text
Walk / Sprint / Crouch
→ Jump / Slope / Step
→ Ledge Detect / Climb
→ Third Person Camera Rotate
→ Camera Wall Collision
```

다음 일차에서는 22일차에서 일부 선행 구현된 Camera Collision을 정리하고, 남은 **Camera FOV 및 카메라 사용감 보정**을 진행한다.
