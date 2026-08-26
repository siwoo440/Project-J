---
# Project J - 146일차 개발일지

---
## 개발 주제

Day144에서 구축한 **10×10×10 정육면체 Map Module**과 Day145에서 구축한 **기존 발판·장애물 기반 10m 모듈 6종**을 실제 게임 흐름으로 연결하여, START부터 CP1~CP4를 거쳐 FINISH까지 이어지는 **시연용 고정 코스**를 구축하였다.

이번 일차에서는 새로운 맵 Runtime 시스템을 추가하지 않고 기존 시스템을 재사용하는 것을 원칙으로 하였다.

```text
Day144
10×10×10 기본·바리에이션 모듈

+

Day145
Moving / Rotating / Ghost / Ice / Spring / AirBag
장애물 모듈

↓

Day146
START → CP1 → CP2 → CP3 → CP4 → FINISH
시연용 고정맵 조립
```

---
## 기준 커밋

이번 개발 정리 시점의 최신 커밋:

```text
019182bc70d1ea5392017422c200d8a3da6d5b66
```

현재 커밋 메시지:

```text
a
```

이전 정식 일차 커밋:

```text
2adf6c0 145일차 : 기존 발판·장애물 기반 10m 모듈 6종 구축
```

---
## 개발 목표

이번 일차의 목표는 다음과 같다.

- Day144 / Day145 모듈을 이용해 실제 완주 가능한 고정 코스 구성
- 모든 모듈을 10m Grid 단위로 배치
- 수평 진행뿐 아니라 Y축 수직 상승 구간 포함
- Branch → Side Route → Merge 구조 포함
- 기존 Checkpoint 시스템을 이용해 Start / CP1 / CP2 / CP3 / CP4 배치
- 기존 Finish 시스템을 이용해 결승 Trigger와 도착 순위 처리 연결
- 코스 전체를 하나의 `PJ146_DemoCourse.prefab`으로 관리
- Game Scene에는 코스 Prefab Instance 하나를 배치
- Editor 메뉴에서 코스 생성·검증·Scene 배치·삭제 가능하도록 구성
- Day144 / Day145 원본 모듈은 수정·삭제하지 않음

---
## 시연용 코스 기본 규격

코스는 Day144에서 확정한 규격을 그대로 사용한다.

```text
Module Size
= 10m × 10m × 10m

Grid 간격
X = 10m
Y = 10m
Z = 10m
```

코스 생성 도구의 현재 기준 모듈 수:

```text
34 Modules
```

결승 구간 마지막 Module Cell:

```text
(0, 2, 28)
```

모든 배치는 임의의 실수 좌표가 아니라 `Vector3Int Cell × 10m` 방식으로 계산한다.

---
## 전체 코스 흐름

고정 코스는 다음 진행 구조를 사용한다.

```text
START
  ↓
Section 01
  ↓
CP1
  ↓
Section 02
  ↓
CP2
  ↓
Section 03
  ↓
CP3
  ↓
Section 04
  ├─ Main Route
  └─ Side Route
       ↓
      Merge
       ↓
CP4
  ↓
Section 05
  ↓
FINISH
```

각 구간은 기존 Module Prefab을 직접 재사용한다.

---
## Section 01 - 기본 이동과 첫 장애물

START 이후 첫 구간은 기본 이동과 쉬운 장애물을 배치하였다.

```text
START
↓
Straight Slalom
↓
Moving Platform
↓
Jump Single
↓
Straight Pillars
↓
CP1
```

주요 목적:

- 기본 이동 확인
- 점프 확인
- 이동 발판 적응
- 장애물 회피 감각 확인
- CP1까지 안정적으로 진입

---
## Section 02 - 타이밍 장애물

CP1 이후에는 Day145에서 만든 시간·움직임 기반 장애물을 집중적으로 사용한다.

```text
CP1
↓
Ghost Platform
↓
Rotating Platform
↓
Narrow Bridge
↓
AirBag
↓
CP2
```

이 구간에서는 다음 요소를 확인한다.

- 사라지는 발판 타이밍
- 회전 발판 대응
- 좁은 발판 이동
- AirBag 밀어내기 대응

---
## Section 03 - 수직 상승 구간

Project J의 3차원 적층형 맵 구조를 실제 코스에 적용하였다.

```text
CP2
↓
South → Up
↓
Vertical Platforms
↓
Down → North
↓
Spring Platform
↓
CP3
```

주요 Cell 배치:

```text
(0, 0, 11)
↓ Up
(0, 1, 11)
↓ Up
(0, 2, 11)
↓ North
(0, 2, 12)
↓
(0, 2, 13) = CP3
```

수직 구간도 기존 Socket 연결 규칙을 그대로 사용하며 Module 자체를 임의로 회전시키지 않는다.

---
## Section 04 - Branch / Merge 구간

CP3 이후에는 두 경로로 나뉘었다가 다시 합쳐지는 구조를 배치하였다.

```text
                 ┌─ Main : Ice Surface ─────────┐
CP3 → Branch ────┤                              ├→ Merge
                 └─ Side : Merge Entry          │
                           → Ghost               │
                           → Corner ─────────────┘

Merge
↓
Double Side Drop
↓
Straight Pillars
↓
Rotating Platform
↓
CP4
```

Main Route와 Side Route는 모두 동일한 10m Grid를 사용하며, 분기에서 생성된 Exit가 실제 인접 Cell의 Entrance로 연결되도록 구성하였다.

---
## Section 05 - 최종 종합 구간

CP4 이후에는 앞 구간에서 경험한 장애물들을 조합한 최종 코스를 구성하였다.

```text
CP4
↓
Jump Double
↓
Moving Platform
↓
Ghost Platform
↓
AirBag
↓
Jump Stepping Stones
↓
Straight Narrow
↓
Straight Slalom
↓
FINISH
```

새로운 규칙을 추가하기보다 앞에서 배운 이동·점프·타이밍·위험 회피를 다시 사용하는 종합 구간으로 구성하였다.

---
## 기존 Checkpoint 시스템 재사용

새로운 Day146 전용 Checkpoint Runtime을 만들지 않았다.

기존 `ProjectJ.Checkpoint.Checkpoint`와 `CheckpointId`를 그대로 사용한다.

사용 ID:

```text
Start
CP1
CP2
CP3
CP4
```

각 Checkpoint에는 다음 구성이 자동 생성된다.

```text
Checkpoint_CP*
├─ BoxCollider (Trigger)
├─ Rigidbody (Kinematic)
├─ Checkpoint
└─ RespawnPoint
```

Trigger 크기:

```text
8 × 3 × 1.2
```

RespawnPoint는 Trigger 바로 이전의 안전한 바닥 쪽에 배치하여 낙하 직후 다시 위험 지역에 생성되는 상황을 줄이도록 구성하였다.

---
## START Spawn Marker

코스에는 플레이어 생성 위치 연결을 위한 Marker를 추가하였다.

```text
PJ146_DemoCourse
└─ Gameplay
   └─ StartSpawnPoint
```

이 Marker는 첫 번째 START Module의 남쪽 안전 공간에 배치되어 있다.

기존 Game / Network Spawn 시스템이 별도의 Spawn Transform 참조를 사용한다면 Scene 또는 기존 Spawn 설정에서 이 Transform을 연결해야 한다.

---
## 기존 FINISH 시스템 재사용

FINISH 역시 Day146 전용 Runtime 시스템을 만들지 않고 기존 기능을 재사용한다.

```text
Gameplay
├─ FinishSystem
│  └─ FinishOrderManager
│
└─ FinishTrigger
```

`FinishTrigger`는 기존 `FinishOrderManager`를 직접 참조하도록 자동 구성한다.

FINISH Gate는 마지막 Module의 북쪽 안전 영역에 배치하였다.

Trigger 크기:

```text
8 × 3 × 1.5
```

---
## Demo Course Prefab

전체 고정맵은 하나의 Prefab으로 저장한다.

```text
Assets/ProjectJ/Prefabs/Map/Courses/
└─ PJ146_DemoCourse.prefab
```

Prefab 내부 기본 구조:

```text
PJ146_DemoCourse
├─ Modules
│  ├─ START
│  ├─ Section_01
│  ├─ CP1
│  ├─ Section_02
│  ├─ CP2
│  ├─ Section_03
│  ├─ CP3
│  ├─ Section_04
│  ├─ Section_04_Main
│  ├─ Section_04_Side
│  ├─ CP4
│  ├─ Section_05
│  └─ FINISH
│
└─ Gameplay
   ├─ StartSpawnPoint
   ├─ Checkpoint_Start
   ├─ Checkpoint_CP1
   ├─ Checkpoint_CP2
   ├─ Checkpoint_CP3
   ├─ Checkpoint_CP4
   ├─ FinishSystem
   └─ FinishTrigger
```

각 Module은 Day144 또는 Day145 원본 Prefab Instance로 배치하여 기존 Prefab 연결을 유지한다.

---
## Game Scene 배치

생성한 `PJ146_DemoCourse.prefab`은 실제 Game Scene에도 배치하였다.

대상 Scene:

```text
Assets/ProjectJ/Scenes/Game.unity
```

Scene에서는 코스를 개별 Module 수십 개로 직접 관리하지 않고 다음 Prefab Instance 하나로 관리한다.

```text
PJ146_DemoCourse
```

재배치 시에는 기존 Scene의 모든 맵 오브젝트를 삭제하지 않고, 이름이 정확히 `PJ146_DemoCourse`인 이전 Day146 루트만 제거한 뒤 새 인스턴스를 배치한다.

---
## Editor 자동 생성 도구

추가한 Editor Script:

```text
Assets/ProjectJ/Editor/Day146/
└─ Day146DemoCourseSetup.cs
```

Editor 메뉴:

```text
ProjectJ
└─ Day146
   ├─ 1. Build Demo Course Prefab
   ├─ 2. Validate Demo Course Prefab
   ├─ 3. Rebuild And Place In Game Scene
   ├─ 4. Delete Demo Course From Game Scene
   └─ 5. Delete Demo Course Prefab
```

### 1. Build Demo Course Prefab

필요한 Day144 / Day145 원본 Prefab을 확인한 뒤 34개 Module을 지정 Cell에 배치하고, Checkpoint 및 FINISH를 구성하여 `PJ146_DemoCourse.prefab`으로 저장한다.

### 2. Validate Demo Course Prefab

생성된 코스가 현재 규격을 만족하는지 검사한다.

### 3. Rebuild And Place In Game Scene

코스 Prefab을 다시 생성하고 자동 검증한 뒤 실제 `Game.unity`에 배치한다.

Scene에 이미 Day146 코스가 있으면 해당 인스턴스만 교체한다.

### 4. Delete Demo Course From Game Scene

`Game.unity`에서 `PJ146_DemoCourse` 루트만 제거한다.

### 5. Delete Demo Course Prefab

`PJ146_DemoCourse.prefab`만 삭제하며 Day144 / Day145 원본 Module은 유지한다.

---
## 자동 검증 항목

Day146 생성 도구는 다음을 검사한다.

```text
필요 Day144 / Day145 원본 Prefab 존재 여부

전체 Module 수 = 34

모든 Module = 10m Grid 정렬
동일 Cell 중복 배치 금지
Module Size = 10
MapModule Definition 유효
Socket = 6개

Exit 방향 다음 Cell 존재 여부
Exit ↔ 반대 방향 Entrance 연결 가능 여부

START 포함 Checkpoint = 5개
Checkpoint ID 중복 금지
Start / CP1 / CP2 / CP3 / CP4 모두 존재

FinishOrderManager = 1개
FinishTrigger = 1개
FinishTrigger → FinishOrderManager 참조 일치
```

마지막 FINISH Module의 North Exit만 실제 다음 Module이 없어도 정상 종료로 허용한다.

---
## 수정 및 추가 파일

이번 커밋의 주요 변경 파일:

```text
Assets/ProjectJ/Editor/Day146/
└─ Day146DemoCourseSetup.cs

Assets/ProjectJ/Prefabs/Map/Courses/
└─ PJ146_DemoCourse.prefab

Assets/ProjectJ/Scenes/
└─ Game.unity
```

또한 Unity에서 Animator Controller가 다시 저장되면서 `char_AC.controller` 내부 transition fileID가 변경되었으나, 확인된 diff에서는 해당 transition의 조건 자체를 변경한 작업은 포함되지 않았다.

---
## 최신 커밋 점검

최신 커밋은 Day145 정식 커밋보다 1개 앞선 상태이며 Day146 관련 변경이 하나의 커밋에 포함되어 있다.

확인된 주요 사항:

```text
Day146 Editor 생성 도구 존재
PJ146_DemoCourse.prefab 존재
Game.unity에 PJ146_DemoCourse Prefab Instance 반영
34 Module 기준 정의
START + CP1~CP4 생성 코드 존재
FinishOrderManager + FinishTrigger 생성 코드 존재
10m Grid / 중복 Cell / Socket 연결 자동 검증 존재
안전한 Day146 전용 삭제 범위 존재
```

저장소 정적 검토 기준으로 즉시 수정이 필요한 구조적 문제는 확인되지 않았다.

다만 현재 GitHub 커밋에는 실행된 CI / Unity Test Runner 결과가 등록되어 있지 않다. 따라서 다음 항목은 Unity Editor에서 실제 확인이 필요하다.

```text
C# 실제 컴파일
Console Error 없음
Day146 Validation PASS 로그
Player START → FINISH 완주
CP1~CP4 활성화와 부활 위치
Moving / Rotating / Ghost / AirBag 실제 동작
수직 구간 착지 가능 여부
Branch 양쪽 루트 완주 가능 여부
FINISH 도착 순위 등록
Bot 완주 가능 여부
```

---
## 146일차 결과

이번 일차로 다음 연결이 완성되었다.

```text
Day144
맵 Module 제작

↓

Day145
발판·장애물 Module 제작

↓

Day146
실제 Demo Course 조립
```

따라서 이후에는 새로운 맵 구조를 추가하기보다 현재 코스를 실제로 반복 플레이하며 난이도와 통과 가능성을 검증하는 단계로 넘어간다.

---
## 다음 147일차 방향

147일차의 핵심은 **맵 플레이 검증과 수정**이다.

우선 확인할 항목:

```text
Player 전체 완주
Bot 전체 완주
체크포인트 부활
낙하 판정
Jump 거리
Moving Platform 이동 타이밍
Ghost Platform 대기 시간
Rotating Platform 난이도
AirBag 밀어내기 강도
수직 구간 착지 안정성
Branch 양쪽 경로 난이도 차이
FINISH 도착 처리
```

Day147에서는 Module 시스템을 다시 만드는 것보다 플레이 테스트에서 실제 문제가 확인된 Module 또는 배치 값만 조정하는 방향으로 진행한다.

---
## 추천 커밋 제목

```text
146일차 : 10m 모듈 기반 시연용 고정맵 및 체크포인트 코스 구축
```
