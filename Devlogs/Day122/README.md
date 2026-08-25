# 122일차 : 거대 풍선 서버 권한 지속 상승 및 종료 하강 상태 구현

## 개발 목표

- 거대 풍선을 네트워크 아이템으로 등록한다.
- 서버 권한으로 거대 풍선 상태를 시작한다.
- 6초 동안 최소 4m/s의 상승 상태를 유지한다.
- 거대 풍선 활성 중 수평 이동 조작을 기존의 60%로 제한한다.
- 상승 중 천장 충돌 시 위쪽 이동을 차단한다.
- 6초 상승 종료 후 1.5초 동안 -2m/s의 완만한 하강 상태로 전환한다.
- 하강 상태 종료 후 일반 이동과 중력 처리로 복귀한다.
- 제트팩과 거대 풍선이 동시에 활성화되지 않도록 한다.
- 부활, 경기 종료, Gameplay Lock 시 거대 풍선 상태를 제거한다.
- 기존 밀치기 등 외부 힘 시스템은 차단하지 않는다.

## 네트워크 아이템 등록

`ProjectJNetworkItemCatalog`에 거대 풍선을 등록했다.

```text
Network ID: GiantBalloon = 21
Key: giant_balloon
Display Name: 거대 풍선
```

기존 `Trampoline = 20` 다음 번호를 사용한다.

기존 ItemDefinition은 다음 값을 사용한다.

```text
itemId: giant_balloon
displayName: 거대 풍선
duration: 6
isPlaceable: 0
```

## 거대 풍선 상태 구조

거대 풍선은 세 단계 상태로 관리한다.

```text
Inactive
Rising
Descending
```

Networked 상태:

```text
NetworkGiantBalloonPhaseValue
NetworkGiantBalloonTimer
```

상태 흐름:

```text
거대 풍선 사용
→ Rising
→ 6초
→ Descending
→ 1.5초
→ Inactive
```

## 거대 풍선 정책

`ProjectJGiantBalloonPolicy.cs`를 추가했다.

핵심 수치:

```text
RisingDurationSeconds = 6초
DescendingDurationSeconds = 1.5초
RisingSpeed = 4m/s
HorizontalControlMultiplier = 0.6
DescendingSpeed = -2m/s
```

정책 클래스에서 다음을 담당한다.

- 사용 가능 여부
- Rising / Descending / Inactive 상태 판정
- 수평 이동 속도 계산
- 상승 및 하강 수직 속도 계산
- 천장 차단 처리
- 상태 전환
- 단계별 Timer 시간
- Gameplay 종료 및 Object 무효 상태 정리

## 서버 권한 사용

`ProjectJNetworkItemInventory.GiantBalloon.cs`를 추가했다.

사용 시 다음 조건을 검사한다.

```text
Runner 준비
State Authority
Gameplay 허용
제트팩 비활성
거대 풍선 비활성
```

모든 조건이 맞으면:

```text
Phase = Rising
Timer = 6초
```

로 시작한다.

이미 거대 풍선 상태가 활성화되어 있거나 제트팩이 활성화되어 있으면 사용에 실패하고 아이템을 소비하지 않는다.

## 제트팩 상호 배제

기존 `ProjectJNetworkItemInventory.Jetpack.cs`의 제트팩 사용 진입점에도 거대 풍선 상태 검사를 추가했다.

```text
제트팩 활성 중
→ 거대 풍선 사용 실패

거대 풍선 활성 중
→ 제트팩 사용 실패
```

먼저 활성화된 상승 계열 아이템을 유지하고 나중에 사용한 아이템은 소비하지 않는다.

## Rising 단계

Rising 단계는 6초 동안 유지된다.

기존 중력 계산 이후 수직 속도를 다음 방식으로 보정한다.

```text
최종 Y 속도
= Max(현재 Y 속도, 4m/s)
```

따라서:

```text
낙하 중 -5m/s
→ 4m/s 상승

기존 점프 상승 7m/s
→ 7m/s 유지
```

처럼 기존의 더 높은 상승 속도는 강제로 낮추지 않는다.

## 수평 이동 60%

거대 풍선의 Rising과 Descending 상태 모두에서 현재 이동 속도에 0.6 배율을 적용한다.

처리 순서:

```text
기본 이동 속도
→ 깃털 신발 보정
→ 눈덩이 보정
→ 제트팩 보정
→ 거대 풍선 0.6 배율
```

예:

```text
걷기 5m/s
→ 3m/s

달리기 8m/s
→ 4.8m/s
```

## 천장 충돌 처리

기존 제트팩의 위쪽 SphereCast 천장 검사 함수를 재사용한다.

Rising 상태에서 이번 Tick의 예상 상승 거리를 계산한 뒤 위쪽에 외부 Collider가 존재하는지 검사한다.

천장에 막힌 경우:

```text
위쪽 수직 속도 제거
```

처리를 적용한다.

천장에 막혀도 Rising Timer는 계속 감소한다.

## Descending 단계

Rising 6초가 종료되면 자동으로 Descending 상태로 전환한다.

```text
Phase = Descending
Timer = 1.5초
```

공중에 있는 동안 Y 속도를:

```text
-2m/s
```

로 유지한다.

바닥에 닿으면 Y 속도는 0으로 정리된다.

1.5초가 끝나면:

```text
Phase = Inactive
Timer = None
```

으로 복귀하고 이후 기존 중력과 이동 시스템을 그대로 사용한다.

## Player 이동 시스템 연결

`ProjectJNetworkPlayer.cs`에 거대 풍선 상태 조회를 추가했다.

추가 조회 상태:

```text
IsGiantBalloonActive
IsGiantBalloonRising
IsGiantBalloonDescending
```

`CurrentMoveSpeed`에서 거대 풍선 수평 조작 60%를 적용한다.

`FixedUpdateNetwork()`에서는:

```text
Rising
→ 천장 검사
→ 최소 4m/s 상승

Descending
→ -2m/s 하강
```

을 기존 이동 계산에 연결했다.

거대 풍선 전용 새 Player 이동기를 만들지 않고 기존 `NetworkVerticalVelocity` 흐름을 재사용한다.

## 외부 힘 처리

거대 풍선은 외부 밀치기 시스템을 비활성화하지 않는다.

따라서:

```text
거대 풍선 상승
+
밀치기 또는 다른 외부 힘
→ 기존 외부 힘 정상 적용
```

구조를 유지한다.

## 초기화 및 종료

`ProjectJNetworkItemInventory.cs`에 다음 연결을 추가했다.

### Spawn 초기화

```text
InitializeGiantBalloonAuthority()
```

### 매 Network Tick

```text
UpdateGiantBalloonAuthority()
```

### 일반 상태 Clear

```text
ClearGiantBalloonAuthority()
```

### Respawn

```text
ClearGiantBalloonAuthority()
```

따라서 부활이나 경기 상태 종료 후 거대 풍선 Timer가 남지 않는다.

## 자동 Player 패치 처리

122일차 배포 ZIP에는 `ProjectJNetworkPlayer.cs` 변경을 적용하기 위한 1회용 Unity Editor 자동 패처를 사용했다.

자동 패처는 다음을 적용했다.

```text
거대 풍선 상태 프로퍼티
CurrentMoveSpeed 수평 60% 처리
FixedUpdateNetwork Rising / Descending 이동 처리
```

최신 커밋 확인 결과 Player 수정 내용은 실제 파일에 반영되어 있다.

1회용 자동 패처 파일은 최신 커밋에 남아 있지 않다.

## 테스트 추가

`ProjectJGiantBalloonPolicyTests.cs`를 추가했다.

총 42개 테스트 사례가 작성되어 있다.

검증 항목:

- Rising 6초
- Descending 1.5초
- 상승 속도 4m/s
- 수평 조작 60%
- 하강 속도 -2m/s
- Runner 준비 여부
- Gameplay 허용 여부
- 제트팩 활성 중 사용 차단
- 거대 풍선 재사용 차단
- Active / Rising / Descending 상태 판정
- Rising 중 수평 속도 60%
- Descending 중 수평 속도 60%
- 음수 기본 이동 속도 보정
- 낙하 중 Rising 시 4m/s 전환
- 기존 7m/s 상승 속도 보존
- 천장 충돌 시 상승 제거
- Descending 중 -2m/s 유지
- Descending 중 착지 시 Y 속도 0
- Rising → Descending 상태 전환
- Descending → Inactive 상태 전환
- 단계별 Timer 시간
- Gameplay 종료 Clear
- Object 무효 상태 Clear

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.GiantBalloon.cs
├─ ProjectJNetworkItemInventory.GiantBalloon.cs.meta
├─ ProjectJNetworkItemInventory.Jetpack.cs
├─ ProjectJNetworkItemInventory.cs
└─ ProjectJNetworkPlayer.cs

Assets/ProjectJ/Runtime/Items/
├─ ProjectJGiantBalloonPolicy.cs
└─ ProjectJGiantBalloonPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJGiantBalloonPolicyTests.cs
└─ ProjectJGiantBalloonPolicyTests.cs.meta
```

삭제한 게임 런타임 파일은 없다.

122일차 적용을 위해 사용된 1회용 Editor 자동 패처는 최종 최신 커밋에는 남아 있지 않다.

## 최신 커밋 검증

확인한 최신 `main` 커밋:

```text
0f26539dcb440bfc57db8424b33a5ded79bd2560
```

현재 커밋 메시지는 임시 제목:

```text
a
```

다.

정적 확인 내용:

- `GiantBalloon = 21` 등록 확인
- `giant_balloon` Key 매핑 확인
- `거대 풍선` 표시 이름 확인
- 인벤토리 사용 switch 연결 확인
- Rising / Descending Networked 상태 확인
- Rising 6초 Timer 확인
- Descending 1.5초 Timer 확인
- 최소 상승 속도 4m/s 확인
- 수평 이동 배율 0.6 확인
- 종료 하강 속도 -2m/s 확인
- 제트팩 활성 중 거대 풍선 사용 차단 확인
- 거대 풍선 활성 중 제트팩 사용 차단 확인
- Player의 거대 풍선 상태 조회 연결 확인
- Player `CurrentMoveSpeed` 거대 풍선 60% 적용 확인
- 기존 제트팩 천장 검사 재사용 확인
- Rising 천장 차단 처리 확인
- Respawn 시 거대 풍선 상태 제거 확인
- 일반 Clear 시 거대 풍선 상태 제거 확인
- Gameplay 상태 종료 시 거대 풍선 상태 제거 흐름 확인
- `ProjectJGiantBalloonPolicyTests` 테스트 사례 작성 확인
- 1회용 자동 패처가 최신 커밋에 남아 있지 않음 확인

GitHub Actions workflow run과 Commit Status가 등록되어 있지 않으므로 Unity Editor 실제 컴파일 성공과 EditMode Test Runner 전체 통과 여부는 GitHub만으로 확정하지 않았다.

## 테스트맵 Pickup 배치 보류

이번 일차에도 `Day49_AllSystemsTest`에 거대 풍선 Pickup을 개별 배치하지 않는다.

Fusion Scene NetworkObject SortKey/Bake 문제를 줄이기 위해 신규 아이템 Pickup 배치는 현재 아이템 구현 페이즈 종료 후 한 번에 통합한다.

따라서 Pickup 미배치는 계획된 단계 보류다.

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJGiantBalloonPolicyTests` 전체 통과 확인
3. 거대 풍선 사용 시 아이템 정상 소비 확인
4. 사용 직후 Rising 상태가 시작되는지 확인
5. 약 6초 동안 상승하는지 확인
6. 낙하 중 사용하면 최소 4m/s 상승으로 전환되는지 확인
7. 이미 4m/s보다 빠르게 상승 중이면 기존 높은 속도를 보존하는지 확인
8. Rising 중 수평 이동이 기존의 약 60%인지 확인
9. 천장에 닿으면 위쪽으로 관통하지 않는지 확인
10. 천장에 막혀 있는 동안에도 6초 Timer가 계속 감소하는지 확인
11. Rising 종료 후 Descending으로 자동 전환되는지 확인
12. Descending이 약 1.5초 유지되는지 확인
13. Descending 중 Y 속도가 약 -2m/s인지 확인
14. Descending 중 수평 이동도 약 60%인지 확인
15. 바닥에 닿으면 수직 속도가 0으로 정리되는지 확인
16. Descending 종료 후 일반 중력과 이동으로 복귀하는지 확인
17. 제트팩 활성 중 거대 풍선을 사용하면 실패하고 거대 풍선 아이템이 유지되는지 확인
18. 거대 풍선 활성 중 제트팩을 사용하면 실패하고 제트팩 아이템이 유지되는지 확인
19. 거대 풍선 중 상대 밀치기 등 외부 힘이 정상 적용되는지 확인
20. Respawn 시 거대 풍선 상태가 즉시 제거되는지 확인
21. 경기 종료 또는 Gameplay Lock 시 거대 풍선 상태가 제거되는지 확인
22. Host와 Client에서 단계와 이동 결과가 일관되게 보이는지 확인
23. Pickup 배치는 아이템 구현 페이즈 종료 후 통합
