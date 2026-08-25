# 115일차 : 복어 풍선옷 서버 권한 근접 자동 밀치기 및 대상별 재발동 제한

## 개발 목표

- 복어 풍선옷을 네트워크 아이템으로 등록한다.
- 사용 시 서버 권한으로 5초간 효과를 활성화한다.
- 효과 중 사용자 주변 1.2m 범위의 상대를 자동 감지한다.
- 감지된 상대를 사용자 바깥 방향으로 6m/s 밀어낸다.
- 같은 상대에게는 1초에 한 번만 다시 발동하도록 제한한다.
- 여러 상대가 동시에 범위에 들어오면 각각 독립적으로 처리한다.
- 기존 Jelly 보호막, Respawn 보호, 외부 힘 처리 구조를 재사용한다.
- 효과 종료 또는 Respawn 시 즉시 상태를 정리한다.

## 구현 내용

### 네트워크 아이템 등록

`ProjectJNetworkItemCatalog`에 복어 풍선옷을 등록했다.

- Network ID: `PufferBalloonSuit = 14`
- Key: `puffer_balloon_suit`
- 표시 이름: `복어 풍선옷`

기존 `Bomb = 13` 다음 번호를 사용했다.

기존 ItemDefinition도 다음 값으로 유지한다.

```text
itemId: puffer_balloon_suit
displayName: 복어 풍선옷
duration: 5
```

### 서버 권한 5초 상태

`ProjectJNetworkItemInventory.PufferBalloonSuit.cs`를 추가했다.

Fusion `TickTimer`를 사용해 State Authority가 복어 풍선옷의 지속 시간을 관리한다.

사용 조건:

- Runner가 존재
- NetworkObject가 유효
- State Authority 보유
- Gameplay 입력 허용
- 이미 복어 풍선옷이 활성 상태가 아님

조건을 만족할 때만 5초 Timer를 생성하고 아이템 사용을 성공 처리한다.

### 메인 인벤토리 연결

`ProjectJNetworkItemInventory.cs`에 복어 풍선옷의 실제 실행 연결을 추가했다.

연결 항목:

- `Spawned()`에서 초기화
- `FixedUpdateNetwork()`에서 자동 밀치기 업데이트
- `ClearAuthority()`에서 효과 제거
- `HandleRespawnAuthority()`에서 효과 제거
- `TryUseSelectedItemAuthority()`에서 아이템 사용 분기 연결

따라서 네트워크 아이템 등록만 존재하고 실제 사용이 불가능했던 연결 누락 상태를 해소했다.

### 1.2m 근접 자동 감지

효과가 활성 상태일 때 State Authority에서 현재 Runner의 Player 목록을 조회한다.

사용자와 각 대상의 3차원 거리를 계산하고 다음 조건을 만족하는 대상만 처리한다.

- 자기 자신이 아님
- NetworkObject가 유효
- 대상 Gameplay 상태가 유효
- 사용자와의 거리가 1.2m 이하
- 해당 대상의 1초 재발동 제한이 끝난 상태

실제 Player Collider 크기를 변경하지 않고 별도의 거리 판정만 사용한다.

### 6m/s 바깥 방향 밀치기

범위 안 상대가 감지되면 사용자에서 상대 방향으로 수평 벡터를 계산한다.

최종 외력:

```text
사용자 → 상대 방향
× 6m/s
```

높이 차이는 감지 거리에는 포함하지만 밀치기 방향에서는 Y축을 제거해 수평 방향으로 적용한다.

사용자와 대상 위치가 거의 같은 경우 사용자 전방을 대체 방향으로 사용한다.

### 기존 외력 시스템 재사용

복어 풍선옷은 별도의 이동 시스템을 만들지 않고 기존 다음 경로를 사용한다.

```text
복어 풍선옷 자동 감지
→ TryApplyExternalVelocityChange()
→ ProjectJExternalForceSource.Item
→ 기존 외부 힘 보호 및 이동 처리
```

따라서 기존 적대 아이템과 동일하게 Jelly 보호막과 Respawn 보호 판정을 재사용한다.

보호 상태 때문에 외력 적용이 실패한 경우에는 대상별 1초 재발동 제한을 시작하지 않는다.

### 대상별 1초 재발동 제한

각 대상의 `PlayerRef.AsIndex`를 Key로 사용해 개별 TickTimer를 관리한다.

동작 예시:

```text
P2 밀치기 성공
→ P2 1초 제한 시작

P3가 바로 범위 진입
→ P3는 별도 대상이므로 즉시 밀치기 가능

1초 후
→ P2 다시 밀치기 가능
```

따라서 여러 Player가 동시에 범위에 있어도 한 명의 쿨타임이 다른 Player에게 영향을 주지 않는다.

### 효과 종료 처리

다음 상황에서 효과를 즉시 제거한다.

- 5초 지속 시간 종료
- Gameplay 입력 불가 상태
- 인벤토리 전체 Clear
- Respawn

효과를 제거할 때 대상별 재발동 Timer 기록도 함께 초기화한다.

## 정책 분리

`ProjectJPufferBalloonSuitPolicy.cs`를 추가했다.

주요 수치:

- 지속 시간: 5초
- 감지 반경: 1.2m
- 밀치기 외력: 6m/s
- 대상별 재발동 제한: 1초

정책 클래스에서 다음 규칙을 담당한다.

- 아이템 활성화 가능 여부
- 감지 반경 포함 여부
- 자기 자신 제외
- 대상별 쿨타임 중 재발동 차단

## 테스트 추가

`ProjectJPufferBalloonSuitPolicyTests`를 추가했다.

총 15개 테스트 사례를 구성했다.

검증 항목:

- 지속 시간 5초
- 감지 반경 1.2m
- 밀치기 속도 6m/s
- 대상별 재발동 제한 1초
- 정상 상태 사용 허용
- 이미 활성 상태일 때 중복 사용 차단
- Gameplay 잠금 상태 사용 차단
- Runner 준비 실패 차단
- 거리 0m 포함
- 1.2m 경계 포함
- 1.2m 초과 제외
- 잘못된 음수 거리 보정
- 다른 대상 발동 허용
- 자기 자신 제외
- 대상 쿨타임 중 재발동 차단

## 변경 파일

115일차 기준 주요 변경 파일:

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.cs
├─ ProjectJNetworkItemInventory.PufferBalloonSuit.cs
└─ ProjectJNetworkItemInventory.PufferBalloonSuit.cs.meta

Assets/ProjectJ/Runtime/Items/
├─ ProjectJPufferBalloonSuitPolicy.cs
└─ ProjectJPufferBalloonSuitPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJPufferBalloonSuitPolicyTests.cs
└─ ProjectJPufferBalloonSuitPolicyTests.cs.meta
```

기존 `Item_PufferBalloonSuit.asset`은 이미 필요한 ID와 5초 지속 시간이 설정되어 있어 수정하지 않았다.

## 최신 커밋 검증

확인한 최신 `main` 커밋:

```text
0f066221f1533a6523c5b07c603481ae925efe3c
```

현재 커밋 메시지는 임시 제목 `a`다.

확인 내용:

- `PufferBalloonSuit = 14` 등록 확인
- `puffer_balloon_suit` Key 매핑 확인
- `복어 풍선옷` 표시 이름 확인
- `InitializePufferBalloonSuitAuthority()` 연결 확인
- `UpdatePufferBalloonSuitAuthority()` Tick 연결 확인
- `ClearAuthority()` 효과 제거 연결 확인
- `HandleRespawnAuthority()` 효과 제거 연결 확인
- 아이템 사용 switch 분기 연결 확인
- State Authority 기반 5초 TickTimer 확인
- 1.2m 대상 감지 확인
- 6m/s 수평 바깥 방향 외력 확인
- 대상별 1초 재발동 제한 확인
- 여러 대상 독립 처리 구조 확인
- 기존 `TryApplyExternalVelocityChange()` 재사용 확인
- 기존 ItemDefinition `puffer_balloon_suit / 5초` 확인
- `ProjectJPufferBalloonSuitPolicyTests` 15개 테스트 사례 확인

GitHub에 등록된 CI Status가 없으므로 Unity Editor 실제 컴파일과 EditMode Test Runner 통과 여부는 GitHub만으로 확정하지 않았다.

## 테스트맵 Pickup 배치 보류

이번 일차에도 `Day49_AllSystemsTest`에 복어 풍선옷 Pickup을 별도로 배치하지 않았다.

Fusion Scene NetworkObject SortKey/Bake 문제를 줄이기 위해 신규 아이템 Pickup 배치는 현재 아이템 구현 페이즈 종료 후 한 번에 통합한다.

따라서 115일차 구현 범위는 다음까지다.

```text
복어 풍선옷 네트워크 등록
→ 서버 권한 5초 상태
→ 반경 1.2m 자동 감지
→ 상대 바깥 방향 6m/s 밀치기
→ 대상별 1초 재발동 제한
→ Jelly·Respawn 보호 재사용
→ 효과 종료·Respawn 정리
→ 정책 테스트 작성
```

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJPufferBalloonSuitPolicyTests` 전체 통과 확인
3. 복어 풍선옷 사용 시 아이템이 정상 소비되는지 확인
4. 사용 직후 5초 효과가 시작되는지 확인
5. 1.2m 이내 상대가 자동으로 밀리는지 확인
6. 1.2m 밖 상대에게 적용되지 않는지 확인
7. 상대가 사용자 바깥 방향으로 약 6m/s 밀리는지 확인
8. 같은 상대는 1초 동안 연속 발동하지 않는지 확인
9. 1초 뒤 같은 상대에게 다시 발동하는지 확인
10. 여러 상대가 동시에 범위에 들어오면 각각 독립적으로 밀리는지 확인
11. Jelly 보호막이 복어 풍선옷 외력을 차단하는지 확인
12. Respawn 보호 중 외력이 차단되는지 확인
13. 보호로 외력이 실패했을 때 대상별 쿨타임이 시작되지 않는지 확인
14. 사용자가 Respawn하면 복어 풍선옷 효과가 즉시 종료되는지 확인
15. 5초가 지나면 자동 밀치기가 완전히 중단되는지 확인
16. Host와 Client에서 효과 결과가 일관되게 보이는지 확인
17. Pickup은 아이템 구현 페이즈 후반 통합 작업에서 별도로 배치
