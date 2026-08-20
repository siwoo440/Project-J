# 31일차 개발일지 - 체크포인트 건너뛰기 및 최고값 유지

## 1. 개발 목표

31일차의 목표는 30일차에서 구현한 체크포인트 기본 저장 시스템을 확장하여, 플레이어가 체크포인트를 순서대로 밟지 않아도 더 높은 체크포인트를 직접 활성화할 수 있게 하고 한 번 도달한 최고 체크포인트보다 낮은 값으로는 되돌아가지 않도록 만드는 것이다.

현재 기준 커밋:

```text
463fb4b9195e66b4c8082ef66c67d9d1fd94829a
```

현재 커밋 메시지:

```text
32
```

다만 실제 변경 내용은 31일차의 체크포인트 건너뛰기·최고값 유지 구현에 해당한다.

---

## 2. 핵심 규칙

31일차부터 체크포인트 갱신 규칙은 다음과 같다.

```text
새 Checkpoint > 현재 Checkpoint
→ 갱신

새 Checkpoint == 현재 Checkpoint
→ 무시

새 Checkpoint < 현재 Checkpoint
→ 무시
```

예:

```text
START → CP3
= CP3 저장

CP3 → CP1
= CP3 유지

CP1 → CP4
= CP4 저장

CP4 → CP2
= CP4 유지
```

---

## 3. 체크포인트 건너뛰기 허용

체크포인트는 반드시 순서대로 밟을 필요가 없다.

예:

```text
START
↓
CP1 건너뜀
↓
CP3 직접 접촉
↓
CurrentCheckpoint = CP3
```

또한:

```text
START → CP4
```

도 정상적으로 허용된다.

하위 체크포인트를 먼저 활성화했는지 여부는 상위 체크포인트 활성화 조건으로 사용하지 않는다.

---

## 4. 최고 체크포인트 유지

한 번 더 높은 체크포인트를 저장한 뒤 낮은 체크포인트에 다시 접촉해도 현재 최고값은 유지한다.

예:

```text
CP4 활성화
↓
CP1 접촉
↓
CurrentCheckpoint = CP4 유지
```

동시에 다음 값도 CP4 기준을 그대로 유지한다.

```text
CurrentCheckpointId
CurrentCheckpoint
RespawnPosition
RespawnRotation
```

따라서 향후 Respawn 시스템은 항상 플레이어가 도달한 가장 높은 체크포인트를 기준으로 사용할 수 있다.

---

## 5. PlayerCheckpointTracker 수정

수정 파일:

```text
Assets/ProjectJ/Runtime/Checkpoint/PlayerCheckpointTracker.cs
```

`ActivateCheckpoint()`에 현재 체크포인트와 새 체크포인트를 비교하는 검증을 추가했다.

새 체크포인트가 더 높은 경우에만 실제 저장을 수행한다.

```text
IsHigherCheckpoint(candidate, current)
```

비교 결과가 false이면:

```text
CurrentCheckpointId 변경 X
CurrentCheckpoint 변경 X
RespawnPosition 변경 X
RespawnRotation 변경 X
CheckpointChanged Event 발생 X
```

상태로 유지한다.

---

## 6. CheckpointId 순서 사용

현재 CheckpointId는 다음 순서다.

```text
Start = 0
CP1 = 1
CP2 = 2
CP3 = 3
CP4 = 4
```

따라서 31일차에서는 enum 값을 정수로 비교하여 상위 체크포인트인지 판단한다.

```text
candidate > current
```

이면 높은 체크포인트로 처리한다.

---

## 7. 동일 체크포인트 중복 활성화 방지

같은 체크포인트를 다시 밟는 경우에는 재활성화하지 않는다.

예:

```text
CP2 활성화
↓
CP2 다시 접촉
↓
변화 없음
```

이 경우:

```text
ActivateCheckpoint() = false
CheckpointChanged Event 추가 발생 X
```

로 처리한다.

이를 통해 향후 체크포인트 효과음이나 연출이 반복 실행되는 문제를 막을 수 있다.

---

## 8. CheckpointChanged Event

새로운 최고 체크포인트가 실제로 저장된 경우에만:

```text
CheckpointChanged
```

Event를 발생시킨다.

예:

```text
START → CP2
→ Event 1회

CP2 → CP2
→ Event 없음

CP2 → CP1
→ Event 없음

CP2 → CP4
→ Event 1회
```

---

## 9. Respawn 데이터 유지

낮은 체크포인트 접촉 시 Respawn 정보도 덮어쓰지 않는다.

예:

```text
CP4 저장
RespawnPosition = CP4 위치

↓ CP1 접촉

RespawnPosition = CP4 위치 유지
```

RespawnRotation도 동일한 방식으로 유지한다.

31일차에서는 실제 Respawn 이동을 수행하지 않으며, 최고 체크포인트의 부활 기준 정보만 안정적으로 유지한다.

---

## 10. EditMode 테스트 수정

수정 파일:

```text
Assets/ProjectJ/Tests/EditMode/CheckpointTests.cs
```

31일차 규칙에 맞춰 기존 30일차 테스트를 수정하고 새 검증 항목을 추가했다.

주요 테스트:

- 새 Tracker가 Start에서 시작하는지 확인
- 높은 Checkpoint 활성화 시 ID와 RespawnPoint가 저장되는지 확인
- 낮은 CP를 건너뛰고 CP3를 직접 활성화할 수 있는지 확인
- CP1 이후 CP3가 정상적으로 최고값을 갱신하는지 확인
- CP4 이후 CP1이 최고값을 덮어쓰지 못하는지 확인
- 낮은 CP 접촉 시 RespawnPosition과 RespawnRotation이 유지되는지 확인
- 같은 Checkpoint를 두 번 활성화하지 않는지 확인
- 동일 CP 접촉 시 CheckpointChanged Event가 중복 발생하지 않는지 확인
- START에서 CP4를 바로 활성화할 수 있는지 확인
- null Checkpoint를 거부하는지 확인
- CheckpointId 비교 함수가 예상 결과를 반환하는지 확인

---

## 11. 대표 테스트 시나리오

### START → CP3

```text
CurrentCheckpoint = Start
↓
CP3 접촉
↓
CurrentCheckpoint = CP3
```

정상.

### CP4 → CP1

```text
CurrentCheckpoint = CP4
↓
CP1 접촉
↓
CurrentCheckpoint = CP4
```

정상.

### CP2 → CP2

```text
CurrentCheckpoint = CP2
↓
CP2 재접촉
↓
변화 없음
```

정상.

### START → CP4

```text
CurrentCheckpoint = Start
↓
CP4 직접 접촉
↓
CurrentCheckpoint = CP4
```

정상.

---

## 12. 생성·수정·삭제 요소

이번 일차에서는 새 Runtime 시스템을 추가하지 않고 기존 30일차 구조를 수정했다.

수정 파일:

```text
Assets/ProjectJ/Runtime/Checkpoint/PlayerCheckpointTracker.cs
Assets/ProjectJ/Tests/EditMode/CheckpointTests.cs
```

새 파일:

```text
없음
```

삭제 파일:

```text
없음
```

---

## 13. 이번 일차에서 구현하지 않은 기능

다음 기능은 아직 구현하지 않았다.

```text
추락 감지
맵 하단 Fall Limit
실제 Respawn 이동
Rigidbody 속도 초기화
외부 힘 초기화
3초 보호 상태
```

31일차는 체크포인트 진행 데이터의 최고값 유지까지만 담당한다.

---

## 14. 수동 검증 체크리스트

- [ ] Unity Console Error 0
- [ ] EditMode 전체 Green
- [ ] START에서 CP3 직접 활성화 가능
- [ ] START에서 CP4 직접 활성화 가능
- [ ] CP3 이후 CP1 접촉 시 CP3 유지
- [ ] CP4 이후 CP2 접촉 시 CP4 유지
- [ ] 같은 CP를 재접촉해도 값이 변하지 않음
- [ ] 낮은 CP 접촉 후 Respawn 위치가 바뀌지 않음
- [ ] 높은 CP 활성화 시 HUD가 새 최고 Checkpoint로 갱신됨
- [ ] PlayMode 테스트 전체 Green
- [ ] Console Error 0

---

## 15. 개발 결과

31일차에서는 체크포인트를 순서대로 밟지 않아도 상위 체크포인트를 바로 활성화할 수 있도록 하고, 한 번 도달한 가장 높은 체크포인트보다 낮은 체크포인트가 현재 진행 상태를 덮어쓰지 못하도록 수정했다.

최종 규칙:

```text
건너뛰기 허용
+
최고값만 갱신
+
동일·낮은 Checkpoint 무시
+
Respawn 정보 최고값 유지
```

이 구조를 통해 이후 추락 및 Respawn 시스템은 플레이어가 도달한 가장 높은 체크포인트를 안정적으로 기준점으로 사용할 수 있다.

GitHub 저장소에는 현재 이 커밋에 대한 별도 CI 상태가 등록되어 있지 않으므로 최종 완료 판정은 Unity 로컬 환경에서 EditMode / PlayMode 테스트와 Console Error 0을 확인한 뒤 확정한다.
