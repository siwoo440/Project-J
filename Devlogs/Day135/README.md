# Project J - 135일차 개발일지

## 개발 주제

카메라 기준 플레이어 이동 및 이동 방향 회전 구현

## 개발 목표

기존 월드 X/Z축에 고정되어 있던 플레이어 이동을 로컬 카메라의 수평 시선 방향 기준으로 변경한다.

플레이어가 W/S/A/D 입력에 따라 카메라가 바라보는 방향을 기준으로 이동하게 하고, 실제 이동 방향이 존재할 때 캐릭터 몸체도 해당 방향을 바라보도록 회전시킨다.

기존 걷기, 달리기, 앉기, 점프, 이동 속도 아이템 등 이동 관련 규칙은 유지한다.

## 주요 구현 내용

- Fusion 입력에 이미 포함되어 있는 `AimDirection`을 카메라 방향 기준으로 사용
- `AimDirection`의 Y축 성분을 제거하여 수평 이동 전용 방향 계산
- 카메라의 수평 전방 벡터와 오른쪽 벡터를 이용해 WASD 이동 방향 변환
- W/S 입력을 카메라 기준 전진/후진으로 처리
- A/D 입력을 카메라 기준 좌/우 이동으로 처리
- 대각선 입력 시 이동 방향을 정규화하여 이동 속도가 증가하지 않도록 유지
- 카메라가 위나 아래를 바라보더라도 플레이어의 수직 이동에는 영향을 주지 않도록 처리
- 실제 이동 방향이 있을 때 캐릭터 몸체가 이동 방향을 바라보도록 회전
- 정지 상태에서는 카메라만 회전하고 캐릭터 몸체는 마지막 방향 유지
- 몸체 회전은 Fusion Simulation 시간인 `Runner.DeltaTime` 기준으로 처리
- `Quaternion.RotateTowards`를 사용해 순간 회전이 아닌 부드러운 방향 전환 적용

## 추가된 이동 계산 구조

카메라 기준 이동 계산을 플레이어 본체 코드에서 분리하기 위해 `ProjectJCameraRelativeMovementPolicy`를 추가했다.

이 정책 클래스는 다음 역할을 담당한다.

1. 입력 벡터와 카메라 조준 방향을 이용한 수평 이동 방향 계산
2. 카메라의 수평 방향이 유효하지 않을 경우 플레이어 전방 방향 사용
3. 이동 방향 기준 캐릭터 목표 회전 계산
4. 최대 회전량을 적용한 부드러운 몸체 회전 처리

이동 속도 자체는 기존 `ProjectJNetworkPlayer`에서 계산하던 값을 그대로 사용하기 때문에 Sprint, Crouch, 이동 속도 아이템 등의 기존 배율 구조는 변경하지 않았다.

## ProjectJNetworkPlayer 변경

기존에는 입력값을 월드 좌표에 직접 적용했다.

```text
Move.x -> World X
Move.y -> World Z
```

135일차부터는 다음 구조로 변경했다.

```text
Move Input
→ Camera AimDirection
→ Horizontal Forward / Right 계산
→ Camera Relative Move Direction
→ 기존 Horizontal Move Speed 적용
→ Player Position 반영
```

따라서 카메라가 어느 방향을 바라보고 있더라도 W는 화면 기준 전방, S는 후방, A는 왼쪽, D는 오른쪽으로 동작한다.

## 캐릭터 회전 규칙

캐릭터 회전은 카메라 회전과 독립적으로 처리한다.

- 이동 중: 실제 이동 방향을 바라봄
- 정지 중: 현재 몸체 방향 유지
- 카메라 회전: 몸체 방향에 직접 영향 없음
- 대각선 이동: 대각선 실제 이동 방향을 바라봄

초기 몸체 회전 속도는 초당 720도로 설정했다.

## 수정 중 확인된 회귀 오류

개발 과정에서 Day111 풀 공 Pickup 자동 Installer가 Script Reload마다 실행되면서 Fusion SortKey 검증 오류를 반복시키는 문제가 확인되었다.

해당 Installer는 자동 실행을 제거하고 Unity 메뉴에서 직접 실행하는 방식으로 변경했다.

또한 풀 공 Pickup을 생성한 직후 SortKey를 검사하던 흐름을 수정하여 Scene 저장 이후 Fusion 구성을 최종 검증하도록 변경했다.

이 수정은 135일차 이동 기능 자체와는 별개의 회귀 오류 대응이다.

## 수정 및 추가 파일

```text
Assets/ProjectJ/Network/Fusion/Player/ProjectJCameraRelativeMovementPolicy.cs
Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkPlayer.cs
Assets/ProjectJ/Editor/ProjectJDay111PoolBallSceneInstaller.cs
Assets/ProjectJ/Tests/Manual/Day49/Day49_AllSystemsTest.unity
```

## 구현 결과

플레이어의 수평 이동 기준을 월드 좌표축에서 카메라 방향 기준으로 전환했다.

기존 Fusion 입력 구조의 `AimDirection`을 그대로 활용하여 별도의 Networked 변수나 신규 입력 필드를 추가하지 않았으며, 기존 이동 속도 계산과 상태 시스템을 유지한 채 이동 방향 계산 부분만 교체했다.

캐릭터 몸체 역시 실제 이동 방향을 기준으로 회전하도록 연결하여 카메라 방향, 입력 방향, 캐릭터 시각 방향이 자연스럽게 일치하도록 정리했다.

## 후속 작업

AI 봇 경로 이동, 체크포인트 및 부활 연동은 후속 일차로 이월한다.
