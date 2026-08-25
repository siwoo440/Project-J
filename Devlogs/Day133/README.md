# Project J 개발 일지 - 133일차

## 개발 주제

**신규 아이템 Pickup 일괄 배치 및 Fusion Bake·획득 통합 테스트 맵 구성**

133일차에는 지금까지 아이템 구현 단계에서 보류해 두었던 월드 Pickup 배치를 한 번에 통합했다.

기존 경기 코스에 즉시 분산 배치하지 않고, 시작 지점 옆에 별도의 Pickup 통합 테스트 구역을 만들어 현재 Network Item ID 1~30을 한 장소에서 확인할 수 있도록 구성했다.

또한 Scene에 배치되는 각 Pickup은 기존 `ItemPickup` 데이터와 `ProjectJNetworkItemBox` 네트워크 획득 구조를 함께 사용하도록 구성하고, Fusion Scene NetworkObject Bake가 적용된 상태로 `Game.unity`에 저장했다.

---

## 1. Pickup 통합 테스트 맵 추가

`Game.unity`에 다음 테스트 전용 Root를 추가했다.

`=== ITEM PICKUP TEST MAP ===`

테스트 구역은 시작 지점 옆에 별도의 플랫폼 형태로 배치했다.

주요 구성:

- `TestArena_Floor`
- `TestArena_Bridge`
- 외곽 Rail
- `TestArena_PickupGallery`
- `=== ITEM PICKUPS ===`
- 각 아이템별 Pedestal
- 각 Pickup의 임시 Cube Visual
- 각 Pickup 위 Network ID와 표시 이름 Label

테스트 맵의 목적은 실제 경기 밸런스 배치가 아니라 모든 아이템의 획득·인벤토리·사용 흐름을 한 장소에서 빠르게 검증하는 것이다.

---

## 2. 30개 Pickup 일괄 배치

현재 Network Item Catalog에 등록된 ID 1~30을 각각 1개씩 배치했다.

배치 대상:

1. 스프링 신발
2. 젤리 보호막
3. 바나나 쿠션
4. 풍선 나팔
5. 물총
6. 폭죽
7. 깃털 신발
8. 눈덩이
9. 지뢰
10. 풀 공
11. 제트팩
12. 망치
13. 폭탄
14. 복어 풍선옷
15. 먹물 문어
16. 낚시대
17. 갈고리
18. 비눗방울
19. 연막탄
20. 트램폴린
21. 거대 풍선
22. 카트
23. 되감기 시계
24. 유도탄
25. 소형화 물약
26. 가시 갑옷
27. 드론
28. 투명 망토
29. 저격 물총
30. 손거울

각 Pickup 이름은 Network ID 순서가 확인되도록 다음 형식을 사용한다.

`Pickup_XX_item_key`

예:

- `Pickup_01_spring_shoes`
- `Pickup_29_sniper_water_gun`
- `Pickup_30_hand_mirror`

---

## 3. Pickup 기본 구성

각 Pickup GameObject는 다음 요소를 사용한다.

- `NetworkObject`
- `ProjectJNetworkItemBox`
- `ItemPickup`
- `BoxCollider`
- `Rigidbody`
- 임시 Visual
- Label

Physics 기본 규칙:

- BoxCollider는 Trigger
- Rigidbody는 Kinematic
- Gravity 사용 안 함

기존 로컬 `ItemPickup` 직접 획득 기능은 비활성화하고, 실제 네트워크 획득 판정은 `ProjectJNetworkItemBox`가 담당한다.

`ItemPickup`은 어떤 ItemDefinition을 지급할 것인지 연결하는 데이터 역할로 사용한다.

---

## 4. 기존 ItemDefinition 재사용

새로운 ItemDefinition을 대량 생성하지 않고 기존 `Assets/ProjectJ/Data/Items` 데이터를 그대로 사용했다.

Scene 생성 시 기존 ItemDefinition과 Network Item Catalog를 기준으로 ID 1~30을 연결하도록 구성했다.

따라서 Pickup → ItemDefinition → Network Item ID → Inventory 흐름이 기존 아이템 데이터와 같은 기준을 사용한다.

---

## 5. Pickup 테스트 맵 구조

Pickup Gallery는 여러 아이템을 한눈에 확인하기 쉽도록 5열 × 6행 형태로 구성했다.

각 Pickup 아래에는 별도의 Pedestal을 배치해 테스트 중 어떤 Pickup에 접근하고 있는지 구분하기 쉽게 했다.

테스트 맵은 기존 경기 코스를 대체하지 않는다.

실제 경기에서 사용할 랜덤 배치, Spawn 확률, 코스별 밸런스 배치는 이후 별도 작업에서 조정한다.

---

## 6. Fusion Scene NetworkObject Bake

각 Pickup은 Scene NetworkObject이므로 단순 GameObject 생성으로 끝내지 않고 Fusion Bake가 필요한 구조다.

Pickup에 필요한 NetworkBehaviour 구성을 완료한 뒤 `NetworkObjectBaker`를 사용해 Scene NetworkObject Bake를 적용하도록 구성했다.

목표는 다음 문제를 한 번에 방지하는 것이다.

- Scene NetworkObject SortKey 누락
- Scene Object 중복 ID
- NetworkObject Bake 누락
- Host와 Client 간 Pickup 상태 불일치

이번 작업부터 개별 아이템 일차마다 Pickup을 따로 추가하던 보류 정책을 종료하고, 30개 Pickup을 한 번에 Scene에 통합했다.

---

## 7. 네트워크 획득 흐름

Pickup 획득의 기본 흐름은 다음과 같다.

1. Player가 Pickup Trigger에 접촉
2. State Authority가 획득 후보를 확인
3. 해당 Player Inventory가 아이템을 받을 수 있는지 검사
4. ItemDefinition을 Network Item ID로 변환
5. Inventory 저장 시도
6. 저장 성공 시 Pickup을 획득 완료 상태로 변경
7. 다른 Peer에서도 Pickup Visual과 Trigger 제거

Inventory가 가득 차 있거나 저장할 수 없다면 Pickup은 소비되지 않아야 한다.

동일 Pickup에 여러 Player가 접근하는 경우에도 네트워크 권한 기준으로 한 번만 획득되는 기존 `ProjectJNetworkItemBox` 구조를 그대로 사용한다.

---

## 8. Pool Ball Stack 회귀 대상

풀 공은 일반 단일 슬롯 아이템과 달리 Stack 규칙을 사용한다.

따라서 이번 Pickup 통합 테스트에서는 다음을 별도로 확인할 수 있다.

- 빈 Inventory에 Pool Ball 획득
- 기존 Pool Ball Stack에 추가 획득
- 최대 Stack 규칙 유지
- 일반 아이템과 2슬롯 규칙 충돌 여부

---

## 9. 최근 신규 아이템 Pickup 연결

이번 일차를 통해 최근 구현한 다음 아이템도 실제 월드 Pickup에서 Inventory로 들어가는 흐름을 검증할 수 있게 됐다.

- 드론
- 투명 망토
- 저격 물총
- 손거울

즉 이전까지의 코드/ItemDefinition 구현에서 한 단계 더 나아가 월드 획득 경로를 Scene에 연결했다.

---

## 10. EditMode Scene 테스트 추가

다음 테스트 파일을 추가했다.

`Assets/ProjectJ/Tests/EditMode/ProjectJDay133PickupIntegrationSceneTests.cs`

총 5개의 Scene 통합 테스트를 작성했다.

검증 대상:

- Pickup 통합 테스트 맵 Root 존재
- Floor / Bridge / Gallery 존재
- Pickup 정확히 30개
- Pickup 이름과 ItemDefinition ID가 1~30 순서와 일치
- Pickup마다 Trigger와 Kinematic Rigidbody 존재
- `Fusion.NetworkObject` 존재
- `ProjectJNetworkItemBox` 존재
- 모든 ItemDefinition 유효성
- Item ID 중복 없음

---

## 11. EditMode Assembly 참조 오류 수정

첫 적용 과정에서 Day133 테스트 파일이 다음 네트워크 네임스페이스를 직접 참조하면서 컴파일 오류가 발생했다.

- `Fusion`
- `ProjectJ.Networking.Fusion`

현재 `ProjectJ.Tests.EditMode.asmdef`는 네트워크 Fusion Assembly를 직접 참조하지 않는 구조이기 때문에 해당 테스트 파일에서 네트워크 타입을 직접 사용하는 것은 올바르지 않았다.

발생했던 대표 오류:

- `CS0246: Fusion namespace를 찾을 수 없음`
- `CS0234: ProjectJ.Networking.Fusion namespace를 찾을 수 없음`
- `CS0246: ProjectJNetworkItemBox를 찾을 수 없음`

수정 후에는 Network 타입을 컴파일 타임에 직접 참조하지 않고 Scene Component의 FullName을 이용해 다음 존재 여부를 검사한다.

- `Fusion.NetworkObject`
- `ProjectJ.Networking.Fusion.ProjectJNetworkItemBox`

따라서 기존 EditMode Assembly 구조를 변경하지 않고 Day133 Scene 검증을 유지하도록 수정했다.

---

## 12. 변경 파일

132일차 커밋과 비교한 133일차 최신 커밋에는 다음 4개 파일 변경이 포함되어 있다.

### 추가

- `Assets/ProjectJ/Network/Fusion/World/Editor.meta`
- `Assets/ProjectJ/Tests/EditMode/ProjectJDay133PickupIntegrationSceneTests.cs`
- `Assets/ProjectJ/Tests/EditMode/ProjectJDay133PickupIntegrationSceneTests.cs.meta`

### 수정

- `Assets/ProjectJ/Scenes/Game.unity`

Pickup 테스트 맵 생성에 사용한 1회성 Editor Installer는 적용 후 제거되기 때문에 최종 커밋에는 남지 않는다.

---

## 13. 최신 커밋 확인

개발일지 작성 시점의 `main` 최신 커밋:

- SHA: `c31e4556687a0fd9a59aa743570b70bc096ed0d9`
- 현재 커밋 메시지: `a`
- 이전 커밋: `93648a452ae8dc2d36553c2a71dd30843a00a8d6`
- 이전 커밋 제목: `132일차 : 유도탄·드론 Route Node Scene 배치 및 장애물 우회 검증`
- 이전 커밋 대비: 1 commit ahead / 0 behind

최신 커밋에는 실제 `Game.unity` 수정과 수정된 Day133 Scene 테스트가 함께 들어 있다.

---

## 14. 최신 GitHub 상태 정적 확인

최신 `Game.unity`에서 다음 항목이 저장된 것을 확인했다.

- `=== ITEM PICKUP TEST MAP ===`
- `TestArena_Floor`
- `Pickup_01_spring_shoes`
- `Pickup_30_hand_mirror`
- 다수의 `Pickup_XX_item_key` 오브젝트

최신 Day133 테스트 파일에서는 이전 컴파일 오류를 발생시킨 `using Fusion;`과 `using ProjectJ.Networking.Fusion;` 직접 참조가 제거된 상태다.

GitHub Combined Status에는 별도 상태 체크가 등록되어 있지 않았고, 해당 SHA에 연결된 GitHub Actions Workflow Run도 확인되지 않았다.

따라서 GitHub 정적 검토에서는 이번 일차를 막는 추가 문제를 확인하지 못했지만, 이 개발일지에서는 Unity 전체 컴파일 성공이나 Day133 테스트 5/5 통과를 별도로 확정하지 않는다.

---

## 15. Unity에서 최종 확인할 항목

1. Console에 C# 컴파일 오류가 없는지 확인
2. `ProjectJDay133PickupIntegrationSceneTests` 실행
3. `=== ITEM PICKUP TEST MAP ===`이 시작 지점 옆에 존재하는지 확인
4. Bridge를 통해 테스트 플랫폼으로 이동 가능한지 확인
5. Pickup 30개가 모두 표시되는지 확인
6. Pickup 01 스프링 신발 획득 확인
7. Pickup 29 저격 물총 획득 확인
8. Pickup 30 손거울 획득 확인
9. 일반 아이템이 빈 Inventory 슬롯에 저장되는지 확인
10. Inventory가 가득 찼을 때 Pickup이 사라지지 않는지 확인
11. Pool Ball Stack 증가 확인
12. Host/Client가 동시에 접근했을 때 한 명만 획득하는지 확인
13. 다른 Peer 화면에서도 획득된 Pickup이 사라지는지 확인
14. 새 경기 시작 시 Pickup 초기 상태를 확인

---

## 133일차 결과

30개의 네트워크 아이템을 한 번에 확인할 수 있는 전용 Pickup 통합 테스트 맵을 `Game.unity`에 추가했다.

각 Pickup을 기존 ItemDefinition과 Network Item Catalog에 연결하고 Scene NetworkObject 구조로 배치하여, 아이템의 월드 획득부터 Inventory 저장까지 통합 검증할 수 있는 기반을 마련했다.

초기 Day133 EditMode 테스트에서 발생한 Fusion Assembly 직접 참조 문제는 네트워크 타입을 직접 참조하지 않는 Scene Component FullName 검증 방식으로 수정했다.

실제 경기 코스의 최종 아이템 배치는 이번 테스트 결과를 기반으로 이후 별도 밸런스 작업에서 진행한다.
