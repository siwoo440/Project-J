---
# 107일차 : 폭죽 서버 권한 구현 및 29종 아이템 Definition 구성

---
## 개발 목표

- 준비 시간이 끝난 뒤 전방 넓은 범위의 여러 플레이어를 밀어내는 폭죽 구현
- 준비 취소, 거리, 각도, 벽 차폐와 보호 상태를 서버 권한으로 판정
- 기획서 기준 초기 출시 아이템 29종의 `ItemDefinition` 데이터 선구성
- 수동 테스트 Scene에 폭죽 아이템 상자 배치

---
## 구현 내용

### 폭죽

- 네트워크 아이템 ID `firework` 추가
- 준비 시간 `0.9초` 적용
- 전방 사거리 `8m`, 전체 각도 `100도` 적용
- 범위 안의 여러 유효 Target에 수평 외부 속도 최대 `9m/s` 적용
- Raycast를 이용한 벽 차폐 판정
- 젤리 보호막과 부활 보호 상태의 외부 힘 차단 규칙 재사용
- 준비 중 부활하거나 경기 입력이 종료되면 발동 취소
- 준비, 발동, 취소와 마지막 Target 수를 네트워크 상태와 디버그 화면에 기록

### 아이템 데이터

- 최신 기획서 기준 초기 출시 아이템 29종 구성
- 각 Definition에 ID, 표시 이름, 분류, 사용 방식, Target 방식과 지속 시간 입력
- 기존 아이콘 연결 및 폭죽 아이콘 추가
- 손거울은 전용 이미지 제작 전까지 젤리 보호막 아이콘을 임시 사용
- 기획서에 없는 가시 갑옷 제외
- 복어 풍선옷, 저격 물총과 손거울 포함
- 바나나 쿠션 지속 시간 `20초`, 물총 지속 시간 `2.5초` 반영
- 미구현 아이템의 네트워크 ID와 실제 효과 코드는 각 개발 일차까지 보류

### 테스트 Scene

- `Day49_AllSystemsTest` Scene에 `Pickup_6_firework` 추가
- 폭죽 Definition과 Network Item Box 연결
- Fusion NetworkObject와 고유 SortKey 저장

---
## 테스트 추가

### `ProjectJFireworkPolicyTests`

- 준비 시작 조건
- 경기 종료와 부활에 따른 준비 취소
- 전방 각도와 8m 사거리 경계
- 후방 Target 제외
- 수평 외력과 최종 속도 9m/s 제한

### `ProjectJItemDefinitionCatalogTests`

- 기획서 기준 29종 자산 존재 확인
- ID, 표시 이름, 분류, 사용 방식, Target 방식과 지속 시간 확인
- Item ID 중복과 공통 유효성 확인
- 아이콘 누락 확인
- 손거울 임시 아이콘 확인

---
## 검증 결과

- 기준 커밋: `2b5a2f17a45c025a227af5df54322cdc1abb2b52`
- 작업 커밋: `61847350082e71ebcaebe20336c8a49e288b9855`
- 변경 파일: 62개
- ItemDefinition: 29종
- Item ID: 29개, 중복 없음
- ItemDefinition GUID: 29개, 중복 없음
- 아이콘 누락: 0개
- 폭죽 Scene 참조: 1개
- 폭죽 NetworkObject SortKey 중복: 없음
- Scene YAML fileID 중복: 없음
- `ProjectJFireworkPolicyTests`: 15개 테스트 항목 구성
- `ProjectJItemDefinitionCatalogTests`: 아이템별 29개 항목과 전체 목록·손거울 아이콘 검증 구성

Unity 실행 환경에서 컴파일, EditMode 전체 테스트와 Host·Client 실사용 검증이 필요하다. 현재 검토 환경에서는 해당 실행 결과를 재현하지 못했다.

`Day49_AllSystemsTest.unity`에는 Unity가 직렬화한 빈 문자열 줄의 후행 공백이 3곳 남아 있다. 기능에는 영향을 주지 않지만 `git diff --check`에서 경고가 발생한다.

---
## 수동 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJFireworkPolicyTests` 15개 실행
3. `ProjectJItemDefinitionCatalogTests` 31개 실행
4. 전체 EditMode 테스트 실행
5. Host와 Client가 폭죽을 획득하고 0.9초 뒤 발동하는지 확인
6. 8m 밖, 후방과 벽 뒤 Target이 제외되는지 확인
7. 여러 Target이 동시에 밀려나는지 확인
8. 젤리 보호막과 부활 보호 상태에서 외력이 차단되는지 확인
9. 준비 중 부활과 경기 종료 시 폭죽이 취소되는지 확인
10. Console Error 0건 확인

---
## 주요 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.cs
├─ ProjectJNetworkItemInventory.Firework.cs
└─ ProjectJNetworkExternalGameplay.cs

Assets/ProjectJ/Runtime/Items/
└─ ProjectJFireworkPolicy.cs

Assets/ProjectJ/Data/Items/
└─ ItemDefinition 29종

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJFireworkPolicyTests.cs
└─ ProjectJItemDefinitionCatalogTests.cs

Assets/ProjectJ/Tests/Manual/Day49/
└─ Day49_AllSystemsTest.unity
```
