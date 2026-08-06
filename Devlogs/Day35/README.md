# 프로젝트 J 개발일지

---

## 35일차 : XYZ 수직 연결 및 목표 높이 기반 선형 생성

- 작업일: 2026년 8월 6일
- 기준 커밋: `a9a09c8d45bda520290fd84f33bc1c6cdc052453`
- 이전 커밋: `7c77cd617e99ed0bb497ccbb75f8c2680ea91c5f`
- 개발 환경: Unity 6, C#

---

## 개발 목표

34일차에 제작한 수직 연결 데이터와 상승 모듈 3종을 절차 생성기에 연결하고, 모듈의 입구와 출구를 XYZ 공간에서 정확하게 정렬하는 수직 선형 맵 생성을 구현했다.

정해진 모듈 수 안에서 목표 높이와 최소 상승 모듈 수를 달성하도록 후보를 선택하고, 같은 시드에서는 같은 수직 구조가 다시 생성되도록 구성했다.

---

## 주요 구현 내용

### 1. 수직 생성 설정 확장

`MapGenerationSettings`에 다음 설정을 추가했다.

| 설정 | 적용값 | 용도 |
|---|---:|---|
| Use Vertical Generation | 활성화 | 수직 선형 생성 사용 |
| Minimum Target Height | `8m` | 최소 목표 높이 |
| Maximum Target Height | `16m` | 최대 목표 높이 |
| Minimum Ascending Modules | `3` | 최소 상승 모듈 수 |
| Maximum Consecutive Flat Modules | `2` | 연속 평지 모듈 제한 |
| Allow Descending Modules | 비활성화 | 하강 모듈 생성 차단 |
| Module Count | `8` | 생성 모듈 수 |
| Maximum Placement Attempts | `128` | 최대 배치 시도 횟수 |
| Seed | `35001` | 고정 생성 시드 |

수직 생성이 활성화된 동안에는 35일차 범위에 맞춰 분기 경로를 비활성화하고 선형 경로를 생성한다.

### 2. 수직 생성 규칙 구현

`MapVerticalGenerationRules`를 추가해 다음 내용을 판정하도록 구성했다.

- 모듈별 예상 상승량 계산
- 상승 모듈 여부 판정
- 후보 목록의 최대 상승량 계산
- 설정된 목표 높이의 달성 가능 여부 검사
- 남은 슬롯을 이용한 최소 상승 모듈 수 달성 가능 여부 검사
- 연속 평지 모듈 제한 검사
- 하강 모듈 허용 여부 검사
- 최종 높이와 상승 모듈 수 결과 검사

후보를 배치한 뒤 실패를 확인하는 방식이 아니라, 현재 후보를 선택한 뒤 남은 슬롯으로 목표를 달성할 수 있는지 먼저 계산하여 불가능한 후보를 제외한다.

### 3. XYZ 연결 정렬

이전 모듈의 `Exit`와 다음 모듈의 `Entrance`가 월드 공간의 X, Y, Z 좌표에서 같은 위치에 오도록 배치 방식을 확장했다.

- 출구와 입구의 월드 위치 정렬
- 연결 방향과 회전값 반영
- 상승 모듈의 출구 높이를 다음 모듈 배치에 누적
- 연결 위치 허용 오차 `0.02m` 적용
- 실제 시작 입구와 마지막 출구의 높이 차이 계산

### 4. 목표 높이 기반 후보 선택

생성기는 현재 높이, 현재 상승 모듈 수, 연속 평지 수와 남은 슬롯을 함께 사용해 후보를 선택한다.

다음 조건을 만족할 수 없는 후보는 배치 전에 제외된다.

- 최종 목표 높이에 도달할 수 없음
- 최소 상승 모듈 수를 채울 수 없음
- 연속 평지 제한을 초과함
- 하강이 금지된 상태에서 높이가 내려감

### 5. 생성 결과 정보 확장

`ProceduralMapGenerator`에서 다음 수직 생성 결과를 확인할 수 있도록 확장했다.

- 이번 생성의 목표 높이
- 실제 생성 높이
- 배치된 상승 모듈 수
- 관찰된 최대 연속 평지 모듈 수
- 동일 시드 결과 비교용 생성 서명
- 수직 생성 규칙을 포함한 최종 성공 여부

### 6. 자동 설정 도구 추가

`Day35VerticalGenerationSetupTool`을 추가해 Unity 메뉴에서 수직 생성 설정을 적용할 수 있도록 구성했다.

```text
Project J
└── Day 35
    └── Configure Vertical Generation
```

이 도구는 `MAP-001`부터 `MAP-008`까지 모듈 8종을 생성 후보에 등록하고, 상승 모듈 3종의 수직 데이터가 유효한지 확인한 뒤 35일차 권장 설정을 저장한다.

---

## 테스트 추가

### MapVerticalGenerationRulesTests

수직 생성 규칙을 검증하는 EditMode 테스트 7개를 추가했다.

- 달성 가능한 수직 설정 허용
- 상승 후보가 없는 설정 차단
- 목표 높이 달성을 불가능하게 만드는 후보 차단
- 최소 상승 모듈 수를 채울 수 없는 후보 차단
- 세 번째 연속 평지 모듈 차단
- 목표에 도달하는 마지막 상승 후보 허용
- 목표 높이에 미달한 최종 결과 차단

### ProceduralVerticalMapGeneratorTests

수직 생성기를 통합 검증하는 EditMode 테스트 3개를 추가했다.

- 고정 시드의 목표 높이와 최소 상승 모듈 수 달성 확인
- 모든 연결 지점의 XYZ 위치 일치 확인
- 같은 시드에서 같은 수직 생성 서명 재현 확인

저장소에는 테스트 코드가 포함되어 있지만, Unity Test Runner의 실제 실행 및 통과 기록은 커밋에서 확인되지 않았다.

---

## 변경 파일

### 신규 파일

- `Assets/_ProjectJ/Scripts/Editor/Day35VerticalGenerationSetupTool.cs`
- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapVerticalGenerationRules.cs`
- `Assets/_ProjectJ/Tests/EditMode/MapVerticalGenerationRulesTests.cs`
- `Assets/_ProjectJ/Tests/EditMode/ProceduralVerticalMapGeneratorTests.cs`
- 각 신규 스크립트의 `.meta` 파일

### 수정 파일

- `Assets/_ProjectJ/Data/Definitions/Map/MAP-GEN-001_DefaultGenerationSettings.asset`
- `Assets/_ProjectJ/Scenes/Game/Game.unity`
- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapGenerationSettings.cs`
- `Assets/_ProjectJ/Scripts/Runtime/MapGeneration/ProceduralMapGenerator.cs`

---

## 완료 결과

- 상승 모듈 3종을 실제 절차 생성 후보에 등록
- 모듈 연결 지점의 XYZ 전체 정렬 구현
- 목표 높이 `8~16m` 기반 수직 생성 설정 적용
- 최소 상승 모듈 3개 보장 규칙 구현
- 연속 평지 모듈 최대 2개 제한 구현
- 목표 달성이 불가능한 후보의 사전 제외 구현
- 수직 생성 결과 높이와 통계 정보 제공
- 동일 시드 수직 구조 재현 기능 유지
- 수직 생성 규칙 테스트 7개와 통합 테스트 3개 추가

---

## 확인이 필요한 항목

- Unity Console 컴파일 오류 여부
- EditMode 전체 테스트 통과 여부
- 실제 생성 결과의 모듈 8개 배치 여부
- 최종 생성 높이의 목표 높이 도달 여부
- 상승 모듈 3개 이상 배치 여부
- 연결 지점 XYZ 오차 `0.02m` 이하 여부
- 플레이어가 상승 모듈을 실제로 통과할 수 있는지
- Scene 변경량이 큰 만큼 기존 오브젝트와 설정이 의도대로 유지되는지

---

## 다음 개발 방향

36일차에는 수직 선형 생성 규칙을 분기·합류 구조로 확장한다. 좌우 분기의 누적 높이를 각각 계산하고, 두 경로가 같은 높이에서 합류할 수 있도록 후보 조합과 재시도 규칙을 구현해야 한다.

---

## 커밋 기록

```text
35일차 : XYZ 수직 연결 및 목표 높이 기반 선형 생성
```

커밋 SHA: `a9a09c8d45bda520290fd84f33bc1c6cdc052453`
