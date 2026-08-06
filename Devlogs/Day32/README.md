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
