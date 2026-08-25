# 119일차 : 비눗방울 서버 권한 투사체 및 점프 탈출형 이동 제한 상태 구현

## 개발 목표

- 비눗방울을 네트워크 아이템으로 등록한다.
- 서버 권한 직선 투사체를 발사한다.
- 투사체 속도 13m/s, 최대 이동 거리 16m를 적용한다.
- 상대 Player 적중 시 최대 2.5초 동안 이동·달리기·앉기 입력을 제한한다.
- 점프 입력은 유지하고, 점프 버튼을 새로 누른 횟수를 서버에서 기록한다.
- 점프 입력 6회 시 비눗방울에서 조기 탈출한다.
- 자기 자신, 완주 Player, Respawn 보호 Player에게는 효과를 적용하지 않는다.
- Respawn, 경기 종료, 상태 Clear 시 비눗방울 효과를 제거한다.

## 구현 내용

### 네트워크 아이템 등록

`ProjectJNetworkItemCatalog`에 비눗방울을 등록했다.

```text
Network ID: SoapBubble = 18
Key: soap_bubble
Display Name: 비눗방울
```

기존 `GrapplingHook = 17` 다음 번호를 사용한다.

기존 ItemDefinition은 다음 값을 유지한다.

```text
itemId: soap_bubble
displayName: 비눗방울
duration: 2.5
```

## 비눗방울 정책

`ProjectJSoapBubblePolicy.cs`를 추가했다.

핵심 수치:

```text
DurationSeconds = 2.5초
ProjectileSpeed = 13m/s
MaximumTravelDistance = 16m
CollisionRadius = 0.3m
EscapeJumpPressCount = 6회
```

정책 클래스에서 다음을 담당한다.

- Target 적용 가능 여부
- 이동 제한 활성 여부
- 새로운 점프 입력 시작 판정
- 점프 입력 횟수 증가
- 6회 탈출 판정
- 투사체 최대 이동 거리
- 재적중 시 2.5초 갱신

## 서버 권한 직선 투사체

`ProjectJNetworkSoapBubbleProjectile.cs`를 추가했다.

먹물 문어 투사체 구조를 기반으로 다음 흐름을 사용한다.

```text
비눗방울 사용
→ Runner.Spawn()
→ State Authority에서 직선 이동
→ SphereCast 충돌 판정
→ Player 또는 지형 적중
→ Player면 상태 적용 시도
→ 적중 또는 16m 도달 시 Despawn
```

투사체는 수평 전방 방향으로 이동한다.

사용자 자신의 Collider는 충돌 대상에서 제외한다.

## 투사체 이동 수치

```text
속도: 13m/s
최대 거리: 16m
충돌 반경: 0.3m
```

매 Network Tick마다 남은 거리를 계산해 최대 이동 거리를 넘지 않도록 이동한다.

## Target 유효성 검사

비눗방울 적중 시 다음 조건을 서버에서 검사한다.

```text
Runner 준비 완료
Gameplay 활성
자기 자신 아님
완주 Player 아님
Respawn 보호 아님
```

조건을 통과한 Player의 `ProjectJNetworkItemInventory`에 비눗방울 상태를 적용한다.

비눗방울은 외력 효과가 아니라 이동 입력 제한 상태이므로 Jelly Shield 외력 차단 판정을 사용하지 않는다.

## Networked 상태

`ProjectJNetworkItemInventory.SoapBubble.cs`에 다음 상태를 추가했다.

```text
NetworkSoapBubbleTimer
NetworkSoapBubbleJumpPressCount
NetworkSoapBubblePreviousJumpPressed
```

적중 시:

```text
Timer = 2.5초
JumpPressCount = 0
```

으로 시작한다.

효과 중 다시 적중하면 강도나 단계는 중첩하지 않고 지속 시간을 다시 2.5초로 갱신한다.

## 이동 입력 제한

`ProjectJNetworkPlayer.cs`의 이동 처리에서 비눗방울 상태를 확인한다.

비눗방울 활성 중:

```text
Move   → 차단
Sprint → 차단
Crouch → 차단
Jump   → 허용
```

실제 구현에서는 다음 상태만 변경한다.

```text
moveInput = Vector2.zero
LastReceivedSprint = false
LastReceivedCrouch = false
```

`LastReceivedJump`는 변경하지 않으므로 기존 점프 로직은 그대로 유지된다.

## 점프 6회 조기 탈출

점프 버튼을 누르고 있는 매 Tick을 세지 않고 새로운 눌림 시작만 계산한다.

```text
Jump 누름 시작
→ +1

계속 누르고 있음
→ 증가 없음

Jump 해제
→ 다음 입력 준비

다시 누름
→ +1
```

`NetworkSoapBubblePreviousJumpPressed`를 사용해 이전 Tick의 점프 입력을 기록한다.

점프 입력 횟수가 6회에 도달하면:

```text
Timer 제거
JumpPressCount 초기화
→ 이동 제한 즉시 종료
```

한다.

점프 동작 자체는 비눗방울 상태에서도 정상적으로 수행된다.

## 효과 종료 조건

다음 상황에서 비눗방울 상태를 제거한다.

- 2.5초 Timer 종료
- 점프 입력 6회
- Gameplay 비활성
- 경기 종료
- 완주
- 사용자 Respawn
- 인벤토리 Clear

## 메인 인벤토리 연결

`ProjectJNetworkItemInventory.cs`에 다음 연결을 추가했다.

```text
Spawned()
→ InitializeSoapBubbleAuthority()

FixedUpdateNetwork()
→ UpdateSoapBubbleLifetimeAuthority()

입력 수신 후
→ UpdateSoapBubbleJumpInputAuthority(input)

ClearAuthority()
→ ClearSoapBubbleAuthority()

HandleRespawnAuthority()
→ ClearSoapBubbleAuthority()

TryUseSelectedItemAuthority()
→ UseSoapBubbleAuthority()
```

## Network Prefab

다음 Resource Prefab을 추가했다.

```text
Assets/ProjectJ/Network/Fusion/Player/Resources/
└─ ProjectJNetworkSoapBubbleProjectile.prefab
```

구성:

```text
NetworkObject
NetworkTransform
ProjectJNetworkSoapBubbleProjectile
```

프로토타입 확인용 기본 Sphere Mesh를 사용한다.

최종 비눗방울 Mesh, Material, Shader, 파열 연출은 추후 아트 단계에서 교체한다.

## 테스트 추가

`ProjectJSoapBubblePolicyTests`를 추가했다.

총 36개 테스트 사례가 작성되어 있다.

검증 항목:

- 지속 시간 2.5초
- 투사체 속도 13m/s
- 최대 이동 거리 16m
- 충돌 반경 0.3m
- 탈출 점프 횟수 6회
- 정상 Target 허용
- Runner 미준비 차단
- Gameplay 비활성 차단
- 자기 자신 차단
- 완주 Player 차단
- Respawn 보호 차단
- 이동 제한 상태 판정
- 점프 눌림 시작 판정
- 점프 Hold 중복 카운트 차단
- 점프 횟수 6회 상한 처리
- 6회 탈출 판정
- 16m 경계 이동 종료
- 재적중 시 2.5초 갱신

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.SoapBubble.cs
├─ ProjectJNetworkItemInventory.SoapBubble.cs.meta
├─ ProjectJNetworkItemInventory.cs
├─ ProjectJNetworkPlayer.cs
├─ ProjectJNetworkSoapBubbleProjectile.cs
└─ ProjectJNetworkSoapBubbleProjectile.cs.meta

Assets/ProjectJ/Network/Fusion/Player/Resources/
├─ ProjectJNetworkSoapBubbleProjectile.prefab
└─ ProjectJNetworkSoapBubbleProjectile.prefab.meta

Assets/ProjectJ/Runtime/Items/
├─ ProjectJSoapBubblePolicy.cs
└─ ProjectJSoapBubblePolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJSoapBubblePolicyTests.cs
└─ ProjectJSoapBubblePolicyTests.cs.meta
```

삭제한 파일은 없다.

## 최신 커밋 검증

확인한 최신 `main` 커밋:

```text
d4c18ae1b84abc8b003f5290314752bf14458e66
```

현재 커밋 메시지는 임시 제목 `A`다.

정적 확인 내용:

- `SoapBubble = 18` 등록 확인
- `soap_bubble` Key 매핑 확인
- `비눗방울` 표시 이름 확인
- `InitializeSoapBubbleAuthority()` 연결 확인
- `UpdateSoapBubbleLifetimeAuthority()` 연결 확인
- `UpdateSoapBubbleJumpInputAuthority(input)` 연결 확인
- `ClearAuthority()` 비눗방울 상태 제거 확인
- `HandleRespawnAuthority()` 비눗방울 상태 제거 확인
- 아이템 사용 switch 연결 확인
- 서버 권한 투사체 Spawn 확인
- 투사체 13m/s 확인
- 최대 이동 거리 16m 확인
- 충돌 반경 0.3m 확인
- Respawn 보호 Target 차단 확인
- 2.5초 Networked Timer 확인
- Move·Sprint·Crouch 제한 확인
- Jump 입력 유지 확인
- 점프 Hold 중복 카운트 방지 확인
- 점프 6회 조기 탈출 확인
- 재적중 시 2.5초 갱신 확인
- Prefab의 비눗방울 스크립트 GUID와 `.meta` GUID 일치 확인
- `ProjectJSoapBubblePolicyTests` 36개 테스트 사례 작성 확인

GitHub에 등록된 CI Status가 없으므로 Unity Editor 실제 컴파일과 EditMode Test Runner 통과 여부는 GitHub만으로 확정하지 않았다.

## 테스트맵 Pickup 배치 보류

이번 일차에도 `Day49_AllSystemsTest`에 비눗방울 Pickup을 개별 배치하지 않는다.

Fusion Scene NetworkObject SortKey/Bake 문제를 줄이기 위해 신규 아이템 Pickup 배치는 현재 아이템 구현 페이즈 종료 후 한 번에 통합한다.

따라서 Pickup 미배치는 미완료가 아니라 계획된 단계 보류다.

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJSoapBubblePolicyTests` 전체 통과 확인
3. 비눗방울 사용 시 아이템 정상 소비 확인
4. 서버에서 투사체가 정상 Spawn되는지 확인
5. 투사체가 약 13m/s로 이동하는지 확인
6. 최대 16m 이동 후 제거되는지 확인
7. 지형 충돌 시 제거되는지 확인
8. 사용자 자신에게 적중하지 않는지 확인
9. 상대 Player 적중 시 비눗방울 상태가 적용되는지 확인
10. 이동 입력이 차단되는지 확인
11. 달리기가 차단되는지 확인
12. 앉기가 차단되는지 확인
13. 점프는 정상적으로 동작하는지 확인
14. 점프 버튼 Hold 중 카운트가 반복 증가하지 않는지 확인
15. 점프를 새로 6회 누르면 즉시 탈출하는지 확인
16. 2.5초 후 자동으로 이동 제한이 해제되는지 확인
17. 효과 중 재적중 시 지속 시간이 2.5초로 갱신되는지 확인
18. Respawn 보호 Player에게 효과가 적용되지 않는지 확인
19. Respawn 시 효과가 제거되는지 확인
20. 경기 종료 시 효과가 제거되는지 확인
21. Host와 Client에서 상태와 이동 제한 결과가 일관되게 보이는지 확인
22. Pickup 배치는 아이템 구현 페이즈 종료 후 통합
