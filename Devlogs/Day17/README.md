---

# Project J 17일차 개발일지

---

## 개발 목표

Google Sheets에서 내려받은 CSV 데이터를 Unity의 데이터 에셋으로 가져오고, 잘못된 데이터가 게임 실행이나 빌드에 포함되지 않도록 검증 체계를 구축했다.

데이터 변경 시 런타임 카탈로그를 자동으로 갱신하며, 치명적인 오류가 발견되면 MainMenu 진입을 중단하고 오류 안내 화면을 표시하도록 구성했다.

---

## 주요 구현 내용

### 1. CSV 데이터 가져오기

- `Category`, `DataId`, `DisplayName`, `Version` 공통 열을 사용하는 CSV 형식 정의
- Unity 메뉴에서 기본 CSV 템플릿을 생성하는 기능 추가
- CSV를 검사한 뒤 ScriptableObject를 생성하거나 갱신하는 가져오기 기능 추가
- 같은 ID의 기존 데이터가 있으면 표시 이름과 버전 갱신
- 새로운 ID라면 분류에 맞는 데이터 에셋 생성
- 오류가 하나라도 발견되면 전체 가져오기 취소
- CSV에 없는 기존 데이터 에셋은 유지

### 2. CSV 사전 검증

- CSV 머리글 이름과 순서 검사
- 필수 열 개수 검사
- 지원하지 않는 데이터 분류 검사
- 분류와 데이터 ID 접두사 일치 여부 검사
- ID 번호 `001~999` 범위 검사
- CSV 내부 중복 ID 검사
- 빈 표시 이름 검사
- 데이터 버전 형식 검사

### 3. 런타임 데이터 카탈로그

- 전체 데이터 에셋을 보관하는 `ProjectDataCatalog` 추가
- 데이터 에셋 변경 시 카탈로그 자동 재구성
- `Resources/ProjectDataCatalog.asset`을 통한 런타임 로드
- 데이터 ID를 이용한 형식별 조회 기능 추가
- Player, Map, Obstacle, Item, Cosmetic, Audio 필수 분류 검사

### 4. 런타임 치명 오류 차단

- Bootstrap 초기화 단계에서 전체 데이터 카탈로그 검증
- 데이터 오류 발생 시 서비스 초기화 실패 처리
- 초기화 실패 시 MainMenu 전환 차단
- 오류 원인과 Console 확인 안내를 표시하는 `FatalErrorScreen` 추가
- 치명 오류 화면 표시 시 마우스 커서 잠금 해제
- 별도의 Canvas나 TextMeshPro 설정 없이 오류 화면 자동 생성

### 5. 에디터 및 빌드 검증

- 데이터 에셋 변경 감지와 자동 카탈로그 갱신 기능 추가
- Unity 에디터에서 전체 데이터 검증 기능 확장
- Windows 빌드 직전 카탈로그 재구성 및 데이터 검증 실행
- 잘못된 데이터가 발견되면 빌드 중단
- 런타임, 에디터, 빌드 전 단계로 이어지는 3단계 검증 구조 완성

### 6. 자동 테스트

- 필수 데이터 분류가 모두 존재할 때 검증 성공 확인
- 필수 데이터 분류 누락 시 오류 발생 확인
- 카탈로그의 데이터 에셋 참조 보관 확인
- 기존 데이터 검증 및 로그 테스트와 함께 EditMode 테스트 진행

`ProjectLogTests.ExpectedErrorLogDoesNotFailTest`에서 출력된 다음 로그는 오류 출력 기능을 검사하기 위해 의도적으로 발생시킨 정상 테스트 로그다.

```text
[ProjectJ][Test][TEST_EXPECTED_ERROR] Expected test error.
```

해당 테스트가 `Passed` 상태라면 실제 프로젝트 오류로 처리하지 않는다.

---

## 추가 및 수정 파일

### 새로 추가한 파일

| 파일 | 역할 |
|---|---|
| `ProjectDataCatalog.cs` | 런타임 데이터 에셋 목록 관리 |
| `FatalErrorScreen.cs` | Bootstrap 치명 오류 안내 화면 |
| `ProjectDataCatalogBuilder.cs` | 데이터 카탈로그 생성 및 갱신 |
| `ProjectDataCsvImporter.cs` | CSV 검증과 데이터 에셋 가져오기 |
| `ProjectDataBuildValidator.cs` | 빌드 전 데이터 검증과 차단 |
| `ProjectDataCatalogTests.cs` | 데이터 카탈로그 EditMode 테스트 |

### 수정한 파일

| 파일 | 변경 내용 |
|---|---|
| `ProjectDataValidator.cs` | 필수 분류 및 카탈로그 검증 추가 |
| `DataValidationService.cs` | 카탈로그 로드, 런타임 검증, ID 조회 추가 |
| `CommonServiceInitializer.cs` | 서비스 초기화 실패 원인 전달 |
| `BootstrapEntryPoint.cs` | 치명 오류 시 MainMenu 전환 차단 |
| `ProjectDataAssetDatabase.cs` | 카탈로그 기준 에디터 검증 적용 |
| `ProjectDataAssetPostprocessor.cs` | 데이터 변경 시 카탈로그 자동 갱신 |

---

## 데이터 처리 흐름

```text
Google Sheets
→ ProjectData.csv
→ CSV 전체 사전 검증
→ ScriptableObject 생성·갱신
→ ProjectDataCatalog 자동 구성
→ Editor·Build·Runtime 검증
→ 정상일 때만 MainMenu 진입
```

---

## 테스트 및 확인 내용

- CSV 템플릿 생성 확인
- 정상 CSV 가져오기 확인
- 기존 데이터 갱신 및 신규 데이터 생성 확인
- 잘못된 CSV의 부분 적용 차단 확인
- 런타임 데이터 카탈로그 생성 및 자동 갱신 확인
- 필수 데이터 분류 누락 감지 확인
- 중복 ID와 잘못된 버전 감지 확인
- Bootstrap 데이터 검증과 MainMenu 진입 차단 확인
- 치명 오류 안내 화면 표시 확인
- 잘못된 데이터가 포함된 빌드 차단 확인
- EditMode 테스트 통과 확인
- 기존 플레이 기능 회귀 테스트 확인
- Windows 개발 빌드 실행 확인

---

## 완료 결과

17일차에는 외부 시트 데이터를 Unity 프로젝트로 안전하게 가져오는 기반을 완성했다. 데이터 오류는 가져오기 전, 에디터 작업 중, 빌드 전, 게임 실행 시점에 각각 감지되며, 치명적인 오류가 있는 상태에서는 게임 진입과 빌드가 차단된다.

이를 통해 이후 개별 데이터 종류의 세부 수치를 시트에서 확장하더라도 공통 ID와 버전을 기준으로 일관되게 관리할 수 있는 구조를 마련했다.

---

## 커밋 제목

```text
17일차 : 시트 데이터 가져오기 및 치명 오류 검증 체계 구축
```
