---
# Project J - 141일차 개발일지

---
## 개발 주제

PHASE 11 진입을 위한 ThirdParty 에셋 정식 편입 및 프로젝트 구조 정리

---
## 개발 목표

프로젝트에 임시로 보관되어 있던 `Assets/ProjectJ/ThirdParty` 에셋을 실제 사용 여부와 참조 관계에 따라 분류하고, 필요한 에셋은 Project J 정식 폴더 구조로 이동한다.

이동 과정에서는 Unity Asset GUID를 유지하여 기존 Prefab, Scene, Material, Texture, Animation 등의 참조가 끊기지 않도록 한다.

사용하지 않는 샘플, 데모, 문서, 잔여 에셋은 안전 검증 후 제거하고, 최종적으로 `ThirdParty` 폴더 자체를 삭제하여 PHASE 11의 정식 제작 구조를 확립한다.

---
## 기준 커밋

이번 개발일지 작성 시점의 `main` 기준:

```text
7e919b29fdfbb94f19d2b351c52f778410b8b2c2
```

커밋 메시지:

```text
141
```

이전 기준 커밋:

```text
ff7c8813ed074e22e1e0a1b9c9028206c12df051
```

---
## 주요 작업 내용

- `Assets/ProjectJ/ThirdParty` 전체 에셋 분류
- ThirdParty 의존성 분석용 Day141 Editor 도구 구성
- 실제 사용 중인 Mesh / Material / Texture / Animation / Prefab / Script 분류
- `AssetDatabase.MoveAsset` 기반 GUID 유지 이동
- Project J 정식 `Art`, `Prefabs`, `Runtime`, `Physics` 구조로 에셋 편입
- 외부 참조가 남은 Asset의 삭제 차단
- REVIEW 대상 Asset의 추가 검증
- `Resources.Load` 문자열 참조 검사 추가
- ProjectSettings GUID 참조 검사 추가
- 미사용 샘플 / 데모 / 문서 / 잔여 Resource 정리
- `ThirdParty` 전체 삭제 전 Verify Gate 구성
- 최종 검증 완료 후 `Assets/ProjectJ/ThirdParty` 제거

---
## 정식 에셋 구조 편입

ThirdParty에 있던 제작용 에셋을 Project J 내부 정식 구조로 이동했다.

대표 구조는 다음과 같다.

```text
Assets/ProjectJ/
├─ Art/
│  ├─ Characters/
│  │  ├─ Animations/
│  │  ├─ Materials/
│  │  ├─ Meshes/
│  │  └─ Textures/
│  ├─ Environment/
│  │  ├─ Materials/
│  │  ├─ Meshes/
│  │  └─ Textures/
│  └─ Props/
│     └─ Materials/
│
├─ Prefabs/
├─ Runtime/
│  └─ Props/
│
└─ Physics/
   └─ Materials/
```

이동 작업은 파일 시스템 직접 이동이 아니라 Unity의 `AssetDatabase.MoveAsset`을 사용했다.

이를 통해 기존 Scene과 Prefab이 참조하고 있던 GUID를 유지하면서 에셋 위치만 정식 구조로 변경했다.

---
## 캐릭터 에셋 정리

PartyCharacters에서 실제 사용 중인 캐릭터 관련 에셋을 다음 구조로 편입했다.

```text
Assets/ProjectJ/Art/Characters/
├─ Animations/Imported/
├─ Materials/Imported/
├─ Meshes/Imported/
└─ Textures/Imported/
```

주요 편입 대상에는 다음 항목이 포함된다.

```text
party_character.fbx
customize_objects.fbx
Idle.anim
Jump.anim
fall.anim
running default.anim
Win.anim
Lose.anim
char_AC.controller
ColorPalette.mat
ColorPalette.png
face Material / Texture
Player별 Body Material
```

캐릭터 Prefab이 실제로 참조하는 Material과 Texture는 삭제 대상에서 제외하고 정식 위치로 이동했다.

---
## 환경 및 Props 에셋 정리

Playground, POLY STYLE, ToyBox, Shipping Container 등에서 실제 제작에 활용할 Mesh와 Material을 정식 구조로 편입했다.

대표적으로 다음 종류를 이동했다.

```text
Playground Mesh
Environment Mesh
Ground / Grass / Terrain Material
Skybox Material
Shipping Container Mesh / Material
ToyBox 장애물 Material
RollerBall Mesh / Material
```

이후 PHASE 11 맵 모듈 제작에서는 ThirdParty 원본 경로가 아니라 Project J 정식 폴더의 에셋을 사용한다.

---
## ThirdParty 검증 도구

Day141 작업 중 대량 에셋 이동으로 인한 Missing Reference를 방지하기 위해 단계별 Editor 도구를 구성했다.

```text
Project J
→ Day141
→ ThirdParty
```

주요 처리 단계:

```text
1. Analyze ThirdParty
2. Promote Production Assets
2B. Resolve Verification Blockers
2C. Resolve Remaining Resources
3. Verify References
4. Delete ThirdParty
```

`Verify References` 단계에서 외부 Asset 참조와 REVIEW 대상이 남아 있으면 삭제를 막도록 구성했다.

따라서 `ThirdParty` 전체 삭제는 검증 절차를 통과한 뒤에만 진행했다.

---
## 2B 안전장치 오탐 수정

잔여 REVIEW를 정리하는 과정에서 2B 도구의 `Resources.Load` 검사가 자기 자신의 Editor Script를 참조 대상으로 잘못 판단하는 문제가 발생했다.

대표 결과:

```text
이동 0
삭제 0
안전 차단 58
```

원인은 정리 도구 내부에 다음 두 정보가 동시에 존재했기 때문이다.

```text
Resources.Load
+
정리 대상 Asset 경로 문자열
```

실제 Runtime에서 해당 Resource를 로드하지 않아도 자기 자신의 문자열 때문에 사용 중인 것으로 오판했다.

이를 해결하기 위해 2C 단계에서는:

```text
Editor Script 검사 제외
실제 Resources.Load("경로") 형태만 검사
Resources.LoadAsync("경로") 형태 검사
```

방식으로 검증 범위를 수정했다.

---
## 컴파일 오류 수정

ThirdParty Script를 정식 Runtime 폴더로 옮긴 뒤 두 개의 컴파일 충돌이 발생했다.

### MovingPlatform 이름 충돌

기존 Project J에는 다음 클래스가 존재했다.

```text
ProjectJ.Platforms.MovingPlatform
```

ToyBox에도 전역 namespace의 `MovingPlatform`이 존재하여 EditMode Test가 잘못된 타입을 참조했다.

대표 오류:

```text
CS0117:
'MovingPlatform' does not contain a definition for
'CalculateNextPosition'
```

ToyBox Script를 다음 namespace로 분리했다.

```text
ProjectJ.Imported.ToyBox
```

기존 Project J의 Gameplay `MovingPlatform`과 Imported ToyBox Script의 이름 충돌을 제거했다.

### Match 이름 충돌

Day141 Resolver에서 정규식 타입:

```text
System.Text.RegularExpressions.Match
```

를 사용하던 중 프로젝트의:

```text
ProjectJ.Match
```

namespace와 이름이 충돌했다.

대표 오류:

```text
CS0118:
'Match' is a namespace but is used like a type
```

정규식 타입을 완전 수식 이름으로 사용하여 충돌을 제거했다.

---
## 최종 ThirdParty 제거

검증과 실제 실행 확인이 끝난 뒤:

```text
Assets/ProjectJ/ThirdParty
```

폴더를 최종 제거했다.

이에 따라 프로젝트는 더 이상 기존 ThirdParty 원본 폴더를 제작 경로로 사용하지 않는다.

현재 제작용 외부 에셋은 Project J 내부의 역할별 정식 폴더에서 관리한다.

---
## 최종 검증

ThirdParty 제거 후 다음 항목을 실제 Unity 프로젝트에서 확인했다.

```text
Console Error 0
Game.unity 정상 로드
Missing Script 없음
Missing Prefab 없음
Missing Mesh 없음
Missing Material 없음
Player Material / Face 정상
Bot Visual 정상
Moving Platform 정상
FlipPad 정상
Checkpoint 정상
Lever 정상
RollerBall 정상
EditMode Test 정상
Host + Client + Bot 실제 플레이 정상
Checkpoint / Item / Finish 흐름 정상
```

Day141 과정에서 발생했던 두 컴파일 오류도 최종적으로 제거했다.

---
## 결과

141일차에는 기존 ThirdParty 에셋을 단순히 폴더째 유지하는 방식에서 벗어나, 실제 프로젝트가 사용하는 에셋만 Project J의 정식 구조로 편입했다.

대량 Asset 이동 시 참조가 끊기지 않도록 GUID 유지 이동을 사용했고, 외부 참조와 `Resources.Load` 가능성을 검사하는 안전장치를 거쳐 미사용 에셋을 정리했다.

검증 도구의 오탐과 `MovingPlatform`, `Match` 이름 충돌로 발생한 컴파일 문제도 함께 수정했다.

마지막으로 Unity Scene, EditMode Test, Host / Client / Bot 실제 Gameplay 검증을 마친 뒤 `Assets/ProjectJ/ThirdParty` 폴더를 제거했다.

이제 Project J는 PHASE 11의 캐릭터, 애니메이션, 맵 모듈, 장애물, 아이템 등 실제 에셋 제작을 정식 폴더 구조에서 이어갈 수 있는 상태가 되었다.

---
## 다음 일차

142일차에서는 캐릭터 모델과 Player Visual 구조를 정식 적용한다.

핵심 방향:

```text
Player Gameplay Root
├─ Network / Movement / Collider
└─ Visual
   └─ Character Model
```

Gameplay Collider와 Visual을 분리하여 모델 교체가 기존 이동, 충돌, Fusion 동작에 영향을 주지 않도록 구성한다.

Human과 Bot의 기본 이동 성능은 동일하게 유지하고, 8인 플레이를 고려한 캐릭터 시각 구분 기반도 함께 정리한다.
