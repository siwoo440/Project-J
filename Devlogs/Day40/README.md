# 40일차 개발일지 - 밀치기 Target 선택

## 1. 개발 목표

40일차의 목표는 밀치기 실행 시 플레이어 전방의 유효 대상 중 **가장 가까운 한 명만 Target으로 선택하는 구조**를 구현하는 것이다.

현재 기준 커밋:

```text
3c7f23e78f6be177b189fb75a96ac73c1a33e215
```

현재 커밋 메시지:

```text
40
```

이번 일차에서는 실제 밀치기 힘이나 쿨타임은 구현하지 않고, Target 선택 정확성만 구현한다.

---

## 2. PlayerPushTargetSelector 추가

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Push/PlayerPushTargetSelector.cs
```

Player 전방의 Player Collider를 검색하고 유효한 밀치기 대상 중 가장 가까운 한 명을 선택한다.

기본 설정값:

```text
Search Range = 2.5
Search Angle = 90
Player Layers = Player
```

Player Prefab에도 `PlayerPushTargetSelector` 컴포넌트를 추가했다.

---

## 3. Target 탐색 흐름

Target 선택 흐름은 다음과 같다.

```text
TryFindTarget()
↓
Player 주변 Search Range 검색
↓
Player Layer Collider 수집
↓
자기 자신 제외
↓
비활성 Collider 제외
↓
완주 Player 제외
↓
정면 Search Angle 검사
↓
거리 비교
↓
가장 가까운 Player 1명 선택
```

검색에는:

```text
Physics.OverlapSphereNonAlloc
```

을 사용한다.

검색 결과 버퍼를 재사용해 매 탐색마다 새로운 Collider 배열을 생성하지 않도록 구성했다.

---

## 4. 전방 각도 판정

플레이어의:

```text
transform.forward
```

를 기준으로 대상 방향과의 각도를 계산한다.

기본 `Search Angle = 90`이면 좌우 각각 45도 범위가 유효한 Target 영역이 된다.

예:

```text
        각도 밖

          B

A  →      C

          D
```

A의 전방 범위 안에 있는 Player만 후보가 된다.

뒤쪽이나 옆쪽의 Player는 거리가 더 가까워도 선택되지 않는다.

---

## 5. 최근접 Target 선택

여러 Player가 모두 유효 범위 안에 있으면 실행자와 가장 가까운 한 명만 선택한다.

예:

```text
A → B       C
    1.2m    2.0m
```

결과:

```text
Target = B
```

동시에 여러 명을 Target으로 반환하지 않는다.

---

## 6. 자기 자신 제외

Physics Query에는 자신의 Collider도 포함될 수 있으므로:

```text
candidate == selfFinishState
```

조건으로 실행자 자신을 Target에서 제외한다.

따라서 주변에 다른 Player가 없으면:

```text
TryFindTarget() = false
CurrentTarget = null
```

이 된다.

---

## 7. 완주 Player 제외

기존 `PlayerFinishState`를 이용해:

```text
IsFinished == true
```

인 Player를 Target에서 제외한다.

완주 Player는 경기 경쟁에서 빠진 상태이므로 밀치기 대상이 될 수 없다.

또 실행자 자신이 이미 완주한 경우에도 Target 탐색을 수행하지 않는다.

---

## 8. CurrentTarget 관리

Target 탐색에 성공하면:

```text
CurrentTarget
```

에 선택된 `PlayerFinishState`를 저장한다.

탐색에 실패하면:

```text
CurrentTarget = null
```

로 초기화한다.

별도로:

```text
ClearTarget()
```

을 제공해 필요할 때 현재 Target을 명시적으로 해제할 수 있다.

---

## 9. Player Collider 유지

39일차에서 Player 간 상시 몸체 충돌은 제거했지만 Collider 자체는 유지했다.

따라서 40일차에서는 같은 Player Collider를 Physics Query 대상으로 사용한다.

구조:

```text
평상시 Player ↔ Player
→ 서로 통과

밀치기 Target 탐색
→ Player Collider 검색 가능
```

상시 물리 충돌과 게임플레이 Target 판정을 분리한 구조다.

---

## 10. Scene Gizmo

`PlayerPushTargetSelector`가 붙은 Player를 Scene View에서 선택하면 검색 범위를 확인할 수 있도록 Gizmo를 추가했다.

표시:

```text
Wire Sphere
→ Search Range

전방 좌우 Line
→ Search Angle 경계
```

이를 이용해 밀치기 범위와 각도를 시각적으로 확인할 수 있다.

---

## 11. Player Prefab 수정

수정 파일:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
```

추가 컴포넌트:

```text
PlayerPushTargetSelector
```

설정:

```text
Search Range = 2.5
Search Angle = 90
Player Layers = Player
Self Finish State = 자동 탐색
```

기존 Player Layer는 그대로 유지한다.

---

## 12. EditMode 테스트 추가

새 테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/PlayerPushTargetSelectorTests.cs
```

주요 검증 항목:

```text
정면 Player 선택
여러 정면 Player 중 최근접 대상 선택
뒤쪽 Player 제외
Search Angle 밖 Player 제외
Search Range 밖 Player 제외
자기 자신 제외
FINISH Player 제외
완주한 실행자의 Target 탐색 차단
ClearTarget 동작
```

---

## 13. 생성 및 수정 요소

### 생성

```text
Assets/ProjectJ/Runtime/Push.meta

Assets/ProjectJ/Runtime/Push/
├─ PlayerPushTargetSelector.cs
└─ PlayerPushTargetSelector.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ PlayerPushTargetSelectorTests.cs
└─ PlayerPushTargetSelectorTests.cs.meta
```

### 수정

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
```

### 삭제

```text
없음
```

---

## 14. 이번 일차에서 구현하지 않은 기능

40일차에서는 Target 선택까지만 구현한다.

아직 구현하지 않은 기능:

```text
LMB 밀치기 실행 연결
실제 Rigidbody 밀치기 힘
밀치기 쿨타임
동시 밀치기 힘 누적
서버 권한 판정
밀치기 애니메이션
밀치기 효과음
```

실제 밀치기 힘과 쿨타임은 다음 개발 단계에서 연결한다.

---

## 15. 수동 테스트 기준

Player와 여러 Target Player를 배치해 다음을 확인한다.

```text
정면 1명
→ 해당 Player 선택

정면에 가까운 Player와 먼 Player
→ 가까운 Player 선택

뒤쪽 Player
→ 선택되지 않음

옆쪽 Search Angle 밖 Player
→ 선택되지 않음

Search Range 밖 Player
→ 선택되지 않음

완주 Player
→ 선택되지 않음
```

Scene View에서는 Gizmo로 Search Range와 Search Angle을 함께 확인한다.

---

## 16. 자동 테스트 기준

Unity에서:

```text
Window
→ General
→ Test Runner
→ EditMode
→ Run All
```

을 실행한다.

확인 항목:

```text
Target 선택 테스트 전체 Green
기존 39일차 Player Collision 테스트 Green
기존 Phase 3 테스트 회귀 오류 없음
Console Error 0
```

---

## 17. 개발 결과

40일차에서는 밀치기 기능의 첫 단계로 **전방 범위 내 최근접 Player 한 명을 선택하는 Target Selector**를 구현했다.

최종 흐름:

```text
Player 전방 검색
↓
유효 Player Collider 수집
↓
자기 자신 / 완주 대상 제외
↓
거리 / 각도 검사
↓
최근접 Player 1명
↓
CurrentTarget 저장
```

이를 통해 다음 단계에서 실제 밀치기 힘과 쿨타임을 Target 선택 로직과 분리해서 연결할 수 있는 기반을 마련했다.

---

## 18. 저장소 검토 메모

GitHub 최신 커밋에는 `PlayerPushTargetSelector`, EditMode 테스트, Push 폴더 메타 파일, Player Prefab 컴포넌트 추가가 모두 포함되어 있다.

정적 코드 검토 기준으로 40일차 목표를 막는 문제는 확인되지 않았다.

다만 GitHub에는 해당 커밋에 연결된 별도 CI 상태가 없으므로 최종 완료 판정은 로컬 Unity에서:

```text
EditMode 테스트 통과
PlayMode 기존 테스트 통과
Console Error 0
```

을 확인한 결과를 기준으로 한다.
