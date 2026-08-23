# Project J - 97일차 개발일지

---

## 오늘의 목표

96일차에 구성한 Fusion Server Mode 기반을 실제 Windows Dedicated Server 실행 파일로 확장하고, 개발 중 누적된 `OnGUI` Debug Window를 정리한다.

97일차의 핵심 목표는 다음과 같다.

```text
Unity Server Subtarget 빌드
→ ProjectJ_Server.exe 생성
→ UNITY_SERVER 전용 진단 로그
→ 기존 Server Mode 구조 재사용
→ Client와 독립 실행 가능한 Dedicated Server 기반 확보
```

동시에 기존 숫자키 기반 Debug Window 선택 방식을 제거하고 `F1`, `F2` 두 개의 직접 Hotkey만 사용하는 구조로 단순화한다.

---

## 작업 내용

### 1. Windows Dedicated Server Build 기능 추가

Editor 전용 `ProjectJDay97DedicatedServerBuilder`를 추가했다.

Unity 상단 메뉴에서 다음 경로로 Dedicated Server Build를 실행할 수 있다.

```text
Project J
→ Build
→ 97일차 Windows Dedicated Server Build
```

빌드는 일반 Windows Client가 아니라 `StandaloneBuildSubtarget.Server`를 사용하며, 시작 Scene은 96일차 Server Mode 테스트 Scene을 그대로 사용한다.

```text
Assets/ProjectJ/Scenes/Day96_ServerModeTest.unity
```

빌드 결과는 다음 위치에 생성된다.

```text
Build/
└─ Server/
   └─ Windows/
      └─ ProjectJ_Server.exe
```

Development Build 옵션을 사용해 Dedicated Server 전환 과정에서 필요한 로그를 확인할 수 있도록 구성했다.

---

### 2. Dedicated Server 시작 진단 로그 추가

`ProjectJDay97DedicatedServerDiagnostics`를 추가했다.

`UNITY_SERVER`가 정의된 Server Build에서 Scene이 로드되기 전에 Dedicated Server 실행 여부를 로그로 출력한다.

확인 대상은 다음과 같다.

```text
UNITY_SERVER = True
Application.isBatchMode
Dedicated Server Build 시작
```

일반 Client Build에는 Server 전용 진단 코드가 포함되지 않도록 전처리 조건으로 분리했다.

---

### 3. 96일차 Server Mode 구조 재사용

97일차에서 Server 동작 구조 자체를 다시 만들지 않고 96일차의 `ProjectJDay96ServerModeBootstrap`을 그대로 Dedicated Build의 시작점으로 사용한다.

현재 Server Mode 기준은 다음과 같다.

```text
GameMode       : Server
ProvideInput   : False
InputProvider  : 없음
Server Player  : 없음
Room Code      : 960001
```

서버 자체 Local Player를 만들지 않고 실제 Client가 접속했을 때 Client용 Network Player만 Spawn하는 기존 구조를 유지한다.

---

### 4. F1 / F2 직접 Debug Hotkey 구조로 변경

기존 Debug Menu는 `F1`로 선택 메뉴를 열고 숫자키 `1~9, 0`으로 Debug Window를 고르는 방식이었다.

97일차 최종 구조에서는 선택 메뉴와 숫자키 입력을 제거하고 두 개의 직접 Hotkey만 사용하도록 단순화했다.

```text
F1
→ 정렬된 Debug Window 목록의 첫 번째 Window 열기 / 닫기

F2
→ 정렬된 Debug Window 목록의 두 번째 Window 열기 / 닫기
```

다른 Window가 열려 있는 상태에서 F1 또는 F2를 누르면 기존 Window를 닫고 새 대상 하나만 표시한다.

Scene이 변경되면 선택 상태를 초기화하고 모든 Debug Window를 다시 숨긴다.

---

### 5. 기존 Debug View의 내부 표시 상태 동기화

기존 Debug View들은 Component의 `enabled` 외에 별도의 내부 표시 변수를 사용하고 있었다.

대표적인 필드 이름은 다음과 같다.

```text
visible
isVisible
```

이 때문에 Component만 활성화해도 `OnGUI()` 내부에서 다시 숨겨지는 문제가 발생할 수 있었다.

97일차 Debug Hotkey 관리자는 Reflection을 이용해 해당 bool 필드를 함께 갱신한다.

```text
선택 Window
→ visible / isVisible = true
→ enabled = true

비선택 Window
→ visible / isVisible = false
→ enabled = false
```

이를 통해 F1 / F2 선택 상태와 기존 Debug View 내부 상태가 서로 어긋나는 문제를 방지한다.

---

### 6. Dedicated Server에서 Debug GUI 제외

`ProjectJDebugWindowMenu` 전체는 다음 조건으로 Client / Editor 계열에서만 포함된다.

```text
#if !UNITY_SERVER
```

따라서 Dedicated Server Build에는 Debug Hotkey 관리용 GUI가 실행되지 않는다.

Headless Server는 Network와 Server Mode 동작에만 집중하고, Client 측 개발용 Debug GUI와 분리된다.

---

## 주요 변경 파일

```text
Assets/ProjectJ/Editor/
├─ ProjectJDay97DedicatedServerBuilder.cs
└─ ProjectJDay97DedicatedServerBuilder.cs.meta

Assets/ProjectJ/Network/Fusion/Bootstrap/
├─ ProjectJDay97DedicatedServerDiagnostics.cs
├─ ProjectJDay97DedicatedServerDiagnostics.cs.meta
├─ ProjectJDebugWindowMenu.cs
└─ ProjectJDebugWindowMenu.cs.meta
```

기존 96일차 Server Scene을 Dedicated Build의 시작 Scene으로 재사용한다.

```text
Assets/ProjectJ/Scenes/Day96_ServerModeTest.unity
```

---

## Dedicated Server 실행 기준

빌드된 Server는 다음 명령 형태로 실행할 수 있다.

```text
Build\Server\Windows\ProjectJ_Server.exe -batchmode -nographics -logFile -
```

Server Mode에서 기대하는 기준은 다음과 같다.

```text
UNITY_SERVER    : True
GameMode        : Server
ProvideInput    : False
InputProvider   : 없음
Server Player   : 없음
Room Code       : 960001
```

Client 접속 시에는 Server 자체 Player가 아니라 접속한 Client에 대응하는 Network Player만 생성되는 구조를 사용한다.

---

## Debug Hotkey 기준

97일차 최종 Debug 입력 기준은 다음과 같다.

```text
시작
→ 모든 관리 대상 Debug Window 숨김

F1
→ 첫 번째 Debug Window 표시

F1 다시 입력
→ 첫 번째 Debug Window 숨김

F2
→ 두 번째 Debug Window 표시

F2 다시 입력
→ 두 번째 Debug Window 숨김

다른 Hotkey 선택
→ 이전 Window 숨김
→ 선택 Window 하나만 표시

Scene 전환
→ 선택 초기화
→ 모든 Window 숨김
```

기존 숫자키 `1~9, 0` 선택 방식은 사용하지 않는다.

---

## 검토 결과

최신 GitHub 커밋 `6bf3819aaa84a40425ff59f011a9aa005bafed50`을 96일차 커밋과 비교해 다음을 확인했다.

- 96일차 커밋에서 97일차 커밋까지 1개 커밋이 추가됨
- Windows Dedicated Server Builder가 추가됨
- `StandaloneBuildSubtarget.Server`를 사용하도록 구성됨
- Server Build 시작 Scene이 실제 존재하는 `Day96_ServerModeTest.unity`로 지정됨
- Dedicated Server 전용 `UNITY_SERVER` 진단 로그가 추가됨
- Debug GUI 관리 코드는 `!UNITY_SERVER`에서만 포함됨
- 기존 숫자키 선택 로직이 제거되고 F1 / F2 직접 선택 구조로 변경됨
- F1은 첫 번째, F2는 두 번째 관리 대상 Debug Window를 전환함
- 같은 Hotkey를 다시 누르면 해당 Window가 닫히도록 구성됨
- `visible` / `isVisible` 내부 bool 상태를 함께 동기화하도록 구성됨
- Scene 전환 시 Debug Window 선택 상태가 초기화됨
- 최신 커밋에 등록된 GitHub CI / 자동 빌드 상태 체크는 없음

GitHub에서 확인 가능한 소스 구조 기준으로 즉시 확인되는 명백한 차단 문제는 발견하지 못했다.

다만 GitHub 저장소만으로 Unity Editor의 실제 `Console Error 0`, Windows Dedicated Server Build 성공, `ProjectJ_Server.exe` 실행, Client 접속 및 재접속 성공까지 증명할 수는 없다. 해당 항목은 로컬 Unity 실행 결과를 최종 기준으로 판단한다.

---

## 최신 커밋

```text
SHA
6bf3819aaa84a40425ff59f011a9aa005bafed50

현재 커밋 메시지
97
```

---

## 97일차 결과

97일차에서는 96일차에 만든 Fusion Server Mode를 실제 Windows Dedicated Server Build 경로로 확장했다.

Unity Editor에서 Server Subtarget을 사용해 `ProjectJ_Server.exe`를 생성할 수 있는 Builder와 Server 전용 시작 진단 로그를 추가했으며, Client 개발 화면에 누적되어 있던 Debug Window 관리 방식도 정리했다.

Debug 입력은 기존 F1 선택 메뉴와 숫자키 방식에서 F1 / F2 직접 Hotkey 방식으로 단순화했고, 기존 Debug View의 `visible` / `isVisible` 상태까지 함께 제어해 표시 상태가 어긋나는 문제를 줄였다.

다음 단계에서는 Dedicated Server가 실제 경기의 이동, Checkpoint, FINISH, 아이템 판정 같은 권한을 어느 범위까지 담당할지 순차적으로 이전하고 검증한다.
