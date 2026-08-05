# Project J

3D 3인칭 온라인 수직 점프 경쟁 파티 게임 **Project J**의 개발 저장소입니다.

플레이어는 절차적으로 구성된 수직 경기장을 올라가며 다른 플레이어와 밀치기, 아이템, 장애물 경쟁을 벌입니다.  
초기 출시 목표는 Steam PC 플랫폼의 4~8인 서버 권한 온라인 경기입니다.

---

# 개발 환경

| 항목 | 내용 |
|---|---|
| 프로젝트명 | Project J |
| 개발 인원 | 1인 개발 |
| 게임 엔진 | Unity 6 |
| Unity 버전 | 6000.3.21f1 |
| 프로젝트 템플릿 | Universal 3D |
| 렌더 파이프라인 | URP |
| 대상 플랫폼 | Steam PC |
| 버전 관리 | Git / GitHub |
| 저장소 | siwoo440/Project-J |

---

# 1일차 : 프로젝트 생성 및 버전 고정

## 개발 목표

Unity 프로젝트의 기본 개발 환경을 구성하고, 모든 개발 환경에서 동일한 Unity 버전과 패키지를 사용할 수 있도록 프로젝트 버전을 고정합니다.

게임 기능 구현보다 프로젝트 기반의 안정성을 먼저 확보하는 것을 목표로 진행했습니다.

---

## 구현 내용

### 1. Unity 프로젝트 생성

Unity Hub에서 다음 설정으로 새 프로젝트를 생성했습니다.

| 항목 | 설정 |
|---|---|
| Unity Editor | 6000.3.21f1 |
| Template | Universal 3D |
| Project Name | Project-J |
| Render Pipeline | Universal Render Pipeline |

Universal 3D 템플릿을 사용해 URP가 기본 적용된 3D 프로젝트를 구성했습니다.

### 2. URP 적용 확인

다음 항목을 확인했습니다.

- Universal Render Pipeline 패키지 설치
- Graphics 설정의 Render Pipeline Asset 연결
- 기본 씬 정상 실행
- Play Mode 진입과 종료 정상 작동
- Console의 컴파일 오류 및 예외 없음

### 3. Unity 버전 고정

프로젝트의 다음 파일을 통해 Unity Editor 버전을 확인했습니다.

```text
ProjectSettings/ProjectVersion.txt
```

적용된 버전은 다음과 같습니다.

```text
m_EditorVersion: 6000.3.21f1
```

앞으로 프로젝트의 Unity 버전은 `ProjectVersion.txt`를 최종 기준으로 사용합니다.

다른 버전의 Unity에서 프로젝트를 임의로 변환하지 않습니다.

### 4. 버전 관리 설정

Unity 프로젝트의 버전 관리 호환성을 위해 다음 설정을 적용했습니다.

#### Version Control

```text
Edit
→ Project Settings
→ Version Control
→ Mode: Visible Meta Files
```

#### Asset Serialization

```text
Edit
→ Project Settings
→ Editor
→ Asset Serialization
→ Mode: Force Text
```

위 설정을 통해 에셋의 `.meta` 파일과 씬, 프리팹, ScriptableObject 변경 내용을 Git으로 관리할 수 있도록 구성했습니다.

### 5. Git 제외 파일 설정

프로젝트 루트에 `.gitignore` 파일을 생성했습니다.

Unity에서 자동으로 생성되는 다음 폴더와 파일은 Git 관리 대상에서 제외했습니다.

```text
Library/
Temp/
Obj/
Build/
Builds/
Logs/
UserSettings/
MemoryCaptures/
Recordings/
.vs/
.vscode/
.idea/
```

다음 핵심 폴더와 파일은 Git에 포함합니다.

```text
Assets/
Packages/
ProjectSettings/
.gitignore
```

### 6. GitHub 저장소 연결

로컬 Unity 프로젝트를 다음 원격 저장소와 연결했습니다.

```text
https://github.com/siwoo440/Project-J.git
```

기본 브랜치는 `main`으로 설정했습니다.

---

## 사용한 Git 명령어

```bash
git init
git branch -M main
git remote add origin https://github.com/siwoo440/Project-J.git
git add .
git status
git commit -m "1일차 : 프로젝트 생성 및 버전 고정"
git push -u origin main
git status
```

원격 저장소가 이미 등록되어 있는 경우 다음 명령어로 주소를 수정할 수 있습니다.

```bash
git remote set-url origin https://github.com/siwoo440/Project-J.git
```

저장소 소유권 경고가 발생하는 경우 프로젝트 경로에 맞게 다음 명령어를 사용합니다.

```bash
git config --global --add safe.directory F:/Project-J
```

---

## 주요 프로젝트 구조

```text
Project-J/
├─ Assets/
├─ Packages/
│  ├─ manifest.json
│  └─ packages-lock.json
├─ ProjectSettings/
│  └─ ProjectVersion.txt
├─ UserSettings/
└─ .gitignore
```

`UserSettings` 폴더는 로컬 사용자 설정이므로 Git에 포함하지 않습니다.

---

## 검증 결과

| 검증 항목 | 결과 |
|---|:---:|
| Unity 6000.3.21f1 프로젝트 생성 | 완료 |
| Universal 3D 템플릿 적용 | 완료 |
| URP 패키지 및 설정 확인 | 완료 |
| 기본 씬 실행 | 완료 |
| Play Mode 실행 | 완료 |
| Console Error 0개 | 완료 |
| Visible Meta Files 설정 | 완료 |
| Force Text 설정 | 완료 |
| ProjectVersion.txt 확인 | 완료 |
| .gitignore 생성 | 완료 |
| Git 저장소 초기화 | 완료 |
| GitHub 원격 저장소 연결 | 완료 |
| 최초 커밋 및 Push | 완료 |

---

## 생성 및 수정 파일

### 생성

```text
.gitignore
```

### Unity 자동 생성 및 버전 관리 등록

```text
Assets/
Packages/
ProjectSettings/
```

### C# 스크립트

1일차에는 C# 스크립트를 생성하거나 수정하지 않았습니다.

게임 코드와 프로젝트 전용 폴더 구조는 다음 일차부터 구성합니다.

---

## 발생 가능한 문제와 해결 방법

### Library 폴더가 Git에 포함되는 문제

```bash
git rm -r --cached Library
git add .
git status
```

### 원격 저장소가 이미 등록된 문제

```bash
git remote set-url origin https://github.com/siwoo440/Project-J.git
```

### 저장소 소유권 경고

```bash
git config --global --add safe.directory F:/Project-J
```

### 프로젝트가 잘못된 Unity 버전으로 열리는 문제

Unity Hub에서 `6000.3.21f1`을 선택해 프로젝트를 다시 엽니다.

이미 다른 버전으로 변환된 경우에는 Git 변경 내역을 확인하고 변환 전 상태로 복원합니다.

---

# 다음 개발 방향

## 2일차 : 폴더·어셈블리·네임스페이스 구성

다음 일차에는 프로젝트 전용 리소스를 `Assets/_ProjectJ` 아래에 정리합니다.

예정 작업은 다음과 같습니다.

- 프로젝트 전용 폴더 구조 생성
- Runtime, Editor, Tests Assembly Definition 구성
- `ProjectJ` 최상위 네임스페이스 적용
- 책임별 스크립트 폴더 분리
- 어셈블리 참조와 컴파일 상태 확인

---

# 커밋 정보

```text
1일차 : 프로젝트 생성 및 버전 고정
```
