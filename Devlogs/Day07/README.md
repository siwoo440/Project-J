# 7일차 개발일지 - EditMode·PlayMode 테스트 기반 구축

## 오늘의 목표

Project J에서 기능을 구현할 때
직접 플레이해보는 방식만으로 검증하지 않고
Unity Test Framework를 이용해 자동 테스트할 수 있는 기본 구조를 만든다.

이번 일차에서는 실제 플레이어 이동이나 게임 규칙 테스트를 만들지 않고,
EditMode와 PlayMode 테스트가 정상적으로 실행될 수 있는 기반을 구축한다.

## 구현 내용

### 1. EditMode 테스트 Assembly 구성

다음 EditMode 전용 테스트 Assembly를 추가했다.

```text
Assets/ProjectJ/Tests/EditMode/ProjectJ.Tests.EditMode.asmdef
```

`ProjectJ.Runtime`을 참조하도록 설정하고
`TestAssemblies`를 활성화해 Unity Test Framework에서
테스트 Assembly로 인식되도록 구성했다.

EditMode 테스트는 게임을 실행하지 않고도
순수 코드와 데이터 로직을 빠르게 검증할 때 사용한다.

### 2. GameDataId EditMode 테스트 추가

6일차에 만든 `GameDataId`를 대상으로
기본 자동 테스트를 추가했다.

검증 항목:

- 새로 생성된 ID가 32자리인지 확인
- 생성된 ID가 유효한 GUID 형식인지 확인
- 연속으로 생성한 두 ID가 서로 다른지 확인
- 빈 문자열이 유효하지 않은 ID로 판정되는지 확인
- `invalid` 문자열이 유효하지 않은 ID로 판정되는지 확인
- `1234`가 유효하지 않은 ID로 판정되는지 확인

테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/GameDataIdTests.cs
```

### 3. PlayMode 테스트 Assembly 구성

다음 PlayMode 전용 테스트 Assembly를 추가했다.

```text
Assets/ProjectJ/Tests/PlayMode/ProjectJ.Tests.PlayMode.asmdef
```

EditMode와 분리해
실제 Unity PlayMode 환경이 필요한 테스트를 별도로 관리하도록 했다.

이후 플레이어 이동, 체크포인트, 맵 생성 등
실제 Scene과 Frame 진행이 필요한 시스템의 테스트를
이 구조 아래 추가할 수 있다.

### 4. PlayMode Smoke Test 추가

현재는 실제 게임 기능이 아직 충분히 구현되지 않았기 때문에
복잡한 PlayMode 테스트 대신
테스트 환경 자체가 정상적으로 동작하는지 확인하는
최소 Smoke Test를 추가했다.

동작:

```text
PlayMode 시작
→ 1 Frame 대기
→ Application.isPlaying 확인
```

테스트 파일:

```text
Assets/ProjectJ/Tests/PlayMode/ProjectSmokeTests.cs
```

### 5. 테스트 구조 분리

최종 테스트 폴더 구조는 다음과 같다.

```text
Assets/ProjectJ/Tests/
├─ EditMode/
│  ├─ ProjectJ.Tests.EditMode.asmdef
│  └─ GameDataIdTests.cs
└─ PlayMode/
   ├─ ProjectJ.Tests.PlayMode.asmdef
   └─ ProjectSmokeTests.cs
```

앞으로 시스템 성격에 따라
EditMode와 PlayMode 테스트를 각각 분리해서 추가한다.

## 테스트

### EditMode

Unity Test Runner에서 EditMode 테스트를 실행한다.

확인 항목:

- `GameDataIdTests`가 Test Runner에 표시되는지 확인
- 모든 ID 생성 및 유효성 테스트 통과 확인
- 전체 EditMode 테스트 Green 확인

### PlayMode

Unity Test Runner에서 PlayMode 테스트를 실행한다.

확인 항목:

- `ProjectSmokeTests`가 Test Runner에 표시되는지 확인
- PlayMode가 정상적으로 시작되는지 확인
- 1 Frame 진행 후 테스트 통과 확인
- 전체 PlayMode 테스트 Green 확인

### 최종 확인

- EditMode 전체 테스트 Green
- PlayMode 전체 테스트 Green
- Unity Console Error 0 확인

## 결과

Project J에 Unity Test Framework 기반의
EditMode 및 PlayMode 테스트 구조를 구축했다.

이후 기능 구현 시
코드와 데이터는 EditMode,
실제 실행 환경이 필요한 기능은 PlayMode에서 검증할 수 있는
기본 테스트 흐름이 마련되었다.

앞으로 플레이어 이동, 체크포인트, 랭킹,
맵 생성 등의 기능을 구현할 때
해당 기능에 맞는 자동 테스트를 단계적으로 추가할 수 있다.
