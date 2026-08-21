# Project J - 56일차 개발 일지

## 개발 목표

55일차까지 구현한 대표 아이템과 설치/보호 규칙을 실제 플레이 중 확인하기 쉽도록 아이템 상태 HUD와 사용 피드백 UI를 추가하고, 대표 아이템의 런타임 상태 정보를 정리했다.

동시에 화면을 가리던 개발용 디버그 UI를 기본 비활성화하고 `F1` 키로 한 번에 표시할 수 있도록 전역 디버그 토글을 추가했으며, 화면에 표시되는 주요 디버그 문구를 한글로 정리했다.

이번 일차의 핵심 목표는 다음과 같다.

```text
아이템 지속 상태 HUD
+
아이템 사용 실패 피드백
+
대표 아이템 Runtime 상태 노출
+
설치형 아이템 안전 규칙 보강
+
부활 보호 / 젤리 보호막 외력 연동
+
디버그 UI 기본 OFF
+
F1 전역 토글
+
디버그 문구 한글화
```

---

## 주요 개발 내용

### 1. 아이템 상태 데이터 구조 추가

플레이어에게 표시할 지속 효과 정보를 공통 형식으로 전달하기 위해 `PlayerItemStatusEntry`를 추가했다.

상태 데이터에는 다음 정보가 포함된다.

```text
아이콘
아이콘 색상
표시 이름
상세 설명
남은 시간
남은 시간 표시 여부
상태 문자열
```

각 아이템 UI가 개별 Runtime 컴포넌트를 직접 해석하지 않고 동일한 데이터 구조를 통해 표시할 수 있도록 정리했다.

---

### 2. PlayerItemStatusTracker 추가

`PlayerItemStatusTracker`를 추가해 현재 플레이어에게 활성화되어 있는 아이템/보호 상태를 수집하도록 했다.

현재 수집 대상은 다음과 같다.

```text
스프링 신발
젤리 보호막
물총
부활 보호
```

상태 수집 흐름:

```text
Local Player
↓
PlayerItemStatusTracker
↓
각 Runtime 상태 확인
↓
PlayerItemStatusEntry 생성
↓
ItemStatusHudView 전달
```

---

### 3. 스프링 신발 상태 HUD 연동

`SpringShoesBuffState`에서 HUD가 사용할 상태 정보를 노출하도록 보강했다.

표시 정보:

```text
아이템명
남은 지속시간
추가 점프 가능 여부
```

추가 점프가 남아 있을 때:

```text
추가 점프 가능
```

추가 점프를 사용한 뒤에는:

```text
추가 점프 사용함
```

으로 표시된다.

스프링 신발의 기본 기능은 유지된다.

```text
지속시간 동안
공중 추가 점프 1회

착지
→ 추가 점프 다시 사용 가능

버프 종료
→ 추가 점프 기능 종료
```

---

### 4. 젤리 보호막 상태 HUD 연동

`JellyShieldState`에 현재 활성 상태, 남은 시간, 연결된 `ItemDefinition` 정보를 노출하도록 수정했다.

HUD에서는 다음과 같이 표시된다.

```text
젤리 보호막
Push / Item 방어
남은 시간
```

젤리 보호막의 방어 대상은 기존 규칙과 동일하게 유지된다.

```text
ExternalForceSource.Push
→ 차단

ExternalForceSource.Item
→ 차단

월드 장애물 외력
→ 차단하지 않음
```

---

### 5. 물총 Hold 상태 HUD 연동

`WaterGunRuntime`에 현재 활성 여부와 사용 중인 `ItemDefinition` 정보를 추가했다.

물총 사용 중에는 HUD에 다음 상태가 표시된다.

```text
물총
사용 버튼 유지 중
HOLD
```

물총은 사용 버튼을 누르고 있는 동안 일정 간격으로 전방을 검사하고 대상에게 작은 Item Force를 반복 적용한다.

사용 버튼을 놓으면 `IItemUseReleaseHandler`를 통해 Runtime이 종료된다.

---

### 6. 부활 보호 상태 HUD 표시

기존 `PlayerRespawnProtection` 상태도 아이템 상태 HUD에서 함께 확인할 수 있도록 연결했다.

표시 내용:

```text
부활 보호
적대 효과 면역
남은 보호 시간
```

아이템 효과와 부활 보호를 동일한 HUD 영역에서 확인할 수 있어 테스트 중 현재 방어 상태를 쉽게 판단할 수 있게 되었다.

---

### 7. ItemStatusHudView 추가

화면 우측 하단에 활성 상태를 표시하는 `ItemStatusHudView`를 추가했다.

현재 최대 표시 행 수:

```text
4개
```

각 행에는 다음 정보가 표시된다.

```text
아이콘
아이템 또는 상태 이름
상세 설명
남은 시간 또는 상태 문자열
```

활성 상태가 하나도 없으면 패널 전체를 자동으로 숨긴다.

따라서 평상시에는 화면을 차지하지 않고, 지속 효과가 발생한 경우에만 표시된다.

---

### 8. 아이템 사용 실패 전용 Canvas 추가

`ItemUseFeedbackCanvasView`를 추가해 아이템 사용 실패 메시지를 인벤토리 UI와 분리했다.

현재 처리하는 실패 상태:

```text
ItemUseStatus.InvalidPosition
```

표시 문구:

```text
해당 위치는 설치할 수 없습니다.
```

표시 규칙:

```text
설치 실패
↓
PlayerItemUseController.UseCompleted
↓
ItemUseFeedbackCanvasView
↓
빨간 안내 문구 표시
↓
1.6초 후 자동 숨김
```

반복 실패 시 기존 Coroutine을 중단하고 표시 시간을 다시 시작한다.

---

### 9. ItemInventoryRuntimeInstaller 확장

기존 인벤토리 Runtime Installer에서 다음 컴포넌트와 UI까지 자동 준비하도록 범위를 확장했다.

플레이어 측:

```text
PlayerItemInventory
PlayerItemUseController
PlayerItemInventoryInput
PlayerItemStatusTracker
```

UI 측:

```text
ItemInventoryCanvasView
ItemStatusHudView
ItemUseFeedbackCanvasView
```

Local Player를 찾은 뒤 각 View를 해당 컴포넌트에 자동으로 Bind한다.

씬 변경 시에도 Installer가 유지되고 새로운 Local Player를 다시 찾아 연결한다.

---

### 10. 공통 설치 위치 검증 구조 적용

설치형 아이템의 안전 판정을 위해 `ItemPlacementValidator`를 추가했다.

현재 설치 금지 검사:

```text
Day46 NoSpawn 영역
Checkpoint Collider 주변
Checkpoint Respawn 위치
START Respawn 위치
```

Checkpoint 주변에는 추가 Padding을 적용하며, Respawn/START 위치는 일정 반경과 높이 차이를 기준으로 보호한다.

현재 보호 기준:

```text
Respawn 보호 반경
2.5m

높이 허용 차이
3m

Checkpoint 추가 Padding
1.25m
```

---

### 11. 바나나 쿠션 설치 안전성 보강

`BananaCushionEffect`가 공통 `ItemPlacementValidator`를 사용하도록 연결했다.

설치 흐름:

```text
플레이어 앞쪽 위치 계산
↓
아래 방향 Raycast
↓
바닥 존재 여부 확인
↓
경사 확인
↓
설치 후보 Bounds 생성
↓
ItemPlacementValidator
↓
설치 가능 여부 결정
```

설치 불가능한 위치에서는:

```text
ItemUseStatus.InvalidPosition
```

을 반환하고 아이템을 소비하지 않는다.

---

### 12. 바나나 쿠션 보호 상태 상호작용 유지

`BananaCushionRuntime`은 대상에게 실제 Item Force가 적용된 경우에만 소모되도록 정리했다.

```text
일반 플레이어 접촉
→ Item Force 적용
→ 바나나 제거

젤리 보호막 상태
→ Item Force 차단
→ 바나나 유지

부활 보호 상태
→ Item Force 차단
→ 바나나 유지
```

보호 상태 플레이어가 함정을 단순 접촉만으로 제거하는 문제를 방지한다.

---

### 13. 부활 보호와 External Force 연결

`PlayerExternalForceReceiver`에서 부활 보호 상태를 확인하도록 연결했다.

적대 외력 판정:

```text
Push
→ 적대 효과

Item
→ 적대 효과
```

따라서 부활 보호 중에는 플레이어 밀치기와 대표 공격 아이템의 외력이 차단된다.

반면:

```text
AirBag
```

과 같은 월드 장애물은 적대 효과로 분류하지 않으므로 기존 장애물 동작을 유지한다.

---

### 14. 젤리 보호막과 External Force 규칙 유지

외력 처리 순서는 다음과 같이 정리된다.

```text
외력 요청
↓
현재 외력을 받을 수 있는 상태인지 확인
↓
부활 보호 검사
↓
젤리 보호막 검사
↓
External Force Accumulator 적용
```

이를 통해 부활 보호와 젤리 보호막을 서로 다른 방어 규칙으로 유지하면서 동일한 외력 파이프라인을 사용한다.

---

### 15. 전역 디버그 UI 토글 추가

`ProjectJDebugOverlayController`를 추가했다.

게임 시작 시:

```text
IsVisible = false
```

이므로 디버그 UI는 기본적으로 표시되지 않는다.

플레이 중:

```text
F1
```

을 누를 때마다 표시 상태가 전환된다.

```text
게임 시작
↓
디버그 UI OFF

F1
↓
디버그 UI ON

F1
↓
디버그 UI OFF
```

Controller는 Runtime에서 자동 생성되고 `DontDestroyOnLoad`로 유지된다.

---

### 16. 체크포인트/추락 디버그 UI 정리

다음 Debug View를 전역 F1 토글에 연결했다.

```text
CheckpointDebugView
FallLimitDebugView
RespawnDebugView
RespawnProtectionDebugView
```

주요 문구도 한글로 변경했다.

예시:

```text
Checkpoint
→ 체크포인트

Respawn Target
→ 부활 지점

Respawn Count
→ 부활 횟수

Fall Check
→ 추락 판정

Fall Limit Y
→ 추락 기준 Y

Respawn Protection
→ 부활 보호
```

START는 화면에서 `시작 지점`으로 표시된다.

---

### 17. 완주/결과/관전 디버그 UI 한글화

다음 개발용 UI도 F1 표시 상태와 연결하고 한글화했다.

```text
FinishDebugView
PlayerMatchResultDebugView
SpectatorDebugView
```

예시:

```text
Finish Count
→ 완주 인원

Local Player
→ 내 플레이어

Personal Result
→ 개인 결과

Spectating
→ 관전 중

Target
→ 관전 대상

Enter Spectator
→ 관전 시작

Exit Spectator
→ 관전 종료
```

---

### 18. 밀치기 피드백 UI 정리

`PlayerPushFeedbackUI`도 전역 디버그 표시 상태와 연결했다.

기본 상태에서는 밀치기 개발용 피드백 Canvas와 범위 LineRenderer가 보이지 않는다.

F1을 켠 상태에서 다음과 같은 문구를 확인할 수 있다.

```text
밀치기 준비
밀치기 대기
판정 : 명중
판정 : 빗나감
판정 : 재사용 대기
판정 : 보호됨
피격 방향 : 앞 / 뒤 / 왼쪽 / 오른쪽
```

따라서 일반 플레이 화면에서는 개발용 밀치기 텍스트가 화면을 가리지 않게 되었다.

---

## 생성 파일

```text
Assets/ProjectJ/Runtime/Debugging/
└─ ProjectJDebugOverlayController.cs

Assets/ProjectJ/Runtime/Items/Placement/
└─ ItemPlacementValidator.cs

Assets/ProjectJ/Runtime/Items/Status/
├─ PlayerItemStatusEntry.cs
└─ PlayerItemStatusTracker.cs

Assets/ProjectJ/Runtime/UI/
├─ ItemStatusHudView.cs
└─ ItemUseFeedbackCanvasView.cs
```

---

## 주요 수정 파일

```text
Assets/ProjectJ/Runtime/Checkpoint/
├─ CheckpointDebugView.cs
├─ FallLimitDebugView.cs
├─ RespawnDebugView.cs
└─ RespawnProtectionDebugView.cs

Assets/ProjectJ/Runtime/Finish/
└─ FinishDebugView.cs

Assets/ProjectJ/Runtime/Items/
└─ ItemInventoryRuntimeInstaller.cs

Assets/ProjectJ/Runtime/Items/Effects/
├─ BananaCushionEffect.cs
├─ BananaCushionRuntime.cs
├─ JellyShieldEffect.cs
├─ JellyShieldState.cs
├─ SpringShoesBuffState.cs
├─ SpringShoesEffect.cs
├─ WaterGunEffect.cs
└─ WaterGunRuntime.cs

Assets/ProjectJ/Runtime/Push/
├─ PlayerExternalForceReceiver.cs
└─ PlayerPushFeedbackUI.cs

Assets/ProjectJ/Runtime/Results/
└─ PlayerMatchResultDebugView.cs

Assets/ProjectJ/Runtime/Spectator/
└─ SpectatorDebugView.cs
```

---

## 삭제 파일

```text
없음
```

---

## 전체 동작 구조

### 지속 효과 HUD

```text
아이템 사용
↓
Runtime 상태 활성화
↓
PlayerItemStatusTracker
↓
활성 상태 수집
↓
PlayerItemStatusEntry
↓
ItemStatusHudView
↓
아이콘 / 이름 / 상세 / 남은 시간 표시
```

### 설치형 아이템

```text
바나나 쿠션 사용
↓
바닥 / 경사 검사
↓
ItemPlacementValidator
↓
NoSpawn / Checkpoint / Respawn / START 검사
        ↓
 ┌──────┴──────┐
실패          성공
↓              ↓
InvalidPosition 바나나 생성
↓              ↓
아이템 유지    아이템 소비
↓
실패 문구 표시
```

### 적대 외력 방어

```text
Push / Item Force 요청
↓
PlayerExternalForceReceiver
↓
부활 보호 검사
↓
젤리 보호막 검사
↓
허용된 경우만 외력 적용
```

### 디버그 UI

```text
게임 시작
↓
ProjectJDebugOverlayController
↓
IsVisible = false
↓
디버그 UI 숨김

F1 입력
↓
IsVisible 토글
↓
각 Debug View / Push Feedback 표시 전환
```

---

## 테스트 항목

```text
[아이템 상태 HUD]

스프링 신발 사용
→ HUD 표시
→ 남은 시간 감소
→ 추가 점프 상태 표시

젤리 보호막 사용
→ HUD 표시
→ 남은 시간 감소
→ Push / Item 방어 확인

물총 사용
→ HOLD 상태 표시
→ 버튼 해제 시 상태 제거

부활
→ 부활 보호 상태 표시
→ 보호 종료 후 HUD에서 제거
```

```text
[설치형 아이템]

일반 평지
→ 바나나 설치 성공

NoSpawn 영역
→ 설치 실패

Checkpoint 주변
→ 설치 실패

Respawn 주변
→ 설치 실패

START 주변
→ 설치 실패

설치 실패
→ 아이템 유지
→ "해당 위치는 설치할 수 없습니다." 표시
```

```text
[외력 보호]

부활 보호 중 Push
→ 차단

부활 보호 중 Item Force
→ 차단

부활 보호 중 AirBag
→ 허용

젤리 보호막 중 Push
→ 차단

젤리 보호막 중 Item Force
→ 차단
```

```text
[디버그 UI]

게임 시작
→ 디버그 UI가 보이지 않음

F1 1회
→ 디버그 UI 표시

F1 다시 입력
→ 디버그 UI 숨김

표시된 주요 개발용 문구
→ 한글로 출력

밀치기 개발용 Canvas / 범위 선
→ F1 OFF에서는 숨김
```

---

## 저장소 확인

README 작성 시점의 최신 `main` 커밋:

```text
e5137f03af480c6f4ebacfe35b99f1e44fa618d2
56
```

직전 커밋과 비교한 최신 커밋 변경량:

```text
변경 파일 : 33개
추가 : 2290줄
삭제 : 238줄
```

최신 커밋에서 다음 구현을 정적 코드 기준으로 확인했다.

```text
아이템 상태 데이터 구조
PlayerItemStatusTracker
ItemStatusHudView
ItemUseFeedbackCanvasView
Runtime Installer 자동 연결
설치 위치 검증
바나나 쿠션 설치 제한
부활 보호 / 젤리 보호막 외력 처리
스프링 신발 상태 정보
물총 Hold 상태 정보
F1 전역 디버그 토글
체크포인트 / 추락 / 부활 / 완주 / 결과 / 관전 디버그 한글화
밀치기 개발 UI F1 연동
```

GitHub에는 최신 커밋에 연결된 별도의 CI 상태 검사가 등록되어 있지 않았다.

따라서 GitHub 정적 코드 검토 기준으로 다음 개발을 막는 명확한 문제는 확인되지 않았지만, 최종 완료 판정은 Unity에서 컴파일 오류가 없는지와 위 Play Mode 테스트 항목이 실제로 통과하는지를 기준으로 한다.

---

## 다음 개발 방향

다음 단계에서는 현재 대표 아이템 5종과 HUD/보호/설치 규칙을 한 번에 검증하는 통합 테스트를 진행하고, 대표 아이템 단계에서 남아 있는 시각 효과와 게임 플레이 피드백을 정리한다.

우선 확인 대상:

```text
5종 아이템 연속 획득/사용
2슬롯 인벤토리 교체
지속 효과 HUD 중첩
부활 보호와 아이템 동시 작동
설치 제한과 실제 맵 구조 충돌 여부
F1 디버그 토글 상태
대표 아이템 사용 후 잔여 Runtime 정리
```
