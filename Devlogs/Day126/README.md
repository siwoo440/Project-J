# 126일차 개발일지 - 소형화 물약 서버 권한 80% 축소 및 안전 복귀

## 작업 개요

126일차에는 `소형화 물약(Shrink Potion)` 아이템을 네트워크 아이템으로 등록하고, 사용 시 플레이어의 충돌체·외형·카메라 높이를 6초 동안 80%로 축소하는 기능을 구현했다.

지속 시간이 끝났을 때 원래 크기로 돌아갈 공간이 부족한 경우에는 즉시 복구하지 않고 `RestorePending` 상태를 유지하며, 서버가 매 Tick 안전한 복귀 공간을 다시 검사하도록 구성했다.

---

## 구현 내용

### 1. 네트워크 아이템 등록

`ProjectJNetworkItemCatalog`에 다음 항목을 추가했다.

- Network Item ID: `25`
- Key: `shrink_potion`
- 표시 이름: `소형화 물약`

기존 `HomingMissile = 24` 다음 번호를 사용한다.

### 2. 상태 구조

소형화 물약은 다음 세 상태를 사용한다.

```text
Inactive = 0
Active = 1
RestorePending = 2
```

- `Inactive`: 일반 상태
- `Active`: 6초 소형화 지속 중
- `RestorePending`: 6초는 끝났지만 정상 크기로 복귀할 공간이 부족한 상태

`Active`와 `RestorePending`에서는 모두 축소된 크기를 유지한다.

### 3. 주요 정책 값

`ProjectJShrinkPotionPolicy`에 밸런스 값을 분리했다.

| 항목 | 값 |
| --- | ---: |
| 지속 시간 | 6초 |
| 축소 배율 | 80% |
| Standing 기본 Height | 2.0 |
| Crouch 기본 Height | 1.0 |
| 기본 Radius | 0.4 |
| 복귀 공간 검사 Radius 보정 | 95% |

### 4. Standing Collider 축소

기본 Standing Collider:

```text
Height = 2.0
Radius = 0.4
Center Y = 1.0
```

소형화 상태:

```text
Height = 1.6
Radius = 0.32
Center Y = 0.8
```

기존 Player 자세 처리 함수에서 소형화 상태를 함께 계산하도록 연결했다.

### 5. Crouch Collider 축소

기본 Crouch Collider:

```text
Height = 1.0
Radius = 0.4
```

소형화 + Crouch:

```text
Height = 0.8
Radius = 0.32
```

따라서 소형화 중에도 기존 Crouch 기능을 사용할 수 있다.

### 6. Player 전체 Transform은 축소하지 않음

Network Player 루트의 `transform.localScale`은 변경하지 않는다.

대신 다음 요소만 축소한다.

- CapsuleCollider Height
- CapsuleCollider Radius
- Visual 자식 Transform
- 로컬 Camera Marker 높이

이 방식으로 NetworkTransform, Player 위치, 기존 이동 시스템의 기준 좌표를 그대로 유지한다.

### 7. 외형 80% 축소

Player의 Visual 원본 Scale을 캐시한 뒤 소형화 상태에서 X/Y/Z에 축소 배율을 반영한다.

Crouch 상태에서는 기존 Crouch 외형 높이에 소형화 배율을 함께 적용한다.

```text
Standing Visual
→ 기존 외형 × 0.8

Crouch Visual
→ 기존 Crouch 표현 × 0.8
```

소형화가 끝나면 캐시한 원래 Scale을 기준으로 정상 크기로 복구한다.

### 8. 카메라 높이 축소

Authority Camera의 원래 Y 위치를 캐시한다.

소형화 중:

```text
Camera Y
→ 기존 높이 × 0.8
```

기존 Player Prefab의 Camera Marker 높이 1.6 기준으로는 약 1.28이 된다.

FOV와 카메라 감도는 변경하지 않는다.

### 9. 이동·점프 성능 유지

소형화는 크기만 변경한다.

다음 값은 변경하지 않는다.

- Walk Speed
- Sprint Speed
- Jump Speed
- Gravity
- Stamina

따라서 이동 성능 자체는 일반 상태와 동일하다.

### 10. 재사용 중첩 차단

소형화가 이미 `Active` 또는 `RestorePending` 상태라면 두 번째 소형화 물약 사용을 허용하지 않는다.

```text
Inactive
→ 사용 가능

Active
→ 사용 실패

RestorePending
→ 사용 실패
```

사용 실패 시 아이템은 소비하지 않는다.

### 11. 6초 종료 후 안전 복귀 검사

6초 Timer 종료 시 서버가 현재 자세 기준 정상 Collider가 들어갈 공간을 검사한다.

현재 Player가 서 있다면 Standing 크기로 검사하고, Crouch 중이라면 정상 Crouch 크기로 검사한다.

검사에는 `Physics.OverlapCapsuleNonAlloc()`을 사용한다.

자기 자신의 Collider는 검사 대상에서 제외한다.

### 12. RestorePending

정상 크기로 돌아갈 공간이 없는 경우:

```text
Active
→ 6초 종료
→ 복귀 공간 없음
→ RestorePending
```

으로 전환한다.

`RestorePending`에서는 작은 Collider와 Visual을 계속 유지한다.

매 서버 Tick 복귀 공간을 다시 확인하고:

```text
공간 확보
→ Inactive
→ 정상 크기 복귀
```

하도록 처리한다.

좁은 통로 안에서 강제로 커지면서 벽이나 천장과 겹치는 상황을 방지한다.

### 13. Respawn 및 전체 초기화

다음 상황에서는 소형화 상태를 제거한다.

- Inventory 전체 초기화
- Respawn
- Gameplay 비활성 상태

Respawn에서는 이전 소형화 Timer와 `RestorePending`을 유지하지 않고 정상 크기 상태로 초기화한다.

### 14. Inventory 사용 흐름 연결

`ProjectJNetworkItemInventory`에 다음 처리를 연결했다.

```text
FixedUpdateNetwork
→ UpdateShrinkPotionAuthority()

아이템 사용
→ ProjectJNetworkItemId.ShrinkPotion
→ UseShrinkPotionAuthority()

ClearAuthority
→ ClearShrinkPotionAuthority()

HandleRespawnAuthority
→ ClearShrinkPotionAuthority()
```

### 15. Player 표현 연결

`ProjectJNetworkPlayer`의 기존 자세 처리에 소형화 상태를 연결했다.

변경된 주요 흐름:

```text
ApplyColliderPosture()
→ 현재 Crouch 상태 확인
→ IsShrinkApplied 확인
→ Height / Radius 계산
→ CapsuleCollider 적용

ApplyCrouchPresentation()
→ Visual 원본 값 캐시
→ Crouch + Shrink 상태 계산
→ Visual Position / Scale 적용
→ Authority Camera Y 적용
```

`LateUpdate()`에서도 Collider와 Presentation을 계속 다시 적용하므로 Networked 상태가 변경된 뒤 각 Peer의 외형과 충돌 크기가 갱신되는 구조다.

---

## 테스트

`ProjectJShrinkPotionPolicyTests.cs`를 추가했다.

작성된 정책 테스트 사례는 총 34개다.

검증 범위:

- 지속시간 6초
- 축소 배율 0.8
- Standing Height 2.0 → 1.6
- Crouch Height 1.0 → 0.8
- Radius 0.4 → 0.32
- 비활성 상태에서 크기 변경 없음
- `Active`와 `RestorePending`에서 축소 유지
- `Inactive`에서만 사용 가능
- 지속시간 종료 후 공간 유무에 따른 상태
- `RestorePending` 안전 복귀
- 이동 속도 변화 없음
- 점프 속도 변화 없음
- Visual / Camera 표현값 80% 적용

---

## 변경 파일

125일차 커밋 대비 126일차 커밋은 1개 커밋만 앞서 있으며 다음 9개 파일이 변경되었다.

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.ShrinkPotion.cs
├─ ProjectJNetworkItemInventory.ShrinkPotion.cs.meta
├─ ProjectJNetworkItemInventory.cs
└─ ProjectJNetworkPlayer.cs

Assets/ProjectJ/Runtime/Items/
├─ ProjectJShrinkPotionPolicy.cs
└─ ProjectJShrinkPotionPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJShrinkPotionPolicyTests.cs
└─ ProjectJShrinkPotionPolicyTests.cs.meta
```

126일차 적용에 사용한 1회용 Editor Installer는 최종 커밋에 남아 있지 않고, Installer가 적용한 Catalog/Inventory/Player 변경 결과만 저장소에 반영되어 있다.

---

## Pickup 배치

이번에도 `Day49_AllSystemsTest` Scene에 소형화 물약 Pickup을 개별 추가하지 않는다.

Fusion Scene NetworkObject Bake/SortKey 문제를 피하기 위해 남은 아이템 구현이 완료된 뒤 신규 Pickup을 일괄 배치한다.

---

## 최신 커밋 검증

확인 기준 브랜치:

```text
main
```

확인 SHA:

```text
7c1bb58a181116146d4d90cf28ccd1d33762db5a
```

현재 커밋 메시지:

```text
a
```

125일차 커밋:

```text
b2ca192f0db01be1cf9238f0ca119acb89866015
```

비교 결과:

```text
ahead_by = 1
behind_by = 0
total_commits = 1
```

정적 확인 항목:

- `ShrinkPotion = 25` 등록
- `shrink_potion` Key 등록
- `소형화 물약` 표시 이름 등록
- Inventory 사용 분기 연결
- 6초 Network Timer
- `Inactive / Active / RestorePending` 상태
- 80% Collider Height 적용
- 80% Collider Radius 적용
- Standing / Crouch 상태 동시 지원
- `OverlapCapsuleNonAlloc` 안전 복귀 검사
- 공간 부족 시 `RestorePending`
- 공간 확보 후 `Inactive` 복귀
- Clear / Respawn 상태 제거
- Visual 80% 축소
- Authority Camera 높이 80% 적용
- 이동/점프 정책값 유지
- EditMode 정책 테스트 파일 포함
- 테스트 사례 34개 작성

GitHub Combined Status에는 등록된 Status가 없으며, 해당 커밋의 GitHub Actions Workflow Run도 없다.

따라서 저장소 코드 구조와 연결은 정적으로 확인했지만, Unity Editor 실제 컴파일 성공 및 Unity Test Runner 전체 통과 여부는 GitHub에서 증명할 수 없다.

---

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJShrinkPotionPolicyTests` 실행
3. 소형화 사용 시 아이템이 정상 소비되는지 확인
4. Standing Collider Height가 약 1.6인지 확인
5. Collider Radius가 약 0.32인지 확인
6. Visual이 기존 크기의 약 80%가 되는지 확인
7. Camera 높이가 기존의 약 80%가 되는지 확인
8. Walk Speed가 변하지 않는지 확인
9. Sprint Speed가 변하지 않는지 확인
10. Jump 높이가 변하지 않는지 확인
11. 소형화 중 Crouch가 정상 동작하는지 확인
12. Crouch Collider Height가 약 0.8인지 확인
13. Active 중 두 번째 소형화 사용이 실패하는지 확인
14. 6초 후 넓은 공간에서 즉시 정상 크기로 복귀하는지 확인
15. 좁은 공간에서는 `RestorePending`으로 작은 상태를 유지하는지 확인
16. 좁은 공간에서 넓은 곳으로 이동하면 정상 크기로 복귀하는지 확인
17. Respawn 시 정상 크기로 즉시 초기화되는지 확인
18. Host와 Client에서 Visual 크기가 일치하는지 확인
19. Host에서 Collider 판정이 소형화 크기와 일치하는지 확인
20. Pickup 배치는 아이템 구현 페이즈 종료 후 통합
