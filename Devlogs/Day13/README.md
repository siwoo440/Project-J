# Project J

3D 3인칭 온라인 수직 점프 경쟁 파티 게임 **Project J**의 개발 기록입니다.

---

## 13일차 : 높이·체크포인트·추락·부활·최소 HUD 구현

### 개발 목표

수직 이동 테스트 구간에 진행 상태와 실패 복귀 구조를 추가했다. 플레이어의 현재 높이와 최고 높이를 표시하고, 체크포인트를 기준으로 추락 후 부활하는 기본 진행 흐름을 구성했다.

---

### 구현 내용

| 기능 | 구현 내용 |
| --- | --- |
| 높이 측정 | `HeightOrigin`의 Y 위치를 기준으로 플레이어의 현재 높이를 계산 |
| 최고 높이 | 플레이 중 도달한 최대 높이를 기록하고 유지 |
| 시작 체크포인트 | 플레이어의 최초 위치를 `START` 부활 지점으로 자동 등록 |
| 체크포인트 | `CP-01`, `CP-02` 트리거 진입 시 마지막 부활 위치와 방향 갱신 |
| 추락 판정 | 플레이어 Y 위치가 `-5` 이하가 되면 추락 처리 시작 |
| 부활 | `0.75초` 후 마지막 체크포인트로 위치와 회전 복구 |
| 상태 초기화 | 부활 시 이동 속도·점프 보조·달리기·앉기·스태미나 상태 초기화 |
| 최소 HUD | 현재 높이·최고 높이·체크포인트·스태미나와 부활 안내 표시 |

---

### 씬 구성

`Game` 씬에 높이 기준점, 체크포인트 두 개, HUD를 연결했다.

```text
Game
├─ HeightOrigin
├─ Player
│  ├─ PlayerMovementController
│  └─ PlayerRespawnController
├─ Checkpoints
│  ├─ Checkpoint_01 (CP-01)
│  └─ Checkpoint_02 (CP-02)
└─ GameplayHUD
   └─ MinimalPlayerHud
```

- `Checkpoint_01`은 달리기 착지대에 배치
- `Checkpoint_02`는 최종 점프 발판에 배치
- 체크포인트의 `BoxCollider`는 Trigger로 사용
- HUD는 별도 Canvas나 TextMeshPro 없이 `OnGUI()`로 표시

---

### 부활 처리 흐름

1. 플레이어가 Y `-5` 이하로 추락한다.
2. 입력·이동·`CharacterController`를 일시 비활성화한다.
3. 화면 중앙에 `추락 / 체크포인트로 복귀 중` 안내를 표시한다.
4. `0.75초` 후 마지막 체크포인트의 위치와 회전을 적용한다.
5. 충돌체를 다시 활성화하고 이동·자세·스태미나 상태를 초기화한다.
6. 입력과 이동을 다시 활성화한다.

---

### 주요 스크립트

```text
Assets/_ProjectJ/Scripts/Runtime/Player/Movement/PlayerMovementController.cs
Assets/_ProjectJ/Scripts/Runtime/Player/Respawn/PlayerRespawnController.cs
Assets/_ProjectJ/Scripts/Runtime/Player/Respawn/CheckpointTrigger.cs
Assets/_ProjectJ/Scripts/Runtime/UI/HUD/MinimalPlayerHud.cs
```

`PlayerMovementController`에는 부활 직후 이동 상태를 정리하는 `ResetAfterRespawn()`을 추가했다. `PlayerRespawnController`는 이 메서드를 호출해 남은 속도, 앉기 상태, 달리기 상태와 스태미나를 초기화한다.

---

### 확인 결과

- 최신 커밋 `610f4e7`의 제목과 13일차 핵심 파일 반영 확인
- `Game.unity`에 `HeightOrigin`, 체크포인트, 부활 컨트롤러, 최소 HUD 구성 확인
- `CP-01`, `CP-02` 체크포인트 ID 반영 확인
- 추락 기준 Y `-5`, 부활 지연 `0.75초` 설정 확인
- `PlayerRespawnController`의 `ResetAfterRespawn()` 호출과 이동 스크립트의 public 메서드 정의 확인
- 체크포인트 Trigger 강제 설정 및 HUD 참조 누락 방지 코드 확인

GitHub 커밋 내용에서는 구조적 누락이나 호출 불일치를 발견하지 못했다. Unity Play Mode의 실제 동작과 Console 오류 여부는 프로젝트 에디터에서 최종 확인이 필요하다.

---

### 완료 결과

수직 코스에서 실패해도 마지막으로 통과한 지점부터 다시 도전할 수 있는 기본 진행 구조를 완성했다. HUD로 높이, 최고 기록, 체크포인트, 스태미나를 확인할 수 있어 다음 구간 설계와 난이도 조정의 기준도 마련했다.
