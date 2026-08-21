# Project J - 47일차 개발 일지

## 개발 목표

플레이어가 주변의 상호작용 가능한 오브젝트를 탐색하고, 가장 가까운 유효 대상을 선택하여 공통 입력으로 상호작용할 수 있는 기본 시스템을 구현한다.

---

## 주요 개발 내용

### 1. 공통 상호작용 인터페이스 구현

- `IInteractable` 인터페이스를 추가했다.
- 모든 상호작용 대상이 공통으로 사용할 수 있도록 다음 기능을 정의했다.
  - 상호작용 기준 위치 반환
  - 현재 상호작용 가능 여부 확인
  - 실제 상호작용 실행
- 공통 기반 클래스인 `InteractableBehaviour`를 추가했다.

### 2. 상호작용 Target 선택 규칙 구현

- 플레이어 주변의 상호작용 후보 중 가장 가까운 대상을 선택하도록 구현했다.
- 최대 상호작용 거리를 기준으로 범위 밖 대상을 제외한다.
- 비활성화된 대상과 현재 사용할 수 없는 대상을 제외한다.
- 같은 대상의 Collider가 여러 개 감지되어도 중복 등록되지 않도록 처리했다.
- 거리가 같은 경우에도 동일한 결과를 얻을 수 있도록 Instance ID를 이용한 동점 처리 규칙을 추가했다.

### 3. 플레이어 공통 상호작용 Controller 구현

- `PlayerInteractionController`를 추가했다.
- 플레이어의 `PlayerInput`에서 `Interact` Action을 찾아 입력 이벤트를 연결한다.
- 매 프레임 `Physics.OverlapSphereNonAlloc`을 이용해 주변 상호작용 대상을 탐색한다.
- 탐색 결과에서 가장 가까운 유효 Target을 현재 Target으로 지정한다.
- 실제 입력 시 Target의 거리와 사용 가능 상태를 다시 확인한 뒤 상호작용을 실행한다.
- 기본 상호작용 거리는 3m로 설정했다.
- Scene View에서 상호작용 범위를 Gizmo로 확인할 수 있도록 구성했다.

### 4. 상호작용 입력 정리

- 키보드 `F` 입력을 기존대로 `Interact`에 사용한다.
- 게임패드 상호작용 입력을 `buttonWest`에서 `D-Pad Down`으로 변경했다.
- 키보드와 게임패드가 동일한 `Interact` Action을 사용하도록 통일했다.

### 5. Player Prefab 적용

- Player Prefab에 `PlayerInteractionController`를 추가했다.
- 플레이어 하위에 `InteractionOrigin`을 생성했다.
- 상호작용 탐색 기준점을 플레이어 몸통 높이에 배치했다.
- 공통 상호작용 거리와 탐색 설정을 Prefab에 저장했다.

### 6. 테스트용 상호작용 오브젝트 구현

- `TestInteractableButton`을 추가했다.
- 상호작용할 때 활성 상태가 전환되도록 구현했다.
- 실행 횟수를 기록하도록 구성했다.
- 현재 사용할 수 없는 버튼도 테스트할 수 있도록 활성 여부 설정을 추가했다.

### 7. Phase 4 수동 테스트 구역 추가

- `Phase4_InteractionTest` Scene에 47일차 전용 테스트 구역을 추가했다.
- 다음 상황을 직접 확인할 수 있도록 테스트 버튼을 배치했다.
  - 가까운 정상 Target
  - 두 번째 정상 Target
  - 사용 불가능한 Target
  - 상호작용 거리 밖 Target
- 기존 Phase 4 테스트 구역에서 이동할 수 있도록 연결 바닥을 추가했다.

### 8. Editor 자동 설정 도구 추가

- `ProjectJ/Day47/Setup Common Interaction` 메뉴를 추가했다.
- 메뉴 실행 시 다음 항목이 자동으로 적용되도록 구성했다.
  - 게임패드 Interact 바인딩 수정
  - Player Prefab 상호작용 기능 적용
  - `InteractionOrigin` 생성
  - Phase 4 상호작용 테스트 구역 생성

### 9. EditMode 테스트 추가

`InteractionTargetRulesTests`를 추가하고 다음 항목을 검사하도록 구성했다.

- 가장 가까운 유효 Target 선택
- 사용 불가능한 Target 제외
- 최대 거리 밖 Target 제외
- 선택된 Target 하나만 상호작용 실행

---

## 추가된 주요 스크립트

```text
Assets/ProjectJ/Runtime/Interaction/
├─ IInteractable.cs
├─ InteractableBehaviour.cs
├─ InteractionTargetRules.cs
├─ PlayerInteractionController.cs
└─ TestInteractableButton.cs

Assets/ProjectJ/Editor/
└─ Day47CommonInteractionSetup.cs

Assets/ProjectJ/Tests/EditMode/
└─ InteractionTargetRulesTests.cs
```

---

## 수정된 주요 요소

- `Assets/InputSystem_Actions.inputactions`
  - 게임패드 Interact 입력을 `D-Pad Down`으로 변경

- `Assets/ProjectJ/Prefabs/Player/Player.prefab`
  - `PlayerInteractionController` 추가
  - `InteractionOrigin` 추가

- `Assets/ProjectJ/Tests/Manual/Phase4/Phase4_InteractionTest.unity`
  - Day47 공통 상호작용 테스트 구역 추가

---

## 구현 결과

- 하나의 공통 인터페이스로 여러 종류의 상호작용 오브젝트를 처리할 수 있게 되었다.
- 플레이어 주변에 여러 대상이 있어도 가장 가까운 유효 Target 하나만 선택된다.
- 범위 밖 또는 사용할 수 없는 대상은 상호작용 대상에서 제외된다.
- 입력 직전에 Target을 다시 검사해 잘못된 상호작용 실행을 방지한다.
- 키보드 `F`와 게임패드 `D-Pad Down`을 동일한 공통 상호작용 입력으로 사용할 수 있게 되었다.
- 이후 체크포인트, 장치, 문, 레버 등 다양한 오브젝트가 동일한 상호작용 구조를 재사용할 수 있는 기반을 마련했다.

---

## 검토

최신 47일차 커밋 기준으로 코드 구조와 참조 관계를 검토했으며, 정적 검토상 개발 진행을 막는 문제는 확인되지 않았다.

GitHub 저장소에는 해당 커밋의 CI 상태가 등록되어 있지 않으므로 Unity Editor의 실제 컴파일 성공 여부와 Test Runner 실행 결과는 로컬 Unity 환경에서 최종 확인한다.
