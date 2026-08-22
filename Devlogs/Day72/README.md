# Project J - 72일차 개발 일지

## 개발 기준

71일차 완료 커밋:

```text
c1739a1b4345c0eba5c63e99cc11dd4ceb27484c
71일차 : Network Countdown·경기 타이머·FINISH 및 최종 순위 동기화 구현
```

72일차 최신 커밋:

```text
fffe7ffec12305aab0ccda6900c0f18d9ffe124c
72일차 : 동적 장애물·아이템 상자 및 2슬롯 인벤토리 네트워크 동기화 구현
```

이번 일차에서는 71일차까지 완성한 경기 시작·진행·종료 네트워크 흐름 위에 경기 중 상호작용하는 아이템 상자, 2슬롯 인벤토리, 동적 플랫폼, AirBag을 Fusion State Authority 기준으로 연결했다.

---

## 72일차 목표

경기 중 월드 오브젝트와 아이템 보유 상태가 Client별 로컬 상태로 분리되지 않도록 State Authority가 최종 결과를 확정한다.

핵심 목표:

```text
Q / E 아이템 슬롯 선택 네트워크 입력
2슬롯 Network Inventory
Item Box 단일 획득자 판정
Item ID 동기화
선택 슬롯 교체 규칙
Moving Platform 동기화
Rotating Platform 동기화
Ghost Platform 상태 동기화
AirBag External Force 연결
Player Prefab 네트워크 인벤토리 연결
Day49 통합 테스트 Scene 연결
```

---

## 1. Fusion 입력에 Q / E 슬롯 선택 추가

기존 `ProjectJNetworkInput`에 두 개의 Network Button을 추가했다.

```text
ItemSlotLeft
ItemSlotRight
```

입력 규칙:

```text
Q
→ 첫 번째 슬롯 선택

E
→ 두 번째 슬롯 선택
```

`ProjectJFusionInputProvider`는 Q/E 입력을 프레임에서 감지한 뒤 다음 Fusion Tick까지 단발 입력으로 보존하여 제출한다.

이를 통해 슬롯 선택도 이동·점프·밀치기와 동일하게 Fusion 입력 흐름을 사용한다.

---

## 2. ProjectJNetworkItemInventory 생성

새 네트워크 인벤토리:

```text
ProjectJNetworkItemInventory
```

Player별로 다음 값을 Networked 상태로 관리한다.

```text
Slot 1 Item ID
Slot 2 Item ID
Selected Slot Index
Inventory Revision
```

실제 `ItemDefinition` ScriptableObject 자체를 네트워크로 전송하지 않고 정수 Item ID만 동기화한다.

기존 아이템 ID:

```text
ITM-001
ITM-002
ITM-003
```

를 다음처럼 변환한다.

```text
ITM-001 → 1
ITM-002 → 2
ITM-003 → 3
```

빈 슬롯은 `0`을 사용한다.

---

## 3. 기존 2슬롯 저장 규칙 유지

기존 로컬 인벤토리의 저장 규칙을 네트워크 인벤토리에서도 유지했다.

```text
Slot 1 비어 있음
→ Slot 1 저장

Slot 1 사용 중
Slot 2 비어 있음
→ Slot 2 저장

두 슬롯 모두 사용 중
→ 현재 선택 슬롯 교체
```

모든 슬롯 변경은 State Authority에서만 확정한다.

---

## 4. 경기 상태와 인벤토리 입력 연결

`ProjectJNetworkItemInventory`는 기존 `ProjectJNetworkExternalGameplay.GameplayInputAllowed`를 확인한다.

따라서 다음 상태에서는 Q/E 슬롯 선택과 월드 아이템 획득을 차단한다.

```text
Preparing
Countdown
개인 FINISH 후
경기 Finished 후
```

실제 아이템 상호작용은 경기 중 `Playing` 상태에서만 허용된다.

---

## 5. Network Player Prefab 연결

`ProjectJNetworkPlayer.prefab`에 다음 컴포넌트를 추가했다.

```text
ProjectJNetworkItemInventory
```

Player Prefab의 NetworkBehaviour 구성은 다음 구조로 확장됐다.

```text
ProjectJNetworkPlayer
├─ NetworkObject
├─ NetworkTransform
├─ ProjectJNetworkPlayer
├─ ProjectJNetworkExternalGameplay
└─ ProjectJNetworkItemInventory
```

따라서 Host와 Client가 Spawn될 때 각 Player가 자체 네트워크 인벤토리를 가진다.

---

## 6. ProjectJNetworkItemBox 생성

기존 `ItemPickup` 데이터를 재사용하면서 네트워크 획득 판정을 담당하는:

```text
ProjectJNetworkItemBox
```

를 추가했다.

기존 `ItemPickup`은 ItemDefinition 데이터 보관용으로 남겨두고 로컬 즉시 획득 동작은 비활성화한다.

네트워크 획득 흐름:

```text
Player가 Item Box Trigger 접촉
↓
State Authority가 획득 후보 확인
↓
Network Inventory 저장 가능 여부 검사
↓
Item ID 저장
↓
Collector 확정
↓
Item Box 획득 상태 고정
↓
모든 Peer에서 Collider / Visual 비활성화
```

---

## 7. Item Box 단일 획득자 판정

같은 상자에 여러 Player가 접근하더라도 한 명에게만 아이템을 지급하도록 구성했다.

동일 시점 후보가 여러 명일 경우 현재 테스트 구현에서는 낮은 Player Index를 우선한다.

예:

```text
P0 접촉
P1 접촉
↓
State Authority 판정
↓
P0 승인
↓
NetworkCollected = true
↓
P1 추가 획득 차단
```

상자에는 다음 결과가 Networked 상태로 남는다.

```text
Collected
Collector Player Index
Awarded Item ID
Stored Slot Index
```

---

## 8. Item Box 표현 동기화

아이템 획득 후 GameObject를 각 Client가 개별 삭제하지 않는다.

대신 Networked 획득 상태를 기준으로:

```text
획득 전
→ Trigger ON
→ Renderer ON

획득 후
→ Trigger OFF
→ Renderer OFF
```

상태를 모든 Peer에서 동일하게 적용한다.

이를 통해 Host와 Client에서 같은 상자가 서로 다른 시점에 보이거나 중복 획득되는 문제를 방지한다.

---

## 9. ProjectJNetworkDynamicPlatform 생성

기존 플랫폼 시스템을 네트워크 환경에서 재사용하기 위해:

```text
ProjectJNetworkDynamicPlatform
```

을 추가했다.

지원 대상:

```text
MovingPlatform
RotatingPlatform
GhostPlatform
```

하나의 Bridge가 같은 GameObject에 어떤 기존 플랫폼 컴포넌트가 있는지 확인하여 필요한 동기화 방식을 적용한다.

---

## 10. Moving Platform State Authority 실행

기존 `MovingPlatform`은 Host와 Client에서 각각 독립적으로 `FixedUpdate`를 실행하지 않도록 변경했다.

네트워크 구조:

```text
State Authority
→ 기존 MovingPlatform 실행
→ 위치 계산
→ NetworkTransform

Proxy Client
→ 기존 MovingPlatform 비활성
→ NetworkTransform 결과 수신
```

따라서 플랫폼 위치 계산 기준을 Host 하나로 통일한다.

---

## 11. Rotating Platform State Authority 실행

회전 플랫폼도 Moving Platform과 같은 구조를 사용한다.

```text
State Authority
→ RotatingPlatform 회전 계산

Proxy
→ 로컬 회전 계산 중지

NetworkTransform
→ Host 회전 결과 동기화
```

이를 통해 Client별 회전 누적 오차를 줄인다.

---

## 12. Ghost Platform 상태 동기화

Ghost Platform은 위치 대신 상태 변화가 중요하므로 Networked 상태를 별도로 사용한다.

동기화 상태:

```text
Active
Warning
Hidden
```

추가로 Warning 단계의 Alpha 값도 동기화한다.

Host:

```text
GhostPlatform 시간 계산
↓
현재 State 저장
↓
현재 Alpha 저장
```

Client:

```text
Network State 수신
↓
Collider 상태 적용
Renderer 상태 적용
Alpha 적용
```

따라서 Ghost Platform이 모든 Peer에서 같은 시점에 사라지고 다시 나타나도록 구성했다.

---

## 13. AirBag Network Bridge 추가

기존 `AirBagObstacle`의 설정값은 그대로 사용하면서 Network Player에 외력을 전달하기 위한:

```text
ProjectJNetworkAirBagBridge
```

를 추가했다.

기존 AirBag에서 재사용하는 값:

```text
Horizontal Velocity Change
Local Push Direction
Contact Spread
```

네트워크 흐름:

```text
Network Player가 AirBag 접촉
↓
기존 AirBag 계산식으로 방향 계산
↓
ProjectJNetworkExternalGameplay
↓
ProjectJExternalForceSource.AirBag
↓
State Authority External Velocity 적용
```

기존 Player External Force 네트워크 구조와 연결되므로 별도의 AirBag 위치 상태는 동기화하지 않는다.

---

## 14. Day49 통합 테스트 Scene 자동 연결

기존 통합 테스트 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day49/Day49_AllSystemsTest.unity
```

에 72일차 네트워크 컴포넌트를 실제로 연결했다.

Scene의 기존 대상 전체를 기준으로:

```text
ItemPickup
→ ProjectJNetworkItemBox

MovingPlatform
RotatingPlatform
GhostPlatform
→ ProjectJNetworkDynamicPlatform

AirBagObstacle
→ ProjectJNetworkAirBagBridge
```

구조를 적용했다.

필요한 NetworkObject, NetworkTransform, Rigidbody 등은 각 네트워크 컴포넌트의 RequireComponent 구성에 따라 함께 추가됐다.

---

## 15. 72일차 테스트용 Network Inventory Debug

로컬 Input Authority Player에서는 개발용 인벤토리 상태를 확인할 수 있다.

표시 정보:

```text
DAY 72 NETWORK INVENTORY

Slot 1 [Q]
Slot 2 [E]
Selected
Revision
```

예:

```text
Slot 1 [Q] : ITM-001
Slot 2 [E] : EMPTY
Selected : 1
Revision : 1
```

이를 통해 Host와 Client에서 슬롯 상태가 동일한지 확인할 수 있다.

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Input/
├─ ProjectJFusionInputProvider.cs
└─ ProjectJNetworkInput.cs

Assets/ProjectJ/Network/Fusion/Player/Resources/
└─ ProjectJNetworkPlayer.prefab

Assets/ProjectJ/Tests/Manual/Day49/
└─ Day49_AllSystemsTest.unity
```

---

## 생성 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkItemInventory.cs
└─ ProjectJNetworkItemInventory.cs.meta

Assets/ProjectJ/Network/Fusion/World/
├─ ProjectJNetworkItemBox.cs
├─ ProjectJNetworkItemBox.cs.meta
├─ ProjectJNetworkDynamicPlatform.cs
├─ ProjectJNetworkDynamicPlatform.cs.meta
├─ ProjectJNetworkAirBagBridge.cs
└─ ProjectJNetworkAirBagBridge.cs.meta

Assets/ProjectJ/Network/Fusion/
└─ World.meta
```

자동 설정 과정에서 생성된 `Assets/ProjectJ/Editor/Day72.meta`가 저장소에 남아 있으나 런타임 기능에는 관여하지 않는다.

---

## 삭제 파일

```text
없음
```

일회성 Editor 자동 설정 스크립트는 적용 완료 후 제거되어 최종 커밋에는 포함되지 않았다.

---

## 테스트 항목

```text
Unity Console
→ Error 0

Host + Client 접속
↓
Countdown 종료
↓
Playing

Q 입력
→ Slot 1 선택

E 입력
→ Slot 2 선택

Host / Client
→ 선택 슬롯 상태 동일

Item Box 접촉
→ 한 Player만 획득

동일 Item Box 동시 접촉
→ 한 명만 승인
→ 중복 획득 없음

Item 획득
→ Network Inventory 저장

Slot 1 비어 있음
→ Slot 1 저장

Slot 1 사용 중
Slot 2 비어 있음
→ Slot 2 저장

두 슬롯 가득 참
→ 현재 선택 슬롯 교체

Item Box 획득 후
→ Host에서 숨김
→ Client에서도 숨김

Moving Platform
→ Host 계산
→ Client 위치 동기화

Rotating Platform
→ Host 계산
→ Client 회전 동기화

Ghost Platform
→ Active / Warning / Hidden 동기화
→ Collider 상태 일치
→ Alpha 변화 일치

AirBag
→ Network Player 외력 적용
→ Host / Client 이동 결과 일치

기존 기능
→ 이동
→ Jump
→ Sprint
→ Stamina
→ Crouch
→ Push
→ Checkpoint
→ Respawn
→ 보호 시간
→ Height / Rank
→ Countdown
→ Match Timer
→ FINISH
정상 유지
```

---

## 코드 및 연결 검토 결과

GitHub 최신 커밋 기준으로 다음 항목을 확인했다.

```text
Q / E Fusion Input
→ 구현됨

2슬롯 Network Inventory
→ 구현됨

Player Prefab Network Inventory 연결
→ 반영됨

Item Box State Authority 획득
→ 구현됨

Item Box Scene 연결
→ 반영됨

Moving / Rotating Platform Network Bridge
→ 구현됨

Ghost Platform 상태 동기화
→ 구현됨

Dynamic Platform Scene 연결
→ 반영됨

AirBag Network External Force Bridge
→ 구현됨

AirBag Scene 연결
→ 반영됨
```

GitHub 저장소에는 자동 Unity 빌드 CI가 없어 컴파일 및 실제 Host/Client 런타임 성공 여부는 GitHub만으로 확정할 수 없다.

최종 완료 기준:

```text
Unity Console Error 0
+
Host / Client 2인 테스트 통과
```

---

## 72일차 완료 구조

```text
Fusion Input
├─ 이동 / 점프 / Sprint / Crouch / Push
└─ Q / E Item Slot
        ↓
Network Player
├─ ProjectJNetworkPlayer
├─ ProjectJNetworkExternalGameplay
└─ ProjectJNetworkItemInventory
        ↓
State Authority
├─ 2슬롯 Inventory
├─ Selected Slot
└─ Item Box 획득 판정
        ↓
Network World
├─ Item Box
├─ Moving Platform
├─ Rotating Platform
├─ Ghost Platform
└─ AirBag
        ↓
Host / Client 동일 경기 상태
```

---

## 다음 개발 방향

73일차에서는 72일차에서 동기화한 아이템 보유 상태 위에 실제 아이템 사용 효과를 네트워크화한다.

```text
선택 Item
↓
우클릭 사용 요청
↓
State Authority 검증
↓
대상 결정
↓
아이템 소비
↓
Network Effect 적용
↓
대표 아이템 5종 테스트
```

72일차까지는 아이템을 누가 획득했고 어느 슬롯에 보관했는지를 네트워크에서 확정하는 기반을 완성하고, 73일차부터 실제 아이템 효과를 경기 결과에 반영하는 단계로 진행한다.
