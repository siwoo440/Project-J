---

# Project J - 102일차 개발일지

---

## 개발 방향

101일차 이동 품질 진단 화면을 Host에서도 안전하게 사용할 수 있도록 네트워크 디버그 단축키 충돌을 해소한다.

F6은 이동 품질 진단 전용으로 유지하고, 기존 강제 경기 종료 기능은 F11로 분리한다. 단축키 배정을 공통 정책으로 관리하고 EditMode 테스트를 추가해 이후 기능 확장에서도 중복을 확인할 수 있도록 구성한다.

Player Prefab, Scene, Prediction과 `NetworkTransform` 설정은 변경하지 않는다.

---

## 변경 내용

---

### 1. F6 단축키 충돌 해소

기존에는 F6이 다음 두 기능에서 동시에 사용되고 있었다.

- Day101 이동 품질 진단 화면 전환
- Host의 강제 경기 종료

Host가 진단 화면을 열기 위해 F6을 누르면 경기가 함께 종료될 수 있으므로 기능을 다음과 같이 분리했다.

| 단축키 | 기능 |
| --- | --- |
| `F5` | 단독 경기 시작 |
| `F6` | 이동 품질 진단 화면 전환 |
| `F10` | 측정 구간 초기화 |
| `F11` | Host 강제 경기 종료 |

---

### 2. 공통 단축키 정책 추가

`ProjectJNetworkDebugHotkeyPolicy`를 추가해 네트워크 디버그 기능과 실제 키 배정을 Runtime 정책으로 분리했다.

정책에서 관리하는 기능은 다음과 같다.

- `SoloStart` → F5
- `MovementDiagnostics` → F6
- `MeasurementReset` → F10
- `ForceMatchEnd` → F11
- 미등록 기능 → `Key.None`
- 전체 등록 키 중복 검사

기존 입력 코드는 직접 F키를 조회하지 않고 공통 정책에서 키를 받아 사용하도록 변경했다.

---

### 3. 강제 경기 종료 조건 강화

F11 강제 종료는 다음 조건을 모두 만족할 때만 실행되도록 구성했다.

1. Game Scene 또는 Day49 통합 테스트 Scene 실행
2. State Authority 보유
3. 현재 객체가 Match Coordinator
4. 실제 경기 입력이 허용된 상태

Lobby, 경기 시작 전, 결과 확정 후에는 F11을 눌러도 강제 종료가 실행되지 않는다.

---

### 4. 진단 화면 안내 갱신

F6 네트워크 진단 화면 제목과 단축키 안내를 102일차 기준으로 변경했다.

```text
DAY 102 - DEBUG HOTKEYS & MOVEMENT QUALITY / F6 Toggle
F10 : RESET
F11 : FORCE END (HOST)
```

기존 경기 디버그 화면의 안내도 `F11: Force End`로 수정했다.

---

### 5. EditMode 테스트 추가

`ProjectJNetworkDebugHotkeyPolicyTests`에 다음 테스트를 구성했다.

- F5 단독 경기 시작 키 확인
- F6 이동 품질 진단 키 확인
- F10 측정 초기화 키 확인
- F11 강제 경기 종료 키 확인
- 등록된 단축키 전체 중복 없음 확인
- 미등록 기능의 `Key.None` 반환 확인
- 정상 Host 경기의 강제 종료 허용 확인
- Game Scene 미활성 상태 차단 확인
- State Authority 미보유 상태 차단 확인
- Match Coordinator 불일치 상태 차단 확인
- 경기 진행 전후 상태 차단 확인

총 11개 테스트 사례로 구성했다.

---

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
└─ ProjectJNetworkExternalGameplay.cs

Assets/ProjectJ/Network/Fusion/Test/
└─ ProjectJDay79NetworkConditionDebugView.cs

Assets/ProjectJ/Runtime/Debugging/
├─ ProjectJNetworkDebugHotkeyPolicy.cs
└─ ProjectJNetworkDebugHotkeyPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJNetworkDebugHotkeyPolicyTests.cs
└─ ProjectJNetworkDebugHotkeyPolicyTests.cs.meta
```

- 수정 파일: 2개
- 생성 파일: 4개
- 삭제 파일: 없음

Scene, Hierarchy, Inspector와 Player Prefab 변경은 없다.

---

## 확인 절차

1. Unity의 `Window → General → Test Runner`에서 EditMode를 연다.
2. `ProjectJNetworkDebugHotkeyPolicyTests`의 테스트 11개를 실행한다.
3. Console에 컴파일 Error가 없는지 확인한다.
4. Host와 Client에서 Steam을 실행하고 서로 다른 계정으로 로그인한다.
5. Windows Development Build에서 Private Room을 생성하고 Room Code로 참가한다.
6. Lobby를 거쳐 Game Scene에 진입한다.
7. Host와 Client에서 F6을 눌러 진단 화면만 전환되는지 확인한다.
8. F6 입력으로 경기가 종료되지 않는지 확인한다.
9. F10으로 측정 구간이 초기화되는지 확인한다.
10. Client의 F11 입력으로 경기가 종료되지 않는지 확인한다.
11. Host가 경기 진행 중 F11을 눌렀을 때만 강제 종료되는지 확인한다.
12. 기존 F5·F7·F8·F9·F12 기능이 유지되는지 확인한다.
13. Console Error가 0건인지 확인한다.

---

## 검증 결과

GitHub 최신 커밋의 변경 파일 6개가 102일차 배포 패키지와 일치하는 것을 확인했다.

- 변경 범위: 수정 2개, 생성 4개, 삭제 0개
- 배포 패키지와 최신 커밋 파일 6개 바이트 일치
- Git diff 공백 오류 없음
- 기존 F6 강제 종료 입력 잔존 없음
- F5·F6·F10·F11 정책 배정 확인
- Runtime과 EditMode 어셈블리의 Input System 참조 확인
- `.meta` GUID 중복 없음

현재 검증 환경에는 Unity Editor가 없어 실제 컴파일, EditMode Test Runner, Windows Development Build와 Host·Client 2인 접속은 실행하지 못했다. 해당 실행 결과는 Unity에서 최종 확인이 필요하다.

---

## 구현 확인 기준 커밋

개발일지 반영 전 확인한 커밋은 다음과 같다.

```text
d6e22cd64b9c7c2c3ba9277804412bda0180f5c8
102
```

---

## 102일차 결과

F6 이동 품질 진단과 Host 강제 경기 종료 입력을 분리하고, 네트워크 디버그 단축키를 공통 정책과 테스트로 관리할 수 있는 구조를 구성했다.

다음 일차에서는 101일차 진단값을 실제 Host·Client 환경에서 수집한 뒤, 원격 Proxy에서만 끊김이 확인될 때 `NetworkTransform` 보정값을 한 항목씩 비교한다.
