# 113일차 : 망치 서버 권한 밀치기 강화 및 기존 Push 연동

## 개발 목표

- 기존 `Item_Hammer.asset`을 네트워크 인벤토리에서 사용할 수 있도록 연결한다.
- 망치 사용 시 서버 권한으로 6초 동안 밀치기 강화 상태를 유지한다.
- 새로운 밀치기 시스템을 별도로 만들지 않고 기존 `ProjectJNetworkExternalGameplay`의 Push 흐름을 그대로 재사용한다.
- 망치 활성 중에만 Push 사거리, 외력, 재사용 시간을 전용 정책값으로 교체한다.
- 기존 젤리 보호막, 부활 보호, 최근접 Target 선정과 외부 속도 처리 규칙을 그대로 유지한다.
- Respawn 또는 상태 초기화 시 망치 효과를 즉시 제거한다.
- 망치 수치와 활성 정책을 EditMode 테스트로 검증할 수 있도록 분리한다.

## 구현 내용

### 망치 네트워크 아이템 등록

- `ProjectJNetworkItemCatalog`에 `Hammer = 12`를 추가했다.
- 문자열 ID `hammer`를 네트워크 아이템 ID와 연결했다.
- 표시 이름을 `망치`로 등록했다.
- 기존 아이템 ID 0~11의 순서를 변경하지 않고 다음 번호를 사용했다.

### 서버 권한 강화 상태

- `ProjectJNetworkItemInventory.Hammer.cs`를 추가했다.
- 망치 지속 상태를 Fusion `TickTimer` 기반 Networked 값으로 관리한다.
- State Authority에서 망치를 사용할 때 6초 타이머를 생성한다.
- `IsHammerActive`로 현재 강화 상태를 조회할 수 있다.
- `HammerRemaining`으로 남은 강화 시간을 조회할 수 있다.
- 이미 망치가 활성화된 상태에서는 중복 활성화를 차단한다.
- 인벤토리 Spawn 시 망치 상태를 초기화한다.
- 인벤토리 전체 초기화와 Respawn 시 망치 효과를 즉시 제거한다.

### 기존 아이템 사용 경로 연결

- 기존 선택 아이템 사용 분기에 `Hammer`를 추가했다.
- 선택 아이템이 망치이면 `UseHammerAuthority()`를 호출한다.
- 사용에 성공하면 기존 아이템 소비, Revision 증가, 마지막 사용 아이템 기록 흐름을 그대로 사용한다.
- 망치 전용으로 별도의 입력 시스템이나 소비 시스템을 만들지 않았다.

### 기존 Push 시스템 연동

- `ProjectJNetworkExternalGameplay`을 `partial` 클래스로 변경해 망치 전용 확장 파일과 함께 사용할 수 있도록 했다.
- `ProjectJNetworkExternalGameplay.Hammer.cs`에서 현재 망치 활성 여부를 확인한다.
- 기존 `ProcessPush()`와 `FindClosestPushTarget()`의 구조를 유지한 채 필요한 수치만 망치 정책값으로 교체한다.
- 망치 비활성 상태에서는 기존 Push 수치를 그대로 사용한다.
- 망치 활성 상태에서는 `CurrentPushSearchRange`, `CurrentPushForce`, `CurrentPushCooldownSeconds`를 사용한다.

### 망치 활성 중 Push 수치

망치 정책값:

- 지속 시간: 6초
- Push 사거리: 3.2m
- Push 외부 속도: 11m/s
- Push 재사용 시간: 1.4초

기존 Push 기준값:

- Push 사거리: 2.5m
- Push 외부 속도: 12m/s
- Push 재사용 시간: 1.5초

현재 데이터 기준으로 망치는 사거리와 재사용 시간이 강화되며, 외부 속도 값은 기존 Push 12m/s보다 낮은 11m/s를 사용한다.
기획 데이터의 값을 임의로 변경하지 않고 그대로 적용했다.

### 기존 보호 규칙 유지

- 최근접 Player Target 선정 규칙을 그대로 사용한다.
- 기존 전방 각도 제한을 그대로 사용한다.
- 젤리 보호막이 Push를 차단하는 기존 규칙을 그대로 사용한다.
- 부활 보호 상태의 Player에게 Push 외력이 적용되지 않는 기존 규칙을 그대로 사용한다.
- 외력 적용은 기존 `TryApplyExternalVelocityChange()` 흐름을 그대로 사용한다.
- 망치를 위해 별도의 외력 시스템을 만들지 않았다.

### Respawn과 초기화

- `ClearAuthority()`에서 망치 타이머를 제거한다.
- `HandleRespawnAuthority()`에서 망치 타이머를 즉시 제거한다.
- 따라서 망치 사용 중 낙하 또는 수동 Respawn이 발생하면 강화 상태가 이어지지 않는다.
- 경기 시작 전 전체 인벤토리 초기화 흐름에서도 망치 효과가 남지 않는다.

## 컴파일 오류 수정

초기 113일차 적용 과정에서 다음 컴파일 오류가 발생했다.

```text
CS0260: Missing partial modifier on declaration of type
'ProjectJNetworkExternalGameplay';
another partial declaration of this type exists
```

원인은 새로 추가한 `ProjectJNetworkExternalGameplay.Hammer.cs`가 `partial class`로 선언되어 있었지만 기존 `ProjectJNetworkExternalGameplay.cs`는 일반 `class` 선언을 유지하고 있었기 때문이다.

기존 클래스 선언을 다음과 같이 변경했다.

```csharp
public sealed partial class ProjectJNetworkExternalGameplay :
```

이후 망치용 partial 파일과 기존 ExternalGameplay가 동일 클래스로 결합될 수 있도록 구조를 맞췄다.

## 테스트 추가

`ProjectJHammerPolicyTests`에 총 15개 테스트 사례를 구성했다.

주요 검증 항목:

- 지속 시간 6초 확인
- 망치 Push 사거리 3.2m 확인
- 망치 Push 외부 속도 11m/s 확인
- 망치 Push 재사용 시간 1.4초 확인
- 비활성 상태에서 망치 사용 허용
- 이미 활성 상태에서 중복 활성화 차단
- 비활성 상태에서 기존 Push 사거리 유지
- 활성 상태에서 망치 Push 사거리 적용
- 잘못된 음수 기본 사거리 보정
- 비활성 상태에서 기존 Push 외력 유지
- 활성 상태에서 망치 외력 적용
- 잘못된 음수 기본 외력 보정
- 비활성 상태에서 기존 Push 재사용 시간 유지
- 활성 상태에서 망치 Push 재사용 시간 적용
- 잘못된 음수 기본 재사용 시간 보정

## 변경 파일

113일차 최신 커밋 기준:

- 기능 구현 수정: 3개
- 기능 구현 생성: 8개
- 임시 적용 스크립트 생성: 2개
- 삭제: 없음
- 합계: 13개

주요 기능 변경 경로:

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkExternalGameplay.cs
├─ ProjectJNetworkExternalGameplay.Hammer.cs
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.cs
└─ ProjectJNetworkItemInventory.Hammer.cs

Assets/ProjectJ/Runtime/Items/
└─ ProjectJHammerPolicy.cs

Assets/ProjectJ/Tests/EditMode/
└─ ProjectJHammerPolicyTests.cs
```

신규 C# Script 4개에는 각각 `.meta` 파일이 함께 추가됐다.

현재 저장소 루트에는 113일차 적용 과정에서 사용한 다음 임시 파일도 포함되어 있다.

```text
Apply_Day113_Fix.cmd
Apply_Day113_Fix.ps1
```

이 두 파일은 Unity 런타임 기능에는 필요하지 않으며 최종 저장소 정리 시 삭제 대상이다.

## 검증 결과

- 113일차 확인 커밋: `a93b54d176ee14e48f9af49e5d0402a76547babf`
- 112일차 이후 망치 관련 기능 파일과 기존 시스템 수정이 실제 Git 변경사항으로 반영된 것을 확인했다.
- `ProjectJNetworkExternalGameplay`이 `partial` 선언으로 변경된 것을 확인했다.
- Push 사거리 계산이 `CurrentPushSearchRange`를 사용하도록 연결된 것을 확인했다.
- Push 외력 계산이 `CurrentPushForce`를 사용하도록 연결된 것을 확인했다.
- Push 재사용 시간이 `CurrentPushCooldownSeconds`를 사용하도록 연결된 것을 확인했다.
- 네트워크 카탈로그에 `Hammer = 12`, `hammer`, `망치` 변환 경로가 추가된 것을 확인했다.
- 망치 Networked `TickTimer`와 6초 지속 시간이 구현된 것을 확인했다.
- 인벤토리 Spawn, 전체 초기화, Respawn 경로에 망치 상태 초기화가 연결된 것을 확인했다.
- 선택 아이템 사용 분기에 망치 서버 권한 활성화가 연결된 것을 확인했다.
- `ProjectJHammerPolicyTests`에 총 15개 테스트 사례가 구성되어 있다.
- GitHub에 자동 CI Status가 등록되어 있지 않아 Unity Editor 실제 컴파일, EditMode Test Runner, Host·Client 플레이 결과는 독립적으로 검증하지 못했다.
- 저장소 루트의 `Apply_Day113_Fix.cmd`, `Apply_Day113_Fix.ps1`은 임시 적용 파일로 최종 정리 대상이다.

## 테스트맵 Pickup 배치 보류

- 이번 일차에는 `Day49_AllSystemsTest`에 망치 Pickup을 새로 배치하지 않았다.
- 신규 아이템별 Scene `NetworkObject`를 매 일차 추가하면서 발생할 수 있는 Fusion SortKey/Bake 문제를 피하기 위한 기존 방침을 유지한다.
- 신규 아이템 Pickup은 현재 아이템 구현 페이즈 후반에 여러 아이템을 한 번에 통합 배치한다.
- 따라서 113일차 완료 범위는 네트워크 아이템 등록, 서버 권한 강화 상태, 기존 Push 연동, Respawn 해제와 정책 테스트까지다.

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJHammerPolicyTests` 15개 테스트 사례 통과 확인
3. 테스트용 인벤토리에 망치를 넣고 사용 성공 확인
4. 망치 사용 직후 강화 상태가 약 6초 동안 유지되는지 확인
5. 망치 비활성 상태에서 기존 Push 사거리 2.5m가 유지되는지 확인
6. 망치 활성 상태에서 Push 사거리가 3.2m로 적용되는지 확인
7. 망치 활성 상태에서 Push 외부 속도 11m/s가 적용되는지 확인
8. 망치 활성 상태에서 Push 재사용 시간이 1.4초로 적용되는지 확인
9. 6초 종료 후 기존 Push 수치로 자동 복귀하는지 확인
10. 망치 활성 중 Respawn하면 효과가 즉시 제거되는지 확인
11. 젤리 보호막이 망치 강화 Push도 기존과 동일하게 차단하는지 확인
12. 부활 보호 상태의 Player가 망치 강화 Push에 영향을 받지 않는지 확인
13. 여러 Player가 Push 범위에 있을 때 기존 최근접 Target 선정 규칙이 유지되는지 확인
14. Host와 Client에서 망치 활성 상태가 일관되게 동작하는지 확인
15. Day49 망치 Pickup 배치는 아이템 구현 페이즈 후반 통합 작업에서 별도로 진행
