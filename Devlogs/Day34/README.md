# 34일차 개발일지 - 3초 부활 보호 시스템

## 1. 개발 목표

34일차의 목표는 체크포인트 부활 직후 플레이어에게 3초 동안 보호 상태를 부여하고, 보호 중에는 적대적인 밀치기·방해 효과만 차단하면서 이동과 점프 등 자신의 조작은 계속 허용하는 기반을 만드는 것이다.

현재 기준 커밋:

```text
78422c1983ccf45a830145cdf858f5a684f685dd
```

현재 커밋 메시지:

```text
34
```

이번 일차의 기본 흐름은 다음과 같다.

```text
Respawn 완료
↓
Respawned Event
↓
3초 보호 시작
↓
적대 효과 차단
↓
이동·점프는 유지
↓
3초 경과
↓
보호 종료
```

---

## 2. PlayerRespawnProtection

새 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Checkpoint/PlayerRespawnProtection.cs
```

Player 오브젝트에 추가되는 부활 보호 전용 컴포넌트다.

주요 역할:

- PlayerRespawnController 참조
- Respawned Event 구독
- Respawn 직후 3초 보호 시작
- 보호 남은 시간 계산
- 보호 상태 조회
- 적대 효과 수신 가능 여부 제공
- 보호 시작/종료 Event 제공
- 반복 Respawn 시 보호시간 다시 시작

---

## 3. 기존 Respawn 시스템과 연결

33일차의 PlayerRespawnController는 Respawn 완료 후:

```text
Respawned
```

Event를 발생시킨다.

PlayerRespawnProtection은 이 Event를 구독한다.

```text
PlayerRespawnController.Respawned
↓
PlayerRespawnProtection.StartProtection()
↓
3초 보호
```

따라서 기존 Respawn 로직을 수정하지 않고 보호 시스템을 독립적으로 연결했다.

---

## 4. 보호 시간

보호 기본 시간:

```text
3초
```

보호 시작 시:

```text
IsProtected = true
RemainingProtectionTime = 3
```

시간이 지나면서 RemainingProtectionTime이 감소한다.

3초가 끝나면:

```text
IsProtected = false
RemainingProtectionTime = 0
```

으로 변경한다.

---

## 5. 로컬 시간 사용

현재는 실제 서버 시간이 연결되지 않은 개발 단계이므로 로컬 시간을 서버 시간의 임시 대체값으로 사용한다.

Runtime 기준:

```text
Time.unscaledTimeAsDouble
```

을 사용한다.

게임의 Time Scale 영향을 받지 않는 시간으로 보호 종료 시점을 계산하도록 구성했다.

향후 네트워크 서버 시간이 연결되면 이 기준을 서버 authoritative 시간으로 교체할 수 있다.

---

## 6. 보호 종료 시각 방식

프레임 수로 3초를 계산하지 않고 종료 시각을 저장한다.

개념:

```text
보호 시작 시간
+
3초
=
보호 종료 시간
```

매 Update에서 현재 시간과 종료 시간을 비교한다.

이 방식은 FPS에 따라 보호시간이 달라지는 문제를 방지한다.

---

## 7. 적대 효과 차단 판정

보호 시스템은 다음 값을 제공한다.

```text
CanReceiveHostileEffect
```

보호 중:

```text
CanReceiveHostileEffect = false
```

보호 종료 후:

```text
CanReceiveHostileEffect = true
```

또한 테스트 및 향후 효과 시스템 연결을 위한:

```text
TryAcceptHostileEffect()
```

메서드를 제공한다.

보호 중에는 false, 보호가 끝난 뒤에는 true를 반환한다.

---

## 8. 현재 차단 대상으로 보는 효과

현재 단계에서 보호 대상으로 정의하는 것은 적대적 효과다.

예정 연결 대상:

```text
다른 플레이어의 밀치기
적대 아이템의 넉백
상대가 발생시킨 방해 효과
적대적인 외력
```

현재 프로젝트에 실제 밀치기·적대 아이템 시스템이 완성되어 있지 않기 때문에 34일차에서는 공통 수신 판정만 구현한다.

향후 해당 시스템에서:

```text
CanReceiveHostileEffect
```

를 확인하도록 연결할 수 있다.

---

## 9. 보호 중에도 허용되는 기능

부활 보호는 Player 자체를 비활성화하는 무적 상태가 아니다.

보호 중에도 다음 기능은 계속 사용할 수 있다.

```text
이동
점프
달리기
앉기
자신의 정상적인 Rigidbody 이동
체크포인트 활성화
```

PlayerInput이나 이동 스크립트를 비활성화하지 않는다.

---

## 10. 반복 Respawn 처리

보호 중 다시 Respawn하면 남은 보호시간에 3초를 더하지 않는다.

예:

```text
첫 Respawn
→ 3초 보호 시작

1초 경과
→ 2초 남음

다시 Respawn
→ 다시 3초부터 시작
```

즉 새로운 Respawn이 발생한 시점을 기준으로 보호 종료 시각을 다시 계산한다.

---

## 11. ProtectionStarted / ProtectionEnded Event

보호 시스템은 다음 Event를 제공한다.

```text
ProtectionStarted
ProtectionEnded
```

향후 다음 기능과 연결할 수 있다.

```text
보호 VFX
보호 HUD
보호 효과음
캐릭터 표시 변경
네트워크 상태 동기화
```

34일차에서는 보호 상태와 시간 관리까지만 구현한다.

---

## 12. RespawnProtectionDebugView

새 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Checkpoint/RespawnProtectionDebugView.cs
```

화면에 다음 내용을 표시한다.

```text
Respawn Protection : ON / OFF
Remaining : 남은 보호시간
Hostile Effect : 테스트 결과
```

보호 중 예:

```text
Respawn Protection : ON
Remaining : 2.45s
Hostile Effect : BLOCKED
```

보호 종료 후:

```text
Respawn Protection : OFF
Remaining : 0.00s
Hostile Effect : ACCEPTED
```

---

## 13. Debug 테스트 버튼

테스트 화면에는 다음 버튼을 제공한다.

```text
Direct Respawn
Test Hostile Effect
```

### Direct Respawn

기존 PlayerRespawnController.RequestRespawn()을 호출한다.

Respawn이 완료되면 Respawned Event를 통해 3초 보호가 자동 시작된다.

### Test Hostile Effect

현재 보호 상태에서 적대 효과를 받을 수 있는지 검사한다.

```text
보호 중
→ BLOCKED

보호 종료
→ ACCEPTED
```

---

## 14. Player Prefab 구조

현재 체크포인트·추락·부활 관련 Player 구조는 다음과 같다.

```text
Player
├─ PlayerCheckpointTracker
├─ PlayerFallTracker
├─ PlayerRespawnController
└─ PlayerRespawnProtection
```

각 책임:

```text
PlayerCheckpointTracker
→ 최고 체크포인트와 Respawn 위치 저장

PlayerFallTracker
→ 추락 감지

PlayerRespawnController
→ 실제 Respawn 처리

PlayerRespawnProtection
→ Respawn 직후 3초 적대 효과 보호
```

---

## 15. Editor 자동 설정

새 Editor 스크립트:

```text
Assets/ProjectJ/Editor/Day34RespawnProtectionSetup.cs
```

Unity 메뉴:

```text
ProjectJ
→ Day34
→ Setup Respawn Protection
```

실행 시 다음을 자동 처리한다.

```text
Player.prefab 확인
↓
PlayerRespawnController 확인
↓
PlayerRespawnProtection 추가
↓
보호시간 3초 설정
↓
Day25 고정맵 Debug 연결
↓
Day34 테스트 Scene 생성
```

---

## 16. Day34 수동 테스트 Scene

생성 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day34/
└─ Day34_RespawnProtectionTest.unity
```

Day33 체크포인트 Respawn 테스트 Scene을 기반으로 생성된다.

대표 테스트:

```text
Direct Respawn
↓
Protection ON
↓
3초 감소
↓
Protection OFF
```

그리고:

```text
Protection ON
↓
Test Hostile Effect
↓
BLOCKED
```

보호 종료 후:

```text
Test Hostile Effect
↓
ACCEPTED
```

를 확인한다.

---

## 17. EditMode 자동 테스트

새 테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/PlayerRespawnProtectionTests.cs
```

주요 검증 항목:

- Respawn Event 이후 3초 보호가 시작되는지 확인
- 3초 이전에는 보호가 유지되는지 확인
- 정확히 3초 시점에 보호가 종료되는지 확인
- 보호 중 적대 효과가 차단되는지 확인
- 보호 종료 후 적대 효과가 허용되는지 확인
- 반복 Respawn 시 보호시간이 새 3초로 초기화되는지 확인
- ProtectionEnded Event가 한 번만 발생하는지 확인
- 보호 시작이 Rigidbody 선형 속도를 변경하지 않는지 확인
- 보호 시작이 Rigidbody 회전 속도를 변경하지 않는지 확인

---

## 18. 생성 및 수정 요소

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Checkpoint/PlayerRespawnProtection.cs
Assets/ProjectJ/Runtime/Checkpoint/RespawnProtectionDebugView.cs
```

새 Editor 파일:

```text
Assets/ProjectJ/Editor/Day34RespawnProtectionSetup.cs
```

새 Test 파일:

```text
Assets/ProjectJ/Tests/EditMode/PlayerRespawnProtectionTests.cs
```

수정 요소:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
Assets/ProjectJ/Tests/Manual/Day25/Day25_ModuleFixedMap.unity
```

새 수동 테스트 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day34/Day34_RespawnProtectionTest.unity
```

삭제 파일:

```text
없음
```

---

## 19. 이번 일차에서 구현하지 않은 기능

34일차에서는 다음 기능을 아직 실제 게임 시스템과 연결하지 않는다.

```text
실제 플레이어 밀치기 차단 연결
적대 아이템 차단 연결
실제 방해 효과 차단 연결
보호 VFX
보호 SFX
최종 HUD
서버 시간 동기화
```

현재는 향후 해당 시스템들이 사용할 공통 보호 상태와 수신 판정 기반을 구현한 단계다.

---

## 20. 검증 체크리스트

- [ ] Unity Console Error 0
- [ ] Player Prefab에 PlayerRespawnProtection 존재
- [ ] Respawn 직후 IsProtected = true
- [ ] 보호시간이 3초부터 감소
- [ ] 3초 이전에는 보호 유지
- [ ] 3초 시점 이후 보호 종료
- [ ] 보호 중 Hostile Effect = BLOCKED
- [ ] 보호 종료 후 Hostile Effect = ACCEPTED
- [ ] 보호 중 이동 가능
- [ ] 보호 중 점프 가능
- [ ] 보호 중 다시 Respawn 시 3초로 재시작
- [ ] EditMode 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

## 21. 개발 결과

34일차에서는 체크포인트 Respawn 완료 직후 3초 동안 적대 효과를 차단하는 부활 보호 시스템을 추가했다.

최종 흐름:

```text
Respawn 완료
↓
Respawned Event
↓
PlayerRespawnProtection
↓
3초 보호 시작
↓
적대 효과 차단
↓
플레이어 자신의 이동·점프 유지
↓
3초 경과
↓
일반 상태 복귀
```

현재 GitHub 최신 커밋에는 PlayerRespawnProtection, Debug View, Editor Setup, EditMode 테스트, Player Prefab 연결 및 Day34 수동 테스트 Scene이 포함되어 있다.

GitHub에는 해당 커밋의 별도 CI 상태가 등록되어 있지 않으므로 최종 완료 판정은 로컬 Unity에서 EditMode / PlayMode 테스트와 Console Error 0을 확인한 결과를 기준으로 한다.
