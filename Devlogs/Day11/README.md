# Project J

3D 3인칭 온라인 수직 점프 경쟁 파티 게임 **Project J**의 개발 저장소입니다.

---

# 11일차 : 기본 이동·점프·3인칭 카메라 구현

## 개발 목표

수직 점프 경쟁 게임의 기본 조작 기반을 구축했습니다. 플레이어는 카메라 방향을 기준으로 이동하고, 점프·중력·공중 제어를 적용받으며, 마우스와 게임패드로 3인칭 카메라를 조작할 수 있습니다.

이번 구현은 기존 입력 액션과 플레이어 데이터 에셋을 재사용합니다. 따라서 이동과 점프의 핵심 수치는 코드에 중복하지 않고 `PLY-001_DefaultPlayer`에서 관리합니다.

---

# 구현 범위

| 기능 | 구현 내용 |
|---|---|
| 기본 이동 | WASD와 왼쪽 스틱의 2축 입력 지원 |
| 이동 기준 | 카메라의 수평 방향 기준 이동 |
| 캐릭터 회전 | 이동 방향을 향해 부드럽게 회전 |
| 점프 | Space와 게임패드 남쪽 버튼 지원 |
| 점프 보조 | 코요테 타임과 점프 입력 버퍼 적용 |
| 낙하 처리 | 중력, 최대 낙하 속도, 천장 충돌 처리 |
| 공중 조작 | 플레이어 데이터 기반 공중 제어 비율 적용 |
| 3인칭 카메라 | 마우스와 오른쪽 스틱 회전 지원 |
| 카메라 제한 | 상하 회전 각도 제한 및 `LateUpdate()` 추적 |

달리기, 앉기, 끝자락 딛고 올라오기, 카메라 벽 충돌 보정은 이번 일차 범위에서 제외했습니다.

---

# 생성한 스크립트

```text
Assets/_ProjectJ/Scripts/Runtime/Player
├─ Input
│  └─ PlayerInputReader.cs
├─ Movement
│  └─ PlayerMovementController.cs
└─ Camera
   └─ ThirdPersonCameraController.cs
```

| 파일 | 역할 |
|---|---|
| `PlayerInputReader.cs` | `Move`, `Look`, `Jump` 입력을 읽고 다른 컴포넌트에 제공 |
| `PlayerMovementController.cs` | `CharacterController` 기반 이동·회전·점프·중력 처리 |
| `ThirdPersonCameraController.cs` | 플레이어 추적, 마우스·오른쪽 스틱 회전, 상하 각도 제한 |

---

# 재사용한 기존 데이터

## 입력 액션

```text
Assets/_ProjectJ/Settings/Input/InputSystem_Actions.inputactions
```

사용 액션:

```text
Move
Look
Jump
```

## 플레이어 수치

```text
Assets/_ProjectJ/Data/Definitions/Player/PLY-001_DefaultPlayer.asset
```

적용 수치:

| 항목 | 값 |
|---|---:|
| 이동 속도 | 6 |
| 지상 가속도 | 24 |
| 지상 감속도 | 30 |
| 회전 속도 | 720 |
| 점프 높이 | 2.4 |
| 코요테 타임 | 0.12초 |
| 점프 버퍼 | 0.12초 |
| 중력 | -25 |
| 최대 낙하 속도 | 35 |
| 공중 제어 비율 | 0.65 |

---

# 씬 구성

대상 씬:

```text
Assets/_ProjectJ/Scenes/Game/Game.unity
```

최종 플레이어 계층:

```text
Player
├─ Visual
└─ CameraTarget
```

| 오브젝트 | 필수 구성 |
|---|---|
| `Player` | `CharacterController`, `PlayerInputReader`, `PlayerMovementController` |
| `Visual` | 플레이어 외형 표시 전용 자식 오브젝트 |
| `CameraTarget` | 카메라 추적 기준점 |
| `Main Camera` | `ThirdPersonCameraController` |
| `Ground` | `Ground` 레이어의 기본 바닥 |

`Player` 루트에는 `Rigidbody`와 일반 `CapsuleCollider`를 추가하지 않습니다. 실제 이동과 충돌은 `CharacterController`가 담당합니다.

---

# 조작 방법

| 조작 | 기능 |
|---|---|
| WASD | 카메라 방향 기준 이동 |
| 왼쪽 스틱 | 아날로그 이동 |
| 마우스 이동 | 카메라 회전 |
| 오른쪽 스틱 | 카메라 회전 |
| Space | 점프 |
| 게임패드 남쪽 버튼 | 점프 |

---

# 완료 기준

- Player가 Ground 위에 정상적으로 서 있음
- WASD와 왼쪽 스틱으로 카메라 기준 이동 가능
- 대각선 이동 속도가 직선 이동보다 빨라지지 않음
- 캐릭터가 실제 이동 방향으로 회전
- Space와 게임패드 남쪽 버튼으로 점프 가능
- 지면 끝에서 약 0.12초 이내 점프 가능
- 착지 직전 점프 입력이 착지 직후 실행
- 천장 충돌 시 상승 속도 제거
- 마우스와 오른쪽 스틱 카메라 회전 정상
- 카메라 상하 회전 제한 정상
- Console Error 0개

---

# 검증 기록

GitHub에서 확인한 직전 기준 커밋은 `10일차 : 개발 빌드 프로필 구성`입니다. 11일차 플레이어 조작 구현은 Unity 로컬 환경에서 다음 항목을 확인한 뒤 커밋합니다.

```text
Game 씬 Play Mode 실행
기본 이동·점프·카메라 조작 확인
Console Error 0개 확인
EditMode 및 PlayMode 테스트 확인
```

---

# 다음 개발 방향

## 12일차 : 달리기·앉기·고정 수직 테스트 맵

다음 일차에는 이동 상태를 확장하고 수직 이동 검증용 테스트 맵을 구성합니다.

```text
달리기 입력과 속도 상태
앉기 입력, 높이 변경과 통과 판정
서기 가능 공간 확인
점프·앉기·달리기 검증용 고정 수직 맵
기본 이동 구간의 난이도 측정
```

---

# 커밋 정보

```text
11일차 : 기본 이동·점프·3인칭 카메라 구현
```
