# 37일차 개발일지 - 기본 관전 전환 및 FINISH 퇴장 처리

## 1. 개발 목표

37일차의 목표는 플레이어가 정상에 도달한 뒤 더 이상 직접 조작되지 않도록 경기장에서 퇴장시키고, 아직 경기 중인 다른 플레이어를 3인칭 카메라로 관전할 수 있는 최소 관전 구조를 구현하는 것이다.

현재 기준 커밋:

```text
7e3f8038db2f5fd425e6d4d416c00b0c9de19865
```

현재 커밋 메시지:

```text
37
```

이번 일차의 핵심 흐름은 다음과 같다.

```text
Player FINISH 도달
↓
FinishOrder / FinishTime 확정
↓
개인 결과 Snapshot 생성
↓
완주 Player 이동 및 물리 정지
↓
완주 Player Renderer / Animator / Collider 비활성화
↓
완주 Player 경기장에서 사라짐
↓
관전 가능한 Player 탐색
↓
Spectator Camera 전환
↓
다음 / 이전 관전 대상 변경
```

---

## 2. SpectatorController

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Spectator/SpectatorController.cs
```

기본 관전 상태를 관리한다.

주요 역할:

```text
관전 시작
관전 종료
현재 Target 저장
관전 가능한 Player 목록 계산
다음 Target
이전 Target
현재 Target 완주 시 자동 변경
```

관전 가능한 대상은:

```text
자기 자신이 아님
IsFinished == false
GameObject가 활성 상태
```

인 Player로 제한한다.

---

## 3. 관전 대상 순환

관전 대상은 현재 경기 중인 Player들 사이에서 순환한다.

예:

```text
Player B
↓ Next
Player C
↓ Next
Player B
```

Previous는 반대 방향으로 동일하게 순환한다.

현재 관전 중인 Player가 FINISH에 도달하면 해당 Player를 관전 대상에서 제외하고 다음 유효 Player로 자동 전환한다.

남은 관전 대상이 없으면 관전 상태를 종료한다.

---

## 4. 입력 소유권 분리

관전 중 카메라 Target은 다른 Player로 변경하지만 카메라 입력은 Local Player의 PlayerInput을 계속 사용한다.

구조:

```text
Camera Target
→ 다른 Player

Camera Input Source
→ Local PlayerInput
```

따라서 관전자의 마우스 입력은 관전 카메라에 사용할 수 있지만 관전 대상 Player의 이동 권한을 가져오지 않는다.

---

## 5. Local Player Gameplay 차단

관전 시작 시 Local Player의:

```text
PlayerCameraRelativeMovement
```

를 비활성화한다.

반면:

```text
PlayerInput
```

자체는 유지한다.

이유는 관전 카메라가 Local PlayerInput을 계속 사용해야 하기 때문이다.

따라서:

```text
WASD 이동
→ Local Player 이동 차단

마우스 Look
→ 관전 카메라에서 계속 사용
```

구조가 된다.

---

## 6. 기존 3인칭 카메라 재사용

관전 시스템은 기존:

```text
PlayerThirdPersonCamera
```

구조를 재사용한다.

기존 Gameplay Camera Rig가 Scene에 있으면 이를 사용하고, 없으면 Day37 Editor Setup에서 자동 생성한다.

자동 생성 구조:

```text
=== Day37 Gameplay Camera Rig ===
└─ CameraPivot
   └─ Main Camera
```

그 뒤 Gameplay Camera Rig를 복제해:

```text
=== Spectator Camera Rig ===
```

를 생성한다.

---

## 7. Camera Rig 자동 생성 보강

초기 Day37 Setup에서는 Day36 테스트 Scene에 이미 PlayerThirdPersonCamera가 존재한다고 가정해:

```text
Day37 설정에 필요한 Local Player 또는 3인칭 Camera Rig를 찾을 수 없습니다.
```

오류가 발생했다.

이를 수정해 다음 순서로 처리하도록 변경했다.

```text
기존 PlayerThirdPersonCamera 탐색
↓
있으면 재사용
↓
없으면 Gameplay Camera Rig 자동 생성
↓
Scene Camera 재사용 또는 Main Camera 생성
↓
PlayerThirdPersonCamera 연결
```

따라서 Day36 Scene에 별도 Camera Rig가 없어도 Day37 테스트 환경을 생성할 수 있다.

---

## 8. Spectator Camera AudioListener 처리

Gameplay Camera Rig를 복제해 Spectator Camera를 만들 때 AudioListener가 중복될 수 있다.

따라서 Spectator Camera에 복제된:

```text
AudioListener
```

가 존재하면 제거한다.

이를 통해 Scene 내 AudioListener 중복 경고를 방지한다.

---

## 9. SpectatorDebugView

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Spectator/SpectatorDebugView.cs
```

개발 테스트용으로 다음 정보를 화면에 표시한다.

```text
Spectating 상태
현재 Target
관전 가능한 Target 수
Camera Input Owner
Local Gameplay 상태
```

테스트 버튼:

```text
Enter Spectator
Previous
Next
Exit Spectator
```

를 제공한다.

---

## 10. Day37 Editor 자동 설정

새 Editor 파일:

```text
Assets/ProjectJ/Editor/Day37SpectatorSetup.cs
```

Unity 메뉴:

```text
ProjectJ
→ Day37
→ Setup Basic Spectator
```

실행 시 다음을 자동 처리한다.

```text
Day36 테스트 Scene 복사
↓
Local Player 검색
↓
PlayerInput 확인
↓
PlayerCameraRelativeMovement 확인
↓
Gameplay Camera Rig 검색 또는 생성
↓
Spectator Camera Rig 생성
↓
관전 테스트용 Dummy Player B / C 생성
↓
SpectatorController 생성
↓
SpectatorDebugView 생성
↓
Day37 테스트 Scene 저장
```

---

## 11. 테스트용 관전 Player

Day37 수동 테스트 Scene에는 다음 Dummy Player를 자동 배치한다.

```text
SpectatorDummy_B
SpectatorDummy_C
```

Dummy Player는 실제 Local Player 조작과 충돌하지 않도록 테스트에 불필요한 입력, 이동, Ranking, Fall, Respawn 관련 기능을 비활성화한다.

관전 대상 검색에서는 완주하지 않은 Player로 사용할 수 있다.

---

## 12. FINISH 도달 후 Player 정지 문제 수정

초기 상태에서는 Player가 FINISH Trigger에 접촉한 뒤에도 이동 입력과 Rigidbody 속도가 남아 있어 계속 앞으로 달려가는 현상이 있었다.

이를 수정하기 위해:

```text
PlayerFinishState.ApplyFinishedPlayerDeparture()
```

를 추가했다.

FINISH 확정 후 다음 처리를 수행한다.

```text
PlayerCameraRelativeMovement 비활성화
PlayerLedgeClimber 비활성화
PlayerLedgeDetector 비활성화

Rigidbody.linearVelocity = Vector3.zero
Rigidbody.angularVelocity = Vector3.zero
Rigidbody.detectCollisions = false
Rigidbody.isKinematic = true
```

따라서 FINISH 이후 캐릭터가 계속 이동하는 현상을 차단한다.

---

## 13. FINISH 도달 후 캐릭터 제거 표현

기획에 맞게 정상 도달 Player가 경기장에 계속 남아 있지 않도록 시각 및 충돌 요소를 제거한다.

FINISH 후:

```text
모든 자식 Collider 비활성화
모든 Animator 비활성화
모든 Renderer 비활성화
```

를 적용한다.

결과:

```text
FINISH 도달
↓
캐릭터 즉시 정지
↓
경기장에서 시각적으로 사라짐
↓
다른 Player와 충돌하지 않음
↓
관전 시스템으로 전환 가능
```

Player GameObject 자체를 Destroy하지 않기 때문에 Finish 결과, PlayerId, MatchResult 등의 데이터는 유지된다.

---

## 14. FINISH Event 처리 순서

FINISH 처리 순서는 다음과 같다.

```text
IsFinished = true
FinishOrder 저장
FinishTime 저장
↓
높이 Ranking 경쟁에서 제외
↓
Finished Event 발생
↓
개인 결과 및 관전 관련 시스템 Event 처리
↓
ApplyFinishedPlayerDeparture()
↓
캐릭터 정지 및 시각적 제거
```

Event를 먼저 전달한 뒤 Renderer와 Gameplay 기능을 제거하므로 기존 결과 및 관전 시스템이 Finish 상태를 정상적으로 받을 수 있다.

---

## 15. FinishDeparture 중복 처리 차단

PlayerFinishState에:

```text
FinishDepartureApplied
```

상태를 추가했다.

이미 퇴장 처리가 수행된 경우:

```text
ApplyFinishedPlayerDeparture()
```

를 다시 호출해도 중복 처리를 하지 않는다.

---

## 16. EditMode Input System 참조 수정

Day37 Spectator 테스트에서:

```text
UnityEngine.InputSystem
PlayerInput
```

을 직접 사용하면서 EditMode asmdef에 Input System 참조가 없어 컴파일 오류가 발생했다.

오류:

```text
CS0234
UnityEngine.InputSystem namespace를 찾을 수 없음

CS0246
PlayerInput type을 찾을 수 없음
```

수정 파일:

```text
Assets/ProjectJ/Tests/EditMode/ProjectJ.Tests.EditMode.asmdef
```

references에 다음을 추가했다.

```text
Unity.InputSystem
```

현재 참조 구조:

```text
ProjectJ.Runtime
Unity.InputSystem
```

---

## 17. SpectatorController EditMode 테스트

새 테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/SpectatorControllerTests.cs
```

주요 검증 항목:

- 관전 시작 시 유효한 미완주 Player 선택
- 완주 Player 관전 대상 제외
- Next Target 전환
- Previous Target 순환
- Local PlayerInput 소유권 유지
- Local Gameplay 기능만 비활성화
- 관전 종료 시 기존 Camera 상태 복구
- 현재 관전 Target 완주 시 다음 Player 자동 전환
- 관전 가능한 Player가 없을 경우 관전 진입 차단

---

## 18. FINISH 퇴장 EditMode 테스트

새 테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/PlayerFinishDepartureTests.cs
```

주요 검증 항목:

```text
FINISH 후 Rigidbody 속도 0
Rigidbody isKinematic = true
Rigidbody detectCollisions = false
Player Collider 비활성화
Renderer 비활성화
Animator 비활성화
FinishDepartureApplied = true
중복 퇴장 처리 방지
```

---

## 19. Day37 수동 테스트 Scene

생성 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day37/
└─ Day37_SpectatorTest.unity
```

테스트 흐름:

```text
Play
↓
Enter Spectator
↓
SpectatorDummy_B 관전
↓
Next
↓
SpectatorDummy_C 관전
↓
Previous
↓
SpectatorDummy_B 관전
↓
Exit Spectator
```

FINISH 테스트:

```text
Play
↓
Local Player로 FINISH 도달
↓
Player 즉시 정지
↓
Renderer 제거
↓
Collider 제거
↓
개인 결과 생성
↓
관전 가능한 Player 존재 시 자동 관전 전환
```

---

## 20. 생성 및 수정 요소

새 Runtime 폴더:

```text
Assets/ProjectJ/Runtime/Spectator/
```

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Spectator/SpectatorController.cs
Assets/ProjectJ/Runtime/Spectator/SpectatorDebugView.cs
```

수정 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Finish/PlayerFinishState.cs
```

새 Editor 파일:

```text
Assets/ProjectJ/Editor/Day37SpectatorSetup.cs
```

새 EditMode 테스트:

```text
Assets/ProjectJ/Tests/EditMode/SpectatorControllerTests.cs
Assets/ProjectJ/Tests/EditMode/PlayerFinishDepartureTests.cs
```

수정 Assembly Definition:

```text
Assets/ProjectJ/Tests/EditMode/ProjectJ.Tests.EditMode.asmdef
```

새 수동 테스트 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day37/Day37_SpectatorTest.unity
```

삭제 파일:

```text
없음
```

---

## 21. 이번 일차에서 구현하지 않은 기능

37일차에서는 기본 관전 전환까지만 구현한다.

아직 구현하지 않은 기능:

```text
완성된 관전 UI
관전 Player 이름 표시 UI
자유 카메라
킬캠
리플레이
전체 결과 화면
로비 복귀
네트워크 Input Authority 연동
Fusion State Authority 기반 관전 대상 동기화
```

실제 멀티플레이 권한 처리는 이후 네트워크 단계에서 현재 Local 구조를 서버 권한 기반으로 교체한다.

---

## 22. 검증 체크리스트

- [ ] Unity Console Error 0
- [ ] Day37 Setup 실행 성공
- [ ] Gameplay Camera Rig 자동 생성 또는 재사용
- [ ] Spectator Camera Rig 생성
- [ ] SpectatorDummy_B / C 생성
- [ ] Enter Spectator 정상 동작
- [ ] Next / Previous Target 정상 순환
- [ ] 완주 Player 관전 대상 제외
- [ ] Local PlayerInput 유지
- [ ] 관전 중 Local Player 이동 차단
- [ ] 관전 대상 Player 조작 권한 변경 없음
- [ ] 현재 Target 완주 시 다음 대상 자동 전환
- [ ] FINISH 시 Local Player 즉시 정지
- [ ] FINISH 후 Rigidbody 속도 0
- [ ] FINISH 후 Collider 비활성화
- [ ] FINISH 후 Renderer 비활성화
- [ ] FINISH 후 Animator 비활성화
- [ ] FINISH 후 캐릭터가 경기장에서 보이지 않음
- [ ] 개인 결과 데이터 정상 유지
- [ ] EditMode 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

## 23. 개발 결과

37일차에서는 FINISH 이후 플레이어를 경기에서 안전하게 퇴장시키고 다른 경기 중 Player를 3인칭 카메라로 관전할 수 있는 최소 구조를 구현했다.

최종 흐름:

```text
FINISH
↓
도착 순위 / 시간 확정
↓
개인 결과 생성
↓
완주 Player 이동 및 물리 정지
↓
Collider / Animator / Renderer 비활성화
↓
경기장에서 캐릭터 제거 표현
↓
미완주 Player 검색
↓
Spectator Camera 전환
↓
Next / Previous 관전
```

또한 Day37 테스트 과정에서 확인된 Input System asmdef 참조 누락과 Day36 Scene의 3인칭 Camera Rig 부재 문제를 보강하여, Day37 Setup이 필요한 테스트 환경을 스스로 구성할 수 있도록 수정했다.

GitHub 최신 커밋에는 Day37 관전 Runtime, Editor Setup, 수동 테스트 Scene, EditMode 테스트, Input System asmdef 참조 수정, FINISH Player 정지 및 퇴장 처리가 모두 포함되어 있다.

GitHub에는 해당 커밋에 연결된 별도 CI 상태가 없으므로 최종 완료 판정은 로컬 Unity에서 EditMode / PlayMode 테스트와 Console Error 0을 확인한 결과를 기준으로 한다.
