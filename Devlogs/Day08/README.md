# Project J

3D 3인칭 온라인 수직 점프 경쟁 파티 게임 **Project J**의 개발 저장소입니다.

---

# 개발 환경

| 항목 | 내용 |
|---|---|
| 게임 엔진 | Unity 6 |
| Unity 버전 | 6000.3.21f1 |
| 프로젝트 템플릿 | Universal 3D |
| 렌더 파이프라인 | URP |
| 대상 플랫폼 | Steam PC |
| 개발 인원 | 1인 개발 |
| 기본 온라인 인원 | 4~8인 |
| 입력 시스템 | Unity Input System 1.20.0 |
| 플레이어 이동 예정 방식 | CharacterController |
| 저장소 | siwoo440/Project-J |

---

# 8일차 : 물리 레이어와 충돌 행렬 구성

## 개발 목표

플레이어, 지면, 장애물, 체크포인트, 아이템 상자, 상호작용 오브젝트, 밀치기 판정과 부활 보호 상태를 서로 다른 Unity 물리 레이어로 분리했습니다.

각 레이어의 고정 번호와 이름을 Runtime 코드로 정의하고, Unity의 `TagManager.asset`과 3D Physics Layer Collision Matrix를 Editor 도구로 자동 구성하도록 구현했습니다.

이번 일차에서는 실제 플레이어 이동, 체크포인트, 아이템 획득, 밀치기와 부활 보호 기능을 구현하지 않았습니다. 이후 시스템이 공통으로 사용할 물리 판정 기준과 레이어 마스크를 먼저 고정했습니다.

주요 목표는 다음과 같습니다.

- Project J 전용 사용자 레이어 8개 정의
- 사용자 레이어 번호를 8~15번으로 고정
- 레이어 이름과 번호를 Runtime 코드에서 공통 관리
- Project J 전용 레이어 사이 충돌 규칙 정의
- 체크포인트와 아이템 상자의 불필요한 월드 충돌 차단
- 밀치기 판정 대상을 일반 플레이어로 제한
- 부활 보호 상태에서 플레이어 몸 충돌과 밀치기 차단
- 부활 보호 중에도 지면·장애물·체크포인트·아이템 상자 판정 유지
- 자주 사용하는 물리 레이어 마스크 제공
- `TagManager.asset` 레이어 이름 자동 구성
- `DynamicsManager.asset` 충돌 행렬 자동 구성
- 기존 사용자 레이어를 자동으로 덮어쓰지 않는 보호 처리
- 현재 프로젝트 설정을 다시 검사하는 Editor 메뉴 제공
- 레이어 번호·이름·충돌 규칙·물리 행렬을 검사하는 EditMode 테스트 추가

---

# 최신 커밋

| 항목 | 내용 |
|---|---|
| 커밋 제목 | `8일차 : 물리 레이어와 충돌 행렬 구성` |
| 커밋 SHA | `baabecd03244435a8d2224e563c8c1841ab08afb` |
| 브랜치 | `main` |
| 이전 커밋 | `acf3065c1662a5640ad20e910adab5d14f912921` |
| 커밋 링크 | https://github.com/siwoo440/Project-J/commit/baabecd03244435a8d2224e563c8c1841ab08afb |

---

# 최신 커밋 검토 결과

최신 커밋을 기준으로 다음 항목을 확인했습니다.

- 커밋 제목이 `8일차 : 물리 레이어와 충돌 행렬 구성`으로 정상 등록
- Project J 전용 사용자 레이어 8개 추가
- Layer 8~15에 고정된 레이어 번호 선언
- `TagManager.asset`에 전용 레이어 이름 반영
- `DynamicsManager.asset`의 3D 물리 충돌 행렬 변경
- Runtime 레이어 enum 추가
- Runtime 레이어 이름·번호·마스크 관리 코드 추가
- Runtime 충돌 규칙 코드 추가
- Runtime 공통 LayerMask 코드 추가
- Editor 레이어 자동 구성 도구 추가
- Editor 충돌 행렬 자동 구성 도구 추가
- 기존 사용자 레이어 보호 처리 추가
- 레이어 이름과 충돌 행렬 수동 검증 메뉴 추가
- 물리 레이어 EditMode 테스트 8개 추가
- 관련 스크립트·폴더의 `.meta` 파일 추가

저장소에서 확인 가능한 범위에서는 수정이 필요한 치명적인 구조 문제를 발견하지 못했습니다.

현재 커밋에는 GitHub Actions 상태 검사나 Unity 자동 테스트 결과가 등록되어 있지 않습니다. 따라서 다음 항목은 로컬 Unity 에디터에서 최종 확인해야 합니다.

```text
Console Error: 0개
EditMode Passed: 39개
EditMode Failed: 0개
Configure Physics Layers 실행 성공
Validate Physics Layers 실행 성공
Tags and Layers의 Layer 8~15 확인
Physics Layer Collision Matrix 확인
```

---

# 전용 물리 레이어

## 1. 레이어 번호와 이름

Project J 전용 물리 레이어는 Unity 사용자 레이어 범위인 8번부터 15번까지 사용합니다.

| 레이어 번호 | 레이어 이름 | 주요 역할 |
|---:|---|---|
| 8 | Player | 일반 플레이어 본체 |
| 9 | Ground | 바닥·벽·고정 발판 |
| 10 | Obstacle | 이동·회전·낙하 장애물 |
| 11 | Checkpoint | 체크포인트 Trigger |
| 12 | ItemBox | 아이템 상자 Trigger |
| 13 | Interactable | F 상호작용 대상 |
| 14 | PushHitbox | 밀치기 판정 Trigger |
| 15 | RespawnProtection | 부활 보호 상태 플레이어 |

Unity 기본 레이어는 변경하지 않았습니다.

```text
Default
TransparentFX
Ignore Raycast
Water
UI
```

---

## 2. ProjectPhysicsLayer

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Physics/ProjectPhysicsLayer.cs
```

Project J 전용 레이어 번호를 enum으로 고정합니다.

```text
Player = 8
Ground = 9
Obstacle = 10
Checkpoint = 11
ItemBox = 12
Interactable = 13
PushHitbox = 14
RespawnProtection = 15
```

게임 코드에서는 다음처럼 직접 숫자를 작성하지 않습니다.

```csharp
gameObject.layer = 8;
```

대신 다음 공통 형식을 사용합니다.

```csharp
gameObject.layer =
    ProjectPhysicsLayers.GetIndex(ProjectPhysicsLayer.Player);
```

레이어 번호를 한 곳에서 관리하므로 프로젝트 설정과 코드 사이의 불일치를 줄일 수 있습니다.

---

# 레이어별 역할

## 3. Player

일반 플레이어 캐릭터 본체에 사용합니다.

예상 적용 대상:

```text
CharacterController
플레이어 본체 Collider
플레이어 충돌 판정
체크포인트 진입 판정
아이템 상자 진입 판정
상호작용 범위 판정
밀치기 대상 판정
```

일반 플레이어끼리는 서로 몸 충돌이 발생하도록 구성했습니다.

---

## 4. Ground

플레이어가 서거나 착지할 수 있는 고정 월드 지형에 사용합니다.

예상 적용 대상:

```text
바닥
벽
계단
고정 발판
경사면
낙하하지 않는 맵 구조물
```

이후 CharacterController 이동과 지면 감지에서 `Ground` 레이어를 사용합니다.

---

## 5. Obstacle

플레이어를 방해하거나 움직이는 물리 오브젝트에 사용합니다.

예상 적용 대상:

```text
회전 봉
움직이는 벽
이동 발판
회전 원판
낙하 장애물
움직이는 맵 장치
```

`Ground`와 분리했기 때문에 향후 장애물의 네트워크 동기화, 충돌 효과와 개별 동작을 별도로 관리할 수 있습니다.

---

## 6. Checkpoint

체크포인트 진입 감지용 Trigger Collider에 사용합니다.

예상 구성:

```text
Checkpoint
└─ Box Collider
   └─ Is Trigger: 활성화
```

일반 플레이어와 부활 보호 플레이어는 모두 체크포인트에 진입할 수 있습니다.

다음 월드 레이어와는 불필요하게 상호작용하지 않습니다.

```text
Ground
Obstacle
ItemBox
PushHitbox
```

---

## 7. ItemBox

아이템 상자 획득 범위 Trigger에 사용합니다.

예상 구성:

```text
ItemBox
├─ Mesh
└─ Trigger Collider
   ├─ Layer: ItemBox
   └─ Is Trigger: 활성화
```

일반 플레이어와 부활 보호 플레이어 모두 아이템 상자를 획득할 수 있도록 충돌 행렬을 유지했습니다.

---

## 8. Interactable

플레이어가 F키로 상호작용하는 오브젝트에 사용합니다.

예상 적용 대상:

```text
버튼
레버
문
승강기 호출 장치
안내판
상호작용 가능한 맵 장치
```

이후 상호작용 Raycast 또는 Overlap 검사에서는 `InteractionTargets` 마스크를 사용할 수 있습니다.

---

## 9. PushHitbox

밀치기 공격의 판정 Trigger에 사용합니다.

예상 플레이어 구조:

```text
Player
└─ PushHitbox
   ├─ Layer: PushHitbox
   └─ Collider Is Trigger: 활성화
```

밀치기 판정은 일반 `Player` 레이어만 대상으로 합니다.

다음 대상은 밀치기 판정에서 제외됩니다.

```text
Ground
Obstacle
Checkpoint
ItemBox
Interactable
PushHitbox
RespawnProtection
```

---

## 10. RespawnProtection

부활 직후 일정 시간 동안 플레이어 몸 충돌과 밀치기를 차단하기 위한 레이어입니다.

부활 보호 중 차단되는 조합:

```text
RespawnProtection ↔ Player
RespawnProtection ↔ PushHitbox
RespawnProtection ↔ RespawnProtection
```

부활 보호 중에도 유지되는 조합:

```text
RespawnProtection ↔ Ground
RespawnProtection ↔ Obstacle
RespawnProtection ↔ Checkpoint
RespawnProtection ↔ ItemBox
RespawnProtection ↔ Interactable
```

이를 통해 부활 직후 다른 플레이어나 밀치기에 의해 즉시 다시 추락하는 상황을 막으면서, 맵 이동과 진행 Trigger는 정상적으로 사용할 수 있습니다.

실제 보호 시간과 레이어 전환 기능은 이후 부활 시스템에서 구현합니다.

---

# 레이어 이름과 번호 관리

## 11. ProjectPhysicsLayers

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Physics/ProjectPhysicsLayers.cs
```

다음 기능을 제공합니다.

```text
전용 레이어 전체 목록
레이어 개수
레이어 고정 번호 조회
레이어 고정 이름 조회
단일 레이어 마스크 생성
프로젝트 설정과 이름 일치 여부 검사
```

주요 사용 예시:

```csharp
int playerLayer =
    ProjectPhysicsLayers.GetIndex(ProjectPhysicsLayer.Player);
```

```csharp
string playerLayerName =
    ProjectPhysicsLayers.GetName(ProjectPhysicsLayer.Player);
```

```csharp
int playerMask =
    ProjectPhysicsLayers.GetMask(ProjectPhysicsLayer.Player);
```

`GetName`은 정의되지 않은 enum 값이 전달되면 예외를 발생시켜 잘못된 레이어 사용을 조기에 발견하도록 구성했습니다.

---

# 충돌 규칙

## 12. ProjectPhysicsCollisionRules

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Physics/ProjectPhysicsCollisionRules.cs
```

Project J 전용 레이어 사이의 충돌 허용 여부를 코드로 정의합니다.

두 레이어의 전달 순서와 무관하게 같은 결과가 나오도록 번호가 작은 레이어와 큰 레이어로 정렬한 뒤 규칙을 검사합니다.

예시:

```text
Player → Ground
Ground → Player
```

두 방향 모두 같은 결과를 반환합니다.

---

## 13. Player 충돌 규칙

| 대상 | 충돌·Trigger 판정 |
|---|:---:|
| Player | 허용 |
| Ground | 허용 |
| Obstacle | 허용 |
| Checkpoint | 허용 |
| ItemBox | 허용 |
| Interactable | 허용 |
| PushHitbox | 허용 |
| RespawnProtection | 차단 |

일반 플레이어는 월드, 진행 Trigger, 상호작용 대상과 밀치기 판정에 참여합니다.

부활 보호 상태 플레이어와는 몸 충돌하지 않습니다.

---

## 14. Ground 충돌 규칙

`Ground`는 다음 전용 레이어와 상호작용합니다.

```text
Player
Ground
Obstacle
Interactable
RespawnProtection
```

다음 Trigger·판정 레이어와는 불필요하게 충돌하지 않습니다.

```text
Checkpoint
ItemBox
PushHitbox
```

---

## 15. Obstacle 충돌 규칙

`Obstacle`은 다음 전용 레이어와 상호작용합니다.

```text
Player
Ground
Obstacle
Interactable
RespawnProtection
```

진행 Trigger와 밀치기 판정에는 참여하지 않습니다.

---

## 16. Checkpoint 충돌 규칙

`Checkpoint`는 다음 플레이어 상태만 감지합니다.

```text
Player
RespawnProtection
```

다음 레이어와는 충돌하지 않습니다.

```text
Ground
Obstacle
Checkpoint
ItemBox
Interactable
PushHitbox
```

---

## 17. ItemBox 충돌 규칙

`ItemBox`는 다음 플레이어 상태만 감지합니다.

```text
Player
RespawnProtection
```

월드, 다른 아이템 상자와 밀치기 판정에는 참여하지 않습니다.

---

## 18. PushHitbox 충돌 규칙

`PushHitbox`는 일반 `Player`와만 상호작용합니다.

```text
PushHitbox ↔ Player: 허용
```

다음 조합은 차단합니다.

```text
PushHitbox ↔ Ground
PushHitbox ↔ Obstacle
PushHitbox ↔ Checkpoint
PushHitbox ↔ ItemBox
PushHitbox ↔ Interactable
PushHitbox ↔ PushHitbox
PushHitbox ↔ RespawnProtection
```

---

## 19. RespawnProtection 충돌 규칙

부활 보호 상태는 다음 월드·진행 레이어와 상호작용합니다.

```text
Ground
Obstacle
Checkpoint
ItemBox
Interactable
```

다음 플레이어 관련 판정은 차단합니다.

```text
Player
PushHitbox
RespawnProtection
```

---

# 공통 LayerMask

## 20. ProjectPhysicsLayerMasks

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Physics/ProjectPhysicsLayerMasks.cs
```

각 단일 레이어 마스크와 자주 사용하는 결합 마스크를 제공합니다.

단일 마스크:

```text
Player
Ground
Obstacle
Checkpoint
ItemBox
Interactable
PushHitbox
RespawnProtection
```

결합 마스크:

```text
World
ProgressTriggers
InteractionTargets
PushTargets
AllProjectLayers
```

---

## 21. World 마스크

구성:

```text
Ground
Obstacle
Interactable
```

향후 사용 대상:

```text
CharacterController 이동 충돌 검사
지면 감지
머리 위 공간 검사
월드 Raycast
장애물 접촉 검사
```

사용 예시:

```csharp
int worldMask = ProjectPhysicsLayerMasks.World;
```

---

## 22. ProgressTriggers 마스크

구성:

```text
Checkpoint
ItemBox
```

향후 사용 대상:

```text
체크포인트 Trigger
아이템 상자 Trigger
진행 구역 검사
```

---

## 23. InteractionTargets 마스크

구성:

```text
Interactable
ItemBox
```

향후 사용 대상:

```text
F 상호작용 Raycast
근처 상호작용 대상 검색
아이템 상자 상호작용 검사
```

---

## 24. PushTargets 마스크

구성:

```text
Player
```

`RespawnProtection`은 포함하지 않습니다.

따라서 밀치기 대상 검사에서 부활 보호 플레이어를 자연스럽게 제외할 수 있습니다.

---

# Unity 프로젝트 설정 변경

## 25. TagManager.asset

수정 파일:

```text
ProjectSettings/TagManager.asset
```

Layer 8~15에 다음 이름이 등록되었습니다.

```text
8  Player
9  Ground
10 Obstacle
11 Checkpoint
12 ItemBox
13 Interactable
14 PushHitbox
15 RespawnProtection
```

기존 Unity 기본 레이어와 다른 사용자 레이어 번호는 변경하지 않았습니다.

---

## 26. DynamicsManager.asset

수정 파일:

```text
ProjectSettings/DynamicsManager.asset
```

3D Physics Layer Collision Matrix가 Project J 전용 충돌 규칙에 맞게 변경되었습니다.

커밋에서는 기존에 모든 조합이 활성화되어 있던 충돌 행렬 값이 전용 레이어 규칙을 반영한 값으로 갱신되었습니다.

Unity가 현재 버전 형식으로 프로젝트 설정을 다시 저장하면서 다음 일부 직렬화 필드도 갱신되었습니다.

```text
m_SimulationMode
m_ThreadingMode
m_InvokeCollisionCallbacks
```

이는 레이어 구성 도구 실행 후 Unity가 물리 설정 에셋을 현재 직렬화 형식으로 저장한 결과입니다.

---

# Editor 자동 구성

## 27. ProjectPhysicsLayerEditorUtility

파일 위치:

```text
Assets/_ProjectJ/Scripts/Editor/Physics/ProjectPhysicsLayerEditorUtility.cs
```

다음 기능을 담당합니다.

```text
TagManager.asset 불러오기
Layer 8~15 이름 검사
전용 레이어 이름 적용
기존 사용자 레이어 보호
Physics 충돌 행렬 적용
TagManager와 DynamicsManager 저장
현재 레이어 이름 검증
현재 Physics Matrix 검증
검증 오류 목록 반환
```

---

## 28. 기존 사용자 레이어 보호

Layer 8~15에 이미 다른 이름이 등록되어 있으면 자동으로 덮어쓰지 않습니다.

예시:

```text
Layer 8: Enemy
```

예상 오류:

```text
Layer 8는 이미 'Enemy' 이름으로 사용 중입니다.
자동으로 덮어쓰지 않습니다.
```

빈 레이어 또는 이미 예상 이름이 등록된 레이어만 설정합니다.

이를 통해 다른 시스템이나 패키지가 사용 중인 사용자 레이어를 실수로 삭제하는 상황을 방지합니다.

---

## 29. 충돌 행렬 적용

모든 Project J 전용 레이어 조합을 한 번씩 순회합니다.

동작 흐름:

```text
첫 번째 전용 레이어 선택
→ 두 번째 전용 레이어 선택
→ ProjectPhysicsCollisionRules 검사
→ 충돌 허용 여부 결정
→ Physics.IgnoreLayerCollision 적용
```

동일한 조합을 두 번 처리하지 않도록 두 번째 반복은 첫 번째 인덱스부터 시작합니다.

---

# Unity 메뉴

## 30. Day08PhysicsLayerSetupTool

파일 위치:

```text
Assets/_ProjectJ/Scripts/Editor/Day08PhysicsLayerSetupTool.cs
```

Unity 상단 메뉴에 다음 항목을 추가했습니다.

```text
Project J
└─ Day 08
   ├─ Configure Physics Layers
   └─ Validate Physics Layers
```

Play Mode 실행 또는 진입 중에는 두 메뉴가 비활성화됩니다.

---

## 31. Configure Physics Layers

다음 작업을 자동으로 수행합니다.

```text
레이어 이름 적용
→ 프로젝트 설정 저장
→ Physics Layer Collision Matrix 적용
→ 물리 설정 저장
→ 전체 설정 재검증
→ 성공 또는 실패 로그 출력
→ 결과 대화상자 표시
```

정상 로그:

```text
[Day08] Project J 물리 레이어 8개와 3D 충돌 행렬 구성을 완료했습니다.
```

예외가 발생하면 전체 예외를 Console에 출력하고 실패 대화상자를 표시합니다.

---

## 32. Validate Physics Layers

현재 프로젝트의 레이어와 충돌 행렬을 수정하지 않고 검사합니다.

검사 항목:

```text
Layer 8~15 이름
고정 번호와 이름 일치
모든 전용 레이어 조합의 충돌 규칙
Unity Physics Matrix와 코드 규칙 일치
```

정상 로그:

```text
[Day08] Project J 물리 레이어 이름과 충돌 행렬 검증을 통과했습니다.
```

오류 예시:

```text
Layer 8: 예상 이름 'Player', 현재 이름 ''
```

```text
Player ↔ Ground:
예상 충돌 True, 실제 충돌 False
```

---

# EditMode 테스트

## 33. ProjectPhysicsLayerTests

파일 위치:

```text
Assets/_ProjectJ/Tests/EditMode/ProjectPhysicsLayerTests.cs
```

8일차에 다음 8개의 테스트를 추가했습니다.

### LayerIndicesAreUniqueAndUseUserLayerRange

다음을 검사합니다.

```text
레이어 번호 중복 없음
모든 번호가 8~31 범위
전용 레이어 수 8개
```

### ConfiguredLayerNamesMatchExpectedIndices

실제 `TagManager.asset`에 등록된 레이어 이름과 코드의 고정 번호가 일치하는지 검사합니다.

### CollisionRulesAreSymmetric

두 레이어의 전달 순서를 바꿔도 같은 결과가 나오는지 검사합니다.

```text
Player → Ground
Ground → Player
```

### PhysicsMatrixMatchesProjectCollisionRules

코드의 충돌 규칙과 Unity 3D Physics Layer Collision Matrix가 모든 전용 레이어 조합에서 일치하는지 검사합니다.

### PlayerCollidesWithWorldProgressAndPushHitbox

일반 플레이어가 다음 대상과 정상적으로 상호작용하는지 검사합니다.

```text
Player
Ground
Obstacle
Checkpoint
ItemBox
Interactable
PushHitbox
```

### TriggerLayersIgnoreUnrelatedWorldPairs

Checkpoint와 ItemBox가 지면·장애물·다른 Trigger와 불필요하게 충돌하지 않는지 검사합니다.

### RespawnProtectionIgnoresPlayersAndPushHitboxesButKeepsWorld

부활 보호 상태가 다음 규칙을 만족하는지 검사합니다.

```text
Player 차단
PushHitbox 차단
RespawnProtection 차단
Ground 허용
Obstacle 허용
Checkpoint 허용
ItemBox 허용
```

### CommonLayerMasksContainExpectedLayers

다음 결합 LayerMask의 구성을 검사합니다.

```text
World
ProgressTriggers
PushTargets
```

---

# 전체 테스트 구성

기존 테스트:

```text
2일차 ProjectStructureTests: 2개
3일차 GameSceneCatalogTests: 3개
4일차 GameServiceRegistryTests: 4개
5일차 InputActionAssetTests: 6개
6일차 ProjectDataValidatorTests: 8개
7일차 PlayerSettingsTests: 8개
```

8일차 신규 테스트:

```text
ProjectPhysicsLayerTests: 8개
```

예상 전체 결과:

```text
Passed: 39
Failed: 0
Ignored: 0
```

GitHub에는 Unity Test Runner를 실행하는 CI가 등록되어 있지 않으므로 실제 통과 여부는 로컬 Unity에서 확인해야 합니다.

---

# 생성·수정된 주요 파일

## 새로 생성된 Runtime 파일

```text
Assets/_ProjectJ/Scripts/Runtime/Core/Physics
├─ ProjectPhysicsLayer.cs
├─ ProjectPhysicsLayers.cs
├─ ProjectPhysicsCollisionRules.cs
└─ ProjectPhysicsLayerMasks.cs
```

## 새로 생성된 Editor 파일

```text
Assets/_ProjectJ/Scripts/Editor
├─ Day08PhysicsLayerSetupTool.cs
└─ Physics
   └─ ProjectPhysicsLayerEditorUtility.cs
```

## 새로 생성된 테스트 파일

```text
Assets/_ProjectJ/Tests/EditMode
└─ ProjectPhysicsLayerTests.cs
```

## 수정된 프로젝트 설정 파일

```text
ProjectSettings/TagManager.asset
ProjectSettings/DynamicsManager.asset
```

각 새 폴더와 스크립트의 `.meta` 파일도 함께 Git에 등록했습니다.

---

# 주요 프로젝트 구조

```text
Assets/_ProjectJ
├─ Scripts
│  ├─ Runtime
│  │  └─ Core
│  │     └─ Physics
│  │        ├─ ProjectPhysicsLayer.cs
│  │        ├─ ProjectPhysicsLayers.cs
│  │        ├─ ProjectPhysicsCollisionRules.cs
│  │        └─ ProjectPhysicsLayerMasks.cs
│  └─ Editor
│     ├─ Day08PhysicsLayerSetupTool.cs
│     └─ Physics
│        └─ ProjectPhysicsLayerEditorUtility.cs
└─ Tests
   └─ EditMode
      └─ ProjectPhysicsLayerTests.cs

ProjectSettings
├─ TagManager.asset
└─ DynamicsManager.asset
```

---

# 수동 검증 절차

## 34. Tags and Layers 확인

Unity 메뉴:

```text
Edit
→ Project Settings
→ Tags and Layers
```

다음 레이어를 확인합니다.

```text
Layer 8  Player
Layer 9  Ground
Layer 10 Obstacle
Layer 11 Checkpoint
Layer 12 ItemBox
Layer 13 Interactable
Layer 14 PushHitbox
Layer 15 RespawnProtection
```

---

## 35. Physics Matrix 확인

Unity 메뉴:

```text
Edit
→ Project Settings
→ Physics
```

`Layer Collision Matrix`에서 핵심 조합을 확인합니다.

활성화:

```text
Player ↔ Player
Player ↔ Ground
Player ↔ Obstacle
Player ↔ Checkpoint
Player ↔ ItemBox
Player ↔ Interactable
Player ↔ PushHitbox
RespawnProtection ↔ Ground
RespawnProtection ↔ Obstacle
RespawnProtection ↔ Checkpoint
RespawnProtection ↔ ItemBox
```

비활성화:

```text
Player ↔ RespawnProtection
PushHitbox ↔ RespawnProtection
RespawnProtection ↔ RespawnProtection
Checkpoint ↔ Ground
Checkpoint ↔ Obstacle
ItemBox ↔ Ground
ItemBox ↔ Obstacle
Checkpoint ↔ ItemBox
```

---

## 36. Editor 검증 메뉴 실행

Unity 메뉴:

```text
Project J
→ Day 08
→ Validate Physics Layers
```

정상 로그:

```text
[Day08] Project J 물리 레이어 이름과 충돌 행렬 검증을 통과했습니다.
```

---

## 37. Test Runner 실행

Unity 메뉴:

```text
Window
→ General
→ Test Runner
```

또는 Unity 환경에 따라:

```text
Window
→ Analysis
→ Test Runner
```

`EditMode` 탭에서 `Run All`을 실행합니다.

예상 결과:

```text
Passed: 39
Failed: 0
Ignored: 0
```

---

# 검증 결과

| 검증 항목 | 저장소 확인 |
|---|:---:|
| 최신 커밋 제목 정상 | 완료 |
| Player 레이어 8번 등록 | 완료 |
| Ground 레이어 9번 등록 | 완료 |
| Obstacle 레이어 10번 등록 | 완료 |
| Checkpoint 레이어 11번 등록 | 완료 |
| ItemBox 레이어 12번 등록 | 완료 |
| Interactable 레이어 13번 등록 | 완료 |
| PushHitbox 레이어 14번 등록 | 완료 |
| RespawnProtection 레이어 15번 등록 | 완료 |
| Runtime 레이어 enum 추가 | 완료 |
| Runtime 레이어 이름·번호 관리 추가 | 완료 |
| Runtime 충돌 규칙 추가 | 완료 |
| Runtime 공통 LayerMask 추가 | 완료 |
| 기존 사용자 레이어 보호 추가 | 완료 |
| TagManager.asset 변경 | 완료 |
| DynamicsManager.asset 변경 | 완료 |
| Physics Matrix 자동 구성 도구 추가 | 완료 |
| 설정 수동 검증 메뉴 추가 | 완료 |
| EditMode 테스트 8개 추가 | 완료 |
| GitHub CI 상태 검사 | 미구성 |

로컬 Unity 최종 확인 항목:

```text
Console Error: 0개
EditMode Passed: 39개
EditMode Failed: 0개
Tags and Layers 설정 정상
Physics Layer Collision Matrix 정상
Validate Physics Layers 통과
```

---

# 이후 활용 방향

8일차에서 구성한 레이어와 마스크는 이후 다음 시스템에서 사용합니다.

| 레이어·마스크 | 연결 예정 기능 |
|---|---|
| Player | 플레이어 본체와 몸 충돌 |
| Ground | CharacterController 이동과 지면 감지 |
| Obstacle | 이동·회전 장애물 |
| Checkpoint | 체크포인트 Trigger |
| ItemBox | 아이템 획득 Trigger |
| Interactable | F 상호작용 |
| PushHitbox | 밀치기 판정 |
| RespawnProtection | 부활 직후 보호 |
| World | 지면·벽·장애물 검사 |
| ProgressTriggers | 체크포인트·아이템 상자 검사 |
| InteractionTargets | 상호작용 대상 검색 |
| PushTargets | 일반 플레이어 밀치기 대상 검색 |

실제 기능을 구현할 때 레이어 이름 문자열이나 번호를 직접 반복하지 않고 이번 일차에서 만든 공통 형식을 사용합니다.

---

# 커밋 정보

```text
8일차 : 물리 레이어와 충돌 행렬 구성
```

```text
https://github.com/siwoo440/Project-J/commit/baabecd03244435a8d2224e563c8c1841ab08afb
```
