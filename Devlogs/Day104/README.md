---

# Project J - 104일차 개발일지

---

## 개발 방향

103일차까지 구성한 이동 품질 진단 환경을 유지하면서, 개발용 UI를 조작할 때 ALT 커서 해제가 안정적으로 유지되지 않던 문제를 수정한다.

ALT를 누르고 있는 동안만 커서를 표시하던 구조를 토글 방식으로 변경하고, 커서가 해제된 동안에는 Fusion 로컬 카메라의 회전과 줌 입력을 차단한다.

추가로 MainMenu와 Lobby의 UI 배치·텍스트 크기를 조정하고, Game 카메라와 Character Preview Light에 필요한 URP 추가 데이터를 Scene에 반영한다.

NetworkTransform, Prediction, 플레이어 이동값과 카메라 보간값은 변경하지 않는다.

---

## 변경 내용

---

### 1. ALT 커서 입력을 토글 방식으로 변경

기존 ALT 커서 기능은 키를 누르고 있는 동안만 커서 잠금을 해제했다.

```text
기존
ALT 누르고 있음 → 커서 표시
ALT 해제 → 즉시 커서 잠금 복구
```

104일차에서는 다음과 같은 토글 방식으로 변경했다.

```text
변경
ALT 한 번 누름 → 커서 표시·잠금 해제
ALT 다시 누름 → 이전 잠금 상태 복구
```

왼쪽 ALT와 오른쪽 ALT 입력은 `wasPressedThisFrame`을 사용해 한 번만 처리한다.

---

### 2. 커서 해제 상태 유지

ALT로 커서를 해제한 뒤 포커스가 바뀌어도 즉시 잠금 상태로 돌아가지 않도록 수정했다.

커서가 해제된 상태에서 다른 컴포넌트가 다시 커서를 잠그면 다음 Update에서 잠금 해제와 표시 상태를 다시 적용한다.

컨트롤러가 비활성화될 때는 ALT 입력 전에 저장한 커서 잠금 상태와 표시 상태를 복구한다.

해당 기능은 기존 정책대로 Unity Editor와 Development Build에서만 작동한다.

---

### 3. 커서 사용 중 Fusion 카메라 입력 차단

커서를 해제한 상태에서 UI를 클릭하거나 마우스를 이동할 때 로컬 카메라가 함께 회전하는 문제를 방지했다.

`ProjectJLocalPlayerPresentationController`는 커서 해제 상태를 확인해 다음 입력만 일시적으로 건너뛴다.

- 마우스 시점 회전
- 마우스 휠 카메라 줌

카메라 위치 추적과 FOV 갱신은 계속 처리하므로 네트워크 플레이어 표시와 카메라 보간 상태는 유지된다.

---

### 4. 커서 정책과 EditMode 테스트 추가

ALT 커서 상태 판단을 `ProjectJDebugCursorReleasePolicy`로 분리했다.

다음 4개 사례를 EditMode 테스트로 구성했다.

- 잠금 상태에서 ALT 입력 시 해제 상태 전환
- 해제 상태에서 ALT 입력 시 잠금 상태 전환
- 커서 잠금 상태에서 카메라 입력 허용
- 커서 해제 상태에서 카메라 입력 차단

---

### 5. MainMenu UI 조정

MainMenu Scene에서 다음 UI 요소를 직접 조정했다.

- 메뉴 버튼과 아이콘 위치 조정
- 제목·안내문·버튼 텍스트 최대 크기 조정
- 하단 UI 영역의 위치와 크기 조정
- Private Room 안내문 줄바꿈 정리
- 일부 패널과 UI 오브젝트 활성 상태 조정
- Character Preview Light에 URP Additional Light Data 반영

MainMenu의 기존 Host·Join·Private Room 기능과 네트워크 연결 코드는 변경하지 않았다.

---

### 6. Lobby UI 조정

Lobby Scene에서 플레이어 카드와 텍스트의 위치·최대 크기를 조정했다.

- 플레이어 카드 하단 UI 위치 조정
- Lobby 제목과 상태 텍스트 위치 조정
- 버튼·플레이어 정보 텍스트 최대 크기 조정

Lobby Ready, Leave와 Game 진입 로직은 변경하지 않았다.

---

### 7. Game 카메라 URP 데이터 반영

Game Scene의 Main Camera에 `UniversalAdditionalCameraData`가 직렬화됐다.

기존 카메라 Transform과 네트워크 플레이 카메라 제어 로직은 변경하지 않았다.

---

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Presentation/
└─ ProjectJLocalPlayerPresentationController.cs

Assets/ProjectJ/Runtime/Debugging/
├─ ProjectJDebugCursorReleaseController.cs
├─ ProjectJDebugCursorReleasePolicy.cs
└─ ProjectJDebugCursorReleasePolicy.cs.meta

Assets/ProjectJ/Scenes/
├─ Game.unity
├─ Lobby.unity
└─ MainMenu.unity

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJDebugCursorReleasePolicyTests.cs
└─ ProjectJDebugCursorReleasePolicyTests.cs.meta
```

- 수정 파일: 5개
- 생성 파일: 4개
- 삭제 파일: 없음

---

## 확인 절차

1. Unity를 실행하고 컴파일 완료를 기다린다.
2. Console에 컴파일 Error가 없는지 확인한다.
3. `Window → General → Test Runner → EditMode`를 연다.
4. `ProjectJDebugCursorReleasePolicyTests`의 테스트 4개를 실행한다.
5. MainMenu와 Lobby UI 배치가 의도한 상태인지 확인한다.
6. Host와 Client가 Private Room으로 Game Scene에 진입한다.
7. ALT를 한 번 눌러 커서가 표시되고 잠금이 해제되는지 확인한다.
8. 커서를 움직이거나 휠을 사용할 때 카메라가 회전·확대되지 않는지 확인한다.
9. ALT를 다시 눌러 커서 잠금과 카메라 입력이 복구되는지 확인한다.
10. ALT로 커서를 해제한 뒤 창 포커스를 전환하고 돌아와 상태가 유지되는지 확인한다.
11. F6 진단창과 F10 측정 초기화가 기존처럼 작동하는지 확인한다.
12. Host·Client 전체 경기 1회를 완료하고 Console Error가 0건인지 확인한다.

---

## 검증 결과

GitHub 최신 커밋의 변경 파일 9개를 확인했다.

- ALT 커서 배포 파일 6개와 최신 커밋의 코드 변경 범위 일치
- 의도된 Scene 변경 3개 확인
- 수정 5개, 생성 4개, 삭제 0개 확인
- ALT 토글 정책 테스트 사례 4개 구성 확인
- Runtime과 EditMode 어셈블리 참조 구조 확인
- `.meta` GUID 중복 없음
- NetworkTransform, Prediction과 플레이어 이동값 미변경 확인

Unity Scene YAML의 빈 `m_Name` 직렬화 줄 2곳에서 Git trailing whitespace 경고가 확인됐다. Unity가 생성한 빈 직렬화 필드이며 C# 컴파일 문제는 아니다.

현재 검증 환경에는 Unity Editor가 없어 실제 컴파일, EditMode Test Runner, Windows Development Build와 Host·Client 실행은 확인하지 못했다. ALT 토글과 변경된 UI 배치는 Unity에서 최종 확인이 필요하다.

---

## 구현 확인 기준 커밋

개발일지 반영 전 확인한 커밋은 다음과 같다.

```text
5d6e25ceea8e9f2c56e8759747d52f40f9eae890
a
```

---

## 104일차 결과

개발용 커서 조작을 ALT 토글 방식으로 변경하고, 커서 사용 중 Fusion 카메라 입력을 차단해 디버그 UI 조작 흐름을 안정화했다.

MainMenu와 Lobby UI 배치를 조정하고 Game 카메라와 Character Preview Light의 URP 추가 데이터를 Scene에 반영했다.

다음 일차에서는 103일차 진단 화면을 사용해 Host·Client 이동 품질을 실제로 측정하고, NetworkTransform 또는 Prediction 설정 변경 여부를 측정 결과에 따라 결정한다.
