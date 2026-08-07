# Project J 기능 분류표

> 기준 커밋: `daac03295c83305222edcd7fe35e23ba4c97a7b1` — `45일차 : 아이템 28종 통합 테스트 및 밸런스 검증`

46일차에서는 Unity 상단 `Project J` 메뉴를 개발 일차 기준에서 실제 기능 기준으로 재분류한다. 기존 파일명·클래스명·메서드명은 유지하며, 최초 개발 일차는 Git 커밋 기록과 기존 Day 파일명을 기준으로 기록한다.

## 최종 메뉴 구조

```text
Project J
├─ 01. 프로젝트 설정
│  ├─ 씬
│  ├─ 서비스
│  └─ 물리
├─ 02. 플레이어와 입력
│  ├─ 입력
│  ├─ Play Mode
│  └─ 플레이어 설정
├─ 03. 데이터
│  ├─ 기본 데이터
│  ├─ CSV
│  └─ 카탈로그
├─ 04. 테스트
│  └─ 테스트 프레임워크
├─ 05. 빌드
│  └─ 개발 빌드
├─ 06. 맵
│  ├─ 맵 모듈
│  ├─ 맵 생성
│  └─ 검증
├─ 07. 장애물
│  └─ 맵 장애물
├─ 08. 아이템
│  ├─ 인벤토리
│  ├─ 아이템 상자
│  ├─ 효과
│  └─ 통합 검증
└─ 09. UI
   └─ 게임 화면
```

## 기능 목록

| 기능 분류 | 기능 이름 | 한글 설명 | 관련 스크립트 | 최초 개발 일차 | 현재 상태 |
|---|---|---|---|---:|---|
| 프로젝트 설정 / 씬 | 씬 흐름 뼈대 생성 | 기본 게임 씬을 생성·보완하고 Build Settings와 Play Mode 시작 씬을 구성 | Day03SceneSetupTool.cs | 03일차 | 구현 완료 · 메뉴 재분류 |
| 프로젝트 설정 / 서비스 | 공통 서비스 구성 | Bootstrap 씬에 공통 서비스 초기화와 씬 흐름 필수 컴포넌트를 구성 | Day04ServiceSetupTool.cs | 04일차 | 구현 완료 · 메뉴 재분류 |
| 플레이어와 입력 / 입력 | 입력 디버그 구성 | Input Action 에셋과 Tests 씬 입력 디버그 오브젝트를 검사·구성 | Day05InputSetupTool.cs | 05일차 | 구현 완료 · 메뉴 재분류 |
| 플레이어와 입력 / Play Mode | Tests 씬 시작 설정 | Tests 씬을 Play Mode 시작 씬으로 지정 | Day05InputSetupTool.cs | 05일차 | 구현 완료 · 메뉴 재분류 |
| 플레이어와 입력 / Play Mode | Bootstrap 씬 시작 복원 | Play Mode 시작 씬을 Bootstrap으로 복원 | Day05InputSetupTool.cs | 05일차 | 구현 완료 · 메뉴 재분류 |
| 데이터 / 기본 데이터 | 샘플 데이터 에셋 생성 | Player·Map·Obstacle·Item·Cosmetic·Audio 기본 데이터 에셋 생성 | Day06DataSetupTool.cs | 06일차 | 구현 완료 · 메뉴 재분류 |
| 데이터 / 기본 데이터 | 전체 데이터 에셋 검증 | 프로젝트 공통 데이터 ID와 필수 값을 전체 검증 | Day06DataSetupTool.cs, Data/ProjectDataAssetDatabase.cs | 06일차 | 구현 완료 · 메뉴 재분류 |
| 플레이어와 입력 / 플레이어 설정 | 기본 플레이어 설정 구성 | PLY-001 기본 이동·달리기·앉기·점프·중력·스태미나 설정 적용 | Day07PlayerSettingsSetupTool.cs | 07일차 | 구현 완료 · 메뉴 재분류 |
| 플레이어와 입력 / 플레이어 설정 | 기본 플레이어 설정 선택 | PLY-001 기본 플레이어 데이터 에셋 선택 | Day07PlayerSettingsSetupTool.cs | 07일차 | 구현 완료 · 메뉴 재분류 |
| 프로젝트 설정 / 물리 | 물리 레이어 구성 | Project J 전용 3D 물리 레이어 이름과 충돌 행렬 구성 | Day08PhysicsLayerSetupTool.cs, Physics/ProjectPhysicsLayerEditorUtility.cs | 08일차 | 구현 완료 · 메뉴 재분류 |
| 프로젝트 설정 / 물리 | 물리 레이어 검증 | 현재 레이어 이름과 충돌 행렬을 코드 규칙과 비교 검증 | Day08PhysicsLayerSetupTool.cs, Physics/ProjectPhysicsLayerEditorUtility.cs | 08일차 | 구현 완료 · 메뉴 재분류 |
| 테스트 / 테스트 프레임워크 | 테스트 프레임워크 구성 | EditMode·PlayMode asmdef와 Tests 씬 기본 구조 구성 | Day09TestFrameworkSetupTool.cs | 09일차 | 구현 완료 · 메뉴 재분류 |
| 테스트 / 테스트 프레임워크 | 테스트 프레임워크 검증 | 테스트 어셈블리와 Tests 씬 필수 구성을 검증 | Day09TestFrameworkSetupTool.cs | 09일차 | 구현 완료 · 메뉴 재분류 |
| 빌드 / 개발 빌드 | 개발 Build Profile 구성 | Windows 개발 Build Profile과 디버깅 옵션·씬 목록 구성 | Day10DevelopmentBuildTool.cs, Build/ProjectDevelopmentBuildValidator.cs | 10일차 | 구현 완료 · 메뉴 재분류 |
| 빌드 / 개발 빌드 | 개발 Build Profile 검증 | 개발 Build Profile의 씬·Define·빌드 옵션 검증 | Day10DevelopmentBuildTool.cs, Build/ProjectDevelopmentBuildValidator.cs | 10일차 | 구현 완료 · 메뉴 재분류 |
| 빌드 / 개발 빌드 | 개발 클라이언트 빌드 | 검증된 개발 Profile로 Windows 개발 클라이언트 빌드 | Day10DevelopmentBuildTool.cs | 10일차 | 구현 완료 · 메뉴 재분류 |
| 빌드 / 개발 빌드 | 개발 클라이언트 빌드 후 실행 | 개발 클라이언트를 빌드한 뒤 실행 | Day10DevelopmentBuildTool.cs | 10일차 | 구현 완료 · 메뉴 재분류 |
| 빌드 / 개발 빌드 | 최신 빌드 요약 열기 | 가장 최근 개발 빌드 요약 로그 열기 | Day10DevelopmentBuildTool.cs | 10일차 | 구현 완료 · 메뉴 재분류 |
| 데이터 / CSV | 데이터 CSV 템플릿 생성 | ProjectData.csv 기본 템플릿 생성 | Data/ProjectDataCsvImporter.cs | 17일차 | 구현 완료 · 메뉴 재분류 |
| 데이터 / CSV | 프로젝트 데이터 CSV 가져오기 | 통합 CSV를 프로젝트 ScriptableObject 데이터에 적용 | Data/ProjectDataCsvImporter.cs | 17일차 | 구현 완료 · 메뉴 재분류 |
| 데이터 / 카탈로그 | 런타임 데이터 카탈로그 재생성 | 전체 데이터 에셋을 런타임 카탈로그에 재등록하고 검증 | Data/ProjectDataCatalogBuilder.cs | 17일차 | 구현 완료 · 메뉴 재분류 |
| 맵 / 맵 모듈 | 기본 맵 모듈 생성 | 이동 기준과 고정·낮은 통로·점프 간격 기본 모듈 생성 | Day30MapModuleSetupTool.cs | 30일차 | 구현 완료 · 메뉴 재분류 |
| 맵 / 맵 생성 | 절차적 맵 생성기 구성 | 기본 생성 설정과 Scene 절차적 맵 생성기 구성 | Day31ProceduralMapSetupTool.cs | 31일차 | 구현 완료 · 메뉴 재분류 |
| 맵 / 맵 생성 | 분기·합류 맵 생성 구성 | 분기·합류 모듈과 다중 연결 경로 생성 설정 구성 | Day32BranchedMapSetupTool.cs | 32일차 | 구현 완료 · 메뉴 재분류 |
| 맵 / 맵 모듈 | 수직 상승 모듈 생성 | 계단·지그재그·점프 수직 상승 모듈 3종 생성 | Day34VerticalMapModuleSetupTool.cs | 34일차 | 구현 완료 · 메뉴 재분류 |
| 맵 / 맵 생성 | 수직 맵 생성 구성 | 8종 모듈을 사용하는 목표 높이 기반 수직 생성 설정 구성 | Day35VerticalGenerationSetupTool.cs | 35일차 | 구현 완료 · 메뉴 재분류 |
| 맵 / 맵 생성 | 수직 분기 맵 생성 구성 | 수직 분기와 합류를 포함하는 생성 설정 구성 | Day36VerticalBranchGenerationSetupTool.cs | 36일차 | 구현 완료 · 메뉴 재분류 |
| 맵 / 검증 | 플레이 가능 경로 검증 구성 | 생성 맵 경로 검사와 디버그 시각화 구성 | Day37MapPlayabilitySetupTool.cs | 37일차 | 구현 완료 · 메뉴 재분류 |
| 장애물 / 맵 장애물 | 분기 장애물 구성 | 위험도 기반 분기 장애물 데이터·Prefab·생성 계획 구성 | Day38BranchObstacleSetupTool.cs | 38일차 | 구현 완료 · 메뉴 재분류 |
| 아이템 / 인벤토리 | 2슬롯 인벤토리와 테스트 상자 구성 | 기본 아이템 데이터와 플레이어 2슬롯 인벤토리·테스트 상자 구성 | Day39ItemInventorySetupTool.cs | 39일차 | 구현 완료 · 메뉴 재분류 |
| UI / 게임 화면 | Game Scene Canvas UI 구성 | HUD·아이템 슬롯·ESC 메뉴·결과 화면을 Canvas 기반으로 구성 | Day40CanvasUISetupTool.cs | 40일차 | 구현 완료 · 메뉴 재분류 |
| 아이템 / 아이템 상자 | 아이템 상자와 설치 위치 검사 구성 | 절차 맵용 아이템 상자 생성기와 공통 설치 위치 검사기 구성 | Day41ItemChestPlacementSetupTool.cs | 41일차 | 구현 완료 · 메뉴 재분류 |
| 아이템 / 효과 | 아이템 28종 데이터와 P0 효과 구성 | 28종 데이터 기준과 P0 아이템 10종 사용 시스템 구성 | Day42ItemSystemSetupTool.cs | 42일차 | 구현 완료 · 메뉴 재분류 |
| 아이템 / 효과 | P1 아이템 11종 효과 구성 | P1 아이템 11종 데이터와 효과 시스템 구성 | Day43P1ItemSetupTool.cs | 43일차 | 구현 완료 · 메뉴 재분류 |
| 아이템 / 효과 | P2 아이템 7종 효과 구성 | P2 아이템 7종 데이터와 효과 시스템 구성 | Day44P2ItemSetupTool.cs | 44일차 | 구현 완료 · 메뉴 재분류 |
| 아이템 / 통합 검증 | 아이템 28종 통합 검증 | 28종 수량·ID·효과·우선순위·가중치 구조를 통합 검증 | Day45ItemIntegrationValidationTool.cs | 45일차 | 구현 완료 · 메뉴 재분류 |
| 아이템 / 통합 검증 | 아이템 밸런스 기준 CSV 내보내기 | 현재 28종 수치와 등장 확률을 수동 검증용 CSV로 출력 | Day45ItemIntegrationValidationTool.cs | 45일차 | 구현 완료 · 메뉴 재분류 |
| 프로젝트 관리 | Editor 메뉴 공통 경로 | Project J Editor 메뉴 9개 대분류와 하위 경로를 단일 상수 파일에서 관리 | ProjectJEditorMenuPaths.cs | 46일차 | 신규 구현 |
| 테스트 / 테스트 프레임워크 | Editor 메뉴 분류 회귀 검증 | Editor 소스에 구형 Project J/Day 경로가 다시 추가되지 않는지 자동 검사 | EditorMenuClassificationTests.cs | 46일차 | 신규 구현 |

## 46일차 규칙

- 기존 `Project J/Day XX/...` 상단 메뉴는 모두 제거
- 기존 스크립트 파일명·클래스명·메서드명 유지
- 기존 `.meta` GUID 유지
- 메뉴 문자열만 기능별 공통 경로로 변경
- 파일 이동과 폴더 재구성은 47~48일차에서 진행
- 전체 컴파일·참조·메뉴·테스트·빌드 최종 검증은 49일차에서 진행

