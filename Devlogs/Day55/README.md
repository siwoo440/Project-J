# Project J - 55일차 개발 일지

## 개발 목표

54일차에서 구현한 대표 아이템 5종을 실제 게임 규칙에 안전하게 연결하기 위해 설치형 아이템의 설치 제한, 부활 보호와 적대 아이템 외력의 상호작용, 설치 실패 UI 피드백을 구현했다.

이번 일차의 핵심 목표는 다음과 같다.

```text
바나나 쿠션 설치 위치 검증
+
Day46 NoSpawn 영역 재사용
+
START / Checkpoint / Respawn 보호
+
부활 보호 중 Push / Item Force 차단
+
설치 실패 시 빨간 안내 문구 표시
```

---

## 주요 개발 내용

### 1. 공통 아이템 설치 위치 검사 구조 추가

설치형 아이템이 각자 금지 위치 판정을 반복하지 않도록 공통 설치 검사기 `ItemPlacementValidator`를 추가했다.

검사 흐름:

```text
설치 후보 위치 계산
↓
Day46 NoSpawn 영역 검사
↓
Checkpoint Trigger 주변 검사
↓
Checkpoint Respawn 위치 검사
↓
START 위치 검사
↓
모든 조건 통과
↓
설치 허용
```

현재 바나나 쿠션이 이 공통 검사 구조를 사용한다.

이후 지뢰, 트램폴린 등 다른 설치형 아이템도 같은 구조를 재사용할 수 있다.

---

### 2. Day46 NoSpawn 영역 재사용

기존 장애물 배치 안전 규칙에서 사용하던 `MapObstaclePlacementVolume`의 `NoSpawn` 영역을 아이템 설치 검사에도 연결했다.

따라서 Module Socket, Entrance / Exit 주변 등 기존 NoSpawn 영역에서는 바나나 쿠션을 설치할 수 없다.

또한 필수 착지면이나 좁은 통로처럼 추가 보호가 필요한 장소는 기존 `MapObstaclePlacementVolume`을 `NoSpawn`으로 배치해 동일한 설치 금지 규칙을 적용할 수 있다.

---

### 3. Checkpoint 설치 제한

Checkpoint Collider Bounds를 기준으로 주변에 추가 여유 공간을 적용해 설치 금지 영역으로 사용한다.

```text
Checkpoint Trigger
+
주변 Padding
↓
바나나 쿠션 설치 불가
```

이를 통해 체크포인트 진입 지점에 설치형 아이템이 배치되어 진행을 방해하는 문제를 방지한다.

---

### 4. Respawn 위치 보호

각 Checkpoint의 `RespawnPosition` 주변을 설치 금지 영역으로 추가했다.

현재 보호 범위:

```text
반경
2.5m

높이 허용 차이
3m
```

해당 범위 안에서는 바나나 쿠션 설치가 실패한다.

따라서 부활 직후 플레이어가 설치형 함정을 바로 밟는 상황을 방지한다.

---

### 5. START 위치 보호

`PlayerCheckpointTracker`가 가지고 있는 초기 Respawn 위치를 START 위치로 사용한다.

START 주변도 Checkpoint Respawn과 동일한 방식으로 설치 금지 처리한다.

```text
게임 시작 위치
↓
2.5m 보호 범위
↓
설치형 아이템 배치 금지
```

---

### 6. 바나나 쿠션 설치 실패 규칙 통일

기존에는 설치 실패 이유에 따라 서로 다른 메시지를 반환했다.

이번 작업에서는 다음 상황을 모두 공통 설치 실패로 처리한다.

```text
바닥 없음
경사 과다
NoSpawn 영역
Checkpoint 주변
Respawn 주변
START 주변
```

공통 결과:

```text
ItemUseStatus.InvalidPosition

해당 위치는 설치할 수 없습니다.
```

Effect가 실패한 경우 기존 아이템 사용 규칙에 따라 인벤토리에서 바나나 쿠션을 소비하지 않는다.

---

### 7. 설치 실패 UI 피드백 추가

아이템 설치가 실패하면 화면 아래 중앙에 빨간색 안내 문구가 표시되도록 인벤토리 Canvas UI를 확장했다.

표시 문구:

```text
해당 위치는 설치할 수 없습니다.
```

표시 규칙:

```text
InvalidPosition 발생
↓
PlayerItemUseController.UseCompleted
↓
ItemInventoryCanvasView 수신
↓
빨간 안내 문구 표시
↓
1.6초 후 자동 제거
```

문구에는 검은색 Outline을 적용해 밝은 배경에서도 확인하기 쉽게 했다.

반복해서 설치에 실패하면 기존 표시 Coroutine을 중단하고 표시 시간을 다시 시작한다.

---

### 8. Inventory UI와 ItemUseController 연결

기존 인벤토리 Runtime Installer는 `PlayerItemInventory`만 Canvas에 연결했다.

이번 작업에서는 `PlayerItemUseController`도 함께 전달해 UI가 아이템 사용 결과를 직접 받을 수 있도록 수정했다.

```text
Local Player
├─ PlayerItemInventory
└─ PlayerItemUseController
        ↓
ItemInventoryCanvasView.Bind()
```

이를 통해 향후 Cooldown, InvalidTarget, Blocked 등의 사용 실패 메시지도 동일한 UI 구조에서 확장할 수 있다.

---

### 9. 부활 보호와 적대 외력 연결

기존 `PlayerRespawnProtection`의 `TryAcceptHostileEffect()`를 `PlayerExternalForceReceiver`에 연결했다.

부활 보호가 활성화된 동안 다음 외력을 적대 효과로 처리한다.

```text
ExternalForceSource.Push
ExternalForceSource.Item
```

따라서 부활 직후 3초 동안:

```text
일반 플레이어 밀치기
→ 차단

풍선 나팔
→ 차단

물총
→ 차단

바나나 쿠션
→ 차단
```

된다.

---

### 10. 월드 장애물 외력 유지

부활 보호 상태에서도 `ExternalForceSource.AirBag`은 적대 효과로 취급하지 않는다.

```text
Push
→ 적대 효과

Item
→ 적대 효과

AirBag
→ 월드 장애물
```

따라서 부활 보호 중에도 에어백과 같은 월드 장애물의 힘은 정상적으로 적용된다.

보호 상태가 게임의 장애물 규칙까지 무효화하지 않도록 구분했다.

---

### 11. 젤리 보호막과 기존 외력 규칙 유지

`PlayerExternalForceReceiver`는 부활 보호 검사 이후 기존 젤리 보호막 검사도 계속 수행한다.

```text
외력 요청
↓
부활 보호 검사
↓
젤리 보호막 검사
↓
External Force 적용
```

기존 Jelly Shield의 Push / Item 차단 규칙은 유지된다.

---

### 12. 보호 상태 플레이어의 바나나 처리 수정

기존 바나나 쿠션은 대상에게 Force가 실제로 적용되었는지와 관계없이 접촉하면 바로 사라졌다.

이번 수정에서는:

```text
바나나 접촉
↓
Item Force 적용 요청
↓
적용 성공 여부 확인
```

후 실제 Force 적용에 성공한 경우에만 바나나를 제거한다.

따라서:

```text
부활 보호 상태
또는
젤리 보호막 상태
↓
Item Force 거부
↓
바나나 유지
```

가 된다.

보호 상태 플레이어가 함정을 무료로 제거하는 문제를 방지했다.

---

## 생성 파일

```text
Assets/ProjectJ/Runtime/Items/Placement/
└─ ItemPlacementValidator.cs
```

---

## 수정 파일

```text
Assets/ProjectJ/Runtime/Items/Effects/
├─ BananaCushionEffect.cs
└─ BananaCushionRuntime.cs

Assets/ProjectJ/Runtime/Items/
└─ ItemInventoryRuntimeInstaller.cs

Assets/ProjectJ/Runtime/Push/
└─ PlayerExternalForceReceiver.cs

Assets/ProjectJ/Runtime/UI/
└─ ItemInventoryCanvasView.cs
```

---

## 삭제 파일

```text
없음
```

---

## 동작 구조

### 바나나 쿠션 설치

```text
바나나 쿠션 사용
↓
바닥 Raycast
↓
경사 검사
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
빨간 안내 문구
↓
1.6초 후 제거
```

### 부활 보호

```text
Respawn
↓
3초 보호 시작
↓
PlayerExternalForceReceiver

Push / Item
→ 차단

AirBag
→ 허용
```

---

## 테스트 항목

```text
[설치 위치]

일반 평지
→ 바나나 설치 성공

바닥이 없는 위치
→ 설치 실패
→ 아이템 유지
→ 빨간 문구 표시

경사가 큰 위치
→ 설치 실패

Day46 NoSpawn 영역
→ 설치 실패

Checkpoint 주변
→ 설치 실패

Respawn 주변
→ 설치 실패

START 주변
→ 설치 실패
```

```text
[설치 실패 UI]

설치 실패
→ "해당 위치는 설치할 수 없습니다." 표시

문구 색상
→ 빨간색

약 1.6초 후
→ 자동으로 사라짐

연속 실패
→ 표시 시간 다시 시작
```

```text
[부활 보호]

Respawn 직후 일반 Push
→ 차단

Respawn 직후 풍선 나팔
→ 차단

Respawn 직후 물총
→ 차단

Respawn 직후 바나나 쿠션
→ 차단

Respawn 직후 AirBag
→ 정상 적용
```

```text
[바나나 보호 상호작용]

젤리 보호막 플레이어가 접촉
→ Force 차단
→ 바나나 유지

부활 보호 플레이어가 접촉
→ Force 차단
→ 바나나 유지

일반 플레이어가 접촉
→ Force 적용
→ 바나나 제거
```

---

## 저장소 확인

README 작성 시점의 최신 `main` 커밋:

```text
a8208b3cae5ccec4fddbd068d0b2898b81adabd1
55
```

최신 커밋에서 다음 구현을 확인했다.

```text
ItemPlacementValidator 추가
바나나 설치 위치 검증 연결
설치 실패 InvalidPosition 통일
실패 UI 이벤트 연결
빨간 설치 실패 메시지
1.6초 자동 제거
부활 보호 Push / Item 차단
AirBag 허용
바나나 Force 성공 시에만 제거
```

GitHub에는 별도의 CI 상태 검사가 등록되어 있지 않으므로 최종 완료 여부는 Unity Play Mode에서 위 테스트 항목을 확인하는 것을 기준으로 한다.

정적 코드 검토 기준으로 다음 개발을 막는 명확한 문제는 확인되지 않았다.

---

## 다음 개발 방향

다음 단계에서는 아이템 HUD, 지속 효과 상태, 사용 가능 여부와 실패 상태를 플레이어에게 더 명확하게 노출하는 작업을 진행한다.

주요 대상:

```text
아이템 지속시간 표시
Hold 아이템 사용 상태
젤리 보호막 활성 상태
스프링 신발 지속시간
사용 실패 메시지 확장
아이템 사용 시 시각 피드백
```
