# Project J - 57일차 개발 일지

## 개발 목표

PHASE 5의 마지막 일차로, 대표 아이템 5종과 인벤토리·상태 HUD·설치 제한·부활 보호·외력 처리·디버그 UI까지 지금까지 구현한 아이템 시스템을 최종 통합 검증한다.

이번 일차에서는 새로운 Runtime 기능을 추가하지 않고, PHASE 5에서 구현한 기능이 서로 충돌하지 않는지 자동 테스트와 Day49 수동 테스트를 통해 확인하는 것을 목표로 한다.

---

## 주요 개발 내용

### 1. PHASE 5 최종 통합 테스트 추가

다음 테스트 파일을 신규 추가했다.

```text
Assets/ProjectJ/Tests/EditMode/
└─ Phase5FinalIntegrationTests.cs
```

기존 Runtime 코드는 수정하지 않고, 현재 프로젝트의 실제 ItemDefinition과 아이템 사용 파이프라인을 이용해 PHASE 5 핵심 기능을 검증하도록 구성했다.

---

### 2. 대표 아이템 5종 데이터 검증

다음 대표 아이템 데이터를 실제 프로젝트 Asset에서 불러와 검증한다.

```text
스프링 신발
젤리 보호막
바나나 쿠션
풍선 나팔
물총
```

검사 항목:

```text
ItemDefinition 존재 여부
Item ID
한글 Display Name
ItemDefinition 유효성
아이콘 연결 여부
```

---

### 3. 2슬롯 인벤토리 통합 검증

현재 인벤토리의 핵심 규칙을 다시 확인한다.

```text
슬롯 수 = 2
빈 슬롯 우선 저장
Q / E 슬롯 선택
두 슬롯이 가득 찬 경우
→ 현재 선택 슬롯 교체
```

세 번째 아이템을 획득했을 때 선택 슬롯만 정상적으로 교체되고 다른 슬롯은 유지되는지 테스트한다.

---

### 4. 스프링 신발 최종 검증

스프링 신발을 공통 아이템 사용 파이프라인으로 사용한다.

확인 항목:

```text
아이템 사용 성공
↓
인벤토리에서 아이템 소비
↓
SpringShoesBuffState 생성
↓
버프 활성화
↓
남은 시간 존재
↓
추가 점프 사용 가능
```

실제 추가 점프 조작은 Day49 Play Mode에서 최종 확인한다.

---

### 5. 젤리 보호막 외력 규칙 검증

젤리 보호막의 외력 차단 규칙을 검사한다.

```text
Player Push
→ 차단

Item Force
→ 차단

AirBag
→ 허용
```

플레이어와 아이템에서 발생한 적대 외력은 막고, 월드 장애물의 힘은 유지하는 현재 규칙을 기준선으로 고정한다.

---

### 6. 바나나 쿠션 설치 실패 규칙 검증

바닥이 없는 위치에서 바나나 쿠션을 사용하도록 테스트해 설치 실패 흐름을 확인한다.

```text
바나나 쿠션 사용
↓
설치 위치 없음
↓
InvalidPosition
↓
사용 실패
↓
아이템 유지
```

설치 실패 시 아이템이 소비되지 않는 공통 규칙을 다시 검증한다.

실제 NoSpawn / Checkpoint / Respawn / START 설치 제한은 Day49에서 수동 확인한다.

---

### 7. 풍선 나팔 사용 흐름 검증

주변 대상이 없는 상태에서도 풍선 나팔 사용 자체는 정상 완료되는지 확인한다.

```text
풍선 나팔 사용
↓
Effect 실행 성공
↓
아이템 정상 소비
```

실제 대상 밀치기, 젤리 보호막 차단, 부활 보호 차단은 Day49에서 최종 확인한다.

---

### 8. 물총 Hold / Release 검증

물총의 Hold 사용 흐름을 검사한다.

```text
물총 사용
↓
WaterGunRuntime 생성
↓
IsActive = true
↓
아이템 소비

사용 입력 해제
↓
NotifyUseInputReleased()
↓
WaterGunRuntime 비활성화
```

버튼을 놓은 뒤 Hold 상태가 계속 남는 문제를 방지한다.

---

### 9. 아이템 상태 Tracker 통합 검증

`PlayerItemStatusTracker`가 동시에 활성화된 상태를 정상적으로 수집하는지 확인한다.

테스트 상태:

```text
스프링 신발
젤리 보호막
물총
```

검증 항목:

```text
상태 3개 수집
아이템 이름 정상
물총 HOLD 상태 정상
```

이를 통해 56일차에서 구현한 상태 HUD 데이터 흐름이 정상 유지되는지 확인한다.

---

### 10. F1 디버그 UI 기본 상태 검증

`ProjectJDebugOverlayController`의 초기 상태가 숨김인지 테스트한다.

```text
게임 시작
↓
Debug Overlay
↓
IsVisible = false
```

실제 F1 입력을 통한 표시/숨김 전환과 한글 디버그 UI는 Day49 Play Mode에서 최종 확인한다.

---

## 생성 파일

```text
Assets/ProjectJ/Tests/EditMode/
└─ Phase5FinalIntegrationTests.cs
```

---

## 수정 파일

```text
없음
```

---

## 삭제 파일

```text
없음
```

---

## 자동 테스트 항목

Unity Test Runner의 EditMode에서 `Phase5FinalIntegrationTests`를 실행한다.

검증 항목:

```text
대표 5종 ItemDefinition 정상
2슬롯 인벤토리 정상
선택 슬롯 교체 정상
스프링 신발 사용 및 소비 정상
젤리 보호막 외력 구분 정상
바나나 설치 실패 시 아이템 유지
풍선 나팔 사용 및 소비 정상
물총 Hold / Release 정상
상태 Tracker 정상
디버그 UI 기본 OFF 정상
```

---

## Day49 수동 통합 테스트

자동 테스트 통과 후 다음 씬에서 최종 확인한다.

```text
Assets/ProjectJ/Tests/Manual/Day49/
Day49_AllSystemsTest.unity
```

확인 순서:

```text
1. 대표 아이템 5종 Pickup
2. Q / E 슬롯 선택
3. 2슬롯 교체 규칙
4. 스프링 신발 실제 추가 점프
5. 젤리 보호막 상태에서 Push 차단
6. 젤리 보호막 상태에서 풍선 나팔 차단
7. 젤리 보호막 상태에서 물총 차단
8. AirBag은 정상 작동
9. 바나나 일반 바닥 설치 성공
10. 설치 금지 위치에서 설치 실패
11. 설치 실패 시 빨간 안내 문구
12. 풍선 나팔 실제 밀치기
13. 물총 Hold 및 Release
14. 지속 효과 HUD 중첩
15. Respawn 후 부활 보호
16. Respawn 후 조작 상태 정상
17. F1 디버그 UI 표시 / 숨김
18. Console Error 없음
```

---

## PHASE 5 완료 기준

다음 조건을 모두 만족하면 PHASE 5를 완료 처리한다.

```text
대표 아이템 5종 획득 가능
↓
2슬롯 인벤토리 정상
↓
Q / E 선택 정상
↓
5종 아이템 사용 정상
↓
성공 시 아이템 소비
↓
실패 시 아이템 유지
↓
설치 제한 정상
↓
젤리 보호막 정상
↓
부활 보호 정상
↓
Push / Item / AirBag 규칙 충돌 없음
↓
상태 HUD 정상
↓
물총 Hold 종료 정상
↓
Respawn 후 상태 이상 없음
↓
F1 디버그 UI 정상
↓
EditMode 테스트 통과
↓
Console Error 없음
```

위 기준을 통과하면 PHASE 5의 대표 아이템 시스템 구현과 통합 검증을 종료하고 다음 PHASE로 넘어간다.

---

## 다음 개발 방향

57일차 완료 후 PHASE 5를 종료한다.

다음 PHASE부터는 대표 아이템 5종의 오프라인 기능 구현을 기준선으로 유지하고, 멀티플레이 환경에서 필요한 Authority와 네트워크 동기화 구조를 단계적으로 연결한다.
