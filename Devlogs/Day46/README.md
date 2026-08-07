# Project J 개발 일지

## 46일차 : Unity Editor 메뉴 재분류 및 한국어 기능 목록 작성

### 개발 목표

기존에 개발 일차별로 분산되어 있던 `Project J/Day XX/...` 형태의 Unity Editor 메뉴를 실제 기능 기준으로 재분류하고, 프로젝트에서 사용하는 Editor 기능을 한글 문서로 정리했습니다.

이번 작업에서는 기존 Runtime 기능이나 게임 로직을 변경하지 않고, Editor 도구의 접근성과 유지보수성을 높이는 데 집중했습니다.

### 주요 작업 내용

#### 1. Unity Editor 메뉴 기능별 재분류

기존 Day 기준 메뉴를 다음 9개 기능 대분류로 정리했습니다.

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

기존 Editor 도구의 파일명, 클래스명, 메서드명은 유지하고 `[MenuItem]`에서 사용하는 메뉴 경로만 변경했습니다.

#### 2. 공통 Editor 메뉴 경로 관리

새로운 `ProjectJEditorMenuPaths.cs`를 추가했습니다.

각 Editor 도구가 직접 전체 메뉴 경로 문자열을 작성하지 않고 공통 상수를 참조하도록 변경해, 이후 메뉴 구조를 수정하거나 기능을 추가할 때 일관된 구조를 유지할 수 있도록 했습니다.

주요 분류는 다음과 같습니다.

- 프로젝트 설정
- 플레이어와 입력
- 데이터
- 테스트
- 빌드
- 맵
- 장애물
- 아이템
- UI

#### 3. 기존 Day 메뉴 일괄 변환

기존 Editor 스크립트에 남아 있던 `Project J/Day XX/...` 메뉴를 기능별 메뉴로 변경했습니다.

최신 45일차 코드 기준으로 기존 Editor 스크립트 25개의 메뉴 경로 37개를 대상으로 작업했습니다.

변경 예시는 다음과 같습니다.

```text
Project J/Day 03/Create Scene Flow Skeleton
→ Project J/01. 프로젝트 설정/씬/씬 흐름 뼈대 생성 (Day 03일차)
```

```text
Project J/Day 17/Import Data CSV
→ Project J/03. 데이터/CSV/프로젝트 데이터 CSV 가져오기 (Day 17일차)
```

```text
Project J/Day 42/Configure 28 Items And P0 Effects
→ Project J/08. 아이템/효과/아이템 28종 데이터와 P0 효과 구성 (Day 42일차)
```

```text
Project J/Day 45/Validate 28 Item Integration
→ Project J/08. 아이템/통합 검증/아이템 28종 통합 검증 (Day 45일차)
```

기능 이름 뒤에는 기존 기능의 최초 구현 시점을 확인할 수 있도록 `(Day XX일차)` 표기를 유지했습니다.

#### 4. 메뉴 변환 안전 검사 추가

`Day46EditorMenuReclassificationTool.cs`를 이용해 기존 메뉴 문자열이 예상한 최신 코드와 정확히 일치하는 경우에만 메뉴 경로를 변경하도록 구성했습니다.

일부 파일만 잘못 수정되는 상황을 막기 위해 전체 대상 파일과 문자열을 먼저 검사한 뒤 일괄 적용하는 방식으로 처리했습니다.

변환 완료 후에는 Editor 스크립트에 구형 `Project J/Day XX/...` 메뉴가 남아 있는지도 다시 검사했습니다.

일회성 변환 작업이 정상적으로 완료된 후 해당 변환 도구는 제거되도록 구성했습니다.

#### 5. 한국어 기능 분류 문서 작성

다음 문서를 추가했습니다.

```text
Assets/_ProjectJ/Documentation/ProjectJ_기능_분류표.md
```

문서에는 현재 Editor 기능을 다음 기준으로 정리했습니다.

- 기능 분류
- 기능 이름
- 한글 설명
- 관련 스크립트
- 최초 개발 일차
- 현재 상태

이를 통해 이후 개발 과정에서 각 Editor 도구의 역할과 위치를 빠르게 확인할 수 있도록 했습니다.

#### 6. Editor 메뉴 회귀 테스트 추가

다음 EditMode 테스트를 추가했습니다.

```text
Assets/_ProjectJ/Tests/EditMode/EditorMenuClassificationTests.cs
```

추가한 주요 검증 항목은 다음과 같습니다.

- Editor 스크립트에 구형 `Project J/Day XX/...` 메뉴가 남아 있지 않은지 검사
- `ProjectJEditorMenuPaths.cs`에 9개 기능 대분류가 모두 존재하는지 검사

앞으로 새로운 Editor 도구를 추가하면서 다시 Day 기준 메뉴를 작성하거나 메뉴 대분류를 실수로 제거하는 문제를 자동으로 발견할 수 있게 했습니다.

### 생성된 주요 파일

```text
Assets/_ProjectJ/Scripts/Editor/ProjectJEditorMenuPaths.cs
Assets/_ProjectJ/Tests/EditMode/EditorMenuClassificationTests.cs
Assets/_ProjectJ/Documentation/ProjectJ_기능_분류표.md
```

46일차 적용 과정에서 사용한 `Day46EditorMenuReclassificationTool.cs`는 메뉴 변환을 위한 일회성 도구로 사용했습니다.

### 기존 기능 보존

이번 작업에서는 다음 항목을 변경하지 않았습니다.

- Runtime 게임 로직
- 기존 Editor 도구의 클래스명
- 기존 Editor 도구의 메서드명
- 기존 스크립트 파일명
- 기존 `.meta` GUID
- Scene 구성
- Prefab 구성
- 데이터 에셋 구조

따라서 이번 일차의 핵심 변경 범위는 Editor 메뉴 구조와 관리 문서입니다.

### 테스트 및 확인

46일차 작업 완료 후 다음 항목을 확인했습니다.

- Unity 상단 `Project J` 메뉴의 9개 기능 대분류 확인
- 기존 `Project J/Day XX/...` 메뉴 제거 확인
- 공통 메뉴 경로 파일 적용 확인
- 한국어 기능 분류 문서 생성 확인
- EditMode 메뉴 분류 회귀 테스트 확인
- 기존 EditMode 테스트 확인
- 기존 PlayMode 테스트 확인
- Unity Console 컴파일 오류 확인

### 46일차 결과

기존에는 Editor 도구를 찾기 위해 어느 개발 일차에 구현했는지 기억해야 했지만, 이제는 `맵`, `아이템`, `데이터`, `빌드`, `UI`와 같은 실제 기능을 기준으로 도구를 찾을 수 있게 되었습니다.

또한 공통 메뉴 경로와 회귀 테스트를 추가하여 이후 Editor 기능이 늘어나더라도 현재의 메뉴 구조를 일관되게 유지할 수 있는 기반을 마련했습니다.

### 다음 개발 방향

47일차부터는 메뉴 구조 정리에 이어 실제 스크립트 폴더 구조를 기능별로 정리합니다.

우선 Runtime 및 Data 스크립트를 기능 기준 폴더로 통합하며, Unity의 `.meta` GUID와 Scene·Prefab 참조가 유지되도록 파일 이동을 진행합니다.

---

## 46일차 커밋

```text
46일차 : Unity Editor 메뉴 재분류 및 한국어 기능 목록 작성
```
