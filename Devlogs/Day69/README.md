# Project J - 69일차 개발 일지

## 개발 기준

68일차 완료 커밋:

```text
5f1f003be84825cc135197763142478aaee76e58
68일차 : Local Player Camera·AudioListener 및 UI 소유권 분리 구현
```

69일차 최신 커밋:

```text
d88097d476c6cca4fb3f09f719f5f075de25ec6f
69일차 : External Force·State Authority Push 및 Checkpoint 네트워크화 구현
```

이번 일차에서는 기존 Fusion Network Player에 다음 세 기능을 연결했다.

```text
External Force 네트워크 상태
+
State Authority 기반 Player Push
+
State Authority 기반 Checkpoint 저장
```

---

## 69일차 목표

플레이어 자신의 이동 입력뿐 아니라 다른 플레이어와 게임 시스템이 플레이어에게 가하는 외력과 체크포인트 진행 상태도 Host의 State Authority가 확정하도록 구성한다.

전체 흐름:

```text
Local Input
↓
Fusion Input 전송
↓
State Authority
↓
Push Target 판정
↓
External Velocity 적용
↓
NetworkTransform으로 결과 동기화

Checkpoint Trigger
↓
ICheckpointReceiver
↓
State Authority
↓
최고 Checkpoint / Respawn 정보 저장
```

---

## 1. Networked External Force 구현

새로운 네트워크 외력 처리 컴포넌트를 추가했다.

```text
ProjectJNetworkExternalGameplay
```

주요 Networked 상태:

```text
NetworkExternalVelocity
NetworkLastExternalForceSource
NetworkExternalForceApplyCount
```

외력 원인은 다음 Enum으로 구분한다.

```text
None
Push
AirBag
Item
Obstacle
```

현재 69일차에서는 Player Push를 실제 연결하고, 이후 장애물과 아이템에서도 동일한 외력 구조를 재사용할 수 있도록 기반을 준비했다.

---

## 2. Fusion Tick 기반 External Velocity 처리

외부 속도는 `FixedUpdateNetwork()`에서 State Authority가 처리한다.

기본값:

```text
External Velocity Decay
12 / sec

Stop Threshold
0.05
```

처리 흐름:

```text
External Velocity
×
Runner.DeltaTime
↓
Player Position 반영
↓
Vector3.MoveTowards()
↓
매 Tick 감속
```

현재 외력은 X/Z 수평 방향을 기준으로 처리한다.

---

## 3. 공통 External Force API 구현

State Authority가 외력을 적용할 수 있도록 다음 API를 추가했다.

```text
TryApplyExternalVelocityChange(
    ProjectJExternalForceSource source,
    Vector3 velocityChange
)
```

처리 기준:

```text
State Authority만 실행
↓
Y 외력 제거
↓
기존 External Velocity와 합산
↓
외력 Source 저장
↓
적용 Count 증가
```

이 API를 Player Push뿐 아니라 이후 AirBag, Item, Obstacle에도 사용할 수 있다.

---

## 4. Fusion Push Input 추가

기존 네트워크 입력:

```text
Jump
Sprint
Crouch
```

에 다음 입력을 추가했다.

```text
Push
```

실제 조작:

```text
마우스 좌클릭
```

69일차 테스트 입력:

```text
G
```

Push는 Hold 방식이 아니라 눌린 순간을 다음 Fusion Tick까지 보존하는 `pendingPush` 방식으로 처리한다.

---

## 5. State Authority 기반 Player Push

Client는 Push 입력만 전송한다.

실제 대상 선정과 성공 여부는 State Authority가 결정한다.

```text
Push Input
↓
State Authority
↓
Target 탐색
↓
거리 확인
↓
전방 각도 확인
↓
가장 가까운 Player 1명 선택
↓
External Velocity 적용
```

Client가 직접 다른 Player의 위치나 Push 성공 여부를 확정하지 않는다.

---

## 6. Push Target 규칙

기본값:

```text
Range
2.5m

Angle
90°

Cooldown
1.5초

Force
12
```

전방 범위는 기준 방향에서 좌우 45도씩 총 90도로 판정한다.

대상 후보에서 다음 Player는 제외한다.

```text
자기 자신
Invalid NetworkObject
State Authority가 없는 대상
2.5m 범위 밖
90도 전방 범위 밖
```

여러 Player가 조건을 만족하면 가장 가까운 Player 한 명만 선택한다.

---

## 7. Push 기준 방향

현재 Player의 별도 회전 네트워크 시스템을 Push 판정에 의존하지 않도록 마지막 이동 입력 방향을 저장한다.

```text
NetworkPushForward
```

예:

```text
W
→ +Z

D
→ +X

W + D
→ 대각선
```

이동 입력이 없는 동안에는 마지막 방향을 유지한다.

초기 방향:

```text
Vector3.forward
```

---

## 8. Push Cooldown 및 결과 상태

Fusion `TickTimer`로 Push 쿨다운을 관리한다.

```text
NetworkPushCooldown
```

Push 시도 결과는 다음 상태로 저장한다.

```text
None
Success
Miss
Cooldown
Invalid
```

추가 Networked 상태:

```text
NetworkLastPushResult
NetworkLastPushTargetIndex
NetworkPushAttemptCount
NetworkPushSuccessCount
```

Target이 없더라도 Push를 시도한 시점부터 1.5초 쿨다운이 시작된다.

---

## 9. Push와 External Force 연결

유효한 Target이 선택되면 별도 Push 이동 코드를 만들지 않고 공통 외력 API를 사용한다.

```text
Source Player
↓
Target 방향 계산
↓
Y = 0
↓
Normalize
↓
PushForce 12
↓
TryApplyExternalVelocityChange()
```

따라서 Push 판정과 실제 밀림 이동을 서로 분리했다.

---

## 10. Checkpoint Receiver 공통 인터페이스 추가

기존 `Checkpoint.cs`가 Fusion 구현을 직접 참조하지 않도록 다음 공통 인터페이스를 추가했다.

```text
ICheckpointReceiver
```

구조:

```text
Checkpoint
↓
ICheckpointReceiver
↓
ProjectJNetworkExternalGameplay
```

Fusion Network Player에서는 `ProjectJNetworkExternalGameplay`가 해당 인터페이스를 구현한다.

기존 오프라인 `PlayerCheckpointTracker`도 유지하여 기존 로컬 시스템과의 호환성을 남겼다.

---

## 11. Checkpoint 네트워크 상태 저장

State Authority가 다음 정보를 Networked 상태로 저장한다.

```text
NetworkCheckpointId
NetworkRespawnPosition
NetworkRespawnEulerAngles
NetworkCheckpointActivationCount
```

Spawn 초기 상태:

```text
Checkpoint
Start

Respawn Position
Network Player Spawn 위치

Respawn Rotation
Network Player Spawn 회전
```

실제 부활 실행은 70일차에서 구현한다.

---

## 12. 가장 높은 Checkpoint만 유지

현재 저장된 Checkpoint보다 높은 ID만 인정한다.

예:

```text
Start → CP1
저장

CP1 → CP2
저장

CP2 → CP2
무시

CP2 → CP1
무시

CP2 → CP4
저장
```

따라서 플레이어가 이전 구간으로 내려가더라도 최고 Checkpoint와 Respawn 정보는 낮아지지 않는다.

---

## 13. Checkpoint 타입 충돌 수정

69일차 구현 과정에서 다음 컴파일 오류가 발생했다.

```text
CS0118
'Checkpoint' is a namespace but is used like a type

CS0535
ICheckpointReceiver.ReceiveCheckpoint(Checkpoint) 미구현
```

원인은 `ProjectJ.Checkpoint` 네임스페이스와 `Checkpoint` 클래스의 이름이 동일하여 타입 해석이 충돌한 것이었다.

다음과 같이 완전한 타입명을 사용하도록 수정했다.

```text
global::ProjectJ.Checkpoint.Checkpoint
```

적용 위치:

```text
ICheckpointReceiver.cs
ProjectJNetworkExternalGameplay.cs
```

이를 통해 인터페이스와 구현체의 메서드 시그니처를 동일하게 맞췄다.

---

## 14. Network Player Prefab 연결

`ProjectJNetworkPlayer.prefab` 루트에 다음 컴포넌트를 추가했다.

```text
ProjectJNetworkExternalGameplay
```

또한 Fusion `NetworkObject`의 `NetworkedBehaviours`에도 등록되어 Networked 프로퍼티가 Fusion 상태에 포함되도록 구성했다.

현재 Network Player의 주요 네트워크 구성:

```text
NetworkObject
NetworkTransform
ProjectJNetworkPlayer
ProjectJNetworkExternalGameplay
CapsuleCollider
```

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Input/
├─ ProjectJFusionInputProvider.cs
└─ ProjectJNetworkInput.cs

Assets/ProjectJ/Network/Fusion/Player/Resources/
└─ ProjectJNetworkPlayer.prefab

Assets/ProjectJ/Runtime/Checkpoint/
└─ Checkpoint.cs
```

---

## 생성 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJExternalForceSource.cs
└─ ProjectJNetworkExternalGameplay.cs

Assets/ProjectJ/Runtime/Checkpoint/
└─ ICheckpointReceiver.cs
```

각 신규 C# 파일의 Unity `.meta` 파일도 함께 추가했다.

---

## 삭제 파일

```text
없음
```

---

## 테스트 항목

69일차 기능 확인 항목:

```text
Host + Client 접속

마우스 좌클릭 Push 입력
G 테스트 Push 입력

2.5m Push 범위
90도 전방 판정
가장 가까운 Player 1명 선택
1.5초 Push Cooldown
Push Force 적용
External Velocity 감속

Checkpoint 접촉
Start → CP1 → CP2 진행
동일 Checkpoint 재진입 무시
낮은 Checkpoint 재진입 무시
Respawn Position 저장
Respawn Rotation 저장

Host / Client 위치 동기화
Remote Interpolation 유지
Local Camera 유지
Console Error 0
```

---

## 69일차 완료 내용

```text
Fusion Push Input
↓
State Authority Target 판정
↓
가장 가까운 Player 선정
↓
External Velocity 적용
↓
Tick 기반 External Velocity 감속
↓
Networked Push 결과 및 Cooldown
↓
Checkpoint 공통 Receiver
↓
State Authority Checkpoint 갱신
↓
최고 Checkpoint 유지
↓
Respawn Position / Rotation 저장
↓
Network Player Prefab 등록
↓
Checkpoint Namespace / Type 충돌 수정
```

---

## 다음 개발 방향

70일차에서는 69일차에 저장한 Checkpoint와 External Force 상태를 기반으로 실제 부활 시스템과 3초 보호, 높이 및 순위 동기화를 구현한다.

예상 흐름:

```text
낙하 또는 직접 부활 요청
↓
State Authority 판정
↓
최고 Checkpoint Respawn 위치로 이동
↓
수직 속도 초기화
↓
External Velocity 초기화
↓
3초 Respawn Protection 시작
↓
적대적 Push / Item Force 차단
↓
장애물 Force는 유지
↓
현재 발 높이 0.00 단위 계산
↓
State Authority 순위 계산 및 동기화
```
