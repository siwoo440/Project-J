# Project J - 95일차 개발일지

## 오늘의 목표

95일차는 Host 1명과 Client 1명을 별도 실행하여 실제 UI만으로 전체 멀티플레이 흐름을 검증하는 날이다.

주요 검증 흐름은 다음과 같다.

```text
Bootstrap
→ MainMenu
→ Private Match
→ 방 생성 / Room Code 참가
→ Lobby
→ Ready
→ Game
→ Countdown
→ 이동
→ Checkpoint
→ FINISH
→ Result
```

---

## 작업 내용

### 1. 동일 Steam 계정 기반 2인 개발 테스트 대응

Editor와 Development Build가 같은 Steam 계정을 사용하면 Fusion에서 동일 UserId로 판단하여 두 번째 접속이 거절되는 문제가 있었다.

Development Build와 Unity Editor에서는 실제 Steam ProjectAccountId 뒤에 개발용 고유 식별자를 붙여 Fusion UserId가 서로 다르게 생성되도록 수정했다.

Release Build에서는 기존 Steam 기반 ID를 그대로 사용하도록 분리했다.

---

### 2. Fusion 입력 Callback 연결 수정

게임 Scene에 진입한 뒤 캐릭터가 생성되어도 이동 입력이 전달되지 않는 문제가 있었다.

`ProjectJFusionInputProvider`를 생성한 뒤 `NetworkRunner.AddCallbacks()`로 명시 등록하도록 수정했다.

수정 후 Host와 Client 양쪽에서 다음 입력을 확인했다.

- 이동
- 점프
- 달리기
- 앉기

---

### 3. 입력이 없는 Tick에서도 중력 유지

기존 `ProjectJNetworkPlayer.FixedUpdateNetwork()`는 Fusion 입력을 받지 못한 Tick에서 즉시 `return`하여 중력 계산까지 중단되는 구조였다.

입력이 없는 경우 이동 입력만 0으로 처리하고, 중력과 Ground 판정은 계속 실행되도록 수정했다.

수정 후 Spawn된 캐릭터가 공중에 고정되지 않고 정상적으로 바닥까지 낙하하는 것을 확인했다.

---

### 4. Scene 전환 후 Camera / AudioListener 중복 대응

Network Player의 Local Presentation이 Scene 전환 뒤에도 유지되면서 새 Scene의 Camera와 AudioListener가 동시에 활성화되는 문제가 있었다.

Scene 변경을 감지한 뒤 Local Player Presentation에서 Camera와 AudioListener 상태를 다시 정리하도록 수정했다.

목표 상태:

```text
MainCamera: 1
AudioListener: 1
```

---

### 5. Checkpoint Trigger 수정

Network Player가 Checkpoint를 통과해도 Trigger가 동작하지 않는 문제가 있었다.

Checkpoint 오브젝트가 런타임에서 다음 설정의 Rigidbody를 보장하도록 수정했다.

```text
Is Kinematic = ON
Use Gravity = OFF
Detect Collisions = ON
```

수정 후 Checkpoint Trigger가 정상 동작하는 것을 확인했다.

---

### 6. FINISH Trigger 수정

Checkpoint와 같은 원인으로 FINISH Trigger 역시 Network Player와 정상적으로 충돌 이벤트를 발생시키지 못했다.

FINISH Trigger에도 Kinematic Rigidbody를 보장하도록 수정했다.

수정 후 목표 지점 도착 처리가 정상 동작하는 것을 확인했다.

---

## 실제 확인된 내용

95일차 테스트에서 다음 항목을 실제로 확인했다.

- Host / Client 2인 접속
- Room Code 기반 동일 세션 참가
- Lobby에서 2명 표시
- Ready 이후 Game Scene 전환
- Countdown 이후 Playing 진입
- 캐릭터 바닥 낙하
- Host / Client 이동
- 점프 및 기본 이동 입력
- Checkpoint Trigger 동작
- FINISH Trigger 동작

---

## 남은 문제

이번 일차 마무리 시점에 아래 UI 관련 문제는 다음 작업으로 이월한다.

### Result 관전 버튼

먼저 FINISH한 플레이어의 관전 버튼이 정상적으로 눌리지 않는 문제가 남아 있다.

### UI 문자열의 `\n`

일부 안내 Text에서 실제 줄바꿈 대신 `\n` 두 글자가 그대로 표시되는 부분이 있다.

### Debug Window 정리

현재 여러 Debug GUI가 동시에 표시되어 화면을 가린다.

추후 다음 형태로 정리할 예정이다.

```text
기본 상태
→ 모든 Debug Window 숨김

F1
→ Debug 메뉴 표시

숫자키
→ 원하는 Debug Window만 표시
```

해당 기능을 위해 임시로 작성했던 `ProjectJDay95RuntimeFixes.cs`는 Assembly 참조 오류가 발생하여 제거했다.

---

## 변경된 주요 파일

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
└─ ProjectJFusionBootstrap.cs

Assets/ProjectJ/Network/Fusion/Player/
└─ ProjectJNetworkPlayer.cs

Assets/ProjectJ/Network/Fusion/Presentation/
└─ ProjectJLocalPlayerPresentationController.cs

Assets/ProjectJ/Runtime/Checkpoint/
└─ Checkpoint.cs

Assets/ProjectJ/Runtime/Finish/
└─ FinishTrigger.cs
```

Game Scene 및 Development Build 설정 일부도 테스트 과정에서 조정되었다.

---

## 최신 커밋

```text
SHA
65130ea105c2d918373c79be63ab2659e6231ad4

현재 커밋 메시지
95
```

이 커밋에는 95일차에서 진행한 Fusion 입력 연결, 개발용 Fusion UserId 분리, 중력 처리, Camera / AudioListener Scene 전환 대응, Checkpoint / FINISH Trigger 수정 등이 포함되어 있다.

GitHub Actions / CI 상태는 등록되어 있지 않으므로 최종 검증은 Unity Editor와 Development Build의 실제 실행 테스트를 기준으로 한다.

---

## 95일차 결과

95일차에서는 2인 멀티플레이의 핵심 흐름을 실제 실행 가능한 상태까지 연결하고, 개발 테스트를 막던 주요 네트워크·입력·Trigger 문제를 수정했다.

다음 작업에서는 Result 이후 관전 처리, 잘못 표시되는 UI 줄바꿈, Debug Window 통합을 먼저 정리한 뒤 전체 Scene Integration Gate 검증으로 이어간다.
