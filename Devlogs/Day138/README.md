# Project J - 138일차 개발일지

## 개발 주제

AI 봇 Push·아이템 판단·부족 인원 Bot 충원 및 전원 충원 Countdown 구현

## 개발 목표

136~137일차에서 구축한 Bot Route 이동·Checkpoint·Respawn 기반을 실제 경기 참가자 수준으로 확장한다.

Bot이 다른 플레이어를 적극적으로 추적해 싸우는 것보다 Route·Checkpoint·FINISH 방향으로 계속 올라가는 것을 우선하도록 행동 기준을 정리하고, 진행을 바로 막는 상대에게만 제한적으로 Push하도록 한다.

또한 Host가 목표 참가 인원에서 부족한 수만큼 Bot을 자동 충원·회수하고 Human과 Bot을 합친 전체 인원이 목표 인원에 도달한 뒤에만 3초 Countdown을 시작하도록 Roster와 Match 시작 조건을 연결한다.

## 주요 구현 내용

- `ProjectJBotCompetitionPolicy` 추가
- Bot Push 판단, Item 사용 판단, 목표 Bot 수 계산 정책 분리
- `ProjectJNetworkBotActionController` 추가
- 기존 Bot Navigator의 Route 이동을 유지하면서 경쟁 행동을 별도 계층으로 처리
- 가까운 상대를 검색하되 Push 때문에 Route 이동 목표가 바뀌지 않도록 구성
- Push 거리를 약 1.35m로 축소하고 거의 정면의 진행 방해 상대만 대상으로 제한
- Bot 자체 Push 판단 간격을 추가해 반복 밀치기 감소
- 공격형 Item 상대 탐색 범위 축소
- Item 판단 주기를 늘려 Route 진행 우선
- 지상에서 안정적으로 이동 중인 상태를 Item 사용 기본 조건으로 적용
- `ProjectJNetworkExternalGameplay.BotAI`를 통해 기존 State Authority Push 처리 재사용
- `ProjectJNetworkItemInventory.BotAI`를 통해 기존 Slot 선택·Item 사용·Hold 입력 처리 재사용
- 물총·저격 물총 등 Hold형 Item의 가상 Hold/Release 입력 처리
- `ProjectJNetworkBotRosterManager` 추가
- Host가 Human 수와 목표 참가 인원을 비교해 부족 Bot 자동 Spawn
- Human 증가 시 초과 Bot 자동 Despawn
- Bot은 `PlayerRef.None`으로 생성해 Host State Authority에서 Simulation
- 기존 Day136 개발용 단일 Bot Spawner를 실제 Roster Fill 구조로 대체
- Game Scene에 `=== DAY138 BOT ROSTER ===` 구성
- Human + Bot이 목표 인원에 도달해야 Countdown 허용
- 전원 충원 후 짧은 안정화 시간을 둔 뒤 3초 Countdown 시작
- Countdown 중 참가자가 빠지면 Preparing으로 복귀
- 부족 Bot 재충원 후 Countdown을 처음부터 다시 시작하는 구조 추가
- `ProjectJNetworkExternalGameplay.BotRoster`를 통해 Roster와 기존 Match Countdown 연결
- 기존 `BeginCountdownAuthority()` 진입 경로에 Roster Gate를 적용할 Editor Setup 추가
- EditMode에서 Push·Roster·Item 판단 정책 테스트 추가

## Bot 행동 우선순위

```text
1. Route / Checkpoint / Finish 진행
2. 점프와 경로 복구
3. 이동에 도움이 되는 Item 사용
4. 바로 앞에서 진행을 막는 상대만 Push
```

Bot은 주변 상대를 찾아 이동 방향을 바꾸지 않는다.

```text
상대가 가까움
→ 대부분 무시
→ Route 이동 계속

상대가 약 1.35m 이내
+ 거의 정면
+ Push Cooldown 종료
+ Bot 판단 Cooldown 종료
→ Push 1회
→ 다시 Route 이동
```

## 부족 인원 Bot 충원

목표 참가 인원이 8명인 경우 다음처럼 동작한다.

```text
Human 1 → Bot 7
Human 2 → Bot 6
Human 4 → Bot 4
Human 7 → Bot 1
Human 8 → Bot 0
```

Host만 Roster를 계산하고 Bot Spawn/Despawn을 수행한다.

Human이 새로 참가하면 초과 Bot을 제거하고, Human이 이탈하면 부족한 자리에 Bot을 다시 생성한다.

## 전원 충원 Countdown

기존에는 경기 시작 요청이 들어오면 바로 Countdown에 진입할 수 있었지만, 138일차에서는 Roster가 모두 채워진 뒤에만 시작하도록 변경한다.

```text
Host 준비
→ Human Spawn
→ 부족 Bot 순차 Spawn
→ Human + Bot = Target Participant Count
→ 약 0.75초 안정화
→ 3초 Countdown
→ Playing
```

Countdown 도중 인원이 부족해지면 다음처럼 처리한다.

```text
Countdown
→ Human 또는 Bot 감소
→ Preparing 복귀
→ Countdown 취소
→ Bot 재충원
→ 전체 인원 재확인
→ 안정화
→ 3초 Countdown 재시작
```

## Item 사용 판단

Bot 전용 Item Effect를 새로 구현하지 않고 기존 Player Item 시스템을 그대로 재사용한다.

```text
Bot 판단
→ Slot 선택
→ 사용 조건 판단
→ 기존 State Authority Item 처리 호출
→ 기존 Target / Hit / Spawn / Effect / Consume 흐름 사용
```

공격·방해형 Item은 주변에 유효한 상대가 있을 때만 사용하는 방향으로 제한하고, 이동·방어·설치형 Item은 상대가 없어도 사용할 수 있도록 공통 정책으로 나눴다.

## Network 구조

```text
Host
↓
ProjectJNetworkBotRosterManager
↓
Human 수 / Bot 수 계산
↓
부족 Bot Spawn 또는 초과 Bot Despawn
↓
ProjectJNetworkBotController
├─ Route 이동 입력
└─ ProjectJNetworkBotActionController
   ├─ Push 판단
   └─ Item 판단
↓
기존 ProjectJNetworkPlayer / ExternalGameplay / ItemInventory
↓
Fusion State Authority 확정
```

## 수정 및 추가 파일

ThirdParty 에셋은 이번 개발일지의 Gameplay 변경 목록에서 제외한다.

```text
Assets/ProjectJ/Editor/
├─ ProjectJDay138BotCompetitionSetup.cs
└─ ProjectJDay138RosterCountdownSetup.cs

Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkBotActionController.cs
├─ ProjectJNetworkBotController.cs
├─ ProjectJNetworkBotRosterManager.cs
├─ ProjectJNetworkExternalGameplay.BotAI.cs
├─ ProjectJNetworkExternalGameplay.BotRoster.cs
├─ ProjectJNetworkItemInventory.BotAI.cs
└─ Resources/ProjectJNetworkBot.prefab

Assets/ProjectJ/Runtime/AI/
└─ ProjectJBotCompetitionPolicy.cs

Assets/ProjectJ/Scenes/
└─ Game.unity

Assets/ProjectJ/Tests/EditMode/
└─ ProjectJBotCompetitionPolicyTests.cs
```

신규 Unity Asset의 `.meta` 파일도 함께 추가되었다.

## 테스트 항목

- Human 수에 맞는 목표 Bot 수 계산
- Human + Bot 합계가 목표 참가 인원과 일치하는지 확인
- Bot Spawn/Despawn 중복 여부
- Human 참가·이탈 후 Roster 재조정
- Roster 미충원 상태 Countdown 차단
- 전원 충원 후 안정화 시간 이후 Countdown 시작
- Countdown 중 인원 부족 시 Preparing 복귀
- Bot Route 이동 지속
- 가까운 정면 상대만 Push
- 멀리 있거나 후방의 상대 Push 차단
- Push 판단 간격 적용
- 공격형 Item 대상 없음 상태 사용 차단
- Utility Item 단독 사용 허용
- 물총·저격 물총 Hold/Release 처리
- Checkpoint·Respawn·Stuck 복구와 경쟁 행동 동시 사용
- Host/Client에서 Bot 위치·상태·Item 결과 일치 여부

## 최신 저장소 확인 상태

확인한 최신 `main` 커밋은 `f99985356a0cb9df6a57612074fa39def705dae5`이며 커밋 메시지는 `a`이다.

해당 커밋에는 Day138 Bot Competition, Bot Roster, Bot AI Push/Item Bridge, Roster Countdown Setup, Game Scene Roster와 관련 테스트가 포함되어 있다.

`Assets/ProjectJ/ThirdParty` 아래 대량 에셋은 이번 138일차 Gameplay 검토 범위에서 제외한다.

현재 최신 `main`의 `ProjectJNetworkExternalGameplay.cs`에는 `ProjectJNetworkBotRosterManager.IsCountdownAllowed(Runner)` 직접 Gate가 아직 반영되지 않은 상태이므로 Unity에서 다음 메뉴를 실행한 뒤 생성되는 Source 변경까지 최종 138일차 커밋에 포함해야 한다.

```text
Project J
→ Day138
→ Apply Roster Countdown And Climb Priority
```

따라서 저장소 구조 기준 핵심 Day138 기능 파일은 준비되어 있지만, 위 Source Gate 반영과 Unity 실제 컴파일·EditMode Test·Host/Client 런타임 확인이 최종 완료 조건이다.

## 후속 방향

139일차에서는 신규 기능을 추가하지 않고 30종 Item, AI Bot, Roster Fill, 전원 충원 Countdown을 1~8인 Host/Client 환경에서 통합 검증해 Gameplay 기준선을 고정한다.

ThirdParty 에셋은 139일차 기능 Gate에서 내용 자체를 검증하지 않고 원본 보관 영역으로 유지한다.

140일차부터 ThirdParty·Tripo.ai·Unity Primitive Placeholder를 조합해 실제 Visual을 단계적으로 적용하며 Gameplay Root의 Collider·NetworkObject·Trigger·AI·Route와 Visual을 분리해 모델 교체가 기능 판정에 영향을 주지 않도록 진행한다.
