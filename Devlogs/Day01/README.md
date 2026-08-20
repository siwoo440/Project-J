# Project J

## 개발 일지

### 1일차 : 프로젝트 기준선 및 초기 개발 환경 구축

#### 개발 목표

새로운 개발 일정에 맞춰 Project J의 Unity 프로젝트를 처음부터 다시 구성하고, 이후 개발에 사용할 안정적인 기준선을 만든다.

기존 프로젝트의 기능을 바로 옮기지 않고 새로운 Unity 프로젝트를 생성한 뒤, 버전 관리와 기본 프로젝트 설정을 먼저 확정한다.

---

#### 개발 환경

| 항목 | 내용 |
| --- | --- |
| Engine | Unity 6 |
| Unity Version | 6000.3.21f1 |
| Render Pipeline | Universal Render Pipeline |
| URP Version | 17.3.0 |
| Input System | Unity Input System 1.20.0 |
| Version Control | Git / GitHub |
| Repository | siwoo440/Project-J |

---

#### 구현 및 설정 내용

##### 1. 새 Unity 프로젝트 생성

- Unity 6 `6000.3.21f1` 기준으로 새 프로젝트 생성
- Universal Render Pipeline 기반 프로젝트 구성
- 기존 Project J 프로젝트와 분리된 새로운 개발 기준선 구축

##### 2. Unity 버전 관리 설정

Unity 프로젝트의 변경 내용을 Git에서 안정적으로 관리할 수 있도록 다음 설정을 적용했다.

- Version Control Mode : `Visible Meta Files`
- Asset Serialization Mode : `Force Text`

이를 통해 `.meta`, Scene, Prefab, ScriptableObject 등의 Unity 파일을 텍스트 기반으로 추적할 수 있도록 구성했다.

##### 3. Git 추적 규칙 구성

프로젝트 루트에 `.gitignore`를 추가했다.

Git에서 제외되는 주요 자동 생성 파일 및 폴더:

```text
Library/
Temp/
Obj/
Build/
Builds/
Logs/
UserSettings/
.vs/
.idea/
.vscode/
```

Git에서 관리하는 주요 프로젝트 데이터:

```text
Assets/
Packages/
ProjectSettings/
```

##### 4. Git 속성 설정

`.gitattributes`를 추가해 코드와 Unity 텍스트 직렬화 파일의 줄바꿈 형식을 `LF`로 통일했다.

주요 대상:

```text
*.cs
*.shader
*.json
*.asmdef
*.inputactions
*.meta
*.unity
*.prefab
*.asset
*.mat
*.anim
*.controller
```

##### 5. 기본 Unity 패키지 구성 확인

초기 프로젝트에 다음 주요 패키지가 포함된 것을 확인했다.

- Universal Render Pipeline `17.3.0`
- Input System `1.20.0`
- Unity Test Framework `1.6.0`
- Multiplayer Center `1.0.1`
- Visual Studio Integration `2.0.26`
- Rider Integration `3.0.40`

현재 단계에서는 Photon Fusion, 기존 NGO 네트워크 코드, 절차적 맵 생성 코드 등의 게임 기능은 추가하지 않았다.

---

#### GitHub 기록

최초 개발 기준선을 GitHub 저장소에 기록했다.

```text
1일차 : 프로젝트 기준선 및 초기 개발 환경 구축
```

Commit:

```text
ff8445345d71c3663e30a162b2164274ea448e32
```

---

#### 확인 결과

GitHub 저장소 기준으로 다음 항목을 확인했다.

- Unity 버전 `6000.3.21f1` 적용
- URP 프로젝트 구성 확인
- `Visible Meta Files` 적용
- `Force Text` 적용
- `.gitignore` 적용
- `.gitattributes` 적용
- Unity 자동 생성 폴더가 최초 커밋에 포함되지 않음
- 프로젝트 기본 데이터인 `Assets`, `Packages`, `ProjectSettings` 추적

다음 항목은 GitHub 파일만으로 확인할 수 없으므로 Unity Editor에서 직접 확인한다.

- Console Error 0건
- 기본 Scene 정상 실행
- Play Mode 정상 진입 및 종료

---

#### 1일차 완료 기준

다음 조건을 만족하면 1일차를 완료한 것으로 판단한다.

- 새 Unity 프로젝트 생성 완료
- Unity 프로젝트 버전 및 URP 기준 확정
- Git 버전 관리 설정 완료
- 프로젝트 직렬화 설정 완료
- Git 추적 제외 규칙 구성 완료
- GitHub 최초 커밋 완료
- Unity Editor에서 Console Error 0건 확인
- 기본 Scene Play Mode 정상 실행 확인

---

## 다음 개발 방향

### 2일차 : 핵심 폴더·Assembly 구조 재정리

다음 일차에서는 Project J의 장기 개발을 위한 기본 코드 구조를 만든다.

주요 작업 예정:

- `Runtime`
- `Editor`
- `Tests`
- `Data`

등의 핵심 폴더 구조를 정리하고, 필요한 Assembly Definition을 구성해 이후 시스템이 무분별하게 서로 참조하지 않도록 프로젝트 구조를 확정한다.
