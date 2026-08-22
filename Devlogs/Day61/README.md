# Project J - 61일차 개발 일지

## 개발 목표

60일차에서 구축한 Network Player Spawn 및 State/Input Authority 구조 위에 Photon Fusion의 Tick 기반 입력 수집 구조를 연결한다.

이번 일차의 핵심 목표는 다음과 같다.

```text
로컬 키보드 입력
↓
ProjectJFusionInputProvider
↓
INetworkRunnerCallbacks.OnInput()
↓
ProjectJNetworkInput
↓
Fusion Tick
↓
Network Player FixedUpdateNetwork()
↓
GetInput()
```

실제 Player 이동 동기화는 아직 구현하지 않고, 입력 데이터가 Fusion Tick을 통해 올바른 Player에 전달되는지 확인하는 단계로 제한한다.

---

## 주요 개발 내용

### 1. Fusion 입력 데이터 구조 추가

신규 파일:

```text
Assets/ProjectJ/Network/Fusion/Input/
└─ ProjectJNetworkInput.cs
```

`INetworkInput`을 구현하는 `ProjectJNetworkInput` 구조체를 추가했다.

현재 입력 데이터:

```text
Move
→ Vector2

Buttons
→ NetworkButtons
```

버튼 종류:

```text
Jump
Sprint
Crouch
```

---

### 2. 기본 입력 매핑

현재 61일차 입력은 다음과 같이 구성했다.

```text
WASD
→ Move

Space
→ Jump

Left/Right Shift
→ Sprint

Left/Right Ctrl
→ Crouch
```

대각선 이동 입력은 길이가 1을 넘지 않도록 정규화한다.

---

### 3. Fusion Input Provider 추가

신규 파일:

```text
Assets/ProjectJ/Network/Fusion/Input/
└─ ProjectJFusionInputProvider.cs
```

`ProjectJFusionInputProvider`는 `INetworkRunnerCallbacks`를 구현한다.

역할:

```text
Update()
→ 실제 키보드 상태 수집

OnInput()
→ 현재 입력을 ProjectJNetworkInput으로 구성
→ NetworkInput.Set() 호출
```

장치 입력 수집과 Network Player의 시뮬레이션 처리를 분리했다.

---

### 4. ProvideInput 활성화

기존:

```text
runner.ProvideInput = false
```

를 다음과 같이 변경했다.

```text
runner.ProvideInput = true
```

이제 Host와 Client Runner 모두 자신의 Local Player 입력을 Fusion에 제공할 수 있다.

---

### 5. Input Provider 자동 등록 구조

`NetworkRunner`를 생성할 때 같은 Runner GameObject에 다음 Component를 추가하도록 했다.

```text
ProjectJFusionInputProvider
```

따라서 별도의 Scene 오브젝트나 Inspector 연결 없이 Runner 생성 시 입력 Provider도 함께 준비된다.

---

### 6. Jump 입력 보존 처리

Jump는 `Space`를 짧게 눌렀을 때 Render Frame과 Fusion Tick 사이에서 입력이 사라질 수 있으므로 별도의 `pendingJump` 값을 사용한다.

```text
Update()
→ Space Down 감지
→ pendingJump = true

OnInput()
→ Jump 버튼에 반영
→ Fusion 입력 전달
→ pendingJump = false
```

이를 통해 짧은 버튼 입력을 다음 Input Tick까지 보존한다.

---

## Network Player 입력 수신

### 7. 직접 Keyboard 입력 제거

60일차 Network Player는 Local Input 확인을 위해 Player 자체에서 `Keyboard.current`를 읽었다.

61일차에서는 Network Player가 직접 키보드를 읽지 않도록 변경했다.

현재 구조:

```text
실제 입력 장치
→ ProjectJFusionInputProvider

Network Player
→ Fusion GetInput()
```

이 구조를 통해 Local Player와 Remote Player가 동일한 시뮬레이션 코드를 사용할 수 있는 기반을 마련했다.

---

### 8. FixedUpdateNetwork 입력 수신

`ProjectJNetworkPlayer.FixedUpdateNetwork()`에서 다음 입력 구조를 읽는다.

```text
GetInput<ProjectJNetworkInput>()
```

수신한 데이터:

```text
Move
Jump
Sprint
Crouch
```

현재는 입력 값을 기록하고 디버그 UI에 표시하기만 하며 Transform 이동에는 사용하지 않는다.

---

### 9. 수신 Tick 기록

각 Network Player가 입력을 수신하면 현재 Fusion Simulation Tick을 기록한다.

```text
Runner.Tick
```

이를 통해 F2 디버그 UI에서 Player별 입력 수신이 계속 진행되는지 확인할 수 있다.

---

## F2 네트워크 디버그 UI 확장

### 10. 61일차 디버그 창 확대

F2 디버그 창을 다음 크기로 확장했다.

```text
1000 × 900
```

기존 Session / Authority 정보와 함께 Fusion Tick Input 정보를 한 화면에서 확인할 수 있도록 했다.

---

### 11. Input Provider 정보 표시

추가된 항목:

```text
ProvideInput
Local PlayerRef
Provider Move
Provider Buttons
Input Tick / Count
```

예:

```text
ProvideInput
: TRUE

Provider Move
: (0.00, 1.00)

Provider Buttons
: Jump -   Sprint TRUE   Crouch -

Input Tick / Count
: 1500 / 1498
```

---

### 12. Player별 수신 입력 표시

최대 8명의 Player를 한 화면에서 확인할 수 있도록 변경했다.

표시 항목:

```text
Player
State
Input
Move
Jump
Sprint
Crouch
Camera
Received Tick
```

이를 통해 Host에서 Client Player 입력이 실제 Fusion Tick을 통해 들어오는지 비교할 수 있다.

---

## 전체 입력 흐름

### Host

```text
Host Keyboard
↓
Host ProjectJFusionInputProvider
↓
OnInput()
↓
ProjectJNetworkInput
↓
Fusion
↓
Host Player FixedUpdateNetwork()
↓
GetInput()
```

### Client

```text
Client Keyboard
↓
Client ProjectJFusionInputProvider
↓
OnInput()
↓
ProjectJNetworkInput
↓
Fusion
↓
Client Input Authority Player
↓
Host Simulation에서도 해당 입력 수신
```

---

## 생성 파일

```text
Assets/ProjectJ/Network/Fusion/Input/
├─ ProjectJNetworkInput.cs
└─ ProjectJFusionInputProvider.cs
```

관련 `.meta` 파일과 `Input.meta`도 추가됐다.

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
├─ ProjectJFusionBootstrap.cs
└─ ProjectJFusionBootstrapDebugView.cs

Assets/ProjectJ/Network/Fusion/Player/
└─ ProjectJNetworkPlayer.cs
```

---

## 삭제 파일

```text
없음
```

---

## 테스트 확인 항목

### Host

```text
비공개 방 생성
↓
ProvideInput = TRUE
↓
Input Tick / Count 증가
↓
WASD 입력 시 Provider Move 변화
↓
Space / Shift / Ctrl 입력 반영
```

### Client

```text
Development Build 실행
↓
Host 방 코드 참가
↓
ProvideInput = TRUE
↓
Client 키보드 입력 수집
↓
Client Player에서 해당 입력 수신
```

### Host에서 Client 입력 확인

```text
Client에서 D + Shift 입력
↓
Host F2
↓
Client Player 행 확인
↓
Move = (1.00, 0.00)
Sprint = TRUE
```

이 흐름이 정상이라면 Client 입력이 Host Simulation까지 전달된 것이다.

---

## 61일차 완료 기준

```text
ProjectJNetworkInput 생성
↓
WASD Move 입력 수집
↓
Jump / Sprint / Crouch 버튼 입력 수집
↓
NetworkRunner.ProvideInput = true
↓
OnInput() 동작
↓
NetworkInput.Set() 전달
↓
FixedUpdateNetwork()에서 GetInput() 성공
↓
Host 입력 수신
↓
Client 입력 수신
↓
Host에서 Client Player 입력 확인
↓
Network Player의 직접 Keyboard 입력 제거
↓
F2에서 Input Tick 및 Player별 입력 확인
↓
실제 Player 이동은 아직 적용하지 않음
```

---

## 현재 남아 있는 Fusion Scene Warning

현재 다음 Warning은 여전히 발생할 수 있다.

```text
[Fusion] NetworkRunner started with no scene in StartGameArgs.Scene.
No network scene will be loaded and no scene NetworkObjects will be spawned.
```

현재 Network Player는 Scene에 미리 배치된 NetworkObject가 아니라 `Runner.Spawn()`으로 Runtime 생성하고 있으므로, 61일차 Tick Input 검증 단계에서는 이 Warning이 작업 진행을 막는 오류는 아니다.

Scene NetworkObject와 MatchLoading 동기화가 필요한 단계에서 Network Scene 구조를 별도로 연결한다.

---

## 최신 커밋 확인

README 작성 시점 최신 `main` 커밋:

```text
5ad3bf2ef08b688d5868dfc54ab97714d1257c9b
61
```

이 커밋은 60일차 커밋 바로 다음에 이어지는 1개 커밋이며, 61일차 변경 파일만 추가·수정된 상태다.

GitHub 저장소에는 이 커밋에 대한 CI 상태가 등록되어 있지 않으므로 실제 Unity Compile 및 Host/Client Runtime 테스트 결과는 로컬 테스트 기준으로 최종 확인한다.

---

## 다음 개발 방향

다음 62일차에서는 이번에 수신한 `Move` 입력을 실제 Network Player 이동에 연결한다.

예상 흐름:

```text
ProjectJNetworkInput.Move
↓
FixedUpdateNetwork()
↓
State Authority 이동 계산
↓
NetworkTransform 반영
↓
Host / Client에서 동일 위치 확인
```

점프, 중력, Sprint, Crouch는 기본 이동 네트워크화 이후 순차적으로 연결한다.
