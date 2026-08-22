# Project J - 73일차 개발 일지

## 개발 기준

72일차 완료 커밋:

```text
72dcbb51b9da62b7d237e31c59d832a962128d1d
72일차 : 동적 장애물·아이템 상자 및 2슬롯 인벤토리 네트워크 동기화 구현
```

73일차 최신 커밋:

```text
24229f24dca588e333fca43b828efa400ab33ac1
73
```

이번 일차에서는 72일차에 구현한 네트워크 아이템 획득·2슬롯 인벤토리 위에 우클릭 사용 입력과 대표 아이템 5종의 State Authority 기반 효과를 연결했다.

---

## 73일차 목표

네트워크 인벤토리에 들어온 아이템을 실제 경기에서 사용할 수 있도록 다음 흐름을 완성한다.

```text
아이템 상자 획득
↓
Network Inventory 저장
↓
Q / E 슬롯 선택
↓
우클릭 사용
↓
State Authority 검증
↓
아이템 효과 적용
↓
효과 성공 시 슬롯 소비
↓
Host / Client 결과 동기화
```

대표 네트워크 아이템 5종:

```text
spring_shoes
jelly_shield
banana_cushion
balloon_horn
water_gun
```

---

## 1. 실제 ItemDefinition ID 기준 Network Item Catalog 추가

기존 72일차에서는 `ITM-001`처럼 숫자가 포함된 ID를 정수로 변환하는 방식을 사용했지만, 실제 ItemDefinition 데이터는 다음 문자열 ID를 사용한다.

```text
spring_shoes
jelly_shield
banana_cushion
balloon_horn
water_gun
```

따라서 `ProjectJNetworkItemCatalog`를 새로 추가하고 대표 5종을 고정 Network ID로 매핑했다.

```text
None           = 0
SpringShoes    = 1
JellyShield    = 2
BananaCushion  = 3
BalloonHorn    = 4
WaterGun       = 5
```

이를 통해 기존 ScriptableObject 참조를 직접 Networked 값으로 보내지 않고 정수 ID만 동기화하면서도 실제 아이템 데이터와 정확하게 연결할 수 있게 했다.

---

## 2. Fusion 입력에 아이템 사용 입력 추가

`ProjectJNetworkInput`에 다음 버튼을 추가했다.

```text
ItemUse
ItemUseHeld
```

입력 규칙:

```text
우클릭 누름
→ ItemUse = true

우클릭 유지
→ ItemUseHeld = true

우클릭 해제
→ ItemUseHeld = false
```

`ProjectJFusionInputProvider`에서는 우클릭 시작을 다음 Fusion Tick까지 단발 입력으로 보존하고, 우클릭 유지 여부는 현재 입력 상태로 계속 전달한다.

이 구조를 통해 즉시 사용 아이템과 Hold 방식 아이템을 같은 Fusion Input 흐름에서 처리할 수 있다.

---

## 3. Network Inventory 아이템 사용·소비 구조 구현

`ProjectJNetworkItemInventory`에 실제 사용 판정을 추가했다.

기본 처리 순서:

```text
State Authority 확인
↓
경기 입력 가능 상태 확인
↓
현재 선택 슬롯 확인
↓
아이템 ID 확인
↓
아이템별 효과 실행
↓
효과 성공 여부 확인
↓
성공 시 선택 슬롯 Empty
↓
Inventory Revision 증가
```

효과 적용에 실패한 경우 아이템을 소비하지 않는다.

예:

```text
바나나 설치 위치 유효
→ 설치 성공
→ 아이템 소비

바나나 설치 위치 없음
→ 사용 실패
→ 아이템 유지
```

---

## 4. 스프링 신발 네트워크 효과

스프링 신발은 사용 후 8초 동안 추가 점프 기능을 활성화한다.

네트워크 상태:

```text
NetworkSpringShoesTimer
NetworkSpringExtraJumpAvailable
NetworkSpringAirborneSeconds
```

처리 흐름:

```text
스프링 신발 사용
↓
8초 TickTimer 시작
↓
공중 상태 확인
↓
Space 재입력
↓
State Authority가 추가 점프 판정
↓
수직 속도 8 적용
↓
추가 점프 1회 소비
```

착지하면 추가 점프 사용 가능 상태가 다시 충전된다.

첫 점프 직후 같은 입력이 추가 점프로 중복 처리되지 않도록 짧은 공중 체류 보호 시간을 적용했다.

---

## 5. ProjectJNetworkPlayer 아이템 수직 속도 API 추가

스프링 신발 효과가 Player의 실제 네트워크 이동 상태를 변경할 수 있도록 다음 State Authority 전용 기능을 추가했다.

```text
TrySetItemVerticalVelocityAuthority()
```

검증 조건:

```text
NetworkObject 유효
State Authority 보유
경기 조작 가능
양수 수직 속도
```

조건을 통과한 경우 네트워크 Player의 수직 속도를 변경하고 Grounded 상태를 해제한다.

---

## 6. 젤리 보호막 네트워크 효과

젤리 보호막 사용 시 4초 동안 Network TickTimer를 활성화한다.

```text
NetworkJellyShieldTimer
```

보호 대상:

```text
Push
Item External Force
```

따라서 보호막이 활성화된 Player는 다음 적대 효과를 차단할 수 있다.

```text
기본 밀치기
풍선 나팔
물총
바나나 쿠션
```

AirBag과 같은 환경 외력은 Item/Push가 아니므로 보호막으로 막지 않는다.

---

## 7. External Force와 젤리 보호막 연결

`ProjectJNetworkExternalGameplay`에서 Player의 `ProjectJNetworkItemInventory`를 참조하도록 확장했다.

외력 판정은 다음과 같이 처리된다.

```text
외력 요청
↓
경기 상태 확인
↓
젤리 보호막 확인
↓
부활 보호 확인
↓
외력 적용
```

기본 Push 대상이 젤리 보호막 상태인 경우 Push 결과에 다음 상태를 기록한다.

```text
Shielded
```

기존 `Protected`는 부활 보호에 사용하고, `Shielded`는 아이템 보호막 차단을 구분하는 용도로 사용한다.

---

## 8. 풍선 나팔 네트워크 효과

풍선 나팔은 State Authority가 실제 Target을 결정한다.

기준값:

```text
범위: 6m
전방 반각: 55°
전체 판정각: 110°
외력: 30
```

처리:

```text
풍선 나팔 사용
↓
State Authority가 Network Player 검색
↓
6m 범위 확인
↓
전방 ±55° 확인
↓
대상에게 Item External Force 적용
```

대상에게 젤리 보호막 또는 부활 보호가 활성화되어 있다면 기존 외력 검증 단계에서 차단된다.

Target이 없는 경우에도 풍선 나팔 자체의 사용은 성공으로 처리한다.

---

## 9. 바나나 쿠션 네트워크 효과

바나나 쿠션은 사용자의 전방 바닥을 State Authority가 검사한 뒤 설치한다.

설치 검사:

```text
플레이어 전방 1.5m
↓
아래 방향 Raycast
↓
바닥 존재 확인
↓
경사 제한 확인
↓
ItemPlacementValidator 검사
↓
설치
```

네트워크 상태:

```text
NetworkBananaActive
NetworkBananaPosition
NetworkBananaNormal
NetworkBananaLifetimeTimer
NetworkBananaRevision
```

바나나는 별도 Network Prefab을 생성하지 않고 소유 Player의 Networked 상태를 사용하여 Host와 Client가 같은 위치에 로컬 외형을 표시한다.

기본 수명:

```text
15초
```

접촉 시:

```text
State Authority가 주변 Player 검색
↓
소유자 제외
↓
미끄러짐 방향 계산
↓
Item External Force 6.5 적용
↓
성공 시 바나나 제거
```

부활 보호 또는 젤리 보호막 때문에 외력이 차단되면 바나나는 소비되지 않고 유지된다.

현재 73일차 구현에서는 Player 한 명당 동시에 활성화할 수 있는 바나나 쿠션을 1개로 제한한다.

---

## 10. 물총 네트워크 Hold 효과

물총은 대표 5종 중 지속 입력이 필요한 아이템이다.

기준값:

```text
사거리: 12m
SphereCast 반경: 0.3
판정 주기: 0.1초
Tick당 외력: 0.55
```

처리 흐름:

```text
우클릭 시작
↓
물총 활성화
↓
첫 Tick 즉시 판정
↓
우클릭 유지
↓
0.1초마다 State Authority SphereCast
↓
첫 번째 대상에게 Item External Force 적용
↓
우클릭 해제
↓
물총 종료
```

`Time.time` 대신 Fusion `TickTimer`를 사용하여 반복 판정 주기를 네트워크 Simulation 기준으로 관리한다.

---

## 11. 경기 상태와 아이템 사용 연결

아이템 사용은 기존 `GameplayInputAllowed` 상태를 그대로 따른다.

따라서 다음 시점에는 아이템 사용이 차단된다.

```text
경기 준비 중
카운트다운 진행 중
개인 FINISH 후
경기 종료 후
```

경기가 잠기면 물총 Hold 상태도 자동 종료된다.

경기 전체가 Finished가 되면 남아 있는 바나나 쿠션도 제거한다.

---

## 12. 아이템 상자 Network ID 표시 수정

`ProjectJNetworkItemBox`는 아이템 획득 후 실제 지급된 Network Item ID를 저장한다.

73일차부터 Debug Log에서도 숫자 ID만 출력하지 않고 `ProjectJNetworkItemCatalog`를 통해 실제 아이템 Key를 표시한다.

예:

```text
P0 / spring_shoes / Slot 1
P1 / balloon_horn / Slot 2
```

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Input/
├─ ProjectJFusionInputProvider.cs
└─ ProjectJNetworkInput.cs

Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJExternalForceSource.cs
├─ ProjectJNetworkExternalGameplay.cs
├─ ProjectJNetworkItemInventory.cs
└─ ProjectJNetworkPlayer.cs

Assets/ProjectJ/Network/Fusion/World/
└─ ProjectJNetworkItemBox.cs
```

---

## 생성 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkItemCatalog.cs
└─ ProjectJNetworkItemCatalog.cs.meta
```

---

## 삭제 파일

```text
없음
```

---

## 73일차 테스트 항목

```text
Unity Console
→ Error 0

Host + Client 접속
↓
경기 시작
↓
Item Box 획득
↓
Q / E 슬롯 선택
↓
우클릭 아이템 사용
```

### 공통

```text
실제 5종 아이템이 Network Inventory에 정상 저장되는지 확인
효과 성공 후 선택 슬롯이 Empty가 되는지 확인
효과 실패 시 아이템이 유지되는지 확인
Host와 Client에서 슬롯 상태가 동일한지 확인
FINISH 후 아이템 사용이 차단되는지 확인
```

### 스프링 신발

```text
사용 후 8초 활성
공중 Space 재입력
추가 점프 1회 발생
착지 후 추가 점프 재충전
지속 시간 종료 후 기능 해제
```

### 젤리 보호막

```text
사용 후 4초 활성
기본 Push 차단
풍선 나팔 차단
물총 차단
바나나 쿠션 외력 차단
AirBag 환경 외력은 정상 적용
```

### 풍선 나팔

```text
전방 6m 대상 판정
범위 밖 Player 제외
뒤쪽 Player 제외
범위 내 대상에게 큰 Item External Force 적용
```

### 바나나 쿠션

```text
유효한 바닥에 설치
Host / Client 동일 위치 표시
15초 후 자동 제거
상대 접촉 시 외력 적용 후 제거
보호 상태 Player 접촉 시 유지
동시에 두 번째 바나나 설치 차단
```

### 물총

```text
우클릭 시작 시 첫 판정
우클릭 유지 중 0.1초 주기 판정
첫 Target에 외력 적용
일반 지형에 막힘
우클릭 해제 시 즉시 종료
```

### 기존 기능 회귀 테스트

```text
이동
점프
Sprint
Stamina
Crouch
Push
Checkpoint
Respawn
3초 부활 보호
Height / Rank
Countdown
Match Timer
FINISH
동적 플랫폼
Item Box
2슬롯 Inventory
정상 유지
```

---

## 코드 검토 결과

최신 GitHub 커밋 기준으로 다음 연결을 확인했다.

```text
ItemUse / ItemUseHeld Fusion Input
→ 구현됨

실제 문자열 Item ID 5종 Catalog
→ 구현됨

Network Inventory 사용·소비
→ 구현됨

스프링 신발 TickTimer·추가 점프
→ 구현됨

젤리 보호막 Push·Item 차단
→ 구현됨

풍선 나팔 State Authority Target 판정
→ 구현됨

바나나 설치·수명·접촉 동기화
→ 구현됨

물총 Hold·반복 판정
→ 구현됨

기존 Item Box Network ID 연결
→ 수정됨
```

GitHub 저장소에 자동 Unity 빌드/테스트 CI가 등록되어 있지 않으므로 실제 Unity 컴파일과 Host/Client 런타임 성공 여부는 원격 저장소만으로 확정할 수 없다.

최종 완료 기준:

```text
Unity Console Error 0
+
Host / Client 2인 테스트 통과
```

---

## 73일차 완료 구조

```text
Item Box
↓
ProjectJNetworkItemCatalog
↓
Network Inventory
├─ Slot 1
├─ Slot 2
└─ Selected Slot
        ↓
Fusion Input
├─ Q
├─ E
├─ Right Click
└─ Right Click Hold
        ↓
State Authority
├─ Spring Shoes
├─ Jelly Shield
├─ Banana Cushion
├─ Balloon Horn
└─ Water Gun
        ↓
External Force / Player Movement / Networked State
        ↓
Host / Client 동일 결과
```

---

## 다음 개발 방향

74일차에서는 Phase 6의 개별 시스템 구현을 실제 경기 입장 흐름으로 묶는다.

핵심 방향:

```text
Lobby
↓
Player Ready
↓
전원 준비 확인
↓
MatchLoading
↓
게임 Scene 이동
↓
2인 Spawn
↓
Countdown
↓
10분 경기
↓
FINISH / 시간 종료
↓
Results
```

72~73일차에서 구현한 아이템 상자·인벤토리·아이템 효과까지 포함하여 Host + Client 두 명이 하나의 완전한 경기 흐름을 처음부터 끝까지 진행할 수 있는 상태를 목표로 한다.
