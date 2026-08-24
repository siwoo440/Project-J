---

# Project J - 105일차 개발일지

---

## 개발 방향

화면 곳곳에 개별적으로 표시되던 `OnGUI` 디버그 창을 기본 상태에서 모두 숨기고, F1 하나로 여닫는 통합 디버그 패널에서 필요한 진단 정보만 선택해 확인할 수 있도록 구성한다.

기존 네트워크 측정값과 Steam·세션·경기 진단 기능은 삭제하지 않는다. 실제 게임 UI와 ALT 커서 전환을 유지하면서 디버그 정보의 표시 경로만 통합한다.

---

## 변경 내용

---

### 1. 모든 디버그 창 기본 숨김

자동 설치되는 진단창 중 기본 표시 상태였던 다음 화면을 숨김 상태로 변경했다.

- Day79 Network Condition
- Day81 Steam Invite
- Day82 Scene Flow

게임 시작과 Scene 전환 시 통합 패널과 개별 디버그 창이 모두 닫힌 상태로 초기화된다.

Day80 Steam Identity 창이 Day79 Component를 직접 비활성화하던 연결도 제거했다. 패널 표시 여부와 관계없이 Day79의 FPS·RTT·보정값 측정은 계속 갱신된다.

---

### 2. F1 입력 처리 단일화

기존에는 `ProjectJDebugOverlayController`와 `ProjectJDebugWindowMenu`가 F1 입력을 각각 처리했다.

이번 변경에서 F1 입력은 `ProjectJDebugWindowMenu`만 처리하도록 통합했다.

- F1 최초 입력: 통합 패널 열기
- F1 재입력: 통합 패널 닫기
- Scene 전환: 통합 패널 자동 닫기
- ALT: 기존 커서 잠금·해제 유지

기존 F2~F9·F12 진단창의 독립 출력 상태는 매 프레임 해제된다. F10 측정 초기화와 F11 강제 경기 종료처럼 실제 진단 동작을 수행하는 입력은 유지된다.

---

### 3. 탭형 통합 디버그 패널 구성

통합 패널을 화면 중앙에 배치하고 다음 다섯 개 탭으로 진단창을 분류했다.

| 탭 | 주요 내용 |
| --- | --- |
| 개요 | Fusion Bootstrap, Phase Gate, Day76 테스트 흐름 |
| 네트워크 | RTT, Jitter, Prediction, NetworkTransform 진단 |
| 플레이어 | 로컬 플레이어, 4인·8인 Gate, 체크포인트, 부활, 관전 |
| 세션·Steam | Steam Identity, 초대, Lobby, Scene Flow, 연결 복구 |
| 게임 상태 | Match, Timer, Finish, Fall, 아이템과 경기 상태 |

좌측 목록에서 진단창을 선택하면 우측 내용 영역에 선택한 창 하나만 표시된다. 기존 고정 좌표 기반 디버그 창을 확인할 수 있도록 내용 영역에 가로·세로 스크롤을 적용했다.

패널 크기는 현재 해상도 안에서 자동 제한되며, 실제 게임 화면 위에 여러 진단창이 동시에 겹치지 않는다.

---

### 4. 기능 Component 내부 디버그 출력 통합

별도 Debug View가 아니라 실제 기능 Component의 `OnGUI()`에 포함되어 있던 진단 화면도 통합 패널 표시 상태를 따르도록 변경했다.

- `ProjectJNetworkExternalGameplay`
- `ProjectJNetworkItemInventory`
- `ProjectJLocalPlayerPresentationController`
- `ProjectJNetworkLobbyFlow`
- `ProjectJDay76TestFlow`
- `MatchStateDebugView`
- `MatchTimerDebugView`

해당 Component는 비활성화하지 않는다. 이동·카메라·Lobby·아이템·경기 상태 로직은 계속 실행하고, 독립적인 `OnGUI` 출력만 차단한다.

---

### 5. 통합 패널 정책과 EditMode 테스트 추가

`ProjectJUnifiedDebugPanelPolicy`를 추가해 다음 규칙을 Runtime 코드에서 분리했다.

- 기본 패널 표시 상태
- F1 표시 상태 전환
- 진단창 타입별 탭 분류
- 탭 한글 표시 이름
- 기능 Component 내부 진단창 판정

`ProjectJUnifiedDebugPanelPolicyTests`에는 총 18개 테스트 사례를 구성했다.

- 기본 숨김 상태 1개
- F1 표시 전환 2개
- 진단창 탭 분류 5개
- 한글 탭 이름 5개
- 기능 Component 내부 진단창 판정 5개

---

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
└─ ProjectJDebugWindowMenu.cs

Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkExternalGameplay.cs
└─ ProjectJNetworkItemInventory.cs

Assets/ProjectJ/Network/Fusion/Presentation/
└─ ProjectJLocalPlayerPresentationController.cs

Assets/ProjectJ/Network/Fusion/Session/
└─ ProjectJNetworkLobbyFlow.cs

Assets/ProjectJ/Network/Fusion/Test/
├─ ProjectJDay76TestFlow.cs
├─ ProjectJDay79NetworkConditionDebugView.cs
├─ ProjectJDay80SteamIdentityDebugView.cs
├─ ProjectJDay81SteamInviteDebugView.cs
└─ ProjectJDay82SceneFlowDebugView.cs

Assets/ProjectJ/Runtime/Debugging/
├─ ProjectJDebugOverlayController.cs
├─ ProjectJUnifiedDebugPanelPolicy.cs
└─ ProjectJUnifiedDebugPanelPolicy.cs.meta

Assets/ProjectJ/Runtime/Match/
├─ MatchStateDebugView.cs
└─ MatchTimerDebugView.cs

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJUnifiedDebugPanelPolicyTests.cs
└─ ProjectJUnifiedDebugPanelPolicyTests.cs.meta
```

- 수정 파일: 13개
- 생성 파일: 4개
- 삭제 파일: 없음

Scene, Hierarchy, Inspector, Prefab과 Build Settings 변경은 없다.

---

## 확인 절차

1. Unity를 실행하고 컴파일 완료 후 Console Error가 없는지 확인한다.
2. `Window → General → Test Runner → EditMode`를 연다.
3. `ProjectJUnifiedDebugPanelPolicyTests`의 18개 사례를 실행한다.
4. Play 시작 시 디버그 창이 하나도 표시되지 않는지 확인한다.
5. F1을 눌러 통합 디버그 패널을 연다.
6. `개요`, `네트워크`, `플레이어`, `세션·Steam`, `게임 상태` 탭을 차례대로 확인한다.
7. 좌측 진단창을 선택할 때 우측 내용만 변경되는지 확인한다.
8. 긴 진단 화면에서 가로·세로 스크롤이 작동하는지 확인한다.
9. F2~F9와 F12 입력으로 독립 창이 다시 나타나지 않는지 확인한다.
10. F10 측정 초기화와 F11 테스트 동작이 유지되는지 확인한다.
11. ALT 입력으로 마우스 커서를 잠금·해제할 수 있는지 확인한다.
12. Scene 전환 후 통합 패널이 닫힌 상태로 초기화되는지 확인한다.
13. Host·Client가 Private Room과 Room Code로 접속해 전체 경기 1회를 진행한다.
14. 경기 종료까지 Console Error가 0건인지 확인한다.

---

## 검증 결과

GitHub 최신 커밋의 변경 파일 17개가 105일차 배포 패키지와 일치하는 것을 확인했다.

- 최신 커밋 변경 범위: 수정 13개, 생성 4개, 삭제 0개
- 배포 패키지와 최신 커밋 파일 17개 바이트 일치
- Git diff 공백 오류 없음
- 변경 C# 파일의 중괄호와 전처리기 균형 확인
- F1 입력 처리 위치 1곳 확인
- 기능 Component 내부 진단창 7개의 통합 표시 조건 확인
- 신규 `.meta` GUID 중복 없음
- Scene, Prefab과 Project Settings 미변경 확인

현재 검증 환경에는 Unity Editor가 없어 실제 컴파일, EditMode Test Runner, Play Mode, Windows Development Build와 Host·Client 2인 접속은 실행하지 못했다. 통합 패널의 실제 배치와 버튼 입력은 Unity에서 최종 확인이 필요하다.

---

## 구현 확인 기준 커밋

개발일지 반영 전 확인한 커밋은 다음과 같다.

```text
cea500e0884412ad2e3853110991b21b40052046
105
```

---

## 105일차 결과

화면 곳곳에 겹쳐 표시되던 디버그 창을 기본 숨김 상태로 변경하고, F1 단일 입력과 다섯 개 탭을 사용하는 통합 진단 패널로 정리했다.

기존 진단 계산과 테스트 동작은 유지하면서 표시 경로만 통합해, 필요한 정보 하나를 선택하고 스크롤해서 확인할 수 있는 구조를 구성했다.
