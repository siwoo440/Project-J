# 93일차 개발일지

## 작업명
Game HUD·Countdown·순위·Result 연결

## 작업 목표
Game Scene에서 플레이어가 경기 진행에 필요한 핵심 정보를 실제 Canvas UI로 확인할 수 있도록 HUD를 구성한다.

기존 Fusion Networked 상태를 기준으로 남은 시간, 현재 높이, 순위, 스태미나, 아이템 슬롯, 부활 보호 상태를 표시하고 Countdown 및 Result 화면까지 연결한다.

---

## 주요 작업 내용

### 1. Game HUD 구성
`Game.unity`에 93일차 전용 HUD Canvas를 추가했다.

표시 항목은 다음과 같다.

- 남은 경기 시간
- 플레이어 현재 높이
- 현재 순위 / 참가 인원
- Stamina 수치 및 게이지
- 좌·우 2슬롯 아이템
- 현재 선택된 아이템 슬롯
- 부활 보호 상태 및 남은 시간

HUD는 로컬 Input Authority를 가진 Network Player를 찾아 해당 플레이어의 실제 Networked 상태를 표시하도록 구성했다.

---

### 2. Fusion 네트워크 데이터 연결
`ProjectJDay93GameHUD`를 추가하여 기존 네트워크 시스템과 HUD를 연결했다.

주요 참조 대상은 다음과 같다.

- `ProjectJNetworkPlayer`
- `ProjectJNetworkExternalGameplay`
- `ProjectJNetworkItemInventory`
- `ProjectJDay82SceneFlowCoordinator`

HUD는 일정 간격으로 로컬 플레이어 참조를 다시 탐색하여 Scene 전환이나 Network Player 생성 시점 차이에도 대응하도록 구성했다.

---

### 3. Countdown UI 연결
경기 시작 전 Fusion의 Countdown 상태를 읽어 화면 중앙에 Countdown을 표시하도록 구현했다.

표시 순서는 다음과 같다.

```text
3
2
1
GO!
```

Countdown 상태가 아닐 때에는 Countdown Panel을 자동으로 숨긴다.

---

### 4. Result UI 연결
플레이어 개인 결과 및 경기 최종 결과를 표시하는 Result Panel을 구성했다.

표시 정보는 다음과 같다.

- FINISH / MATCH OVER 상태
- 최종 순위
- Finish 시간
- 최고 높이
- 경기 종료 사유
- Lobby 복귀 버튼

Lobby 복귀는 새 Scene 이동 로직을 별도로 만들지 않고 기존 `ProjectJDay82SceneFlowCoordinator`의 Lobby 복귀 흐름을 사용하도록 연결했다.

---

### 5. Game HUD 자동 구성 Installer 추가
Game Scene의 HUD를 자동으로 생성하고 연결하는 Editor Installer를 추가했다.

실행 메뉴:

```text
Project J
→ Scene
→ 93일차 Game HUD 구성
```

Installer는 기존 `Day93GameHUDCanvas`가 존재하면 제거한 뒤 다시 생성하므로 반복 실행 시 HUD가 중복 생성되지 않도록 구성했다.

생성되는 주요 Hierarchy는 다음과 같다.

```text
Day93GameHUDCanvas
├─ MatchHUD
├─ RespawnProtection
├─ CountdownPanel
└─ ResultPanel
```

또한 Button 입력을 위해 Game Scene에 필요한 EventSystem을 확인하고 없을 경우 자동 생성하도록 구성했다.

---

### 6. TMP Font NullReferenceException 수정
초기 93일차 Installer에서 TextMeshPro 기본 Font를 가져오는 과정에서 다음 오류가 발생했다.

```text
NullReferenceException
TMPro.TMP_Settings.get_defaultFontAsset()
```

프로젝트에 TMP Settings Asset이 없는 상태에서 `TMP_Settings.defaultFontAsset`을 직접 참조한 것이 원인이었다.

Project J의 기존 UI 구조와 통일하기 위해 93일차 HUD의 TextMeshPro 의존성을 제거하고 `UnityEngine.UI.Text` 방식으로 변경했다.

Font는 Unity 기본 Runtime Font를 사용한다.

```text
LegacyRuntime.ttf
```

이에 따라 TMP Settings가 없는 프로젝트에서도 HUD Installer가 Font 설정 과정에서 NullReferenceException을 발생시키지 않도록 수정했다.

---

## 주요 변경 파일

```text
Assets/ProjectJ/Editor/
└─ ProjectJDay93GameHUDInstaller.cs

Assets/ProjectJ/Network/Fusion/UI/
└─ ProjectJDay93GameHUD.cs

Assets/ProjectJ/Scenes/
└─ Game.unity
```

각 신규 Script의 `.meta` 파일도 함께 추가했다.

---

## 확인 사항

저장소에 반영된 93일차 변경 내용을 기준으로 다음 사항을 확인했다.

- `ProjectJDay93GameHUDInstaller.cs` 추가
- `ProjectJDay93GameHUD.cs` 추가
- `Game.unity`에 Day93 HUD 구조 저장
- TMP 관련 기본 Font 참조 제거
- `UnityEngine.UI.Text` 기반 UI로 전환
- `LegacyRuntime.ttf` Font 적용
- 기존 Fusion 경기 상태와 HUD 연결
- 기존 Scene Flow를 통한 Lobby 복귀 연결

GitHub Actions 또는 자동 PlayMode 테스트는 현재 저장소에 연결되어 있지 않으므로 Host/Client 실제 화면 동기화는 Unity PlayMode 수동 검증 대상으로 남는다.

---

## 93일차 결과
Game Scene에 실제 경기용 HUD 구조를 추가하고 기존 Fusion Networked 데이터와 연결했다.

경기 시간, 높이, 순위, Stamina, 2슬롯 아이템, 부활 보호 상태를 표시할 수 있으며 3초 Countdown과 Result 화면, Lobby 복귀 흐름까지 하나의 Canvas UI로 통합했다.

초기 HUD Installer에서 발생한 TMP 기본 Font NullReferenceException도 Project J 기존 UI 방식에 맞춰 `UnityEngine.UI.Text + LegacyRuntime.ttf` 구조로 변경하여 해결했다.
