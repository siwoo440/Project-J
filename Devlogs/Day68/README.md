# Project J - 68일차 개발 일지

## 개발 목표

67일차까지 구현한 Network Player 이동, 점프·중력, Sprint·Stamina, Crouch 구조를 유지하면서 각 실행 환경에서 **Input Authority를 가진 Local Player만 Gameplay Camera·AudioListener·Local UI를 사용하도록 분리**한다.

이번 일차의 핵심 흐름은 다음과 같다.

```text
Network Player Spawn
↓
Input Authority 확인
↓
Local Player
→ Gameplay Camera 연결
→ AudioListener 연결
→ Local UI 연결

Remote Player
→ Camera 사용 안 함
→ AudioListener 사용 안 함
→ Local UI 사용 안 함
```

---

## 주요 개발 내용

### 1. Local Player Presentation Controller 추가

새 파일:

```text
Assets/ProjectJ/Network/Fusion/Presentation/
└─ ProjectJLocalPlayerPresentationController.cs
```

Runtime 시작 시 자동 생성되며 Local Player의 카메라와 로컬 표현 기능을 관리한다.

Hierarchy 예시:

```text
[ProjectJ] Local Player Presentation
└─ ProjectJ_LocalGameplayCameraRig
   └─ PitchPivot
      └─ LocalGameplayCamera
```

별도의 Inspector 연결 없이 Runtime에서 자동 구성된다.

---

## Local Player 판정

### 2. Input Authority 기준으로 Local Player 선택

Network Player가 Spawn되면 다음 조건을 확인한다.

```text
Object.HasInputAuthority
```

TRUE인 Player만 Local Presentation에 연결한다.

```text
Host 실행 환경
→ Host Player만 Local

Client 실행 환경
→ Client Player만 Local
```

State Authority가 아니라 Input Authority를 기준으로 하므로 Host가 다른 Client Player의 카메라까지 소유하지 않는다.

---

## Gameplay Camera 분리

### 3. Local Gameplay Camera 생성

Local Player가 연결되면 전용 3인칭 Gameplay Camera를 Runtime에 생성한다.

기본값:

```text
Camera Distance
7.5

Minimum Distance
3.5

Maximum Distance
10

Mouse Sensitivity
0.15

Pitch
-45 ~ 70

Normal FOV
60

Sprint FOV
68
```

마우스 이동으로 카메라 회전, 마우스 휠로 줌을 조작한다.

카메라 조작 자체는 네트워크로 전송하지 않고 Local 전용으로 처리한다.

---

### 4. Crouch 높이 연동

카메라 기준 높이를 Player 상태에 맞춰 변경한다.

```text
Standing
→ Target Height 1.5

Crouch
→ Target Height 0.85
```

67일차의 Networked Crouch 상태를 그대로 사용한다.

---

### 5. Sprint FOV 연동

Local Player가 Sprint 중이면 카메라 FOV를 확대한다.

```text
Walk
→ FOV 60

Sprint
→ FOV 68
```

기존 Networked Sprint 상태를 사용한다.

---

## Authority 테스트 카메라 정리

### 6. AuthorityCameraMarker 비활성 유지

기존 Network Player Prefab의 `AuthorityCameraMarker`는 실제 Gameplay Camera로 사용하지 않는다.

```text
AuthorityCameraMarker
→ OFF

LocalGameplayCamera
→ Local Player에서만 ON
```

기존 Authority 확인용 RenderTexture 생성도 제거했다.

---

## AudioListener 분리

### 7. Local AudioListener 하나만 활성화

Local Gameplay Camera에 `AudioListener`를 추가하고, 다른 활성 AudioListener는 Local Player가 연결된 동안 비활성화한다.

```text
Local Gameplay Camera
→ AudioListener ON

기존 Scene AudioListener
→ 임시 OFF
```

Local Player 연결이 해제되면 기존 AudioListener 상태를 복원한다.

이를 통해 멀티플레이 환경에서 여러 AudioListener가 동시에 활성화되는 문제를 방지한다.

---

## 기존 Camera 처리

### 8. Scene의 다른 Camera 임시 비활성화

Local Gameplay Camera가 활성화될 때 기존 활성 Camera를 찾아 임시로 비활성화한다.

```text
LocalGameplayCamera
→ ON

기존 활성 Camera
→ OFF
```

Local Player가 Despawn되면 기존 Camera를 다시 활성화한다.

---

## Local UI 분리

### 9. Local UI 소유자 구분

68일차에서는 실제 아이템 HUD 데이터를 Fusion으로 동기화하지 않고, **현재 실행 환경에서 Local UI를 사용할 Player가 누구인지 구분하는 기반**만 만든다.

```text
Local Player
→ Local UI Owner

Remote Player
→ Local UI 미연결
```

현재 디버그 표시에서는 Local UI 연결 상태를 Local Presentation Controller의 Binding 상태로 확인한다.

---

## Despawn 처리

### 10. Local Player 연결 해제

Network Player가 Despawn되면 Local Presentation 연결도 해제한다.

```text
Local Player Despawn
↓
Gameplay Camera OFF
↓
AudioListener OFF
↓
기존 Camera 복원
↓
기존 AudioListener 복원
↓
Cursor Unlock
```

`OnDestroy()`에서도 같은 정리 처리를 수행해 참조가 남지 않도록 했다.

---

## F2 디버그 UI 확장

### 11. 68일차 표시 추가

F2 디버그 화면 제목:

```text
Project J - Fusion 68일차
```

추가 진단:

```text
Local Presentation
Camera / UI / Audio
```

정상 상태 예시:

```text
Local Presentation
P0

Camera / UI / Audio
Camera TRUE
UI TRUE
Audio TRUE
```

기존 Player별 Crouch·Sprint·Jump·Interpolation 진단도 유지한다.

---

## Development Build 표시

### 12. Local Player 확인 UI

Editor 또는 Development Build에서는 화면 우측 상단에 Local Player 확인용 UI를 표시한다.

예:

```text
LOCAL PLAYER P0
Camera ON | UI LOCAL | Audio ON
```

Remote Player가 늘어나더라도 이 Local Presentation은 한 명에게만 연결되어야 한다.

---

## 테스트 흐름

### Host 단독

```text
Host Session 시작
↓
Host Player Spawn
↓
Local Gameplay Camera 생성
↓
Host Player 추적
↓
Mouse Look / Zoom 확인
```

확인 항목:

```text
Gameplay Camera 1개
AudioListener 1개
Local Player 표시 정상
WASD / Jump / Sprint / Crouch 정상
```

---

### Host / Client

```text
Editor
→ Host

Development Build
→ Client
```

Host:

```text
Host Player
→ Host Camera Target

Client Player
→ Remote
```

Client:

```text
Client Player
→ Client Camera Target

Host Player
→ Remote
```

상대 Player가 움직여도 자신의 Camera Target이 상대방으로 변경되면 안 된다.

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
└─ ProjectJNetworkPlayer.cs

Assets/ProjectJ/Network/Fusion/Bootstrap/
└─ ProjectJFusionBootstrapDebugView.cs
```

---

## 생성 파일

```text
Assets/ProjectJ/Network/Fusion/Presentation/
└─ ProjectJLocalPlayerPresentationController.cs
```

---

## 삭제 파일

```text
없음
```

---

## 68일차 완료 기준

```text
Input Authority Player 식별
↓
Local Presentation 연결
↓
Local Gameplay Camera 생성
↓
Remote Player Camera 미사용
↓
Local AudioListener 단일 활성
↓
Local UI Owner 분리
↓
Mouse Look Local 전용
↓
Crouch Camera Height 연동
↓
Sprint FOV 연동
↓
Despawn 시 Camera / Audio 연결 해제
↓
F2 Local Presentation 진단 추가
↓
기존 이동·Jump·Sprint·Crouch 유지
```

---

## 다음 개발 방향

69일차에서는 **External Force 네트워크화**를 진행한다.

예상 흐름:

```text
외부 힘 발생
↓
State Authority에서 힘 확정
↓
Network Player 이동 상태에 반영
↓
Prediction / Resimulation
↓
Host / Client 동일 결과 확인
```

밀치기, 스프링, 폭발, 아이템 반동처럼 Player 자신의 WASD 입력이 아닌 외부 힘을 네트워크 이동 시스템에 연결하기 위한 기반을 만든다.
