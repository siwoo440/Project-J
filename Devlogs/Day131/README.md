# Project J 개발 일지 - 131일차

## 개발 주제

**손거울 서버 권한 4초 투사체 반사 및 반복 소유권 이전 구현**

이번 일차에서는 방어형 아이템 **손거울(Hand Mirror)** 을 실제 네트워크 전투 흐름에 연결했다.

손거울은 사용 후 4초 동안 활성화되며, 활성 상태에서 반사 가능한 적대 투사체에 맞으면 해당 공격을 그대로 받는 대신 진행 방향을 반대로 돌리고 공격 소유권을 손거울 사용자에게 이전한다.

반사된 공격이 다시 다른 손거울 사용자에게 닿을 경우 같은 규칙을 다시 적용할 수 있도록 구성하여, A → B → C 형태의 연속 반사에서도 현재 공격 소유권이 마지막 반사자에게 넘어가도록 했다.

---

## 1. 네트워크 아이템 등록

손거울의 네트워크 아이템 ID를 새로 등록했다.

- Network Item ID: `30`
- Key: `hand_mirror`
- 표시 이름: `손거울`
- 분류: 방어형
- 사용 방식: 즉시 사용
- 대상: 자신
- 지속 시간: `4초`
- 재사용 대기시간: `0초`

기존에 제작되어 있던 `Item_HandMirror.asset` 데이터를 그대로 사용했으며 별도의 ItemDefinition 자산은 새로 생성하지 않았다.

---

## 2. 서버 권한 손거울 상태 구현

`ProjectJNetworkItemInventory.HandMirror.cs`를 추가하여 손거울 상태를 Fusion Networked 상태로 관리하도록 구성했다.

주요 Networked 값:

- 손거울 활성 여부
- 손거울 지속 시간 TickTimer
- 손거울 상태 Revision

아이템 사용에 성공하면 State Authority에서 4초 타이머가 시작된다.

다음 상황에서는 손거울 상태를 제거한다.

- 4초 지속 시간이 종료된 경우
- 경기 입력이 더 이상 허용되지 않는 경우
- 플레이어가 부활한 경우
- 인벤토리 및 플레이어 상태가 전체 초기화된 경우

---

## 3. 반사 가능 여부 판정

공통 정책 `ProjectJHandMirrorPolicy`를 추가하여 반사 조건을 게임 로직과 분리했다.

반사는 다음 조건을 모두 만족할 때만 허용한다.

- State Authority가 정상적으로 존재
- 손거울이 현재 활성 상태
- 플레이어가 정상적인 경기 진행 상태
- 들어오는 공격의 현재 소유자가 손거울 사용자 자신이 아님
- 손거울 사용자가 되감기 상태가 아님

자신이 소유한 공격이 자기 손거울에 다시 닿아 즉시 재반사되는 상황은 차단했다.

---

## 4. 투사체 방향 반전

반사 시 기존 투사체의 진행 방향을 반대로 계산한다.

예를 들어 기존 진행 방향이 다음과 같다면:

`A → B`

B가 손거울로 반사한 이후:

`B → A`

형태로 방향을 전환한다.

3차원 진행 방향 전체를 반전시키며, 잘못된 0 벡터 또는 비정상 방향이 전달되는 경우에는 안전한 대체 방향을 사용한다.

---

## 5. 반사 직후 재충돌 방지

반사 순간 투사체가 손거울 사용자의 Collider 내부 또는 표면에 그대로 남아 있으면 다음 Physics Tick에서 같은 플레이어와 다시 충돌할 수 있다.

이를 방지하기 위해 반사 직후 충돌 지점에서 반사 방향으로 약간 떨어진 위치로 투사체를 이동시킨다.

- Reflection Separation: `0.35m`

이 값은 실제 공격 사거리나 위력에는 영향을 주지 않고 반사 직후 자기 Collider 재충돌을 방지하기 위한 안전 간격으로만 사용한다.

---

## 6. 공격 소유권 이전

손거울 반사의 핵심 규칙으로 공격의 `NetworkOwner`를 현재 손거울 사용자로 변경하도록 구현했다.

예시:

1. 플레이어 A가 공격 발사
2. 플레이어 B가 손거울로 반사
3. 공격 소유권 `A → B`
4. 플레이어 C가 같은 공격을 다시 반사
5. 공격 소유권 `B → C`

따라서 여러 플레이어가 연속으로 반사하더라도 최종 공격자는 마지막으로 반사한 플레이어가 된다.

---

## 7. 일반 직선 투사체 반사 연결

현재 손거울 반사 처리를 다음 기존 네트워크 투사체에 연결했다.

- 먹물 문어
- 눈덩이
- 비눗방울
- 풀 공

각 투사체는 플레이어 충돌 시 기존 효과를 바로 적용하기 전에 손거울 활성 여부를 먼저 확인한다.

손거울 반사에 성공하면:

1. 기존 공격 효과 적용을 취소
2. NetworkOwner를 손거울 사용자로 변경
3. 진행 방향을 반전
4. 반사 방향으로 안전 간격 이동
5. 투사체를 제거하지 않고 계속 이동

하도록 처리했다.

---

## 8. 유도탄 손거울 반사

유도탄은 일반 직선 투사체와 달리 별도의 추적 Target을 보유하기 때문에 추가 처리를 적용했다.

유도탄이 손거울 사용자에게 적중하려는 순간 반사가 성공하면:

- 유도탄의 NetworkOwner를 손거울 사용자로 이전
- 진행 방향을 반전
- 기존 Route 경로를 초기화
- 가능하면 반사 전 공격 소유자를 새로운 추적 대상으로 지정

하도록 구성했다.

반사 전 소유자가 정상적인 경기 상태이며 자동 추적 가능한 대상이면 해당 플레이어를 우선적으로 다시 추적한다.

반사 전 소유자를 추적할 수 없는 경우에는 기존 유도탄 재탐색 규칙을 사용한다.

투명 망토 등으로 자동 추적 대상에서 제외된 플레이어는 반사된 유도탄의 우선 추적 대상에서도 제외된다.

---

## 9. 반복 반사 지원

반사 이후 NetworkOwner 자체가 실제 반사 사용자로 변경되기 때문에 같은 투사체가 다른 손거울 사용자에게 도달했을 때 다시 반사할 수 있다.

별도의 "한 번만 반사 가능" 플래그를 두지 않았으며, 투사체가 살아 있는 동안 정상적인 손거울 조건을 만족하면 반복 반사가 가능하다.

---

## 10. 기존 인벤토리 흐름 연결

기존 `ProjectJNetworkItemInventory.cs`에 손거울을 연결했다.

추가된 주요 처리:

- Spawn 시 손거울 Networked 상태 초기화
- 매 Fusion Tick 손거울 시간 종료 확인
- 인벤토리 전체 초기화 시 손거울 제거
- 부활 시 손거울 제거
- 선택 아이템 사용 Switch에 HandMirror 분기 추가
- 정상 사용 성공 시 기존 아이템 소비 흐름 사용

따라서 손거울만 별도의 인벤토리 소비 규칙을 만들지 않고 기존 서버 권한 아이템 사용 구조를 그대로 따른다.

---

## 11. 정책 테스트 작성

`ProjectJHandMirrorPolicyTests.cs`를 추가하여 손거울 순수 정책 로직을 검증할 수 있도록 구성했다.

작성된 검증 항목:

- Network Item ID 30
- 지속 시간 4초
- 반사 안전 간격이 0보다 큰지 확인
- 권한·Runner·게임 진행 상태별 활성 가능 여부
- 손거울 활성 상태별 반사 가능 여부
- 자기 소유 공격 반사 차단
- 되감기 중 반사 차단
- 3차원 공격 방향 반전
- 잘못된 방향의 fallback 처리
- 반사 직후 안전 간격 위치 계산
- 유도탄의 이전 소유자 우선 추적 조건

소스 기준 NUnit Test/TestCase 선언은 총 **19개 케이스**다.

주의: 테스트 코드가 작성되어 있다는 것과 실제 Unity Test Runner에서 모든 테스트가 통과했다는 것은 별개다.

---

## 12. 변경된 프로젝트 파일

130일차 커밋과 비교했을 때 131일차 최신 커밋에는 총 13개 파일 변경이 포함되어 있다.

### 추가

- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkItemInventory.HandMirror.cs`
- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkItemInventory.HandMirror.cs.meta`
- `Assets/ProjectJ/Runtime/Items/ProjectJHandMirrorPolicy.cs`
- `Assets/ProjectJ/Runtime/Items/ProjectJHandMirrorPolicy.cs.meta`
- `Assets/ProjectJ/Tests/EditMode/ProjectJHandMirrorPolicyTests.cs`
- `Assets/ProjectJ/Tests/EditMode/ProjectJHandMirrorPolicyTests.cs.meta`

### 수정

- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkHomingMissile.cs`
- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkInkOctopusProjectile.cs`
- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkItemCatalog.cs`
- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkItemInventory.cs`
- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkPoolBallProjectile.cs`
- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkSnowballProjectile.cs`
- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkSoapBubbleProjectile.cs`

구현용 일회성 Editor Installer는 적용 후 자기 자신을 삭제하도록 구성했기 때문에 최종 커밋에는 포함되어 있지 않다.

---

## 13. Scene 및 Pickup 처리

이번 일차에서는 개별 Pickup 오브젝트를 Scene에 추가하지 않았다.

손거울 구현은 기존 `Item_HandMirror.asset`과 네트워크 인벤토리/투사체 코드에 연결했으며, Scene에 별도의 손거울 전용 NetworkObject나 Prefab을 새로 배치하지 않았다.

아이템 Pickup의 일괄 배치는 아이템 구현 단계가 끝난 뒤 통합 작업에서 처리한다.

---

## 14. 최신 커밋 확인

확인한 최신 `main` 커밋:

- SHA: `30381aa167248c3f6266ab19f1ef1a3fd121055b`
- 현재 커밋 메시지: `a`
- 이전 커밋: `c3ebf6a3d7a1700420b53fe4ddc21958929e3549`
- 이전 커밋 제목: `130일차 : 저격 물총 서버 권한 장거리 히트스캔 및 2배·4배 줌 조준 구현`
- 이전 커밋 대비: 1 commit ahead / 0 behind
- 변경 파일: 13개

GitHub의 최신 커밋에는 별도의 Commit Status 결과가 등록되어 있지 않았으며, 해당 커밋에 연결된 GitHub Actions Workflow Run도 확인되지 않았다.

---

## 15. 검토 상태

최신 커밋의 diff와 현재 손거울 관련 소스를 다시 확인했다.

정적 코드 검토 기준으로 이번 변경에서 새로 확인된 **차단 수준의 문제는 없었다.**

확인한 주요 내용:

- 손거울 ID 30 등록
- 기존 `Item_HandMirror.asset`의 `hand_mirror`, 4초 데이터 유지
- State Authority 기반 손거울 활성 상태
- 경기 종료·부활·전체 초기화 시 상태 제거
- 자기 소유 공격 재반사 차단
- 직선 투사체 방향 반전 및 NetworkOwner 이전
- 유도탄 반사 후 이전 소유자 우선 추적
- 반복 반사를 막는 1회성 제한이 없음
- 테스트 정책 코드 존재
- 일회성 Installer가 최종 커밋에 남아 있지 않음

다만 GitHub에는 이 커밋의 Unity 컴파일 결과나 Unity Test Runner 실행 결과가 연결되어 있지 않다.

따라서 이 개발 일지에서는 **Unity 전체 컴파일 성공 또는 19/19 테스트 통과를 확정하지 않는다.**

---

## 16. Unity에서 최종 확인할 항목

Unity에서 다음 항목을 직접 확인하면 131일차 작업을 최종 검증할 수 있다.

1. Console에 C# 컴파일 오류가 없는지 확인
2. EditMode에서 `ProjectJHandMirrorPolicyTests` 실행
3. 손거울 사용 후 약 4초간 활성 상태 유지 확인
4. 먹물 문어 반사 확인
5. 눈덩이 반사 확인
6. 비눗방울 반사 확인
7. 풀 공 반사 확인
8. 유도탄 반사 후 기존 공격자 방향으로 재추적하는지 확인
9. 두 명 이상의 손거울 사용자 사이에서 반복 반사 확인
10. 부활 또는 경기 종료 시 손거울 상태가 즉시 제거되는지 확인
