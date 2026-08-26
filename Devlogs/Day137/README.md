# Project J - 137일차 개발일지

## 개발 주제

AI 봇 실제 코스 경로 세분화·체크포인트 기반 경로 복구 및 플레이어 수평 충돌·계단 이동 개선

## 개발 목표

136일차에 구축한 AI Bot Route 이동 기반을 실제 Game Scene 코스에 맞게 확장한다.

START, CP1~CP4, FINISH 사이에 세부 Route Node를 추가할 수 있는 구조를 마련하고, 부활 후 이미 통과한 Checkpoint 이전 Route로 되돌아가지 않도록 진행 구간을 제한한다.

또한 실제 코스 테스트 과정에서 확인된 Player의 발판 측면 관통 문제를 수정하여 사람 Player와 AI Bot이 동일한 이동 시스템을 사용하면서 벽, 높은 발판, 낮은 턱, 계단형 오브젝트와 정상적으로 상호작용할 수 있도록 한다.

## 주요 구현 내용

- 기존 0 / 100 / 200 / 300 / 400 / 500 Route Anchor 사이 자동 세분화 기능 추가
- 각 Checkpoint 구간을 25% / 50% / 75% 위치의 보조 Route Node로 분할
- 자동 생성 Route Node를 개별적으로 삭제할 수 있는 Editor 메뉴 추가
- 현재 Checkpoint ID를 Route Order와 연결하여 최소 탐색 Route 제한
- CP1 이후 100 미만, CP2 이후 200 미만과 같이 이미 지난 Route 재선택 차단
- Respawn 후 현재 Checkpoint 이후 Route 중 최근접 지점 재탐색
- AI Bot 진행 거리 감시 및 Stuck 상태 판정 추가
- 일정 시간 수평 진행이 없을 경우 다음 Route 범위에서 경로 복구
- 수직 이동만 발생하는 제자리 점프를 정상 진행으로 오인하지 않도록 처리
- Player 수평 이동에 CapsuleCast 기반 충돌 검사 추가
- 발판 및 벽 측면 Collider 관통 방지
- 충돌 후 남은 이동을 벽 접선 방향으로 변환하는 Wall Slide 추가
- Player 발바닥 바닥 검사를 Raycast 중심점 방식에서 SphereCast 방식으로 보강
- 수직 벽 면을 Ground로 잘못 판정하지 않도록 Ground Normal 기준 추가
- 낮은 턱과 일반 계단을 자동으로 오를 수 있는 Step Up 처리 추가
- 자동 Step Up 최대 높이 0.35m 적용
- Step Up 후보 위치에서 몸통 Overlap 검사 및 상단 이동 경로 검사
- 기존 Jump, Gravity, Crouch, Sprint, Item, Fusion 이동 구조 유지
- AI Bot도 동일한 `ProjectJNetworkPlayer` 이동 충돌 규칙을 사용하도록 유지
- Bot Navigation 및 Player Collision 정책 EditMode 테스트 추가

## AI Route 세분화 구조

136일차의 기본 Route는 다음과 같은 큰 단위로 구성되어 있었다.

```text
000 Start
100 CP1
200 CP2
300 CP3
400 CP4
500 Finish
```

137일차에는 각 구간 사이에 자동 보조 Node를 추가할 수 있도록 Editor 기능을 구성했다.

```text
000 Start
025 Auto
050 Auto
075 Auto
100 CP1

125 Auto
150 Auto
175 Auto
200 CP2

225 Auto
250 Auto
275 Auto
300 CP3

325 Auto
350 Auto
375 Auto
400 CP4

425 Auto
450 Auto
475 Auto
500 Finish
```

자동 Node는 Checkpoint Anchor 사이의 초기 경로 기준점이며, 실제 발판, 장애물, 회전 구간, 점프 위치에 맞춰 Scene에서 세부 위치를 조정하는 구조다.

## Checkpoint 기반 Route 복구

AI Bot은 현재 최고 Checkpoint를 기준으로 최소 Route Order를 계산한다.

```text
Start -> 0
CP1   -> 100
CP2   -> 200
CP3   -> 300
CP4   -> 400
```

예를 들어 CP2까지 도달한 Bot이 추락하여 부활한 경우 Route 0~199는 다시 탐색하지 않는다.

```text
CP2 Respawn
→ Minimum Route Order = 200
→ Route 200 이상만 검색
→ 현재 위치에서 가장 가까운 허용 Route 선택
→ 진행 재개
```

이를 통해 Checkpoint까지 진행한 Bot이 부활 후 이전 구간으로 역주행하는 문제를 방지했다.

## AI Stuck 복구

Bot이 벽이나 장애물 때문에 목표 Route로 이동하지 못할 경우를 감지하기 위해 수평 진행 거리를 기록한다.

일정 시간 동안 충분한 수평 이동이 없으면 Stuck 상태로 판단하고 현재 Node 이후의 Route 중 안전한 지점을 다시 탐색한다.

```text
현재 Route 이동
→ 수평 진행 거리 감시
→ 일정 시간 진행 없음
→ Stuck 판정
→ 현재 Node 이후 Route 재탐색
→ 이동 재개
```

위아래로만 움직이는 제자리 점프는 진행 거리로 인정하지 않아 동일 위치에서 반복 점프하는 상태도 복구할 수 있도록 했다.

## Player 수평 충돌 문제

Game Scene 테스트 중 Player가 높은 발판의 측면으로 이동하면 CapsuleCollider가 있음에도 발판 내부를 관통한 뒤 떨어지는 문제가 확인되었다.

기존 Player 이동은 Rigidbody 기반 충돌 해결이 아니라 직접 다음 위치를 계산하여 `transform.position`에 적용하는 구조였고, 아래 방향 Ground 검사만 별도로 수행하고 있었다.

따라서 위에서 내려오는 착지는 정상적으로 처리되었지만 수평 이동 방향에는 몸통 충돌 검사가 없어 벽과 발판 측면을 통과할 수 있었다.

## CapsuleCast 기반 수평 충돌

수평 이동 전에 현재 Player Capsule 형태를 기준으로 `CapsuleCastNonAlloc`을 실행하도록 변경했다.

```text
수평 이동 요청
→ CapsuleCast
→ 충돌 없음: 전체 이동
→ 충돌 있음: Collider 앞까지만 이동
→ 남은 이동량 계산
→ Wall Slide
```

이를 통해 높은 발판과 벽의 측면을 직접 관통하지 않도록 수정했다.

## Wall Slide

벽에 비스듬하게 이동하는 경우 충돌 지점에서 모든 이동을 정지시키지 않고, 충돌 Normal 방향 성분만 제거한다.

```text
대각선 이동
→ 벽 충돌
→ 벽 내부 방향 제거
→ 벽 접선 방향 이동 유지
```

벽을 밀면서 이동할 때 걸리는 느낌을 줄이고 코너와 긴 벽을 따라 자연스럽게 이동할 수 있도록 했다.

## Ground 검사 보강

기존 Ground 검사는 Player 발 중심에서 아래 방향 Raycast를 사용했다.

137일차부터 Player Capsule 반경을 기준으로 한 SphereCast 방식으로 보강하여 발 중앙이 플랫폼 가장자리를 벗어나더라도 발바닥 일부가 지면 위에 있다면 Ground를 더 안정적으로 감지할 수 있도록 했다.

또한 충돌 면의 Normal Y 값을 검사하여 수직 벽이나 지나치게 가파른 면을 Ground로 처리하지 않도록 했다.

## 계단 및 낮은 턱 Step Up

향후 Game Scene에 계단과 낮은 장애물을 배치했을 때 수평 Collider에 막혀 이동하지 못하는 문제를 방지하기 위해 Step Up 처리를 추가했다.

현재 기본 최대 Step 높이는 0.35m이다.

```text
낮은 턱 접근
→ 수평 Capsule 충돌
→ 턱 상단 높이 검사
→ 높이 <= 0.35m
→ 상승 위치 Capsule 공간 검사
→ 전방 이동 경로 검사
→ 안전하면 자동 Step Up
```

0.35m보다 높은 발판은 자동으로 올라가지 않고 일반적인 벽처럼 막히며 점프를 이용해야 한다.

## 수정 및 추가 파일

```text
Assets/ProjectJ/Editor/ProjectJDay137BotRouteSetup.cs
Assets/ProjectJ/Editor/ProjectJDay137PlayerCollisionSetup.cs

Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkBotController.cs
Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkPlayer.cs

Assets/ProjectJ/Runtime/AI/ProjectJBotNavigationPolicy.cs

Assets/ProjectJ/Runtime/Movement/
└─ ProjectJCharacterCollisionPolicy.cs

Assets/ProjectJ/Scenes/Game.unity

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJBotNavigationPolicyTests.cs
└─ ProjectJCharacterCollisionPolicyTests.cs
```

신규 Unity Asset의 `.meta` 파일과 `Movement` 폴더 `.meta`도 함께 추가되었다.

## 테스트 정책

Bot Navigation 정책에는 다음 조건을 검증하는 EditMode 테스트를 추가했다.

- Checkpoint ID별 최소 Route Order 계산
- 지정 Route Order 이후 첫 Route 검색
- 이전 Checkpoint Route 제외
- Stuck Timeout 도달 여부
- 정상 수평 이동 시 Stuck 복구 차단
- Timeout 이전 조기 복구 차단
- 수직 이동만 있는 경우 진행 거리 제외

Player Collision 정책에는 다음 조건을 검증하는 테스트를 추가했다.

- 충돌 거리보다 멀리 이동하지 않도록 제한
- 음수 충돌 거리 방지
- Wall Slide 시 벽 Normal 방향 제거
- 일반적인 낮은 계단 Step Up 허용
- 높은 발판 자동 Step Up 차단
- 수직 벽 Ground 판정 차단
- 위쪽 바닥 면 Ground 허용

## 구현 결과

136일차의 AI Route 기반 위에 실제 코스용 중간 Route 세분화, Checkpoint 이후 경로 제한, Stuck 복구 구조를 추가했다.

동시에 실제 플레이 테스트 중 드러난 Player의 수평 Collider 관통 문제를 수정하여 Player와 AI Bot 모두 동일한 Capsule 기반 충돌, Wall Slide, Ground 검사, Step Up 규칙을 사용할 수 있는 이동 기반을 마련했다.

이로써 단순 평면 이동뿐 아니라 향후 벽, 높은 발판, 낮은 턱, 계단형 오브젝트를 사용하는 코스에서도 기존 Transform 직접 이동 방식으로 인한 관통 문제를 줄일 수 있는 구조가 추가되었다.

## 확인 상태

최신 확인 커밋은 `ca5761c54b9f652835fc84baca3c6914d3bf453d`이다.

저장소 기준으로 Day137 Route 세분화, Checkpoint 경로 제한, Stuck 복구, Player Capsule 충돌, Wall Slide, Ground 보강, Step Up, 관련 테스트 파일이 함께 반영된 것을 확인했다.

GitHub Commit Status에는 자동 CI 결과가 등록되어 있지 않으므로 Unity Editor 컴파일, EditMode Test Runner 전체 통과, 실제 Host/Client 런타임 결과는 저장소 기록만으로 확정하지 않는다.

## 후속 작업

Game Scene의 자동 Route Node는 두 Checkpoint 사이를 기준으로 생성한 초기 위치이므로 실제 장애물 배치에 맞춰 세부 위치를 조정해야 한다.

점프가 필요한 Route Node에는 `Requires Jump`를 지정하고, 실제 계단과 발판 높이에 따라 Route 위치와 점프 시작 거리를 조정한다.

이후 AI 난이도, 반응 시간, 실수 모델은 다음 일차에서 기존 Route와 이동 충돌 기반 위에 추가한다.
