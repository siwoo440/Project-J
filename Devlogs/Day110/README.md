# 110일차 : 지뢰 서버 권한 설치·감지 및 폭발 외력

## 개발 목표

- 지뢰를 서버 권한 설치형 아이템으로 구현한다.
- 소유자가 아닌 상대의 접근을 감지해 위쪽과 바깥쪽 폭발 외력을 적용한다.
- 설치 금지 위치, 소유자 판정, 다중 Target, 보호 상태와 동시 폭발을 검증한다.
- 고정 테스트맵에 지뢰 Pickup을 배치해 Host·Client 환경에서 반복 시험할 수 있게 한다.

## 구현 내용

### 지뢰 아이템 등록

- 네트워크 아이템 카탈로그에 `Mine = 9`를 추가했다.
- 기존 `Item_Mine.asset`과 Pickup·인벤토리 네트워크 ID를 연결했다.
- 지뢰 설치와 서버 초기화가 성공한 경우에만 아이템을 소비한다.

### 서버 권한 설치와 수명

- Host·Server만 `ProjectJNetworkMine` NetworkObject를 생성한다.
- 사용자 전방의 지면을 Raycast로 찾고 지면 법선에 맞춰 배치한다.
- 설치 후 0.75초가 지나면 접근 감지를 시작한다.
- 발동하지 않은 지뢰는 25초 후 자동으로 Despawn한다.
- 지뢰끼리 1.5m 이상 떨어진 위치에만 설치할 수 있다.

### 설치 금지 위치

- 지면을 찾지 못한 위치와 급경사를 차단한다.
- Rigidbody가 있는 동적 물체, 플레이어와 기존 지뢰 위 설치를 차단한다.
- NoSpawn, Checkpoint, Respawn, Fusion Start와 FINISH 주변 설치를 차단한다.
- 설치가 거부되면 인벤토리 아이템을 유지한다.

### 접근 감지와 폭발

- 활성화된 지뢰는 2.25m 안의 유효한 상대 접근을 서버에서 감지한다.
- 소유자, 완주자, 부활 보호 대상과 Jelly 보호막 대상은 Trigger에서 제외한다.
- 폭발 반경 3.5m 안의 모든 유효 Target에게 외력을 적용한다.
- 바깥쪽 8m/s와 위쪽 6m/s 속도 변화를 합산한다.
- 기존 수평 밀치기·아이템 외력은 수직 성분 제거 규칙을 그대로 유지한다.
- 같은 Tick에 여러 지뢰가 폭발하면 기존 외력 누적 구조를 통해 힘을 합산한다.
- 한 지뢰는 한 번만 폭발하며 지뢰끼리 연쇄 폭발하지 않는다.

### 네트워크와 성능 처리

- 설치 사용자, 활성화 Timer, 유지 Timer와 폭발 상태를 Networked 값으로 동기화한다.
- Fusion Prefab 라벨과 NetworkTransform을 포함한 지뢰 Prefab을 추가했다.
- 지뢰마다 Scene 전체 Player 배열을 생성하지 않고 Runner별 활성 Player Registry를 재사용한다.

### 고정 테스트맵 배치

- `Day49_AllSystemsTest` Scene에 다음 Pickup 두 개를 추가했다.
  - `Pickup_9_mine_A`
  - `Pickup_9_mine_B`
- 두 Pickup 모두 `Item_Mine.asset`을 참조한다.
- 추가 Pickup 공간을 확보하기 위해 Day51 Pickup Lane 바닥을 확장했다.

## 테스트 추가

`ProjectJMinePolicyTests`에 27개 테스트 사례를 추가했다.

- 정상 지면과 급경사·지면 누락 판정
- 공통 금지 구역과 기존 지뢰 최소 간격 판정
- 소유자·완주자·부활 보호·Jelly 보호막 제외
- 활성화 전·후 Trigger 판정
- Fusion Start Respawn 수평·수직 보호 범위 경계
- 일반 위치와 같은 위치의 폭발 방향
- 바깥쪽과 위쪽 폭발 속도
- 지뢰 수명·활성화·감지·폭발 기획 수치

## 변경 파일

- 수정: 4개
- 생성: 10개
- 삭제: 없음
- 합계: 14개

주요 변경 경로:

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkExternalGameplay.cs
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.cs
├─ ProjectJNetworkItemInventory.Mine.cs
├─ ProjectJNetworkMine.cs
└─ Resources/ProjectJNetworkMine.prefab

Assets/ProjectJ/Runtime/Items/
└─ ProjectJMinePolicy.cs

Assets/ProjectJ/Tests/EditMode/
└─ ProjectJMinePolicyTests.cs

Assets/ProjectJ/Tests/Manual/Day49/
└─ Day49_AllSystemsTest.unity
```

## 검증 결과

- 기준 커밋: `5e618b22f2d7f24cb735b44a3d6f55242d11ece7`
- 110일차 확인 커밋: `cffedf047701c5e35f649e28d0445e78ea448b6a`
- 최신 커밋의 14개 변경 파일이 110일차 작업 범위와 일치함을 확인했다.
- 지뢰 ID, 정책 참조, 서버 권한 설치, 보호 판정과 3차원 외력 경로를 확인했다.
- 지뢰 Prefab의 Fusion 라벨, NetworkedBehaviour 참조와 Scene Pickup 두 개를 확인했다.
- Scene FileID 12개와 NetworkObject SortKey에 중복이 없음을 확인했다.
- 테스트 파일에 27개 테스트 사례가 포함된 것을 확인했다.
- Prefab의 빈 `m_Name:` 세 곳에 후행 공백이 있으나 Unity 직렬화 동작을 막는 기능 문제는 아니다.
- 현재 검토 환경에는 Unity 실행 파일이 없어 컴파일, EditMode Test Runner, Fusion Prefab Table 재베이크와 실제 Host·Client 플레이는 독립적으로 실행하지 못했다.

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건
2. `ProjectJMinePolicyTests` 27개 통과
3. Fusion Prefab Table에 지뢰 Prefab 등록 확인
4. Host가 설치한 지뢰 위치와 제거 상태가 Client에 동기화되는지 확인
5. 설치 후 0.75초 활성화 대기와 25초 자동 제거 확인
6. 소유자 접근 시 미폭발 확인
7. 상대 접근 시 위쪽·바깥쪽 외력 확인
8. 다중 Target과 여러 지뢰 동시 폭발 외력 확인
9. 완주·부활 보호·Jelly 보호막 대상 제외 확인
10. Checkpoint·Respawn·Start·FINISH와 기존 지뢰 근처 설치 차단 확인
11. 설치 실패 시 아이템 미소비 확인
12. 테스트맵의 지뢰 Pickup 두 개 획득·사용 확인
