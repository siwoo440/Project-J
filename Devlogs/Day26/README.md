# 프로젝트 J 26일차 개발 일지

---

## 개발 주제

4개 체크포인트 활성화와 부활 지점 관리, 정상 지점 도달 판정, HUD 진행 상태 표시를 구현했다.

---

## 개발 목표

- 전체 체크포인트를 4개로 확장
- 가장 높은 번호의 체크포인트만 활성 상태로 유지
- 활성 체크포인트를 플레이어 부활 지점으로 사용
- 체크포인트 번호와 ID를 HUD에 표시
- 정상 지점 도달 상태 기록 및 표시
- 체크포인트 진행 규칙의 EditMode 테스트 추가

---

## 구현 내용

### 1. 체크포인트 진행 규칙

`CheckpointProgressRules`를 추가하여 체크포인트 개수와 현재 번호를 안전한 범위로 제한하도록 구성했다.

- 체크포인트 개수의 최솟값을 1로 제한
- 시작 상태를 체크포인트 0번으로 관리
- 마지막 체크포인트보다 큰 번호를 최대 번호로 보정
- 1번 이상의 체크포인트만 유효한 트리거 번호로 판정
- 현재 번호보다 높은 체크포인트만 새로 활성화
- 동일하거나 낮은 번호 재통과 시 기존 부활 지점 유지
- 현재 체크포인트 기준 진행률 계산

### 2. 플레이어 부활 상태 확장

`PlayerRespawnController`에 체크포인트 진행 상태를 추가했다.

- 전체 체크포인트 개수 4개 관리
- 현재 활성 체크포인트 번호와 ID 저장
- 가장 높은 체크포인트의 위치와 방향 저장
- 추락 시 마지막 활성 체크포인트에서 부활
- 정상 지점 도달 여부 저장
- HUD에서 사용할 체크포인트 진행 정보 제공

### 3. 체크포인트 트리거

`CheckpointTrigger`를 통해 플레이어가 체크포인트 영역에 진입하면 해당 지점을 활성화하도록 구성했다.

- 플레이어 진입 여부 확인
- 체크포인트 번호와 ID 전달
- 지정된 RespawnPoint를 부활 위치로 등록
- 낮은 번호 및 중복 활성화 차단
- 활성화 성공 시 표시 오브젝트 색상 변경

체크포인트 구성은 다음과 같다.

| 체크포인트 | 번호 | ID |
|---|---:|---|
| `Checkpoint_01` | 1 | `CP-01` |
| `Checkpoint_02` | 2 | `CP-02` |
| `Checkpoint_03` | 3 | `CP-03` |
| `Checkpoint_04` | 4 | `CP-04` |

### 4. 정상 지점 판정

`CourseTopTrigger`를 추가하여 플레이어의 정상 도달 여부를 기록하도록 구성했다.

- 플레이어의 최초 정상 진입 감지
- `HasReachedCourseTop` 상태 활성화
- 중복 도달 처리 차단
- 정상 지점 표시 색상 변경
- HUD 정상 도달 상태 갱신

정상 도달 이후에도 이번 단계에서는 경기와 플레이어 조작을 즉시 종료하지 않는다.

### 5. HUD 진행 상태 표시

`MinimalPlayerHud`에 체크포인트와 정상 지점 상태를 추가했다.

```text
체크포인트 0/4 | START
정상 지점 : 미도달
```

체크포인트 활성화 후에는 현재 번호와 ID가 표시된다.

```text
체크포인트 2/4 | CP-02
정상 지점 : 미도달
```

정상 도달 후에는 다음과 같이 표시된다.

```text
체크포인트 4/4 | CP-04
정상 지점 : 도달
```

기존 높이, 구간, 시간, 순위, 스태미나 표시는 유지했다.

### 6. 자동 테스트 추가

`CheckpointProgressRulesTests`에 다음 EditMode 테스트 9개를 추가했다.

- `CheckpointCountNeverFallsBelowOne`
- `StartingIndexRemainsZero`
- `IndexClampsAboveLastCheckpoint`
- `FirstCheckpointIsInsideValidRange`
- `ZeroIsNotAValidCheckpointTriggerIndex`
- `HigherCheckpointCanActivate`
- `SameCheckpointCannotActivateAgain`
- `LowerCheckpointCannotReplaceRespawnPoint`
- `SecondCheckpointReportsHalfProgress`

---

## 수정 및 추가 파일

| 구분 | 파일 |
|---|---|
| 추가 | `Assets/_ProjectJ/Scripts/Runtime/Player/Respawn/CheckpointProgressRules.cs` |
| 수정 | `Assets/_ProjectJ/Scripts/Runtime/Player/Respawn/PlayerRespawnController.cs` |
| 수정 | `Assets/_ProjectJ/Scripts/Runtime/Player/Respawn/CheckpointTrigger.cs` |
| 추가 | `Assets/_ProjectJ/Scripts/Runtime/Player/Respawn/CourseTopTrigger.cs` |
| 수정 | `Assets/_ProjectJ/Scripts/Runtime/UI/HUD/MinimalPlayerHud.cs` |
| 추가 | `Assets/_ProjectJ/Tests/EditMode/CheckpointProgressRulesTests.cs` |
| 수정 | `Assets/_ProjectJ/Scenes/Game/Game.unity` |

---

## 최신 커밋 검토 결과

최신 커밋 `95ad398`을 정적으로 검사한 결과, 스크립트 구조와 Scene 참조에는 명백한 Missing Script가 없었지만 다음 Scene 설정은 추가 수정이 필요하다.

| 우선순위 | 대상 | 확인된 문제 | 수정 방향 |
|---:|---|---|---|
| 1 | 체크포인트 및 정상 위치 | 기존 코스와 분리된 `Y=200~1000`에 배치 | 실제 안전 발판 위치로 이동 |
| 2 | `CheckpointTrigger` 4개 | 부모 Transform이 Respawn Point로 연결 | 각 자식 `RespawnPoint`로 재연결 |
| 3 | 체크포인트 `Visual` 4개 | 불필요한 `BoxCollider` 포함 | `BoxCollider` 제거 |
| 4 | 체크포인트 Trigger 4개 | 감지 범위 절반이 바닥 아래에 위치 | BoxCollider Center를 `(0, 1, 0)`으로 수정 |
| 5 | `Fall Limit Y` | 고층 코스에서는 전역 `-5` 판정이 늦게 작동 | 코스 확장 시 구간별 낙사 판정 검토 |

현재 테스트맵 기준으로 우선 복구할 수 있는 기존 위치는 다음과 같다.

| 오브젝트 | 복구 위치 |
|---|---|
| `Checkpoint_01` | `(0, 0.05, 34)` |
| `Checkpoint_02` | `(-16.5, 3.45, 0)` |

`Checkpoint_03`, `Checkpoint_04`, `CourseTop`은 연결된 실제 발판 위치가 확정되지 않았으므로 정확한 좌표 확인이 필요하다.

---

## 테스트 항목

### 체크포인트 진행 테스트

- 시작 시 `0/4 | START` 표시
- CP-01부터 CP-04까지 높은 번호 활성화
- 동일 체크포인트 재통과 시 상태 유지
- 낮은 체크포인트 재통과 시 최고 체크포인트 유지
- 체크포인트 건너뛰기 시 높은 번호 바로 활성화
- 추락 후 마지막 활성 체크포인트에서 부활
- 부활 위치와 다음 진행 방향 확인

### 정상 지점 테스트

- 최초 통과 시 정상 도달 상태 활성화
- HUD를 `정상 지점 : 도달`로 갱신
- 정상 지점 Visual 색상 변경
- 재통과 시 중복 처리와 오류 없음

### 회귀 테스트

- WASD 이동
- Shift 달리기와 스태미나
- Ctrl 앉기
- 점프, 코요테 타임, 점프 버퍼
- 공중 제어
- 경사, 계단, 모서리 보정
- 끝자락 올라오기
- 외부 힘과 밀치기
- 카메라 벽 충돌과 달리기 FOV
- 밀치기 대상 외곽선
- 현재 및 최고 높이
- 5개 수직 구간
- 실시간 순위
- 경기 종료 후 조작 차단

---

## 현재 상태

- 체크포인트 진행 규칙 및 관련 스크립트 작성 완료
- 정상 지점 도달 상태 및 HUD 표시 작성 완료
- EditMode 테스트 코드 작성 완료
- Scene 오브젝트 수치와 컴포넌트 일부 수정 필요
- Unity 컴파일 결과 확인 필요
- EditMode 전체 테스트 실행 필요
- Play Mode 체크포인트 및 부활 동작 확인 필요
- Windows 개발 빌드 확인 필요

실제 Unity 에디터 실행 결과가 확인되기 전까지 26일차는 **코드 구현 완료, Scene 수정 및 실행 검증 대기** 상태로 기록한다.

---

## 커밋 제목

```text
26일차 : 4개 체크포인트 활성화 및 정상 지점·HUD 진행 표시
```
