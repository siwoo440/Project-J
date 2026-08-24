# Project J - 101일차 개발일지

---

## 개발 방향

100일차에서 로컬 카메라 위치 보간을 적용한 뒤 남아 있는 이동 끊김을 로컬 Player, State Authority Player, 원격 Proxy로 구분해 측정할 수 있도록 진단 기능을 확장한다.

Prediction과 `NetworkTransform` 설정은 변경하지 않고 동일한 조건에서 Host와 Client의 이동 품질을 비교할 수 있는 측정 환경을 구성한다.

---

## 변경 내용

---

### 1. 플레이어 역할 표시 추가

각 PC에서 Player Object가 담당하는 역할을 다음 세 가지로 구분해 F6 진단 화면에 표시한다.

| 표시 | 의미 |
| --- | --- |
| `LOCAL INPUT` | 현재 PC에서 직접 조작하는 Player |
| `STATE AUTHORITY` | Host가 상태 권한을 가진 Player |
| `REMOTE PROXY` | Snapshot으로 표시되는 원격 Player |

Host와 Client가 같은 Player를 서로 다른 역할로 보는지 구분할 수 있도록 구성했다.

---

### 2. 이동 품질 정책 추가

`ProjectJMovementQualityPolicy`를 추가해 진단 화면에서 사용하는 역할 판정과 측정값 계산을 Runtime 정책으로 분리했다.

- Input Authority 우선 역할 판정
- State Authority 역할 판정
- 권한이 없는 Player의 Remote Proxy 판정
- 음수 측정값 방지
- 측정 구간 최대값 누적
- 측정 시작 시각과 현재 시각의 경과 시간 계산

---

### 3. F10 측정 초기화 추가

각 동작을 독립된 구간으로 측정할 수 있도록 `F10` 초기화 기능을 추가했다.

F7은 Day80 Steam Identity, F8은 Day81 Steam Invite가 사용하고 있어 충돌하지 않는 F10을 사용한다.

F10 입력 시 다음 값이 초기화된다.

- RTT와 Jitter 표본
- Render Step
- Simulation Offset
- Camera Step
- Camera Follow Offset
- Resimulation Batch·Tick
- Rollback 거리
- 최근·최대 Correction 거리
- 측정 경과 시간

---

### 4. 측정 구간 최대값 표시

F6 진단 화면에 현재 측정 구간의 경과 시간과 최대값을 추가했다.

```text
MEASURE
PEAK Step
PEAK Offset
PEAK CameraStep
PEAK Follow
```

정지, 걷기, 달리기, 점프, 앉기, 부활을 시작하기 전에 F10을 눌러 각 동작의 결과를 분리할 수 있다.

---

### 5. 플레이어 동작 상태 표시

플레이어별 세 번째 진단 줄에 다음 상태를 표시한다.

- 이동 또는 정지
- 달리기
- 점프 입력
- 지면 판정
- 앉기
- 현재 이동 속도

이동 진단값이 증가한 순간의 Player 동작을 함께 확인할 수 있도록 구성했다.

---

### 6. EditMode 테스트 추가

`ProjectJMovementQualityPolicyTests`에 테스트 8개를 추가했다.

- Input Authority와 State Authority를 모두 가진 Host Player의 역할 판정
- State Authority만 가진 Player의 역할 판정
- 권한이 없는 Remote Proxy 역할 판정
- 더 큰 표본으로 최대값 갱신
- 더 작은 표본에서 기존 최대값 유지
- 음수 표본을 0으로 제한
- 정상 경과 시간 계산
- 역전된 시간에서 음수 결과 방지

---

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
└─ ProjectJNetworkPlayer.cs

Assets/ProjectJ/Network/Fusion/Test/
└─ ProjectJDay79NetworkConditionDebugView.cs

Assets/ProjectJ/Runtime/Debugging/
├─ ProjectJMovementQualityPolicy.cs
└─ ProjectJMovementQualityPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJMovementQualityPolicyTests.cs
└─ ProjectJMovementQualityPolicyTests.cs.meta

Devlogs/Day101/
└─ README.md
```

기능 코드 기준으로 수정 파일 2개, 생성 파일 4개이며 삭제한 파일은 없다.

Player Prefab, Scene, Hierarchy, Inspector 설정은 변경하지 않았다.

---

## 확인 절차

1. Unity의 `Window → General → Test Runner`에서 EditMode를 연다.
2. `ProjectJMovementQualityPolicyTests`의 테스트 8개를 실행한다.
3. Console에 컴파일 Error가 없는지 확인한다.
4. Host와 Client PC에서 Steam Client를 실행하고 서로 다른 계정으로 로그인한다.
5. 일반 Windows Development Build에서 Private Room을 생성하고 Room Code로 참가한다.
6. Lobby를 거쳐 Game Scene으로 진입한다.
7. 양쪽 PC에서 F6을 눌러 Day101 진단 화면을 표시한다.
8. `2P MEASURE GATE : PASS`를 확인한다.
9. 측정할 동작을 시작하기 전에 F10으로 누적값을 초기화한다.
10. 정지, Host 이동, Client 이동, 동시 이동, 달리기, 점프, 앉기, 부활을 각각 측정한다.
11. 각 동작을 바꿀 때마다 F10으로 새 측정 구간을 시작한다.
12. Host와 Client 화면의 역할, RTT, Jitter, Corr, Roll, ReSim, Step, Offset, CameraStep, Follow를 비교한다.
13. Host와 Client가 경기 1회를 완료하고 Console Error가 0건인지 확인한다.

---

## 검증 결과

GitHub 최신 커밋의 변경 파일 6개가 101일차 작업 패키지와 일치하는 것을 확인했다.

- 수정 파일 2개
- 생성 파일 4개
- 삭제 파일 0개
- 배포 ZIP과 최신 커밋 파일 6개 바이트 일치
- Git diff 공백 오류 없음
- Runtime과 EditMode 어셈블리 참조 구조 확인
- `.meta` GUID 중복 없음
- F10 입력 사용 위치 1곳 확인
- EditMode 테스트 8개 구성 확인
- Player Prefab과 `NetworkTransform` 설정 미변경 확인

현재 검증 환경에는 Unity Editor가 없어 실제 컴파일, EditMode Test Runner, Windows Development Build, Host·Client 2인 접속은 실행하지 못했다. 해당 실행 결과는 Unity에서 최종 확인이 필요하다.

---

## 기준 커밋

```text
e5cf0a29b420366247d073f2d09cd7b780e7b4b4
a
```

---

## 101일차 결과

Host와 Client 화면에서 Player 역할과 현재 동작을 구분하고, F10으로 동작별 측정 구간을 초기화할 수 있는 이동 품질 진단 환경을 구성했다.

다음 일차에서는 101일차 측정 결과를 기준으로 원격 Proxy에서만 끊김이 확인될 때 `NetworkTransform` 보정값을 한 항목씩 비교한다.
