# 147일차 개발일지

## 개발 목표

Day146에서 구축한 10m 모듈 기반 고정 시연맵을 월드 좌표 기준에 맞게 정렬하고,
시연 중 맵 외부로 떨어졌을 때 무한 낙하가 발생하지 않도록 안전 바닥을 추가한다.

이번 일차의 기준 좌표는 다음과 같이 고정한다.

- START 모듈을 포함한 고정맵의 최저 아랫면: `Y = 0`
- 고정맵 적층 방향: `+Y`
- 낙하 방지 안전 바닥 윗면: `Y = -1`
- 고정 코스 루트 위치: `(0, 0, 0)`

## 개발 내용

### 1. Day146 고정맵 월드 원점 정렬

`PJ146_DemoCourse`의 기존 10m 모듈은 모듈 중심을 기준으로 배치되어
기본 Floor의 아랫면이 월드 `Y = -5`까지 내려가는 구조였다.

Day147에서는 기존 Day144/Day145 원본 모듈 Prefab을 직접 수정하지 않고,
고정 코스의 `Modules` 루트에 `+5m` Y 오프셋을 적용했다.

이를 통해 첫 START 모듈의 기본 Floor 아랫면을 `Y = 0`에 맞췄다.

상층 모듈은 기존 10m Grid 규칙을 유지하므로 다음과 같이 위쪽으로 적층된다.

- 1층: `Y = 0 ~ 10`
- 2층: `Y = 10 ~ 20`
- 3층: `Y = 20 ~ 30`

### 2. Gameplay 좌표 동기화

모듈만 이동할 경우 START, Checkpoint, Respawn, Finish 위치가 맵과 어긋날 수 있으므로
`Gameplay` 루트에도 동일하게 `+5m` Y 오프셋을 적용했다.

따라서 다음 요소가 고정맵과 같은 좌표 기준을 유지한다.

- StartSpawnPoint
- Checkpoint_Start
- Checkpoint_CP1
- Checkpoint_CP2
- Checkpoint_CP3
- Checkpoint_CP4
- FinishSystem
- FinishTrigger

### 3. 낙하 방지 안전 바닥 추가

고정 코스 전체 아래에 `Day147_SafetyFloor`를 생성했다.

안전 바닥 규칙은 다음과 같다.

- 안전 바닥 윗면: `Y = -1`
- 두께: `1m`
- 중심 높이: `Y = -1.5`
- 고정 코스 X/Z 범위를 기준으로 크기 자동 계산
- 코스 외곽에 추가 Padding 적용
- BoxCollider를 사용해 플레이어의 무한 낙하 방지

안전 바닥은 `PJ146_DemoCourse` 하위에 포함되어
고정맵 Prefab과 Game Scene에서 함께 관리된다.

### 4. Day147 자동 적용 Editor Tool 추가

`Day147DemoCourseStabilization.cs`를 추가하여 다음 작업을 메뉴 한 번으로 처리하도록 구성했다.

`ProjectJ > Day147 > 1. Apply Demo Stabilization`

자동 처리 내용:

1. Day146 고정 코스 Prefab 확인
2. Modules Y 오프셋 적용
3. Gameplay Y 오프셋 적용
4. 기존 Day147 안전 바닥 제거
5. 새 안전 바닥 생성
6. 수정된 Prefab 저장
7. 기존 Game Scene의 `PJ146_DemoCourse` 제거
8. 수정된 Prefab을 월드 원점에 재배치
9. Game Scene 저장
10. 적용 결과 검증

### 5. 좌표 검증 기능 추가

다음 메뉴를 통해 Day147 좌표 규칙을 검증할 수 있도록 했다.

`ProjectJ > Day147 > 2. Validate Demo Stabilization`

검증 항목:

- `Modules` 루트 Y 오프셋이 `5`
- `Gameplay` 루트 Y 오프셋이 `5`
- `Day147_SafetyFloor` 존재
- 안전 바닥 BoxCollider 존재
- 안전 바닥 윗면이 `Y = -1`
- START Floor BoxCollider 존재
- START 발판 아랫면이 `Y = 0`

## 검증 오류 수정

초기 검증에서는 Prefab Asset 상태의 `BoxCollider.bounds` 값을 기준으로 높이를 확인해
다음과 같은 잘못된 오류가 발생했다.

- 안전 바닥 윗면: `-1.5`로 판정
- START 발판 아랫면: `0.2`로 판정

실제 배치 값은 각각 다음과 같았다.

- 안전 바닥 중심 `Y = -1.5`, 두께 `1` → 실제 윗면 `Y = -1`
- START Floor 중심 `Y = 0.2`, 두께 `0.4` → 실제 아랫면 `Y = 0`

Prefab Asset에서도 정확한 Collider 범위를 계산하도록
`GetBoxColliderWorldYRange()` 검증 함수를 추가했다.

이 함수는 Collider의 `center`, `size`, Transform의 `lossyScale`,
회전을 반영하여 실제 Y 최소/최대 범위를 계산한다.

## 변경 파일

주요 변경 파일:

- `Assets/ProjectJ/Editor/Day147/Day147DemoCourseStabilization.cs`
- `Assets/ProjectJ/Prefabs/Map/Courses/PJ146_DemoCourse.prefab`
- `Assets/ProjectJ/Scenes/Game.unity`

Day144/Day145 원본 모듈 Prefab은 수정하지 않고 유지한다.

## 현재 상태

GitHub `main`에는 Day147 안정화 코드와 수정된 Collider 검증 로직이 반영되어 있다.

다만 GitHub에 연결된 CI 상태가 등록되어 있지 않으므로
Unity Editor에서의 최종 Compile, Validation PASS, 실제 플레이 결과는 수동 확인 대상으로 남긴다.

시연 전 최종 확인 순서:

1. Unity Compile 오류 확인
2. `ProjectJ > Day147 > 2. Validate Demo Stabilization`
3. START 발판 위치 확인
4. 안전 바닥 위치 확인
5. Lobby → Ready → Game 진입 확인
6. 플레이어 낙하 시 안전 바닥 충돌 확인

## 다음 개발 방향

148일차에서는 새로운 맵 시스템을 추가하지 않고
Day146 고정 코스를 START부터 FINISH까지 실제로 완주하면서
이동, 충돌, Checkpoint, Respawn 구간의 시연 안정성을 우선 점검한다.
