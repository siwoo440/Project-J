---

# 프로젝트 J — 40일차 개발 일지

---

## 개발 주제

**OnGUI 제거 및 Canvas 기반 UI 시스템 구축**

---

## 개발 목표

기존 `OnGUI()` 방식으로 표시하던 인게임 HUD, 부활 메뉴, 결과 화면과 치명 오류 화면을 Unity Canvas와 TextMeshPro 기반 UI로 교체했다.

39일차에 구현한 2슬롯 인벤토리를 새 HUD에 연결하고, 해상도 변화에 대응할 수 있는 Canvas 구조와 한글 표시가 가능한 폰트 환경을 함께 구축했다.

---

## 주요 개발 내용

### 1. 기존 OnGUI UI를 Canvas 방식으로 교체

기존 UI 스크립트의 파일명과 클래스명은 유지하면서 내부 표시 방식을 Canvas 기반으로 변경했다.

| 스크립트 | 기존 방식 | 변경 결과 |
|---|---|---|
| `MinimalPlayerHud` | `OnGUI()` 직접 그리기 | Canvas HUD와 TextMeshPro 표시 |
| `PrototypeRespawnMenu` | `GUI.Button` 기반 메뉴 | Canvas Panel과 Button 기반 메뉴 |
| `FatalErrorScreen` | `GUI.Box` 기반 오류 안내 | 독립 Canvas 기반 오류 화면 |

기존 클래스명을 유지하여 Scene과 다른 스크립트가 사용하던 참조가 불필요하게 끊어지지 않도록 구성했다.

### 2. 인게임 HUD 구성

`GameCanvas` 아래에 플레이 정보를 역할별로 배치했다.

- 상단 중앙: 남은 경기 시간과 현재 순위
- 왼쪽 상단: 현재 높이, 최고 높이, 구간, 전체 진행률, 체크포인트, 정상 도달 상태
- 오른쪽 상단: 참가자별 실시간 순위
- 왼쪽 하단: 스태미나 수치와 진행 막대
- 오른쪽 하단: 아이템 슬롯 2개
- 화면 중앙: 부활 안내
- 전체 화면 팝업: 경기 결과와 최종 순위

코스 진행률과 스태미나는 `Image.Type.Filled` 방식의 가로 진행 막대로 표시하도록 구성했다.

### 3. 경기 데이터와 HUD 연결

`MinimalPlayerHud`가 기존 게임 시스템의 데이터를 읽어 Canvas 요소를 갱신하도록 수정했다.

- `PrototypeMatchController`: 남은 시간, 참가자 수, 현재 순위, 최종 결과
- `PlayerHeightProgressController`: 현재 높이, 최고 높이, 현재 구간, 전체 진행률
- `PlayerRespawnController`: 체크포인트, 부활 상태, 정상 도달 여부
- `PlayerMovementController`: 현재 스태미나 비율
- `PlayerItemInventory`: 보유 아이템과 인벤토리 변경 이벤트

실시간 순위와 최종 순위는 참가자 수에 맞춰 TextMeshPro 항목을 동적으로 생성하고, 로컬 플레이어는 청록색과 굵은 글씨로 강조하도록 구현했다.

### 4. 2슬롯 인벤토리 표시

`CanvasItemSlotView`를 추가하여 39일차에 구현한 인벤토리 데이터를 Canvas HUD에 표시하도록 연결했다.

| 슬롯 상태 | 표시 방식 |
|---|---|
| 빈 슬롯 | `비어 있음` 문구와 어두운 배경 |
| 아이콘이 있는 아이템 | 등록된 `InventoryIcon` 표시 |
| 아이콘이 없는 아이템 | 아이템의 `PickupColor`를 슬롯 배경에 적용 |
| 아이템 보유 | 표시 이름 또는 데이터 ID 출력 |

`PlayerItemInventory.InventoryChanged` 이벤트가 발생하면 두 슬롯을 즉시 갱신하며, 컴포넌트 비활성화 시 이벤트 연결을 안전하게 해제하도록 구성했다.

### 5. ESC 경기 메뉴와 부활 UI 전환

`PrototypeRespawnMenu`를 Canvas 기반 메뉴로 교체했다.

- ESC 입력으로 메뉴 열기와 닫기
- 메뉴 표시 중 플레이어 입력 차단
- 메뉴 표시 전 커서 잠금과 표시 상태 저장
- 메뉴 종료 시 입력과 커서 상태 복구
- 현재 체크포인트 정보 표시
- 마지막 체크포인트 부활 Button 연결
- 경기 복귀 Button 연결
- 부활 중 전체 화면 암전 표시
- 부활 또는 경기 종료 상태에서 메뉴 자동 닫기

Button 이벤트는 실행 시 연결하고 비활성화 시 해제하여 중복 호출을 방지했다.

### 6. 경기 결과 화면 구성

경기가 종료되면 Canvas 결과 화면을 활성화하고 다음 정보를 표시하도록 구현했다.

- 승리, 공동 승리, 패배 또는 경기 종료 상태
- 정상 지점 도달 또는 제한 시간 만료와 같은 종료 원인
- 플레이어 최종 순위와 전체 참가자 수
- 참가자별 최종 높이
- 공동 순위 표시
- 정상 도달 참가자 표시
- 로컬 플레이어 강조

### 7. 치명 오류 화면 Canvas 전환

`FatalErrorScreen`을 Canvas와 TextMeshPro 기반으로 변경했다.

- 오류 제목과 상세 메시지 표시
- 게임 종료 Button 제공
- 커서 잠금 해제와 커서 표시
- 다른 UI보다 높은 `Sorting Order` 적용
- Scene 참조 누락 시 런타임 최소 오류 Canvas 자동 생성
- Input System용 EventSystem 자동 보장

### 8. Canvas 자동 설정 도구 추가

`Day40CanvasUISetupTool`을 추가하여 `Game` Scene의 UI 구성을 자동으로 생성하고 연결하도록 구성했다.

- `GameCanvas` 생성
- `Screen Space - Overlay` 적용
- 기준 해상도 `1920 × 1080` 적용
- 가로와 세로 대응 비율 `0.5` 적용
- HUD 영역별 Panel, Text, Image 생성
- 실시간 순위와 결과 목록 템플릿 생성
- 아이템 슬롯 2개 생성
- ESC 메뉴와 부활 암전 생성
- 기존 데이터 제공자와 UI 참조 자동 연결
- 기존 `StandaloneInputModule` 제거
- `InputSystemUIInputModule` 보장
- Scene 변경 사항 저장 대상으로 등록

### 9. TextMeshPro와 한글 폰트 적용

TextMeshPro 필수 리소스를 프로젝트에 추가하고 `Noto Sans KR` 폰트를 등록했다.

- Thin
- ExtraLight
- Light
- Regular
- Medium
- SemiBold
- Bold
- ExtraBold
- Black

각 굵기의 TTF와 SDF Font Asset을 추가했으며, `TMP Settings`의 기본 Font Asset을 `NotoSansKR-Regular SDF`로 설정하여 Canvas UI의 한글 깨짐을 해결할 수 있는 기반을 마련했다.

### 10. Canvas 표시 문구 규칙 분리

`CanvasUiTextRules`를 추가하여 HUD에서 사용하는 문구 생성 규칙을 한곳에서 관리하도록 구성했다.

- 남은 시간을 `00:00` 형식으로 변환
- 현재 순위와 참가자 수 표시
- 승리, 공동 승리, 패배 문구 변환
- 경기 종료 원인 문구 변환
- 공동 순위와 로컬 플레이어 표시
- 최종 순위에서 정상 도달 상태 표시

### 11. EditMode 테스트 추가

`CanvasUiTextRulesTests`를 추가하여 Canvas에 표시되는 핵심 문구 규칙을 검사하도록 구성했다.

| 테스트 구분 | 검사 내용 |
|---|---|
| 경기 시간 | 0초, 소수점 시간, 1분 이상, 음수 보정 |
| 순위 | 잘못된 순위와 참가자 수의 최소값 보정 |
| 경기 결과 | 승리, 공동 승리, 패배, 미확정 결과 문구 |
| 순위 항목 | 로컬 플레이어, 공동 순위, 정상 도달 표시 |

총 4개의 테스트 메서드와 10개의 테스트 사례가 추가됐다.

### 12. Assembly 참조 갱신

Canvas와 TextMeshPro API를 사용할 수 있도록 Assembly Definition 참조를 갱신했다.

```text
Unity.InputSystem
Unity.TextMeshPro
UnityEngine.UI
```

---

## 변경된 주요 파일

### 수정 파일

```text
Assets/_ProjectJ/Scenes/Game/Game.unity
Assets/_ProjectJ/Scripts/Runtime/ProjectJ.Runtime.asmdef
Assets/_ProjectJ/Scripts/Editor/ProjectJ.Editor.asmdef
Assets/_ProjectJ/Scripts/Runtime/UI/HUD/MinimalPlayerHud.cs
Assets/_ProjectJ/Scripts/Runtime/UI/Menu/PrototypeRespawnMenu.cs
Assets/_ProjectJ/Scripts/Runtime/UI/System/FatalErrorScreen.cs
```

### 신규 파일

```text
Assets/_ProjectJ/Scripts/Runtime/UI/HUD/CanvasItemSlotView.cs
Assets/_ProjectJ/Scripts/Runtime/UI/HUD/CanvasUiTextRules.cs
Assets/_ProjectJ/Scripts/Editor/Day40CanvasUISetupTool.cs
Assets/_ProjectJ/Tests/EditMode/CanvasUiTextRulesTests.cs
Assets/_ProjectJ/Fonts/Source/NotoSansKR-*.ttf
Assets/_ProjectJ/Fonts/Source/NotoSansKR-* SDF.asset
Assets/TextMesh Pro/
```

---

## 구현 결과

- 플레이어용 `OnGUI()` UI를 Canvas 방식으로 교체
- TextMeshPro 기반 HUD 문구 표시
- 시간, 순위, 높이, 진행률, 체크포인트 표시
- 스태미나 진행 막대 표시
- 실시간 참가자 순위 표시
- 경기 종료 결과와 최종 순위 표시
- 2슬롯 인벤토리 표시
- ESC 경기 메뉴와 Canvas Button 구현
- 부활 안내와 화면 암전 구현
- 치명 오류 화면 Canvas 전환
- Input System 기반 UI 입력 구성
- Canvas 자동 설정 도구 추가
- Noto Sans KR 폰트와 SDF Font Asset 등록
- 프로젝트 기본 TMP 폰트를 Noto Sans KR Regular로 변경
- Canvas 문구 규칙 테스트 추가

---

## 확인 사항

최신 커밋의 변경 내역을 기준으로 다음 항목을 확인했다.

- 40일차 Canvas UI 관련 신규·수정 파일 포함
- `Game.unity`에 Canvas UI 구성 변경 포함
- Runtime과 Editor Assembly에 TextMeshPro 및 uGUI 참조 포함
- `TMP Settings` 기본 Font Asset이 `NotoSansKR-Regular SDF`를 참조
- 기존 HUD, 부활 메뉴, 치명 오류 화면에서 `OnGUI()` 구현 제거
- Canvas 문구 규칙용 EditMode 테스트 코드 포함

Unity Editor의 Console 오류 여부, Test Runner 통과 여부와 해상도별 실제 화면 배치는 저장소 파일만으로 확정할 수 없으므로 별도 실행 확인이 필요하다.

---

## 다음 개발 방향

41일차에는 아이템 상자의 생성 위치, 생성 확률과 재생성 규칙을 구축하고 설치형 아이템이 사용할 공통 위치 검사 기반을 준비한다.

- 맵 내부 상자 생성 지점 정의
- 지점별 상자 생성 확률 적용
- 같은 위치의 중복 생성 방지
- 획득 후 재생성 조건과 대기 시간 구성
- 경기 상태에 따른 상자 생성 제한
- 설치 가능한 지면과 장애물 충돌 검사
- 설치형 아이템 공통 배치 가능 여부 판정 준비
- Canvas의 아이템 슬롯 갱신 연동 확인

---

## 커밋 정보

```text
40일차 : OnGUI 제거 및 Canvas 기반 UI 시스템 구축
```

```text
cc936a53eb2a283009f979b1f7a3aaef9f966a82
```
