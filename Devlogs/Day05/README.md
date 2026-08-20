# 5일차 개발일지 - Physics Layer 및 충돌 규칙 구축

## 오늘의 목표

플레이어 이동과 장애물, 체크포인트, 아이템 등의 기능을 구현하기 전에
Project J에서 사용할 기본 Physics Layer와 충돌 규칙을 확정한다.

이번 일차에서는 실제 플레이어 이동이나 장애물 기능을 구현하지 않고,
이후 여러 시스템에서 공통으로 사용할 물리 기준만 구축한다.

## 구현 내용

### 1. 사용자 Physics Layer 추가

다음 Layer를 새로 추가했다.

- Player
- World
- Obstacle
- GameplayTrigger
- Item

각 Layer의 역할은 다음과 같이 정의했다.

| Layer | 역할 |
| --- | --- |
| Player | 플레이어 캐릭터 |
| World | 바닥, 벽, 고정 플랫폼 등 맵 구조 |
| Obstacle | 움직이는 장애물 및 플레이어에게 영향을 주는 장치 |
| GameplayTrigger | 체크포인트, FINISH, 낙사 판정 등 감지 영역 |
| Item | 아이템 상자 및 아이템 관련 오브젝트 |

### 2. Layer Collision Matrix 설정

기존에는 모든 Layer 사이의 충돌이 허용되어 있었지만,
Project J의 게임 규칙에 맞게 기본 충돌 규칙을 정리했다.

다음 충돌은 비활성화했다.

- Player ↔ Player
- GameplayTrigger ↔ GameplayTrigger

Player끼리는 기본적으로 서로 물리적으로 밀어내거나 막지 않고 통과할 수 있도록 했다.

GameplayTrigger끼리는 물리 충돌이 필요하지 않으므로
서로 충돌하지 않도록 설정했다.

### 3. 주요 충돌 유지

다음 관계는 충돌이 가능하도록 유지했다.

- Player ↔ World
- Player ↔ Obstacle
- Player ↔ GameplayTrigger
- Player ↔ Item

World와 Obstacle은 실제 물리 충돌에 사용하고,
GameplayTrigger와 Item은 이후 Trigger 기반 감지 시스템에서 활용한다.

### 4. GameplayTrigger 사용 기준 확정

GameplayTrigger는 이후 Collider의 `Is Trigger`를 사용하는 감지 영역으로 활용한다.

예상 사용 대상:

- 체크포인트
- FINISH
- 낙사 판정
- 구간 진입 감지
- 기타 게임 진행 Trigger

이번 일차에서는 실제 Trigger 오브젝트나 판정 스크립트는 구현하지 않았다.

### 5. 기존 Physics 설정 유지

중력과 Solver 등의 기존 Physics 기본값은 현재 단계에서 변경하지 않았다.

플레이어 이동 시스템을 구현한 뒤 실제 플레이 감각과 물리 안정성을 확인하면서
필요한 값만 단계적으로 조정한다.

### 6. Unity 자동 직렬화 갱신

Unity가 프로젝트 설정을 저장하는 과정에서
Mobile URP Asset과 Physics 설정 파일의 직렬화 버전이 최신 형식으로 갱신되었다.

현재 확인한 변경에서는 Project J의 렌더링 방향이나 게임 규칙을 의도적으로 변경한 내용은 없으며,
Unity가 현재 버전의 Asset 형식으로 다시 저장한 변경으로 판단했다.

## 테스트

- Tags and Layers에서 Player Layer 확인
- World Layer 확인
- Obstacle Layer 확인
- GameplayTrigger Layer 확인
- Item Layer 확인
- Player ↔ Player 충돌 비활성화 확인
- GameplayTrigger ↔ GameplayTrigger 충돌 비활성화 확인
- Player ↔ World 충돌 활성화 확인
- Player ↔ Obstacle 충돌 활성화 확인
- Player ↔ GameplayTrigger 관계 확인
- Player ↔ Item 관계 확인
- Unity Console Error 0 확인

## 결과

Project J에서 사용할 기본 물리 Layer와 충돌 기준을 구축했다.

이후 플레이어 이동, 장애물, 체크포인트, 낙사 판정,
아이템 획득 등의 시스템을 구현할 때
동일한 Layer 규칙을 기준으로 기능을 연결할 수 있는 상태가 되었다.

또한 플레이어끼리 기본적으로 서로 통과하는 게임 규칙을
Physics Collision Matrix 단계에서부터 반영했다.
