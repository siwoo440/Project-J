# 프로젝트 J 개발일지

---

## 32일차 : 분기·합류 경로 그래프 및 다중 연결 지점 배치

- 작업일: 2026년 8월 6일
- 기준 커밋: `aaa32901bdc6801a981e716b31ef83af490a3cfb`
- 개발 환경: Unity 6, C#

---

## 개발 목표

31일차에 구현한 시드 기반 선형 맵 생성기를 확장하여, 하나의 경로가 두 갈래로 나뉜 뒤 다시 합쳐지는 분기형 맵을 자동 생성할 수 있도록 구현했다.

이번 작업에서는 단순한 모듈 나열을 넘어 생성된 맵의 연결 관계를 그래프로 기록하고, 다중 연결 지점과 연결부 규격을 검사할 수 있는 구조를 마련했다.

---

## 주요 구현 내용

### 1. 분기·합류 경로 생성

- 시작 모듈 이후 분기 모듈 자동 배치
- 분기 지점에서 왼쪽·오른쪽 병렬 경로 생성
- 두 경로를 합류 모듈의 개별 입구에 연결
- 합류 이후 일반 경로 추가 생성
- 기본 설정 기준 총 8개 모듈 배치

기본 생성 구조는 다음과 같다.

```text
시작 모듈
→ 분기 모듈
→ 왼쪽 경로 2개 / 오른쪽 경로 2개
→ 합류 모듈
→ 후속 모듈
```

### 2. 분기·합류 모듈 추가

- `MAP-004_Branch` 분기 모듈 추가
- 중앙 입구 1개와 좌우 출구 2개 구성
- `MAP-005_Merge` 합류 모듈 추가
- 좌우 입구 2개와 중앙 출구 1개 구성
- 각 연결 지점에 너비 `2m`, 높이 `2.2m` 적용

### 3. 생성 그래프 구조 구현

- 생성된 모듈을 그래프 노드로 기록
- 모듈 사이의 연결 관계를 그래프 간선으로 기록
- 출발 모듈과 도착 모듈의 연결 지점 ID 저장
- 시작 노드를 기준으로 모든 모듈에 도달할 수 있는지 검사
- 생성 결과를 비교할 수 있는 고정 시드 서명 생성

### 4. 연결 지점 검사 강화

- 이전 모듈의 모든 사용 가능한 출구 검사
- 후보 모듈의 모든 입구 검사
- 후보 모듈의 모든 허용 회전 조합 검사
- 이미 사용한 연결 지점의 중복 사용 차단
- 연결 지점의 너비와 높이 호환성 검사
- 연결 위치의 허용 오차 검사
- 모듈 Bounds 겹침 차단 유지

### 5. 생성 설정 확장

`MapGenerationSettings`에 다음 설정을 추가했다.

| 설정 | 기본값 | 용도 |
|---|---:|---|
| Connection Size Tolerance | `0.05` | 연결부 너비·높이 허용 오차 |
| Connection Position Tolerance | `0.02` | 연결 지점 위치 허용 오차 |
| Use Branching Path | 활성화 | 분기형 경로 생성 여부 |
| Branch Pair Count | `2` | 좌우 경로별 일반 모듈 수 |

### 6. 자동 구성 도구 추가

- Unity 메뉴에 Day 32 자동 구성 기능 추가
- 분기·합류 Prefab 자동 생성
- 기본 생성 설정 에셋 자동 갱신
- 모듈 Prefab 5종 자동 등록
- `ProceduralMapGenerator`를 기존 테스트 맵과 분리된 `(50, 0, 0)` 위치로 보정
- `GeneratedMap` 아래의 이전 생성 결과 정리

---

## 테스트 추가

### MapGenerationRulesTests

- 동일한 연결부 크기 허용
- 너비가 다른 연결부 차단
- 허용 오차 안의 연결 위치 허용
- 허용 오차 밖의 연결 위치 차단

### MapGenerationGraphRulesTests

- 분기·합류 그래프의 전체 노드 도달 가능 여부 검사
- 끊어진 그래프 차단
- 범위를 벗어난 도착 노드를 가진 간선 차단

### ProceduralMapGeneratorTests

- 고정 시드 기반 8개 모듈 분기 그래프 생성 검사
- 그래프 노드 8개와 간선 8개 구성 검사
- 모든 생성 모듈의 Bounds 비겹침 검사
- 동일 시드 재생성 결과 일치 검사

---

## 변경 파일

### 신규 파일

- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapGenerationGraph.cs`
- `Assets/_ProjectJ/Scripts/Editor/Day32BranchedMapSetupTool.cs`
- `Assets/_ProjectJ/Tests/EditMode/MapGenerationGraphRulesTests.cs`
- `Assets/_ProjectJ/Tests/EditMode/ProceduralMapGeneratorTests.cs`
- `Assets/_ProjectJ/Prefabs/Map/Modules/MAP-004_Branch.prefab`
- `Assets/_ProjectJ/Prefabs/Map/Modules/MAP-005_Merge.prefab`

### 수정 파일

- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapGenerationRules.cs`
- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapGenerationSettings.cs`
- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/ProceduralMapGenerator.cs`
- `Assets/_ProjectJ/Tests/EditMode/MapGenerationRulesTests.cs`
- `Assets/_ProjectJ/Data/Definitions/Map/MAP-GEN-001_DefaultGenerationSettings.asset`
- `Assets/_ProjectJ/Scenes/Game/Game.unity`

---

## 완료 결과

- 선형 맵 생성기를 분기·합류형 맵 생성기로 확장
- 시작 지점에서 모든 생성 모듈로 이어지는 경로 그래프 구성
- 다중 출구·입구를 사용하는 모듈 자동 배치 지원
- 연결부 크기와 위치를 이용한 호환성 검사 적용
- 동일 시드에서 같은 생성 결과를 비교할 수 있는 서명 제공
- 기존 수직 테스트 맵과 절차 생성 맵의 배치 영역 분리
- 생성 규칙·그래프 연결성·고정 시드 재현 테스트 추가

---

## 다음 개발 방향

33일차에는 생성된 전체 맵의 연결성과 실제 이동 가능성을 종합 검증하는 기능을 구현한다. 끊어진 경로, 이동할 수 없는 높이 차이, 잘못된 연결 지점, 겹친 모듈을 자동으로 찾아 생성 실패 원인을 명확하게 출력하는 검증 구조가 필요하다.

---

## 커밋 기록

```text
32일차 : 분기·합류 경로 그래프 및 다중 연결 지점 배치
```

---

## 33일차 : 생성 맵 연결성·이동 가능성 종합 검증

- 작업일: 2026년 8월 6일
- 기준 커밋: `e2f3be8f1d90c37c424c5932f799e387ab752e14`
- 개발 환경: Unity 6, C#

---

## 개발 목표

32일차에 구현한 분기·합류형 절차 생성 맵을 대상으로, 생성 결과가 실제 플레이 가능한 구조인지 자동으로 판정하는 종합 검사 시스템을 구현했다.

단순한 모듈 개수 검사를 넘어 그래프 연결성, 모듈과 노드의 대응, 연결 지점 규격, 이동 가능한 높이 차이, Bounds 겹침을 함께 검사하고 실패 원인을 문제 코드와 관련 노드 번호로 확인할 수 있도록 구성했다.

---

## 주요 구현 내용

### 1. 생성 결과 종합 검사기 구현

- `MapGenerationResultValidator` 정적 검사기 추가
- 생성 모듈·그래프 노드·그래프 간선 통합 검사
- 목표 모듈 수와 실제 생성 수 비교
- 검사 완료 여부와 전체 성공 여부 기록
- 발견된 문제의 종류·설명·관련 노드 번호 저장

### 2. 검사 보고서 구조 추가

- `MapGenerationValidationReport`로 최근 검사 결과 관리
- `IsCompleted`, `IsValid`, `IssueCount`, `Issues` 제공
- 한 줄 요약과 상세 오류 메시지 생성
- 특정 문제 코드 포함 여부 확인 기능 제공
- 검사 실패 내용을 `[ProjectJ][Day33]` 형식으로 Console에 출력

### 3. 그래프 연결성 검사

- 시작 노드에서 모든 생성 노드로 이동 가능한지 확인
- 모듈 수와 그래프 노드 수 일치 여부 확인
- 그래프 노드 번호의 유효성 확인
- 그래프 노드의 모듈 ID와 실제 모듈 ID 비교
- 그래프에 기록된 월드 위치와 실제 모듈 위치 비교
- 잘못되거나 끊어진 그래프 간선 검출

### 4. 연결 지점 종합 검사

- 그래프 간선에 기록된 출구·입구 ID 존재 여부 확인
- `Exit → Entrance` 연결 역할 확인
- 두 연결 지점이 서로 마주 보는 방향인지 확인
- 연결부 너비와 높이의 호환성 확인
- 연결 지점 월드 위치 일치 여부 확인
- 동일 연결 지점의 중복 사용 검출

### 5. 실제 이동 가능성 검사

- 모듈 내부 이동 규격의 유효성 확인
- 연결 지점 사이 상승 높이를 최대 점프 높이와 비교
- 연결 지점 사이 하강 높이를 최대 안전 낙하 높이와 비교
- 플레이어가 이동할 수 없는 높이 차이를 `TraversalHeightExceeded`로 기록

### 6. Bounds 겹침 검사

- 생성된 모든 모듈을 쌍으로 비교
- 허용 오차보다 크게 겹치는 Bounds 검출
- 겹친 두 모듈의 노드 번호와 이름 기록
- 모듈이 겹친 생성 결과를 실패로 판정

### 7. 생성기 성공 기준 강화

- `ProceduralMapGenerator`에 최근 검사 보고서 연결
- 생성 완료 직후 종합 검사 자동 실행
- 종합 검사에 문제가 없어야 최종 생성 성공으로 판정
- Inspector의 `Validate Generated Map` 메뉴로 수동 재검사 지원
- 생성 맵 제거 시 검사 상태와 성공 상태 초기화

---

## 문제 코드

| 문제 코드 | 검출 내용 |
|---|---|
| `GenerationFlowFailed` | 생성 흐름 중단 |
| `EmptyMap` | 생성된 모듈 없음 |
| `TargetModuleCountMismatch` | 목표 모듈 수와 실제 수 불일치 |
| `ModuleNodeCountMismatch` | 모듈 수와 그래프 노드 수 불일치 |
| `MissingModule` | 모듈 참조 누락 |
| `InvalidModuleData` | 모듈 내부 이동 규격 오류 |
| `MissingGraphNode` | 그래프 노드 누락 |
| `InvalidGraphNode` | 그래프 노드 데이터 오류 |
| `NodeModuleMismatch` | 그래프 노드와 실제 모듈 정보 불일치 |
| `InvalidGraphEdge` | 그래프 간선 데이터 오류 |
| `DisconnectedGraph` | 시작점에서 도달할 수 없는 경로 존재 |
| `MissingExitConnection` | 출구 연결 지점 ID 누락 |
| `MissingEntranceConnection` | 입구 연결 지점 ID 누락 |
| `InvalidConnectionRole` | 출구·입구 역할 오류 |
| `ReusedConnection` | 동일 연결 지점 중복 사용 |
| `ConnectionDirectionMismatch` | 연결 방향 불일치 |
| `ConnectionSizeMismatch` | 연결부 크기 불일치 |
| `ConnectionPositionMismatch` | 연결 위치 불일치 |
| `TraversalHeightExceeded` | 플레이어 이동 가능 높이 초과 |
| `ModuleOverlap` | 모듈 Bounds 겹침 |

---

## 테스트 추가

### MapGenerationResultValidatorTests

- 정상 연결 맵의 전체 검사 통과 여부 확인
- 끊어진 그래프의 도달 실패 검출
- 존재하지 않는 연결 지점 ID 검출
- 서로 겹치는 모듈 Bounds 검출
- 이동할 수 없는 높이 차이 검출

### ProceduralMapGeneratorTests

- 생성 직후 종합 검사 완료 상태 확인
- 고정 시드 분기 맵의 종합 검사 성공 확인
- 정상 생성 결과의 발견 문제 `0개` 확인

자동 테스트 코드는 커밋에 포함되어 있다. 실제 Unity Test Runner 실행 결과는 현재 기록에서 확인되지 않았으므로 통과 여부는 별도 확인이 필요하다.

---

## 변경 파일

### 신규 파일

- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapGenerationValidation.cs`
- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapGenerationValidation.cs.meta`
- `Assets/_ProjectJ/Tests/EditMode/MapGenerationResultValidatorTests.cs`
- `Assets/_ProjectJ/Tests/EditMode/MapGenerationResultValidatorTests.cs.meta`

### 수정 파일

- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/ProceduralMapGenerator.cs`
- `Assets/_ProjectJ/Tests/EditMode/ProceduralMapGeneratorTests.cs`
- `README.md`

---

## 완료 결과

- 절차 생성 맵의 종합 검증 시스템 구현
- 생성 모듈과 그래프 데이터의 대응 관계 자동 검사
- 시작점 기준 전체 경로 도달 가능 여부 자동 검사
- 연결 지점 ID·역할·방향·크기·위치 검사 적용
- 연결 지점 중복 사용과 모듈 Bounds 겹침 검출
- 플레이어 이동 능력을 벗어난 높이 차이 검출
- 실패 원인을 문제 코드와 관련 노드 번호로 출력
- 종합 검사 성공 여부를 최종 맵 생성 성공 조건에 포함
- 정상·비정상 생성 결과를 검증하는 EditMode 테스트 추가

---

## 다음 개발 방향

34일차에는 수평 중심의 모듈 생성 구조를 수직 상승형 구조로 확장한다. 입구와 출구의 로컬 Y 높이 차이를 이용하는 상승 모듈을 추가하고, 이전 모듈의 출구와 다음 모듈의 입구가 XYZ 전체 좌표에서 일치하도록 배치 계산을 확장한다.

우선 `MAP-006_StepRise`, `MAP-007_ZigzagRise`, `MAP-008_JumpRise` 상승 모듈을 제작하고, 최종 생성 높이와 최소 상승 모듈 수를 검사할 수 있는 데이터 구조를 마련한다.

---

## 커밋 기록

```text
33일차 : 생성 맵 연결성·이동 가능성 종합 검증
```
