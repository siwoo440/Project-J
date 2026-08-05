# Project J

3D 3인칭 온라인 수직 점프 경쟁 파티 게임 **Project J**의 개발 기록입니다.

---

## 12일차 : 달리기·앉기·고정 수직 테스트 맵 구현

### 개발 목표

11일차에 구현한 기본 이동·점프·3인칭 카메라를 확장했다. 달리기와 앉기 조작, 스태미나 처리, 낮은 통로 충돌 처리와 조작 검증용 고정 수직 테스트 맵을 구성했다.

---

### 구현 내용

| 기능 | 구현 내용 |
| --- | --- |
| 달리기 | Shift 및 게임패드 왼쪽 스틱 클릭 입력으로 이동 속도를 `6`에서 `8`로 변경 |
| 스태미나 | 달리기 중 초당 `20` 소비, 종료 후 `0.75초` 뒤 초당 `25` 회복 |
| 앉기 | 왼쪽 Ctrl 및 게임패드 East 버튼 입력으로 앉기 상태 전환 |
| 앉기 이동 | 앉은 상태에서 이동 속도를 `3.5`로 제한 |
| 충돌체 전환 | `CharacterController` 높이를 서기 `2`에서 앉기 `1.2`로 전환 |
| 낮은 천장 처리 | 천장 여유 공간이 없을 때 앉기 해제 후에도 일어서지 않도록 처리 |
| 외형 전환 | `Visual Root`의 높이와 위치를 충돌체 변화에 맞춰 갱신 |
| 테스트 맵 | 달리기 점프 구간, 낮은 천장 통로, 수직 점프 발판 4개 구성 |

---

### 입력 및 데이터 연결

- 기존 `InputSystem_Actions.inputactions`의 `Sprint`, `Crouch` 액션 재사용
- 기존 `PLY-001_DefaultPlayer.asset`의 이동·달리기·앉기·스태미나 수치 재사용
- 입력과 데이터 에셋을 새로 만들지 않고 런타임 스크립트에서 기존 구조를 확장
- `PlayerInputReader.cs`에서 앉기 입력을 읽도록 추가
- `PlayerMovementController.cs`에서 앉기 상태, 스태미나, 충돌체와 외형 전환 처리 추가

---

### 고정 수직 테스트 맵

`Game` 씬에 `VerticalTestMap` 부모 오브젝트를 만들고 아래 구조를 구성했다.

```text
VerticalTestMap
├─ StartFloor
├─ SprintRunway
├─ SprintLanding
├─ CrouchFloor
├─ CrouchCeiling
├─ JumpPlatform_01
├─ JumpPlatform_02
├─ JumpPlatform_03
└─ JumpPlatform_04
```

- `SprintRunway`와 `SprintLanding` 사이에 약 `6m` 점프 구간 배치
- `CrouchCeiling`으로 서기 높이보다 낮은 통로 구성
- 높이가 점진적으로 증가하는 발판 4개로 수직 점프 성능 확인
- `Visual`의 중복 `CapsuleCollider` 제거 후 부모 `CharacterController`만 충돌 처리 담당

---

### 문제 해결

테스트 맵만 먼저 커밋되어 왼쪽 Ctrl 앉기가 동작하지 않는 문제가 발생했다. 원인은 입력 에셋의 바인딩 문제가 아니라, `Crouch` 입력과 앉기 상태를 처리하는 두 스크립트가 저장소에 반영되지 않은 것이었다.

다음 파일을 반영해 문제를 해결했다.

```text
Assets/_ProjectJ/Scripts/Runtime/Player/Input/PlayerInputReader.cs
Assets/_ProjectJ/Scripts/Runtime/Player/Movement/PlayerMovementController.cs
```

---

### 테스트 결과

- WASD와 왼쪽 스틱 기본 이동 유지
- Shift와 왼쪽 스틱 클릭 달리기 확인
- 달리기 중 스태미나 소비와 종료 후 지연 회복 확인
- 왼쪽 Ctrl과 게임패드 East 버튼 앉기 확인
- 앉기 중 이동 속도 저하 확인
- 앉기 시 충돌체와 외형 높이 전환 확인
- 낮은 천장 통로 통과 및 천장 아래 강제 서기 방지 확인
- 달리기 점프로 점프 구간 통과 확인
- 수직 발판 4개 등반 확인
- 기본 점프·카메라 기능 유지 확인
- Unity Console 오류 없음 확인

---

### 주요 변경 파일

```text
Assets/_ProjectJ/Scripts/Runtime/Player/Input/PlayerInputReader.cs
Assets/_ProjectJ/Scripts/Runtime/Player/Movement/PlayerMovementController.cs
Assets/_ProjectJ/Scenes/Game/Game.unity
README.md
```

---

### 완료 결과

플레이어는 기본 이동과 점프뿐 아니라 상황에 맞게 달리기와 앉기를 사용할 수 있게 됐다. 고정 테스트 맵으로 속도, 낮은 통로, 수직 점프를 반복 검증할 기반도 마련했다.

다음 13일차에는 높이 구간, 체크포인트, 추락 판정·부활, 최소 HUD를 구현해 수직 진행 구조의 첫 완성 구간을 만든다.
