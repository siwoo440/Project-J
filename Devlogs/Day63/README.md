# Project J - 63일차 개발 일지

## 개발 목표

62일차에서 구현한 Fusion Tick 기반 기본 Network Player 이동을 기준으로, Client Prediction과 Resimulation이 실제로 수행되는지 확인할 수 있는 진단 구조를 추가한다.

이번 일차의 핵심 흐름은 다음과 같다.

```text
Client 입력
↓
Input Authority 기반 예측 이동
↓
Host 권위 상태 수신
↓
과거 상태로 Rollback
↓
저장된 입력으로 Resimulation
↓
현재 Tick까지 재계산
↓
보정 결과 확인
```

점프, 중력, Sprint, Crouch 등의 추가 이동 기능은 이번 단계에 포함하지 않는다.

---

## 주요 개발 내용

### 1. Prediction / Resimulation 진단 인터페이스 추가

`ProjectJNetworkPlayer`에 다음 Fusion Tick Callback을 연결했다.

```text
IBeforeAllTicks
IAfterAllTicks
```

이를 통해 일반 Forward Simulation과 Resimulation 구간을 구분해 기록할 수 있도록 했다.

---

### 2. Resimulation 횟수 기록

Local Input Authority Player에서 다음 값을 기록한다.

```text
ResimulationBatchCount
ResimulationTickCount
LastResimulationTickCount
LastForwardTickCount
```

이를 통해 Client가 Host 상태를 받은 뒤 실제로 몇 번의 Resimulation 묶음을 수행했는지 확인할 수 있다.

---

### 3. Rollback 거리 기록

Resimulation 시작 직전의 예측 위치와 Rollback된 위치를 비교한다.

기록 항목:

```text
PredictionPositionBeforeResimulation
RollbackPosition
LastRollbackDistance
```

네트워크 지연이 있는 환경에서는 현재 예측 위치보다 과거 Host 상태로 되돌아가므로 Rollback Distance가 발생할 수 있다.

---

### 4. Correction 거리 기록

Resimulation이 끝난 후 기존 예측 결과와 재계산된 최종 위치를 비교한다.

기록 항목:

```text
CorrectedPositionAfterResimulation
LastCorrectionDistance
MaxCorrectionDistance
```

현재 이동은 고정 입력, 고정 속도, `Runner.DeltaTime` 기반의 단순한 결정적 계산이므로 정상적인 상태에서는 Correction 값이 누적되지 않고 작은 값으로 유지되는 것을 목표로 한다.

---

## 기존 기본 이동 유지

62일차에서 구현한 이동 구조는 변경하지 않았다.

```text
ProjectJNetworkInput.Move
↓
FixedUpdateNetwork()
↓
입력 정규화
↓
World XZ 이동
↓
BaseMoveSpeed = 5
↓
Runner.DeltaTime
↓
NetworkTransform
```

Prediction / Resimulation 진단 기능은 이 이동 구조 위에 추가됐다.

---

## F2 네트워크 디버그 UI 확장

### 5. 63일차 디버그 화면

F2 디버그 창 제목과 섹션을 다음과 같이 변경했다.

```text
Project J - Fusion 63일차

Fusion Tick Input · Prediction / Resimulation
```

창 높이는 Prediction 진단 정보를 표시하기 위해 확장했다.

```text
1180 × 1020
```

---

### 6. Local Prediction Diagnostics 추가

F2 창에 다음 항목을 추가했다.

```text
Resim Batch / Ticks
Last Resim / Forward
Rollback Distance
Correction / Max
Before → Corrected
```

Local Input Authority Player를 기준으로 Prediction과 Resimulation 상태를 직접 확인할 수 있다.

---

## Network Conditions 테스트 기준

Prediction과 Resimulation은 로컬 환경에서는 지연이 너무 낮아 눈으로 확인하기 어렵기 때문에 Fusion Network Conditions를 이용한 지연 테스트를 기준으로 한다.

권장 테스트 값:

```text
Delay
150 ms

Jitter
0

Packet Loss
0
```

테스트 목표:

```text
Client 입력
→ 즉시 Local Player 반응

Host 상태 수신
→ Resimulation Count 증가

Rollback
→ 발생 가능

Correction
→ 지속적으로 누적되지 않음
```

테스트 종료 후 Network Conditions는 다시 비활성화한다.

---

## Prediction 검증 기준

Client에서 빠르게 방향을 변경한다.

```text
W → S
A → D
W + D → S
```

150ms 수준의 지연에서도 Local Player가 Host 응답을 기다리지 않고 바로 반응해야 한다.

또한 다음 현상이 없어야 한다.

```text
지속적인 위치 떨림
반복적인 큰 순간이동
Host / Client 위치 오차 누적
Correction Distance 지속 증가
```

---

## Fusion Obsolete Warning 정리

`ProjectJFusionInputProvider`가 구현 중인 `INetworkRunnerCallbacks`의 빈 Callback에서 다음 타입이 최신 Fusion에서 Obsolete 처리되어 Warning이 발생했다.

```text
SimulationMessagePtr
```

현재 사용하지 않는 Callback이며 인터페이스 호환을 위해 남아 있으므로 해당 메서드 범위에서만 다음 Warning을 억제했다.

```text
CS0618
```

적용 방식:

```text
#pragma warning disable CS0618
OnUserSimulationMessage(...)
#pragma warning restore CS0618
```

입력 수집과 Prediction 동작에는 영향을 주지 않는다.

---

## WaterGun EditMode Test 호환 수정

EditMode Test에서 WaterGun 사용 해제 시 다음 오류가 발생했다.

```text
Destroy may not be called from edit mode!
Use DestroyImmediate instead.
```

원인은 `WaterGunRuntime.OnUseReleased()`에서 EditMode에서도 `Destroy(this)`가 실행된 것이었다.

수정 후 구조:

```text
OnUseReleased()
↓
active = false
↓
EditMode
→ Component 제거 없이 종료
↓
PlayMode
→ 기존처럼 Destroy(this)
```

따라서 실제 게임 Runtime의 WaterGun 종료 동작은 유지하면서 EditMode 테스트에서 잘못된 `Destroy()` 호출이 발생하지 않도록 했다.

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
└─ ProjectJFusionBootstrapDebugView.cs

Assets/ProjectJ/Network/Fusion/Input/
└─ ProjectJFusionInputProvider.cs

Assets/ProjectJ/Network/Fusion/Player/
└─ ProjectJNetworkPlayer.cs

Assets/ProjectJ/Runtime/Items/Effects/
└─ WaterGunRuntime.cs
```

---

## 생성 파일

```text
없음
```

---

## 삭제 파일

```text
없음
```

---

## 63일차 완료 기준

```text
기본 Network Player 이동 유지
↓
IBeforeAllTicks / IAfterAllTicks 연결
↓
Local Player Resimulation 횟수 기록
↓
Rollback 거리 기록
↓
Correction 거리 기록
↓
F2 Prediction Diagnostics 표시
↓
지연 환경에서 Client 즉시 반응 확인
↓
Resimulation Count 증가 확인
↓
Correction 오차 누적 없음
↓
CS0618 Warning 정리
↓
WaterGun EditMode Destroy 오류 수정
```

---

## 최신 커밋 확인

README 작성 시점 최신 `main` 커밋:

```text
44ef5dad2d22c9ddbe24a2ec3d21fa88deeb6fe1
63
```

이 커밋은 62일차 커밋 바로 다음에 이어져 있으며, 63일차 Prediction / Resimulation 진단 코드와 Fusion Obsolete Warning 정리, WaterGun EditMode 호환 수정이 함께 포함되어 있다.

GitHub에는 이 커밋에 대한 CI 상태가 등록되어 있지 않으므로 Unity Compile, Test Runner 전체 통과 여부, Host / Client 지연 테스트 결과는 로컬 실행 결과를 최종 기준으로 확인한다.

---

## 다음 개발 방향

다음 64일차에서는 Remote Player Interpolation을 다룬다.

핵심 목표:

```text
Local Player
→ Prediction 기반 즉시 반응

Remote Player
→ 수신 Snapshot 사이를 Interpolation

결과
→ 상대 Player 이동을 부드럽게 표시
```

Prediction Player와 Remote Player의 표시 방식을 명확히 분리하는 단계로 진행한다.
