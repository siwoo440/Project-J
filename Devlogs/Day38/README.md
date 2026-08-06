# 프로젝트 J 개발 일지

---

## 38일차 : 분기 장애물 배치 및 위험도 기반 경로 구분

### 작업 정보

| 항목 | 내용 |
|---|---|
| 작업일 | 2026년 8월 6일 |
| 개발 환경 | Unity 6, C# |
| 기준 커밋 | `7403e06211d20c5d6dc988d480ad46929a9ad24d` |
| 이전 커밋 | `a258f324e50cac3ad157b8dc0f012c88f3e08cf8` |

### 개발 목표

37일차에 검증한 좌우 플레이 가능 경로에 실제 장애물을 배치하고, 고정 시드를 기준으로 안전 경로와 고위험 경로를 구분한다. 장애물 배치 뒤에도 통로 폭과 위험도 예산을 검사하여 통과할 수 없는 맵이나 난이도 기준을 벗어난 맵이 최종 생성 성공으로 처리되지 않도록 한다.

### 주요 구현 내용

- 일반 맵 모듈에 좌우 장애물 배치 지점 추가
- 장애물 Prefab과 데이터 에셋 구성
- 시드 기반 안전·고위험 분기 결정
- 분기별 목표 위험도에 맞춘 장애물 자동 배치
- 모듈 하나당 장애물 최대 수 제한
- 장애물 배치 뒤 남은 통로 폭 검사
- 같은 배치 지점의 중복 사용 검출
- 공통 경로를 제외한 좌우 분기 전용 배치
- 분기별 위험도 최소·최대 예산 검사
- 안전 경로와 고위험 경로의 최소 위험도 차이 검사
- 장애물 결과를 최종 맵 생성 성공 조건에 포함
- 장애물 배치 지점과 난이도 상태의 Scene 기즈모 표시
- 동일 시드 장애물 계획 서명 재현 검사

### 안전·고위험 경로 규칙

안전 경로는 생성 시드의 홀짝에 따라 결정한다.

| 시드 | 안전 경로 | 고위험 경로 |
|---|---:|---:|
| 짝수 | 왼쪽 `Lane -1` | 오른쪽 `Lane 1` |
| 홀수 | 오른쪽 `Lane 1` | 왼쪽 `Lane -1` |

분기별 위험도 기준은 다음과 같다.

| 구분 | 위험도 범위 |
|---|---:|
| 안전 경로 | `6~12` |
| 고위험 경로 | `18~30` |
| 최소 위험도 차이 | `8` 이상 |

### 장애물 배치 규칙

각 일반 모듈에 다음 배치 지점을 구성했다.

```text
Day38_ObstaclePoints
├── ObstaclePoint_Left
└── ObstaclePoint_Right
```

기본 통로와 장애물 크기는 다음 기준을 사용한다.

| 항목 | 값 |
|---|---:|
| 배치 전 통로 폭 | `3m` |
| 장애물 점유 폭 | `0.8m` |
| 배치 뒤 남은 통로 폭 | `2.2m` |
| 보존할 최소 통로 폭 | `1.1m` |
| 모듈별 최대 장애물 수 | `2개` |

장애물 폭이 배치 지점의 허용 폭을 초과하거나, 배치 뒤 통로 폭이 `1.1m`보다 좁아지면 해당 장애물은 배치 후보에서 제외한다.

### 생성 성공 조건 변경

38일차부터 기존 생성 검사와 플레이 가능 경로 검사에 장애물 계획 검사를 추가했다.

```text
모듈 생성·연결 검사
+ 수직 높이 검사
+ 시작부터 종료까지의 플레이 가능 경로 검사
+ 장애물 통로 폭 검사
+ 분기 위험도 예산 검사
= 최종 맵 생성 성공
```

결과는 다음 항목에서 확인할 수 있다.

- `LastGenerationSucceeded`
- `LastValidationReport`
- `LastPlayableRouteReport`
- `LastObstaclePlanReport`
- `GenerationSignature`

### 중복 장애물 집계 오류 수정

Play Mode에서 장애물을 다시 생성할 때 기존 장애물이 삭제 대기 상태로 같은 프레임에 남아 신규 장애물과 함께 검사되는 문제가 발생했다.

오류 발생 결과:

```text
배치: 12
위험도: L 48/R 24
DuplicateSpawnPoint 발생
RiskBudgetExceeded 발생
```

`Destroy()`는 프레임 종료 시 실제 삭제되므로, 삭제를 요청한 기존 장애물과 이전 맵 모듈을 생성 계층에서 즉시 분리한 뒤 비활성화하도록 수정했다.

수정 결과:

- 삭제 예정 장애물을 생성 맵 계층에서 즉시 분리
- 삭제 예정 이전 맵 모듈을 `GeneratedMap`에서 즉시 분리
- 생성 실패 후보 모듈을 현재 생성 계층에서 즉시 분리
- 같은 프레임의 재생성 검사에서 삭제 대기 오브젝트 제외
- 중복 배치와 위험도 이중 합산 방지

기본 시드 `36001`의 목표 결과는 다음과 같다.

```text
장애물 계획 성공 | 배치: 6 | 위험도: L 24/R 12 | 문제: 0
안전 경로: 1 | 고위험 경로: -1
```

### 디버그 시각화

Scene 뷰에서 장애물 상태를 다음 색상으로 구분한다.

| 색상 | 의미 |
|---|---|
| 회색 | 사용 가능한 빈 배치 지점 |
| 초록색 | 안전 경로 장애물 |
| 주황색 | 고위험 경로 장애물 |
| 빨간색 | 통로 폭 또는 데이터 오류 |

### 수정 파일

- `Assets/_ProjectJ/Scripts/Runtime/Data/Definitions/ObstacleDataDefinition.cs`
- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/ProceduralMapGenerator.cs`
- `Assets/_ProjectJ/Prefabs/Map/Modules/MAP-001_FixedStraight.prefab`
- `Assets/_ProjectJ/Prefabs/Map/Modules/MAP-002_LowPassage.prefab`
- `Assets/_ProjectJ/Prefabs/Map/Modules/MAP-003_JumpGap.prefab`
- `Assets/_ProjectJ/Prefabs/Map/Modules/MAP-006_StepRise.prefab`
- `Assets/_ProjectJ/Prefabs/Map/Modules/MAP-007_ZigzagRise.prefab`
- `Assets/_ProjectJ/Prefabs/Map/Modules/MAP-008_JumpRise.prefab`
- `Assets/_ProjectJ/Resources/ProjectDataCatalog.asset`
- `Assets/_ProjectJ/Scenes/Game/Game.unity`

### 신규 파일

- `Assets/_ProjectJ/Scripts/Editor/Day38BranchObstacleSetupTool.cs`
- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapBranchObstaclePlanner.cs`
- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapObstacleDebugVisualizer.cs`
- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapObstaclePlanning.cs`
- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapObstacleSpawnPoint.cs`
- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapPlacedObstacle.cs`
- `Assets/_ProjectJ/Tests/EditMode/MapObstaclePlacementRulesTests.cs`
- `Assets/_ProjectJ/Tests/EditMode/ProceduralMapObstaclePlannerTests.cs`
- `Assets/_ProjectJ/Prefabs/Obstacles/OBS-038_PrototypeBlock.prefab`
- `Assets/_ProjectJ/Data/Definitions/Obstacle/OBS-038_PrototypeBlock.asset`

### 테스트 구성

38일차 EditMode 테스트 10개를 추가했다.

| 구분 | 개수 | 검사 내용 |
|---|---:|---|
| 장애물 규칙 테스트 | 7개 | 시드별 안전 경로, 위험도 범위, 위험도 차이, 통로 폭 |
| 생성기 통합 테스트 | 3개 | 분기 난이도, 분기 전용 배치, 동일 시드 재현 |

주요 테스트 항목:

- 짝수 시드에서 왼쪽 안전 경로 선택
- 홀수 시드에서 오른쪽 안전 경로 선택
- 반대편 경로의 고위험 설정
- 안전·고위험 위험도 예산 경계값 허용
- 최소 위험도 차이 미달 검출
- 통과 가능한 장애물 폭 허용
- 통로를 막는 장애물 폭 거부
- 생성 맵의 안전·고위험 분기 구성
- 공통 경로 장애물 미배치
- 동일 시드의 장애물 계획 서명 일치

### 저장소 검토 결과

- 최신 커밋 제목과 38일차 작업 범위 일치
- 37일차 대비 신규·수정 파일 31개 반영
- 장애물 데이터와 Prefab 참조 연결 확인
- 모듈 Prefab 6종의 좌우 배치 지점 반영 확인
- 생성기의 장애물 계획 통합 확인
- Play Mode 지연 삭제 중복 집계 수정 반영 확인
- 신규 스크립트 `.meta` GUID와 Scene·Prefab 참조 연결 확인
- GitHub 자동 빌드 및 워크플로 결과 없음

### 완료 확인 항목

- [x] 38일차 스크립트와 에셋 최신 커밋 반영
- [x] 장애물 Prefab과 데이터 에셋 추가
- [x] 일반 모듈 Prefab 6종에 좌우 배치 지점 추가
- [x] 안전·고위험 경로 위험도 기준 구현
- [x] 통로 폭과 위험도 예산 검사 구현
- [x] 장애물 디버그 시각화 구현
- [x] 중복 장애물 집계 수정 반영
- [x] EditMode 테스트 코드 10개 추가
- [ ] Unity Console 컴파일 오류 없음 직접 확인
- [ ] 기본 시드에서 `배치 6`, `L 24/R 12`, `문제 0` 직접 확인
- [ ] 기존 테스트와 신규 EditMode 테스트 전체 통과 직접 확인
- [ ] 실제 플레이에서 양쪽 분기 통과 가능 여부 직접 확인

> 저장소의 코드·에셋 구조와 참조는 검토를 완료했다. 실제 Unity 컴파일, Test Runner 및 플레이 결과는 Unity Editor에서 직접 확인한 결과를 기준으로 한다.

---

## 다음 39일차 개발 방향

안전 경로와 고위험 경로의 선택 결과를 기록하고, 경로 난이도 차이에 맞는 보상 데이터를 연결한다. 플레이어가 더 위험한 분기를 선택했을 때 더 높은 보상을 받을 수 있도록 분기 진입 감지, 선택 기록, 보상 배율, 결과 디버그 표시를 구현한다.

---

## 커밋 정보

```text
38일차 : 분기 장애물 배치 및 위험도 기반 경로 구분
7403e06211d20c5d6dc988d480ad46929a9ad24d
```
