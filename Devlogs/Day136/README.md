# Project J - 136일차 개발일지

## 개발 주제

AI 봇 기본 경로 이동, 체크포인트 및 부활 연동 기반 구현

## 개발 목표

기존 플레이어가 사용하는 Fusion 이동, 점프, 체크포인트, 부활 구조를 재사용하여 Host가 제어하는 AI 봇의 기본 이동 기반을 구축한다.

AI 전용 이동 물리를 새로 만드는 대신 봇이 Route Node를 따라 가상 입력을 생성하고, 해당 입력을 기존 `ProjectJNetworkPlayer`의 이동 처리에 전달하도록 구성한다.

또한 AI 봇이 일반 플레이어와 함께 경기 시스템에 참여하면서도 Match Coordinator로 선택되지 않도록 분리하고, 추락 후 부활했을 때 현재 위치와 가까운 Route부터 다시 진행할 수 있도록 한다.

## 주요 구현 내용

- `ProjectJBotNavigationPolicy` 추가
- `ProjectJBotRouteNode` 기반 경로 지점 구조 추가
- Route Order를 기준으로 AI 이동 순서 정렬
- 현재 위치에서 목표 Route Node까지의 수평 이동 방향 계산
- Route Node별 도달 반경 판정
- 점프가 필요한 Node에서 1회성 점프 입력 생성
- `ProjectJNetworkBotController`에서 AI 가상 입력 생성
- 기존 `ProjectJNetworkPlayer`가 AI 입력과 실제 Player Fusion 입력을 공통 이동 로직으로 처리
- AI 봇에 Input Authority를 부여하지 않고 Host의 State Authority에서 이동 판단
- `ProjectJNetworkBotMarker`를 이용해 AI 봇 식별
- AI 봇을 Match Coordinator 후보에서 제외
- 기존 Checkpoint / Respawn 시스템의 `RespawnCount` 변화를 감지하여 부활 후 Route 재선정
- 개발 환경에서 Host가 Game Scene에 진입하면 테스트용 AI 봇 1명을 생성하는 Spawner 추가
- 기존 Network Player Prefab을 기반으로 `ProjectJNetworkBot.prefab` 구성
- Fusion Prefab 등록용 `FusionPrefab` Label 적용
- Game Scene에 START, CP1~CP4, FINISH 기준 기본 Route Anchor 6개 배치
- Bot Navigation 정책 EditMode 테스트 추가

## AI 입력 처리 구조

136일차부터 Network Player 입력은 다음 순서로 처리된다.

```text
AI Bot
→ ProjectJNetworkBotController
→ Route Node 방향 계산
→ 가상 Move / AimDirection / Jump 입력 생성
→ ProjectJNetworkPlayer
→ 기존 이동·점프·중력 처리

일반 Player
→ Fusion Network Input
→ ProjectJNetworkPlayer
→ 기존 이동·점프·중력 처리
```

따라서 AI 봇과 실제 플레이어가 서로 다른 이동 시스템을 사용하는 것이 아니라 동일한 `ProjectJNetworkPlayer` 이동 규칙을 공유한다.

## Route Node 구조

Game Scene에는 다음 기본 Route Node가 추가되었다.

```text
BotRoute_000_Start
BotRoute_100_CP1
BotRoute_200_CP2
BotRoute_300_CP3
BotRoute_400_CP4
BotRoute_500_Finish
```

각 Node는 `Route Order` 값으로 이동 순서를 결정한다.

기본 6개 Node는 START, Checkpoint, FINISH를 연결하는 큰 단위의 기준점이며, 실제 장애물과 발판을 통과하기 위한 세부 경로는 중간 Route Node를 추가하여 조정하는 구조다.

점프가 필요한 지점은 `Requires Jump`를 사용하며, `Jump Trigger Distance` 안에 들어오고 Bot이 Grounded 상태일 때 점프 입력을 한 번 생성한다.

## 체크포인트 및 부활 연동

AI 봇은 기존 `ProjectJNetworkExternalGameplay`의 체크포인트 및 부활 상태를 그대로 사용한다.

부활 횟수인 `RespawnCount`가 변경되면 Bot Controller가 이를 감지하고 현재 부활 위치에서 가장 가까운 Route Node를 다시 선택한다.

이를 통해 이미 통과한 구간을 처음부터 다시 이동하는 대신 체크포인트 부활 위치를 기준으로 경로 진행을 재개할 수 있는 기반을 만들었다.

## 네트워크 구조

테스트용 AI 봇은 Host에서만 생성한다.

```text
Host
→ ProjectJDay136BotTestSpawner
→ ProjectJNetworkBot Spawn
→ State Authority에서 AI 판단
→ NetworkTransform으로 위치·회전 동기화
```

AI 봇은 `PlayerRef.None`으로 생성하여 실제 사용자 입력 권한을 갖지 않는다.

`ProjectJNetworkBotMarker`가 붙은 객체는 Match Coordinator 선택 대상에서 제외하여 AI 봇이 경기 전체 상태의 기준 Player가 되는 것을 방지한다.

## Bot Prefab 구성

`ProjectJNetworkBot.prefab`은 기존 Network Player Prefab을 기반으로 하며 다음 핵심 구성을 유지한다.

```text
NetworkObject
NetworkTransform
ProjectJNetworkPlayer
CapsuleCollider
ProjectJNetworkExternalGameplay
ProjectJNetworkItemInventory
ProjectJNetworkBotMarker
ProjectJNetworkBotController
```

따라서 이동, 점프, 중력, Collider, NetworkTransform, Checkpoint, Respawn 등 기존 플레이어 시스템을 최대한 재사용한다.

## 수정 및 추가 파일

```text
Assets/ProjectJ/Editor/ProjectJDay136BotSetup.cs
Assets/ProjectJ/Network/Fusion/Player/ProjectJDay136BotTestSpawner.cs
Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkBotController.cs
Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkBotMarker.cs
Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkExternalGameplay.cs
Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkPlayer.cs
Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkBot.prefab
Assets/ProjectJ/Runtime/AI/ProjectJBotNavigationPolicy.cs
Assets/ProjectJ/Runtime/AI/ProjectJBotRouteNode.cs
Assets/ProjectJ/Scenes/Game.unity
Assets/ProjectJ/Tests/EditMode/ProjectJBotNavigationPolicyTests.cs
```

각 신규 Unity Asset의 `.meta` 파일도 함께 추가되었다.

## 구현 결과

기존 플레이어 이동 시스템을 재사용하는 Host 권한 AI 봇의 기본 구조를 추가했다.

AI 봇은 Route Node를 순서대로 탐색하며 이동 방향과 점프 입력을 생성하고, 기존 Checkpoint와 Respawn 상태를 이용하여 부활 후 가까운 Route부터 이동을 재개할 수 있도록 연결했다.

Game Scene에는 START부터 FINISH까지 큰 단위의 기본 Route Anchor 6개가 배치되어 있어 이후 실제 장애물 구조에 맞춰 중간 Node와 점프 지점을 세분화할 수 있다.

## 확인 상태

최신 확인 커밋은 `3e9ffa37162439dad5b6ee6de4045c728b184670`이다.

저장소 기준으로 Day136 신규 Script, Bot Prefab, Game Scene Route Node, Player 입력 연결, Bot Match Coordinator 제외 처리가 반영되어 있는 것을 확인했다.

다만 현재 GitHub에는 이 커밋을 대상으로 한 자동 CI 상태가 등록되어 있지 않으므로 Unity Editor 컴파일, EditMode Test Runner, 실제 Host/Client 런타임 성공 여부는 GitHub 기록만으로 확정하지 않는다.

## 후속 작업

현재 Game Scene에는 START, CP1~CP4, FINISH의 큰 경로 기준점 6개가 배치되어 있다.

실제 코스의 계단, 좁은 발판, 점프 구간, 우회 구간을 안정적으로 통과하려면 장애물 사이에 중간 Route Node를 추가하고 `Requires Jump`, `Arrival Radius`, `Jump Trigger Distance` 값을 실제 플레이 결과에 맞춰 조정해야 한다.
