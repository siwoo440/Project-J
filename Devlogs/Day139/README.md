---
# Project J - 139일차 개발일지

---
## 개발 주제

경로 노드에 의존하지 않는 자율 판단형 AI 봇 이동, 1인 경기 시작 안정화, 플레이어 Spawn·충돌 규칙 통합 및 프로젝트 정리

---
## 개발 목표

138일차까지의 Route 기반 Bot 이동을 주변 지형을 직접 감지하고 판단하는 구조로 교체한다.

Bot이 고정 경로를 그대로 따라가는 대신 현재 위치에서 이동 가능한 방향, 바닥 높이, 단차, 틈, 착지 공간과 점프 도달 가능성을 검사해 Checkpoint와 FINISH를 향해 전진하도록 구성한다.

동시에 1인 Host 경기에서 부족 인원을 Bot으로 안정적으로 충원하고, Human과 Bot이 서로 겹쳐 이동할 수 있도록 Player 충돌 규칙을 통합한다. Human도 Bot과 동일한 `Spawn_00~07` 시작 지점을 사용하도록 Spawn 흐름을 정리한다.

---
## 주요 구현 내용

- `ProjectJBotTraversalSensor` 추가
- 목표 방향 기준 12개 방향의 주변 지형 탐색
- 가까운 바닥과 예상 착지 바닥을 SphereCast로 검사
- 몸통 Capsule 기준 이동 경로와 착지 공간 검사
- 평지·낮은 단차는 걷기, 높은 단차와 작은 틈은 점프로 분류
- 현재 이동 속도, 점프 속도와 중력으로 착지 가능 거리 계산
- 최대 `0.6m` 안전 하강 제한 유지
- Checkpoint 또는 FINISH 아래로 무조건 내려가는 후보 차단
- 최근 정체 방향에 감점을 적용해 같은 벽을 반복해서 미는 현상 감소
- 일정 시간 진행하지 못하면 방향 재탐색
- 장시간 정체 시 현재 Checkpoint Respawn으로 복구
- Route Node 목록 없이 Checkpoint ID와 FINISH를 장거리 목표로 사용
- 기존 Push와 Item 행동은 별도 경쟁 행동 계층으로 유지
- Bot별 출발 시간을 분산해 시작 지점 혼잡 감소
- Human 수에 따라 부족 Bot을 충원하고 초과 Bot을 회수
- 참가자가 점유하지 않은 Spawn 지점을 Bot이 선택하도록 개선
- 1명의 Ready Player만으로 경기 시작 가능
- Human Player도 `Spawn_00~07` 위치와 회전을 사용하도록 변경
- Player와 Bot Prefab 계층을 `Player` Layer로 통일
- Player Layer끼리 물리 충돌을 무시하도록 적용
- 이동·Ground·계단·천장·자세 변경 Physics Query에서 다른 Player 제외
- Bot 센서에서도 자기 Collider와 다른 Player Collider 제외
- 높은 Spawn 지점에서 바닥으로 착지한 뒤 모든 이동 후보가 거부되던 안전 높이 계산 수정
- MonoBehaviour 정적 초기화 중 `LayerMask.NameToLayer`를 호출하던 Unity 예외 수정
- Network Item Prefab의 Built-in·Legacy Material을 URP 대응 Material로 교체하는 정책과 Setup 추가
- 참조가 없는 136일차 이전 기능 추가용 Editor Installer 스크립트와 `.meta` 정리
- 사용하지 않는 ThirdParty Demo Scene, Lightmap, 중복 Prefab·Mesh와 Editor 홍보 스크립트 정리
- Colorful UI Sprite를 `icons` 중심 구조로 정리

---
## 자율 이동 판단 구조

```text
다음 Checkpoint 또는 FINISH 확인
→ 목표 방향 기준 12방향 생성
→ 가까운 바닥과 착지 바닥 검사
→ 이동 Capsule과 착지 공간 검사
→ 걷기 또는 점프 가능성 계산
→ 낙하·막힘·머리 공간 부족 후보 제거
→ 목표 정렬도와 최근 실패 방향으로 점수 계산
→ 최고 점수 방향을 일반 Player 입력으로 변환
```

Bot은 미리 배치된 Route Node를 순서대로 따라가지 않는다.

Checkpoint와 FINISH는 장거리 진행 목표로만 사용하고, 실제 한 걸음의 방향과 점프 여부는 매 판단 시점의 주변 Physics 결과로 결정한다.

---
## 안전 이동과 정체 복구

```text
정상 이동
→ 일정 거리 진행 확인
→ 현재 위치를 최근 안전 위치로 저장

짧은 정체
→ 현재 방향 실패 기록
→ 해당 방향 감점
→ 즉시 주변 방향 재탐색

장시간 정체
→ State Authority 확인
→ 현재 Checkpoint Respawn 요청
→ 이동 상태 초기화
```

Spawn Transform의 높이와 실제 착지한 발 높이가 다른 경우에는 현재 발 높이를 안전 기준에 포함한다. 이 보정으로 `Y=2`에서 Spawn된 Bot이 바닥에 착지한 뒤 평지를 위험한 하강으로 오판하던 문제를 해결했다.

---
## Player 충돌 규칙

Human과 Bot은 모두 `Player` Layer를 사용한다.

```text
Player ↔ Player
→ Physics Layer 충돌 무시
→ 이동 Query에서도 Player Collider 제외
→ 서로 밀려나거나 이동이 막히지 않음

Player ↔ World / Obstacle
→ 기존 충돌 유지
→ Ground, 계단, 벽, 천장 판정 유지
```

Push와 Item에 의한 외력은 기존 네트워크 Gameplay 로직으로 처리하며, 일반 이동 Collider 충돌과 분리한다.

---
## Human과 Bot 시작 위치

Human Player Spawner도 Game Scene의 번호 Spawn 지점을 우선 사용한다.

```text
첫 번째 Human → Spawn_00
두 번째 Human → Spawn_01
...
여덟 번째 Human → Spawn_07
```

Bot Roster는 이미 참가한 Human과 Bot의 위치를 확인하고 점유되지 않은 나머지 Spawn 지점을 선택한다.

장면에 해당 번호 Spawn 지점이 없을 때만 기존 계산 좌표를 예비 위치로 사용한다.

---
## Network Item Material 보정

Network Item Prefab이 Built-in `Standard`, Legacy Shader 또는 Unity 기본 Material을 사용하면 URP에서 분홍색으로 표시될 수 있다.

`ProjectJNetworkItemMaterialPolicy`로 교체 대상을 판정하고, Day139 Material Fix Setup에서 Project Material을 생성·연결하도록 구성했다.

다음 유형은 교체 대상으로 처리한다.

- `Standard`
- `Legacy Shaders/*`
- 누락된 Shader 이름
- `Resources/unity_builtin_extra`

`Universal Render Pipeline/Lit`과 프로젝트 내부 Material은 유지한다.

---
## 프로젝트 정리

현재 Runtime과 Scene에서 참조되지 않는 136일차 이전 일회성 Editor Installer 스크립트와 연결 `.meta`를 제거했다.

ThirdParty 영역에서는 실제 게임에서 사용하지 않는 Demo Scene, Lightmap, 중복 환경 Prefab·Mesh, 홍보용 Editor Window를 정리했다. Colorful UI의 실제 사용 Sprite는 `icons` 폴더 중심으로 재배치했다.

Runtime에서 사용하는 ThirdParty Material과 Prefab은 유지하고 필요한 Material 참조만 보정했다.

---
## 수정 및 추가 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkBotController.cs
├─ ProjectJNetworkBotRosterManager.cs
├─ ProjectJNetworkExternalGameplay.cs
├─ ProjectJNetworkPlayer.cs
├─ ProjectJNetworkPlayerSpawner.cs
└─ Resources/
   ├─ ProjectJNetworkBot.prefab
   ├─ ProjectJNetworkPlayer.prefab
   └─ Network Item Prefabs

Assets/ProjectJ/Network/Fusion/Session/
└─ ProjectJNetworkLobbyFlow.cs

Assets/ProjectJ/Runtime/AI/
├─ ProjectJBotNavigationPolicy.cs
├─ ProjectJBotSpawnPolicy.cs
└─ ProjectJBotTraversalSensor.cs

Assets/ProjectJ/Runtime/Items/
└─ ProjectJNetworkItemMaterialPolicy.cs

Assets/ProjectJ/Runtime/Player/
└─ PlayerCollisionRules.cs

Assets/ProjectJ/Tests/EditMode/
├─ PlayerCollisionRulesTests.cs
├─ ProjectJBotAutonomousNavigationPolicyTests.cs
├─ ProjectJBotNavigationPolicyTests.cs
├─ ProjectJBotTraversalSensorTests.cs
├─ ProjectJDay139BotSpawnSoloStartTests.cs
└─ ProjectJNetworkItemMaterialPolicyTests.cs

Assets/ProjectJ/Scenes/
└─ Game.unity
```

신규 Unity Asset의 `.meta` 파일도 함께 추가되었다.

---
## 검증 내용

- 높은 Spawn 기준 아래의 현재 평지 이동 허용 테스트
- 낮은 단차 걷기 분류 테스트
- 도달 가능한 높은 단차 점프 분류 테스트
- 안전 높이 아래 착지 후보 차단 테스트
- 과도한 낙하 후보 차단 테스트
- 막힌 머리 공간 후보 차단 테스트
- 목표 방향 우선 선택 테스트
- 최근 실패 방향 회피 테스트
- 이동 속도·점프 속도·중력 기반 착지 거리 테스트
- 평지 Physics Sensor 선택 테스트
- 자기 Collider를 포함한 점프 궤적 제외 테스트
- Player Layer 이동 충돌 제외 설정 테스트
- Player 간 Layer 충돌 무시 테스트
- Bot Spawn Slot 점유 검사 테스트
- Bot 출발 시간 분산 테스트
- 1인 Lobby 시작 조건 테스트
- 번호 Spawn Pose 해석 테스트
- Player와 Bot Prefab Layer 테스트
- Network Item Shader·Material 교체 대상 판정 테스트
- `ProjectJ.Runtime` 빌드 오류 0 확인
- `Assembly-CSharp` 빌드 오류 0 확인
- `ProjectJ.Tests.EditMode` 빌드 오류 0 확인
- 안전 높이 재현 테스트 RED → GREEN 확인
- Unity 실제 실행에서 Bot Spawn과 MonoBehaviour 초기화 예외 수정 확인

Unity Editor가 프로젝트를 열고 있어 별도 Batch Mode Test Runner는 실행하지 못했다. 대신 생성된 C# 프로젝트 빌드, 정책 직접 실행과 센서 테스트 별도 컴파일로 변경 내용을 검증했다.

---
## 코드 검토에서 확인한 잔여 위험

- 현재 점프 센서는 1.4m 앞의 착지 후보를 검사하지만, 기본 점프 체공 중 실제 전진 거리는 더 길 수 있어 검사 지점을 지나쳐 낙하할 가능성이 있다.
- 점프 궤적 검사는 중심 Sphere 위주이므로 Capsule 상단의 머리 공간까지 완전히 보장하지 못한다.
- 정체 시 기록한 실패 방향 감점이 Checkpoint 또는 Respawn 전까지 유지되어, 일시적으로 막힌 필수 진행 방향을 오래 회피할 가능성이 있다.
- 실제 탄도 착지 지점, Capsule 상단 장애물, 절벽 가장자리, Player Layer 제외를 재현하는 Physics 회귀 테스트가 추가로 필요하다.

위 항목은 139일차 구현을 제거하지 않고 다음 일차의 안전성 개선 과제로 남긴다.

---
## 결과

139일차에는 Route Node를 따라가는 Bot 구조를 주변 지형을 직접 판단하는 자율 이동 구조로 전환했다.

Bot은 Checkpoint와 FINISH 방향을 유지하면서 평지, 단차, 작은 틈과 위험한 낙하를 구분하고, 정체 시 방향 재탐색 또는 Checkpoint Respawn으로 복구한다.

1인 Host 시작, Human·Bot Spawn Slot 분리, Player 통과 이동과 Network Item Material 문제도 함께 정리해 다음 일차 Gameplay 통합 테스트를 진행할 수 있는 기준선을 마련했다.

---
## 후속 확인 항목

- 전체 코스에서 Bot 점프 성공률과 낙하 빈도 측정
- 실제 체공 시간과 이동 속도로 점프 착지 거리 계산 및 전체 Capsule 궤적 검사
- 실패 방향 감점의 유효 시간 또는 진행 기반 감쇠 적용
- 경사면·이동 발판·좁은 계단에서 센서 판단 확인
- 1~8인 Host/Client 참가 변화에 따른 Spawn Slot 재사용 확인
- Human과 Bot 중첩 이동 중 Push·Item 판정 확인
- Checkpoint 이후 높은 구간에서 정체 복구 위치 확인
- Network Item Prefab 전체의 URP Material 표시 확인
