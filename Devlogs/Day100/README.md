# Project J - 100일차 개발일지

---

## 개발 방향

99일차 진단 기준선을 유지하면서 Photon Fusion 경기의 로컬 카메라가 Player Transform의 Tick 이동과 위치 보정을 그대로 따라가며 발생시키는 시각적 끊김을 완화한다.

네트워크 Simulation, Prediction, 이동 속도, Player Prefab의 `NetworkTransform` 설정은 변경하지 않고 카메라 표시 위치만 프레임 독립 방식으로 보간한다.

---

## 변경 내용

---

### 1. 카메라 위치 보간 정책 추가

`ProjectJCameraSmoothingPolicy`를 추가해 카메라 위치 계산을 Runtime 코드와 EditMode 테스트에서 함께 사용할 수 있도록 분리했다.

- 지수 감쇠 기반 프레임 독립 위치 보간
- 음수 보간 속도와 Delta Time의 안전 처리
- 목표 위치 초과 방지
- 지정 거리 이상의 순간이동 판정

---

### 2. Fusion 로컬 카메라 위치 보간 적용

`ProjectJLocalPlayerPresentationController`가 Player 위치를 즉시 복사하던 구조를 보간된 카메라 Rig 위치를 사용하는 구조로 변경했다.

- 카메라 위치 추적 속도: `18`
- 순간이동 Snap 거리: `4m`
- 마우스 회전과 줌: 기존 즉시 반영 유지
- Spawn과 최초 연결: 목표 위치로 즉시 이동
- Player 연결 해제: 이전 보간 상태 초기화
- Scene 전환: 이전 Scene의 보간 위치 초기화
- 부활과 큰 위치 이동: 카메라가 맵을 가로질러 이동하지 않고 즉시 추적

---

### 3. 로컬 카메라 진단값 추가

카메라 보간 결과를 F6 진단 화면에서 확인할 수 있도록 다음 값을 공개했다.

| 표시 | 측정 내용 |
| --- | --- |
| `Local Camera Step` | 최근 프레임에서 카메라 Rig가 이동한 거리 |
| `Follow Offset` | 현재 카메라 Rig 위치와 Player 기준 목표 위치의 차이 |

`Follow Offset`은 이동 중 일시적으로 발생할 수 있으며, Player가 정지하면 다시 0에 가까워지는지 확인한다.

---

### 4. F6 진단 화면 갱신

기존 Day99 네트워크 진단 화면을 Day100 Prediction·Interpolation·Camera 확인 화면으로 갱신했다.

기존 FPS, RTT, Jitter, Correction, Rollback, Resimulation, Render Step, Simulation Offset 표시와 `2P MEASURE GATE`는 그대로 유지했다.

---

### 5. EditMode 테스트 추가

`ProjectJCameraSmoothingPolicyTests`를 추가해 다음 조건을 검증하도록 구성했다.

- 목표 위치 방향으로 이동하며 목표를 초과하지 않는지 확인
- 30FPS와 60FPS에서 1초 후 결과가 같은지 확인
- `4m` 경계에서 즉시 Snap하는지 확인
- `4m` 미만에서 보간을 유지하는지 확인
- Delta Time이 0일 때 현재 위치를 유지하는지 확인

---

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Presentation/
└─ ProjectJLocalPlayerPresentationController.cs

Assets/ProjectJ/Network/Fusion/Test/
└─ ProjectJDay79NetworkConditionDebugView.cs

Assets/ProjectJ/Runtime/Camera/
├─ ProjectJCameraSmoothingPolicy.cs
└─ ProjectJCameraSmoothingPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJCameraSmoothingPolicyTests.cs
└─ ProjectJCameraSmoothingPolicyTests.cs.meta

Devlogs/Day100/
└─ README.md
```

기능 코드 기준으로 수정 파일 2개, 생성 파일 4개이며 삭제한 파일은 없다.

---

## 확인 절차

1. Unity의 `Window → General → Test Runner`에서 EditMode를 연다.
2. `ProjectJCameraSmoothingPolicyTests`의 테스트 5개를 실행한다.
3. Console에 컴파일 Error가 없는지 확인한다.
4. Host와 Client PC에서 Steam Client를 실행하고 서로 다른 계정으로 로그인한다.
5. 일반 Windows Development Build에서 Private Room을 생성하고 Room Code로 참가한다.
6. Lobby를 거쳐 Game Scene에 진입한다.
7. 양쪽 PC에서 F6을 눌러 Day100 진단 화면을 표시한다.
8. `2P MEASURE GATE : PASS`를 확인한다.
9. 정지, 걷기, 달리기, 점프, 앉기, 카메라 회전, 낙하 후 부활을 차례로 확인한다.
10. 마우스 회전 입력이 밀리지 않는지 확인한다.
11. 정지 후 `Follow Offset`이 0에 가까워지는지 확인한다.
12. 부활 시 카메라가 새 위치로 즉시 이동하는지 확인한다.
13. 기존보다 Correction, Resimulation, Render Step, Simulation Offset이 증가하지 않는지 비교한다.
14. Host와 Client가 경기 1회를 완료하고 Console Error가 0건인지 확인한다.

---

## 검증 결과

GitHub 최신 커밋의 변경 파일 6개가 100일차 카메라 보간 작업 범위와 일치하는 것을 확인했다.

- 수정 파일 2개
- 생성 파일 4개
- 삭제 파일 0개
- Git diff 공백 오류 없음
- 새 C# 파일과 `.meta` 파일 구성 확인
- Runtime 정책과 EditMode 테스트 어셈블리 참조 확인
- 진단 화면이 참조하는 카메라 측정 Property 존재 확인
- 네트워크 Simulation과 Player Prefab 설정 미변경 확인

현재 검증 환경에는 Unity Editor가 없어 실제 컴파일, EditMode Test Runner, Windows Development Build, Host·Client 2인 접속은 실행하지 못했다. 해당 실행 결과는 Unity에서 최종 확인이 필요하다.

---

## 기준 커밋

```text
e0486a43b9186462813992b5a6b1c3eee9b4adaa
a
```

---

## 100일차 결과

Fusion 로컬 카메라의 위치 추적을 네트워크 이동과 분리해 프레임 독립 보간을 적용하고, Spawn·Scene 전환·부활과 같은 큰 위치 변화에서는 즉시 추적하도록 구성했다.

F6 진단 화면에 카메라 이동 거리와 추적 오차를 추가해 보간 결과를 기존 네트워크 진단값과 함께 비교할 수 있도록 했다.
