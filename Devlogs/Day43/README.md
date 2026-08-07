# 프로젝트 J 개발일지

---

## 43일차 : P1 아이템 11종 효과 구현

### 개발 목표

42일차에 데이터와 실행 종류만 등록했던 P1 아이템 11종을 실제 경기에서 사용할 수 있도록 구현한다. 기존 P0 아이템 기능을 유지하면서 상승, 투척, 끌어당기기, 화면 방해, 설치형 효과에 필요한 공통 구조를 추가한다.

### 완료한 작업

- P1 아이템 11종의 실제 사용 효과 구현
- 제트팩의 `Space` 유지 상승 기능 추가
- 망치 사용 중 밀치기 힘과 사거리 강화 기능 추가
- 폭탄의 투척, 지연 폭발, 범위 밀치기 기능 추가
- 복어 풍선옷의 주기적인 근거리 밀치기 기능 추가
- 먹물 문어의 화면 방해 효과 추가
- 낚시대의 조준 대상 끌어당기기 기능 추가
- 갈고리의 구조물 조준 및 이동 기능 추가
- 비눗방울의 이동 제한과 `A/D` 교대 6회 탈출 기능 추가
- 연막탄의 투척 및 범위형 시야 방해 기능 추가
- 트램폴린의 설치와 사용자 전용 3회 상승 기능 추가
- 거대 풍선의 6초 자동 상승 기능 추가
- 지속 효과와 화면 방해 상태를 관리하는 공통 처리 추가
- P1 아이템 수치를 한곳에서 관리하는 규칙 클래스 추가
- P1 11종을 Scene과 데이터에 연결하는 자동 설정 도구 추가
- P1 공통 규칙을 검사하는 EditMode 테스트 추가
- `ITM-014`의 명칭을 `복어 갑옷`에서 `복어 풍선옷`으로 변경

### P1 아이템 구현 결과

| ID | 아이템 | 구현 효과 |
|---|---|---|
| ITM-011 | 제트팩 | 5초 동안 `Space` 유지 시 초당 5m 상승 |
| ITM-012 | 망치 | 6초 동안 밀치기 힘 1.75배, 사거리 2.5m 적용 |
| ITM-013 | 폭탄 | 투척 후 2.5초 뒤 반경 5m, 힘 10의 폭발 발생 |
| ITM-014 | 복어 풍선옷 | 5초 동안 반경 1.8m의 대상을 0.5초 간격으로 밀침 |
| ITM-015 | 먹물 문어 | 적중한 플레이어의 화면 중앙 65%를 3.5초간 가림 |
| ITM-016 | 낚시대 | 최대 14m의 조준 대상을 힘 10으로 끌어당김 |
| ITM-017 | 갈고리 | 최대 20m의 구조물까지 초당 12m로 이동 |
| ITM-018 | 비눗방울 | 이동·달리기·앉기를 제한하고 `A/D` 교대 6회 입력 시 탈출 |
| ITM-019 | 연막탄 | 반경 5m에 6초 동안 유지되는 시야 방해 구역 생성 |
| ITM-020 | 트램폴린 | 설치한 플레이어에게 상승 속도 12를 최대 3회 적용 |
| ITM-021 | 거대 풍선 | 6초 동안 초당 3.5m로 자동 상승 |

### 주요 구현 구조

- `P1ItemRules`에서 P1 아이템의 지속 시간, 범위, 힘, 입력 횟수 등 공통 수치 관리
- `PlayerItemUseController`에서 P1 아이템별 사용 방식 분기
- `PlayerItemEffectController`에서 제트팩, 망치, 복어 풍선옷, 비눗방울, 거대 풍선 등의 지속 효과 관리
- `ThrownItemEffect`에서 폭탄과 연막탄의 투척 및 충돌 처리
- `SmokeCloudEffect`에서 연막 범위와 화면 방해 처리
- `PlayerScreenObscureView`에서 먹물과 연막의 Canvas 화면 효과 표시
- `PlayerInputReader`에서 비눗방울 상태의 이동·달리기·앉기 입력 제한
- `PlayerPushController`에서 망치의 밀치기 배율과 사거리 적용
- `Day43P1ItemSetupTool`에서 데이터와 Scene 구성을 자동 연결

### 입력 규칙

| 입력 | 기능 |
|---|---|
| `Q` | 첫 번째 아이템 슬롯 선택 |
| `E` | 두 번째 아이템 슬롯 선택 |
| 마우스 우클릭 | 선택한 아이템 사용 |
| `Space` 유지 | 제트팩 효과 중 상승 |
| `A/D` 교대 6회 | 비눗방울 탈출 |

비눗방울 탈출 입력은 같은 방향을 연속으로 눌러도 횟수에 포함되지 않는다.

### 데이터 및 명칭 변경

`ITM-014`의 명칭과 내부 식별자를 다음과 같이 통일했다.

```text
ITM-014
PufferBalloonSuit
복어 풍선옷
```

데이터 에셋 파일도 `ITM-014_PufferBalloonSuit.asset`으로 변경했다.

### 추가 및 변경된 주요 파일

#### 신규 파일

- `Assets/_ProjectJ/Scripts/Editor/Day43P1ItemSetupTool.cs`
- `Assets/_ProjectJ/Scripts/Runtime/Items/P1ItemRules.cs`
- `Assets/_ProjectJ/Scripts/Runtime/Items/PlayerScreenObscureView.cs`
- `Assets/_ProjectJ/Scripts/Runtime/Items/SmokeCloudEffect.cs`
- `Assets/_ProjectJ/Scripts/Runtime/Items/ThrownItemEffect.cs`
- `Assets/_ProjectJ/Tests/EditMode/P1ItemRulesTests.cs`

#### 주요 변경 파일

- `Assets/_ProjectJ/Scripts/Runtime/Data/Definitions/ItemDataDefinition.cs`
- `Assets/_ProjectJ/Scripts/Runtime/Items/ItemProjectileEffect.cs`
- `Assets/_ProjectJ/Scripts/Runtime/Items/PlacedItemEffect.cs`
- `Assets/_ProjectJ/Scripts/Runtime/Items/PlayerItemEffectController.cs`
- `Assets/_ProjectJ/Scripts/Runtime/Items/PlayerItemUseController.cs`
- `Assets/_ProjectJ/Scripts/Runtime/Player/Input/PlayerInputReader.cs`
- `Assets/_ProjectJ/Scripts/Runtime/Player/Interaction/PlayerPushController.cs`
- `Assets/_ProjectJ/Scenes/Game/Game.unity`
- `Assets/_ProjectJ/Data/Definitions/Item/ITM-011`~`ITM-021` 데이터 에셋

### 테스트 항목

- P1 11종 아이템의 획득, 선택, 사용, 소비 확인
- 제트팩과 거대 풍선의 상승 종료 시점 확인
- 망치의 밀치기 힘과 사거리 강화 확인
- 폭탄의 지연 폭발과 범위 판정 확인
- 복어 풍선옷의 반복 밀치기 간격 확인
- 먹물과 연막의 화면 방해 표시 및 해제 확인
- 낚시대와 갈고리의 최대 거리 및 대상 판정 확인
- 비눗방울 상태에서 이동 입력 제한과 교대 입력 탈출 확인
- 트램폴린의 설치 검사, 사용자 판정, 3회 사용 제한 확인
- 부활 및 경기 종료 시 지속 효과와 화면 방해 제거 확인
- 기존 P0 아이템 10종의 기능 유지 확인
- EditMode의 `P1ItemRulesTests` 통과 확인

### 확인 필요 항목

저장소의 커밋 내용만으로 Unity Editor의 실제 실행 결과까지는 확인할 수 없으므로 다음 항목은 프로젝트에서 직접 검증해야 한다.

- Unity 전체 Assembly 컴파일 성공 여부
- EditMode Test Runner 통과 여부
- CharacterController 기반 상승과 갈고리 이동의 실제 체감
- 실제 Player 사이의 밀치기, 끌어당기기, 화면 방해 동작
- 트램폴린 접촉 판정과 사용 횟수 처리
- 부활 및 경기 종료 시 모든 P1 지속 효과 초기화

### 다음 개발 방향

44일차에는 되감기 시계, 유도탄, 소형화 물약, 드론, 투명 망토, 저격 물총, 카트로 구성된 P2 아이템 7종의 효과를 구현한다.

### 커밋 정보

```text
43일차 : P1 아이템 11종 효과 구현
```

- 기준 커밋: `e41ba267457f11eaa02f70791a8e12b4b6006986`
