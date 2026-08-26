---
# Project J - 145일차 개발일지

---
## 개발 주제

Day144에서 확정한 **10×10×10 정육면체 Map Module 규격**을 유지하면서, 기존 Project J에 구현되어 있던 발판·장애물 Runtime 기능을 실제 맵 모듈에 적용하였다.

이번 일차에서는 단순한 Greybox 지형만 사용하는 것이 아니라, 다음의 기존 게임플레이 기능을 모듈 단위로 재사용할 수 있도록 구성하였다.

```text
MovingPlatform
RotatingPlatform
GhostPlatform
IceSurface
SpringPlatform
AirBagObstacle
```

---
## 개발 목표

이번 일차의 핵심 목표는 다음과 같다.

- Day144의 10×10×10 Module 규격 유지
- 기존 발판·장애물 Runtime 기능을 Map Module에 적용
- 기존 scripted Prefab이 존재하면 우선 재사용
- 기존 Prefab을 직접 사용할 수 없으면 동일 Runtime Component로 안전한 대체 구조 생성
- 모든 모듈을 South Entrance → North Exit 기본 진행형으로 통일
- 10m Cell 범위를 벗어나는 Prefab 또는 이동 경로 방지
- Day145 생성물만 독립적으로 재생성·검증·삭제할 수 있는 Editor 메뉴 구성
- 실제 Day145 Prefab 6종 생성

---
## 기준 커밋

이번 개발 정리 시점의 최신 `main` 기준:

```text
e708b766620e9c1e77594561e71b95a6120e30bc
```

현재 커밋 메시지:

```text
a
```

이전 정식 일차 커밋:

```text
83406d5 144일차 : 10m 정육면체 맵 모듈 규격 전환 및 40종 바리에이션 구축
```

---
## 공통 Module 규격

Day145 모듈은 Day144에서 정리한 `ProjectJ.Map.MapModule` 체계를 그대로 사용한다.

```text
Module Size = 10 × 10 × 10
```

각 모듈은 6방향 Socket을 가진다.

```text
North
South
East
West
Up
Down
```

이번 Day145 장애물 모듈의 기본 연결 상태는 다음과 같다.

```text
South = Entrance
North = Exit

East  = Closed
West  = Closed
Up    = Closed
Down  = Closed
```

Socket 위치는 10m Cell의 각 면 중심에 맞춘다.

```text
North = ( 0,  0, +5)
South = ( 0,  0, -5)

East  = (+5,  0,  0)
West  = (-5,  0,  0)

Up    = ( 0, +5,  0)
Down  = ( 0, -5,  0)
```

따라서 Day145 모듈도 Day144에서 만든 수평·수직 정육면체 Grid와 동일한 생성 규칙 안에서 사용할 수 있다.

---
## 기존 Prefab 재사용 구조

Day145 생성 도구는 다음 경로 아래의 Prefab을 검색한다.

```text
Assets/ProjectJ/Prefabs
```

대상 기능 Component가 포함된 Prefab을 발견하면 우선 해당 Prefab을 Module 내부에 Instance로 배치한다.

```text
기존 scripted Prefab 검색
↓
MapModule이 포함된 Wrapper Prefab 제외
↓
대상 기능 Component 확인
↓
10m Cell 내부 Bounds 확인
↓
MovingPlatform은 이동점 범위 추가 확인
↓
조건 만족 시 기존 Prefab 직접 재사용
```

기존 Prefab이 없거나 10m Cell 범위를 벗어나면 원본 Prefab을 수정하지 않고, 기존 Runtime Component를 이용해 Day145 전용 안전 구조를 생성한다.

이를 통해 기존 기능과 새 맵 모듈 시스템을 분리하지 않고 그대로 연결할 수 있도록 하였다.

---
## 생성된 Day145 Module 6종

생성 위치:

```text
Assets/ProjectJ/Prefabs/Map/Modules/Day145/
```

현재 생성된 Prefab은 다음 6종이다.

```text
PJ145_Module_MovingPlatform_SouthNorth
PJ145_Module_RotatingPlatform_SouthNorth
PJ145_Module_GhostPlatform_SouthNorth
PJ145_Module_IceSurface_SouthNorth
PJ145_Module_SpringPlatform_SouthNorth
PJ145_Module_AirBag_SouthNorth
```

---
## 1. Moving Platform Module

파일:

```text
PJ145_Module_MovingPlatform_SouthNorth.prefab
```

중앙 구간을 좌우로 이동하는 발판을 사용한다.

대체 생성 기준:

```text
MovePoint A = (-1.8, -4.35, 0)
MovePoint B = (+1.8, -4.35, 0)

Platform Size = 3.2 × 0.5 × 3.2
Move Speed = 2.5
```

`MovingPlatform`의 기존 이동 로직과 `PlatformPassengerCarrier` 구조를 그대로 사용한다.

이동점도 10m Cell 내부에 존재하는지 별도로 검사하여, 발판 자체는 Cell 안에 있지만 이동 중 Cell 밖으로 빠져나가는 경우를 방지한다.

---
## 2. Rotating Platform Module

파일:

```text
PJ145_Module_RotatingPlatform_SouthNorth.prefab
```

중앙에 회전 발판을 배치한다.

대체 생성 기준:

```text
Platform Size = 4 × 0.5 × 4
Rotation Axis = Up
Rotation Speed = 35°/s
```

기존 `RotatingPlatform` 기능을 사용하여 플레이어가 회전하는 발판 위를 통과하는 구간으로 활용할 수 있다.

---
## 3. Ghost Platform Module

파일:

```text
PJ145_Module_GhostPlatform_SouthNorth.prefab
```

일정 주기로 나타나고 사라지는 발판을 중앙 통과 구간에 사용한다.

대체 생성 기준:

```text
Platform Size = 4 × 0.5 × 4

Active  = 3초
Warning = 1초
Hidden  = 2초
```

Player Layer가 존재하면 해당 Layer를 사용하고, 없으면 기존 기본값인 Layer 8 기준을 사용한다.

---
## 4. Ice Surface Module

파일:

```text
PJ145_Module_IceSurface_SouthNorth.prefab
```

중앙 통과 구간을 빙판으로 구성한다.

대체 생성 기준:

```text
Ice Size = 4.5 × 0.4 × 5

Acceleration     = 6
Deceleration     = 2.5
TurnAcceleration = 3
```

진입 바닥과 이탈 바닥 사이의 중앙 구간을 `IceSurface`가 직접 담당한다.

---
## 5. Spring Platform Module

파일:

```text
PJ145_Module_SpringPlatform_SouthNorth.prefab
```

중앙에 점프 강화 발판을 배치한다.

대체 생성 기준:

```text
Platform Size = 3.2 × 0.4 × 3.2
Jump Multiplier = 1.5
```

현재 Spring 발판은 10m 위쪽 Cell로 직접 이동시키는 필수 수직 연결 장치로 사용하지 않고, South → North 수평 진행 모듈 내부의 점프 변화 요소로 사용한다.

기본 점프 성능만으로도 정상 진행 경로 자체는 유지하도록 구성하였다.

---
## 6. AirBag Obstacle Module

파일:

```text
PJ145_Module_AirBag_SouthNorth.prefab
```

전체 안전 바닥 위 우측에 에어백 장애물을 배치한다.

대체 생성 기준:

```text
Position = (2.7, FloorTop + 1, 0)
Size = 1.4 × 2 × 1.8

Horizontal Velocity Change = 12
Push Direction = Left
Contact Spread = 0.35
```

필수 진행선을 완전히 막는 구조가 아니라 회피 또는 접촉에 의해 플레이어 위치를 흔드는 장애물로 사용한다.

---
## 기본 진행 지형

AirBag을 제외한 발판 계열 모듈은 남쪽 진입부와 북쪽 이탈부에 고정 바닥을 둔다.

```text
South Entry Floor
        ↓
발판 / 특수 지형
        ↓
North Exit Floor
```

AirBag은 낙하 위험 없이 장애물 자체의 효과를 확인할 수 있도록 전체 바닥을 사용한다.

이 구조는 아이템이나 다른 플레이어의 도움 없이 기본 이동으로 Entrance에서 Exit까지 접근 가능한 형태를 유지하기 위한 기반이다.

---
## Editor 자동 생성 도구

이번 일차에 추가한 Editor Script:

```text
Assets/ProjectJ/Editor/Day145/
└─ Day145ObstacleModuleSetup.cs
```

Unity 상단 메뉴:

```text
ProjectJ
└─ Day145
   ├─ 1. Rebuild Existing Platform Obstacle Modules
   ├─ 2. Validate Day145 Modules
   ├─ 3. Delete Day145 Modules
   └─ 4. Log Existing Platform Obstacle Prefabs
```

### 1. 전체 재생성

```text
Rebuild Existing Platform Obstacle Modules
```

실행 순서:

```text
기존 Day145 생성 폴더 삭제
↓
Day145 출력 폴더 생성
↓
6종 기능별 기존 Prefab 검색
↓
재사용 가능하면 기존 Prefab Instance 적용
↓
불가능하면 Runtime Component 기반 대체 생성
↓
Prefab 저장
↓
자동 검증
```

### 2. 검증

```text
Validate Day145 Modules
```

다음 항목을 검사한다.

```text
Prefab 수 = 6
MapModule = 루트에 1개
Module Size = 10
MapModule 정의 유효성
6방향 Socket
Socket 위치 = ±5m Cell 경계
Down 방향 정상 Exit 금지
대상 기능 Component 존재
Renderer / Collider의 10m Cell 범위
```

### 3. Day145만 삭제

```text
Delete Day145 Modules
```

삭제 대상은 다음 폴더로 제한된다.

```text
Assets/ProjectJ/Prefabs/Map/Modules/Day145
```

기존 발판·장애물 Prefab과 Runtime Script는 삭제하지 않는다.

### 4. 기존 Prefab 확인

```text
Log Existing Platform Obstacle Prefabs
```

각 기능마다 현재 Project에서 직접 재사용 가능한 scripted Prefab이 무엇인지 Console에 출력한다.

---
## 안전 검증 구조

기존 Prefab을 무조건 모듈에 넣지 않고 다음 조건을 검사한다.

```text
MapModule 중첩 여부
기능 Component 존재 여부
Renderer Bounds
Collider Bounds
10m Cell 범위
MovingPlatform 이동점 범위
```

10m Cell을 벗어나는 기존 Prefab은 직접 수정하거나 강제로 축소하지 않는다.

대신 같은 Runtime Component를 사용하는 Day145 전용 대체 구조를 생성한다.

이 방식으로 원본 자산을 보존하면서 정육면체 랜덤맵 규격을 유지한다.

---
## 추가 및 생성 파일

Editor Script:

```text
Assets/ProjectJ/Editor/Day145/
└─ Day145ObstacleModuleSetup.cs
```

생성 Module:

```text
Assets/ProjectJ/Prefabs/Map/Modules/Day145/
├─ PJ145_Module_MovingPlatform_SouthNorth.prefab
├─ PJ145_Module_RotatingPlatform_SouthNorth.prefab
├─ PJ145_Module_GhostPlatform_SouthNorth.prefab
├─ PJ145_Module_IceSurface_SouthNorth.prefab
├─ PJ145_Module_SpringPlatform_SouthNorth.prefab
└─ PJ145_Module_AirBag_SouthNorth.prefab
```

각 Unity Asset에는 대응하는 `.meta` 파일도 함께 생성되어 있다.

---
## 최신 커밋 확인

최신 커밋 `e708b766620e9c1e77594561e71b95a6120e30bc`에는 다음 Day145 핵심 변경이 포함되어 있다.

```text
Day145 Editor 생성 도구 추가
Day145 Module 폴더 추가
Day145 Prefab 6종 추가
각 생성 Asset의 meta 파일 추가
```

추가로 다음 Animator Controller가 수정되어 있다.

```text
Assets/ProjectJ/Art/Characters/Animations/Imported/char_AC.controller
```

확인된 Diff에서는 `run` 조건의 동일 transition 내용이 유지된 상태에서 내부 transition fileID가 변경된 형태이며, Day145 장애물 모듈 기능 자체와 직접 연결된 변경은 아니다.

GitHub Commit Status에는 별도의 CI 상태가 등록되어 있지 않다.

따라서 저장소 구조와 직렬화된 Prefab 존재 여부는 확인할 수 있지만, Unity Editor의 실제 컴파일 성공 및 Play Mode 동작 성공 여부는 이 개발일지에서 확정하지 않는다.

---
## 145일차 완료 상태

현재 저장소 기준으로 다음 작업이 반영되어 있다.

```text
[완료] 10×10×10 Module 규격 유지
[완료] 기존 발판·장애물 Runtime 기능 연결
[완료] 기존 scripted Prefab 우선 재사용 구조
[완료] Cell 범위 초과 시 Runtime 기반 대체 생성
[완료] Moving Platform Module 생성
[완료] Rotating Platform Module 생성
[완료] Ghost Platform Module 생성
[완료] Ice Surface Module 생성
[완료] Spring Platform Module 생성
[완료] AirBag Obstacle Module 생성
[완료] Day145 전용 재생성 메뉴
[완료] Day145 전용 검증 메뉴
[완료] Day145 전용 삭제 메뉴
[완료] 실제 Prefab 6종 저장
```

---
## 다음 개발 방향

다음 단계에서는 Day144의 기본·경로 Module과 Day145의 발판·장애물 Module을 함께 사용하여 실제 시연용 맵을 조립한다.

예정된 진행 구조:

```text
START
↓
CP1
↓
CP2
↓
CP3
↓
CP4
↓
FINISH
```

Day146에서는 개별 Module 기능 제작보다 **실제 플레이 가능한 데모 코스 구성과 구간 연결**을 우선한다.
