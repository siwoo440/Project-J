# Project J 개발 일지

## 49일차 : 컴파일·참조·메뉴·테스트·빌드 최종 검증

### 개발 목표

46~48일차 동안 진행한 프로젝트 구조 정리 작업 이후 전체 프로젝트가 정상 상태를 유지하는지 최종 검증했습니다.

이번 일차에서는 새로운 게임 기능을 추가하지 않고 다음 항목을 집중적으로 확인했습니다.

- Unity 프로젝트 전체 컴파일 상태
- Runtime·Editor·Tests Assembly Definition 참조 상태
- Unity Editor 기능별 메뉴 구조
- 47일차 Runtime·Data 폴더 구조
- 48일차 Editor·Tests 폴더 구조
- EditMode·PlayMode 테스트
- Scene·Prefab 스크립트 참조
- Game Scene 실제 실행
- 개발 빌드 가능 상태

49일차를 프로젝트 구조 정리 구간의 최종 검증 단계로 설정했습니다.

---

### 기준 상태

49일차 검증은 다음 48일차 완료 상태를 기준으로 진행했습니다.

```text
48일차 : Editor·Tests 스크립트 기능별 폴더 통합
```

기준 커밋:

```text
c591035bf8a525ec438141f59732ac483b7a8f28
```

46일차부터 진행한 구조 정리 흐름은 다음과 같습니다.

```text
46일차
Unity Editor 메뉴 기능별 재분류

47일차
Runtime·Data 스크립트 기능별 폴더 통합

48일차
Editor·Tests 스크립트 기능별 폴더 통합

49일차
전체 프로젝트 최종 검증
```

---

### 1. 프로젝트 전체 컴파일 검증

Unity 프로젝트를 실행하고 모든 Asset Import와 Script Compilation이 완료된 뒤 Console 상태를 확인했습니다.

주요 확인 대상은 다음과 같습니다.

```text
CS0246
CS0234
MissingReferenceException
NullReferenceException
Assembly reference error
```

구조 변경 과정에서 스크립트의 실제 위치가 크게 변경되었기 때문에 클래스나 namespace 자체보다 Assembly 참조와 기존 asset GUID가 정상적으로 연결되어 있는지를 중심으로 확인했습니다.

---

### 2. Assembly Definition 구조 검증

현재 프로젝트의 주요 Assembly Definition은 다음 네 영역으로 구성되어 있습니다.

```text
Assets/_ProjectJ/Scripts/Runtime/
ProjectJ.Runtime.asmdef

Assets/_ProjectJ/Scripts/Editor/
ProjectJ.Editor.asmdef

Assets/_ProjectJ/Tests/EditMode/
ProjectJ.Tests.EditMode.asmdef

Assets/_ProjectJ/Tests/PlayMode/
ProjectJ.Tests.PlayMode.asmdef
```

46~48일차 폴더 재구성 과정에서 기능별 하위 폴더를 추가했지만 별도의 중첩 asmdef는 만들지 않았습니다.

따라서 기존 Assembly 경계를 유지하면서 모든 하위 기능 스크립트가 기존 Assembly에 포함되는 구조를 유지했습니다.

---

### 3. Runtime 구조 재검증

47일차에서 정리한 Runtime 구조가 그대로 유지되는지 확인했습니다.

주요 영역은 다음과 같습니다.

```text
Runtime
├─ Audio
├─ Common
├─ Core
├─ Data
├─ Gameplay
├─ Input
├─ Items
├─ Map
├─ Player
├─ UI
└─ ProjectJ.Runtime.asmdef
```

Data·Map·Item 영역의 기능별 하위 폴더가 정상적으로 유지되고 기존 루트 위치에 이동 전 스크립트가 다시 생성되지 않았는지 확인했습니다.

---

### 4. Editor 구조 재검증

48일차에서 정리한 Editor 기능별 구조도 다시 확인했습니다.

```text
Editor
├─ ProjectManagement
├─ ProjectSettings
├─ Player
├─ Data
├─ Testing
├─ Build
├─ Map
├─ Items
├─ UI
├─ Common
└─ ProjectJ.Editor.asmdef
```

특히 다음 항목을 확인했습니다.

- `ProjectJEditorMenuPaths.cs`의 프로젝트 관리 폴더 유지
- 47·48일차 구조 관리 도구의 Structure 폴더 유지
- Data 도구의 Setup·CSV·Catalog 분리 유지
- Map 도구의 Modules·Generation·Validation·Obstacles 분리 유지
- Item 도구의 Inventory·Chests·Effects·Validation 분리 유지

---

### 5. Tests 구조 재검증

EditMode와 PlayMode 테스트 역시 기능별 폴더 구조를 유지하는지 확인했습니다.

EditMode 주요 구조:

```text
Tests/EditMode
├─ Common
├─ Data
├─ Gameplay
├─ Items
├─ Map
├─ Player
├─ ProjectSettings
├─ Structure
├─ UI
└─ ProjectJ.Tests.EditMode.asmdef
```

PlayMode 주요 구조:

```text
Tests/PlayMode
├─ Items
├─ Testing
└─ ProjectJ.Tests.PlayMode.asmdef
```

기존 테스트 클래스의 namespace와 실제 테스트 로직은 변경하지 않고 파일 위치만 기능별로 정리된 상태를 유지했습니다.

---

### 6. Unity Editor 메뉴 구조 검증

46일차에서 확정한 Unity Editor 상단 메뉴가 48일차의 실제 파일 이동 이후에도 정상적으로 유지되는지 확인했습니다.

```text
Project J
├─ 01. 프로젝트 설정
├─ 02. 플레이어와 입력
├─ 03. 데이터
├─ 04. 테스트
├─ 05. 빌드
├─ 06. 맵
├─ 07. 장애물
├─ 08. 아이템
└─ 09. UI
```

Editor 스크립트의 실제 물리적 경로가 변경되어도 `[MenuItem]` 경로와 `ProjectJEditorMenuPaths` 참조가 정상적으로 유지되는지를 확인했습니다.

---

### 7. 구조 회귀 테스트 검증

47일차와 48일차에서 추가한 구조 회귀 테스트를 포함해 프로젝트 구조가 다시 이전 상태로 돌아가지 않았는지 검사했습니다.

주요 검증 항목:

```text
Runtime 루트 구조

Editor 루트 구조

EditMode 루트 구조

PlayMode 루트 구조

asmdef 위치와 개수

ProjectJEditorMenuPaths 위치

기존 경로 스크립트 잔존 여부
```

폴더 구조 자체도 테스트 대상으로 관리하여 이후 개발 중 스크립트가 다시 루트에 무분별하게 추가되는 상황을 감지할 수 있도록 했습니다.

---

### 8. EditMode 테스트 전체 검증

Unity Test Runner에서 EditMode 테스트 전체를 실행하여 기존 시스템 로직과 구조 테스트를 함께 검증했습니다.

검증 범위에는 다음 시스템들이 포함됩니다.

```text
프로젝트 구조
데이터
플레이어
맵 생성
맵 검증
장애물
아이템
경기 진행
UI
Editor 메뉴
폴더 구조
```

49일차에서는 특정 기능 하나만 검사하지 않고 기존 EditMode 테스트 전체를 회귀 테스트로 활용했습니다.

---

### 9. PlayMode 테스트 전체 검증

EditMode 검증 이후 PlayMode 테스트도 전체 실행하여 실제 Unity 실행 환경에서 문제가 발생하지 않는지 확인했습니다.

주요 대상:

```text
Tests Scene
아이템 통합 동작
실제 Runtime Component 연결
PlayMode 초기화 흐름
```

폴더 이동은 소스 코드 로직을 직접 수정하지 않았지만 Unity asset GUID와 Assembly 참조 문제가 실제 실행 단계에서 발생할 가능성이 있기 때문에 EditMode와 별도로 PlayMode 검증을 진행했습니다.

---

### 10. Scene·Prefab 스크립트 참조 확인

구조 변경 이후 주요 Scene과 Prefab의 MonoBehaviour 참조 상태를 확인했습니다.

중점 확인 항목:

```text
Missing (Mono Script)
Missing Reference
잘못된 Component 참조
```

확인 대상은 프로젝트의 주요 Scene과 플레이어·맵·아이템 관련 Prefab을 중심으로 진행했습니다.

`.meta` GUID를 유지한 상태에서 `AssetDatabase.MoveAsset()`으로 이동했기 때문에 기존 Scene·Prefab 참조를 그대로 보존하는 것을 기준으로 검증했습니다.

---

### 11. Game Scene 실제 실행 검증

자동 테스트 외에도 Game Scene을 직접 실행하여 주요 기능을 간단하게 확인했습니다.

주요 확인 항목:

```text
플레이어 이동
달리기
점프
앉기
카메라

밀치기

절차적 맵 생성
체크포인트
추락과 부활

아이템 상자
아이템 인벤토리
아이템 선택
아이템 사용

HUD
경기 진행
결과 처리
```

49일차의 목적은 각 기능의 밸런스를 다시 조정하는 것이 아니라 구조 변경 이후 기존 기능이 정상적으로 유지되는지 확인하는 것이었습니다.

---

### 12. 개발 빌드 검증

Unity Editor 내부 실행만으로 검증을 끝내지 않고 기존 Development Build 흐름도 확인했습니다.

기존 빌드 도구를 기준으로 다음 항목을 점검했습니다.

```text
Build Profile
Build Settings Scene 목록
필수 Runtime 데이터
게임 시작 Scene
Windows Development Build
```

빌드 단계에서는 Editor에서는 발견되지 않을 수 있는 다음 문제도 검증 대상으로 포함했습니다.

```text
Scene 누락
Runtime 데이터 누락
Assembly 참조 문제
빌드 전용 초기화 오류
Asset 참조 누락
```

---

### 13. 빌드 실행 확인

개발 빌드가 생성된 이후 실제 실행 파일을 기준으로 기본 실행 흐름을 확인했습니다.

```text
프로그램 실행
초기 Scene 진입
Game Scene 진입
플레이어 조작
맵 생성
UI 출력
아이템 기본 기능
프로그램 종료
```

Unity Editor에 의존하지 않는 독립 실행 상태에서도 프로젝트의 핵심 흐름이 유지되는지를 최종 기준으로 삼았습니다.

---

### 14. 구조 정리 구간 최종 결과

46~49일차를 통해 프로젝트 구조 정리와 검증 작업을 완료했습니다.

정리 전에는 개발 일차를 기준으로 Editor 도구와 테스트 파일이 루트에 누적되는 형태가 많았지만 현재는 각 파일이 실제 기능을 기준으로 분류되는 구조를 갖게 됐습니다.

최종적으로 프로젝트 코드 영역은 다음 기준으로 구분됩니다.

```text
Runtime
→ 실제 게임 실행 기능

Editor
→ 프로젝트 제작·설정·검증 도구

Tests/EditMode
→ 로직·데이터·구조 단위 테스트

Tests/PlayMode
→ 실제 Unity 실행 환경 테스트

Documentation
→ 구조와 개발 규칙 기록
```

또한 구조 자체를 자동 테스트 대상으로 추가하여 이후 새로운 시스템이 늘어나더라도 정리된 구조를 지속적으로 유지할 수 있는 기반을 마련했습니다.

---

### 49일차 완료 기준

```text
[완료] Unity 프로젝트 컴파일 확인
[완료] Runtime asmdef 구조 확인
[완료] Editor asmdef 구조 확인
[완료] EditMode asmdef 구조 확인
[완료] PlayMode asmdef 구조 확인
[완료] 47일차 Runtime·Data 구조 확인
[완료] 48일차 Editor·Tests 구조 확인
[완료] Project J 기능별 메뉴 구조 확인
[완료] EditMode 전체 테스트 검증
[완료] PlayMode 전체 테스트 검증
[완료] Scene·Prefab 참조 상태 확인
[완료] Game Scene 기본 실행 확인
[완료] Development Build 흐름 확인
```

---

### 다음 개발 방향

50일차부터는 46~49일차의 프로젝트 구조 정리 구간을 종료하고 다시 기능 개발 단계로 전환합니다.

다음 단계에서는 설정 시스템을 중심으로 다음 기능을 정리합니다.

```text
SettingsManager
설정 데이터 구조
기본값
설정 저장
설정 불러오기
설정 초기화
설정 UI 연결 기반
```

설정 시스템을 Runtime에서 독립적으로 관리할 수 있는 구조를 먼저 만들고 이후 그래픽·오디오·입력·카메라 등의 개별 설정을 연결하는 방향으로 진행합니다.

---

## 49일차 커밋 제목

```text
49일차 : 컴파일·참조·메뉴·테스트·빌드 최종 검증
```

현재 저장소에서 확인된 최신 커밋은 아직 48일차이므로 49일차 작업을 커밋한 뒤 위 제목을 사용합니다.
