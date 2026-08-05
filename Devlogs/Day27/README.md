---

# 프로젝트 J 27일차 개발 일지

---

## 개발 주제

체크포인트 기준 동적 추락 판정과 ESC 직접 부활 기능을 구현하고, 부활 시 플레이어의 위치·회전·이동 상태를 초기화했다.

---

## 개발 목표

- 마지막 활성 체크포인트를 기준으로 추락 한계 계산
- 추락 한계 도달 시 자동 부활
- ESC 경기 메뉴에서 마지막 체크포인트 직접 부활
- 부활 시 위치와 회전 복원
- 이동·낙하·외부 힘 상태 초기화
- 부활 중 화면 암전과 입력 차단
- 경기 종료 후 부활 요청 차단
- 추락 판정 계산 규칙 자동 테스트 추가

---

## 구현 내용

---

### 1. 체크포인트 기준 동적 추락 판정

기존의 고정된 월드 추락 한계를 보완하기 위해 `RespawnFallLimitRules`를 추가했다.

현재 추락 한계는 다음 계산식을 사용한다.

```text
현재 추락 한계 Y
= Max(월드 최저 추락선, 마지막 체크포인트 Y - 체크포인트 아래 허용 거리)
```

적용 수치는 다음과 같다.

| 항목 | 값 |
|---|---:|
| Minimum World Fall Limit Y | `-5` |
| Fall Distance Below Checkpoint | `25` |

시작 지점에서는 기존 월드 추락선 `Y=-5`를 사용한다. 체크포인트가 활성화되면 해당 지점보다 `25m` 아래의 높이와 월드 최저 추락선 중 더 높은 값을 새 추락 한계로 사용한다.

---

### 2. 자동 부활 처리

`PlayerRespawnController`가 플레이어의 현재 높이를 동적 추락 한계와 비교하도록 수정했다.

- 추락 한계보다 높은 위치에서는 현재 플레이 유지
- 추락 한계와 같거나 낮은 위치에서는 자동 부활 요청
- 부활 진행 중 중복 요청 차단
- 마지막 활성 체크포인트가 없으면 경기 시작 지점으로 복귀
- 마지막 활성 체크포인트가 있으면 해당 `RespawnPoint`로 복귀

---

### 3. ESC 직접 부활 메뉴

`PrototypeRespawnMenu`를 추가하여 경기 중 ESC 키로 직접 부활 메뉴를 열 수 있도록 구성했다.

메뉴의 기능은 다음과 같다.

- ESC 키로 메뉴 열기와 닫기
- 메뉴가 열린 동안 플레이어 이동과 카메라 입력 차단
- 마우스 커서 잠금 해제
- 마지막 체크포인트에서 즉시 부활
- 경기에 돌아가기
- 메뉴가 열려 있어도 경기 시간 계속 진행
- 경기 종료 상태에서는 직접 부활 메뉴 사용 차단

메뉴 설정값은 다음과 같다.

| 항목 | 값 |
|---|---:|
| Menu Size | `(420, 230)` |
| Respawn Fade Alpha | `0.65` |
| 직접 부활 확인 창 | 없음 |
| 직접 부활 재사용 대기시간 | 없음 |

---

### 4. 부활 상태 초기화

자동 부활과 직접 부활 모두 같은 부활 처리 흐름을 사용하도록 구성했다.

부활 시 다음 상태를 초기화한다.

- 플레이어 월드 위치
- 플레이어 진행 방향과 회전
- 입력 기반 수평 이동 속도
- 점프 상승 속도
- 낙하 속도
- 밀치기와 장애물 외부 힘
- 이동 발판 전달 속도
- 달리기 상태
- 앉기 상태
- 끝자락 올라오기 상태
- 경사와 모서리 탐지 상태

부활 위치에는 `Respawn Vertical Offset=0.05`를 적용하여 바닥과 겹치거나 즉시 다시 추락하는 상황을 방지했다.

---

### 5. 부활 암전 처리

부활 요청 후 `0.75초` 동안 화면을 반투명 검은색으로 가리도록 구성했다.

| 항목 | 값 |
|---|---:|
| Respawn Delay | `0.75` |
| Respawn Fade Alpha | `0.65` |
| Respawn Vertical Offset | `0.05` |

암전 중에는 플레이어 조작과 `CharacterController`를 잠시 차단하고, 체크포인트 이동과 상태 초기화가 끝나면 다시 활성화한다.

---

### 6. 경기 종료 상태 연동

`PlayerRespawnController`에 경기 종료 상태를 외부 UI에서 확인할 수 있는 `IsMatchFinished` 속성을 추가했다.

이를 통해 `PrototypeRespawnMenu`가 경기 종료 여부를 확인하고, 종료된 경기에서 메뉴나 직접 부활 기능이 다시 실행되지 않도록 처리했다.

---

### 7. 컴파일 오류 수정

`PrototypeRespawnMenu`에서 참조하는 `PlayerRespawnController.IsMatchFinished` 속성이 누락되어 발생한 `CS1061` 오류를 수정했다.

```text
PrototypeRespawnMenu.cs(62,35): error CS1061
PrototypeRespawnMenu.cs(123,83): error CS1061
```

`PlayerRespawnController`에 경기 종료 상태 반환 속성을 추가하여 두 오류를 함께 해결했다.

---

### 8. 자동 테스트 추가

`RespawnFallLimitRulesTests`에 추락 한계 계산과 경계값을 검사하는 EditMode 테스트를 추가했다.

- `FallDistanceNeverFallsBelowMinimum`
- `StartPointUsesMinimumWorldFallLimit`
- `HighCheckpointUsesRelativeFallLimit`
- `MinimumWorldLimitWinsNearCourseStart`
- `PositionAboveFallLimitDoesNotTrigger`
- `PositionOnFallLimitTriggers`
- `PositionBelowFallLimitTriggers`

---

## 체크포인트 및 정상 지점 좌표

체크포인트와 정상 지점의 현재 좌표는 코스의 높이 구간 기준으로 그대로 유지했다.

| 오브젝트 | World Position | 계산된 추락 한계 Y |
|---|---|---:|
| `Checkpoint_01` | `(0, 200, 34)` | `175` |
| `Checkpoint_02` | `(0, 400, 34)` | `375` |
| `Checkpoint_03` | `(0, 600, 34)` | `575` |
| `Checkpoint_04` | `(0, 800, 34)` | `775` |
| `CourseTop` | `(0, 1000, 34)` | 해당 없음 |

이번 일차에서는 위 좌표를 변경하지 않았다. 이후 코스 오브젝트를 각 높이 구간에 맞춰 연결한다.

---

## 수정 및 추가 파일

| 구분 | 파일 |
|---|---|
| 수정 | `Assets/_ProjectJ/Scripts/Runtime/Player/Respawn/PlayerRespawnController.cs` |
| 추가 | `Assets/_ProjectJ/Scripts/Runtime/Player/Respawn/RespawnFallLimitRules.cs` |
| 추가 | `Assets/_ProjectJ/Scripts/Runtime/UI/Menu/PrototypeRespawnMenu.cs` |
| 추가 | `Assets/_ProjectJ/Tests/EditMode/RespawnFallLimitRulesTests.cs` |
| 수정 | `Assets/_ProjectJ/Scenes/Game/Game.unity` |

---

## 테스트 내용

- 시작 지점에서 직접 부활 시 경기 시작 위치로 복귀
- 체크포인트 활성화 후 직접 부활 시 마지막 `RespawnPoint`로 복귀
- 추락 한계 도달 시 자동 부활
- 낮은 번호 체크포인트 재통과 후에도 최고 체크포인트 유지
- 부활 후 위치와 회전 정상 적용
- 달리기·점프·추락·밀치기 중 부활 상태 초기화
- 부활 중 입력 차단과 화면 암전
- ESC 메뉴 열기·닫기와 경기 복귀
- 메뉴가 열린 동안 경기 시간 유지
- 경기 종료 후 직접 부활 차단
- 신규 EditMode 테스트 7개 확인
- 기존 체크포인트 진행 테스트와 이동 기능 회귀 확인

---

## 완료 상태

- 체크포인트 기준 동적 추락 한계 계산 완료
- 추락 한계 통과 시 자동 부활 완료
- ESC 경기 메뉴와 직접 부활 완료
- 마지막 체크포인트 위치·회전 복원 완료
- 이동·낙하·외부 힘 초기화 완료
- 부활 암전과 입력 차단 완료
- 경기 종료 상태 연동 완료
- `IsMatchFinished` 누락에 따른 `CS1061` 오류 수정 완료
- 체크포인트 및 정상 지점 좌표 유지
- 27일차 개발 완료

---

## 커밋 제목

```text
27일차 : 체크포인트 기준 추락 판정 및 ESC 직접 부활
```
