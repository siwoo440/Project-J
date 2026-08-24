---

# Project J - 103일차 개발일지

---

## 개발 방향

101일차부터 수집한 이동 품질 진단값을 실제 `NetworkTransform` 동작 경로와 연결할 수 있도록 런타임 적용 상태를 F6 화면에 표시한다.

Inspector에 저장된 Forecast와 보정값을 바로 변경하지 않고, 각 Player가 실제로 사용하는 Render Timeframe, Physics Body, Forecast 활성 여부를 먼저 확인한다.

Player Prefab, `NetworkProjectConfig.fusion`, Prediction, 이동값과 NetworkTransform 보정값은 변경하지 않는다.

---

## 변경 내용

---

### 1. NetworkTransform 런타임 상태 공개

`ProjectJNetworkPlayer`에서 다음 런타임 상태를 확인할 수 있도록 읽기 전용 속성을 추가했다.

- NetworkTransform Physics Body 존재 여부
- 실제 Forecast Physics 활성 여부
- Remote Render Timeframe 강제 여부
- 실제 Remote Render Timeframe 사용 여부
- 현재 Render Timeframe 표시 문자열

Inspector의 직렬화 값이 아니라 Fusion이 런타임에서 계산한 결과를 기준으로 표시한다.

---

### 2. NetworkTransform 진단 정책 추가

`ProjectJNetworkTransformDiagnosticsPolicy`를 추가해 F6 화면의 판단 규칙을 Runtime 정책으로 분리했다.

Physics Forecast 상태는 다음과 같이 구분한다.

| 표시 | 의미 |
| --- | --- |
| `NO PHYSICS BODY` | Rigidbody 또는 Rigidbody2D 동기화 대상이 아님 |
| `FORECAST INACTIVE` | Physics Body는 있지만 Forecast가 비활성 |
| `FORECAST ACTIVE` | Physics Body와 Forecast가 모두 활성 |

Render 경로는 다음과 같이 구분한다.

| 표시 | 의미 |
| --- | --- |
| `NO NETWORK TRANSFORM` | NetworkTransform 미존재 |
| `FORCED REMOTE` | Remote Timeframe 강제 사용 |
| `REMOTE INTERPOLATED` | 원격 Snapshot 보간 경로 |
| `LOCAL TIMEFRAME` | 로컬 또는 예측 시간축 사용 |

Physics Body와 Forecast가 모두 활성일 때만 Physics 보정값 조정 대상으로 판단한다.

---

### 3. F6 진단 화면 확장

F6 화면 제목을 103일차 기준으로 변경했다.

```text
DAY 103 - NETWORK TRANSFORM RUNTIME STATE / F6 Toggle
```

각 Player의 네 번째 줄에 다음 항목을 추가했다.

```text
NT Frame
Path
Physics
Tune
ForceRemote
```

플레이어당 표시 줄이 세 줄에서 네 줄로 늘어남에 따라 진단창 높이 계산도 함께 조정했다.

---

### 4. EditMode 테스트 추가

`ProjectJNetworkTransformDiagnosticsPolicyTests`에 총 10개 테스트 사례를 구성했다.

- Physics Body가 없을 때 `NO PHYSICS BODY` 반환
- Physics Body가 있고 Forecast가 꺼졌을 때 `FORECAST INACTIVE` 반환
- Physics Body와 Forecast가 활성일 때 `FORECAST ACTIVE` 반환
- Physics 보정값 조정 불가 사례 2개
- Physics 보정값 조정 가능 사례 1개
- NetworkTransform 미존재 Render 경로
- 강제 Remote Render 경로
- 정상 Remote 보간 경로
- Local Timeframe 경로

---

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
└─ ProjectJNetworkPlayer.cs

Assets/ProjectJ/Network/Fusion/Test/
└─ ProjectJDay79NetworkConditionDebugView.cs

Assets/ProjectJ/Runtime/Debugging/
├─ ProjectJNetworkTransformDiagnosticsPolicy.cs
└─ ProjectJNetworkTransformDiagnosticsPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJNetworkTransformDiagnosticsPolicyTests.cs
└─ ProjectJNetworkTransformDiagnosticsPolicyTests.cs.meta
```

- 수정 파일: 2개
- 생성 파일: 4개
- 삭제 파일: 없음

Scene, Hierarchy, Inspector와 Player Prefab 변경은 없다.

---

## 확인 절차

1. Unity의 `Window → General → Test Runner`에서 EditMode를 연다.
2. `ProjectJNetworkTransformDiagnosticsPolicyTests`의 테스트 사례 10개를 실행한다.
3. Console에 컴파일 Error가 없는지 확인한다.
4. Host와 Client에서 Steam을 실행하고 서로 다른 계정으로 로그인한다.
5. Windows Development Build에서 Private Room을 생성하고 Room Code로 참가한다.
6. Lobby를 거쳐 Game Scene에 진입한다.
7. 양쪽 PC에서 F6을 눌러 Day103 진단 화면을 표시한다.
8. Client 화면에서 상대 Player의 역할이 `REMOTE PROXY`인지 확인한다.
9. 상대 Player의 `NT Frame`과 `Path`를 확인한다.
10. `Physics`, `Tune`, `ForceRemote` 표시를 확인한다.
11. 각 동작 전에 F10으로 측정 구간을 초기화한다.
12. 정지, 걷기, 달리기, 점프, 앉기, 부활과 카메라 회전을 각각 측정한다.
13. Host·Client 전체 경기 1회를 완료하고 Console Error가 0건인지 확인한다.

---

## 검증 결과

GitHub 최신 커밋의 변경 파일 6개가 103일차 배포 패키지와 일치하는 것을 확인했다.

- 변경 범위: 수정 2개, 생성 4개, 삭제 0개
- 배포 패키지와 최신 커밋 파일 6개 바이트 일치
- Git diff 공백 오류 없음
- Fusion SDK의 `HasPhysicsBody`, `HasForecastEnabled`, `RenderTimeframe`, `ForceRemoteRenderTimeframe` API 확인
- Runtime과 EditMode 어셈블리 참조 구조 확인
- `.meta` GUID 중복 없음
- Player Prefab과 `NetworkProjectConfig.fusion` 미변경 확인

현재 검증 환경에는 Unity Editor가 없어 실제 컴파일, EditMode Test Runner, Windows Development Build와 Host·Client 2인 접속은 실행하지 못했다. 실제 런타임의 `NT Frame`, `Path`, `Physics`, `Tune`, `ForceRemote` 결과는 Unity에서 최종 확인이 필요하다.

---

## 구현 확인 기준 커밋

개발일지 반영 전 확인한 커밋은 다음과 같다.

```text
fdcda121ae1aed39a4fcd72239045d3d8b32f9f3
103
```

---

## 103일차 결과

Player별 NetworkTransform Render 경로와 Physics Forecast 적용 상태를 실제 런타임 값으로 구분할 수 있는 진단 기반을 구성했다.

다음 일차에서는 Host·Client 측정 결과에서 원격 Proxy만 끊기는 것이 확인된 경우에만 실제 적용 가능한 보간 항목 하나를 선택해 비교한다. 원격 Proxy 문제가 확인되지 않으면 NetworkTransform 값을 변경하지 않는다.
