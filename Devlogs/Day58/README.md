# Project J - 58일차 개발 일지

## 개발 목표

PHASE 6의 첫 번째 일차로 Photon Fusion 2를 Project J에 도입하고, 기존 오프라인 게임 시스템과 분리된 최소 `NetworkRunner` Bootstrap 구조를 구축한다.

이번 일차에서는 플레이어 이동이나 아이템을 네트워크화하지 않고 다음 기반만 준비한다.

```text
Photon Fusion 2 도입
↓
Fusion App 설정
↓
NetworkRunner Bootstrap 생성
↓
Host Mode 시작 / 종료
↓
개발용 네트워크 상태 UI
↓
기존 오프라인 시스템과 네트워크 계층 분리
```

---

## 주요 개발 내용

### 1. Photon Fusion 2 SDK 도입

Photon Fusion 2 SDK를 프로젝트에 추가했다.

주요 추가 경로:

```text
Assets/Photon/Fusion/
```

Fusion Runtime, Editor, CodeGen, NetworkProjectConfig, PhotonAppSettings 및 플랫폼별 네트워크 플러그인이 프로젝트에 포함되었다.

기존 NGO/Dedicated 관련 레거시 코드는 수정하지 않고 기존 `Assets/ProjectJ/Legacy/Networking` 영역에 그대로 격리된 상태를 유지한다.

---

### 2. Photon Fusion App 설정

Fusion용 `PhotonAppSettings`를 프로젝트에 생성하고 Fusion App ID를 연결했다.

설정 파일:

```text
Assets/Photon/Fusion/Resources/
└─ PhotonAppSettings.asset
```

이를 통해 Project J가 Photon Cloud의 Fusion 애플리케이션을 사용하여 Session 연결을 시작할 수 있는 기반을 마련했다.

---

### 3. Fusion 전용 Network 계층 생성

기존 Runtime 게임 시스템과 Photon Fusion 코드를 직접 섞지 않도록 별도 네트워크 경로를 추가했다.

```text
Assets/ProjectJ/Network/Fusion/
└─ Bootstrap/
```

이후 PHASE 6의 플레이어 Spawn, 입력, 이동, Authority, Match 시스템 네트워크화도 이 계층을 중심으로 확장한다.

---

### 4. ProjectJFusionBootstrap 구현

`ProjectJFusionBootstrap`을 추가해 최소 `NetworkRunner` 생명주기를 관리하도록 했다.

지원 상태:

```text
Idle
Starting
Running
Stopping
Failed
```

현재 지원하는 동작:

```text
Host 시작
Client 접속 요청
Runner 종료
Session Name 지정
현재 역할 확인
현재 연결 상태 확인
실패 상태 확인
```

기본 Session Name:

```text
ProjectJ-Day58
```

---

### 5. NetworkRunner Runtime 생성

Fusion 연결을 시작할 때만 Runtime에서 `NetworkRunner`를 생성하도록 구성했다.

```text
=== Project J Fusion Bootstrap ===
        ↓
Host / Client 시작 요청
        ↓
=== Fusion NetworkRunner ===
        ↓
NetworkRunner.StartGame()
```

게임을 실행했다는 이유만으로 자동으로 온라인 Session에 참가하지 않는다.

Runner 종료 후에는 기존 Runner를 다시 사용하지 않고 새로운 연결 시 새로운 Runner를 생성하도록 구성했다.

---

### 6. 현재 Scene을 Fusion Scene으로 연결

현재 활성화된 Unity Scene의 Build Index가 존재하면 `NetworkSceneInfo`에 Scene을 등록하도록 했다.

현재 Fusion 2 API에 맞춰:

```text
SceneRef.FromIndex(...)
+
LoadSceneMode.Single
```

방식을 사용한다.

개발 중 확인된 이전 `AddSceneRef()` 인수 형식 오류는 수정했다.

---

### 7. Input 네트워크화는 보류

58일차에서는 아직 Fusion Tick Input을 구현하지 않으므로:

```text
runner.ProvideInput = false
```

상태로 시작한다.

따라서 다음 기능은 아직 기존 오프라인 시스템을 그대로 사용한다.

```text
플레이어 이동
점프
달리기
앉기
밀치기
아이템
체크포인트
부활
순위
경기 결과
```

이 기능들은 이후 PHASE 6에서 순차적으로 Fusion Simulation에 연결한다.

---

### 8. Runtime Bootstrap Installer 추가

씬에 Fusion 오브젝트를 직접 배치하지 않아도 Runtime에서 Bootstrap을 자동 생성하도록 `ProjectJFusionBootstrapRuntimeInstaller`를 추가했다.

게임 실행 후:

```text
=== Project J Fusion Bootstrap ===
```

오브젝트가 한 번만 생성되고 Scene 변경 후에도 유지된다.

기존 Bootstrap이 존재하면 중복 생성하지 않는다.

---

### 9. F2 네트워크 테스트 UI 추가

Fusion 연결 상태를 빠르게 확인하기 위한 개발용 UI를 추가했다.

조작:

```text
F2
→ Fusion 테스트 창 표시 / 숨김
```

UI에서 확인할 수 있는 정보:

```text
세션 이름
현재 상태
현재 역할
Bootstrap 상태 메시지
```

제공 버튼:

```text
Host 시작
Client 접속
Runner 종료
```

Editor 또는 Development Build에서만 개발용 테스트 UI를 사용할 수 있도록 구성했다.

---

### 10. 개발용 ALT 커서 해제 기능 추가

기존 3인칭 카메라는 Play Mode에서 마우스 커서를 잠그기 때문에 F2 테스트 UI 버튼을 클릭할 수 없는 문제가 있었다.

이를 해결하기 위해 `ProjectJDebugCursorReleaseController`를 추가했다.

동작:

```text
평상시
→ 기존 커서 잠금 상태 유지

ALT 누르고 있음
→ Cursor Lock 해제
→ 커서 표시

ALT 해제
→ 이전 Cursor 상태 복구
```

왼쪽 ALT와 오른쪽 ALT 모두 사용할 수 있다.

이 기능 역시 Editor 또는 Development Build에서만 동작한다.

---

## 생성 파일

Project J 전용 코드:

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
├─ ProjectJFusionBootstrap.cs
├─ ProjectJFusionBootstrapDebugView.cs
└─ ProjectJFusionBootstrapRuntimeInstaller.cs

Assets/ProjectJ/Runtime/Debugging/
└─ ProjectJDebugCursorReleaseController.cs
```

Photon SDK:

```text
Assets/Photon/Fusion/
└─ Photon Fusion 2 SDK 관련 Runtime / Editor / CodeGen / Resources / Plugins
```

---

## 주요 설정 파일

```text
Assets/Photon/Fusion/Resources/
├─ NetworkProjectConfig.fusion
└─ PhotonAppSettings.asset
```

---

## 수정 파일

Project J 기존 오프라인 Runtime 기능에 대한 직접 수정:

```text
없음
```

---

## 삭제 파일

```text
없음
```

---

## 네트워크 Bootstrap 동작 구조

```text
게임 실행
↓
ProjectJFusionBootstrapRuntimeInstaller
↓
ProjectJFusionBootstrap 자동 생성
↓
상태 = Idle
↓
F2
↓
네트워크 테스트 UI 표시
↓
ALT를 누른 상태에서 버튼 조작
↓
Host 시작
↓
NetworkRunner 생성
↓
Fusion StartGame
↓
상태 = Running
↓
역할 = Host
```

종료:

```text
Runner 종료
↓
NetworkRunner.Shutdown()
↓
Runner 오브젝트 제거
↓
상태 = Idle
↓
새 연결 시 새로운 Runner 생성
```

---

## 테스트 결과

### 컴파일

초기 구현 과정에서 현재 Fusion 2의 `NetworkSceneInfo.AddSceneRef()` API와 맞지 않는 인수 사용으로 컴파일 오류가 발생했다.

기존:

```text
LoadSceneMode + bool
```

형태를 제거하고 현재 설치된 Fusion 2 API에 맞게 수정했다.

수정 후 F2 네트워크 테스트 UI가 정상 실행되는 것을 확인했다.

---

### Host Mode

F2 테스트 UI에서 Host 시작 후 다음 상태를 확인했다.

```text
세션 이름 : ProjectJ-Day58
상태 : 실행 중
역할 : 호스트
호스트 실행 중
```

Host가 실행 중일 때:

```text
Host 시작
→ 비활성

Client 접속
→ 비활성

Runner 종료
→ 활성
```

상태가 정상적으로 전환되는 것도 확인했다.

---

### 커서 제어

```text
ALT 누름
→ 마우스 커서 활성화
→ F2 버튼 조작 가능

ALT 해제
→ 기존 게임 커서 상태 복구
```

동작을 확인했다.

---

## 58일차 완료 범위

이번 일차에서 완료한 범위:

```text
Photon Fusion 2 SDK 도입
↓
Fusion App 설정
↓
Fusion 전용 코드 경로 분리
↓
NetworkRunner Bootstrap 구현
↓
Runtime 자동 생성
↓
Host Mode 시작
↓
Host 상태 표시
↓
Runner 종료 구조
↓
F2 네트워크 디버그 UI
↓
ALT 개발용 커서 조작
↓
기존 오프라인 Runtime 유지
```

현재 업로드된 실행 결과에서는 Host Mode가 정상적으로 `Running` 상태에 진입한 것을 확인했다.

Client 인스턴스와의 실제 2인 동시 접속은 다음 비공개 Session 생성·참가 흐름을 구현하면서 이어서 검증한다.

---

## 다음 개발 방향

59일차에서는 현재 `ProjectJFusionBootstrap`을 기반으로 비공개 Session 생성·참가 흐름을 구현한다.

주요 목표:

```text
Host 비공개 Session 생성
↓
Client Session 참가
↓
동일 Session 연결 확인
↓
참가 / 이탈 처리
↓
2개 인스턴스 연결 검증
```

아직 Player Spawn이나 이동 네트워크화는 진행하지 않는다.
