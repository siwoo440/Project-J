# 11일차 개발일지 - 카메라 기준 플레이어 이동 및 테스트 맵 구축

## 오늘의 목표

Project J의 본격적인 플레이어 조작 개발을 시작한다.

WASD 입력을 월드 고정 방향이 아니라
현재 카메라가 바라보는 방향을 기준으로 변환해
플레이어가 화면 기준으로 자연스럽게 이동하도록 구현한다.

또한 이후 플레이어 이동 기능을 지속적으로 검증할 수 있도록
전용 이동 테스트 맵과 플레이어 프리팹을 구축한다.

---

## 구현 내용

### 1. 카메라 기준 이동 시스템 구현

다음 스크립트를 추가했다.

```text
Assets/ProjectJ/Runtime/Player/PlayerCameraRelativeMovement.cs
```

Input System의 `Move` Action에서 `Vector2` 입력을 받아
카메라의 Forward / Right 방향을 기준으로
월드 이동 방향을 계산한다.

이동 처리 흐름:

```text
WASD / Left Stick
↓
Vector2 이동 입력
↓
Camera Forward / Right
↓
Y축 제거
↓
지면 기준 이동 방향 계산
↓
Rigidbody 이동
```

카메라가 위나 아래를 바라보더라도
플레이어가 공중이나 지면 아래 방향으로 움직이지 않도록
카메라 방향의 Y축 성분을 제거했다.

---

### 2. 대각선 이동 속도 정규화

W+D 또는 W+A처럼 두 방향을 동시에 입력해도
직선 이동보다 이동 속도가 빨라지지 않도록 처리했다.

```text
W
→ 기본 이동 속도

W + D
→ 이동 방향만 대각선으로 변경
→ 이동 속도는 동일하게 유지
```

입력 벡터와 최종 이동 방향을 최대 길이 1로 제한해
방향에 따른 속도 차이가 발생하지 않도록 했다.

---

### 3. 플레이어 방향 회전

플레이어가 이동할 때
현재 이동 방향을 바라보도록 회전하도록 구현했다.

플레이어 프리팹에는 앞 방향을 쉽게 확인할 수 있도록
`FacingMarker`를 추가했다.

```text
Player
└─ FacingMarker
```

테스트 중 플레이어가 어떤 방향을 보고 있는지
즉시 확인할 수 있다.

---

## 플레이어 프리팹 구축

다음 프리팹을 생성했다.

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
```

주요 구성:

```text
Player
├─ Capsule Collider
├─ Rigidbody
├─ Player Input
├─ PlayerCameraRelativeMovement
└─ FacingMarker
```

Layer:

```text
Player
```

Rigidbody는 중력을 사용하며
X/Z 회전은 고정해 이동 중 캐릭터가 넘어지지 않도록 구성했다.

---

## Input System 연결

기존에 구축한 다음 Input Action을 사용한다.

```text
Player / Move
```

입력:

```text
Keyboard
W
A
S
D

Gamepad
Left Stick
```

플레이어 Runtime 코드에서 Input System을 직접 사용하기 위해
다음 Assembly Definition도 수정했다.

```text
Assets/ProjectJ/Runtime/ProjectJ.Runtime.asmdef
```

추가 참조:

```text
Unity.InputSystem
```

---

# 이동 테스트 맵 구축

플레이어 이동을 지속적으로 검증할 수 있도록
전용 Manual Test Scene을 생성했다.

```text
Assets/ProjectJ/Tests/Manual/Day11/Day11_MovementTest.unity
```

기존 실제 게임용 `Game.unity` Scene은 수정하지 않고
별도의 테스트 환경으로 분리했다.

---

## 테스트 맵 구조

```text
Directional Light

CameraRig
└─ Main Camera

Player

=== Day11 Test Map ===
├─ BaseFloor
├─ Zone_01_OpenMovement
├─ Zone_02_StraightMovement
├─ Zone_03_DiagonalMovement
├─ Zone_04_CollisionCorridor
└─ Zone_05_FutureTraversal_NotDay11PassCriteria
```

---

## Zone 01 - 기본 이동 구역

```text
Zone_01_OpenMovement
```

넓은 평지에서 다음을 확인하기 위한 공간이다.

- W/S/A/D 이동
- 이동 방향 변경
- 플레이어 회전
- FacingMarker 방향
- 카메라 기준 이동

---

## Zone 02 - 직선 이동 구역

```text
Zone_02_StraightMovement
```

일정 간격의 거리 표시를 배치해
직선 이동을 쉽게 확인할 수 있도록 구성했다.

확인 목적:

- W 또는 S 직선 이동
- 일정한 이동 속도
- 이동 방향 흔들림 여부

---

## Zone 03 - 대각선 이동 구역

```text
Zone_03_DiagonalMovement
```

대각선 표시를 따라 이동하면서
다음 입력을 검증한다.

```text
W + D
W + A
S + D
S + A
```

대각선 이동 속도가 직선보다 빨라지지 않는지 확인한다.

---

## Zone 04 - 충돌 테스트 구역

```text
Zone_04_CollisionCorridor
```

벽과 장애물을 배치해
Player와 Obstacle Layer 사이의 충돌을 확인한다.

구성:

```text
Wall_Left
Wall_Right
Obstacle_Box_01
Obstacle_Box_02
```

확인 목적:

- 플레이어가 벽을 통과하지 않음
- 장애물 통과 방지
- 장애물에 접촉한 상태에서도 입력 정상

---

## Zone 05 - 이후 이동 기능 테스트 구역

```text
Zone_05_FutureTraversal_NotDay11PassCriteria
```

이후 플레이어 이동 개발에서도 동일한 테스트 맵을
재사용할 수 있도록 미리 구조물을 배치했다.

구성:

```text
Future_SmallStep
Future_Ramp
Future_LowCeiling
```

이 구역은 다음 기능 검증에 사용할 예정이다.

```text
단차
경사
앉기
낮은 천장
지형 이동
```

11일차에는 해당 기능을 구현하지 않았으며
이번 일차 완료 조건에도 포함하지 않는다.

---

# 카메라 방향 테스트

`CameraRig`의 Y Rotation을 직접 변경해
카메라 기준 이동이 정상인지 확인할 수 있다.

테스트 값:

```text
0°
90°
180°
270°
```

각 방향에서 `W`를 눌렀을 때
현재 화면에서 보이는 앞쪽 방향으로 이동하는지 확인한다.

카메라 회전이 바뀌면
플레이어의 월드 이동 방향도 함께 변경되지만
사용자 입장에서는 항상:

```text
W = 화면 기준 앞으로
```

동작한다.

---

# 자동 테스트 추가

다음 EditMode 테스트를 추가했다.

```text
Assets/ProjectJ/Tests/EditMode/PlayerCameraRelativeMovementTests.cs
```

검증 내용:

### ForwardInput_UsesCameraForward

카메라가 기본 방향일 때
앞 입력이 Camera Forward 방향과 일치하는지 확인한다.

### RotatedCamera_ChangesWorldMoveDirection

카메라가 회전했을 때
W 입력의 월드 이동 방향도 함께 변경되는지 확인한다.

### CameraTilt_DoesNotCreateVerticalMovement

카메라가 위 또는 아래로 기울어져 있어도
이동 방향 Y값이 0으로 유지되는지 확인한다.

### DiagonalInput_DoesNotExceedUnitLength

대각선 입력 시
최종 이동 방향의 크기가 1을 초과하지 않는지 확인한다.

### ZeroInput_ReturnsZeroDirection

아무 입력이 없을 때
이동 방향이 Vector3.zero인지 확인한다.

---

# 테스트용 Material

테스트 맵 가독성을 높이기 위해
다음 Material을 생성했다.

```text
FacingMarker.mat
Floor.mat
FutureTraversal.mat
Obstacle.mat
Player.mat
Route.mat
```

각 구역과 플레이어를 시각적으로 구분하기 위한
개발용 Material이다.

---

# 임시 Editor 도구 정리

테스트 Scene과 Player Prefab을 자동 생성하기 위해
일회성 Editor Setup 스크립트를 사용했다.

```text
Assets/Editor/Day11MovementTestSetup.cs
```

맵과 프리팹 생성이 완료된 뒤
더 이상 필요하지 않으므로 삭제했다.

따라서 최종 프로젝트에는
실제 Runtime 코드와 테스트 결과물만 남긴다.

---

# 확인 항목

## 플레이어 이동

- [ ] W 입력 시 카메라 기준 앞으로 이동
- [ ] S 입력 시 카메라 기준 뒤로 이동
- [ ] A 입력 시 카메라 기준 왼쪽 이동
- [ ] D 입력 시 카메라 기준 오른쪽 이동
- [ ] W+D 대각선 이동 정상
- [ ] W+A 대각선 이동 정상
- [ ] 대각선 속도가 직선보다 빨라지지 않음

## 카메라 기준 이동

- [ ] CameraRig Y = 0° 정상
- [ ] CameraRig Y = 90° 정상
- [ ] CameraRig Y = 180° 정상
- [ ] CameraRig Y = 270° 정상
- [ ] 카메라 기울기가 이동 Y축에 영향을 주지 않음

## 충돌

- [ ] Player가 World 바닥 위에서 정상 이동
- [ ] Player가 Obstacle 벽을 통과하지 않음
- [ ] Player가 Obstacle Box를 통과하지 않음

## 자동 테스트

- [ ] PlayerCameraRelativeMovementTests 전체 Green
- [ ] 기존 GameDataIdTests 전체 Green
- [ ] 기존 PlayMode 테스트 Green

## 프로젝트 상태

- [ ] Console Error 0
- [ ] 기존 Scene 흐름 정상
- [ ] 임시 Day11MovementTestSetup.cs 삭제
- [ ] Player.prefab 정상
- [ ] Day11_MovementTest.unity 정상

---

# 결과

Project J의 첫 번째 실제 플레이어 조작 기능으로
카메라 기준 이동 시스템을 구현했다.

플레이어는 카메라 회전과 관계없이
항상 화면 기준 WASD 방향으로 이동하며,
대각선 입력 시에도 이동 속도가 증가하지 않는다.

또한 Player Prefab과 전용 이동 테스트 Scene을 구축해
앞으로 추가될 플레이어 기능을
동일한 환경에서 반복 검증할 수 있는 기반을 마련했다.

현재 이동은 입력과 동시에 목표 속도로 변경되는
기본 형태로 구현되어 있다.

다음 12일차에서는
지상 가속과 감속을 추가해
현재의 즉각적인 이동을 자연스러운 이동감으로 발전시킨다.
