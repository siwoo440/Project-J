# 8일차 개발일지 - Windows Development Build Profile 구축

## 오늘의 목표

Unity Editor 안에서만 프로젝트를 확인하는 단계를 넘어,
현재 Project J 상태를 실제 Windows 실행 파일로 반복 빌드할 수 있는
개발용 Build Profile 기준을 구축한다.

이번 일차에서는 릴리스 빌드나 Steam 배포 설정을 진행하지 않고,
개발 중 기능 확인에 사용할 Windows Development Build 환경만 구성한다.

## 구현 내용

### 1. Windows Development Build Profile 생성

Unity 6의 Build Profiles 기능을 이용해
다음 개발용 프로파일을 생성했다.

```text
ProjectJ_Windows_Development
```

Windows 플랫폼을 대상으로 사용하는
Project J 전용 개발 빌드 프로파일이다.

### 2. Development Build 활성화

개발 과정에서 디버깅과 문제 확인에 사용할 수 있도록
Development Build를 활성화했다.

설정 기준:

```text
Development Build       ON
Autoconnect Profiler    OFF
Deep Profiling          OFF
Script Debugging        OFF
```

현재 단계에서는 일반적인 개발 빌드에 필요한 기능만 활성화하고,
불필요하게 실행 성능에 영향을 줄 수 있는 옵션은 사용하지 않는다.

### 3. 개발용 압축 방식 설정

개발 빌드의 반복 생성 속도를 고려해
Compression Method를 LZ4 기준으로 설정했다.

이를 통해 이후 기능 구현 과정에서
Windows 빌드를 반복적으로 생성하고 확인하기 위한 기준을 마련했다.

### 4. Windows 64-bit 빌드 기준 확정

Windows PC용 빌드 프로파일을 사용하도록 구성하고,
Project J의 기본 PC 개발 빌드 기준을 확정했다.

이후 Steam 및 정식 Windows 빌드도
현재 PC 빌드 기반을 확장하는 방향으로 진행할 수 있다.

### 5. 기존 Global Scene List 사용

Build Profile에서 별도의 Scene 목록을 덮어쓰지 않고
기존 프로젝트의 Global Scene List를 그대로 사용하도록 설정했다.

현재 Scene 순서:

```text
Bootstrap
MainMenu
Game
```

따라서 Windows Build에서도
Bootstrap을 최초 진입 Scene으로 사용하는 기존 흐름을 유지한다.

### 6. Build Profile 에셋 저장

Unity가 생성한 Build Profile 에셋이
프로젝트 내부에 저장되도록 구성했다.

생성된 주요 에셋:

```text
Assets/Settings/Build Profiles/ProjectJ_Windows_Development.asset
```

Build Profile이 프로젝트 Asset으로 저장되므로
Git을 통해 개발 환경 설정을 함께 관리할 수 있다.

### 7. Unity 자동 설정 갱신

Build Profile 생성 및 Windows Build 설정 과정에서
Unity가 일부 프로젝트 및 URP 설정 파일을 다시 저장했다.

주요 변경 대상:

```text
Assets/Settings/PC_RPAsset.asset
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
ProjectSettings/ProjectSettings.asset
ProjectSettings/UnityConnectSettings.asset
```

이 변경들은 Build Profile 생성과 프로젝트 설정 저장 과정에서
Unity가 현재 버전 기준으로 직렬화하거나 필요한 빌드 설정을 반영한 내용이다.

## 테스트

Windows Development Build를 생성한 뒤
실제 실행 파일에서 다음 Scene 흐름을 확인한다.

```text
Bootstrap
↓
MainMenu
↓
START
↓
Game
↓
BACK TO MENU
↓
MainMenu
```

확인 항목:

- Windows Build 성공
- 실행 파일 정상 실행
- Bootstrap 최초 진입 확인
- MainMenu 표시 확인
- START 버튼으로 Game 이동 확인
- Game에서 MainMenu 복귀 확인
- Development Build 설정 확인
- Unity Console Error 0 확인

## 결과

Project J의 Windows 개발 빌드 기준을 구축했다.

이제 Unity Editor에서만 기능을 확인하는 것이 아니라
현재 프로젝트 상태를 실제 Windows 실행 파일로 빌드해
Editor 외부 환경에서도 반복 검증할 수 있다.

앞으로 플레이어 조작, 맵 생성, 멀티플레이 등의 기능을 추가한 뒤에도
동일한 Windows Development Build Profile을 이용해
실제 실행 환경 기준으로 기능을 확인할 수 있는 기반이 마련되었다.
