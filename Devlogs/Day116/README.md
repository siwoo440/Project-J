# 116일차 : 먹물 문어 서버 권한 투사체 및 대상 시야 방해 상태 동기화

## 개발 목표

- 먹물 문어를 네트워크 아이템으로 등록한다.
- 서버 권한으로 직선 투사체를 생성한다.
- 투사체를 16m/s로 이동시키고 최대 18m까지 유지한다.
- 상대 Player 적중 시 3.5초 먹물 상태를 적용한다.
- 먹물 상태는 중첩하지 않고 재적중 시 지속 시간만 3.5초로 갱신한다.
- 먹물 상태에 걸린 Player의 로컬 화면에만 시야 방해 Overlay를 표시한다.
- Respawn, 상태 해제, 경기 입력 차단 시 먹물 효과를 제거한다.

## 구현 내용

### 네트워크 아이템 등록

`ProjectJNetworkItemCatalog`에 먹물 문어를 추가했다.

```text
Network ID: InkOctopus = 15
Key: ink_octopus
Display Name: 먹물 문어
```

기존 `PufferBalloonSuit = 14` 다음 번호를 사용한다.

기존 ItemDefinition은 다음 값을 유지한다.

```text
itemId: ink_octopus
displayName: 먹물 문어
duration: 3.5
```

### 서버 권한 직선 투사체

`ProjectJNetworkInkOctopusProjectile`을 추가했다.

눈덩이 투사체 구조를 기준으로 다음 흐름을 사용한다.

```text
먹물 문어 사용
→ Runner.Spawn()
→ State Authority에서 이동
→ SphereCastNonAlloc 충돌 판정
→ 최초 유효 충돌 처리
→ Player 또는 지형 충돌 후 제거
```

주요 수치:

```text
투사체 속도: 16m/s
최대 이동 거리: 18m
충돌 반경: 0.3m
```

투척 사용자 자신의 Collider는 적중 대상에서 제외한다.

### 먹물 상태 적용

대상 Player의 `ProjectJNetworkItemInventory`에 `NetworkInkOctopusTimer`를 추가했다.

상대 적중 시 서버가 다음 조건을 검사한다.

- Runner와 State Authority가 유효한가
- Gameplay 입력이 허용된 Player인가
- 투척 사용자 자신이 아닌가
- 이미 완주한 Player가 아닌가
- Respawn 보호 상태가 아닌가

조건을 만족하면 대상에게 3.5초 먹물 상태를 적용한다.

### 먹물 중첩 규칙

먹물 농도는 중첩하지 않는다.

```text
첫 적중
→ 3.5초 먹물 상태

효과 중 재적중
→ 화면 방해 강도는 그대로 유지
→ 남은 시간을 다시 3.5초로 갱신
```

따라서 여러 번 적중해도 Overlay가 더 어두워지거나 추가로 쌓이지 않는다.

### 로컬 시야 방해 Overlay

먹물 상태는 Networked Timer로 동기화하고 실제 화면 효과는 Input Authority Player에게만 표시한다.

`Render()`에서 로컬 Player 여부와 먹물 상태를 확인한 뒤 필요한 경우 Canvas와 Image를 생성한다.

현재 프로토타입 Overlay:

```text
Canvas: Screen Space Overlay
가로 범위: 화면의 90%
세로 범위: 화면의 75%
면적: 약 67.5%
Alpha: 0.82
```

기획의 “화면 중앙 약 65% 시야 방해”를 임시 검은 Overlay로 구현했다.

현재는 최종 먹물 Sprite 없이 기능 검증용 UI이며 추후 실제 먹물 이미지로 교체할 수 있다.

### Respawn 및 상태 초기화

다음 상황에서 먹물 Timer를 제거한다.

- 인벤토리 전체 Clear
- Respawn
- 효과 시간 종료

메인 `ProjectJNetworkItemInventory.cs`에 다음 연결이 추가됐다.

```text
Spawned()
→ InitializeInkOctopusAuthority()

ClearAuthority()
→ ClearInkOctopusAuthority()

HandleRespawnAuthority()
→ ClearInkOctopusAuthority()

TryUseSelectedItemAuthority()
→ UseInkOctopusAuthority()
```

### 보호 규칙

이번 먹물 문어는 외부 속도를 주는 아이템이 아니라 시야 방해 상태이므로 `TryApplyExternalVelocityChange()`를 사용하지 않는다.

적용 차단:

- 자기 자신
- 완주 Player
- Gameplay 비활성 Player
- Respawn 보호 Player

현재 Jelly Shield는 먹물 상태 자체를 차단하지 않는다.

## 정책 분리

`ProjectJInkOctopusPolicy.cs`를 추가해 먹물 문어의 수치와 공통 판정을 분리했다.

주요 정책:

- 지속 시간 3.5초
- 투사체 속도 16m/s
- 최대 거리 18m
- 충돌 반경 0.3m
- Overlay 크기
- 대상 적용 가능 여부
- 재적중 시 지속 시간 갱신
- 최대 이동 거리 도달 판정

## 테스트 추가

`ProjectJInkOctopusPolicyTests`를 추가했다.

총 20개 테스트 사례를 구성했다.

검증 항목:

- 지속 시간 3.5초
- 투사체 속도 16m/s
- 최대 거리 18m
- 충돌 반경 0.3m
- Overlay 면적 약 67.5%
- 정상 Target 적용 허용
- Runner 준비 실패 차단
- Gameplay 비활성 차단
- 사용자 자신 차단
- 완주 Player 차단
- Respawn 보호 차단
- 기존 시간이 0초일 때 3.5초 갱신
- 기존 시간이 남아 있어도 3.5초 갱신
- 음수 기존 시간 보정
- 18m 미만 이동 유지
- 18m 이상 도달 시 종료

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkInkOctopusProjectile.cs
├─ ProjectJNetworkInkOctopusProjectile.cs.meta
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.cs
├─ ProjectJNetworkItemInventory.InkOctopus.cs
└─ ProjectJNetworkItemInventory.InkOctopus.cs.meta

Assets/ProjectJ/Network/Fusion/Player/Resources/
├─ ProjectJNetworkInkOctopusProjectile.prefab
└─ ProjectJNetworkInkOctopusProjectile.prefab.meta

Assets/ProjectJ/Runtime/Items/
├─ ProjectJInkOctopusPolicy.cs
└─ ProjectJInkOctopusPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJInkOctopusPolicyTests.cs
└─ ProjectJInkOctopusPolicyTests.cs.meta
```

## 최신 커밋 검증

확인한 최신 `main` 커밋:

```text
6c50845fab6ddd6b3adb9c7d3d68b103c8704221
```

현재 커밋 메시지는 임시 제목 `a`다.

정적 확인 내용:

- `InkOctopus = 15` 등록 확인
- `ink_octopus` Key 매핑 확인
- `먹물 문어` 표시 이름 확인
- `InitializeInkOctopusAuthority()` 연결 확인
- `ClearInkOctopusAuthority()` Clear 연결 확인
- Respawn 시 먹물 상태 제거 연결 확인
- 먹물 문어 아이템 사용 switch 연결 확인
- 서버 권한 투사체 생성 구조 확인
- 투사체 16m/s 이동 확인
- 최대 이동 거리 18m 확인
- 3.5초 Networked 먹물 Timer 확인
- 재적중 시 농도 중첩 없이 3.5초 갱신 확인
- 자기 자신 적중 제외 확인
- 완주 Player 차단 확인
- Respawn 보호 차단 확인
- Input Authority Player에게만 Overlay 표시 확인
- Overlay 중앙 약 67.5% 범위 확인
- Network Prefab의 스크립트 GUID 연결 확인
- 정책 EditMode 테스트 20개 사례 작성 확인

GitHub에 등록된 CI Status가 없으므로 Unity Editor 실제 컴파일과 EditMode Test Runner 통과 여부는 GitHub만으로 확정하지 않았다.

## 테스트맵 Pickup 배치 보류

이번 일차에도 `Day49_AllSystemsTest`에 먹물 문어 Pickup을 개별 배치하지 않는다.

Fusion Scene NetworkObject SortKey/Bake 문제를 줄이기 위해 신규 아이템 Pickup 배치는 현재 아이템 구현 페이즈 종료 후 한 번에 통합한다.

따라서 Pickup 미배치는 미완료가 아니라 계획된 단계 보류다.

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJInkOctopusPolicyTests` 전체 통과 확인
3. 먹물 문어 사용 시 아이템 정상 소비 확인
4. 서버에서 투사체 정상 Spawn 확인
5. 투사체가 16m/s로 직선 이동하는지 확인
6. 최대 18m에서 제거되는지 확인
7. 지형 최초 충돌 시 제거되는지 확인
8. 사용자 자신의 Collider를 무시하는지 확인
9. 상대 적중 시 3.5초 먹물 상태가 적용되는지 확인
10. 대상 Player 화면에만 Overlay가 표시되는지 확인
11. 다른 Player 화면에는 Overlay가 표시되지 않는지 확인
12. 재적중 시 Overlay 농도가 중첩되지 않는지 확인
13. 재적중 시 지속 시간이 다시 3.5초로 갱신되는지 확인
14. Respawn 보호 Player에게 먹물이 적용되지 않는지 확인
15. 완주 Player에게 적용되지 않는지 확인
16. Respawn 시 먹물 상태가 즉시 제거되는지 확인
17. 3.5초 후 Overlay가 사라지는지 확인
18. Host와 Client에서 적중 결과가 동일하게 보이는지 확인
19. 실제 먹물 Sprite 적용은 추후 아트 단계에서 교체
20. Pickup 배치는 아이템 구현 페이즈 종료 후 통합
