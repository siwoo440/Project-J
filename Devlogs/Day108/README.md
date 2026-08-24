---
# 108일차 : 깃털 신발 서버 권한 효과 및 테스트맵 배치

---
## 개발 목표

- 깃털 신발 사용 시 7초 동안 이동과 달리기 속도 강화
- 효과 중 달리기 스태미나 추가 소모 적용
- 반복 사용, 효과 종료와 부활 초기화 규칙 구성
- Host·Client가 같은 효과 상태를 확인할 수 있도록 Fusion TickTimer 사용
- 수동 테스트맵에 반복 사용 검증용 깃털 신발 Pickup 두 개 배치

---
## 구현 내용

### 깃털 신발 효과

- 네트워크 아이템 ID `FeatherShoes = 7` 등록
- 문자열 ID `feather_shoes`와 표시 이름 `깃털 신발` 연결
- 효과 지속 시간 `7초` 적용
- 일반 이동 속도 `5 → 6.25` 적용
- 달리기 속도 `8 → 10` 적용
- 달리기 스태미나 소모 `초당 25 → 28.75` 적용
- 스태미나 회복 속도와 기존 이동·점프 규칙 유지
- 같은 효과를 다시 사용하면 강도 중첩 없이 남은 시간을 7초로 갱신
- 효과 Timer 종료 후 기본 이동 수치로 자동 복원
- 낙하 부활 시 준비 중인 폭죽과 깃털 신발 효과를 공통 정리 경로에서 제거

### 네트워크 처리

- `ProjectJNetworkItemInventory`의 partial 파일로 깃털 신발 상태 분리
- `[Networked] TickTimer`를 이용한 효과 남은 시간 동기화
- State Authority의 아이템 사용 성공 시 슬롯 소비
- `ProjectJNetworkPlayer.FixedUpdateNetwork()`의 기존 이동 계산 흐름 유지
- 이동 속도와 달리기 스태미나 소모 계산에만 깃털 신발 정책 적용
- 통합 디버그 패널의 아이템 영역에 깃털 신발 남은 시간 표시

### 테스트맵

- `Day49_AllSystemsTest` Scene의 아이템 테스트 레인에 다음 Pickup 추가

```text
Pickup_7_feather_shoes_A
Pickup_7_feather_shoes_B
```

- 두 Pickup 모두 `Item_FeatherShoes.asset` 연결
- 기존 Network Item Box Prefab 구성 재사용
- 각 Pickup에 고유 NetworkObject SortKey 저장
- 두 아이템을 연속 획득해 효과 재사용과 Timer 갱신 검증 가능

---
## 테스트 추가

### `ProjectJFeatherShoesPolicyTests`

- 일반 이동 속도 25% 증가
- 달리기 속도 25% 증가
- 비활성 상태의 기본 이동 속도 유지
- 음수 이동 속도 입력 방어
- 달리기 스태미나 소모 15% 증가
- 비활성 상태의 기본 소모량 유지
- 음수 소모량 입력 방어
- 지속 시간 7초 확인
- 반복 활성화 시 단일 속도 배율 유지

총 9개 테스트 항목을 구성했다.

---
## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkExternalGameplay.cs
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.cs
├─ ProjectJNetworkItemInventory.FeatherShoes.cs
├─ ProjectJNetworkItemInventory.FeatherShoes.cs.meta
└─ ProjectJNetworkPlayer.cs

Assets/ProjectJ/Runtime/Items/
├─ ProjectJFeatherShoesPolicy.cs
└─ ProjectJFeatherShoesPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJFeatherShoesPolicyTests.cs
└─ ProjectJFeatherShoesPolicyTests.cs.meta

Assets/ProjectJ/Tests/Manual/Day49/
└─ Day49_AllSystemsTest.unity
```

- 수정 파일: 5개
- 생성 파일: 6개
- 삭제 파일: 없음

---
## 검증 결과

- 기준 커밋: `17b839ef2c62de47049fc8d9cc4edb93759782dc`
- 작업 커밋: `ebafda618911af9b233a14d07d9d3bc7ab4c3953`
- 최신 커밋 변경 파일: 11개
- Git diff 공백 오류: 없음
- 깃털 신발 네트워크 ID: `7`
- 깃털 신발 Definition 참조: 2개
- 테스트맵 Pickup: 2개
- NetworkObject SortKey 중복: 없음
- 신규 `.meta` GUID 중복: 없음
- `ProjectJFeatherShoesPolicyTests`: 9개 테스트 항목 구성

정적 검토에서 명확한 코드·Scene 참조 문제는 발견되지 않았다.

현재 검토 환경에는 Unity Editor가 없어 실제 컴파일, EditMode Test Runner, Fusion Bake, Play Mode와 Host·Client 접속 결과는 재현하지 못했다.

---
## Unity 확인 항목

1. Unity 실행 후 컴파일 Error 0건 확인
2. `ProjectJFeatherShoesPolicyTests` 9개 실행
3. 전체 EditMode 테스트 실행
4. `Day49_AllSystemsTest` Scene의 깃털 신발 Pickup 두 개 확인
5. Host와 Client에서 깃털 신발 획득·사용 확인
6. 일반 이동 속도 `6.25`, 달리기 속도 `10` 확인
7. 달리기 스태미나 소모 `초당 28.75` 확인
8. 7초 후 기본 수치 복원 확인
9. 두 번째 사용 시 속도 중첩 없이 Timer만 7초로 갱신되는지 확인
10. 효과 중 낙하 부활 시 즉시 효과가 제거되는지 확인
11. 전체 확인 과정의 Console Error 0건 확인
