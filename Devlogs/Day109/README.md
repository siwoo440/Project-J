# 109일차 : 눈덩이 서버 권한 투사체 및 3초 이동 감속

## 개발 목표

- 눈덩이를 서버 권한 투사체 아이템으로 구현한다.
- 적중한 상대의 이동·달리기 속도를 3초 동안 감소시킨다.
- 빗나감, 자기 피격, 보호 상태, 재적중, 부활과 기존 아이템 효과의 상호작용을 검증한다.
- 고정 테스트맵에 눈덩이 Pickup을 배치해 Host·Client 환경에서 반복 시험할 수 있게 한다.

## 구현 내용

### 눈덩이 아이템 등록

- 네트워크 아이템 카탈로그에 `Snowball = 8`을 추가했다.
- 기존 `Item_Snowball.asset`을 사용하며 Pickup과 인벤토리 아이템 ID를 연결했다.
- 아이템 사용 성공 시에만 보유 아이템을 소비하도록 구성했다.

### 서버 권한 투사체

- 서버에서 `ProjectJNetworkSnowballProjectile` NetworkObject를 생성한다.
- 투사체 속도는 초당 16m, 최대 이동 거리는 15m, 충돌 반경은 0.3m다.
- 서버가 이동, 충돌 대상 선택, 적중 처리와 Despawn을 담당한다.
- 사용자 본인은 적중 대상에서 제외한다.
- 최대 거리에 도달하거나 유효한 대상에 적중하면 투사체를 제거한다.
- 투사체 Prefab을 `Resources`와 Fusion Prefab 라벨에 등록했다.

### 이동 감속과 보호 판정

- 적중 대상의 걷기·달리기 속도를 기존 값의 75%로 3초 동안 낮춘다.
- 재적중 시 감속 강도는 중첩하지 않고 남은 시간을 3초로 갱신한다.
- 완주자, 부활 보호 대상, Jelly 보호막 대상에게는 감속을 적용하지 않는다.
- 부활 시 남아 있는 눈덩이 감속을 즉시 제거한다.
- 깃털 신발과 눈덩이 효과는 독립적으로 계산하며 동시에 활성화될 수 있다.

### 고정 테스트맵 배치

- `Day49_AllSystemsTest` Scene에 다음 Pickup 두 개를 추가했다.
  - `Pickup_8_snowball_A`
  - `Pickup_8_snowball_B`
- 두 Pickup 모두 `Item_Snowball.asset`을 참조한다.
- 추가 Pickup 공간을 확보하기 위해 Day51 Pickup Lane 바닥을 확장했다.

## 테스트 추가

`ProjectJSnowballPolicyTests`에 18개 TestCase를 추가했다.

- 감속 활성·비활성 시 걷기 및 달리기 속도
- 깃털 신발과 눈덩이 동시 적용 결과
- 정상 적중과 Runner·Gameplay 조건 누락
- 자기 피격 차단
- 완주·부활 보호·Jelly 보호막 차단
- 재적중 시 3초 갱신과 비중첩
- 최대 이동 거리 도달 전·후 판정
- 음수 입력값 안전 처리

## 변경 파일

- 수정: 5개
- 생성: 10개
- 삭제: 없음
- 합계: 15개

주요 변경 경로:

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkExternalGameplay.cs
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.cs
├─ ProjectJNetworkItemInventory.Snowball.cs
├─ ProjectJNetworkPlayer.cs
├─ ProjectJNetworkSnowballProjectile.cs
└─ Resources/ProjectJNetworkSnowballProjectile.prefab

Assets/ProjectJ/Runtime/Items/
└─ ProjectJSnowballPolicy.cs

Assets/ProjectJ/Tests/EditMode/
└─ ProjectJSnowballPolicyTests.cs

Assets/ProjectJ/Tests/Manual/Day49/
└─ Day49_AllSystemsTest.unity
```

## 검증 결과

- 기준 커밋: `d649ec43f905ae869db07052bfbe059fe1ddf2ee`
- 109일차 확인 커밋: `48f24eb401d131eca70628eafef8f8845dec396b`
- 최신 커밋의 15개 변경 파일이 109일차 작업 범위와 일치함을 확인했다.
- 눈덩이 ID, 정책 참조, 투사체 Prefab의 Fusion 라벨, Scene Pickup 두 개와 Item Definition 참조를 확인했다.
- 테스트 파일에 18개 TestCase가 포함된 것을 확인했다.
- Prefab의 빈 `m_Name:` 세 곳에 후행 공백이 있으나 Unity 직렬화 동작을 막는 기능 문제는 아니다.
- 현재 검토 환경에는 Unity 실행 파일이 없어 컴파일, EditMode Test Runner, Fusion Prefab Table 재베이크와 실제 Host·Client 플레이는 독립적으로 실행하지 못했다.

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건
2. `ProjectJSnowballPolicyTests` 18개 통과
3. Fusion Prefab Table에 눈덩이 투사체 등록 확인
4. Host가 던진 눈덩이가 Client에게 동기화되는지 확인
5. 적중 시 걷기·달리기 감속과 3초 후 정상 복구 확인
6. 빗나감, 자기 피격, 완주·부활 보호·Jelly 보호막 차단 확인
7. 재적중 시 감속 비중첩과 지속 시간 갱신 확인
8. 부활 시 감속 제거 확인
9. 깃털 신발과 동시 활성 상태의 이동 속도 확인
10. 테스트맵의 눈덩이 Pickup 두 개 획득·사용 확인
