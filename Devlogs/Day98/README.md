# Project J - 98일차 개발일지

---

## 개발 방향

Dedicated Server 실험 코드는 삭제하지 않고 보존하면서, 일반 Windows 실행 흐름을 Photon Fusion Host Mode 중심으로 복구한다.

일반 실행 경로는 다음 순서를 유지한다.

```text
Bootstrap
→ MainMenu
→ Private Match
→ Lobby
→ Game
```

---

## 변경 내용

### 1. 네트워크 실행 모드 정책 추가

`ProjectJNetworkExecutionPolicy`를 추가해 일반 Host/Client 실행과 Dedicated Server 실행 조건을 한곳에서 판정하도록 구성했다.

- 일반 Windows·Editor 실행: Host/Client Bootstrap 설치
- `UNITY_SERVER` 실행: Host/Client Bootstrap 설치 차단
- Dedicated 자동 시작: `UNITY_SERVER`와 `startOnPlay`가 모두 참일 때만 허용

### 2. Host/Client Bootstrap 복구

기존 Runtime Installer가 Day96 Server Mode 컴포넌트 존재 여부로 일반 Bootstrap 설치를 중단하던 조건을 제거했다.

일반 실행에서는 Day96 실험 컴포넌트가 존재하더라도 `ProjectJFusionBootstrap`과 Lobby·Scene Flow 구성요소를 설치한다.

### 3. Dedicated Server 자동 시작 격리

`ProjectJDay96ServerModeBootstrap`의 자동 시작을 `UNITY_SERVER` 빌드로 제한했다.

일반 Windows Build와 Unity Editor에서는 `startOnPlay`가 켜져 있어도 Server Mode가 자동 실행되지 않는다. 기존 수동 실행 메서드와 Day96·97 실험 파일은 삭제하지 않았다.

### 4. EditMode 회귀 테스트 추가

다음 조건을 검증하는 `ProjectJNetworkExecutionPolicyTests`를 추가했다.

- 일반 실행에서 Host/Client Bootstrap 설치 허용
- Dedicated Server 빌드에서 Host/Client Bootstrap 설치 차단
- Dedicated 자동 시작은 Server 빌드와 자동 시작 설정이 모두 활성화된 경우에만 허용

---

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
├─ ProjectJFusionBootstrapRuntimeInstaller.cs
└─ ProjectJDay96ServerModeBootstrap.cs

Assets/ProjectJ/Runtime/SceneFlow/
├─ ProjectJNetworkExecutionPolicy.cs
└─ ProjectJNetworkExecutionPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJNetworkExecutionPolicyTests.cs
└─ ProjectJNetworkExecutionPolicyTests.cs.meta

Devlogs/Day98/
└─ README.md
```

삭제한 파일은 없다.

---

## 실제 확인 결과

Unity Editor에서 `Bootstrap → MainMenu → Private Match` 경로로 진입해 Host Room 생성을 확인했다.

첫 실행에서는 Steam Client가 꺼져 있어 다음 경고와 함께 방 생성이 중단됐다.

```text
[Project J/Fusion] Steam 인증 필요: Steam Client가 실행 중이 아닙니다.
```

Steam Client를 실행하고 로그인한 뒤 다시 시도하자 Steam 인증 단계를 통과하고 Private Room 생성 흐름이 진행됐다.

현재 98일차 Host Mode 실행에는 다음 조건이 필요하다.

- Steam Client 실행
- Steam 계정 로그인
- 프로젝트 루트의 `steam_appid.txt` 유지
- `Bootstrap` Scene부터 일반 실행
- Dedicated Server Build와 `Day96_ServerModeTest` Scene 미사용

일반 실행에서 Dedicated Server가 자동 시작되는 현상은 확인되지 않았다.

다음 일차 진행 전 EditMode Test Runner 전체 통과, Host 1명과 Client 1명의 Room Code 참가, Lobby·Game 진입과 Console Error 0건을 최종 확인한다.

---

## 기준 커밋

```text
0942006ac3ffceac437e5ca5119798e478593116
98
```

---

## 98일차 결과

Day96~97 Dedicated Server 실험 코드는 보존하면서 일반 Host/Client 실행과 자동 시작 조건을 분리했다.

일반 Windows 실행에서는 Host Mode 기반 Bootstrap이 설치되고, Dedicated Server 자동 시작은 `UNITY_SERVER` 빌드에서만 허용된다.

Steam Client가 실행된 환경에서 Private Room 생성 흐름이 진행되는 것을 확인했으며, Steam 실행과 로그인이 현재 Host Mode 테스트의 선행 조건임을 기록했다.
