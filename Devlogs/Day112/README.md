# 112일차 : 제트팩 서버 권한 상승·수평 조정 및 연료 동기화

## 개발 목표

- 기존 `Item_Jetpack.asset`을 네트워크 인벤토리에서 사용할 수 있도록 연결한다.
- 제트팩 사용 시 서버 권한으로 최대 5초 동안 연료 상태를 유지한다.
- 제트팩 활성 중 상승하면서 기존 WASD 수평 이동을 그대로 사용할 수 있도록 한다.
- 천장과 충돌하는 상황에서 계속 위로 밀고 올라가지 않도록 상승을 차단한다.
- 연료가 끝나면 별도의 강제 상태를 남기지 않고 기존 중력 규칙으로 자연스럽게 낙하하도록 한다.
- 부활, 완주, 경기 입력 잠금과 기존 Fusion Prediction 이동 구조를 유지한다.
- 제트팩 이동 정책을 EditMode 테스트로 검증할 수 있도록 분리한다.

## 구현 내용

### 제트팩 네트워크 아이템 등록

- `ProjectJNetworkItemCatalog`에 `Jetpack = 11`을 추가했다.
- 문자열 ID `jetpack`을 네트워크 아이템 ID와 연결했다.
- 표시 이름을 `제트팩`으로 등록했다.
- 기존 아이템 ID 0~10의 순서를 변경하지 않고 다음 번호를 사용했다.

### 서버 권한 연료 상태

- `ProjectJNetworkItemInventory.Jetpack.cs`를 추가했다.
- 제트팩 지속 상태를 Fusion `TickTimer` 기반 Networked 값으로 관리한다.
- State Authority에서 아이템을 사용할 때 5초 타이머를 생성한다.
- `IsJetpackActive`로 현재 활성 상태를 조회할 수 있다.
- `JetpackRemaining`으로 Host·Client에서 남은 연료 시간을 확인할 수 있다.
- 인벤토리 초기화 시 제트팩 타이머를 초기화한다.
- 인벤토리 전체 초기화 시 제트팩 효과를 제거한다.
- Respawn 처리 시 남아 있던 제트팩 효과를 즉시 제거한다.

### 아이템 사용 경로 연결

- 기존 선택 아이템 사용 분기에 제트팩을 추가했다.
- 선택 아이템이 제트팩이면 `UseJetpackAuthority()`를 호출한다.
- 제트팩 사용 성공 여부는 기존 아이템 사용 성공/실패 처리 흐름을 그대로 사용한다.
- 별도의 클라이언트 전용 연료 상태를 만들지 않고 Networked 상태를 공통 기준으로 사용한다.

### 제트팩 상승 이동

- `ProjectJNetworkPlayer`가 Networked 제트팩 활성 상태를 조회하도록 연결했다.
- 기존 걷기, 달리기, 깃털 신발, 눈덩이 감속 계산 이후 제트팩 수평 이동 정책을 적용한다.
- 제트팩의 프로토타입 수평 배율은 `1.0`으로 설정해 기존 WASD 이동 속도를 변경하지 않는다.
- 프로토타입 최소 상승 속도는 `4m/s`로 설정했다.
- 기존 점프 등으로 이미 더 높은 상승 속도를 가지고 있다면 그 값을 강제로 낮추지 않는다.
- 제트팩 연료가 활성 상태인 동안 현재 수직 속도와 최소 상승 속도를 비교해 상승을 유지한다.
- 상승 중에는 Grounded 상태가 잘못 유지되지 않도록 공중 상태로 갱신한다.

### 천장 충돌 처리

- Player 캡슐 상단을 기준으로 위쪽 `SphereCastNonAlloc` 검사를 추가했다.
- 이번 Tick의 예상 상승 거리와 0.05m의 검사 여유값을 이용해 바로 위의 충돌체를 확인한다.
- Trigger와 자신의 Collider는 천장 판정에서 제외한다.
- 외부 Collider가 위쪽에 감지되면 양수 방향 수직 속도를 제거한다.
- 이미 하강 중인 경우에는 하강 속도를 유지한다.
- 이를 통해 낮은 천장에서 제트팩이 지속적으로 위쪽으로 밀어붙이는 상황을 차단한다.

### 연료 종료와 Gameplay Lock

- 제트팩 연료 종료 후에는 제트팩 정책이 수직 속도에 개입하지 않는다.
- 이후 이동은 기존 `Gravity = -20f` 규칙을 그대로 사용해 자연스럽게 낙하한다.
- 완주 또는 경기 입력 잠금 상태는 기존 `GameplayInputAllowed` 검사에서 이동 처리가 먼저 차단된다.
- 제트팩을 위해 별도의 두 번째 이동 시스템을 만들지 않고 기존 `ProjectJNetworkPlayer.FixedUpdateNetwork()` 흐름을 유지했다.
- 따라서 기존 Fusion Prediction/Resimulation 구조 안에서 제트팩 이동이 처리된다.

### 디버그 상태 표시

- 기존 Item Inventory OnGUI 진단 영역 높이를 제트팩 상태 표시 공간만큼 확장했다.
- `Jetpack : 5.0s`와 같이 남은 Networked 연료 시간을 표시한다.
- 비활성 상태에서는 `Jetpack : OFF`로 표시한다.

## 프로토타입 수치

이번 일차에서 확정된 기능 규칙과 프로토타입 조정값은 구분해서 사용한다.

확정 규칙:

- 최대 연료 지속 시간: 5초
- 연료 종료 시 제트팩 효과 즉시 종료
- WASD 수평 조작 가능
- 천장 상승 차단
- Respawn 시 효과 즉시 제거

프로토타입 조정값:

- 최소 상승 속도: 4m/s
- 수평 이동 배율: 1.0배
- 천장 검사 여유 거리: 0.05m

현재 Player의 기존 이동 기준값은 다음과 같다.

- 걷기: 5m/s
- 달리기: 8m/s
- 점프 초기 수직 속도: 7m/s
- 중력 가속도: -20m/s²

상승 속도와 수평 배율은 실제 플레이 테스트 후 밸런스 조정 대상이다.

## 테스트 추가

`ProjectJJetpackPolicyTests`에 총 19개 테스트 사례를 구성했다.

주요 검증 항목:

- 연료 지속 시간 5초 확인
- 프로토타입 상승 속도 4m/s 확인
- 수평 이동 배율 1.0 확인
- 천장 검사 여유 거리 0.05m 확인
- 활성 상태와 Gameplay Lock 조합에 따른 이동 허용 여부
- 걷기·달리기 상태에서 기존 수평 속도 유지
- 잘못된 음수 이동 속도 보정
- 추락 중 제트팩 활성 시 상승 전환
- 낮은 상승 속도를 최소 4m/s로 보정
- 기존 점프 상승 속도 7m/s 계열 보존
- 천장 접촉 시 위쪽 속도 제거
- 천장 근처에서 기존 하강 속도 보존
- 연료 종료 후 기존 중력 기반 수직 상태 유지
- Gameplay Lock 상태에서 제트팩 이동 정책 개입 차단

## 변경 파일

112일차 기능 구현 기준:

- 수정: 3개
- 생성: 6개
- 삭제: 없음
- 합계: 9개

주요 변경 경로:

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.cs
├─ ProjectJNetworkItemInventory.Jetpack.cs
└─ ProjectJNetworkPlayer.cs

Assets/ProjectJ/Runtime/Items/
└─ ProjectJJetpackPolicy.cs

Assets/ProjectJ/Tests/EditMode/
└─ ProjectJJetpackPolicyTests.cs
```

신규 Script 3개에는 각각 `.meta` 파일이 함께 추가됐다.

## 검증 결과

- 112일차 확인 커밋: `c53879aaf41f03011b9cde7e129e5f8f29dec512`
- 111일차 커밋 이후 제트팩 관련 변경 파일 9개가 실제 Git 변경사항으로 반영된 것을 확인했다.
- 네트워크 카탈로그에 `Jetpack = 11`, `jetpack`, `제트팩` 변환 경로가 추가된 것을 확인했다.
- 제트팩 Networked `TickTimer`가 생성되고 초기화·전체 초기화·Respawn 경로에 연결된 것을 확인했다.
- 선택 아이템 사용 분기에 제트팩 서버 권한 활성화가 연결된 것을 확인했다.
- Player 이동 코드에 제트팩 활성 상태, 수평 이동 정책, 상승 처리와 천장 검사가 연결된 것을 확인했다.
- 연료 종료 후 기존 중력 흐름을 사용하는 구조를 확인했다.
- `ProjectJJetpackPolicyTests`에 총 19개 테스트 사례가 구성되어 있다.
- 이전에 임시로 사용했던 `Day112_Jetpack.patch` 파일은 현재 최신 커밋에 포함되어 있지 않다.
- GitHub에 자동 CI Status가 등록되어 있지 않아 Unity Editor 실제 컴파일, EditMode Test Runner, Host·Client 플레이 결과는 독립적으로 검증하지 못했다.

## 테스트맵 Pickup 배치 보류

- 이번 일차에는 `Day49_AllSystemsTest`에 제트팩 Pickup을 새로 배치하지 않았다.
- 신규 아이템별 Scene `NetworkObject`를 매 일차 추가하면서 발생할 수 있는 Fusion SortKey/Bake 문제를 피하기 위한 기존 방침을 유지한다.
- 신규 아이템 Pickup은 현재 아이템 구현 페이즈 후반에 여러 아이템을 한 번에 통합 배치한다.
- 따라서 112일차 완료 범위는 네트워크 아이템 등록, 서버 권한 연료 상태, 상승·수평 이동, 천장 차단과 정책 테스트까지다.

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJJetpackPolicyTests` 19개 테스트 사례 통과 확인
3. 테스트용 인벤토리에 제트팩을 넣고 우클릭 사용 성공 확인
4. 사용 직후 OnGUI의 `Jetpack` 남은 시간이 약 5초부터 감소하는지 확인
5. 제트팩 활성 중 Player가 위쪽으로 지속 상승하는지 확인
6. 상승 중 WASD 수평 이동이 기존 이동 감각을 유지하는지 확인
7. 점프 직후 제트팩을 사용해도 기존보다 빠른 상승 속도가 갑자기 낮아지지 않는지 확인
8. 낮은 천장에서 제트팩을 사용했을 때 천장을 뚫거나 계속 위로 밀지 않는지 확인
9. 5초 연료 종료 직후 기존 중력으로 자연스럽게 낙하하는지 확인
10. 제트팩 활성 중 Respawn하면 효과가 즉시 제거되는지 확인
11. 완주 또는 Gameplay Lock 상태에서 제트팩 이동이 적용되지 않는지 확인
12. Host와 Client에서 활성 상태와 남은 시간이 동일하게 표시되는지 확인
13. 네트워크 지연 환경에서도 Player Prediction/Resimulation 이동이 기존처럼 동작하는지 확인
14. Day49 제트팩 Pickup 배치는 아이템 구현 페이즈 후반 통합 작업에서 별도로 진행
