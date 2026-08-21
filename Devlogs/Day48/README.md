# Project J - 48일차 개발 일지

## 개발 목표

절차 생성 시스템을 구현하기 전에 기존 Day25 Module Prefab을 이용해 분기와 합류가 포함된 고정 Greybox 코스를 구성하고, 실제 Module Socket 규칙으로 연결 상태를 검증할 수 있는 기반을 만든다.

---

## 주요 개발 내용

### 1. Branch·Merge 고정 Greybox 코스 구성

- 기존 Day25 Module Prefab을 재사용해 48일차 전용 고정 코스를 구성했다.
- 시작 구간에서 `Branch` Module을 통해 두 경로로 갈라지도록 배치했다.
- 짧은 기본 경로인 `Path_A_Short`를 구성했다.
- 여러 Straight·Corner Module을 사용하는 우회 경로 `Path_B_Detour`를 구성했다.
- 두 경로가 동일한 `Merge` Module에서 다시 합류하도록 배치했다.
- Merge 이후 Finish 직선 구간까지 연결했다.

기본 진행 구조는 다음과 같다.

```text
                     ┌── Path A ──┐
                     │            │
Start ── Branch ─────┘          Merge ── Finish
          │                       ↑
          │                       │
          └──── Path B Detour ────┘
```

### 2. 기존 Module Prefab 재사용

이번 Greybox에서는 새 Module 규격을 만들지 않고 기존 Module을 그대로 사용했다.

- `PJ_Module_Straight_SouthNorth`
- `PJ_Module_Corner_SouthEast`
- `PJ_Module_Branch_SouthNorthEast`
- `PJ_Module_Merge_SouthWestNorth`

Corner Module은 필요한 진행 방향에 맞게 회전시켜 여러 방향의 코너로 재사용했다.

### 3. 20m Grid 기반 고정 배치

- 기존 `MapModule.DefaultModuleSize`인 20m 규격에 맞춰 Module을 배치했다.
- 각 Module 중심을 20m 단위 Grid에 정렬했다.
- Module 회전은 0°, 90°, 180°, 270° 단위로 제한했다.
- 기존 테스트 공간과 겹치지 않도록 별도의 위치에 48일차 코스를 배치했다.

### 4. Module Socket 연결 검증 구현

`MapModuleConnectionValidator`를 추가했다.

두 Module이 연결될 때 다음 항목을 검사한다.

- 출발 Socket 존재 여부
- 도착 Socket 존재 여부
- 출발 Socket이 `Exit` 상태인지 확인
- 도착 Socket이 `Entrance` 상태인지 확인
- 두 Socket의 월드 위치가 허용 오차 안에서 일치하는지 확인
- 두 Socket이 서로 반대 방향을 바라보는지 확인

연결 실패 원인은 다음과 같이 구분한다.

```text
None
MissingSocket
InvalidStateOrder
PositionMismatch
FacingMismatch
```

### 5. Greybox 전체 연결 자동 검증

48일차 Editor Setup에서 고정 코스를 생성한 뒤 각 구간의 연결을 자동으로 검사하도록 구성했다.

검증 대상에는 다음 연결이 포함된다.

- Start → Branch
- Branch → Path A
- Path A → Merge
- Branch → Path B
- Path B 내부 각 Module
- Path B → Merge
- Merge → Finish

모든 지정 Socket이 정상일 경우 완료 로그를 출력하도록 구성했다.

### 6. 48일차 전용 Scene 생성

다음 Scene을 추가했다.

```text
Assets/ProjectJ/Tests/Manual/Phase4/
└─ Phase4_BranchMergeGreybox.unity
```

기존 `Phase4_InteractionTest` Scene을 기준으로 복사해 기존 Phase 4 테스트 환경을 유지하면서 별도의 Branch·Merge 검증 공간을 구성했다.

### 7. 자동 Setup 도구 구현

다음 Editor 메뉴를 추가했다.

```text
ProjectJ
└─ Day48
   └─ Setup Branch Merge Greybox
```

실행 시 다음 과정을 자동으로 처리한다.

- 48일차 Scene 준비
- 기존 자동 생성 Greybox 제거
- Branch·Merge 코스 생성
- Path A 구성
- Path B 우회 코스 구성
- Start·Goal Marker 생성
- 전체 Socket 연결 검증
- 플레이어 시작 위치 배치
- Scene 저장

### 8. EditMode 테스트 추가

`MapModuleConnectionValidatorTests`를 추가했다.

다음 규칙을 검사하도록 구성했다.

- 정렬된 `Exit → Entrance` 연결 허용
- 잘못된 Socket 상태 순서 거부
- Socket 위치가 어긋난 연결 거부
- Socket 방향이 동일한 잘못된 연결 거부

---

## 추가된 주요 파일

```text
Assets/ProjectJ/Runtime/Map/
└─ MapModuleConnectionValidator.cs

Assets/ProjectJ/Editor/
└─ Day48BranchMergeGreyboxSetup.cs

Assets/ProjectJ/Tests/EditMode/
└─ MapModuleConnectionValidatorTests.cs

Assets/ProjectJ/Tests/Manual/Phase4/
└─ Phase4_BranchMergeGreybox.unity
```

---

## 구현 결과

- 절차 생성 없이 고정된 Branch·Merge 코스를 구성할 수 있게 되었다.
- 하나의 Branch에서 두 경로로 분리된 뒤 동일한 Merge로 다시 합류하는 구조를 실제 Module 단위로 확인할 수 있게 되었다.
- 기존 Straight·Corner Module의 회전 재사용 가능성을 확인할 수 있는 Greybox 기반을 마련했다.
- Module 연결을 눈대중이 아니라 실제 Socket 위치·상태·방향으로 검사할 수 있게 되었다.
- 이후 절차 생성 단계에서 동일한 Socket 연결 규칙을 재사용할 수 있는 검증 기반을 추가했다.
- 기존 46일차 Safe Volume·No Spawn 구조는 Module Prefab 내부 데이터로 그대로 유지된다.

---

## 현재 확인 상태

최신 48일차 GitHub 커밋 기준으로 다음 요소가 정상적으로 포함되어 있다.

- Branch·Merge Greybox Editor Setup
- Module Socket 연결 Validator
- EditMode 연결 규칙 테스트
- 생성된 `Phase4_BranchMergeGreybox` Scene

첨부된 Unity Scene 화면에서도 여러 Module이 고정 코스로 연결되고 분기·우회·합류 형태가 실제 공간으로 생성된 것을 확인했다.

저장소 코드와 Scene 구조를 기준으로 개발 진행을 막는 명확한 문제는 확인되지 않았다.

GitHub 저장소에는 해당 커밋의 CI 상태가 등록되어 있지 않기 때문에 Unity Editor의 실제 컴파일 결과, EditMode Test Runner 성공 여부, Path A와 Path B의 실제 완주 여부는 로컬 Unity 환경에서 최종 확인한다.

---

## 다음 개발 방향

49일차에는 Phase 4에서 구현한 기능을 하나의 통합 환경에서 다시 검증하는 회귀 Gate를 진행한다.

주요 확인 대상은 다음과 같다.

- 플레이어 이동·점프·달리기·앉기
- 플레이어 통과 및 밀치기
- External Force 합산
- 이동·회전·스프링·빙판·유령·에어백 장애물
- 장애물 Safe Volume·No Spawn
- 공통 F 상호작용
- Branch·Merge 고정맵
- Finish까지의 전체 진행

Phase 4의 각 기능이 개별 테스트에서는 정상이어도 서로 함께 존재할 때 문제가 발생하지 않는지 확인하는 것이 49일차의 핵심 목표다.
