---
# Project J - 142일차 개발일지

---
## 개발 주제

Player와 AI Bot의 임시 캡슐 외형을 정식 캐릭터 모델로 교체하고, 이후 꾸미기 기능에서 여러 모델을 선택할 수 있는 Visual 구조 구성

---
## 개발 목표

기존 Player Gameplay Root의 Fusion NetworkObject, 이동, Rigidbody, CapsuleCollider와 아이템 기능을 변경하지 않고 시각 모델만 분리한다.

Human Player와 AI Bot 모두 동일한 외형 시스템을 사용하고, 현재 기본 외형은 검은색 요리사 `Character_4c4e64`로 지정한다.

Imported Character Prefab을 후보 목록으로 유지하여 이후 꾸미기 기능에서 이름 기반으로 외형을 교체할 수 있는 기반을 마련한다.

---
## 기준 커밋

이번 개발 시작 시점의 로컬 및 `origin/main` 기준:

```text
68821888ceb7d3831e2125e91668267c5596cf29
```

커밋 메시지:

```text
142
```

이전 일차 기준 커밋:

```text
5d00db9 141일차 : ThirdParty 에셋 정식 편입·검증 및 제거
```

---
## 주요 작업 내용

- `ProjectJPlayerVisualController` 추가
- `ProjectJPlayerVisualIndex` 추가
- 검은색 요리사 `Character_4c4e64`를 기본 외형으로 지정
- Imported `Character_*.prefab` 6개를 외형 후보로 연결
- 이름 기반 외형 선택과 기본 외형 복구 정책 구성
- Player Gameplay Root와 Character Visual 분리
- Visual 내부 Collider 비활성화
- Visual 내부 Rigidbody 물리 충돌 차단
- Animator Root Motion 비활성화
- Human Player용 `ProjectJNetworkPlayer.prefab`에 Visual 적용
- AI Bot용 `ProjectJNetworkBot.prefab`에 동일 Visual 적용
- 기존 Player와 Bot의 임시 캡슐 `Visual` 제거
- Day142 Editor Setup이 Player와 AI Bot을 함께 처리하도록 수정
- 실제 Network Player와 Bot Prefab을 검사하는 EditMode 회귀 테스트 추가
- 외형 인덱스와 이름 선택 정책 EditMode 테스트 추가

---
## Player Visual 구조

Gameplay 기능과 화면에 표시되는 모델을 다음과 같이 분리했다.

```text
ProjectJNetworkPlayer / ProjectJNetworkBot
├─ Fusion NetworkObject
├─ NetworkTransform
├─ ProjectJNetworkPlayer
├─ Rigidbody
├─ CapsuleCollider
├─ Player / Bot Gameplay Component
├─ AuthorityCameraMarker
└─ VisualRoot
   └─ Runtime Character Visual
```

Player Root의 위치, Rigidbody와 CapsuleCollider는 기존 Gameplay 기준을 유지한다.

캐릭터 모델의 위치, 회전과 크기 보정은 `ProjectJPlayerVisualController`가 생성한 Visual에만 적용한다.

---
## 기본 캐릭터와 외형 후보

현재 기본 외형은 다음 모델이다.

```text
Character_4c4e64
```

검은색 몸체와 흰색 요리사 모자를 가진 캐릭터를 Human Player와 AI Bot의 공통 기본 모델로 사용한다.

`Assets/ProjectJ/Prefabs/Player/Imported`의 `Character_*.prefab`을 이름순으로 검색하고 최대 8개까지 등록하도록 구성했다.

현재 연결된 후보는 6개이며, 요청한 이름을 찾지 못하면 다음 순서로 외형을 선택한다.

```text
요청 외형 이름
→ 기본 외형 Character_4c4e64
→ 첫 번째 유효 Prefab
→ 유효 후보 없음
```

외형 선택은 이름 기반 API를 사용하므로 이후 꾸미기 데이터에서 Prefab 배열 인덱스에 직접 의존하지 않고 모델 이름을 저장할 수 있다.

---
## 실제 Spawn Prefab 적용 문제와 수정

첫 번째 적용에서는 일반 제작용 프리팹인 다음 경로만 수정했다.

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
```

그러나 Game Scene의 Fusion Bootstrap은 다음 Resource Prefab을 실제 Human Player로 생성한다.

```text
Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkPlayer.prefab
```

따라서 Game Scene에서 기존 캡슐이 그대로 표시되는 문제가 발생했다.

Day142 Setup의 대상 경로를 실제 Network Player Prefab으로 교정하고, 프리팹에 남아 있던 기존 `Visual` 자식과 Mesh를 제거했다.

---
## AI Bot Visual 적용

Human Player 수정 후에도 AI Bot은 별도 Resource Prefab을 생성하기 때문에 기존 주황색 캡슐로 표시됐다.

실제 Bot 생성 경로:

```text
Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkBot.prefab
```

Day142 Setup이 Player와 Bot 두 프리팹에 같은 Visual 구성을 적용하도록 공통 처리 메서드로 정리했다.

Bot 프리팹에서는 다음 Gameplay 구성을 그대로 보존했다.

```text
ProjectJNetworkBotMarker
ProjectJNetworkBotController
ProjectJNetworkBotActionController
ProjectJNetworkPlayer
ProjectJNetworkExternalGameplay
ProjectJNetworkItemInventory
NetworkObject
NetworkTransform
Rigidbody
CapsuleCollider
```

기존 주황색 캡슐 `Visual`만 제거하고 검은색 요리사 Visual을 연결했다.

---
## Visual 물리 분리

Imported Character Prefab 내부에는 Collider, Rigidbody와 Animator가 포함될 수 있다.

이 구성요소가 Player Gameplay Root의 물리와 충돌하지 않도록 런타임 생성 직후 다음 규칙을 적용한다.

```text
Visual Collider
→ 비활성화

Visual Rigidbody
→ isKinematic 활성화
→ Collision Detection 비활성화
→ 물리 충돌 차단

Visual Animator
→ Root Motion 비활성화
```

실제 이동과 충돌 판정은 기존 Player Root의 CapsuleCollider와 Rigidbody만 담당한다.

---
## 수정 및 추가 파일

```text
Assets/ProjectJ/Runtime/Player/
├─ ProjectJPlayerVisualController.cs
├─ ProjectJPlayerVisualController.cs.meta
├─ ProjectJPlayerVisualIndex.cs
└─ ProjectJPlayerVisualIndex.cs.meta

Assets/ProjectJ/Editor/
├─ ProjectJDay142CharacterVisualSetup.cs
└─ ProjectJDay142CharacterVisualSetup.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJPlayerVisualIndexTests.cs
├─ ProjectJPlayerVisualIndexTests.cs.meta
├─ ProjectJPlayerVisualPrefabTests.cs
└─ ProjectJPlayerVisualPrefabTests.cs.meta

Assets/ProjectJ/Prefabs/Player/
└─ Player.prefab

Assets/ProjectJ/Network/Fusion/Player/Resources/
├─ ProjectJNetworkPlayer.prefab
└─ ProjectJNetworkBot.prefab
```

---
## 검증 내용

AI Bot 회귀 테스트는 기존 Bot Prefab에 Visual Controller가 없는 상태에서 먼저 실행했다.

```text
NetworkBotPrefab_UsesChefVisualInsteadOfLegacyCapsule
→ RED: 0/1 Passed
→ Visual Controller null 확인
```

Player와 Bot 프리팹 수정 후 동일 테스트를 다시 실행했다.

```text
ProjectJPlayerVisualPrefabTests
→ 2/2 Passed

ProjectJPlayerVisualIndexTests
→ 11/11 Passed
```

정적 검증 결과:

```text
Player Visual Controller 연결
Bot Visual Controller 연결
Player VisualRoot 존재
Bot VisualRoot 존재
기존 Player Visual 캡슐 제거
기존 Bot Visual 캡슐 제거
Character_4c4e64 기본값 연결
외형 Prefab 6개 연결
Bot Marker 보존
Bot Controller 보존
Bot Action Controller 보존
git diff --check 통과
```

Unity 6000.3.21f1 격리 프로젝트에서 Editor Setup 실행, Script Compile과 EditMode Test를 확인했다.

실제 열려 있는 프로젝트에서는 Play Mode를 종료한 뒤 재시작하여 Human, Host, Client와 Bot의 최종 화면 표시를 확인해야 한다.

---
## 결과

142일차에는 기존 캡슐 기반 Player 표시를 정식 Character Visual 구조로 교체했다.

Human Player와 AI Bot은 동일한 Visual Controller와 외형 후보 목록을 사용하며, 현재 기본 외형은 검은색 요리사 `Character_4c4e64`이다.

Gameplay Root와 Visual을 분리하여 캐릭터 모델 교체가 Fusion, 이동, 충돌, 아이템과 Bot AI 동작에 영향을 주지 않도록 구성했다.

실제 Fusion Resource Prefab을 직접 검사하는 테스트를 추가하여 일반 제작용 Player Prefab만 수정하거나 AI Bot Prefab의 캡슐을 남기는 문제가 다시 발생하지 않도록 했다.

---
## 다음 일차

143일차에서는 캐릭터별 위치, 방향과 크기를 실제 화면 기준으로 보정하고 외형 선택 정보를 네트워크 참가자 데이터와 꾸미기 UI에 연결한다.

핵심 방향:

```text
캐릭터별 Transform 보정
Human / Client / Bot 외형 구분
외형 선택 정보 네트워크 동기화
꾸미기 UI와 이름 기반 Visual 선택 연결
Gameplay 이동·충돌 회귀 검증
```

애니메이션 상태 연결은 모델 Transform과 네트워크 외형 선택이 안정화된 뒤 진행한다.
