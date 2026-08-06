---

# 30일차 : 맵 모듈 데이터 규격 및 기본 이동 모듈

---

## 오늘의 목표

절차적 맵 생성에 사용할 공통 모듈 데이터 구조를 설계하고, 플레이어의 이동 능력을 기준으로 각 모듈의 통과 가능 여부를 자동 검증할 수 있는 기반을 구축했다.

---

## 구현 내용

### 1. 맵 모듈 공통 데이터 구조

* 맵 모듈의 식별 정보와 유형 관리
* 모듈의 로컬 배치 영역 설정
* 월드 좌표 기준 배치 영역 계산
* 회전과 크기 변화를 반영한 `WorldBounds` 계산
* 모듈 입구와 출구 연결 지점 관리
* 기본 이동, 앉기, 점프 요구 조건 구분

### 2. 모듈 연결 지점

* 북쪽·동쪽·남쪽·서쪽 연결 방향 지원
* 연결 지점의 너비와 높이 설정
* 연결 방향 호환 여부 검사
* Scene 뷰에서 연결 방향과 통로 크기 표시
* 동쪽·서쪽 연결 지점의 기즈모 회전 보정

### 3. 플레이어 이동 프로필

* 서기 높이와 앉기 높이
* Character Controller 반지름
* 이동 속도
* 점프 높이와 중력
* 최대 안전 점프 거리
* 최대 안전 상승 높이
* 최대 안전 낙하 높이 `3m`
* 통로 검증용 여유 공간 값

### 4. 모듈 이동 가능 여부 검증

* 서서 통과할 수 있는 통로 검사
* 앉아서 통과할 수 있는 통로 검사
* 점프 거리와 상승 높이 검사
* 최대 안전 낙하 높이를 초과한 구간 차단
* 잘못된 모듈 데이터와 연결 지점 검사
* 검사 실패 원인 출력

### 5. 기본 이동 모듈 제작

* 기본 직선 통로 모듈
* 앉아서 통과하는 낮은 통로 모듈
* 점프로 통과하는 이동 모듈
* 공통 이동 프로필 데이터 에셋
* Editor 메뉴를 통한 모듈과 데이터 자동 생성

### 6. 프로젝트 물리 설정 보정

* 프로젝트 전용 물리 레이어 규칙 적용
* `Player ↔ Player` 충돌 활성화
* 물리 충돌 행렬과 테스트 기준 통일

### 7. 자동 테스트 보강

* 모듈 연결 방향 호환성 검사
* 서기·앉기 통로 높이 검사
* 점프 거리와 상승 높이 검사
* 과도한 하향 낙하 차단 검사
* 모듈을 90도 회전했을 때의 `WorldBounds` 검사
* 동쪽·서쪽 연결 기즈모 방향 검사
* 프로젝트 물리 레이어와 충돌 규칙 검사

---

## 수정 과정에서 해결한 문제

* 회전된 모듈의 월드 배치 영역 크기가 잘못 계산되던 문제
* 지나치게 높은 낙하 구간이 유효한 점프로 판정되던 문제
* 동쪽·서쪽 연결 지점의 통로 기즈모 방향이 어긋나던 문제
* `Player ↔ Player` 충돌 설정이 비활성화되어 있던 문제
* 이동 프로필 에셋에 최대 안전 낙하 높이가 저장되지 않던 문제

---

## 테스트 결과

* `MapModuleValidationRulesTests` 16개 통과
* `ProjectPhysicsLayerTests` 8개 통과
* 전체 EditMode 테스트 실패 없음
* 회전된 모듈의 배치 영역 정상 표시 확인
* 동쪽·서쪽 연결 지점 기즈모 정상 표시 확인
* 최대 `3m` 낙하 허용 확인
* `3m`를 초과한 낙하 구간 차단 확인

---

## 주요 변경 파일

```text
Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapModuleDefinition.cs
Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapTraversalProfile.cs
Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapModuleValidationRules.cs
Assets/_ProjectJ/Scripts/Runtime/MapGeneration/MapModuleConnectionPoint.cs
Assets/_ProjectJ/Scripts/Editor/Day30MapModuleSetupTool.cs
Assets/_ProjectJ/Tests/EditMode/MapModuleValidationRulesTests.cs
Assets/_ProjectJ/Tests/EditMode/ProjectPhysicsLayerTests.cs
Assets/_ProjectJ/Data/Definitions/Map/MAP-TRV-001_DefaultTraversal.asset
ProjectSettings/DynamicsManager.asset
```

---

## 완료 결과

절차적 맵 생성에서 공통으로 사용할 맵 모듈 규격과 이동 검증 기반을 완성했다. 이제 맵 생성기가 모듈의 연결 방향, 배치 영역, 통로 높이, 점프 거리와 안전 낙하 높이를 기준으로 플레이어가 실제로 통과할 수 있는 모듈만 선택하도록 확장할 수 있다.
