---

# Project J - 106일차 개발일지

---

## 개발 방향

105일차에 구성한 F1 통합 디버그 패널을 여러 Game View 해상도에서 안정적으로 사용할 수 있도록 배치 계산을 정책으로 분리한다.

패널이 화면 경계를 벗어나지 않게 제한하고, 작은 해상도에서는 좌측 진단창 목록을 자동으로 축소해 우측 내용 영역을 확보한다. 네트워크·이동·카메라·Steam 동작은 변경하지 않는다.

---

## 변경 내용

---

### 1. 통합 패널 배치 정책 추가

`ProjectJUnifiedDebugPanelLayoutPolicy`를 추가해 다음 계산을 Runtime 정책으로 분리했다.

- 화면 내부 패널 영역 계산
- 화면 가장자리 16px 여백 유지
- 최대 패널 크기 `1440×1000` 제한
- 패널 너비에 따른 좌측 목록 너비 계산
- 기존 고정 좌표 진단창용 가상 스크롤 영역 계산

패널 영역은 현재 화면 크기에서 좌우·상하 여백을 제외한 뒤 중앙에 배치된다. 큰 화면에서는 최대 크기를 넘지 않고, 작은 화면에서는 화면 내부 크기에 맞춰 축소된다.

---

### 2. 좌측 진단창 목록 반응형 처리

기존 좌측 목록은 해상도와 관계없이 220px 고정 너비를 사용했다.

이번 변경에서는 다음 기준으로 목록 너비를 계산한다.

```text
기본 계산: 패널 너비의 32%
권장 최소: 120px
최대 제한: 220px
우측 내용 최소 확보: 120px
```

`1920×1080` 같은 큰 화면에서는 기존 220px 너비를 유지하고, `640×360`처럼 작은 Game View에서는 목록이 자동으로 좁아진다.

우측 내용 영역과 목록 스크롤 너비에는 음수 방지 처리를 적용했다.

---

### 3. 기존 진단창 스크롤 영역 유지

기존 디버그 창은 화면 좌표를 기준으로 `OnGUI()` 내용을 출력한다.

통합 패널 안에서 기존 좌표 기반 UI가 잘리지 않도록 다음 최소 가상 영역을 유지한다.

```text
최소 가상 너비: 1280px
최소 가상 높이: 1080px
```

현재 화면이 이 값보다 크면 실제 화면 크기를 사용한다. 작은 화면에서는 통합 패널의 가로·세로 스크롤을 통해 전체 내용을 확인한다.

---

### 4. EditMode 배치 테스트 추가

`ProjectJUnifiedDebugPanelLayoutPolicyTests`에 총 8개 테스트 사례를 구성했다.

- `1920×1080` 화면의 최대 패널 크기와 중앙 배치
- `1280×720` 화면의 16px 외곽 여백
- `320×240` 화면의 경계 초과 방지
- 패널 너비별 좌측 목록 너비 3개
- 화면 크기별 기존 진단창 가상 영역 2개

사용자가 Unity Test Runner에서 Layout Policy 테스트 8개가 모두 통과한 것을 확인했다.

---

### 5. Respawn 진단창 탭 분류 수정

전체 EditMode 테스트 실행 중 `ProjectJUnifiedDebugPanelPolicyTests` 18개 가운데 다음 사례 1개가 실패했다.

```text
RespawnProtectionDebugView
Expected: Gameplay
Actual: Player
```

원인은 `Respawn` 키워드가 Player 분류 규칙에 포함되어 있어 Gameplay 규칙보다 먼저 적용된 것이었다.

테스트 기대값과 통합 패널 설계에 맞춰 `Respawn`을 Player 규칙에서 제거하고 Gameplay 규칙으로 이동했다.

- 체크포인트·로컬 플레이어·관전: 플레이어 탭
- 부활·부활 보호·낙하·완주·경기 상태: 게임 상태 탭

---

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
└─ ProjectJDebugWindowMenu.cs

Assets/ProjectJ/Runtime/Debugging/
├─ ProjectJUnifiedDebugPanelLayoutPolicy.cs
├─ ProjectJUnifiedDebugPanelLayoutPolicy.cs.meta
└─ ProjectJUnifiedDebugPanelPolicy.cs

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJUnifiedDebugPanelLayoutPolicyTests.cs
└─ ProjectJUnifiedDebugPanelLayoutPolicyTests.cs.meta
```

- 수정 파일: 2개
- 생성 파일: 4개
- 삭제 파일: 없음

Scene, Hierarchy, Inspector, Prefab, 네트워크 설정과 Project Settings 변경은 없다.

---

## 확인 절차

1. Unity를 실행하고 컴파일 완료 후 Console Error가 없는지 확인한다.
2. `Window → General → Test Runner → EditMode`를 연다.
3. `ProjectJUnifiedDebugPanelLayoutPolicyTests` 8개를 실행한다.
4. `ProjectJUnifiedDebugPanelPolicyTests` 18개를 다시 실행한다.
5. EditMode 전체 380개 테스트를 실행한다.
6. Game View를 `1920×1080`으로 설정하고 F1 패널을 확인한다.
7. Game View를 `1280×720`으로 설정하고 16px 외곽 여백을 확인한다.
8. Game View를 `640×360`으로 설정하고 좌측 목록이 축소되는지 확인한다.
9. 작은 화면에서도 우측 내용 영역과 가로·세로 스크롤이 남아 있는지 확인한다.
10. `RespawnDebugView`와 `RespawnProtectionDebugView`가 게임 상태 탭에 표시되는지 확인한다.
11. F1 패널 열기·닫기와 ALT 커서 전환을 확인한다.
12. Scene 전환 후 패널이 자동으로 닫히는지 확인한다.
13. Host·Client가 Private Room에서 전체 경기 1회를 완료한다.
14. 경기 종료까지 Console Error가 0건인지 확인한다.

---

## 검증 결과

GitHub 최신 커밋의 변경 파일 6개가 106일차 배치 안정화 패키지와 분류 수정 패키지의 합본과 일치하는 것을 확인했다.

- 최신 커밋 변경 범위: 수정 2개, 생성 4개, 삭제 0개
- 두 배포 패키지와 최신 커밋 파일 6개 바이트 일치
- Git diff 공백 오류 없음
- 변경 C# 파일의 중괄호와 전처리기 균형 확인
- 신규 `.meta` GUID 중복 없음
- Layout Policy 테스트 사례 8개 구성 확인
- 사용자의 Unity Test Runner에서 Layout Policy 테스트 8개 통과 확인
- `Respawn` 키워드가 Gameplay 규칙에만 존재하는지 확인
- Scene, Prefab과 Project Settings 미변경 확인

수정 전 전체 EditMode 380개 실행에서는 Policy 테스트 1개가 실패했고 원인을 수정했다. 수정 적용 후 Policy 테스트 18개와 전체 EditMode 380개의 재실행 결과는 아직 확인되지 않았다.

현재 검증 환경에는 Unity Editor가 없어 실제 컴파일, 수정 후 Test Runner, Play Mode, Windows Development Build와 Host·Client 2인 접속은 직접 실행하지 못했다.

---

## 구현 확인 기준 커밋

개발일지 반영 전 확인한 커밋은 다음과 같다.

```text
2c84ca920a1cf6a3ffc3d0639137f64ec4fcc07e
106
```

---

## 106일차 결과

통합 디버그 패널의 화면 경계와 좌측 목록 너비 계산을 별도 정책으로 분리해 큰 화면과 작은 화면 모두에서 사용할 수 있는 반응형 배치 기반을 구성했다.

전체 테스트에서 발견된 Respawn 탭 분류 충돌도 Gameplay 기준으로 수정해 테스트 기대값과 패널 분류 규칙을 일치시켰다.
