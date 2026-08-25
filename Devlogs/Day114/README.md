# 114일차 : 폭탄 서버 권한 포물선 투척 및 거리 감쇠 범위 폭발

## 개발 목표

- 폭탄을 네트워크 아이템으로 등록한다.
- 서버 권한으로 폭탄을 생성하고 포물선 형태로 투척한다.
- 2.5초 신관이 끝나면 서버에서 범위 폭발을 처리한다.
- 폭발 중심과 대상의 거리에 따라 외력을 10m/s에서 4m/s까지 선형 감쇠한다.
- 기존 Jelly 보호막, Respawn 보호와 외부 속도 처리 구조를 재사용한다.
- 사용자당 활성 폭탄을 최대 1개로 제한한다.
- 폭탄 투척·폭발 수치를 정책 클래스로 분리하고 EditMode 테스트를 추가한다.

## 구현 내용

### 폭탄 네트워크 아이템 등록

`ProjectJNetworkItemCatalog`에 폭탄을 등록했다.

- Network ID: `Bomb = 13`
- Key: `bomb`
- 표시 이름: `폭탄`

기존 아이템 ID 0~12의 순서는 변경하지 않고 다음 번호를 사용했다.

### 서버 권한 폭탄 투척

`ProjectJNetworkItemInventory.Bomb.cs`를 추가했다.

폭탄 사용 시 다음 조건을 서버에서 확인한다.

- NetworkRunner가 존재하는가
- Server에서 실행 중인가
- 사용자의 NetworkObject가 유효한가
- State Authority가 존재하는가
- Gameplay 입력이 허용된 상태인가
- 같은 사용자가 이미 활성 폭탄을 가지고 있지 않은가

조건을 통과하면 `Resources/ProjectJNetworkBomb.prefab`을 불러와 `Runner.Spawn()`으로 생성한다.

폭탄이 정상 생성되고 `ConfigureAuthority()`까지 성공한 경우에만 기존 인벤토리 소비 흐름이 진행된다.

### 사용자당 활성 폭탄 1개 제한

현재 Scene의 `ProjectJNetworkBomb`을 검색해 동일 Input Authority가 소유한 활성 폭탄이 있는지 확인한다.

이미 폭발하지 않은 자신의 폭탄이 존재하면 새 폭탄 투척을 실패 처리하며 아이템은 소비하지 않는다.

### Network Bomb Prefab

다음 Network Prefab을 추가했다.

```text
Assets/ProjectJ/Network/Fusion/Player/Resources/
└─ ProjectJNetworkBomb.prefab
```

주요 Component:

- `NetworkObject`
- `NetworkTransform`
- `ProjectJNetworkBomb`

폭탄 위치는 State Authority에서 계산하며 `NetworkTransform`을 통해 동기화한다.

### 2.5초 서버 권한 신관

`ProjectJNetworkBomb`에 Fusion `TickTimer` 기반 신관을 구현했다.

- 신관 시간: 2.5초
- 신관 종료 시 서버에서 한 번만 폭발
- 폭발 상태를 Networked 값으로 기록해 중복 폭발 차단
- 경기 진행 상태가 종료되면 폭발하지 않고 NetworkObject 제거

### 포물선 투척

폭탄의 초기 이동 속도는 정책 클래스에서 계산한다.

현재 프로토타입 투척 수치:

- 수평 초기 속도: 8m/s
- 수직 초기 속도: 5m/s
- 중력: -12m/s²
- 충돌 검사 반경: 0.3m

폭탄은 서버 Tick마다 현재 속도에 중력을 적용해 포물선 이동한다.

최대 수평 투척 거리는 12m이며 이를 초과하지 않도록 XZ 위치와 수평 속도를 제한한다.

### 충돌 처리

폭탄은 이동 중 `SphereCastNonAlloc()`로 충돌을 검사한다.

- 투척자 자신의 Collider는 제외
- 최초 유효 충돌 위치에서 이동 정지
- Player 또는 World에 충돌해도 즉시 폭발하지 않음
- 남은 신관 시간이 끝날 때까지 충돌 위치에서 대기
- 신관 종료 후 범위 폭발 실행

### 거리 감쇠 범위 폭발

폭발 반경은 5m다.

폭발 중심과 Player 사이의 3차원 거리를 기준으로 외력을 계산한다.

| 폭발 거리 | 적용 외력 |
| --- | ---: |
| 0m | 10m/s |
| 1.25m | 8.5m/s |
| 2.5m | 7m/s |
| 3.75m | 5.5m/s |
| 5m | 4m/s |
| 5m 초과 | 0m/s |

중심 10m/s에서 가장자리 4m/s까지 선형으로 감소한다.

### 기존 외력 시스템 재사용

폭발 외력은 새 이동 시스템을 만들지 않고 기존 다음 경로를 사용한다.

```text
ProjectJNetworkBomb
→ TryApplyExternalVelocityChange3D()
→ ProjectJExternalForceSource.Item
→ 기존 외력 보호 및 이동 처리
```

따라서 기존 적대 아이템 외력 처리 구조와 동일한 흐름을 사용한다.

현재 폭탄을 던진 사용자는 자신의 폭발 대상에서 제외한다.

### 경기 종료 처리

폭탄이 활성 상태일 때 Owner의 Gameplay 입력 상태를 확인한다.

Owner가 경기 진행 상태가 아니거나 유효한 Owner를 찾을 수 없는 경우 폭탄을 제거한다.

경기 종료 뒤 남은 폭탄이 나중에 폭발하는 상황을 방지한다.

## 폭탄 정책 분리

`ProjectJBombPolicy.cs`를 추가했다.

주요 기획 수치:

- 신관: 2.5초
- 최대 투척 거리: 12m
- 폭발 반경: 5m
- 중심 외력: 10m/s
- 가장자리 외력: 4m/s

프로토타입 수치:

- 수평 투척 속도: 8m/s
- 초기 상승 속도: 5m/s
- 중력: -12m/s²
- 충돌 반경: 0.3m

정책 클래스에서 다음 계산을 담당한다.

- 폭탄 사용 가능 여부
- 폭발 반경 포함 여부
- 거리별 폭발 외력
- 초기 포물선 속도
- 수평 이동 거리
- 폭발 방향과 최종 외부 속도

## 테스트 추가

`ProjectJBombPolicyTests`를 추가했다.

총 27개 테스트 사례를 구성했다.

주요 검증 항목:

- 신관 2.5초
- 최대 투척 거리 12m
- 폭발 반경 5m
- 중심 외력 10m/s
- 가장자리 외력 4m/s
- 정상 권한 상태의 폭탄 사용 허용
- Runner 준비 실패 시 사용 차단
- Gameplay 잠금 상태에서 사용 차단
- 활성 폭탄 존재 시 중복 사용 차단
- 폭발 반경 경계값
- 음수 거리 보정
- 중심·중간·가장자리 거리 감쇠
- 폭발 반경 밖 외력 0
- 포물선 초기 속도
- 잘못된 전방 방향 보정
- 높이를 제외한 수평 투척 거리 계산
- 중심 폭발 대체 방향
- 가장자리 폭발 외력
- 폭발 반경 밖 Vector3.zero 반환

## 변경 파일

114일차 기능 변경 기준:

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkBomb.cs
├─ ProjectJNetworkBomb.cs.meta
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.cs
├─ ProjectJNetworkItemInventory.Bomb.cs
├─ ProjectJNetworkItemInventory.Bomb.cs.meta
└─ Resources/
   ├─ ProjectJNetworkBomb.prefab
   └─ ProjectJNetworkBomb.prefab.meta

Assets/ProjectJ/Runtime/Items/
├─ ProjectJBombPolicy.cs
└─ ProjectJBombPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJBombPolicyTests.cs
└─ ProjectJBombPolicyTests.cs.meta
```

기존 113일차 임시 적용 파일도 저장소에서 제거했다.

```text
Apply_Day113_Fix.cmd
Apply_Day113_Fix.ps1
```

## 최신 커밋 검증

확인한 최신 `main` 커밋:

```text
8f005ebbf459c6cf6e8c03215a55a1036b08b1df
```

현재 커밋 메시지는 임시 제목 `a`다.

확인 내용:

- `Bomb = 13` 등록 확인
- `bomb` Key 변환 확인
- `폭탄` 표시 이름 확인
- 인벤토리 `Bomb` 사용 분기 연결 확인
- 서버 권한 `Runner.Spawn()` 폭탄 생성 구조 확인
- 사용자당 활성 폭탄 1개 제한 확인
- `ProjectJNetworkBomb` NetworkBehaviour 생성 확인
- 2.5초 Networked `TickTimer` 신관 확인
- 12m 최대 수평 투척 거리 확인
- 5m 폭발 반경 확인
- 중심 10m/s → 가장자리 4m/s 선형 감쇠 확인
- 기존 `TryApplyExternalVelocityChange3D()` 연결 확인
- 폭탄 Prefab의 Script GUID와 `ProjectJNetworkBomb.cs.meta` GUID 일치 확인
- `ProjectJBombPolicyTests` 27개 테스트 사례 구성 확인
- 113일차 임시 `.cmd/.ps1` 삭제 확인

GitHub에 등록된 CI Status가 없으므로 Unity Editor 실제 컴파일과 EditMode Test Runner 통과 여부는 GitHub만으로 확정하지 않았다.

## 테스트맵 Pickup 배치 보류

이번 일차에도 `Day49_AllSystemsTest`에 폭탄 Pickup을 새로 배치하지 않았다.

Fusion Scene NetworkObject SortKey/Bake 문제를 줄이기 위해 신규 아이템 Pickup은 현재 아이템 구현 페이즈 종료 후 한 번에 통합 배치한다.

따라서 114일차 범위는 다음까지다.

```text
폭탄 네트워크 등록
→ 서버 권한 투척
→ 포물선 이동
→ 2.5초 신관
→ 5m 범위 폭발
→ 10~4m/s 거리 감쇠
→ 기존 외력 시스템 연결
→ 사용자당 활성 폭탄 1개 제한
→ 정책 테스트 작성
```

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJBombPolicyTests` 전체 통과 확인
3. 폭탄 아이템 사용 시 서버에서 Network Bomb 생성 확인
4. 폭탄이 포물선으로 이동하는지 확인
5. 지형 충돌 시 즉시 폭발하지 않고 해당 위치에 멈추는지 확인
6. 사용 후 약 2.5초 뒤 폭발하는지 확인
7. 5m 밖 Player에게 외력이 적용되지 않는지 확인
8. 폭발 중심에 가까울수록 강한 외력이 적용되는지 확인
9. 5m 경계에서 약 4m/s 외력이 적용되는지 확인
10. Jelly 보호막이 폭탄 외력을 기존 적대 아이템과 동일하게 처리하는지 확인
11. Respawn 보호 중 Player가 폭탄 외력으로부터 보호되는지 확인
12. 자신의 활성 폭탄이 남아 있을 때 두 번째 폭탄 사용이 실패하는지 확인
13. 첫 폭탄이 사라진 뒤 다시 폭탄을 사용할 수 있는지 확인
14. 경기 종료 시 남은 폭탄이 제거되는지 확인
15. Host와 Client에서 폭탄 위치와 폭발 결과가 일관되게 보이는지 확인
16. 폭탄 Pickup은 아이템 구현 페이즈 후반 통합 작업에서 별도로 배치
