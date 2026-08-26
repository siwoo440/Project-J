---
# Project J - 143일차 개발일지

---
## 개발 주제

Player와 AI Bot의 Character Visual에 이동·점프·낙하·앉기·밀치기 애니메이션 상태를 연결하고, Game Scene에서 Animator 상태 전환이 정상 동작하는지 디버깅

---
## 개발 목표

142일차에 구성한 Player / AI Bot 공통 Character Visual 구조를 유지한 상태에서 실제 Gameplay 상태를 Animator 상태와 연결한다.

기존 Player 이동, 점프, 앉기, 밀치기, Rigidbody와 Fusion Network 동작은 변경하지 않고 `ProjectJPlayerVisualController`가 시각 상태만 담당하도록 구성한다.

이번 일차에서는 다음 상태를 공통 Animator Controller에서 사용할 수 있도록 연결한다.

```text
Idle
running
jump
fall
Crouch Idle
Crouch Move
Push
```

---
## 기준 커밋

이번 개발 정리 시점의 `origin/main` 기준:

```text
a7d86a34996bc917c119615330de1996de325a4f
```

커밋 메시지:

```text
143
```

이전 일차 기준 커밋:

```text
f27e960 142일차 : Player·AI Bot 캐릭터 Visual 적용 및 외형 교체 기반 구성
```

---
## 주요 작업 내용

- `char_AC.controller`에 앉기와 밀치기 상태 추가
- `Crouch Idle`, `Crouch Move`, `Push` Animator State 구성
- `crouchIdle`, `crouchMove`, `push` Trigger 추가
- 기존 `Idle`, `running`, `jump`, `fall` 상태와 Trigger 체계 유지
- `Push.fbx` 추가
- `Crouching Idle.fbx` 추가
- `Crouched Walking.fbx` 추가
- FBX AnimationClip을 실제 sub-asset 기준으로 연결하는 Editor Setup 추가
- `ProjectJ143AnimationSetup` 추가
- `ProjectJPlayerVisualController`에 Gameplay 상태 기반 애니메이션 판정 추가
- Rigidbody 수평 속도를 이용한 Idle / Run 판정 추가
- `IsGrounded`와 Y 속도를 이용한 Jump / Fall 판정 추가
- `IsCrouching`과 이동 여부를 이용한 Crouch Idle / Crouch Move 판정 추가
- 실제 Push 시도 이벤트를 이용한 Push Trigger 연결
- Visual Animator의 Root Motion 비활성화 유지
- Push 재생 중 이동 상태가 즉시 덮어쓰지 않도록 재생 유지 구간 추가
- 기존 Jump State의 자동 Exit Time 전환 제거
- Run 전환 문제 확인을 위한 `ProjectJAnimatorDirectPlayTest` 추가
- Game Scene 런타임 Animator 및 Trigger 상태 수동 검증 진행

---
## Animator 상태 구성

이번 일차 기준 기본 상태 구성은 다음과 같다.

```text
Idle
↕
running

Idle / running
→ jump
→ fall
→ Idle / running

Idle / running
↔ Crouch Idle
↔ Crouch Move

기본 이동 상태
→ Push
→ 기본 이동 상태
```

Animator Parameter는 Trigger 방식으로 구성했다.

```text
idle
run
jump
fall
crouchIdle
crouchMove
push
```

---
## Gameplay 상태와 Animator 연결

`ProjectJPlayerVisualController`가 Gameplay Root의 상태를 읽어 Visual Animator 상태를 결정하도록 구성했다.

기본 판정 기준:

```text
Rigidbody.linearVelocity
→ 수평 이동 여부

PlayerCameraRelativeMovement.IsGrounded
→ 지상 / 공중 여부

Rigidbody.linearVelocity.y
→ Jump / Fall 구분

PlayerCameraRelativeMovement.IsCrouching
→ Crouch 상태 여부

PlayerPushController.PushAttempted
→ Push 애니메이션 실행
```

이동 속도가 임계값보다 크면 `Run`, 작으면 `Idle`로 판정한다.

공중에서는 Y 속도가 양수이면 `Jump`, 그 외에는 `Fall`을 선택한다.

앉은 상태에서는 수평 이동 여부에 따라 `Crouch Idle` 또는 `Crouch Move`를 선택한다.

---
## Editor 자동 설정

다음 Editor Script를 추가했다.

```text
Assets/ProjectJ/Editor/ProjectJ143AnimationSetup.cs
```

FBX의 AnimationClip은 파일 경로만으로 임의의 fileID를 작성하지 않고 `AssetDatabase.LoadAllAssetsAtPath`를 사용해 실제 AnimationClip sub-asset을 조회하도록 구성했다.

이를 통해 다음 FBX의 실제 Clip을 Animator State에 연결한다.

```text
Assets/ProjectJ/Push.fbx
Assets/ProjectJ/Crouching Idle.fbx
Assets/ProjectJ/Crouched Walking.fbx
```

---
## Run 전환 디버깅

Game Scene에서 Player가 이동해도 Animator가 `Idle`에서 `running`으로 전환되지 않는 문제가 확인됐다.

확인 과정에서 다음 항목을 점검했다.

```text
런타임 Visual_Character 생성 확인
Animator Component 존재 확인
char_AC Controller 연결 확인
Animator 활성화 확인
Root Motion 비활성화 확인
run Trigger Parameter 존재 확인
run → running Transition 목적지 확인
Idle Clip Loop 활성화 확인
```

Animator Controller의 저장 데이터에서 `run` 조건이 `running` State를 향하는 구조 자체는 확인됐다.

`running` State를 직접 재생하는 방식에서는 Running Animation 자체가 재생되는 것도 확인했다.

따라서 Running Clip 또는 State 자체가 손상된 문제보다는 Gameplay 상태 → Visual Controller → Trigger 전달 과정에 문제가 남아 있는 것으로 범위를 좁혔다.

---
## 현재 미해결 문제

Game Scene에서 실제 Player 이동 시 `run` Trigger 기반으로 `running` State가 자동 전환되지 않는다.

현재 가장 우선적으로 확인할 항목은 다음과 같다.

```text
ProjectJPlayerVisualController.Update 실행 여부
activeAnimators 배열의 실제 Animator 대상
Rigidbody.linearVelocity 수평 값
ResolveLocomotionState 결과가 Run인지 여부
SetTriggerIfAvailable에서 run Trigger 호출 여부
Visual Controller가 다른 Trigger로 상태를 다시 덮는지 여부
```

이번 일차에서는 원인을 확정하지 않고 여기까지 진행했다.

---
## 수정 및 추가 파일

```text
Assets/ProjectJ/Art/Characters/Animations/Imported/
└─ char_AC.controller

Assets/ProjectJ/
├─ Push.fbx
├─ Push.fbx.meta
├─ Crouching Idle.fbx
├─ Crouching Idle.fbx.meta
├─ Crouched Walking.fbx
└─ Crouched Walking.fbx.meta

Assets/ProjectJ/Editor/
├─ ProjectJ143AnimationSetup.cs
└─ ProjectJ143AnimationSetup.cs.meta

Assets/ProjectJ/Runtime/Player/
├─ ProjectJPlayerVisualController.cs
├─ ProjectJAnimatorDirectPlayTest.cs
└─ ProjectJAnimatorDirectPlayTest.cs.meta

Assets/ProjectJ/Prefabs/Player/
└─ Player.prefab
```

---
## 검증 내용

수동 Play Mode 검증에서 다음 내용을 확인했다.

```text
Visual Character 런타임 생성 정상
Animator Component 존재
char_AC Controller 연결
Idle Animation 재생
running State 직접 재생 가능
run Trigger 기반 자동 전환 실패
```

현재 문제는 Character Mesh, Running Clip 자체보다는 상태 판정 또는 Trigger 전달 구간에 남아 있는 것으로 판단하고 다음 일차에 이어서 추적한다.

---
## 결과

143일차에는 142일차 Character Visual 구조 위에 Player와 AI Bot이 공통으로 사용할 기본 애니메이션 상태 체계를 추가했다.

Idle, Run, Jump, Fall, Crouch Idle, Crouch Move와 Push 상태를 Animator Controller에 구성하고 Gameplay 상태를 Visual Animator로 변환하는 `ProjectJPlayerVisualController` 로직을 확장했다.

앉기와 밀치기용 FBX를 프로젝트에 추가하고 실제 AnimationClip sub-asset을 안전하게 연결하는 Editor Setup도 구성했다.

다만 Game Scene에서 이동 시 `run` Trigger가 `running` State로 자동 전환되지 않는 문제가 남아 있으며, Running Animation 자체의 직접 재생은 정상임을 확인했다.

따라서 애니메이션 에셋 적용 단계는 완료했지만 Gameplay 상태와 Animator Trigger 사이의 런타임 연결 디버깅은 다음 일차로 이관한다.

---
## 다음 일차

다음 일차에서는 `run` Trigger 자동 전환 문제의 데이터 흐름을 우선 추적한다.

핵심 확인 순서:

```text
Player 실제 Rigidbody 속도 확인
→ ResolveLocomotionState 결과 확인
→ run Trigger 호출 확인
→ Trigger를 받는 실제 Animator 확인
→ 상태 전환 충돌 여부 확인
```

Gameplay 이동 코드는 유지하고 Visual / Animator 영역에서만 원인을 수정한다.
