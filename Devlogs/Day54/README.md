# Project J - 54일차 개발 일지

## 개발 목표

대표 아이템 5종의 실제 게임 효과를 구현하고, 기존 아이템 공통 사용 구조와 플레이어 이동·외력 시스템에 연결한다.

이번 일차의 대상 아이템은 다음과 같다.

```text
스프링 신발
젤리 보호막
바나나 쿠션
풍선 나팔
물총
```

기존의 다음 흐름은 유지한다.

```text
Pickup
↓
2슬롯 Inventory
↓
Q / E 슬롯 선택
↓
UseItem 입력
↓
ItemUseEffectRegistry
↓
Effect 실행
↓
성공 시 아이템 소비
```

---

## 주요 개발 내용

### 1. 대표 아이템 5종 실제 Effect 구현

기존 52일차 임시 소비용 Effect 대신 실제 아이템 기능을 등록하는 구조를 추가했다.

```text
spring_shoes
→ SpringShoesEffect

jelly_shield
→ JellyShieldEffect

banana_cushion
→ BananaCushionEffect

balloon_horn
→ BalloonHornEffect

water_gun
→ WaterGunEffect
```

실제 Effect는 기존 `ItemUseEffectRegistry`에 등록되며, `PlayerItemUseController`의 성공 후 1회 소비 규칙을 그대로 사용한다.

---

### 2. 스프링 신발 구현

스프링 신발 사용 시 8초 동안 추가 공중 점프 기능을 활성화한다.

동작 흐름:

```text
스프링 신발 사용
↓
8초 Buff 시작
↓
기본 점프
↓
공중에서 Jump 재입력
↓
추가 공중 점프 1회
```

착지하면 추가 점프를 다시 1회 사용할 수 있다.

기존 플레이어 이동 시스템의 다음 기능은 유지한다.

```text
기본 점프
Coyote Time
Jump Buffer
Ledge Climb
중력 처리
```

추가 점프 기능은 별도 상태 컴포넌트에서 처리해 기존 이동 코드를 크게 변경하지 않도록 구성했다.

---

### 3. 젤리 보호막 구현

젤리 보호막 사용 시 4초 동안 플레이어와 공격 아이템에 의한 외력을 차단한다.

외력 구분을 위해 기존 `ExternalForceSource`에 Item 타입을 추가했다.

```text
Push
→ 플레이어 기본 밀치기

AirBag
→ 장애물 외력

Item
→ 아이템에 의한 외력
```

보호막 활성 중 처리:

```text
Push
→ 차단

Item
→ 차단

AirBag
→ 정상 적용
```

따라서 보호막 때문에 장애물 자체의 게임 규칙이 무효화되지 않도록 했다.

---

### 4. 바나나 쿠션 구현

바나나 쿠션은 전방 바닥에 설치하는 Place 타입 아이템으로 구현했다.

사용 시:

```text
플레이어 전방 위치 계산
↓
아래 방향 Raycast
↓
설치 가능한 바닥 확인
↓
바나나 쿠션 생성
```

설치할 수 없는 위치에서는 Effect가 실패하며 아이템도 소비되지 않는다.

```text
바닥 없음
경사 과다
설치 위치 부적합
↓
사용 실패
↓
Inventory 유지
```

설치된 바나나 쿠션은 다른 플레이어가 접촉하면 Item Force를 적용한 뒤 제거된다.

테스트용 Runtime 오브젝트는 일정 시간이 지나면 자동 제거되도록 구성했다.

---

### 5. 풍선 나팔 구현

풍선 나팔은 플레이어 전방의 여러 대상을 동시에 밀어내는 공격 아이템으로 구현했다.

대상 판정:

```text
전방 범위 검색
↓
자기 자신 제외
↓
범위 밖 제외
↓
후방 대상 제외
↓
유효한 여러 플레이어에게 Item Force 적용
```

풍선 나팔의 밀치기 힘은 고정값이 아니라 현재 캐릭터 기본 밀치기 힘을 기준으로 계산하도록 수정했다.

```text
풍선 나팔 밀치기 힘
= 현재 캐릭터 기본 밀치기 힘 × 2.5
```

현재 기본 밀치기 값이 12인 경우:

```text
12 × 2.5
= 30
```

따라서 이후 캐릭터 기본 밀치기 값이 바뀌어도 풍선 나팔은 자동으로 2.5배 비율을 유지한다.

젤리 보호막이 활성화된 대상에게는 풍선 나팔 Item Force가 적용되지 않는다.

---

### 6. 물총 Hold 입력 구현

기존 `UseItem` 입력은 한 번의 `performed` 이벤트만 사용했지만, 물총은 버튼을 누르고 있는 동안 지속 동작해야 하므로 입력 해제 처리를 추가했다.

```text
우클릭 입력
→ 물총 사용 시작

우클릭 유지
→ 일정 간격으로 물총 Force 적용

우클릭 해제
→ 물총 사용 종료
```

물총은 전방 대상을 탐색한 뒤 짧은 간격으로 약한 Item Force를 반복 적용한다.

풍선 나팔과의 차이는 다음과 같다.

```text
풍선 나팔
→ 강한 Force 1회
→ 전방 여러 대상

물총
→ 약한 Force 반복
→ 조준 방향의 대상
```

젤리 보호막 활성 대상에게는 물총 Force도 적용되지 않는다.

---

### 7. Hold 아이템 입력 해제 구조 추가

물총처럼 입력을 유지해야 하는 아이템을 위해 사용 버튼 해제를 Runtime Effect에 전달하는 구조를 추가했다.

```text
UseItem canceled
↓
PlayerItemUseController
↓
IItemUseReleaseHandler
↓
활성 Hold Effect 종료
```

이를 통해 이후 지속형 아이템도 같은 구조를 재사용할 수 있다.

---

### 8. Day49 테스트 더미 중력 문제 수정

Day49 통합 테스트의 더미는 실제 `Player.prefab`을 사용하지만 `PlayerCameraRelativeMovement`가 비활성화되어 있었다.

Player Rigidbody는 Unity 기본 Gravity를 사용하지 않고, 실제 플레이어 이동 스크립트가 자체적으로 중력을 계산하는 구조이므로 이동 스크립트가 꺼진 더미는 공중에서 낙하하지 않는 문제가 있었다.

문제 상태:

```text
PlayerCameraRelativeMovement
Off

Rigidbody Use Gravity
Off

결과
→ 외력으로 밀린 뒤 공중에서 떠 있음
```

이를 해결하기 위해 Day49 테스트 더미 전용 중력 보정 컴포넌트를 추가했다.

```text
비활성 PlayerCameraRelativeMovement 감지
↓
테스트 더미로 판단
↓
Gravity -22 적용
```

실제 플레이어 이동 스크립트가 활성화되어 있는 경우에는 보조 중력을 적용하지 않아 중력이 중복되지 않도록 했다.

수정 후:

```text
풍선 나팔로 밀림
↓
절벽 밖 이동
↓
중력 적용
↓
정상 낙하
```

AirBag 등으로 공중에 뜬 테스트 더미도 다시 바닥으로 떨어진다.

---

## 주요 생성 파일

```text
Assets/ProjectJ/Runtime/Items/Effects/
├─ SpringShoesEffect.cs
├─ SpringShoesBuffState.cs
├─ JellyShieldEffect.cs
├─ JellyShieldState.cs
├─ BananaCushionEffect.cs
├─ BananaCushionRuntime.cs
├─ BalloonHornEffect.cs
├─ WaterGunEffect.cs
├─ WaterGunRuntime.cs
└─ ProjectJItemEffectInstaller.cs

Assets/ProjectJ/Runtime/Items/
└─ IItemUseReleaseHandler.cs

Assets/ProjectJ/Tests/Manual/Day49/Scripts/
└─ Day54DummyGravityFallback.cs
```

---

## 주요 수정 파일

```text
Assets/ProjectJ/Runtime/Items/
├─ PlayerItemInventoryInput.cs
└─ PlayerItemUseController.cs

Assets/ProjectJ/Runtime/Push/
├─ ExternalForceSource.cs
└─ PlayerExternalForceReceiver.cs
```

---

## 삭제 대상

52일차 공통 사용 테스트를 위해 사용한 임시 Effect 등록기는 실제 Effect 구현 후 더 이상 사용하지 않는다.

```text
Assets/ProjectJ/Tests/Manual/Day49/Scripts/
└─ Day52ItemUseDebugEffectInstaller.cs
```

Unity Project 창에서 삭제해 `.meta` 파일도 함께 정리한다.

---

## 54일차 완료 확인 항목

Unity에서 다음 항목을 확인한다.

```text
Compile Error 0
Console 반복 Exception 없음
```

아이템별 확인:

```text
[스프링 신발]
사용 후 8초 동안 추가 공중 점프 가능
착지 시 추가 점프 재충전
지속시간 종료 후 기본 점프로 복귀

[젤리 보호막]
4초 동안 플레이어 Push 차단
풍선 나팔 차단
물총 차단
AirBag 외력은 정상 적용

[바나나 쿠션]
평평한 바닥 설치 성공
허공 설치 실패 시 아이템 유지
다른 플레이어 접촉 시 발동 및 제거

[풍선 나팔]
전방 여러 플레이어 대상 적용
자기 자신과 후방 대상 제외
기본 밀치기 힘의 2.5배 적용
보호막 대상은 밀리지 않음

[물총]
우클릭 유지 중 지속 동작
우클릭 해제 시 즉시 종료
조준 방향 대상에 반복 Item Force 적용
보호막 대상은 밀리지 않음

[Day49 Dummy]
밀린 뒤 공중에 떠 있지 않음
절벽 밖으로 밀리면 정상 낙하
AirBag 등에 의해 뜬 뒤 다시 낙하
```

---

## 저장소 확인 상태

README 작성 시점의 원격 `main` 최신 커밋은 아직 53일차이다.

```text
175b48917212ab348c65e97b2a7fc3600f8f97aa
53일차 : 아이템 Sprite Atlas·대표 5종 아이콘 연동 및 인벤토리 UI 개선
```

따라서 54일차 변경 사항은 로컬 Unity에서 위 완료 조건을 확인한 뒤 README와 함께 다음 커밋으로 올린다.

원격 저장소에는 별도의 CI 상태 검사가 등록되어 있지 않으므로 Unity Play Mode 결과를 최종 완료 기준으로 사용한다.

---

## 다음 개발 방향

54일차에서 대표 5종의 실제 동작을 확보했으므로 다음 단계에서는 아이템 사용 피드백과 밸런스 조정, 설치·공격 아이템의 시각 효과, 멀티플레이 동기화 이전에 필요한 로컬 규칙 정리를 진행한다.
