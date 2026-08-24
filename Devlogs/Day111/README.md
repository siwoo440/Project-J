# 111일차 : 풀 공 Stack 인벤토리 및 서버 권한 연속 투사체 구현

## 개발 목표

- 풀 공을 한 슬롯에 최대 5개까지 보유할 수 있는 Stack 아이템으로 구현한다.
- 풀 공 획득 시 기존 Stack을 우선 합산하고 최대 수량을 초과하지 않도록 처리한다.
- 우클릭 사용마다 풀 공 1개를 소비하고 서버 권한 투사체를 생성한다.
- 적중한 상대에게 기존 외력 시스템을 통해 약한 수평 밀치기를 적용한다.
- Stack 수량을 Host·Client 사이에서 동기화하고 HUD에 현재 수량을 표시한다.

## 구현 내용

### 풀 공 아이템 등록

- 네트워크 아이템 카탈로그에 `PoolBall = 10`을 추가했다.
- 문자열 ID `pool_ball`과 표시 이름 `풀 공`을 네트워크 카탈로그에 연결했다.
- 기존 `Item_PoolBall.asset`을 월드 Pickup과 네트워크 인벤토리에서 사용할 수 있도록 연결 경로를 확장했다.

### Stack 인벤토리

- 각 인벤토리 슬롯에 풀 공 Stack 수량을 Networked 값으로 보관한다.
- 풀 공은 한 슬롯에 최대 5개까지 보유할 수 있다.
- 풀 공 Pickup 획득 시 선택 슬롯의 기존 Stack을 먼저 확인하고, 이후 반대 슬롯의 Stack을 확인한다.
- 기존 Stack에 공간이 있으면 1개를 합산한다.
- 빈 슬롯이 있으면 새로운 풀 공 Stack을 수량 1로 생성한다.
- 선택된 풀 공 Stack이 이미 최대 수량이면 해당 Pickup을 소비하지 않는다.
- 일반 아이템은 기존 단일 아이템 저장 규칙을 그대로 사용한다.

### Stack 아이템 사용

- 아이템 사용 입력이 Stack 아이템까지 처리할 수 있도록 공통 사용 진입점을 확장했다.
- 선택된 아이템이 풀 공이 아니면 기존 아이템 사용 로직으로 전달한다.
- 풀 공 사용에 성공할 때마다 선택 슬롯의 수량을 정확히 1 감소시킨다.
- 마지막 1개를 사용해 수량이 0이 되면 해당 슬롯의 아이템 ID를 제거한다.
- 투사체 생성에 실패하면 Stack 수량을 소비하지 않는다.
- 풀 공은 연속 투척을 목적으로 별도의 사용 쿨다운을 두지 않은 프로토타입 규칙을 사용한다.

### 서버 권한 풀 공 투사체

- Host·Server에서 `ProjectJNetworkPoolBallProjectile` NetworkObject를 생성한다.
- 투사체 이동과 충돌 판정, 적중 처리와 Despawn은 State Authority가 담당한다.
- 투사체 속도는 초당 16m다.
- 최대 이동 거리는 28m다.
- 충돌 반경은 0.24m다.
- 적중 외력은 4m/s 수준의 약한 수평 속도 변화로 설정했다.
- 투척한 사용자는 자신의 풀 공 적중 대상에서 제외한다.
- 플레이어나 지형에 처음 충돌하면 투사체를 제거한다.
- 최대 이동 거리에 도달한 경우에도 투사체를 제거한다.
- 빗나가더라도 투사체 Spawn에 성공했다면 사용한 풀 공 1개는 소비된다.

### 기존 외력·보호 시스템 연결

- 풀 공 적중은 `ProjectJNetworkExternalGameplay.TryApplyExternalVelocityChange` 경로를 사용한다.
- 외력 원인은 `ProjectJExternalForceSource.Item`으로 전달한다.
- 따라서 기존 Jelly 보호막, 부활 보호, 완주 및 Gameplay 잠금 판정을 그대로 이용한다.
- 기존 외력 시스템의 수평 처리 규칙을 유지해 풀 공이 임의의 수직 힘을 추가하지 않도록 했다.

### HUD Stack 수량 표시

- HUD가 일반 아이템 이름만 조회하던 방식에서 인벤토리의 슬롯 표시 문자열을 조회하도록 확장했다.
- 일반 아이템은 기존 이름 표시를 유지한다.
- 풀 공은 `풀 공 ×N` 형식으로 현재 Stack 수량을 표시한다.
- 월드 Pickup 저장 처리도 Stack 대응 저장 진입점을 사용하도록 연결했다.

## 테스트 추가

`ProjectJPoolBallPolicyTests`에 총 23개 테스트 사례를 구성했다.

- 최대 Stack 수량 5 확인
- 획득 시 1개 증가 및 최대 수량 Clamp
- 사용 시 정확히 1개 감소
- 0개와 음수 Stack 사용 차단
- 최대 Stack 상태에서 추가 획득 차단
- 투사체 외력 4 확인
- 최대 이동 거리 28m 확인
- 충돌 반경 0.24m 확인
- 투사체 속도 16m/s 확인
- 최대 거리 도달 전·도달·초과 판정

## 변경 파일

111일차 기능 구현 기준:

- 수정: 4개
- 생성: 10개
- 삭제: 없음
- 합계: 14개

주요 변경 경로:

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.cs
├─ ProjectJNetworkItemInventory.PoolBall.cs
├─ ProjectJNetworkPoolBallProjectile.cs
└─ Resources/ProjectJNetworkPoolBallProjectile.prefab

Assets/ProjectJ/Network/Fusion/UI/
└─ ProjectJDay93GameHUD.cs

Assets/ProjectJ/Network/Fusion/World/
└─ ProjectJNetworkItemBox.cs

Assets/ProjectJ/Runtime/Items/
└─ ProjectJPoolBallPolicy.cs

Assets/ProjectJ/Tests/EditMode/
└─ ProjectJPoolBallPolicyTests.cs
```

`.meta` 파일은 신규 Script와 Prefab에 함께 추가됐다.

현재 111일차 Git 커밋에는 이전 일차 기록인 `Devlogs/Day110/README.md` 추가도 함께 포함되어 있으나, 위 변경 파일 수는 111일차 풀 공 기능 구현 범위만 계산했다.

## 검증 결과

- 111일차 확인 커밋: `5102496b7210e376f596d36d1037f6ad31c1683b`
- 네트워크 카탈로그에 `PoolBall = 10`과 `pool_ball` 변환 경로가 추가된 것을 확인했다.
- 풀 공 전용 Networked Stack 수량과 최대 5개 저장 규칙을 확인했다.
- 월드 Pickup 저장 경로가 Stack 대응 저장 함수로 연결된 것을 확인했다.
- 아이템 사용 입력이 Stack 대응 사용 함수로 연결된 것을 확인했다.
- HUD가 풀 공 Stack을 `×N` 형식으로 표시하도록 연결된 것을 확인했다.
- 풀 공 투사체 Prefab에 Fusion Prefab 라벨이 포함된 것을 확인했다.
- 정책 값은 최대 Stack 5, 외력 4, 최대 거리 28m, 충돌 반경 0.24m, 속도 16m/s로 구성되어 있다.
- `ProjectJPoolBallPolicyTests`에는 총 23개 테스트 사례가 포함되어 있다.
- 현재 검토 환경에서는 Unity Editor를 실행할 수 없어 실제 컴파일, EditMode Test Runner와 Host·Client 플레이 결과는 독립적으로 검증하지 못했다.

## 테스트맵 Pickup 배치 보류

- `Day49_AllSystemsTest`에 풀 공 Pickup을 자동 생성하는 과정에서 Fusion Scene `NetworkObject`의 SortKey/Bake 처리 문제가 발생했다.
- 해당 문제는 111일차 핵심 기능 구현 완료 여부와 분리한다.
- 풀 공의 Day49 테스트 Pickup 배치는 현재 일차에서 완료 처리하지 않는다.
- 아이템 구현 페이즈가 끝나갈 때 여러 신규 아이템 Pickup을 한 번에 정리하는 방식으로 다시 진행한다.
- 따라서 111일차 완료 범위는 Stack 인벤토리, HUD 표시, 서버 권한 투사체와 정책 테스트까지다.

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJPoolBallPolicyTests` 23개 통과 확인
3. Fusion Prefab Table에 풀 공 투사체 Prefab 등록 확인
4. 풀 공 획득 시 한 슬롯에 1 → 2 → 3 → 4 → 5로 Stack 증가 확인
5. 최대 5개 상태에서 추가 풀 공 획득이 거부되는지 확인
6. HUD에 `풀 공 ×N`이 Host와 Client 모두 동일하게 표시되는지 확인
7. 빠르게 연속 사용했을 때 Stack이 한 번에 정확히 1개씩 감소하는지 확인
8. 마지막 1개 사용 후 해당 슬롯이 Empty 상태로 정리되는지 확인
9. 빗나간 투사체도 사용한 풀 공 1개를 소비하는지 확인
10. 투사체가 28m 이동 후 제거되는지 확인
11. 상대 적중 시 약한 수평 밀치기가 적용되는지 확인
12. 사용자 본인이 자신의 투사체에 맞지 않는지 확인
13. Jelly 보호막과 부활 보호 상태가 풀 공 외력을 차단하는지 확인
14. Day49 풀 공 Pickup 배치는 아이템 구현 페이즈 후반 통합 작업에서 별도로 진행
