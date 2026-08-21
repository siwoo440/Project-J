# 43일차 개발일지 - 밀치기 최소 피드백 및 임시 판정 UI

## 1. 개발 목표

43일차의 목표는 밀치기 시스템의 실제 판정 결과를 플레이 중 바로 확인할 수 있도록 최소한의 UI/VFX 피드백을 추가하는 것이다.

현재 기준 커밋:

```text
586a3957339bd9ea3b289c69d3581bee355dbbce
```

현재 커밋 메시지:

```text
43
```

이번 일차에서는 최종 연출을 만드는 것이 아니라 개발 중 판정 검증을 위한 최소 피드백을 구현한다.

핵심 확인 대상:

```text
밀치기 사용 가능 여부
밀치기 시도
적중
빗나감
쿨타임 중 요청
부활 보호 대상
피격 방향
실제 판정과 표시의 일치 여부
```

---

## 2. 밀치기 결과 이벤트 추가

수정 파일:

```text
Assets/ProjectJ/Runtime/Push/
└─ PlayerPushController.cs
```

기존에는 `TryPush()` 또는 `TryPushAt()`의 반환값으로만 밀치기 결과를 알 수 있었다.

43일차에서는 밀치기 결과가 확정될 때:

```text
PushAttempted
```

이벤트를 한 번 발생시키도록 추가했다.

전달 데이터:

```text
PushAttemptResult
Target PlayerFinishState
실제로 적용한 Velocity Change
```

구조:

```text
Push 입력
↓
실제 판정
↓
CompleteAttempt()
↓
LastResult / LastTarget 갱신
↓
PushAttempted 이벤트 1회
```

이를 통해 UI/VFX가 물리 판정을 다시 계산하지 않고 실제 결과를 그대로 받아 표시한다.

---

## 3. 판정 중복 방지

43일차에서 중요한 기준은 한 번의 밀치기 요청에 대해 판정 피드백도 한 번만 발생하는 것이다.

정상 구조:

```text
Push 1회
↓
판정 1회
↓
Feedback Event 1회
↓
Text 표시 1회
```

따라서 다음과 같은 중복 표시가 발생하지 않도록 했다.

```text
HIT
HIT
HIT
```

또한 한 번의 요청에서:

```text
HIT
MISS
```

가 동시에 표시되지 않도록 실제 `PushAttemptResult` 하나만 사용한다.

---

## 4. PlayerPushReceiver 피격 이벤트 추가

수정 파일:

```text
Assets/ProjectJ/Runtime/Push/
└─ PlayerPushReceiver.cs
```

실제 외부 힘 적용에 성공했을 때:

```text
PushReceived
```

이벤트를 발생시킨다.

전달 값:

```text
실제로 적용된 수평 Velocity Change
```

이를 피격자 UI에서 사용해 어느 방향에서 밀렸는지 표시할 수 있다.

수직 방향은 기존 규칙과 동일하게 제외한다.

---

## 5. PlayerPushFeedbackUI 추가

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Push/
└─ PlayerPushFeedbackUI.cs
```

주요 역할:

```text
밀치기 Ready 상태 표시
Cooldown 남은 시간 표시
HIT / MISS / PROTECTED 판정 표시
피격 방향 표시
밀치기 시도 범위 VFX 표시
판정 표시 시간 관리
```

최종 UI가 아니라 테스트용 임시 피드백 시스템이다.

---

## 6. 임시 판정 Text UI

Player Prefab에 Screen Space Overlay Canvas를 추가하고 화면 상단에 테스트용 Text를 표시하도록 구성했다.

평상시:

```text
PUSH READY
```

밀치기 성공:

```text
JUDGMENT : HIT
```

빗나감:

```text
JUDGMENT : MISS
```

쿨타임 중 입력:

```text
JUDGMENT : COOLDOWN
```

부활 보호 중인 Target:

```text
JUDGMENT : PROTECTED
```

Receiver가 없는 비정상 Target:

```text
JUDGMENT : NO RECEIVER
```

그 외 유효하지 않은 상태:

```text
JUDGMENT : INVALID
```

판정 Text는 기본 약 `0.65초` 표시 후 다시 현재 밀치기 가능 상태 표시로 돌아간다.

---

## 7. 쿨타임 상태 표시

판정 표시가 끝난 뒤 밀치기가 아직 쿨타임이라면 화면에 남은 시간을 표시한다.

예:

```text
PUSH COOLDOWN 1.4s
PUSH COOLDOWN 0.9s
PUSH COOLDOWN 0.3s
```

쿨타임 종료:

```text
PUSH READY
```

이를 통해 현재 LMB가 실제로 사용 가능한 상태인지 플레이 중 바로 확인할 수 있다.

---

## 8. 피격 방향 표시

로컬 Player가 실제 Push를 받으면 밀치기 벡터를 기준으로 공격이 들어온 방향을 표시한다.

표시:

```text
HIT FROM FRONT
HIT FROM BACK
HIT FROM LEFT
HIT FROM RIGHT
```

방향은 피격자의 Transform 기준으로 계산한다.

예:

```text
Player 정면에서 공격
→ HIT FROM FRONT

Player 뒤에서 공격
→ HIT FROM BACK
```

실제 Push Velocity를 사용하므로 별도의 추정용 Raycast나 중복 Target 판정을 만들지 않는다.

---

## 9. 최소 휘두름 VFX

밀치기를 실제로 시도한 순간 공격자 앞쪽에 짧은 원호 Line VFX를 표시한다.

기본 표시 시간:

```text
0.12초
```

VFX 범위는 `PlayerPushTargetSelector`의 실제 값을 사용한다.

```text
Search Range
Search Angle
```

따라서 Scene에 보이는 임시 원호와 실제 Target 탐색 범위가 최대한 일치하도록 구성했다.

구조:

```text
Player
↓
밀치기 시도
↓
전방 원호 VFX
```

Cooldown 또는 Invalid State처럼 실제 밀치기 시도 단계까지 가지 못한 경우에는 휘두름 VFX를 다시 재생하지 않는다.

---

## 10. Day43 Setup Tool

새 Editor 파일:

```text
Assets/ProjectJ/Editor/
└─ Day43PushFeedbackSetup.cs
```

Unity 메뉴:

```text
ProjectJ
→ Day43
→ Setup Push Feedback UI
```

실행 시 Player Prefab에 다음을 자동 구성한다.

```text
=== Push Feedback UI ===
└─ PushJudgmentText

PushSwingVfx

PlayerPushFeedbackUI
```

Canvas 설정:

```text
Render Mode = Screen Space Overlay
Sorting Order = 500
Reference Resolution = 1920 × 1080
```

판정 Text는 화면 상단 중앙에 배치한다.

---

## 11. UGUI Assembly 참조 추가

수정 파일:

```text
Assets/ProjectJ/Runtime/
└─ ProjectJ.Runtime.asmdef
```

기존:

```text
Unity.InputSystem
```

참조에 추가로:

```text
Unity.UGUI
```

를 등록했다.

`PlayerPushFeedbackUI`에서 `UnityEngine.UI.Text`, `Canvas`, `CanvasScaler` 등을 사용하기 위한 Runtime Assembly 참조다.

---

## 12. 테스트용 Swing Material

Day43 Setup Tool은 다음 경로에 최소 휘두름 VFX용 Material을 준비한다.

```text
Assets/ProjectJ/Tests/Manual/Phase4/Materials/
└─ Day43_PushSwing.mat
```

URP Unlit Shader를 우선 사용하고, 사용할 수 없을 경우 대체 Shader를 찾는다.

이 Material은 최종 아트 리소스가 아니라 Phase 4 수동 테스트를 위한 임시 시각 자료다.

---

## 13. 자동 테스트 추가

새 테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/
└─ PlayerPushFeedbackEventTests.cs
```

주요 검증:

```text
Miss 요청 1회
→ PushAttempted Event 정확히 1회

첫 Push 후 Cooldown 상태에서 재요청
→ 두 번째 요청도 Feedback Event 정확히 1회
```

이 테스트를 통해 판정 이벤트가 중복 실행되지 않는지 확인한다.

---

## 14. 생성·수정·삭제 요소

### 생성

```text
Assets/ProjectJ/Editor/
└─ Day43PushFeedbackSetup.cs

Assets/ProjectJ/Runtime/Push/
└─ PlayerPushFeedbackUI.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlayerPushFeedbackEventTests.cs
```

각 `.meta` 포함.

Setup 실행 결과로 Player Prefab 내부에 임시 Canvas/Text/VFX 구조와 테스트 Material도 추가된다.

### 수정

```text
Assets/ProjectJ/Runtime/
└─ ProjectJ.Runtime.asmdef

Assets/ProjectJ/Runtime/Push/
├─ PlayerPushController.cs
└─ PlayerPushReceiver.cs

Assets/ProjectJ/Prefabs/Player/
└─ Player.prefab
```

### 삭제

```text
없음
```

---

## 15. 현재 밀치기 피드백 흐름

43일차 종료 기준:

```text
LMB
↓
PlayerPushController
↓
실제 Target / Cooldown / Protection 판정
↓
PushAttemptResult 확정
↓
PushAttempted Event 1회
↓
PlayerPushFeedbackUI
↓
HIT / MISS / COOLDOWN / PROTECTED 표시
```

적중 시 Target 쪽:

```text
PlayerPushReceiver
↓
외부 힘 실제 적용 성공
↓
PushReceived Event
↓
피격 방향 Text 표시
```

판정과 시각 피드백을 같은 데이터에서 처리하도록 구성했다.

---

## 16. Phase 4 테스트맵 수동 확인

테스트 Scene:

```text
Assets/ProjectJ/Tests/Manual/Phase4/
└─ Phase4_InteractionTest.unity
```

### Target 적중

```text
PUSH / COOLDOWN 구역
↓
Target 정면 배치
↓
LMB
↓
JUDGMENT : HIT
```

Target이 실제로 밀리는 것과 Text의 HIT가 일치해야 한다.

### 빗나감

```text
아무 Player가 없는 방향
↓
LMB
↓
JUDGMENT : MISS
```

### 쿨타임

```text
첫 Push
↓
1.5초 이내 다시 LMB
↓
JUDGMENT : COOLDOWN
```

### 보호 Target

```text
RESPAWN PROTECTED 구역
↓
LMB
↓
JUDGMENT : PROTECTED
```

실제 Target은 밀리지 않아야 한다.

### 밀치기 범위

밀치기 시 짧게 나타나는 원호 VFX가 현재 Search Range / Search Angle과 대략 일치하는지 확인한다.

---

## 17. 수동 테스트 체크리스트

- [ ] Unity Console Error 0
- [ ] 화면 상단에 PUSH READY 표시
- [ ] 밀치기 성공 시 JUDGMENT : HIT
- [ ] 빗나감 시 JUDGMENT : MISS
- [ ] Cooldown 요청 시 JUDGMENT : COOLDOWN
- [ ] 보호 대상 공격 시 JUDGMENT : PROTECTED
- [ ] 판정 Text가 약 0.65초 후 상태 표시로 복귀
- [ ] Cooldown 중 남은 시간 표시
- [ ] Cooldown 종료 후 PUSH READY 복귀
- [ ] 밀치기 시 짧은 전방 원호 VFX 재생
- [ ] Cooldown 중 VFX 중복 재생 없음
- [ ] 실제 적중 Player와 HIT 표시가 일치
- [ ] 실제 Miss와 MISS 표시가 일치
- [ ] 한 Push에서 HIT와 MISS가 동시에 표시되지 않음
- [ ] 피격 방향 Text와 실제 밀린 방향 일치
- [ ] 기존 밀치기 힘 12 유지
- [ ] 밀치기 외부 힘 감속 정상
- [ ] 수직 튕김 현상 없음
- [ ] Phase 4 기존 테스트 기능 회귀 오류 없음

---

## 18. 자동 테스트 체크리스트

Unity:

```text
Window
→ General
→ Test Runner
→ EditMode
→ Run All
```

확인:

- [ ] `PlayerPushFeedbackEventTests` 전체 Green
- [ ] `PlayerPushControllerTests` 전체 Green
- [ ] `PlayerExternalForceAccumulatorTests` 전체 Green
- [ ] `PlayerPushTargetSelectorTests` 전체 Green
- [ ] `PlayerCollisionRulesTests` 전체 Green
- [ ] 기존 EditMode 테스트 전체 Green
- [ ] 기존 PlayMode 테스트 Green

---

## 19. 이번 일차에서 구현하지 않은 기능

43일차의 피드백은 개발 검증용 최소 기능이다.

아직 구현하지 않은 최종 연출:

```text
캐릭터 Push 전용 애니메이션
최종 타격 VFX
최종 피격 VFX
카메라 흔들림
완성 효과음
HUD 디자인
멀티플레이용 원격 VFX 복제
서버 판정 기반 Feedback 동기화
```

현재 단계에서는 실제 판정과 시각 정보가 정확하게 맞는지를 우선한다.

---

## 20. 개발 결과

43일차에서는 밀치기 시스템에 최소한의 시각 피드백 계층을 추가했다.

최종 확인 흐름:

```text
사용 가능
→ PUSH READY

밀치기 적중
→ JUDGMENT : HIT

밀치기 빗나감
→ JUDGMENT : MISS

쿨타임 중
→ JUDGMENT : COOLDOWN

부활 보호
→ JUDGMENT : PROTECTED

피격
→ HIT FROM 방향
```

또한 한 번의 Push 요청당 하나의 판정 이벤트만 발생하도록 구성해 테스트용 UI와 실제 밀치기 판정이 1:1로 연결되는 기반을 마련했다.

---

## 21. 저장소 검토 메모

GitHub 최신 커밋에는 43일차의 Setup Tool, Player Prefab UI/VFX 구성, UGUI Runtime Assembly 참조, `PlayerPushFeedbackUI`, `PlayerPushController` 판정 이벤트, `PlayerPushReceiver` 피격 이벤트 및 EditMode 테스트가 포함되어 있다.

정적 코드 검토 기준으로 43일차 목표를 막는 문제는 확인되지 않았다.

GitHub 최신 커밋에는 별도 CI 상태가 등록되어 있지 않다.

따라서 최종 완료 판정은 로컬 Unity에서:

```text
Console Error 0
EditMode 전체 통과
PlayMode 회귀 테스트 통과
Phase 4 수동 HIT / MISS / COOLDOWN / PROTECTED 확인
```

을 기준으로 한다.
