# Project J - 99일차 개발일지

---

## 개발 방향

Photon Fusion Host Mode에서 Host와 Client 모두에게 느껴지는 이동 끊김의 원인을 수치로 구분할 수 있도록 진단 화면을 확장한다.

이번 일차에서는 이동, Prediction, Interpolation, 카메라 보간 값을 조정하지 않고 다음 항목을 측정할 수 있는 기준선을 마련한다.

- FPS
- RTT 평균·최대값
- RTT Jitter
- Correction 거리
- Rollback 거리
- Resimulation Batch·Tick
- 최근 Resimulation·Forward Tick
- Render Step 거리
- Simulation·Render 위치 차이

---

## 변경 내용

### 1. F6 진단 화면 단축키 분리

`ProjectJDebugWindowMenu`가 모든 Debug View를 F1·F2 관리 대상으로 처리하면서 Day79 네트워크 진단 화면까지 비활성화하던 충돌을 분리했다.

`ProjectJDay79NetworkConditionDebugView`는 F1·F2 관리 대상에서 제외되고 기존 F6 단축키로 독립 전환된다.

### 2. Debug Window 분리 정책 추가

`ProjectJDebugWindowRoutingPolicy`를 추가해 전용 단축키를 사용하는 진단 화면을 공통 Hotkey 관리 대상에서 제외할 수 있도록 구성했다.

- Day79 네트워크 진단 화면: F6 전용
- 기존 Day77·Day78 Debug View: F1·F2 관리 유지
- 비어 있거나 잘못된 타입 이름: 기존 관리 방식 유지

### 3. 2인 측정 기준 적용

기존 8인 네트워크 상태 확인 화면을 99일차 Host 1명 + Client 1명 측정 기준으로 변경했다.

참가자 2명과 Player Object 2개가 모두 준비되면 다음 문구가 표시된다.

```text
2P MEASURE GATE : PASS
```

### 4. 이동 진단 수치 확장

플레이어별 표시를 두 줄로 분리하고 다음 수치를 추가했다.

| 표시 | 측정 내용 |
| --- | --- |
| `Corr` | 최근 보정 거리 / 최대 보정 거리 |
| `Roll` | 최근 Rollback 거리 |
| `ReSim B/T` | 누적 Resimulation Batch / Tick |
| `Last R/F` | 최근 Resimulation Tick / Forward Tick |
| `Step` | 최근 Render 위치 이동 거리 |
| `Offset` | Simulation 위치와 Render 위치의 차이 |

전체 요약에는 누적 Resimulation Tick, 최대 Render Step, 최대 Simulation Offset을 추가했다.

### 5. EditMode 회귀 테스트 추가

`ProjectJDebugWindowRoutingPolicyTests`를 추가해 다음 조건을 검증하도록 구성했다.

- Day79 네트워크 진단 화면이 전용 F6 단축키 대상으로 분류되는지 확인
- Day77·Day78 Debug View가 기존 F1·F2 관리 대상으로 유지되는지 확인
- 빈 문자열과 null 입력이 안전하게 처리되는지 확인

---

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
└─ ProjectJDebugWindowMenu.cs

Assets/ProjectJ/Network/Fusion/Test/
└─ ProjectJDay79NetworkConditionDebugView.cs

Assets/ProjectJ/Runtime/SceneFlow/
├─ ProjectJDebugWindowRoutingPolicy.cs
└─ ProjectJDebugWindowRoutingPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJDebugWindowRoutingPolicyTests.cs
└─ ProjectJDebugWindowRoutingPolicyTests.cs.meta

Devlogs/Day99/
└─ README.md
```

삭제한 파일은 없다.

---

## 측정 절차

1. Host와 Client PC에서 Steam Client를 실행하고 서로 다른 계정으로 로그인한다.
2. 일반 Windows Development Build를 실행한다.
3. Host가 Private Room을 생성하고 Client가 Room Code로 참가한다.
4. Lobby를 거쳐 Game Scene에 진입한다.
5. 양쪽 PC에서 F6을 눌러 Day99 진단 화면을 표시한다.
6. `2P MEASURE GATE : PASS`를 확인한다.
7. 정지, Host 단독 이동, Client 단독 이동, 동시 이동, 달리기, 점프, 카메라 회전을 각각 측정한다.
8. Host와 Client의 FPS, RTT, Jitter, Corr, Roll, ReSim, Step, Offset을 비교한다.
9. Console Error가 0건인지 확인한다.

측정 중에는 Fusion Tick Rate, Prediction, Interpolation, 이동 속도, 카메라 보간 값을 변경하지 않는다.

---

## 검증 결과

GitHub 최신 커밋의 변경 파일이 99일차 작업 범위와 일치하는 것을 확인했다.

- 변경 파일 2개
- 생성 파일 4개
- 삭제 파일 0개
- Git diff 공백 오류 없음
- 수정된 C# 파일의 중괄호·소괄호 균형 이상 없음
- 진단 화면이 참조하는 측정 Property 존재 확인
- Runtime 정책과 EditMode 테스트 어셈블리 참조 구조 확인

현재 검증 환경에는 Unity Editor가 없어 EditMode Test Runner, Windows Development Build, 실제 Host·Client 접속은 실행하지 못했다. 해당 실행 결과는 Unity에서 최종 확인이 필요하다.

---

## 기준 커밋

```text
336d0401aa8c7f4103e6a3b1803344eab335a220
99
```

---

## 99일차 결과

Host와 Client의 이동 끊김을 FPS 저하, 네트워크 지연, Resimulation, 위치 보정, Render 이동 불연속으로 나누어 관찰할 수 있는 진단 기준선을 구성했다.

다음 100일차에서는 99일차 측정 결과를 근거로 Prediction, Interpolation, 이동 처리, 카메라 보간을 단계적으로 개선한다.
