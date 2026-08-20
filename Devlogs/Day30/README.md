# 30일차 개발일지 - 체크포인트 기본 활성화 시스템

## 1. 개발 목표

30일차의 목표는 플레이어가 CP1~CP4 체크포인트 바닥을 통과했을 때 해당 체크포인트를 즉시 저장하고, 현재 저장된 체크포인트를 화면에서 확인할 수 있는 기본 구조를 만드는 것이다.

현재 기준 커밋:

```text
67d0f38a8be8d1ce8f0f9a2840685f20496fab06
```

현재 커밋 메시지:

```text
30
```

이번 일차의 핵심 흐름은 다음과 같다.

```text
START
↓
CP1 Trigger 접촉
↓
현재 Checkpoint = CP1
↓
CP2 Trigger 접촉
↓
현재 Checkpoint = CP2
↓
CP3 Trigger 접촉
↓
현재 Checkpoint = CP3
↓
CP4 Trigger 접촉
↓
현재 Checkpoint = CP4
```

---

## 2. 체크포인트 ID 정의

체크포인트 식별을 위해 다음 값을 사용한다.

```text
Start
CP1
CP2
CP3
CP4
```

파일:

```text
Assets/ProjectJ/Runtime/Checkpoint/CheckpointId.cs
```

현재 단계에서는 체크포인트의 순서 비교나 최고값 유지 기능을 적용하지 않는다.

---

## 3. Checkpoint 컴포넌트

체크포인트 Trigger를 담당하는 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Checkpoint/Checkpoint.cs
```

주요 역할:

- Checkpoint ID 보관
- RespawnPoint 참조
- Trigger Collider 강제 사용
- Player가 Trigger에 진입했는지 감지
- PlayerCheckpointTracker 검색
- 해당 Player의 현재 체크포인트 갱신

Trigger 접촉 시 흐름:

```text
Player Collider 진입
↓
Checkpoint.OnTriggerEnter()
↓
PlayerCheckpointTracker 탐색
↓
ActivateCheckpoint()
```

---

## 4. PlayerCheckpointTracker

플레이어별 체크포인트 상태를 보관하는 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Checkpoint/PlayerCheckpointTracker.cs
```

Player Prefab에 이 컴포넌트를 추가했다.

저장 데이터:

```text
CurrentCheckpointId
CurrentCheckpoint
RespawnPosition
RespawnRotation
```

게임 시작 시 아직 체크포인트를 밟지 않았다면:

```text
CurrentCheckpointId = Start
```

상태로 시작한다.

각 Player가 독립적으로 자신의 체크포인트를 가지므로 이후 멀티플레이 구조에서도 플레이어별 진행 상태로 확장할 수 있다.

---

## 5. 시작 지점 저장

PlayerCheckpointTracker는 최초 상태에서 현재 Player 위치와 방향을 START 지점으로 저장할 수 있다.

```text
Checkpoint = Start
Respawn Position = 시작 위치
Respawn Rotation = 시작 방향
```

30일차에서는 Respawn 위치를 저장만 하며 실제 부활 이동은 아직 수행하지 않는다.

---

## 6. RespawnPoint 준비

각 체크포인트 Trigger 아래에 다음 Transform을 생성한다.

```text
CheckpointTrigger
└─ RespawnPoint
```

Checkpoint는 이 Transform의 위치와 회전을 저장한다.

이 데이터는 이후 체크포인트 Respawn 구현에서 사용할 예정이다.

현재 30일차에서는:

```text
저장 O
실제 Respawn X
```

상태다.

---

## 7. Day25 고정맵 체크포인트 연결

기존 Day25 고정맵에 이미 존재하는 체크포인트 앵커를 사용한다.

```text
Checkpoint_01_200m
Checkpoint_02_400m
Checkpoint_03_600m
Checkpoint_04_800m
```

각 앵커에 다음 구조를 자동 생성한다.

```text
Checkpoint_01_200m
└─ CheckpointTrigger
   └─ RespawnPoint
```

CP2~CP4도 동일한 구조를 사용한다.

매핑:

```text
Checkpoint_01_200m → CP1
Checkpoint_02_400m → CP2
Checkpoint_03_600m → CP3
Checkpoint_04_800m → CP4
```

---

## 8. Trigger Layer

CheckpointTrigger는 프로젝트에 존재하는 경우:

```text
GameplayTrigger
```

Layer를 사용한다.

Trigger Collider는:

```text
Is Trigger = true
```

로 설정한다.

Player는 Rigidbody와 Collider를 보유하고 있으므로 Trigger 접촉 감지가 가능한 기존 Player 구조를 그대로 사용한다.

---

## 9. Player Prefab 수정

대상:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
```

기존 Player 기능은 유지하면서 다음 컴포넌트를 추가했다.

```text
PlayerCheckpointTracker
```

기존 이동, 높이 추적, 실시간 순위 관련 컴포넌트는 수정하지 않았다.

---

## 10. CheckpointDebugView

현재 체크포인트를 테스트 화면에서 확인하기 위한 Debug View:

```text
Assets/ProjectJ/Runtime/Checkpoint/CheckpointDebugView.cs
```

표시 예:

```text
Checkpoint : Start
```

CP1 접촉 후:

```text
Checkpoint : CP1
```

CP4 접촉 후:

```text
Checkpoint : CP4
```

기존 테스트 UI와 가독성을 맞추기 위해 글자는 검은색으로 표시한다.

이 UI는 최종 HUD가 아니라 기능 검증용 Debug View다.

---

## 11. Editor 자동 설정

Editor 스크립트:

```text
Assets/ProjectJ/Editor/Day30CheckpointSetup.cs
```

Unity 메뉴:

```text
ProjectJ
→ Day30
→ Setup Basic Checkpoints
```

실행 시 다음 작업을 자동으로 수행한다.

```text
Player.prefab에 PlayerCheckpointTracker 추가
↓
Day25 고정맵 CP1~CP4 앵커 검색
↓
각 앵커에 CheckpointTrigger 생성
↓
RespawnPoint 생성
↓
Checkpoint 컴포넌트 설정
↓
CheckpointDebugView 설정
↓
Day30 수동 테스트 Scene 생성
```

---

## 12. Day30 수동 테스트 Scene

생성 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day30/
└─ Day30_CheckpointTest.unity
```

빠른 테스트를 위해 CP1~CP4를 한 평면 위에 순서대로 배치한다.

테스트 흐름:

```text
Player 시작
↓
Checkpoint : Start

CP1 통과
↓
Checkpoint : CP1

CP2 통과
↓
Checkpoint : CP2

CP3 통과
↓
Checkpoint : CP3

CP4 통과
↓
Checkpoint : CP4
```

---

## 13. EditMode 테스트

테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/CheckpointTests.cs
```

주요 검증 항목:

- 새 Tracker가 Start 상태인지 확인
- START 위치가 저장되는지 확인
- Checkpoint 활성화 시 ID가 저장되는지 확인
- RespawnPoint 위치가 저장되는지 확인
- 이후 접촉한 체크포인트가 현재 체크포인트를 대체하는지 확인
- null Checkpoint 입력을 거부하는지 확인

30일차 규칙을 명확히 하기 위해 CP4 이후 CP1을 다시 활성화하면 현재 값이 CP1로 바뀌는 테스트도 포함한다.

이는 오류가 아니라 30일차의 기본 규칙이다.

---

## 14. CS0118 네임스페이스 충돌 수정

초기 구현 후 다음 컴파일 오류가 발생했다.

```text
CS0118: 'Checkpoint' is a namespace but is used like a type
```

원인:

```text
namespace ProjectJ.Checkpoint
class Checkpoint
```

처럼 네임스페이스와 클래스 이름이 동일했기 때문이다.

Editor 및 Test 코드에서 명시적 별칭을 사용하도록 수정했다.

```text
CheckpointComponent =
ProjectJ.Checkpoint.Checkpoint
```

현재 커밋에는 이 수정이 반영되어 있다.

---

## 15. 현재 의도적으로 구현하지 않은 규칙

31일차에서 구현할 기능은 이번 일차에 넣지 않았다.

현재는 다음 동작이 허용된다.

```text
CP4 활성화
↓
CP1 다시 접촉
↓
현재 Checkpoint = CP1
```

30일차 목표는 단순히:

```text
접촉한 Checkpoint를 현재 값으로 저장
```

하는 것이다.

다음 일차에서:

```text
낮은 CP로 되돌아가지 않기
높은 CP를 건너뛰어 직접 활성화하기
항상 가장 높은 활성화 CP 유지
```

규칙을 추가할 예정이다.

---

## 16. 생성 및 수정 요소

새 Runtime 폴더:

```text
Assets/ProjectJ/Runtime/Checkpoint/
```

새 파일:

```text
CheckpointId.cs
Checkpoint.cs
PlayerCheckpointTracker.cs
CheckpointDebugView.cs
```

새 Editor 파일:

```text
Assets/ProjectJ/Editor/Day30CheckpointSetup.cs
```

새 Test 파일:

```text
Assets/ProjectJ/Tests/EditMode/CheckpointTests.cs
```

수정 요소:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
Assets/ProjectJ/Tests/Manual/Day25/Day25_ModuleFixedMap.unity
```

새 수동 테스트 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day30/Day30_CheckpointTest.unity
```

삭제 파일:

```text
없음
```

---

## 17. 수동 검증 체크리스트

- [ ] Unity Console 컴파일 Error 0
- [ ] CS0118 오류가 더 이상 발생하지 않음
- [ ] Player Prefab에 PlayerCheckpointTracker 존재
- [ ] 초기 HUD가 `Checkpoint : Start`
- [ ] CP1 접촉 시 `Checkpoint : CP1`
- [ ] CP2 접촉 시 `Checkpoint : CP2`
- [ ] CP3 접촉 시 `Checkpoint : CP3`
- [ ] CP4 접촉 시 `Checkpoint : CP4`
- [ ] 각 Trigger가 Is Trigger 상태
- [ ] 각 Checkpoint에 RespawnPoint 존재
- [ ] EditMode 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

## 18. 개발 결과

30일차에서는 향후 추락 및 Respawn 시스템의 기준이 되는 **플레이어별 체크포인트 기본 활성화 구조**를 구축했다.

최종 구조:

```text
Player
└─ PlayerCheckpointTracker

CP1~CP4
└─ CheckpointTrigger
   ├─ Checkpoint
   └─ RespawnPoint
```

플레이어가 Trigger에 접촉하면 해당 Checkpoint ID와 Respawn 위치/회전을 저장한다.

현재는 가장 최근에 접촉한 체크포인트를 그대로 저장하며, 가장 높은 체크포인트를 유지하는 규칙은 다음 31일차에서 추가한다.

GitHub 저장소에는 현재 이 커밋에 대한 별도 CI 상태가 등록되어 있지 않으므로 최종 완료 판정은 Unity 로컬 환경에서 EditMode / PlayMode 테스트와 Console Error 0을 확인한 뒤 확정한다.
