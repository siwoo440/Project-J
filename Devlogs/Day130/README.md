# Project J - 130일차 개발일지

## 작업 개요

130일차에는 **저격 물총(Sniper Water Gun)** 을 네트워크 아이템 시스템에 추가했다.

기존 일반 물총과 달리 즉시 효과를 적용하지 않고, 우클릭으로 조준을 시작한 뒤 일정 시간 조준을 유지하면 서버가 장거리 히트스캔 판정을 수행하도록 구성했다.

핵심 목표는 다음과 같다.

- 저격 물총 Network ID 등록
- 0.8초 조준 준비 시간
- 최대 50m 서버 권한 히트스캔
- 적중 시 수평 외부 속도 12m/s 적용
- 조준 중 2배 / 4배 확대 전환
- 조준 취소 시 아이템 유지
- 실제 발사 시 아이템 1회 소비
- 젤리 보호막 및 부활 보호 규칙 재사용
- 투명 망토 사용 해제 규칙과 연동
- Scene NetworkObject 및 개별 Pickup 추가는 보류

---

## 1. 저격 물총 네트워크 ID 등록

`ProjectJNetworkItemCatalog`에 저격 물총을 추가했다.

- Network ID: `29`
- Item Key: `sniper_water_gun`
- 표시 이름: `저격 물총`

기존 `Item_SniperWaterGun.asset`을 재사용하고 새로운 ItemDefinition은 생성하지 않았다.

---

## 2. 네트워크 입력에 조준 방향 추가

기존 `ProjectJNetworkInput`에 다음 입력 데이터를 추가했다.

- `AimDirection`

로컬 입력 공급자는 현재 로컬 Gameplay Camera의 정면 방향을 읽어 Fusion 입력에 포함한다.

Client가 적중 결과나 Target을 직접 전송하지 않고 **조준 방향만 서버에 전달**한다.

최종 적중 판정은 State Authority가 직접 수행한다.

---

## 3. 조준 시작과 준비 시간

저격 물총이 선택된 상태에서 아이템 사용 입력을 시작하면 서버가 조준 상태를 활성화한다.

조준 시작 시 다음 정보를 저장한다.

- 조준 시작 슬롯
- 조준 시작 시점의 Respawn Count
- 0.8초 준비 TickTimer
- 조준 상태 Revision

조준 준비 시간:

`0.8초`

조준 중 우클릭을 유지해야 한다.

다음 상황에서는 발사 전에 조준이 취소된다.

- 우클릭 해제
- 다른 슬롯으로 변경
- 해당 슬롯의 저격 물총이 사라짐
- 부활 발생
- 경기 입력 잠금
- 경기 종료 또는 개인 결과 잠금에 따른 Gameplay 비활성

발사 전에 취소된 경우 저격 물총은 소비하지 않는다.

---

## 4. 서버 권한 히트스캔

0.8초 조준 준비가 끝난 상태에서 사용 입력을 계속 유지하면 State Authority가 발사를 확정한다.

발사 판정:

- 최대 사거리: `50m`
- 방식: `Physics.RaycastNonAlloc`
- 기준 방향: Fusion Input으로 전달된 카메라 AimDirection
- 자기 자신의 Collider는 판정에서 제외
- 가장 가까운 유효 충돌을 사용
- 구조물이 먼저 맞으면 뒤 Player를 관통하지 않음

Projectile NetworkObject를 새로 만들지 않고 즉시 판정하는 Hitscan 방식으로 구현했다.

---

## 5. 적중 효과

Player가 첫 충돌 대상이면 저격 물총의 외부 속도를 적용한다.

외부 속도:

`12m/s`

수직 성분은 제거하고 조준 방향의 수평 방향을 기준으로 적용한다.

기존 공통 외력 API를 사용하기 때문에 다음 방어 규칙을 그대로 따른다.

- 젤리 보호막
- 부활 보호
- 경기 종료 및 완주 후 외력 차단
- 되감기 중 외부 방해 차단

보호 상태로 인해 외력이 적용되지 않아도 이미 발사된 저격 물총은 소비된다.

---

## 6. 조준 Zoom

로컬 Input Authority Player만 저격 조준 화면을 적용한다.

조준 시작 시 기본 확대:

`2x`

마우스 휠 입력 시:

`2x ↔ 4x`

조준 중에는 기존 3인칭 카메라의 휠 거리 조절을 일시적으로 막고 휠을 저격 확대 변경에 사용한다.

조준이 끝나면 기존 카메라 FOV와 휠 거리 조절로 복귀한다.

현재 개발용 조준 표시로 화면 중앙 Crosshair와 다음 정보를 출력한다.

- 조준 준비 진행률
- 현재 Zoom 배율

정식 UI 및 아트 교체는 후속 UI 작업에서 진행할 수 있다.

---

## 7. 인벤토리 소비 규칙

저격 물총은 일반 즉시 소비 아이템과 다르게 조준 시작 시에는 슬롯에서 제거하지 않는다.

### 조준 취소

- 아이템 유지
- 사용 성공 횟수 증가 없음

### 실제 발사

- 현재 저격 물총 슬롯 비우기
- Inventory Revision 증가
- Item Use Success Count 증가
- Last Used Item ID 갱신
- 저격 발사 횟수 증가

명중 여부와 관계없이 실제 사격이 발생하면 1회 소비한다.

---

## 8. 투명 망토 연동

실제 저격 물총 발사는 공격 아이템 사용으로 취급한다.

따라서 투명 망토가 활성화되어 있다면 사격 확정 시 은신을 해제한다.

조준을 시작했다가 발사 전에 취소한 경우에는 공격이 발생하지 않았으므로 이 단계에서 별도 소비 처리는 발생하지 않는다.

---

## 9. Respawn 및 상태 초기화

부활 또는 인벤토리 전체 초기화 시 저격 조준 상태를 제거한다.

부활 전 생명의 조준 상태가 다음 생명으로 이어지지 않도록 조준 시작 당시 Respawn Count도 함께 검증한다.

---

## 10. EditMode 정책 테스트

다음 파일을 추가했다.

`Assets/ProjectJ/Tests/EditMode/ProjectJSniperWaterGunPolicyTests.cs`

작성된 테스트 사례는 총 **41개**이다.

검증 대상으로 작성한 항목:

- Network ID 29
- 준비 시간 0.8초
- 사거리 50m
- 외부 속도 12m/s
- 2x / 4x 확대 상수
- 조준 시작 조건
- 조준 취소 조건
- 50m 경계값
- 조준 준비 진행률
- AimDirection 정규화
- 잘못된 방향의 Fallback
- 수평 외부 속도 계산
- FOV 확대 계산
- 2x / 4x 전환
- 마우스 휠 Dead Zone

이 개발일지 작성 시점에는 GitHub Actions 또는 자동 Unity Test Runner 결과가 연결되어 있지 않다.

따라서 **41개 테스트가 작성되어 있다는 사실과 실제 Unity에서 41/41 통과했다는 것은 구분한다.**

---

## 11. 변경 파일

129일차 기준 커밋과 비교하여 130일차 커밋에는 다음 13개 파일 변경이 포함된다.

### 수정

- `Assets/ProjectJ/Network/Fusion/Input/ProjectJFusionInputProvider.cs`
- `Assets/ProjectJ/Network/Fusion/Input/ProjectJNetworkInput.cs`
- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkItemCatalog.cs`
- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkItemInventory.cs`
- `Assets/ProjectJ/Network/Fusion/Presentation/ProjectJLocalPlayerPresentationController.cs`

### 생성

- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkItemInventory.SniperWaterGun.cs`
- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkItemInventory.SniperWaterGun.cs.meta`
- `Assets/ProjectJ/Network/Fusion/Player/ProjectJSniperWaterGunLocalPresentation.cs`
- `Assets/ProjectJ/Network/Fusion/Player/ProjectJSniperWaterGunLocalPresentation.cs.meta`
- `Assets/ProjectJ/Runtime/Items/ProjectJSniperWaterGunPolicy.cs`
- `Assets/ProjectJ/Runtime/Items/ProjectJSniperWaterGunPolicy.cs.meta`
- `Assets/ProjectJ/Tests/EditMode/ProjectJSniperWaterGunPolicyTests.cs`
- `Assets/ProjectJ/Tests/EditMode/ProjectJSniperWaterGunPolicyTests.cs.meta`

1회용 Editor Installer는 기존 파일 패치를 끝낸 뒤 자동 삭제되었기 때문에 최종 커밋에는 포함되지 않았다.

---

## 12. Scene 및 Pickup

이번 일차에는 Scene에 저격 물총 Pickup을 개별 배치하지 않았다.

아이템 구현 단계 동안 발생할 수 있는 Fusion Scene NetworkObject Bake / SortKey 문제를 피하기 위해 Pickup 통합은 아이템 구현 단계 종료 후 일괄 처리한다.

추가 Network Prefab이나 Scene NetworkObject도 필요하지 않다.

---

## 13. 최신 커밋 확인

개발일지 작성 시점의 `main` 최신 커밋:

- SHA: `a095ffaa2253bde0f0b22e92d9d7901f6a904a0d`
- 현재 커밋 메시지: `a`
- 이전 커밋: `f7c75fadd51b5c0ba80446153154d8667c1d04a4`
- 비교 상태: 1 commit ahead / 0 behind
- GitHub Combined Status: 등록된 Status 없음
- 해당 SHA GitHub Actions Workflow Run: 없음

최신 커밋의 변경 내용을 정적으로 확인했을 때 130일차 저격 물총 구현에 필요한 Catalog, Input, Inventory, Presentation, Policy, Test 변경이 포함되어 있다.

---

## 14. 검증 상태

GitHub 최신 커밋의 변경 파일과 주요 코드 연결을 정적으로 확인했다.

확인 항목:

- Network ID 29 등록
- `sniper_water_gun` Catalog 연결
- `AimDirection` Fusion Input 연결
- Input Provider의 로컬 카메라 방향 제출
- 0.8초 조준 Timer
- 50m 서버 Raycast
- 12m/s Item 외력 적용
- 젤리 보호막 / 부활 보호를 공통 외력 API로 재사용
- 실제 사격 시 슬롯 소비
- 조준 취소 시 미소비
- 2x / 4x 로컬 Zoom
- 조준 중 기존 카메라 휠 거리 조절 차단
- 투명 망토 공격 사용 해제 연동
- Respawn 시 조준 취소
- 41개 정책 테스트 사례 포함
- 1회용 Installer가 최종 커밋에 남아 있지 않음

정적 확인에서는 추가로 확인된 차단 수준의 문제는 없었다.

다만 현재 GitHub에는 이 커밋에 대한 Unity 컴파일 결과나 Test Runner 실행 결과가 연결되어 있지 않으므로, **Unity 전체 컴파일 성공 또는 모든 테스트 통과로 기록하지 않는다.**

---

## 15. Unity에서 최종 확인할 항목

커밋 확정 전 Unity에서 다음을 확인한다.

1. Console에 새 컴파일 Error가 없는지 확인
2. `ProjectJSniperWaterGunPolicyTests` 실행
3. 저격 물총 획득 후 우클릭 조준 시작 확인
4. 0.8초 전에 우클릭 해제 시 아이템 유지 확인
5. 0.8초 유지 시 1회 사격 후 아이템 소비 확인
6. 50m 이내 Player 적중 확인
7. 벽 뒤 Player가 맞지 않는지 확인
8. 적중 시 수평 외부 속도 적용 확인
9. 젤리 보호막 Player에게 외력이 차단되는지 확인
10. 부활 보호 Player에게 외력이 차단되는지 확인
11. 조준 중 휠로 2x / 4x 전환 확인
12. 조준 종료 후 일반 카메라 휠 거리 조절 복구 확인
13. Respawn 시 조준 상태가 남지 않는지 확인
14. Host / Client에서 조준과 적중 결과가 일치하는지 확인

---

## 130일차 결과

저격 물총의 네트워크 데이터 등록부터 조준 입력, 서버 권한 장거리 히트스캔, 보호 규칙, 아이템 소비, 로컬 2x/4x Zoom까지 기본 시스템을 연결했다.

다음 단계에서는 Unity 실제 멀티플레이 테스트 결과를 기준으로 조준 감도, FOV 연출, Hit Feedback, 정식 Crosshair UI 등을 보정할 수 있다.
