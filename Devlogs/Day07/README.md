# Project J

3D 3인칭 온라인 수직 점프 경쟁 파티 게임 **Project J**의 개발 저장소입니다.

---

# 개발 환경

| 항목 | 내용 |
|---|---|
| 게임 엔진 | Unity 6 |
| Unity 버전 | 6000.3.21f1 |
| 프로젝트 템플릿 | Universal 3D |
| 렌더 파이프라인 | URP |
| 대상 플랫폼 | Steam PC |
| 개발 인원 | 1인 개발 |
| 기본 온라인 인원 | 4~8인 |
| 입력 시스템 | Unity Input System 1.20.0 |
| 플레이어 이동 예정 방식 | CharacterController |
| 저장소 | siwoo440/Project-J |

---

# 7일차 : 플레이어 설정 에셋 구성

## 개발 목표

플레이어의 이동·달리기·앉기·점프·중력·공중 제어·스태미나 수치를 코드에서 분리하여 `PLY-001_DefaultPlayer.asset`에서 관리할 수 있도록 구성했습니다.

7일차에서는 실제 캐릭터 이동을 구현하지 않고, 이후 플레이어 컨트롤러가 참조할 공통 설정 에셋과 수치 검증 체계를 먼저 확립했습니다.

이번 일차의 핵심 목표는 다음과 같습니다.

- 기존 `PlayerDataDefinition`을 실제 플레이어 설정 에셋으로 확장
- 지상 이동 설정 분리
- 달리기 설정 분리
- 앉기와 CharacterController 크기 설정 분리
- 기본 점프 설정 분리
- 중력과 최대 낙하 속도 설정 분리
- 공중 방향 제어 설정 분리
- 달리기 스태미나 설정 분리
- 공통 데이터 검증기에 플레이어 전용 검증 연결
- 기본 플레이어 데이터 버전을 `1.1.0`으로 갱신
- Unity 메뉴를 통한 기본 설정 자동 적용
- 플레이어 설정 오류를 검증하는 EditMode 테스트 추가
- Inspector에서 코드 수정 없이 플레이어 수치를 조정하는 기반 완성

---

# 최신 커밋

| 항목 | 내용 |
|---|---|
| 커밋 제목 | `7일차 : 플레이어 설정 에셋 구성` |
| 커밋 SHA | `bb12c035b62ad06f0c6f874f9c15f9bd02994686` |
| 브랜치 | `main` |
| 이전 커밋 | `f0c50f4e185349ff192c0822b0f26f829fca68e5` |
| 커밋 링크 | https://github.com/siwoo440/Project-J/commit/bb12c035b62ad06f0c6f874f9c15f9bd02994686 |

---

# 최신 커밋 검토 결과

최신 커밋을 기준으로 다음 항목을 확인했습니다.

- 커밋 제목이 `7일차 : 플레이어 설정 에셋 구성`으로 정상 등록
- 기존 `PLY-001_DefaultPlayer.asset`의 ID와 표시 이름 유지
- 플레이어 데이터 버전을 `1.0.0`에서 `1.1.0`으로 갱신
- Movement 설정 추가
- Sprint 설정 추가
- Crouch 설정 추가
- Jump 설정 추가
- Gravity 설정 추가
- Air Control 설정 추가
- Stamina 설정 추가
- 플레이어 설정값 전용 직렬화 구조체 7개 추가
- 플레이어 설정 전용 오류 코드와 검증 규칙 추가
- 공통 `ProjectDataValidator`에 플레이어 데이터 세부 검증 연결
- 기본 플레이어 설정 자동 구성 Editor 메뉴 추가
- 플레이어 설정 기본값과 오류를 검사하는 EditMode 테스트 8개 추가
- 관련 폴더·스크립트의 `.meta` 파일 추가

저장소에서 확인 가능한 범위에서는 수정이 필요한 치명적인 구조 오류를 발견하지 못했습니다.

현재 커밋에는 GitHub Actions 상태 검사나 Unity 자동 빌드 결과가 등록되어 있지 않습니다. 따라서 다음 항목은 로컬 Unity 에디터에서 최종 확인해야 합니다.

```text
Console Error: 0개
EditMode Passed: 31개
EditMode Failed: 0개
전체 데이터 검증 성공
PLY-001 에셋 버전: 1.1.0
Inspector 수치 수정과 저장 정상
```

---

# 기본 플레이어 설정 에셋

## 1. PLY-001_DefaultPlayer

파일 위치:

```text
Assets/_ProjectJ/Data/Definitions/Player/PLY-001_DefaultPlayer.asset
```

공통 식별 정보:

```text
Data Id: PLY-001
Display Name: Default Player
Version: 1.1.0
Category: Player
```

6일차의 기본 플레이어 데이터 버전은 `1.0.0`이었습니다.

7일차에서 이동·달리기·앉기·점프·중력·공중 제어·스태미나 필드가 추가되어 부 버전을 올렸습니다.

```text
1.0.0
→ 1.1.0
```

기존 ID인 `PLY-001`은 그대로 유지했습니다.

---

# 플레이어 설정 구조

## 2. 최종 설정 구역

`PLY-001_DefaultPlayer.asset`은 다음 일곱 설정 구역을 가집니다.

```text
PLY-001_DefaultPlayer
├─ Movement
├─ Sprint
├─ Crouch
├─ Jump
├─ Gravity
├─ Air Control
└─ Stamina
```

각 구역은 별도의 직렬화 구조체로 분리했습니다.

이후 플레이어 컨트롤러는 개별 숫자를 직접 보유하지 않고 `PlayerDataDefinition`을 참조하여 값을 읽는 구조로 확장할 수 있습니다.

---

# Movement 설정

## 3. PlayerMovementSettings

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Player/PlayerMovementSettings.cs
```

기본값:

| 필드 | 값 | 역할 |
|---|---:|---|
| Move Speed | 6 | 기본 지상 이동 최고 속도 |
| Acceleration | 24 | 목표 속도까지 증가하는 지상 가속도 |
| Deceleration | 30 | 입력 해제 시 정지하는 지상 감속도 |
| Rotation Speed | 720 | 이동 방향을 향하는 초당 회전 속도 |

`Move Speed`는 연결된 플레이어 데이터 시트의 기본값 `6m/s`를 사용했습니다.

나머지 값은 16~17일차 실제 CharacterController 이동 테스트를 위한 초기값입니다.

검증 규칙:

```text
Move Speed > 0
Acceleration > 0
Deceleration > 0
Rotation Speed > 0
```

오류 코드:

```text
PLAYER_MOVEMENT_INVALID
```

---

# Sprint 설정

## 4. PlayerSprintSettings

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Player/PlayerSprintSettings.cs
```

기본값:

| 필드 | 값 | 역할 |
|---|---:|---|
| Sprint Speed | 8 | 달리기 중 목표 이동 속도 |
| Sprint Acceleration | 30 | 달리기 속도까지 도달하는 가속도 |

검증 규칙:

```text
Sprint Speed > Move Speed
Sprint Acceleration > 0
```

달리기 속도가 기본 이동 속도보다 느리거나 같으면 달리기 기능의 의미가 없어지므로 오류로 처리합니다.

오류 코드:

```text
PLAYER_SPRINT_INVALID
```

실제 Shift 달리기와 스태미나 소비는 18일차에 구현할 예정입니다.

---

# Crouch 설정

## 5. PlayerCrouchSettings

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Player/PlayerCrouchSettings.cs
```

기본값:

| 필드 | 값 | 역할 |
|---|---:|---|
| Crouch Move Speed | 3.5 | 앉은 상태 이동 속도 |
| Standing Height | 2 | 서 있을 때 CharacterController 높이 |
| Crouching Height | 1.2 | 앉았을 때 CharacterController 높이 |
| Controller Radius | 0.45 | 공통 CharacterController 반지름 |
| Height Transition Speed | 8 | 서기와 앉기 크기 전환 속도 |
| Stand Clearance Padding | 0.05 | 일어서기 공간 검사의 여유 거리 |

검증 규칙:

```text
0 < Crouch Move Speed <= Move Speed
Controller Radius > 0
Standing Height >= Controller Radius × 2
Crouching Height >= Controller Radius × 2
Crouching Height < Standing Height
Height Transition Speed > 0
Stand Clearance Padding >= 0
```

CharacterController를 캡슐 형태로 유지하려면 높이가 지름보다 작아지지 않아야 하므로 반지름과 높이 관계도 검사합니다.

오류 코드:

```text
PLAYER_CROUCH_INVALID
```

실제 앉기 충돌체 전환과 머리 공간 검사는 19일차에 구현할 예정입니다.

---

# Jump 설정

## 6. PlayerJumpSettings

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Player/PlayerJumpSettings.cs
```

기본값:

| 필드 | 값 | 역할 |
|---|---:|---|
| Jump Height | 2.4 | 기본 점프 수직 도달 높이 |
| Coyote Time | 0.12 | 지면을 벗어난 직후에도 점프를 허용하는 시간 |
| Jump Buffer Time | 0.12 | 착지 직전 점프 입력을 보관하는 시간 |

`Jump Height`는 연결된 플레이어 데이터 시트의 기본값 `2.4m`를 사용했습니다.

검증 규칙:

```text
Jump Height > 0
0 <= Coyote Time <= 0.5
0 <= Jump Buffer Time <= 0.5
```

오류 코드:

```text
PLAYER_JUMP_INVALID
```

실제 점프 속도 계산과 입력 처리는 22일차에 구현할 예정입니다.

---

# Gravity 설정

## 7. PlayerGravitySettings

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Player/PlayerGravitySettings.cs
```

기본값:

| 필드 | 값 | 역할 |
|---|---:|---|
| Gravity Acceleration | -25 | 공중에서 적용되는 초당 하향 가속도 |
| Grounded Gravity | -2 | 접지 상태 유지를 위한 작은 하향 속도 |
| Maximum Fall Speed | 35 | 최대 낙하 속도의 절댓값 |

검증 규칙:

```text
Gravity Acceleration < 0
Grounded Gravity <= 0
Maximum Fall Speed > 0
```

중력 가속도와 접지 유지 속도는 아래 방향을 나타내는 음수 값을 사용합니다.

최대 낙하 속도는 절댓값 형태의 양수로 저장합니다.

오류 코드:

```text
PLAYER_GRAVITY_INVALID
```

실제 중력 누적과 최대 낙하 속도 제한은 24일차에 구현할 예정입니다.

---

# Air Control 설정

## 8. PlayerAirControlSettings

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Player/PlayerAirControlSettings.cs
```

기본값:

| 필드 | 값 | 역할 |
|---|---:|---|
| Control Ratio | 0.65 | 지상 대비 공중 방향 제어 비율 |
| Acceleration | 12 | 공중에서 목표 방향으로 전환하는 가속도 |

`Control Ratio`는 연결된 플레이어 데이터 시트의 기본값 `0.65`를 사용했습니다.

검증 규칙:

```text
0 < Control Ratio <= 1
Acceleration > 0
```

공중 제어 비율이 1을 초과하면 지상보다 공중 제어가 강해지므로 오류로 처리합니다.

오류 코드:

```text
PLAYER_AIR_CONTROL_INVALID
```

실제 공중 방향 수정은 23일차에 구현할 예정입니다.

---

# Stamina 설정

## 9. PlayerStaminaSettings

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Player/PlayerStaminaSettings.cs
```

기본값:

| 필드 | 값 | 역할 |
|---|---:|---|
| Maximum Stamina | 100 | 스태미나 최대값 |
| Sprint Drain Per Second | 20 | 달리기 중 초당 소비량 |
| Recovery Per Second | 25 | 회복 중 초당 회복량 |
| Recovery Delay | 0.75 | 달리기 종료 후 회복 시작 대기 시간 |
| Minimum Stamina To Start Sprint | 5 | 달리기를 시작하기 위한 최소 스태미나 |

초기값 기준 예상 동작:

```text
완전 충전 상태에서 연속 달리기: 약 5초
0에서 완전 회복: 약 4초
회복 시작 대기: 0.75초
```

검증 규칙:

```text
Maximum Stamina > 0
Sprint Drain Per Second > 0
Recovery Per Second > 0
Recovery Delay >= 0
0 <= Minimum Stamina To Start Sprint <= Maximum Stamina
```

오류 코드:

```text
PLAYER_STAMINA_INVALID
```

실제 스태미나 소비·회복·달리기 중단은 18일차에 구현할 예정입니다.

---

# PlayerDataDefinition 확장

## 10. PlayerDataDefinition 수정

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Definitions/PlayerDataDefinition.cs
```

기존 구조:

```text
Category: Player
```

변경된 구조:

```text
Category
Movement
Sprint
Crouch
Jump
Gravity
AirControl
Stamina
```

각 설정은 읽기 전용 속성으로 외부에 제공합니다.

```csharp
playerData.Movement
playerData.Sprint
playerData.Crouch
playerData.Jump
playerData.Gravity
playerData.AirControl
playerData.Stamina
```

Inspector 필드는 `SerializeField`로 저장되지만 외부 시스템은 속성을 통해서만 값을 읽도록 구성했습니다.

---

## 11. Editor 전용 설정 메서드

다음 메서드는 `UNITY_EDITOR` 조건 안에 구성했습니다.

```text
SetEditorSettings
ResetEditorSettingsToDefaults
```

역할:

```text
Editor 구성 도구에서 설정 일괄 적용
EditMode 테스트에서 임시 설정 교체
7일차 기본값으로 초기화
```

플레이어 빌드에서는 Editor 전용 설정 API가 포함되지 않습니다.

런타임 게임 코드는 에셋의 값을 읽는 역할만 담당하도록 분리했습니다.

---

# 플레이어 설정 검증

## 12. PlayerSettingsValidationRules

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Validation/PlayerSettingsValidationRules.cs
```

플레이어 데이터의 일곱 설정 구역을 차례대로 검사합니다.

검사 순서:

```text
Movement
→ Sprint
→ Crouch
→ Jump
→ Gravity
→ Air Control
→ Stamina
```

각 구역에서 오류가 발견되더라도 다음 구역 검사를 계속 진행합니다.

따라서 여러 설정이 동시에 잘못된 경우 모든 오류를 한 번의 검증 결과에 모을 수 있습니다.

---

## 13. 플레이어 설정 오류 코드

| 오류 코드 | 의미 |
|---|---|
| `PLAYER_MOVEMENT_INVALID` | 이동 속도·가속·감속·회전 값 오류 |
| `PLAYER_SPRINT_INVALID` | 달리기 속도나 달리기 가속도 오류 |
| `PLAYER_CROUCH_INVALID` | 앉기 속도나 CharacterController 크기 오류 |
| `PLAYER_JUMP_INVALID` | 점프 높이나 입력 보정 시간 오류 |
| `PLAYER_GRAVITY_INVALID` | 중력 방향이나 최대 낙하 속도 오류 |
| `PLAYER_AIR_CONTROL_INVALID` | 공중 제어 비율이나 가속도 오류 |
| `PLAYER_STAMINA_INVALID` | 스태미나 소비·회복 관계 오류 |

---

# 공통 검증기 연결

## 14. ProjectDataValidator 수정

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Validation/ProjectDataValidator.cs
```

기존 공통 검사:

```text
null 에셋
ID 누락
ID 형식
표시 이름
버전
중복 ID
```

7일차 추가 검사:

```text
현재 에셋이 PlayerDataDefinition인지 확인
→ PlayerSettingsValidationRules 실행
```

최종 검증 흐름:

```text
공통 데이터 검사
→ 데이터 분류별 세부 검사
→ 전체 ID 중복 검사
```

현재 분류별 세부 검증이 연결된 형식은 `PlayerDataDefinition`입니다.

이후 맵·장애물·아이템·꾸미기·오디오 데이터에 세부 필드가 추가되면 같은 위치에서 분류별 검증기를 연결할 수 있습니다.

---

# Editor 자동화

## 15. Day07PlayerSettingsSetupTool

파일 위치:

```text
Assets/_ProjectJ/Scripts/Editor/Day07PlayerSettingsSetupTool.cs
```

Unity 상단 메뉴에 다음 항목을 추가했습니다.

```text
Project J
└─ Day 07
   ├─ Configure Default Player Settings
   └─ Select Default Player Settings
```

Play Mode 실행 또는 진입 중에는 메뉴를 사용할 수 없습니다.

---

## 16. Configure Default Player Settings

다음 작업을 자동으로 수행합니다.

```text
Player 데이터 폴더 확인
→ PLY-001_DefaultPlayer.asset 검색
→ 에셋이 없으면 생성
→ 기존 Data Id 유지
→ 기존 Display Name 유지
→ 버전을 1.1.0으로 갱신
→ 7일차 기본 설정 적용
→ 에셋 저장
→ 전체 프로젝트 데이터 검증
→ 에셋 선택과 Project 창 위치 강조
```

성공 로그:

```text
[Day07] PLY-001 기본 플레이어 설정과 1.1.0 버전 적용을 완료했습니다.
```

검증 오류가 있으면 다음 형식의 로그를 출력합니다.

```text
[Day07] 플레이어 설정 적용 후 데이터 오류 n개를 발견했습니다.
```

---

## 17. Select Default Player Settings

다음 기본 플레이어 에셋을 Project 창에서 선택합니다.

```text
Assets/_ProjectJ/Data/Definitions/Player/PLY-001_DefaultPlayer.asset
```

기본 플레이어 에셋이 없으면 경로가 포함된 오류를 출력합니다.

---

# 코드 변경 없는 수치 조정

## 18. Inspector 기반 밸런스 조정

7일차 완료 후 다음 값을 C# 파일 수정 없이 Inspector에서 변경할 수 있습니다.

```text
이동 속도
가속·감속
회전 속도
달리기 속도
앉기 속도
CharacterController 높이와 반지름
점프 높이
코요테 타임
점프 버퍼
중력
최대 낙하 속도
공중 제어
스태미나 소비와 회복
```

예시:

```text
Move Speed
6 → 6.5
```

또는:

```text
Jump Height
2.4 → 2.6
```

저장된 값은 이후 플레이어 컨트롤러가 에셋을 참조하도록 구현할 때 그대로 사용됩니다.

현재는 데이터 에셋만 준비됐으며 실제 캐릭터 이동 코드는 아직 구현하지 않았습니다.

---

# EditMode 테스트

## 19. PlayerSettingsTests

파일 위치:

```text
Assets/_ProjectJ/Tests/EditMode/PlayerSettingsTests.cs
```

7일차에 다음 8개의 테스트를 추가했습니다.

### DefaultPlayerSettingsAreValid

7일차 기본 플레이어 설정 전체가 오류 없이 통과하는지 검사합니다.

### SheetBackedValuesMatchInitialDefaults

데이터 시트에서 가져온 세 가지 핵심 기본값을 검사합니다.

```text
Move Speed: 6
Jump Height: 2.4
Air Control Ratio: 0.65
```

### SprintSpeedMustExceedMoveSpeed

달리기 속도가 기본 이동 속도보다 빠른지 검사합니다.

### CrouchingHeightMustBeLowerThanStandingHeight

앉기 높이가 서기 높이보다 낮은지 검사합니다.

### GravityMustPointDownward

중력 가속도가 아래 방향을 의미하는 음수인지 검사합니다.

### AirControlRatioCannotExceedOne

공중 제어 비율이 1을 초과하지 않는지 검사합니다.

### MinimumSprintStaminaCannotExceedMaximum

달리기 시작 최소 스태미나가 최대 스태미나를 초과하지 않는지 검사합니다.

### ProjectDataValidatorReportsMultiplePlayerSettingErrors

여러 설정 구역에 잘못된 값이 있을 때 공통 검증기가 다음 오류를 모두 수집하는지 검사합니다.

```text
PLAYER_MOVEMENT_INVALID
PLAYER_SPRINT_INVALID
PLAYER_GRAVITY_INVALID
PLAYER_STAMINA_INVALID
```

---

# 전체 테스트 구성

기존 테스트:

```text
2일차 ProjectStructureTests: 2개
3일차 GameSceneCatalogTests: 3개
4일차 GameServiceRegistryTests: 4개
5일차 InputActionAssetTests: 6개
6일차 ProjectDataValidatorTests: 8개
```

7일차 신규 테스트:

```text
PlayerSettingsTests: 8개
```

예상 전체 결과:

```text
Passed: 31
Failed: 0
Ignored: 0
```

GitHub에는 Unity Test Runner 결과를 실행하는 CI가 등록되어 있지 않으므로 실제 통과 여부는 로컬 Unity에서 확인해야 합니다.

---

# 생성·수정된 주요 파일

## 수정된 파일

```text
Assets/_ProjectJ/Data/Definitions/Player/PLY-001_DefaultPlayer.asset
Assets/_ProjectJ/Scripts/Runtime/Data/Definitions/PlayerDataDefinition.cs
Assets/_ProjectJ/Scripts/Runtime/Data/Validation/ProjectDataValidator.cs
```

## 새로 생성된 Runtime 설정 파일

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Player
├─ PlayerMovementSettings.cs
├─ PlayerSprintSettings.cs
├─ PlayerCrouchSettings.cs
├─ PlayerJumpSettings.cs
├─ PlayerGravitySettings.cs
├─ PlayerAirControlSettings.cs
└─ PlayerStaminaSettings.cs
```

## 새로 생성된 검증 파일

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Validation
└─ PlayerSettingsValidationRules.cs
```

## 새로 생성된 Editor 파일

```text
Assets/_ProjectJ/Scripts/Editor
└─ Day07PlayerSettingsSetupTool.cs
```

## 새로 생성된 테스트 파일

```text
Assets/_ProjectJ/Tests/EditMode
└─ PlayerSettingsTests.cs
```

각 폴더와 스크립트의 `.meta` 파일도 함께 Git에 등록했습니다.

---

# 주요 프로젝트 구조

```text
Assets/_ProjectJ
├─ Data
│  └─ Definitions
│     └─ Player
│        └─ PLY-001_DefaultPlayer.asset
├─ Scripts
│  ├─ Runtime
│  │  └─ Data
│  │     ├─ Definitions
│  │     │  └─ PlayerDataDefinition.cs
│  │     ├─ Player
│  │     │  ├─ PlayerMovementSettings.cs
│  │     │  ├─ PlayerSprintSettings.cs
│  │     │  ├─ PlayerCrouchSettings.cs
│  │     │  ├─ PlayerJumpSettings.cs
│  │     │  ├─ PlayerGravitySettings.cs
│  │     │  ├─ PlayerAirControlSettings.cs
│  │     │  └─ PlayerStaminaSettings.cs
│  │     └─ Validation
│  │        ├─ ProjectDataValidator.cs
│  │        └─ PlayerSettingsValidationRules.cs
│  └─ Editor
│     └─ Day07PlayerSettingsSetupTool.cs
└─ Tests
   └─ EditMode
      └─ PlayerSettingsTests.cs
```

---

# 수동 검증 절차

## 20. 기본 플레이어 에셋 선택

Unity 메뉴:

```text
Project J
→ Day 07
→ Select Default Player Settings
```

다음 에셋이 선택되어야 합니다.

```text
PLY-001_DefaultPlayer.asset
```

Inspector 확인:

```text
Data Id: PLY-001
Display Name: Default Player
Version: 1.1.0
```

---

## 21. 플레이어 설정값 확인

다음 핵심값을 확인합니다.

```text
Move Speed: 6
Sprint Speed: 8
Crouch Move Speed: 3.5
Jump Height: 2.4
Gravity Acceleration: -25
Air Control Ratio: 0.65
Maximum Stamina: 100
```

---

## 22. 전체 데이터 검사

Unity 메뉴:

```text
Project J
→ Day 06
→ Validate All Data Assets
```

정상 로그:

```text
[Data] 데이터 에셋 6개의 ID와 필수 값 검증을 완료했습니다.
```

플레이어 설정 오류가 있으면 기존 공통 데이터 오류와 함께 Console에 출력됩니다.

---

## 23. 달리기 속도 오류 검사

임시 변경:

```text
Move Speed: 6
Sprint Speed: 6
```

예상 오류:

```text
PLAYER_SPRINT_INVALID
```

검사 후 복원:

```text
Sprint Speed: 8
```

---

## 24. 앉기 높이 오류 검사

임시 변경:

```text
Standing Height: 2
Crouching Height: 2
```

예상 오류:

```text
PLAYER_CROUCH_INVALID
```

검사 후 복원:

```text
Crouching Height: 1.2
```

---

## 25. 중력 방향 오류 검사

임시 변경:

```text
Gravity Acceleration: 25
```

예상 오류:

```text
PLAYER_GRAVITY_INVALID
```

검사 후 복원:

```text
Gravity Acceleration: -25
```

---

## 26. 스태미나 범위 오류 검사

임시 변경:

```text
Maximum Stamina: 100
Minimum Stamina To Start Sprint: 120
```

예상 오류:

```text
PLAYER_STAMINA_INVALID
```

검사 후 복원:

```text
Minimum Stamina To Start Sprint: 5
```

모든 시험용 값을 복원한 뒤 전체 데이터 검증을 다시 실행해야 합니다.

---

# 검증 결과

| 검증 항목 | 저장소 확인 |
|---|:---:|
| 최신 커밋 제목 정상 | 완료 |
| PLY-001 ID 유지 | 완료 |
| 데이터 버전 1.1.0 갱신 | 완료 |
| Movement 설정 추가 | 완료 |
| Sprint 설정 추가 | 완료 |
| Crouch 설정 추가 | 완료 |
| Jump 설정 추가 | 완료 |
| Gravity 설정 추가 | 완료 |
| Air Control 설정 추가 | 완료 |
| Stamina 설정 추가 | 완료 |
| 플레이어 설정 구조체 7개 추가 | 완료 |
| 플레이어 전용 검증 규칙 추가 | 완료 |
| 공통 검증기 연결 | 완료 |
| Editor 자동 구성 메뉴 추가 | 완료 |
| EditMode 테스트 8개 추가 | 완료 |
| GitHub CI 상태 검사 | 미구성 |

로컬 Unity 최종 확인 항목:

```text
Console Error: 0개
EditMode Passed: 31개
EditMode Failed: 0개
PLY-001 Inspector 설정 정상
전체 데이터 수동 검증 성공
시험용 오류 값 전부 복원
```

---

# 이후 확장 방향

7일차에서 구성한 플레이어 설정은 다음 일정에서 사용합니다.

| 일차 | 연결 기능 |
|---:|---|
| 16일차 | CharacterController 기본 이동 |
| 17일차 | 가속·감속과 빠른 정지 |
| 18일차 | 달리기와 스태미나 |
| 19일차 | 앉기 충돌체 전환 |
| 20일차 | 앉기 상태의 이동·점프·밀치기 |
| 22일차 | 기본 점프 |
| 23일차 | 공중 제어 |
| 24일차 | 중력과 최대 낙하 속도 |

실제 플레이어 시스템을 구현할 때 이번 설정값을 코드에 다시 작성하지 않고 `PlayerDataDefinition`에서 읽도록 구성합니다.

---

# 다음 개발 방향

## 8일차 : 물리 레이어와 충돌 행렬

다음 일차에는 다음 물리 레이어를 생성하고 서로 필요한 조합만 충돌하도록 설정합니다.

```text
Player
Ground
Obstacle
Checkpoint
ItemBox
Interactable
PushHitbox
RespawnProtection
```

주요 작업:

- 프로젝트 레이어 이름 정의
- Player와 Ground 충돌 설정
- Player와 Obstacle 충돌 설정
- Checkpoint Trigger 설정
- ItemBox Trigger 설정
- 밀치기 판정용 레이어 분리
- 부활 보호 상태 충돌 규칙 준비
- Physics 충돌 행렬 자동 구성 Editor 도구
- 레이어 번호와 충돌 관계 EditMode 테스트

완료 기준:

```text
의도한 레이어만 충돌 또는 Trigger 판정에 참여한다.
```

---

# 커밋 정보

```text
7일차 : 플레이어 설정 에셋 구성
```

```text
https://github.com/siwoo440/Project-J/commit/bb12c035b62ad06f0c6f874f9c15f9bd02994686
```
