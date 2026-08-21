# Project J - 49일차 개발 일지

## 개발 목표

Phase 4에서 개별적으로 구현한 플레이어 경쟁, 장애물, External Force, 상호작용, Module 기능을 하나의 대형 테스트 Scene에 통합하고 실제 플레이 흐름 안에서 연속적으로 검증할 수 있는 환경을 구성한다.

기존 일차별 테스트 Scene과 일회성 Editor Setup 스크립트를 정리해 앞으로 사용할 공용 테스트 환경을 단순화한다.

---

## 주요 개발 내용

### 1. 대형 통합 테스트 Scene 구성

새로운 공용 테스트 Scene을 추가했다.

```text
Assets/ProjectJ/Tests/Manual/Day49/
└─ Day49_AllSystemsTest.unity
```

기존처럼 기능별 테스트맵이 분리되어 있는 구조를 정리하고, 하나의 넓은 맵 안에서 지금까지 구현한 기능을 연속적으로 확인할 수 있도록 구성했다.

### 2. 자연스러운 전체 테스트 동선 구성

전체 테스트 진행은 다음 흐름을 기준으로 배치했다.

```text
START
  ↓
이동 / 달리기 / 점프 / 앉기
  ↓
플레이어 통과 / 밀치기
  ↓
플랫폼 기믹
  ↓
에어백 / External Force
  ↓
Branch / Merge Module
  ↓
F 상호작용 Gate
  ↓
공통 상호작용
  ↓
RETURN / FINISH
```

각 기능을 단순히 나열하는 대신 실제 게임 플레이처럼 다음 시험 구역으로 자연스럽게 이동하도록 도로와 연결 동선을 구성했다.

### 3. 이동 훈련 구역 확장

기본 이동 시스템을 반복해서 확인할 수 있도록 별도의 Movement Training Ground를 구성했다.

주요 구성 요소는 다음과 같다.

- 긴 달리기 활주로
- 좌우 이동을 확인하는 Slalom 기둥
- 높이가 조금씩 증가하는 점프 허들
- 계단 이동 구간
- 앉아서 통과하는 낮은 터널
- 연속 점프 발판
- 좁은 Balance Beam

이를 통해 이동, 달리기, 점프, 앉기를 하나의 연속 코스에서 확인할 수 있게 되었다.

### 4. 플레이어 통과·밀치기 시험 공간 구성

플레이어 간 충돌 및 밀치기 기능을 보다 자연스럽게 시험할 수 있도록 별도의 경기장 형태 공간을 구성했다.

- 플레이어 Body Blocking 비활성 상태 확인
- 다른 플레이어 통과 확인
- 최근접 밀치기 Target 확인
- 밀치기 Force 확인
- 밀치기 후 External Force 처리 확인

경기장 주변에 기준 기둥과 구역 표시를 추가해 플레이어 위치와 밀치기 방향을 확인하기 쉽도록 구성했다.

### 5. 플랫폼 5종 통합 시험 구역 유지

기존 플랫폼 기믹 구역을 새로운 통합맵 안에 포함했다.

테스트 대상은 다음과 같다.

- Moving Platform
- Rotating Platform
- Spring Platform
- Ice Surface
- Ghost Platform

플랫폼 주변에는 위험 영역과 안내 요소를 추가해 여러 기믹을 연속해서 시험할 수 있도록 구성했다.

### 6. AirBag 및 External Force 시험 구역 확장

에어백과 External Force를 자연스럽게 시험할 수 있도록 접근용 활주로와 착지 Target을 추가했다.

- AirBag 진입 속도 확인
- 외력 적용 확인
- 착지 위치 비교
- 다른 External Force와의 연속 사용 확인
- Force가 비정상적으로 남는지 확인

추가로 관찰용 데크를 배치해 테스트 공간을 확인하기 쉬운 구조로 정리했다.

### 7. Branch·Merge Module 코스 통합

48일차에서 구현한 Branch·Merge Greybox 코스를 대형 테스트맵의 한 시험 구역으로 통합했다.

다음 흐름을 그대로 시험할 수 있다.

```text
진입
 ↓
Branch
 ↙   ↘
A     B
 ↘   ↙
Merge
 ↓
복귀
```

기존 Module Prefab 내부의 Safe Volume과 No Spawn Volume 데이터도 그대로 유지된다.

### 8. 공통 F 상호작용을 실제 진행 요소로 사용

단순 버튼 테스트 외에 실제로 진행 경로를 막는 F 상호작용 Gate를 추가했다.

```text
닫힌 Gate
   ↓
Console 접근
   ↓
F 입력
   ↓
Gate 개방
   ↓
다음 구역 진행
```

테스트 전용 `Day49TestGateInteractable`을 추가해 기존 `IInteractable` 시스템을 실제 진행 상황과 비슷한 방식으로 확인할 수 있도록 구성했다.

```text
Assets/ProjectJ/Tests/Manual/Day49/Scripts/
└─ Day49TestGateInteractable.cs
```

### 9. 중앙 Quick Retest Hub 추가

특정 기능만 반복해서 시험할 때 전체 코스를 다시 돌 필요가 없도록 맵 중앙에 Quick Retest Hub를 구성했다.

중앙 허브에서 다음 방향으로 빠르게 이동할 수 있다.

- Movement / Push
- Platform
- AirBag
- Module / Interaction

전체 코스 검증과 개별 기능 반복 검증을 같은 Scene에서 모두 진행할 수 있게 되었다.

### 10. Recovery Lane 추가

맵 외곽에 복귀용 안전 이동 동선을 구성했다.

- North Recovery Lane
- South Recovery Lane
- East Recovery Lane
- West Recovery Lane

장애물 테스트 중 코스에서 벗어나거나 특정 기능 구역을 건너뛰고 싶을 때 안전하게 이동할 수 있도록 구성했다.

### 11. 시작·복귀 광장 및 안내 요소 추가

통합 테스트맵의 가독성을 높이기 위해 다음 요소를 추가했다.

- Start / Finish Plaza
- 시작 위치 Marker
- 구역 입구 Arch
- 기능별 World Label
- 중앙 기준 Tower
- 기능 구역 연결 Road
- 외곽 Boundary

테스트맵 자체가 하나의 Greybox 경기장처럼 보이도록 구조를 정리했다.

---

## 기존 테스트 Scene 정리

기존에 일차별로 생성했던 분리 테스트 Scene들을 정리했다.

정리 대상에는 다음 개발 단계의 Scene이 포함된다.

```text
Day25
Day27
Day28
Day29
Day30
Day32
Day33
Day34
Day35
Day36
Day37
Phase4의 기존 테스트 Scene
```

기본 이동 비교용 Scene인 다음 파일은 유지한다.

```text
Assets/ProjectJ/Tests/Manual/Day11/
└─ Day11_MovementTest.unity
```

최종적으로 앞으로 주요 수동 테스트는 Day11 기준 Scene과 Day49 통합 Scene을 중심으로 진행할 수 있는 구조가 되었다.

---

## Editor Setup 스크립트 정리

기능 구현 과정에서 사용했던 일회성 Editor Setup 스크립트를 제거했다.

주요 정리 대상은 다음과 같다.

```text
Day25ModuleSetup
Day26PlayerHeightSetup
Day27RankingSetup
Day28MatchCountdownSetup
Day29MatchTimerSetup
Day30CheckpointSetup
Day32FallLimitSetup
Day33RespawnSetup
Day34RespawnProtectionSetup
Day35FinishSetup
Day36ResultSetup
Day37SpectatorSetup

Day42ExternalForceSetup
Day43PushFeedbackSetup
Day44PlatformGimmickSetup
Day45AirBagExternalForceSetup
Day46ModuleSafeVolumeSetup
Day47CommonInteractionSetup
Day48BranchMergeGreyboxSetup

Phase4InteractionTestMapSetup
```

생성이 끝난 테스트 Scene과 Runtime 기능은 유지하면서 다시 실행할 필요가 없는 Setup 코드만 제거했다.

현재 저장소에서는 `Assets/ProjectJ/Editor` 폴더도 정리된 상태다.

---

## 유지한 지원 에셋

기존 테스트 Scene을 제거해도 새 Day49 Scene에서 참조할 수 있는 지원 리소스는 유지했다.

- Runtime 기능 스크립트
- Player Prefab
- Module Prefab
- Platform 및 Obstacle 기능
- Phase4 Material
- 테스트 지원용 Runtime 스크립트
- Day49 Material
- Day49 Gate 테스트 스크립트

Scene 정리 때문에 기존 기능 참조가 끊어지지 않도록 테스트 지원 에셋은 별도로 유지한다.

---

## Day49 주요 추가 파일

```text
Assets/ProjectJ/Tests/Manual/Day49/
├─ Day49_AllSystemsTest.unity
│
├─ Materials/
│  ├─ Day49_Accent.mat
│  ├─ Day49_Boundary.mat
│  ├─ Day49_Floor.mat
│  ├─ Day49_Hazard.mat
│  ├─ Day49_Interaction.mat
│  ├─ Day49_Movement.mat
│  ├─ Day49_Rail.mat
│  └─ Day49_Road.mat
│
└─ Scripts/
   └─ Day49TestGateInteractable.cs
```

---

## 구현 결과

- Phase 4 기능을 하나의 대형 Scene에서 연속적으로 시험할 수 있게 되었다.
- 이동부터 경쟁, 장애물, External Force, Module, 상호작용까지 실제 코스 형태로 연결되었다.
- 전체 회귀 테스트와 특정 기능 반복 테스트를 같은 Scene에서 수행할 수 있게 되었다.
- 중앙 Hub와 Recovery Lane을 통해 테스트 이동 시간을 줄였다.
- 기능 구현 과정에서 누적된 일회성 Editor Setup 스크립트를 제거했다.
- 기존 분리 테스트 Scene을 정리하고 공용 테스트 Scene 중심 구조로 전환했다.
- 이후 기능을 추가할 때도 Day49 통합맵에 새로운 시험 구역을 연결할 수 있는 기반을 마련했다.

---

## 현재 확인 상태

최신 GitHub 커밋 기준으로 다음 항목이 저장소에 반영되어 있다.

- `Day49_AllSystemsTest.unity`
- Day49 전용 Material
- `Day49TestGateInteractable`
- 기존 일차별 Editor Setup 스크립트 제거
- 기존 분리 테스트 Scene 정리
- Day11 기준 Movement Test 유지
- Phase4 지원 Material 및 테스트 지원 스크립트 유지

저장소 구조와 현재 남아 있는 Runtime 참조를 기준으로 개발 진행을 막는 명확한 문제는 확인되지 않았다.

다만 GitHub에는 해당 커밋의 CI 상태 검사가 등록되어 있지 않기 때문에 다음 항목은 로컬 Unity 환경에서 최종 확인한다.

- Unity Editor 컴파일 Error 0
- `Day49_AllSystemsTest` Play Mode 실행
- 전체 코스 1회 이상 완주
- 각 플랫폼 기믹 정상 작동
- Push 및 External Force 정상 작동
- Branch A/B 경로 모두 통과 가능
- F Gate 정상 개폐
- Console 반복 Error 및 Exception 없음

---

## 다음 개발 기준

49일차 통합 테스트맵을 Phase 4의 회귀 검증 기준 Scene으로 사용한다.

이후 기능을 추가할 때 기존 기능의 동작이 깨졌는지 확인할 필요가 있을 경우 새 개별 테스트 Scene을 계속 생성하기보다 `Day49_AllSystemsTest`에 시험 공간을 확장하거나 기존 구역에서 회귀 테스트를 진행한다.
