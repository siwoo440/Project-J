# 프로젝트 J 24일차 개발 일지

## 개발 목표

3인칭 카메라의 벽 관통을 방지하고 달리기 상태에 따라 시야각을 전환한다. 밀치기 가능한 대상을 실시간으로 판정하여 외곽선으로 표시한다.

---

## 구현 내용

### 1. 3인칭 카메라 회전

- 마우스와 게임패드 시점 회전 처리
- 상하 회전 각도 제한
- 설정 서비스의 감도 및 Y축 반전 값 유지
- 현재 Yaw·Pitch·카메라 거리 값 공개

### 2. 카메라 벽 충돌

- 피벗과 카메라 사이의 구체 충돌 검사
- 플레이어 자신의 충돌체와 Trigger 제외
- 벽 접근 시 카메라 거리 즉시 축소
- 벽 이탈 시 기존 거리까지 부드럽게 복구
- 최소 카메라 거리와 벽 여유 거리 적용

### 3. 달리기 FOV 전환

- 일반 이동 시 기본 FOV `60` 적용
- 달리기 중 FOV `68` 적용
- 달리기 시작과 종료 시 시야각 부드러운 전환
- 앉기·스태미나 부족·이동 입력 해제 시 기본 FOV 복귀

### 4. 밀치기 대상 탐색

- 플레이어 전방의 밀치기 가능 대상 실시간 검색
- 여러 대상 중 가장 가까운 대상 우선 선택
- 플레이어 자신의 충돌체 제외
- 벽이나 장애물 뒤에 있는 대상 선택 차단
- 현재 대상과 거리 및 대상 존재 여부 공개

### 5. 대상 외곽선 표시

- 선택된 대상의 Renderer 경계를 기준으로 외곽선 표시
- Renderer가 없는 대상은 Collider 경계 사용
- 밀치기 가능 상태는 청록색으로 표시
- 밀치기 재사용 대기 상태는 회색으로 표시
- 대상이 없거나 벽에 가려지면 외곽선 숨김

### 6. 카메라 계산 테스트

- 충돌 지점에서 여유 거리 차감 검사
- 최소·최대 카메라 거리 제한 검사
- 벽 접근 시 즉시 거리 축소 검사
- 벽 이탈 시 거리 복구 및 초과 방지 검사
- 일반 이동과 달리기 FOV 선택 검사
- FOV 전환 및 목표값 초과 방지 검사

---

## 수정 및 생성 파일

| 구분 | 파일 | 역할 |
|---|---|---|
| 수정 | `ThirdPersonCameraController.cs` | 카메라 회전·벽 충돌·FOV 통합 |
| 생성 | `ThirdPersonCameraCollisionProbe.cs` | 카메라 구체 충돌 검사 |
| 생성 | `ThirdPersonCameraMath.cs` | 거리와 FOV 계산 |
| 수정 | `PlayerPushController.cs` | 현재 밀치기 대상 탐색 및 공개 |
| 생성 | `PushTargetOutlineController.cs` | 대상 경계 외곽선 표시 |
| 생성 | `ThirdPersonCameraMathTests.cs` | 카메라 계산 EditMode 테스트 |

---

## 주요 설정값

### 카메라

| 항목 | 값 |
|---|---:|
| Distance | `5` |
| Gamepad Degrees Per Second | `180` |
| Minimum Pitch | `-75` |
| Maximum Pitch | `80` |
| Starting Pitch | `15` |
| Camera Collision Radius | `0.25` |
| Camera Collision Padding | `0.08` |
| Minimum Camera Distance | `0.35` |
| Distance Recovery Speed | `8` |

### FOV

| 항목 | 값 |
|---|---:|
| Normal Field Of View | `60` |
| Sprint Field Of View | `68` |
| Field Of View Blend Speed | `24` |

### 대상 외곽선

| 항목 | 값 |
|---|---:|
| Ready Color | 청록색 |
| Cooldown Color | 회색 |
| Line Width | `0.035` |
| Bounds Padding | `0.08` |

---

## 테스트 항목

### EditMode 테스트

- `ThirdPersonCameraMathTests` 9개
- `PlayerLedgeClimbControllerTests` 9개
- `ExternalForceRequestTests` 4개
- `PlayerTraversalMathTests` 8개
- 기존 이동 관련 테스트

### 수동 테스트

- 마우스 및 게임패드 카메라 회전
- 수직 회전 각도 제한
- 벽 접근 시 카메라 즉시 축소
- 벽 이탈 시 카메라 거리 복구
- 플레이어 자신의 충돌체 제외
- 달리기 시작 및 종료 시 FOV 전환
- 가장 가까운 밀치기 대상 선택
- 벽 뒤 대상 선택 차단
- 대상 상태별 외곽선 색상 변경
- 기존 이동·점프·앉기·밀치기 기능 회귀 확인
- Windows 개발 빌드 실행 확인

---

## 완료 결과

- 3인칭 카메라 회전과 수직 각도 제한 적용
- 카메라 벽 관통 방지 적용
- 벽 이탈 후 카메라 거리 복구 적용
- 달리기 상태 기반 FOV 전환 적용
- 밀치기 가능 대상 실시간 탐색 적용
- 가장 가까운 유효 대상 우선 선택 적용
- 장애물 뒤 대상 선택 차단 적용
- 대상 경계 외곽선과 상태 색상 적용
- 카메라 계산 EditMode 테스트 추가
- 기존 플레이어 이동 시스템과 연동 완료

---

## 커밋 제목

```text
24일차 : 3인칭 카메라 충돌·달리기 FOV 및 밀치기 대상 외곽선
```
