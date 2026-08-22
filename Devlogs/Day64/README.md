# Project J - 64일차 개발 일지

## 개발 목표

63일차에서 Local Player의 Prediction·Resimulation 진단 구조를 구축한 뒤, 이번 64일차에서는 Remote Player가 Photon Fusion의 `NetworkTransform` 보간을 통해 부드럽게 표시되는지 확인할 수 있는 진단 구조를 추가한다.

핵심 구분은 다음과 같다.

```text
Local Player
→ Input Authority 기반 Prediction

Remote Player
→ NetworkTransform 기반 Interpolation
```

이번 일차에서는 별도의 `Vector3.Lerp()`를 추가하지 않고 Fusion이 제공하는 기본 `NetworkTransform` 보간을 사용한다.

---

## 주요 개발 내용

### 1. Remote Player 구분 정보 추가

`ProjectJNetworkPlayer`에 Local / Remote 상태를 확인하기 위한 속성을 추가했다.

```text
IsRemoteView
IsRemoteProxy
HasNetworkTransform
RemoteInterpolationExpected
```

역할 구분 기준:

```text
LOCAL
→ 현재 실행 환경에서 Input Authority 보유

REMOTE STATE
→ Input Authority는 없지만 State Authority 보유

REMOTE PROXY
→ Input Authority와 State Authority 모두 없음
```

Client에서 다른 Player를 관찰할 때는 주로 `REMOTE PROXY` 상태가 핵심 확인 대상이다.

---

### 2. NetworkTransform 존재 여부 확인

Network Player에서 기존 Prefab에 연결되어 있는 `NetworkTransform`을 Runtime에 확인하도록 했다.

```text
GetComponent<NetworkTransform>()
```

따라서 Remote Player가 다음 조건을 만족하면 자동 보간 대상이라고 진단한다.

```text
Remote Player
+
NetworkTransform 존재
↓
Interpolation = AUTO
```

별도의 이동 RPC 또는 추가 보간 Component는 생성하지 않는다.

---

## Simulation / Render 위치 진단

### 3. Simulation Position 기록

`FixedUpdateNetwork()` 실행 시 Network Simulation 위치를 기록한다.

```text
LastSimulationPosition
```

기존 이동 계산은 그대로 유지한다.

```text
ProjectJNetworkInput.Move
↓
FixedUpdateNetwork()
↓
BaseMoveSpeed
↓
Runner.DeltaTime
↓
Transform 이동
```

Local Player의 Prediction / Resimulation 구조 역시 변경하지 않았다.

---

### 4. Render Position 기록

`LateUpdate()`에서 현재 Transform 위치를 Render Position으로 기록한다.

```text
LastRenderPosition
```

추가 진단값:

```text
RenderSimulationOffset
LastRenderStepDistance
RenderSampleCount
```

이를 통해 Simulation 위치와 화면에 최종 표시되는 위치의 차이를 관찰할 수 있다.

---

### 5. Render Simulation Offset

다음 두 값을 비교한다.

```text
Simulation Position
Render Position
```

거리 차이를 다음 값으로 기록한다.

```text
RenderSimulationOffset
```

Remote Player에서는 NetworkTransform의 Render 보간 때문에 Simulation과 Render 시점 사이에 차이가 나타날 수 있다.

---

### 6. Render Frame 이동량 기록

이전 Render Position과 현재 Render Position을 비교해 다음 값을 기록한다.

```text
LastRenderStepDistance
```

이 값은 Remote Player가 화면에서 한 Render Frame 동안 얼마나 이동했는지 확인하기 위한 진단값이다.

큰 값이 반복적으로 튀는 경우 Remote Player가 부드럽지 않게 표시되는지 확인할 수 있다.

---

## F2 네트워크 디버그 UI 확장

### 7. 64일차 디버그 화면

F2 창 제목을 다음과 같이 변경했다.

```text
Project J - Fusion 64일차
```

63일차의 Local Prediction Diagnostics는 그대로 유지한다.

```text
Resim Batch / Ticks
Last Resim / Forward
Rollback Distance
Correction / Max
Before → Corrected
```

---

### 8. Remote NetworkTransform Interpolation 섹션 추가

새로운 진단 섹션:

```text
Remote NetworkTransform Interpolation
```

표시 항목:

```text
Player
Role
Simulation Position
Render Position
Offset
Render Δ
Interpolation
```

Interpolation 상태:

```text
LOCAL
→ 현재 Local Player

AUTO
→ Remote Player + NetworkTransform

NO NT
→ NetworkTransform 없음
```

정상적인 Remote Player는 `AUTO`로 표시되는 것을 기준으로 한다.

---

## 네트워크 이동 구조

현재 전체 구조는 다음과 같다.

```text
Local Player
키보드 입력
↓
ProjectJFusionInputProvider
↓
NetworkInput
↓
FixedUpdateNetwork()
↓
Prediction
↓
Resimulation / Correction

Remote Player
Host Snapshot 수신
↓
NetworkTransform
↓
Fusion Render Interpolation
↓
부드러운 화면 표시
```

이번 일차에서는 Fusion 보간 위에 별도의 `Lerp()`를 추가하지 않았다.

이중 보간으로 인해 Remote Player가 지나치게 늦게 따라오는 현상을 방지하기 위한 구조다.

---

## 테스트 방법

### Host / Client 기본 확인

```text
Unity Editor
→ Host

Development Build
→ Client
```

Host와 Client가 같은 비공개 Session에 접속한 뒤 F2를 연다.

---

### Client에서 Host Player 확인

```text
Host
→ W 이동

Client
→ Host Player 관찰
```

Client F2에서 Host Player가 다음처럼 표시되는지 확인한다.

```text
Role
→ REMOTE PROXY

Interpolation
→ AUTO
```

그리고 Host Player가 화면에서 끊기지 않고 부드럽게 이동하는지 확인한다.

---

### Host에서 Client Player 확인

```text
Client
→ D 이동

Host
→ Client Player 관찰
```

Host는 State Authority를 가지고 있기 때문에 Client Player가 `REMOTE STATE`로 표시될 수 있다.

이 경우에도 `NetworkTransform`이 존재하고 Local Input Authority가 없는 Player는 Remote 표시 대상으로 진단한다.

---

## 지연 환경 테스트

63일차에서 사용한 Network Conditions를 다시 사용할 수 있다.

권장 시작값:

```text
Delay
150 ms

Jitter
0

Packet Loss
0
```

확인 사항:

```text
Remote Player가 0.1초 단위로 툭툭 이동하지 않음
지속적인 앞뒤 떨림 없음
방향 전환 시 큰 순간이동 없음
정지 후 계속 밀려가는 현상 없음
```

테스트 종료 후 Network Conditions는 다시 비활성화한다.

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
└─ ProjectJFusionBootstrapDebugView.cs

Assets/ProjectJ/Network/Fusion/Player/
└─ ProjectJNetworkPlayer.cs
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

## 64일차 완료 기준

```text
Local Player Prediction 구조 유지
↓
Remote Player Role 구분
↓
NetworkTransform 존재 여부 확인
↓
Simulation Position 기록
↓
Render Position 기록
↓
Render Offset 기록
↓
Render Frame 이동량 기록
↓
F2 Remote Interpolation 진단 표시
↓
Remote Player에서 AUTO 확인
↓
지연 환경에서도 Remote 이동이 부드러움
↓
지속적인 떨림·큰 순간이동 없음
```

---

## 최신 커밋 확인

README 작성 시점 최신 `main` 커밋:

```text
5b07bf7ef76244d456fa1109791f6259be20f5f2
64
```

이 커밋은 63일차 커밋 바로 다음에 이어지는 1개 커밋이며, 변경 파일은 다음 두 개뿐이다.

```text
ProjectJFusionBootstrapDebugView.cs
ProjectJNetworkPlayer.cs
```

GitHub에는 해당 커밋에 대한 CI 상태가 등록되어 있지 않으므로 Unity Compile 및 실제 Host / Client Runtime 테스트 결과는 로컬 실행을 최종 기준으로 확인한다.

---

## 다음 개발 방향

다음 65일차에서는 기본 XZ 이동에 점프와 중력을 추가하고 이를 Fusion Tick Simulation에 연결한다.

예상 흐름:

```text
Jump Input
↓
FixedUpdateNetwork()
↓
수직 속도 계산
↓
Gravity 적용
↓
Y Position 변경
↓
NetworkTransform 동기화
```

Local Prediction과 Remote Interpolation 구조를 유지한 채 수직 이동까지 확장한다.
