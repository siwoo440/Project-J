# 프로젝트 J 개발 일지

---

## 23일차 : 끝자락 올라오기 및 외부 힘 통합

### 개발 목표

22일차에 구현한 끝자락 감지 결과를 실제 올라오기 동작으로 연결하고, 밀치기·이동 발판·장애물에서 발생하는 힘을 하나의 외부 힘 시스템으로 통합했다.

### 구현 내용

- 공중에서 끝자락을 향해 이동할 때 올라오기 시작
- 끝자락 높이, 접근 방향, 자세 및 착지 공간 검사
- `Lifting → Advancing → Idle` 순서의 2단계 올라오기 처리
- 올라오기 중 일반 이동, 점프 및 중력 일시 제한
- 외부 힘 또는 천장 충돌 발생 시 올라오기 취소
- 밀치기·이동 발판·장애물의 외부 힘 요청 구조 통합
- 수평·수직 외부 속도 적용 및 감속 처리
- 이동 발판 속도의 짧은 전달 유예 처리
- 부활 시 올라오기와 외부 힘 상태 초기화
- Scene 뷰에서 올라오기 경로 확인용 기즈모 추가

### 올라오기 진행 구조

```text
끝자락 감지
→ 높이와 접근 방향 확인
→ 최종 착지 공간 확인
→ 몸을 끝자락 위로 올리기
→ 발판 안쪽으로 이동
→ 일반 이동과 중력 복원
```

### 수정 및 생성 파일

| 구분 | 파일 | 역할 |
|---|---|---|
| 수정 | `PlayerMovementController.cs` | 올라오기와 외부 힘 이동 통합 |
| 생성 | `PlayerLedgeClimbController.cs` | 2단계 올라오기 상태와 경로 관리 |
| 생성 | `ExternalForceRequest.cs` | 외부 힘 원인과 결합 방식 정의 |
| 수정 | `ExternalForceReceiver.cs` | 통합 외부 힘 요청 수신 |
| 수정 | `PlayerExternalForceController.cs` | 밀치기·발판·장애물 힘 관리 |
| 수정 | `PlayerPushController.cs` | 통합 밀치기 요청 사용 |
| 생성 | `PlayerLedgeClimbControllerTests.cs` | 올라오기 계산 자동 테스트 |
| 생성 | `ExternalForceRequestTests.cs` | 외부 힘 요청 자동 테스트 |

### 주요 설정값

| 항목 | 값 |
|---|---:|
| Minimum Ledge Height | `0.35` |
| Maximum Ledge Height | `2.2` |
| Minimum Ledge Approach Dot | `0.5` |
| Ledge Wall Gap | `0.05` |
| Ledge Landing Depth | `0.55` |
| Ledge Foot Clearance | `0.05` |
| Ledge Lift Duration | `0.3초` |
| Ledge Forward Duration | `0.2초` |
| Hit Immunity Duration | `0.8초` |
| Impulse Deceleration | `8` |
| Platform Velocity Grace Time | `0.1초` |

### 외부 힘 처리 기준

| 원인 | 결합 방식 | 처리 내용 |
|---|---|---|
| 밀치기 | 기존 순간 힘 교체 | 수평 힘과 연속 피격 면역 적용 |
| 이동 발판 | 전달 속도 갱신 | 수평·수직 이동 속도 유지 |
| 장애물 | 기존 순간 힘에 누적 | 점프대 등의 수직 힘 적용 가능 |

### 자동 테스트

새로운 EditMode 테스트 13개를 추가했다.

- `PlayerLedgeClimbControllerTests` 9개
- `ExternalForceRequestTests` 4개

검증 항목은 끝자락 높이와 접근 방향 판정, 올라오기 목표 위치 생성, 단계별 이동, 취소 처리, 외부 힘별 속도와 결합 방식이다.

### 수동 확인 항목

- 허용 높이의 끝자락에서 몸 올리기와 발판 진입
- 너무 낮거나 높은 끝자락에서 동작 제한
- 벽 반대 방향 입력과 앉은 상태에서 동작 제한
- 착지 공간 또는 머리 위가 막힌 경우 동작 제한
- 올라오기 중 밀치기·장애물 힘·천장 충돌에 따른 취소
- 기존 WASD 이동, 달리기, 앉기, 점프 및 공중 제어
- 경사·계단·모서리 이동과 기존 밀치기
- 체크포인트, 부활 및 경기 종료 후 조작 차단
- Windows 개발 빌드 실행

### 완료 결과

- 끝자락 감지 정보를 실제 올라오기 동작으로 연결
- 몸 올리기와 발판 진입을 분리한 상태 기반 처리 완성
- 잘못된 위치에서 올라오지 않도록 높이·방향·공간 검사 적용
- 올라오기 중 충돌과 외부 힘에 대응하는 취소 처리 적용
- 밀치기·발판·장애물이 공통으로 사용하는 외부 힘 구조 완성
- 부활 시 이동 관련 임시 상태가 남지 않도록 초기화 적용
- 새로운 EditMode 테스트 13개 추가

실제 Unity 컴파일, Play Mode 테스트와 Windows 개발 빌드 결과는 로컬 Unity 에디터에서 최종 확인했다.

### 커밋 제목

```text
23일차 : 끝자락 올라오기 및 밀치기·발판·장애물 외부 힘 통합
```
