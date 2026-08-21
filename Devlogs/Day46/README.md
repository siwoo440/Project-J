# Project J 개발 일지

## 46일차 : Module 장애물 Safe Volume 및 No Spawn 영역 구현

### 개발 목표

정육면체 Module 내부에서 장애물을 안전하게 배치할 수 있도록 공통 설치 가능 영역과 설치 금지 영역을 정의한다.

특정 플랫폼이나 장애물에 종속되지 않고 이후 추가되는 모든 장애물이 동일한 배치 검증 규칙을 사용할 수 있도록 기반 구조를 구성한다.

### 구현 내용

- `MapObstaclePlacementVolume` 추가
  - 장애물 설치 가능 영역인 `Safe` 정의
  - 장애물 설치 금지 영역인 `NoSpawn` 정의
  - Scene View에서 영역을 Wire Cube로 표시
  - Safe 영역은 초록색 선으로 표시
  - No Spawn 영역은 빨간색 선으로 표시
  - Transform 위치·회전·Scale을 이용해 영역을 직접 편집할 수 있도록 구성

- `MapObstaclePlacementValidator` 추가
  - 장애물 전체 Collider Bounds 계산
  - Collider가 없는 경우 Renderer Bounds 사용
  - 장애물 전체 Bounds가 Safe Volume 내부에 포함되는지 검사
  - Safe Volume을 벗어나면 배치 거부
  - No Spawn Volume과 겹치면 배치 거부
  - 이후 추가되는 장애물도 GameObject 또는 Bounds만 전달해 동일한 검증 사용 가능

- `MapObstaclePlacementResult` 추가
  - 배치 허용 여부 저장
  - 배치 거부 원인 구분
  - Module 누락
  - Bounds 누락
  - Safe Volume 누락
  - Safe Volume 이탈
  - No Spawn Volume 침범

- `Day46ModuleSafeVolumeSetup` Editor 도구 추가
  - Day25 Module Prefab을 대상으로 Safe Volume 자동 생성
  - 열린 Socket 주변에 No Spawn Volume 자동 생성
  - 기존 `Gameplay/ObstacleSpawnAreas` 구조 재사용
  - 기존 `Gameplay/NoSpawnAreas` 구조 재사용
  - 선택한 위치에 Safe Volume을 추가하는 Editor 메뉴 제공
  - 선택한 위치에 No Spawn Volume을 추가하는 Editor 메뉴 제공

- 기존 Module Prefab 적용
  - Branch Module
  - Corner Module
  - Drop Module
  - Merge Module
  - Start Module
  - Straight Module
  - Vertical Module

### 테스트 코드

`MapObstaclePlacementVolumeTests`를 추가해 다음 규칙을 검증할 수 있도록 구성했다.

- Safe Volume 내부 장애물 배치 허용
- Safe Volume 외부 장애물 배치 거부
- No Spawn Volume 침범 장애물 배치 거부
- 이후 추가되는 장애물이 Collider Bounds 기반 공통 검증을 사용할 수 있는지 확인

### 현재 구조

```text
Module
└─ Gameplay
   ├─ ObstacleSpawnAreas
   │  └─ Safe Volume
   │
   ├─ ItemSpawnAreas
   │
   └─ NoSpawnAreas
      ├─ Entrance / Exit 주변
      ├─ 열린 Socket 주변
      └─ 추가 수동 금지 영역
```

### 개발 결과

장애물 자체의 종류와 관계없이 Module 내부의 설치 가능 범위를 공통 규칙으로 검사할 수 있는 기반을 완성했다.

Scene View에서 설치 가능 범위와 설치 금지 범위를 선으로 직접 확인하고 위치와 크기를 조정할 수 있게 되어 이후 장애물 제작 시 배치 가능 범위를 시각적으로 관리할 수 있다.

이 구조는 이후 고정맵 장애물 배치뿐 아니라 절차 생성 단계의 장애물 Spawn Slot 및 Safe Volume 검증에도 재사용할 수 있다.

### 확인 사항

GitHub 최신 커밋 기준으로 Safe Volume Runtime 코드, Editor 설정 도구, Module Prefab 반영, EditMode 테스트 코드의 참조 구조에서 명확한 오류는 확인되지 않았다.

GitHub Actions 상태 검사가 등록되어 있지 않아 Unity Editor의 실제 컴파일 및 Test Runner 실행 결과는 저장소에서 자동 확인할 수 없다.

### 다음 개발 방향

47일차에는 버튼, 문, 승강기 등 경기장 장치에 공통으로 사용할 `F` 상호작용 구조를 구현한다.

가장 가까운 유효 대상 하나만 선택하고 상호작용 범위를 벗어난 대상은 실행되지 않도록 공통 인터페이스와 플레이어 상호작용 Controller를 구성한다.
