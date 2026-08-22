# Project J - 60일차 개발 일지

## 개발 목표

Photon Fusion Host Mode에서 Session 참가자마다 하나의 `NetworkObject` Player를 생성하고 Authority 구조를 분리한다.

이번 일차의 핵심 목표는 다음과 같다.

```text
Session 참가
↓
Host가 Network Player Spawn
↓
PlayerRef와 NetworkObject 연결
↓
Host가 State Authority 보유
↓
각 소유자가 자신의 Input Authority 보유
↓
Local / Remote Player 구분
↓
Client 이탈 시 Player Despawn
```

실제 Fusion Tick 입력 전송과 플레이어 이동 네트워크화는 이번 일차 범위에 포함하지 않는다.

---

## 구현 내용

### 1. Network Player Prefab 추가

네트워크 Spawn 테스트용 Player Prefab을 추가했다.

```text
Assets/ProjectJ/Network/Fusion/Player/Resources/
└─ ProjectJNetworkPlayer.prefab
```

기본 구성:

```text
ProjectJNetworkPlayer
├─ NetworkObject
├─ NetworkTransform
├─ ProjectJNetworkPlayer
├─ Visual
│  └─ Capsule
└─ AuthorityCameraMarker
   └─ Camera
```

현재 Prefab은 실제 플레이어 캐릭터를 교체하기 전 Network Spawn과 Authority 구조를 검증하기 위한 테스트용 객체다.

---

### 2. Network Player Prefab 자동 생성 도구 추가

다음 Editor 스크립트를 추가했다.

```text
Assets/ProjectJ/Network/Fusion/Player/Editor/
└─ ProjectJNetworkPlayerPrefabBuilder.cs
```

Prefab이 존재하지 않을 경우 Editor에서 자동 생성하며, 메뉴에서도 다시 생성할 수 있도록 했다.

```text
Tools
→ Project J
→ Fusion
→ 60일차 Network Player Prefab 재생성
```

---

### 3. Player 참가 시 NetworkObject Spawn

`ProjectJNetworkPlayerSpawner`를 추가했다.

```text
Assets/ProjectJ/Network/Fusion/Player/
└─ ProjectJNetworkPlayerSpawner.cs
```

`IPlayerJoined`를 이용해 Session에 새로운 `PlayerRef`가 참가하면 Host가 Network Player를 생성한다.

```text
PlayerJoined
↓
Host 여부 확인
↓
해당 PlayerRef의 Player Object 존재 여부 확인
↓
Runner.Spawn()
↓
Input Authority = 참가한 PlayerRef
↓
Runner.SetPlayerObject()
```

Client가 직접 Player를 Spawn하지 않고 Host가 모든 Player 생성을 담당한다.

---

### 4. PlayerRef와 NetworkObject 연결

Spawn된 Player는 다음 방식으로 참가자와 연결한다.

```text
Runner.SetPlayerObject(
    PlayerRef,
    NetworkObject
)
```

이후 다음 구조로 Player를 찾을 수 있다.

```text
PlayerRef
↓
Runner.TryGetPlayerObject()
↓
해당 Network Player
```

이 연결은 이후 체크포인트, 부활, 순위, 아이템, Finish 등 플레이어별 네트워크 상태를 관리할 때 사용할 기반이 된다.

---

### 5. State Authority 구조 확인

Host Mode에서는 Host가 Spawn을 담당하므로 Network Player의 State Authority는 Host가 가진다.

Host 기준:

```text
Host Player
State Authority = TRUE

Client Player
State Authority = TRUE
```

Client 기준:

```text
Host Player
State Authority = FALSE

Client Player
State Authority = FALSE
```

따라서 네트워크 Player의 최종 상태 결정은 Host 쪽에서 처리하는 구조다.

---

### 6. Input Authority 구조 확인

`Runner.Spawn()` 시 참가한 `PlayerRef`를 Input Authority로 전달한다.

```text
Host Player
→ Input Authority = Host PlayerRef

Client Player
→ Input Authority = Client PlayerRef
```

각 실행 환경에서는 자신의 Player만 다음 값을 가진다.

```text
Object.HasInputAuthority = true
```

이를 Local Player 판정 기준으로 사용한다.

---

### 7. Local Player와 Remote Player 구분

`ProjectJNetworkPlayer`에서 Authority를 기준으로 Local / Remote 상태를 나눈다.

Local Player:

```text
Input Authority = TRUE
Camera Marker = ON
Local Input 감지 가능
Visual = Local 구분 색상
```

Remote Player:

```text
Input Authority = FALSE
Camera Marker = OFF
Local Input 감지 안 함
Visual = Remote 구분 색상
```

이번 일차에서는 실제 플레이 카메라를 교체하지 않고 Authority 구분용 테스트 Camera만 사용한다.

---

### 8. Local Input 감지 테스트 추가

아직 `NetworkRunner.ProvideInput`은 `false` 상태를 유지한다.

따라서 이번 단계에서는 실제 Fusion Tick 입력을 보내지 않는다.

대신 Local Player에서 다음 입력이 발생하는지만 확인한다.

```text
W
A
S
D
Space
```

Local Player에서 입력을 감지하면 F2 디버그 창의 `Local Input` 항목에 잠시 다음 값이 표시된다.

```text
DETECTED
```

Remote Player에서는 입력이 감지되지 않는다.

---

### 9. 테스트용 Authority Camera 분리

각 Network Player Prefab에 Authority 확인용 Camera를 배치했다.

```text
Local Player
→ Camera ON

Remote Player
→ Camera OFF
```

이 Camera는 실제 게임 화면을 덮어쓰지 않도록 작은 RenderTexture로 출력한다.

---

### 10. Unity 6 URP RenderTexture Depth 수정

Unity 6 Render Graph에서 Camera의 Output Texture가 Depth Buffer를 요구하기 때문에 테스트 Camera의 RenderTexture에 Depth Buffer를 추가했다.

수정:

```text
16 × 16 RenderTexture
Depth Buffer = 24
Format = ARGB32
```

그리고 생성 직후 `Create()`를 호출하도록 했다.

이를 통해 다음 Render Graph 경고가 발생하던 원인을 수정했다.

```text
the output Render Texture must have a depth buffer
```

---

### 11. Player 이탈 시 Despawn

`IPlayerLeft`를 이용해 Client가 Session을 떠났을 때 해당 Player Object를 Host가 제거하도록 했다.

```text
PlayerLeft
↓
Runner.TryGetPlayerObject()
↓
해당 NetworkObject 확인
↓
Runner.Despawn()
```

따라서 Client 이탈 후 Host에는 떠난 Client의 Network Player가 남지 않는다.

---

### 12. 테스트용 Spawn 위치 분리

여러 Network Player가 완전히 같은 위치에 생성되지 않도록 참가 순서에 따라 X축 위치를 분리했다.

예:

```text
Player 1
→ X = 0

Player 2
→ X = 3

Player 3
→ X = 6
```

현재 위치는 Network Spawn 검증용이며 실제 경기 시작 위치 시스템과는 아직 연결하지 않는다.

---

## F2 네트워크 디버그 UI 확장

F2 디버그 창을 Network Player와 Authority 상태까지 확인할 수 있도록 확장했다.

창 크기:

```text
820 × 730
```

Session 정보:

```text
상태
역할
현재 방 코드
연결 Session
참가 인원
공개 여부
Region
상태 메시지
마지막 결과
```

Network Player 정보:

```text
Spawn 수
Local PlayerRef
PlayerRef
State Authority
Input Authority
Camera
Local Input
```

Host와 Client 양쪽에서 Authority 차이를 직접 비교할 수 있다.

---

## Authority 기대 구조

### Host 화면

```text
Host Player
State Authority = TRUE
Input Authority = TRUE
Camera = ON

Client Player
State Authority = TRUE
Input Authority = FALSE
Camera = OFF
```

### Client 화면

```text
Host Player
State Authority = FALSE
Input Authority = FALSE
Camera = OFF

Client Player
State Authority = FALSE
Input Authority = TRUE
Camera = ON
```

---

## 59일차 방 코드 기능 연결

59일차에서 구축한 6자리 비공개 방 코드 흐름을 Network Player Spawn 테스트에도 그대로 사용한다.

```text
Host
→ 6자리 방 코드 자동 생성
→ ProjectJ-XXXXXX Session 생성

Client
→ 6자리 코드 입력
→ 동일 Session 참가
→ Network Player Spawn
```

따라서 Network Player 테스트를 위해 별도의 Session 입력 방식을 추가하지 않았다.

---

## Fusion Scene Warning

현재 Runner 시작 시 다음 Warning이 남아 있다.

```text
[Fusion] NetworkRunner started with no scene in StartGameArgs.Scene.
No network scene will be loaded and no scene NetworkObjects will be spawned.
```

현재 단계에서는 의도된 동작이다.

60일차에서는 기존 Unity Scene을 Fusion이 다시 로드하지 않고 Session만 시작한다.

```text
현재 Unity Scene
→ 유지

Fusion Session
→ 시작

Network Player
→ Runner.Spawn()으로 Runtime 생성
```

따라서 `StartGameArgs.Scene`은 비어 있으며, Fusion이 Scene NetworkObject를 자동 Spawn하지 않는다는 안내 Warning이 출력된다.

이번 일차의 Network Player는 Scene에 미리 배치된 NetworkObject가 아니라 `Runner.Spawn()`으로 생성하기 때문에 60일차 목표를 막는 Warning으로 판단하지 않는다.

Scene NetworkObject와 MatchLoading 동기화가 필요한 단계에서 네트워크 Scene 관리 구조를 별도로 연결한다.

---

## 생성 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkPlayer.cs
├─ ProjectJNetworkPlayerSpawner.cs
├─ Editor/
│  └─ ProjectJNetworkPlayerPrefabBuilder.cs
└─ Resources/
   └─ ProjectJNetworkPlayer.prefab

Assets/ProjectJ/Network/Fusion/Session/
└─ ProjectJFusionRoomCode.cs
```

관련 Unity `.meta` 파일과 폴더 `.meta` 파일도 함께 추가됐다.

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
├─ ProjectJFusionBootstrap.cs
└─ ProjectJFusionBootstrapDebugView.cs
```

---

## 삭제 파일

```text
없음
```

---

## 테스트 흐름

```text
Unity Editor
→ Host

Development Build
→ Client
```

Host:

```text
F2
↓
비공개 방 생성
↓
Network Player Spawn
↓
Spawn 수 1
```

Client:

```text
Host 방 코드 입력
↓
방 코드로 참가
↓
Client Player Spawn
↓
양쪽 Spawn 수 2
```

Authority 확인:

```text
Host
→ 모든 Player State Authority 보유
→ Host Player만 Input Authority 보유

Client
→ State Authority 없음
→ Client Player만 Input Authority 보유
```

이탈:

```text
Client 방 나가기
↓
Host에서 Client Player Despawn
↓
Spawn 수 2 → 1
```

재참가:

```text
같은 코드로 Client 참가
↓
Network Player 다시 Spawn
↓
Spawn 수 1 → 2
```

---

## 현재 네트워크 범위

현재까지 구현된 범위:

```text
Photon Fusion NetworkRunner
↓
비공개 Session
↓
6자리 방 코드
↓
Host / Client 참가
↓
Network Player Spawn
↓
PlayerRef 연결
↓
State Authority
↓
Input Authority
↓
Local / Remote Player 구분
↓
이탈 시 Despawn
```

아직 구현하지 않은 범위:

```text
Fusion Tick Input
실제 이동 네트워크화
Prediction
Resimulation
Interpolation
점프 / 중력 동기화
Sprint / Stamina 동기화
Crouch 동기화
```

---

## 최신 커밋 확인

README 작성 시점의 최신 `main` 커밋:

```text
cead030ce0dc0abaeba48f1ab21d510858c1904f
60
```

이 커밋은 바로 이전 59일차 커밋을 부모로 가지며, 60일차 변경만 1개 커밋으로 이어져 있다.

---

## 다음 개발 방향

다음 일차에서는 현재 각 Player에 부여된 Input Authority를 실제 Fusion 입력 시스템에 연결한다.

핵심 흐름:

```text
Local Input
↓
NetworkRunner.ProvideInput 활성화
↓
Fusion Tick마다 입력 수집
↓
NetworkInput 전달
↓
Input Authority Player 입력 수신
```

이 단계에서는 이동 결과를 완전히 네트워크화하기 전에 먼저 입력 데이터 구조와 Tick 기반 수집 흐름을 검증한다.
