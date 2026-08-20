# 9일차 개발일지 - Legacy 시스템 격리 구조 구축

## 오늘의 목표

이전 Project J에서 사용했던 네트워크 및 맵 생성 코드를
현재 새 프로젝트의 Runtime 코드와 섞지 않고
참고용으로 보관할 수 있는 Legacy 영역을 구축한다.

기존 NGO 기반 네트워크 코드와 구형 생성기를
현재 게임 코어에 다시 연결하지 않고,
향후 필요한 부분만 검토할 수 있도록 Assembly 단계에서 분리한다.

## 구현 내용

### 1. Legacy 전용 영역 생성

다음 구조를 새로 추가했다.

```text
Assets/ProjectJ/Legacy/
├─ ProjectJ.Legacy.asmdef
├─ Networking/
└─ Generation/
```

각 영역의 역할은 다음과 같다.

| 경로 | 역할 |
| --- | --- |
| Legacy/Networking | 이전 NGO 및 구형 네트워크 코드 보관 |
| Legacy/Generation | 이전 맵·스테이지 생성 코드 보관 |
| ProjectJ.Legacy.asmdef | Legacy 코드의 별도 Assembly 경계 |

현재 단계에서는 실제 구형 코드를 이식하지 않고
격리 구조만 구축했다.

### 2. Legacy Assembly 분리

`ProjectJ.Legacy` Assembly를 추가했다.

주요 설정:

```text
Assembly Name: ProjectJ.Legacy
Auto Referenced: OFF
Define Constraint: PROJECTJ_LEGACY
```

`Auto Referenced`를 비활성화해
현재 게임 코드가 Legacy Assembly를 자동으로 참조하지 않도록 했다.

또한 `PROJECTJ_LEGACY` Define이 존재할 때만
Legacy Assembly가 컴파일되도록 구성했다.

### 3. Legacy 기본 비활성 상태 유지

현재 프로젝트에는 `PROJECTJ_LEGACY` Define을 추가하지 않았다.

따라서 기본 개발 상태에서는 다음 구조가 유지된다.

```text
PROJECTJ_LEGACY 없음
↓
ProjectJ.Legacy 컴파일 제외
↓
구형 코드가 현재 게임 실행 및 빌드에 영향 없음
```

향후 과거 코드를 조사해야 할 때만
의도적으로 Legacy Assembly를 활성화한다.

### 4. 현재 Runtime과 Legacy 의존성 분리

현재 새 게임의 핵심 Assembly인 `ProjectJ.Runtime`은
Legacy Assembly를 참조하지 않는다.

구조:

```text
ProjectJ.Runtime
     X
     │
     ▼
ProjectJ.Legacy
```

이를 통해 새로운 플레이어, 맵, 네트워크 시스템을 구현할 때
과거 코드가 새 구조의 필수 의존성이 되는 것을 방지한다.

### 5. 빈 Editor Assembly 정리

이전에 일회성 Editor 작업을 위해 사용했던
`ProjectJ.Editor.asmdef` 아래에는 더 이상 실제 Editor 스크립트가 없었다.

Unity에서 다음 경고가 발생했다.

```text
Assembly for Assembly Definition File
'Assets/ProjectJ/Editor/ProjectJ.Editor.asmdef'
will not be compiled, because it has no scripts associated with it.
```

현재 Editor 전용 코드가 필요하지 않으므로
빈 `ProjectJ.Editor.asmdef`와 해당 Editor 영역을 제거했다.

앞으로 실제 Editor 도구가 필요한 개발 단계에서
Editor Assembly를 다시 생성한다.

### 6. Unity Meta 파일 정리

Legacy 폴더와 Assembly 생성 과정에서
Unity가 새 `.meta` 파일과 GUID를 생성했다.

Git에서는 기존 Editor 관련 `.meta`와
일부 Legacy `.meta`가 rename으로 감지될 수 있지만,
실제 GUID는 새로운 값으로 변경되어 있어
새 Legacy Asset으로 정상 구분된다.

## 생성된 주요 요소

```text
Assets/ProjectJ/Legacy/
Assets/ProjectJ/Legacy/Networking/
Assets/ProjectJ/Legacy/Generation/
Assets/ProjectJ/Legacy/ProjectJ.Legacy.asmdef
```

## 삭제된 요소

```text
Assets/ProjectJ/Editor/ProjectJ.Editor.asmdef
```

현재 사용하지 않는 빈 Editor Assembly를 정리했다.

## 테스트

- `Assets/ProjectJ/Legacy` 존재 확인
- `Networking` 폴더 존재 확인
- `Generation` 폴더 존재 확인
- `ProjectJ.Legacy` Assembly 존재 확인
- Auto Referenced OFF 확인
- `PROJECTJ_LEGACY` Define Constraint 확인
- 실제 Scripting Define Symbols에 `PROJECTJ_LEGACY`가 추가되지 않았는지 확인
- `ProjectJ.Runtime`이 Legacy를 참조하지 않는지 확인
- 빈 Editor Assembly 경고 제거 확인
- EditMode 테스트 전체 Green 확인
- PlayMode 테스트 전체 Green 확인
- 기존 Bootstrap → MainMenu → Game → MainMenu 흐름 확인
- Unity Console Error 0 확인

## 결과

Project J의 현재 개발 코드와 과거 시스템을
Assembly 및 폴더 구조 단계에서 명확하게 분리했다.

앞으로 이전 NGO 코드나 구형 맵 생성기를 참고해야 할 경우
Legacy 영역에만 보관할 수 있으며,
기본 개발 상태에서는 해당 코드가 현재 게임 빌드에 포함되지 않는다.

또한 더 이상 사용하지 않는 빈 Editor Assembly를 정리해
불필요한 Unity 컴파일 경고도 제거했다.

이제 1~9일차에서 구축한 프로젝트 기반을 대상으로
전체 회귀 검증을 진행할 준비가 완료되었다.
