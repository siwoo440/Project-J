# Project J 개발 일지 - 134일차

## 개발 주제

**Pickup 테스트장 재배치·5초 네트워크 재생성 및 회귀 오류 수정**

134일차에는 133일차에서 만든 30종 Network Pickup 통합 테스트 환경을 실제 반복 회귀 테스트에 더 적합한 형태로 정리했다.

기존 시작 지점 옆에 있던 테스트 구역을 경기 진행 방향의 반대편으로 재배치하고, 30개 Pickup을 5열 × 6행으로 정돈했다. 각 Pickup에는 상자 형태의 임시 시각 요소를 추가했으며, 정상 획득된 Pickup이 State Authority 기준 5초 후 다시 나타나도록 네트워크 재생성 흐름을 구현했다.

작업 과정에서 확인된 Fusion Spawn 생명주기 오류와 Day111 Pool Ball Scene Installer의 기준 Pickup 이름 불일치도 함께 수정했다.

---

## 1. 134일차 실제 구현 범위

이번 일차에서 실제로 적용한 범위는 다음과 같다.

- Pickup 통합 테스트 구역을 시작 지점 뒤쪽으로 재배치
- 30종 Pickup을 5열 × 6행으로 정렬
- 각 Pickup에 Cube 기반 상자 Visual 추가
- 정상 획득 시 Pickup 즉시 숨김
- State Authority 기준 5초 재생성
- Host와 Client의 획득/재생성 상태 동기화 기반 유지
- Inventory 저장 실패 시 Pickup 유지
- Fusion Spawn 이전 Networked Property 접근 오류 수정
- Day111 Pool Ball 자동 설치기의 Mine Pickup 이름 불일치 수정

기획서의 원래 134일차 목표인 전체 아이템 상호작용·보호·은신·반사 회귀 통합을 수행하기 위한 테스트 기반을 강화한 작업이다.

전체 아이템 효과 조합의 최종 회귀 완료 여부는 실제 Host/Client PlayMode 또는 Development Build 테스트 결과를 기준으로 별도 판정한다.

---

## 2. Pickup 테스트 구역 재배치

133일차 테스트 맵은 시작 지점 옆에 배치되어 있었다.

134일차에서는 경기 시작 지점과 본 코스의 진행을 방해하지 않도록 `=== ITEM PICKUP TEST MAP ===` 전체를 시작 지점 뒤쪽 방향으로 재배치했다.

개별 Pickup과 Floor를 각각 수동 이동하는 대신 테스트 맵 Root 전체의 위치와 회전을 조정하는 방식으로 기존 구조를 보존했다.

주요 대상:

- `=== ITEM PICKUP TEST MAP ===`
- `=== ITEM PICKUPS ===`
- `TestArena_Floor`
- `TestArena_Bridge`
- 30개 Network Pickup

---

## 3. 30개 Pickup 5열 × 6행 정리

30종 아이템을 한눈에 확인하고 반복 획득 테스트를 하기 쉽도록 Pickup을 다음 규격으로 배치했다.

- 열 수: 5
- 행 수: 6
- 총 Pickup 수: 30
- 열 간격: 6
- 행 간격: 6

기존 Network Item ID와 ItemDefinition 연결은 그대로 유지한다.

따라서 배치 변경이 아이템 ID나 실제 지급 데이터에는 영향을 주지 않도록 했다.

---

## 4. Pickup 상자 Visual 추가

각 Network Pickup 아래에 테스트용 Cube Visual을 추가했다.

오브젝트 이름:

`Day134_PickupBoxVisual`

상자 Visual의 Collider는 제거해 실제 획득용 `BoxCollider` Trigger와 충돌하지 않도록 구성했다.

이 Visual은 아이템 획득 상태와 함께 숨겨지고 재생성 시 다시 표시된다.

---

## 5. State Authority 5초 재생성

`ProjectJNetworkItemBox`에 5초 재생성 흐름을 추가했다.

핵심 구성:

- `respawnSeconds = 5f`
- `[Networked] TickTimer RespawnTimer`
- 정상 획득 성공 시 Timer 시작
- `FixedUpdateNetwork()`에서 State Authority만 Timer 만료 검사
- 5초 만료 후 `NetworkCollected = false`
- 획득자·지급 ID·저장 슬롯 기록 초기화
- Trigger와 Renderer 다시 활성화

재생성 시간의 판정은 로컬 `Update()` 시간이 아니라 Fusion Simulation 기준 `TickTimer`를 사용한다.

따라서 Host가 재생성 시점을 확정하고 Networked 상태를 통해 다른 Peer가 동일 상태를 보도록 구성했다.

---

## 6. Inventory 저장 실패 시 Pickup 유지

Pickup은 Inventory 저장이 실제로 성공했을 때만 획득 완료 상태로 변경한다.

현재 흐름:

1. Player가 Pickup Trigger 접촉
2. State Authority가 획득 후보 확인
3. `ProjectJNetworkItemInventory` 검사
4. `TryStoreWorldItemAuthority()` 호출
5. 저장 실패 시 즉시 종료
6. 저장 성공 시 `NetworkCollected = true`
7. 5초 Respawn Timer 시작

따라서 Inventory가 가득 차 있거나 현재 아이템을 받을 수 없는 상태라면 Pickup은 사라지지 않고 재생성 Timer도 시작하지 않는다.

---

## 7. Fusion Spawn 생명주기 오류 수정

테스트 중 다음 오류가 반복 발생했다.

`InvalidOperationException: Networked properties can only be accessed when Spawned() has been called.`

원인은 Unity의 `Update()`가 Fusion의 `Spawned()` 완료 전에도 실행될 수 있는데, 기존 `Update()`에서 바로 `NetworkCollected`를 읽고 있었기 때문이다.

`ProjectJNetworkItemBox.Update()`에 NetworkObject 유효성 검사를 추가했다.

현재 동작:

- `Object == null`이면 종료
- `Object.IsValid == false`이면 종료
- Fusion Spawn이 유효한 시점부터 `ApplyCollectedPresentation()` 실행

이를 통해 Spawn 이전 Networked Property 접근을 차단했다.

---

## 8. Day111 Pool Ball Installer 기준 이름 수정

Script Reload 후 자동 실행되는 Day111 Pool Ball Installer에서 다음 오류가 확인됐다.

`[Project J/Day111] 복제 기준 Network Pickup을 찾지 못했습니다. / Pickup_09_mine_A`

실제 Day49 테스트 Scene의 Mine Pickup 이름은 다음과 같았다.

`Pickup_9_mine_A`

하지만 Installer는 다음 이름을 찾고 있었다.

`Pickup_09_mine_A`

따라서 기준 이름을 실제 Scene과 일치하도록 수정했다.

변경:

`Pickup_09_mine_A` → `Pickup_9_mine_A`

Pool Ball 6개 생성, Definition 교체, Fusion Bake, SortKey 검증 등 기존 Day111 설치 로직은 그대로 유지했다.

---

## 9. 변경 파일

134일차 최신 커밋에서 확인되는 주요 변경 파일은 다음과 같다.

### 수정

- `Assets/ProjectJ/Editor/ProjectJDay111PoolBallSceneInstaller.cs`
- `Assets/ProjectJ/Network/Fusion/World/ProjectJNetworkItemBox.cs`
- `Assets/ProjectJ/Scenes/Game.unity`

### 추가

- `Assets/ProjectJ/Editor/ProjectJDay134PickupTestAreaSetup.cs`
- `Assets/ProjectJ/Editor/ProjectJDay134PickupTestAreaSetup.cs.meta`

`ProjectJDay134PickupTestAreaSetup`은 Game Scene의 기존 Pickup 테스트 구역을 134일차 규격으로 정리하기 위한 Editor 도구다.

---

## 10. 최신 커밋 확인

개발일지 작성 시점의 `main` 최신 커밋:

- SHA: `90bca07e8e1042940d957b712b9169f12d900fee`
- 현재 커밋 메시지: `134`
- 이전 커밋: `e1beaa6932d47b55f9ea230c6aec72527362bc59`
- 이전 커밋 제목: `133일차 : 30종 아이템 Pickup 통합 테스트 맵 및 Fusion Scene 획득 구조 구현`

최신 커밋에서 이번 작업 중 확인된 두 오류의 수정 코드가 모두 반영된 것을 확인했다.

- `ProjectJNetworkItemBox.Update()`의 Fusion Spawn 유효성 검사
- Day111 Installer의 `Pickup_9_mine_A` 기준 이름

---

## 11. GitHub 정적 검토 결과

GitHub 최신 `main` 기준으로 다음 항목을 확인했다.

- `ProjectJNetworkItemBox`에 5초 `TickTimer` 재생성 구조 존재
- State Authority만 재생성 Timer 처리
- Inventory 저장 성공 후에만 Pickup 획득 처리
- Spawn 이전 `NetworkCollected` 접근을 막는 `Object.IsValid` 검사 존재
- Day111 Installer의 기준 Pickup 이름이 실제 Scene 이름과 일치
- 134일차 Editor Setup 스크립트와 Game Scene 변경이 함께 반영됨

GitHub Combined Status에는 별도의 CI 상태 체크가 등록되어 있지 않았다.

따라서 저장소 정적 확인 기준으로 현재까지 보고된 두 오류는 수정된 상태지만, GitHub 상태만으로 Unity 전체 컴파일 성공이나 Host/Client 런타임 테스트 통과를 확정하지는 않는다.

---

## 12. Unity 최종 확인 항목

134일차 완료 판정 전 다음 항목을 확인한다.

1. Unity Console C# 컴파일 Error 0건
2. `Networked properties can only be accessed when Spawned() has been called` 오류 미발생
3. Day111 자동 Installer의 `Pickup_09_mine_A` 오류 미발생
4. Pickup 테스트 구역이 시작 지점 뒤쪽에 위치
5. Pickup 30개가 5열 × 6행으로 표시
6. 각 Pickup에 상자 Visual 표시
7. 정상 획득 시 Host에서 Pickup 즉시 사라짐
8. Client에서도 동일 Pickup이 사라짐
9. 약 5초 후 Host와 Client 모두에서 Pickup 재등장
10. 재등장한 Pickup을 다시 획득 가능
11. Inventory 저장 실패 시 Pickup이 사라지지 않음
12. 여러 Pickup을 연속 획득해도 Respawn Timer가 서로 간섭하지 않음
13. Pool Ball Stack 획득 규칙이 기존과 동일하게 유지
14. Game Scene 본 코스 진행을 Pickup 테스트 구역이 방해하지 않음

---

## 13. 기획서 134일차와의 관계

기획서의 134일차 개발 방향은 다음 주제다.

**전체 아이템 상호작용·보호·은신·반사 회귀 통합**

원래 목표는 현재 구현된 전체 아이템을 한 경기에서 조합하여 Jelly Shield, 부활 보호, External Force, 투명 망토의 자동 추적 제외, 손거울 반사 소유권 이전, 지속 효과 중첩 제한과 Respawn 초기화를 함께 검증하는 것이다.

이번 작업에서는 그 전체 회귀를 안정적으로 반복하기 위한 Pickup 테스트 환경, 5초 재생성, 네트워크 표시 생명주기 및 과거 Editor Installer 회귀 오류를 먼저 정리했다.

따라서 코드와 Scene 기반은 134일차 회귀 테스트를 수행할 수 있는 상태로 개선됐지만, 전체 아이템 조합의 실제 Host/Client 회귀 테스트까지 통과해야 기획서의 134일차 전체 목표를 완전히 검증했다고 볼 수 있다.

---

## 134일차 결과

30종 Network Pickup 통합 테스트 환경을 반복 사용하기 쉬운 구조로 개선했다.

테스트 구역을 시작 지점 뒤쪽으로 이동하고 30개 Pickup을 5열 × 6행으로 정돈했으며, Pickup에 상자 Visual과 State Authority 기준 5초 재생성 흐름을 추가했다.

또한 실제 테스트 과정에서 발견된 Fusion Spawn 이전 Networked Property 접근 오류를 차단하고, Day111 Pool Ball Installer의 잘못된 Mine Pickup 이름도 수정했다.

현재 GitHub 최신 소스에서는 보고된 두 오류에 대한 수정이 반영되어 있으며, 다음 단계는 Unity Host/Client 환경에서 5초 재생성과 전체 아이템 상호작용 회귀를 실제로 검증하는 것이다.
