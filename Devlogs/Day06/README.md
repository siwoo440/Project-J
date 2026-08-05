# Project J

3D 3인칭 온라인 수직 점프 경쟁 파티 게임 **Project J**의 개발 저장소입니다.

---

# 개발 환경

| 항목 | 내용 |
|---|---|
| 게임 엔진 | Unity 6 |
| Unity 버전 | 6000.3.21f1 |
| 프로젝트 템플릿 | Universal 3D |
| 렌더 파이프라인 | URP |
| 대상 플랫폼 | Steam PC |
| 개발 인원 | 1인 개발 |
| 기본 온라인 인원 | 4~8인 |
| 입력 시스템 | Unity Input System 1.20.0 |
| 저장소 | siwoo440/Project-J |

---

# 6일차 : 공통 데이터 ID 규칙 구성

## 개발 목표

플레이어·맵·장애물·아이템·꾸미기·오디오 데이터가 동일한 방식으로 식별되고 검증될 수 있도록 공통 데이터 ID와 버전 규칙을 구성했습니다.

각 데이터는 ScriptableObject 에셋으로 관리하며, 다음 공통 정보를 가집니다.

```text
Data Id
Display Name
Version
Category
```

이번 일차에서는 실제 이동 수치, 맵 모듈 정보, 장애물 동작, 아이템 효과, 꾸미기 옵션과 오디오 클립을 구현하지 않았습니다. 이후 일정에서 확장할 데이터 에셋의 공통 기반과 오류 검출 구조를 먼저 구현했습니다.

주요 목표는 다음과 같습니다.

- 플레이어·맵·장애물·아이템·꾸미기·오디오 데이터 분류 정의
- 분류별 고정 ID 접두사 정의
- `XXX-000` 형식의 공통 ID 규칙 구현
- `Major.Minor.Patch` 데이터 버전 구조 구현
- 모든 데이터 에셋의 공통 ScriptableObject 기반 구현
- 분류별 샘플 데이터 에셋 생성
- ID 형식 오류 자동 감지
- 중복 ID 자동 감지
- 표시 이름과 버전 누락 자동 감지
- Unity 메뉴를 통한 전체 데이터 수동 검사
- 데이터 에셋 저장·이동·삭제 후 자동 재검사
- 공통 ID와 검증기의 EditMode 테스트 작성

---

# 최신 커밋

| 항목 | 내용 |
|---|---|
| 커밋 제목 | `6일차 : 공통 데이터 ID 규칙 구성` |
| 커밋 SHA | `02c32059acfb3ca4e1c0755d16f00f6365472977` |
| 브랜치 | `main` |
| 이전 커밋 | `6102afe29b231b1f77fc3c2dbfb70e40d2bf9cda` |
| 커밋 링크 | https://github.com/siwoo440/Project-J/commit/02c32059acfb3ca4e1c0755d16f00f6365472977 |

---

# 최신 커밋 검토 결과

최신 커밋을 기준으로 다음 항목을 확인했습니다.

- 커밋 제목이 `6일차 : 공통 데이터 ID 규칙 구성`으로 정상 등록
- 플레이어·맵·장애물·아이템·꾸미기·오디오 데이터 분류 추가
- 분류별 ID 접두사와 `001~999` 번호 규칙 추가
- 데이터 버전 값 형식 추가
- 공통 데이터 ScriptableObject 기반 추가
- 여섯 데이터 분류별 ScriptableObject 형식 추가
- 여섯 개 샘플 데이터 에셋 추가
- 샘플 데이터 에셋과 스크립트의 `.meta` 파일 추가
- 전체 데이터 에셋 검색과 검증 Editor 도구 추가
- 데이터 에셋 변경과 저장 후 자동 검증 구조 추가
- ID와 필수 값을 검증하는 EditMode 테스트 8개 추가

저장소에서 확인 가능한 범위에서는 수정이 필요한 치명적인 구조 오류를 발견하지 못했습니다.

현재 커밋에는 GitHub Actions 상태 검사나 다른 CI 결과가 등록되어 있지 않습니다. 따라서 다음 항목은 로컬 Unity 에디터에서 최종 확인해야 합니다.

```text
Console Error: 0개
EditMode Passed: 23개
EditMode Failed: 0개
샘플 데이터 6개 정상 로드
전체 데이터 수동 검증 성공
중복 ID 자동 감지 정상
필수 값 누락 자동 감지 정상
```

---

# 공통 데이터 ID 규칙

## 1. ID 형식

모든 공통 데이터 ID는 다음 형식을 사용합니다.

```text
XXX-000
```

구성:

```text
세 글자 접두사
+
하이픈
+
세 자리 숫자
```

사용 가능한 번호 범위:

```text
001~999
```

`000`은 사용하지 않습니다.

---

## 2. 데이터 분류와 접두사

| 데이터 분류 | 접두사 | 기본 샘플 |
|---|---|---|
| Player | `PLY` | `PLY-001` |
| Map | `MAP` | `MAP-001` |
| Obstacle | `OBS` | `OBS-001` |
| Item | `ITM` | `ITM-001` |
| Cosmetic | `COS` | `COS-001` |
| Audio | `AUD` | `AUD-001` |

각 데이터 분류는 자신의 접두사만 사용할 수 있습니다.

예를 들어 아이템 데이터에는 다음 ID를 사용할 수 있습니다.

```text
ITM-001
ITM-002
ITM-125
ITM-999
```

다음 ID는 사용할 수 없습니다.

```text
PLY-001
itm-001
ITM_001
ITM-01
ITM-000
ITM-1000
```

---

## 3. ID 유지 원칙

ID는 파일 이름이나 표시 이름이 아니라 데이터의 영구 식별값입니다.

예를 들어 다음처럼 표시 이름과 파일 이름을 변경할 수 있습니다.

```text
ITM-001_SpringShoes.asset
→ ITM-001_SuperSpringShoes.asset
```

```text
Spring Shoes
→ Super Spring Shoes
```

그러나 내부 ID는 유지합니다.

```text
ITM-001
```

저장 데이터, 네트워크 메시지와 다른 데이터 참조에서 동일한 항목을 계속 식별하기 위해 사용 중인 ID는 가능한 한 변경하지 않습니다.

삭제된 ID도 다른 항목에 재사용하지 않는 것을 기본 원칙으로 합니다.

---

# 데이터 버전

## 4. ProjectDataVersion

데이터 버전은 다음 구조를 사용합니다.

```text
Major.Minor.Patch
```

초기 버전:

```text
1.0.0
```

| 항목 | 역할 |
|---|---|
| Major | 기존 데이터와 호환되지 않는 구조 변경 |
| Minor | 기존 데이터와 호환되는 필드나 기능 추가 |
| Patch | 값 수정과 작은 오류 수정 |

유효 조건:

```text
Major >= 1
Minor >= 0
Patch >= 0
```

따라서 다음 값은 유효합니다.

```text
1.0.0
1.1.0
1.1.3
2.0.0
```

다음 값은 유효하지 않습니다.

```text
0.0.0
0.1.0
1.-1.0
1.0.-1
```

---

# 공통 데이터 에셋

## 5. ProjectDataAsset

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Definitions/ProjectDataAsset.cs
```

모든 프로젝트 데이터 에셋이 상속하는 공통 기반입니다.

공통 필드:

```text
Data Id
Display Name
Version
```

공통 속성:

```text
Category
DataId
DisplayName
Version
```

`Category`는 파생 데이터 형식이 직접 반환합니다.

예시:

```csharp
public override ProjectDataCategory Category
    => ProjectDataCategory.Item;
```

Editor 도구와 EditMode 테스트에서는 `SetEditorIdentity`를 사용해 ID, 표시 이름과 버전을 설정합니다.

해당 메서드는 `UNITY_EDITOR` 조건 안에 있으므로 실제 플레이어 빌드의 런타임 API에는 포함되지 않습니다.

---

# 데이터 분류별 ScriptableObject

## 6. PlayerDataDefinition

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Definitions/PlayerDataDefinition.cs
```

분류:

```text
Player
```

ID 접두사:

```text
PLY
```

7일차 플레이어 설정 에셋에서 이동·달리기·앉기·점프·중력·공중 제어와 스태미나 값을 추가할 기반입니다.

---

## 7. MapDataDefinition

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Definitions/MapDataDefinition.cs
```

분류:

```text
Map
```

ID 접두사:

```text
MAP
```

이후 맵과 절차 생성 데이터에서 확장할 기반입니다.

---

## 8. ObstacleDataDefinition

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Definitions/ObstacleDataDefinition.cs
```

분류:

```text
Obstacle
```

ID 접두사:

```text
OBS
```

이동 발판, 회전 장애물, 튕김 발판과 기타 장애물 데이터를 확장할 기반입니다.

---

## 9. ItemDataDefinition

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Definitions/ItemDataDefinition.cs
```

분류:

```text
Item
```

ID 접두사:

```text
ITM
```

기획서에 이미 사용 중인 `ITM-001` 형식을 그대로 공통 규칙에 반영했습니다.

이후 아이템 분류, 사용 조건, 아이콘, 효과와 네트워크 검증 값을 추가할 기반입니다.

---

## 10. CosmeticDataDefinition

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Definitions/CosmeticDataDefinition.cs
```

분류:

```text
Cosmetic
```

ID 접두사:

```text
COS
```

외형, 이모트, 프로필과 기타 성능에 영향을 주지 않는 꾸미기 데이터를 확장할 기반입니다.

---

## 11. AudioDataDefinition

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Definitions/AudioDataDefinition.cs
```

분류:

```text
Audio
```

ID 접두사:

```text
AUD
```

BGM, SFX와 보이스 데이터를 확장할 기반입니다.

---

# 샘플 데이터

## 12. 생성된 데이터 에셋

다음 여섯 개 샘플 데이터 에셋을 생성했습니다.

```text
Assets/_ProjectJ/Data/Definitions
├─ Player
│  └─ PLY-001_DefaultPlayer.asset
├─ Map
│  └─ MAP-001_DefaultMap.asset
├─ Obstacle
│  └─ OBS-001_DefaultObstacle.asset
├─ Item
│  └─ ITM-001_SpringShoes.asset
├─ Cosmetic
│  └─ COS-001_DefaultCostume.asset
└─ Audio
   └─ AUD-001_DefaultAudio.asset
```

---

## 13. 샘플 데이터 값

### 기본 플레이어

```text
Data Id: PLY-001
Display Name: Default Player
Version: 1.0.0
Category: Player
```

### 기본 맵

```text
Data Id: MAP-001
Display Name: Default Map
Version: 1.0.0
Category: Map
```

### 기본 장애물

```text
Data Id: OBS-001
Display Name: Default Obstacle
Version: 1.0.0
Category: Obstacle
```

### 스프링 신발

```text
Data Id: ITM-001
Display Name: Spring Shoes
Version: 1.0.0
Category: Item
```

### 기본 코스튬

```text
Data Id: COS-001
Display Name: Default Costume
Version: 1.0.0
Category: Cosmetic
```

### 기본 오디오

```text
Data Id: AUD-001
Display Name: Default Audio
Version: 1.0.0
Category: Audio
```

---

# 공통 데이터 검증

## 14. ProjectDataValidator

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Validation/ProjectDataValidator.cs
```

검증 대상:

```text
null 데이터
ID 누락
ID 형식
분류와 접두사 일치
ID 중복
표시 이름 누락
버전 유효성
```

검증 오류 코드는 다음과 같습니다.

| 오류 코드 | 의미 |
|---|---|
| `DATA_NULL` | null 데이터 또는 데이터 목록 누락 |
| `DATA_ID_MISSING` | 데이터 ID 누락 |
| `DATA_ID_INVALID` | 잘못된 ID 형식이나 접두사 |
| `DATA_ID_DUPLICATE` | 동일 ID 중복 사용 |
| `DATA_NAME_MISSING` | 표시 이름 누락 |
| `DATA_VERSION_INVALID` | 잘못된 데이터 버전 |

---

## 15. 대소문자 중복 감지

ID 중복 검사는 대소문자를 무시하도록 구성했습니다.

예를 들어 다음 두 ID가 동시에 존재하면 중복으로 판단합니다.

```text
ITM-001
itm-001
```

두 번째 ID는 형식 오류도 발생하므로 다음 문제가 함께 검출될 수 있습니다.

```text
DATA_ID_INVALID
DATA_ID_DUPLICATE
```

---

# 검증 결과 구조

## 16. DataValidationSeverity

문제 심각도를 다음 두 단계로 구분합니다.

```text
Warning
Error
```

현재 6일차 검증 규칙은 모두 게임 데이터의 무결성에 영향을 주므로 오류 수준을 사용합니다.

---

## 17. ProjectDataValidationIssue

단일 검증 문제는 다음 정보를 가집니다.

```text
Asset
Severity
Code
Message
```

로그 변환 예시:

```text
[Error] DATA_ID_DUPLICATE / ITM-001_SpringShoes / 데이터 ID ITM-001가 2개 에셋에서 중복 사용되고 있습니다.
```

---

## 18. ProjectDataValidationReport

전체 검증 결과는 다음 정보를 제공합니다.

```text
Issues
IssueCount
ErrorCount
WarningCount
HasErrors
IsValid
```

오류가 한 개도 없으면 다음 값이 됩니다.

```text
IsValid: true
HasErrors: false
ErrorCount: 0
```

---

# Editor 데이터 검색과 검사

## 19. ProjectDataAssetDatabase

파일 위치:

```text
Assets/_ProjectJ/Scripts/Editor/Data/ProjectDataAssetDatabase.cs
```

검사 대상 루트:

```text
Assets/_ProjectJ/Data/Definitions
```

다음 과정을 수행합니다.

```text
Definitions 폴더 검색
→ ScriptableObject 에셋 GUID 검색
→ ProjectDataAsset 형식만 불러오기
→ 전체 데이터 검증
→ 오류 또는 경고 로그 출력
```

검증 성공 시 다음 로그를 출력할 수 있습니다.

```text
[Data] 데이터 에셋 6개의 ID와 필수 값 검증을 완료했습니다.
```

다른 형식의 ScriptableObject가 같은 폴더에 있어도 `ProjectDataAsset`으로 불러올 수 없는 에셋은 검사 목록에서 제외됩니다.

---

# 자동 검증

## 20. ProjectDataAssetPostprocessor

파일 위치:

```text
Assets/_ProjectJ/Scripts/Editor/Data/ProjectDataAssetPostprocessor.cs
```

다음 데이터 에셋 변경을 감지합니다.

```text
가져오기
삭제
이동
이전 이동 경로
저장
```

검사 대상 조건:

```text
Assets/_ProjectJ/Data/Definitions 내부
+
.asset 확장자
```

일반 스크립트나 다른 폴더의 ScriptableObject는 검사하지 않습니다.

여러 에셋 변경이 한 번에 발생해도 `validationQueued`를 사용하여 같은 검사를 반복 예약하지 않도록 구성했습니다.

Play Mode 실행 또는 진입 중에는 자동 검사를 생략합니다.

---

## 21. ProjectDataAssetSaveProcessor

Inspector에서 데이터 값을 수정하고 에셋이 저장될 때 전체 데이터 재검사를 예약합니다.

동작 흐름:

```text
데이터 에셋 저장 요청
→ Definitions 폴더 데이터인지 확인
→ 현재 에셋 저장 완료
→ 지연 호출
→ 모든 데이터 에셋 재검사
```

이를 통해 새 에셋 생성뿐 아니라 기존 에셋의 ID와 표시 이름을 직접 수정한 경우도 자동으로 검사합니다.

---

# 6일차 Editor 메뉴

## 22. Day06DataSetupTool

파일 위치:

```text
Assets/_ProjectJ/Scripts/Editor/Day06DataSetupTool.cs
```

Unity 상단 메뉴에 다음 기능을 추가했습니다.

```text
Project J
└─ Day 06
   ├─ Create Sample Data Assets
   └─ Validate All Data Assets
```

Play Mode 실행 또는 진입 중에는 메뉴가 비활성화됩니다.

---

## 23. Create Sample Data Assets

다음 작업을 자동으로 수행합니다.

```text
데이터 루트 폴더 확인
→ 분류별 폴더 생성
→ 여섯 샘플 데이터 존재 확인
→ 누락된 샘플만 생성
→ 에셋 저장
→ Project 창 새로고침
→ 전체 데이터 검증
```

기존 샘플 에셋이 있으면 덮어쓰지 않습니다.

---

## 24. Validate All Data Assets

현재 Definitions 폴더의 모든 `ProjectDataAsset`을 불러와 전체 검사를 실행합니다.

정상 결과:

```text
[Data] 데이터 에셋 6개의 ID와 필수 값 검증을 완료했습니다.
```

오류가 있는 경우 개별 오류를 출력한 뒤 다음 요약도 출력합니다.

```text
[Day06] 데이터 검증에서 오류 n개를 발견했습니다.
```

---

# EditMode 테스트

## 25. ProjectDataValidatorTests

파일 위치:

```text
Assets/_ProjectJ/Tests/EditMode/ProjectDataValidatorTests.cs
```

6일차에 다음 8개의 테스트를 추가했습니다.

### ValidIdsAreAcceptedForAllCategories

다음 ID가 각 분류에서 정상인지 검사합니다.

```text
PLY-001
MAP-001
OBS-001
ITM-001
COS-001
AUD-001
```

### CreateProducesExpectedThreeDigitId

번호를 ID로 변환한 결과를 검사합니다.

```text
Item + 1 → ITM-001
Map + 125 → MAP-125
Audio + 999 → AUD-999
```

### WrongCategoryPrefixIsRejected

플레이어 데이터에 `ITM-001`을 사용했을 때 거부되는지 검사합니다.

### ZeroIdNumberIsRejected

`OBS-000`이 거부되는지 검사합니다.

### MissingRequiredValuesAreDetected

ID와 표시 이름이 비어 있을 때 다음 오류가 검출되는지 검사합니다.

```text
DATA_ID_MISSING
DATA_NAME_MISSING
```

### DuplicateIdsAreDetected

두 아이템 데이터가 같은 `ITM-001`을 사용할 때 두 에셋 모두에서 중복 오류가 발생하는지 검사합니다.

### InvalidVersionIsDetected

`0.0.0` 버전이 거부되는지 검사합니다.

### SixValidCategoryAssetsProduceNoErrors

여섯 분류의 올바른 데이터가 오류 없이 통과하는지 검사합니다.

---

# 전체 테스트 구성

기존 테스트:

```text
2일차 ProjectStructureTests: 2개
3일차 GameSceneCatalogTests: 3개
4일차 GameServiceRegistryTests: 4개
5일차 InputActionAssetTests: 6개
```

6일차 신규 테스트:

```text
ProjectDataValidatorTests: 8개
```

예상 전체 결과:

```text
Passed: 23
Failed: 0
Ignored: 0
```

GitHub에는 CI 상태 검사가 등록되지 않았으므로 실제 통과 여부는 Unity Test Runner에서 확인해야 합니다.

---

# 생성된 주요 파일

## Runtime 데이터 식별 구조

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Identity
├─ ProjectDataCategory.cs
├─ ProjectDataIdRules.cs
└─ ProjectDataVersion.cs
```

## Runtime 데이터 정의 구조

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Definitions
├─ ProjectDataAsset.cs
├─ PlayerDataDefinition.cs
├─ MapDataDefinition.cs
├─ ObstacleDataDefinition.cs
├─ ItemDataDefinition.cs
├─ CosmeticDataDefinition.cs
└─ AudioDataDefinition.cs
```

## Runtime 검증 구조

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Validation
├─ DataValidationSeverity.cs
├─ ProjectDataValidationIssue.cs
├─ ProjectDataValidationReport.cs
└─ ProjectDataValidator.cs
```

## Editor 구조

```text
Assets/_ProjectJ/Scripts/Editor
├─ Day06DataSetupTool.cs
└─ Data
   ├─ ProjectDataAssetDatabase.cs
   └─ ProjectDataAssetPostprocessor.cs
```

## 테스트 구조

```text
Assets/_ProjectJ/Tests/EditMode
└─ ProjectDataValidatorTests.cs
```

## 샘플 에셋

```text
Assets/_ProjectJ/Data/Definitions
├─ Player/PLY-001_DefaultPlayer.asset
├─ Map/MAP-001_DefaultMap.asset
├─ Obstacle/OBS-001_DefaultObstacle.asset
├─ Item/ITM-001_SpringShoes.asset
├─ Cosmetic/COS-001_DefaultCostume.asset
└─ Audio/AUD-001_DefaultAudio.asset
```

Unity가 생성한 각 폴더·스크립트·에셋의 `.meta` 파일도 함께 Git에 등록했습니다.

---

# 수동 검증 절차

## 26. 전체 데이터 검사

Unity 메뉴:

```text
Project J
→ Day 06
→ Validate All Data Assets
```

정상 로그:

```text
[Data] 데이터 에셋 6개의 ID와 필수 값 검증을 완료했습니다.
```

---

## 27. 중복 ID 검사

`ITM-001_SpringShoes.asset`을 복제합니다.

복제된 에셋의 `Data Id`를 그대로 둡니다.

```text
ITM-001
```

저장하면 다음 오류가 나타나야 합니다.

```text
DATA_ID_DUPLICATE
```

검사 후 복제 에셋을 삭제합니다.

---

## 28. 분류 접두사 검사

`OBS-001_DefaultObstacle.asset`의 ID를 임시 변경합니다.

```text
OBS-001
→ ITM-001
```

저장하면 다음 오류가 나타나야 합니다.

```text
DATA_ID_INVALID
```

검사 후 다시 복원합니다.

```text
OBS-001
```

---

## 29. 표시 이름 검사

`COS-001_DefaultCostume.asset`의 `Display Name`을 비우고 저장합니다.

예상 오류:

```text
DATA_NAME_MISSING
```

검사 후 다시 복원합니다.

```text
Default Costume
```

---

## 30. 버전 검사

`AUD-001_DefaultAudio.asset`의 Major 값을 임시로 0으로 변경합니다.

예상 오류:

```text
DATA_VERSION_INVALID
```

검사 후 다음 값으로 복원합니다.

```text
1.0.0
```

---

# 검증 결과

| 검증 항목 | 저장소 확인 |
|---|:---:|
| 최신 커밋 제목 정상 | 완료 |
| 데이터 분류 6개 추가 | 완료 |
| 분류별 ID 접두사 추가 | 완료 |
| 001~999 범위 검사 | 완료 |
| 데이터 버전 구조 추가 | 완료 |
| 공통 ScriptableObject 기반 추가 | 완료 |
| 분류별 데이터 형식 추가 | 완료 |
| 샘플 데이터 6개 추가 | 완료 |
| ID 누락 검사 | 완료 |
| ID 형식 검사 | 완료 |
| ID 중복 검사 | 완료 |
| 표시 이름 누락 검사 | 완료 |
| 버전 오류 검사 | 완료 |
| 데이터 저장 후 자동 검사 | 완료 |
| EditMode 테스트 8개 추가 | 완료 |
| GitHub CI 상태 검사 | 미구성 |

로컬 Unity 최종 확인 항목:

```text
Console Error: 0개
EditMode Passed: 23개
EditMode Failed: 0개
전체 데이터 수동 검증 성공
중복 ID 자동 검사 성공
필수 값 누락 자동 검사 성공
```

---

# 이후 확장 방향

6일차 공통 데이터 구조는 다음 일정에서 확장합니다.

| 일차 | 확장 내용 |
|---:|---|
| 7일차 | 플레이어 이동·점프·스태미나 설정 데이터 |
| 12일차 | Google Sheets 데이터 가져오기와 검증 |
| 13일차 | 데이터 오류를 표시하는 치명 오류 UI |
| 15일차 | 공통 초기화와 데이터 검증 통합 |
| 46일차 | 맵 모듈 데이터 규격 |
| 61일차 이후 | 장애물 데이터 |
| 66일차 이후 | 아이템 데이터와 2슬롯 |
| 144일차 | 오디오 데이터 |
| 145일차 | 꾸미기 데이터 |

---

# 커밋 정보

```text
6일차 : 공통 데이터 ID 규칙 구성
```

```text
https://github.com/siwoo440/Project-J/commit/02c32059acfb3ca4e1c0755d16f00f6365472977
```
