using System; // 아이템 사용 메시지 이벤트 기능 참조
using System.Collections; // 지속형 물총과 폭죽 대기 기능 참조
using System.Collections.Generic; // 범위 대상 중복 방지 목록 기능 참조
using ProjectJ.Data; // 아이템 데이터와 효과 종류 참조
using ProjectJ.Gameplay; // 경기 종료 상태 기능 참조
using ProjectJ.Player; // 플레이어 상태와 외부 힘 기능 참조
using UnityEngine; // Unity 물리와 오브젝트 기능 참조
using UnityEngine.InputSystem; // Q와 E와 우클릭 입력 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 플레이어당 아이템 사용 관리자 한 개만 허용
    [RequireComponent(typeof(PlayerItemInventory))] // 2슬롯 인벤토리 필수 지정
    [RequireComponent(typeof(PlayerItemEffectController))] // P0와 P1 지속형 효과 관리자 필수 지정
    [RequireComponent(typeof(PlayerP2ItemEffectController))] // P2 효과 관리자 필수 지정
    [RequireComponent(typeof(PlayerSniperWaterGunController))] // 저격 물총 조준 관리자 필수 지정
    public sealed class PlayerItemUseController : MonoBehaviour // 슬롯 선택과 P0와 P1과 P2 아이템 사용 관리자 선언
    { // 플레이어 아이템 사용 관리자 묶음
        [SerializeField] private PlayerItemInventory inventory; // 두 슬롯 아이템 제공자 저장
        [SerializeField] private PlayerItemEffectController effectController; // P0와 P1 지속형 효과 적용 대상 저장
        [SerializeField] private PlayerP2ItemEffectController p2EffectController; // P2 지속 효과와 추적 효과 적용 대상 저장
        [SerializeField] private PlayerSniperWaterGunController sniperController; // 저격 물총 조준과 발사 관리자 저장
        [SerializeField] private PlayerStateController playerStateController; // 행동 가능 상태 제공자 저장
        [SerializeField] private PrototypeMatchController matchController; // 경기 종료 상태 제공자 저장
        [SerializeField] private ItemPlacementValidator placementValidator; // 설치 위치 공통 검사기 저장
        [SerializeField] private Transform itemUseOrigin; // 투사체와 전방 효과 시작 위치 저장
        [SerializeField] private LayerMask effectCollisionLayers = ~0; // 투사체와 범위 효과 충돌 Layer 저장
        [SerializeField, Min(0.1f)] private float placementForwardDistance = 2.5f; // 플레이어 앞 설치 후보 거리 저장
        [SerializeField, Min(0.1f)] private float messageDuration = 2.5f; // HUD 사용 결과 문구 유지 시간 저장

        private string currentMessage = string.Empty; // 현재 HUD 사용 결과 문구 저장
        private float messageRemaining; // 사용 결과 문구 남은 시간 저장

        public event Action<string> UseMessageChanged; // HUD 사용 결과 문구 변경 이벤트

        public string CurrentMessage => currentMessage; // 현재 사용 결과 문구 반환

        private void Awake() // 실행 시작 시 필수 참조 준비
        { // 필수 참조 준비 처리
            ResolveReferences(); // 플레이어와 Scene 기반 누락 참조 자동 연결
        } // 필수 참조 준비 처리 종료

        private void Update() // 슬롯 선택과 우클릭 사용 입력 갱신
        { // 아이템 입력 프레임 처리
            UpdateMessageTime(); // 사용 결과 문구 남은 시간 갱신

            if (sniperController != null && sniperController.IsAiming) // 저격 물총 조준 중 여부 확인
            { // 조준 중 일반 슬롯과 사용 입력 차단 처리
                return; // 조준 관리자가 발사와 배율과 취소 입력을 처리하도록 종료
            } // 조준 중 일반 슬롯과 사용 입력 차단 처리 종료

            if (Keyboard.current != null) // 키보드 연결 여부 확인
            { // 슬롯 선택 키 입력 처리
                if (Keyboard.current.qKey.wasPressedThisFrame) // Q 첫 슬롯 선택 입력 확인
                { // 첫 슬롯 선택 처리
                    inventory.SelectSlot(0); // 첫 번째 슬롯 선택
                    ShowMessage("슬롯 1 선택"); // 선택 결과 HUD 문구 표시
                } // 첫 슬롯 선택 처리 종료

                if (Keyboard.current.eKey.wasPressedThisFrame) // E 둘째 슬롯 선택 입력 확인
                { // 둘째 슬롯 선택 처리
                    inventory.SelectSlot(1); // 두 번째 슬롯 선택
                    ShowMessage("슬롯 2 선택"); // 선택 결과 HUD 문구 표시
                } // 둘째 슬롯 선택 처리 종료
            } // 슬롯 선택 키 입력 처리 종료

            bool mouseUsePressed = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame; // 우클릭 아이템 사용 입력 여부 계산
            bool gamepadAimPressed = Gamepad.current != null && Gamepad.current.leftTrigger.wasPressedThisFrame && inventory != null && inventory.SelectedItem != null && inventory.SelectedItem.EffectType == ItemEffectType.SniperWaterGun; // 저격 물총 선택 중 LT 조준 입력 여부 계산

            if (mouseUsePressed || gamepadAimPressed) // 우클릭 일반 사용 또는 LT 저격 조준 입력 확인
            { // 선택 아이템 사용 처리
                TryUseSelectedItem(); // 현재 선택 슬롯 아이템 사용 시도
            } // 선택 아이템 사용 처리 종료
        } // 아이템 입력 프레임 처리 종료

        public bool TryUseSelectedItem() // 현재 선택 슬롯 아이템 사용 시도
        { // 선택 아이템 사용 처리
            if (matchController != null && matchController.IsMatchFinished) // 경기 종료 상태 여부 확인
            { // 경기 종료 사용 차단 처리
                ShowMessage("경기가 종료되어 아이템을 사용할 수 없습니다."); // 경기 종료 실패 사유 표시
                return false; // 아이템 사용 실패 반환
            } // 경기 종료 사용 차단 처리 종료

            if (playerStateController != null && !playerStateController.CanUseAction) // 현재 행동 가능 상태 확인
            { // 행동 차단 상태 처리
                ShowMessage("현재 상태에서는 아이템을 사용할 수 없습니다."); // 행동 차단 실패 사유 표시
                return false; // 아이템 사용 실패 반환
            } // 행동 차단 상태 처리 종료

            if (p2EffectController != null && (p2EffectController.IsRewinding || p2EffectController.IsCartRiding)) // 되감기 또는 카트 특수 이동 진행 여부 확인
            { // 특수 이동 중 추가 사용 차단 처리
                ShowMessage("되감기 또는 카트 이동 중에는 다른 아이템을 사용할 수 없습니다."); // 특수 이동 중 실패 사유 표시
                return false; // 아이템 사용 실패 반환
            } // 특수 이동 중 추가 사용 차단 처리 종료

            ItemDataDefinition itemData = inventory == null ? null : inventory.SelectedItem; // 현재 선택 아이템 조회

            if (itemData == null) // 선택 슬롯 비어 있음 여부 확인
            { // 빈 슬롯 사용 처리
                ShowMessage("선택한 슬롯이 비어 있습니다."); // 빈 슬롯 실패 사유 표시
                return false; // 아이템 사용 실패 반환
            } // 빈 슬롯 사용 처리 종료

            if (itemData.ImplementationPriority == ItemImplementationPriority.P2 && itemData.EffectType == ItemEffectType.SniperWaterGun) // 저격 물총의 소비 전 조준 모드 진입 여부 확인
            { // 저격 물총 조준 시작 처리
                if (sniperController == null || !sniperController.TryBeginAim(itemData, inventory.SelectedSlotIndex, itemUseOrigin, effectCollisionLayers, ShowMessage)) // 저격 조준 관리자와 시작 성공 여부 확인
                { // 저격 물총 조준 실패 처리
                    ShowMessage("저격 물총 조준을 시작할 수 없습니다."); // 조준 시작 실패 사유 표시
                    return false; // 아이템을 보존한 채 조준 실패 반환
                } // 저격 물총 조준 실패 처리 종료

                return true; // 실제 발사 전 아이템을 소비하지 않은 조준 성공 반환
            } // 저격 물총 조준 시작 처리 종료

            bool executed = itemData.ImplementationPriority == ItemImplementationPriority.P0 ? TryExecuteP0Item(itemData) : itemData.ImplementationPriority == ItemImplementationPriority.P1 ? TryExecuteP1Item(itemData) : TryExecuteP2Item(itemData); // 우선순위 기반 실제 효과 실행

            if (!executed) // 아이템 효과 실행 실패 여부 확인
            { // 실행 실패 처리
                return false; // 아이템을 보존한 채 사용 실패 반환
            } // 실행 실패 처리 종료

            if (!inventory.TryConsumeSelectedItem(out ItemDataDefinition unusedConsumedItem)) // 성공 효과의 선택 아이템 한 개 소비 시도
            { // 예상하지 못한 소비 실패 처리
                ShowMessage("아이템 수량 갱신에 실패했습니다."); // 소비 실패 사유 표시
                return false; // 아이템 사용 실패 반환
            } // 예상하지 못한 소비 실패 처리 종료

            ShowMessage($"{itemData.DisplayName} 사용"); // 사용 성공 문구 표시
            return true; // 아이템 사용 성공 반환
        } // 선택 아이템 사용 처리 종료

        private bool TryExecuteP0Item(ItemDataDefinition itemData) // P0 10종 실제 효과 실행
        { // P0 아이템 효과 분기 처리
            switch (itemData.EffectType) // 아이템 효과 종류 선택
            { // P0 아이템 효과 종류 분기
                case ItemEffectType.SpringShoes: // 스프링 신발 효과 확인
                    effectController.ActivateSpringShoes(itemData.EffectDuration, itemData.PrimaryValue); // 추가 점프 효과 활성화
                    return true; // 스프링 신발 사용 성공 반환
                case ItemEffectType.JellyShield: // 젤리 보호막 효과 확인
                    effectController.ActivateJellyShield(itemData.EffectDuration); // 외부 힘 방어 활성화
                    return true; // 젤리 보호막 사용 성공 반환
                case ItemEffectType.BananaCushion: // 바나나 쿠션 효과 확인
                case ItemEffectType.Mine: // 지뢰 효과 확인
                    return TryPlaceItem(itemData); // 공통 설치 검사 뒤 설치 효과 반환
                case ItemEffectType.BalloonTrumpet: // 풍선 나팔 효과 확인
                    ApplyConePush(itemData); // 전방 부채꼴 범위 밀치기 적용
                    return true; // 풍선 나팔 사용 성공 반환
                case ItemEffectType.WaterGun: // 물총 효과 확인
                    StartCoroutine(UseWaterGun(itemData)); // 지속 물줄기 밀치기 시작
                    return true; // 물총 사용 성공 반환
                case ItemEffectType.Firework: // 폭죽 효과 확인
                    StartCoroutine(UseFirework(itemData)); // 준비 시간 뒤 범위 밀치기 시작
                    return true; // 폭죽 사용 성공 반환
                case ItemEffectType.FeatherShoes: // 깃털 신발 효과 확인
                    effectController.ActivateFeatherShoes(itemData.EffectDuration, itemData.PrimaryValue); // 이동 속도 강화 활성화
                    return true; // 깃털 신발 사용 성공 반환
                case ItemEffectType.Snowball: // 눈덩이 효과 확인
                case ItemEffectType.Ball: // 풀 공 효과 확인
                    SpawnStraightProjectile(itemData); // 직선 투사체 생성
                    return true; // 투사체 아이템 사용 성공 반환
                default: // P0 외 효과 또는 잘못된 데이터 처리
                    ShowMessage("P0 실행 대상이 아닌 아이템입니다."); // 실행 대상 오류 문구 표시
                    return false; // P0 아이템 사용 실패 반환
            } // P0 아이템 효과 종류 분기 종료
        } // P0 아이템 효과 분기 처리 종료

        private bool TryExecuteP1Item(ItemDataDefinition itemData) // P1 11종 실제 효과 실행
        { // P1 아이템 효과 분기 처리
            switch (itemData.EffectType) // P1 아이템 효과 종류 선택
            { // P1 아이템 효과 종류 분기
                case ItemEffectType.Jetpack: // 제트팩 효과 확인
                    effectController.ActivateJetpack(itemData.EffectDuration, itemData.PrimaryValue); // Space 유지 상승 효과 활성화
                    return true; // 제트팩 사용 성공 반환
                case ItemEffectType.Hammer: // 망치 효과 확인
                    effectController.ActivateHammer(itemData.EffectDuration, itemData.PrimaryValue, itemData.EffectRange); // 밀치기 힘과 사거리 강화 활성화
                    return true; // 망치 사용 성공 반환
                case ItemEffectType.Bomb: // 폭탄 효과 확인
                    SpawnThrownItem(itemData, itemData.EffectDuration); // 2.5초 시한 폭탄 투척
                    return true; // 폭탄 사용 성공 반환
                case ItemEffectType.PufferBalloonSuit: // 복어 풍선옷 효과 확인
                    effectController.ActivatePufferBalloonSuit(itemData.EffectDuration, itemData.PrimaryValue, itemData.EffectRadius, itemData.Cooldown); // 접촉 대상 반복 밀치기 활성화
                    return true; // 복어 풍선옷 사용 성공 반환
                case ItemEffectType.InkOctopus: // 먹물 문어 효과 확인
                case ItemEffectType.SoapBubble: // 비눗방울 효과 확인
                    SpawnStraightProjectile(itemData); // 상태 이상 직선 투사체 생성
                    return true; // 상태 이상 투사체 사용 성공 반환
                case ItemEffectType.FishingRod: // 낚시대 효과 확인
                    return TryUseFishingRod(itemData); // 조준 대상 끌어당기기 결과 반환
                case ItemEffectType.GrapplingHook: // 갈고리 효과 확인
                    return TryUseGrapplingHook(itemData); // 조준 구조물 이동 결과 반환
                case ItemEffectType.SmokeGrenade: // 연막탄 효과 확인
                    SpawnThrownItem(itemData, 0.6f); // 짧은 작동 시간 뒤 연막 생성
                    return true; // 연막탄 사용 성공 반환
                case ItemEffectType.Trampoline: // 트램폴린 효과 확인
                    return TryPlaceItem(itemData); // 공통 설치 검사 뒤 사용자 전용 발판 생성
                case ItemEffectType.GiantBalloon: // 거대 풍선 효과 확인
                    effectController.ActivateGiantBalloon(itemData.EffectDuration, itemData.PrimaryValue); // 자동 상승 효과 활성화
                    return true; // 거대 풍선 사용 성공 반환
                default: // P1 외 효과 또는 잘못된 데이터 처리
                    ShowMessage("P1 실행 대상이 아닌 아이템입니다."); // 실행 대상 오류 문구 표시
                    return false; // P1 아이템 사용 실패 반환
            } // P1 아이템 효과 종류 분기 종료
        } // P1 아이템 효과 분기 처리 종료

        private bool TryExecuteP2Item(ItemDataDefinition itemData) // P2 7종 실제 효과 실행
        { // P2 아이템 효과 분기 처리
            if (p2EffectController == null) // P2 효과 관리자 누락 여부 확인
            { // P2 효과 관리자 누락 처리
                ShowMessage("P2 아이템 효과 관리자를 찾을 수 없습니다."); // 누락 참조 실패 사유 표시
                return false; // 아이템을 보존한 채 실행 실패 반환
            } // P2 효과 관리자 누락 처리 종료

            switch (itemData.EffectType) // P2 아이템 효과 종류 선택
            { // P2 아이템 효과 종류 분기
                case ItemEffectType.RewindClock: // 되감기 시계 효과 확인
                    if (!p2EffectController.TryActivateRewind(itemData.EffectDuration, itemData.PrimaryValue > 0f ? itemData.PrimaryValue : P2ItemRules.RewindPlaybackDuration)) // 최근 안전 위치 되감기 시작 여부 확인
                    { // 되감기 기록 부족 처리
                        ShowMessage("되감기에 필요한 안전 위치 기록이 부족합니다."); // 기록 부족 실패 문구 표시
                        return false; // 아이템을 보존한 채 실행 실패 반환
                    } // 되감기 기록 부족 처리 종료

                    return true; // 되감기 시계 사용 성공 반환
                case ItemEffectType.HomingMissile: // 유도탄 효과 확인
                case ItemEffectType.Drone: // 드론 효과 확인
                    if (!p2EffectController.TrySpawnHomingEffect(itemData, GetUseOriginPosition(), GetUseDirection())) // 전방 추적 효과와 최초 목표 생성 여부 확인
                    { // 추적 대상 없음 처리
                        ShowMessage(itemData.EffectType == ItemEffectType.Drone ? "드론이 추적할 1위 대상을 찾지 못했습니다." : "유도탄이 추적할 대상을 찾지 못했습니다."); // 효과 종류에 맞는 목표 없음 문구 표시
                        return false; // 아이템을 보존한 채 실행 실패 반환
                    } // 추적 대상 없음 처리 종료

                    return true; // 유도탄 또는 드론 사용 성공 반환
                case ItemEffectType.MiniaturePotion: // 소형화 물약 효과 확인
                    p2EffectController.ActivateMiniature(itemData.EffectDuration, itemData.PrimaryValue); // 외형과 CharacterController 축소 활성화
                    return true; // 소형화 물약 사용 성공 반환
                case ItemEffectType.InvisibilityCloak: // 투명 망토 효과 확인
                    p2EffectController.ActivateInvisibility(itemData.EffectDuration); // 외형 숨김과 추적 대상 제외 활성화
                    return true; // 투명 망토 사용 성공 반환
                case ItemEffectType.Cart: // 카트 효과 확인
                    if (!p2EffectController.TryActivateCart(itemData.EffectDuration, itemData.PrimaryValue > 0f ? itemData.PrimaryValue : P2ItemRules.CartSpeed, itemData.EffectRange > 0f ? itemData.EffectRange : P2ItemRules.CartRouteSearchRange)) // 가까운 연결 경로 자동 주행 시작 여부 확인
                    { // 카트 경로 없음 처리
                        ShowMessage("가까운 카트 경로 또는 다음 경로 지점을 찾지 못했습니다."); // 경로 없음 실패 문구 표시
                        return false; // 아이템을 보존한 채 실행 실패 반환
                    } // 카트 경로 없음 처리 종료

                    return true; // 카트 사용 성공 반환
                case ItemEffectType.SniperWaterGun: // 저격 물총 효과 확인
                    ShowMessage("저격 물총은 조준 모드에서 사용해야 합니다."); // 잘못된 직접 실행 안내 표시
                    return false; // 조준 없는 직접 실행 실패 반환
                default: // P2 외 효과 또는 잘못된 데이터 처리
                    ShowMessage("P2 실행 대상이 아닌 아이템입니다."); // 실행 대상 오류 문구 표시
                    return false; // P2 아이템 사용 실패 반환
            } // P2 아이템 효과 종류 분기 종료
        } // P2 아이템 효과 분기 처리 종료

        private bool TryPlaceItem(ItemDataDefinition itemData) // 설치 위치 공통 검사 뒤 설치 아이템 생성
        { // 설치형 아이템 사용 처리
            if (placementValidator == null) // 공통 설치 검사기 누락 여부 확인
            { // 설치 검사기 누락 처리
                ShowMessage("설치 위치 검사기를 찾을 수 없습니다."); // 필수 참조 누락 문구 표시
                return false; // 설치 실패 반환
            } // 설치 검사기 누락 처리 종료

            Vector3 forward = GetUseDirection(); // 현재 플레이어 전방 방향 계산
            Vector3 horizontalForward = Vector3.ProjectOnPlane(forward, Vector3.up); // 지면 설치용 수평 전방 계산
            horizontalForward = horizontalForward.sqrMagnitude <= 0.0001f ? transform.forward : horizontalForward.normalized; // 안전한 수평 설치 방향 보정
            Vector3 requestedPosition = transform.position + horizontalForward * placementForwardDistance; // 플레이어 앞 설치 후보 위치 계산
            Quaternion rotation = Quaternion.LookRotation(horizontalForward, Vector3.up); // 플레이어 전방 기반 설치 회전 계산

            if (!placementValidator.TryValidate(requestedPosition, itemData.PlacementHalfExtents, rotation, transform, out ItemPlacementResult placementResult)) // 지면과 경사와 장애물 공통 검사 실행
            { // 설치 위치 검사 실패 처리
                ShowMessage(GetPlacementFailureMessage(placementResult.FailureReason)); // 설치 실패 원인 HUD 문구 표시
                return false; // 설치 실패 반환
            } // 설치 위치 검사 실패 처리 종료

            GameObject placedObject = new GameObject($"Placed_{itemData.DataId}_{itemData.DisplayName}"); // 설치 아이템 루트 생성
            placedObject.transform.position = placementResult.Position; // 검사 완료 지면 위치 적용
            placedObject.transform.rotation = rotation; // 플레이어 전방 설치 회전 적용
            PlacedItemEffect placedEffect = placedObject.AddComponent<PlacedItemEffect>(); // 설치형 실제 효과 기능 추가
            float lifeTime = itemData.EffectDuration > 0f ? itemData.EffectDuration : 20f; // 데이터 시간 또는 안전 기본 수명 선택
            int maximumUses = itemData.EffectType == ItemEffectType.Trampoline ? Mathf.Max(1, Mathf.RoundToInt(itemData.SecondaryValue)) : 1; // 트램폴린 최대 세 번 사용 횟수 계산
            placedEffect.Configure(itemData.EffectType, transform, forward, itemData.PrimaryValue, lifeTime, itemData.PlacementHalfExtents, itemData.PickupColor, maximumUses); // 설치 효과와 표시와 사용 횟수 연결
            return true; // 설치 성공 반환
        } // 설치형 아이템 사용 처리 종료

        private void ApplyConePush(ItemDataDefinition itemData) // 풍선 나팔 전방 부채꼴 밀치기 적용
        { // 전방 부채꼴 밀치기 처리
            Vector3 origin = GetUseOriginPosition(); // 아이템 사용 시작 위치 계산
            Vector3 forward = GetUseDirection(); // 아이템 사용 전방 방향 계산
            Collider[] overlaps = Physics.OverlapSphere(origin, itemData.EffectRange, effectCollisionLayers, QueryTriggerInteraction.Ignore); // 전방 거리 안의 모든 Collider 수집
            HashSet<int> affectedIds = new HashSet<int>(); // 같은 대상 중복 적용 방지 목록 생성
            float minimumDot = Mathf.Cos(Mathf.Clamp(itemData.SecondaryValue, 0f, 180f) * 0.5f * Mathf.Deg2Rad); // 전체 각도 기반 최소 내적 계산

            for (int colliderIndex = 0; colliderIndex < overlaps.Length; colliderIndex++) // 범위 안 Collider 전체 순회
            { // 현재 Collider 부채꼴 판정 처리
                ExternalForceReceiver receiver = overlaps[colliderIndex] == null ? null : overlaps[colliderIndex].GetComponentInParent<ExternalForceReceiver>(); // 현재 외부 힘 대상 조회

                if (!IsValidTarget(receiver, affectedIds)) // 대상 유효성과 중복 여부 확인
                { // 무효 대상 처리
                    continue; // 현재 대상 생략
                } // 무효 대상 처리 종료

                Vector3 targetDirection = receiver.ForceReceiverTransform.position - origin; // 시작점에서 대상 방향 계산
                Vector3 horizontalDirection = Vector3.ProjectOnPlane(targetDirection, Vector3.up).normalized; // 수평 대상 방향 계산

                if (Vector3.Dot(Vector3.ProjectOnPlane(forward, Vector3.up).normalized, horizontalDirection) < minimumDot) // 부채꼴 각도 포함 여부 확인
                { // 부채꼴 밖 대상 처리
                    continue; // 현재 대상 밀치기 생략
                } // 부채꼴 밖 대상 처리 종료

                receiver.TryReceiveExternalForce(horizontalDirection, itemData.PrimaryValue); // 대상 바깥쪽으로 공통 밀치기 힘 적용
                affectedIds.Add(receiver.GetInstanceID()); // 현재 대상 적용 완료 등록
            } // 현재 Collider 부채꼴 판정 처리 종료
        } // 전방 부채꼴 밀치기 처리 종료

        private IEnumerator UseWaterGun(ItemDataDefinition itemData) // 데이터 시간 동안 직선 물줄기 연속 밀치기
        { // 물총 지속 효과 처리
            float remainingDuration = itemData.EffectDuration; // 물총 남은 사용 시간 초기화
            float interval = Mathf.Max(0.05f, itemData.Cooldown); // 최소 물줄기 판정 간격 계산

            while (remainingDuration > 0f) // 물총 지속 시간 동안 반복
            { // 현재 물줄기 판정 처리
                Vector3 origin = GetUseOriginPosition(); // 현재 프레임 물줄기 시작 위치 계산
                Vector3 direction = GetUseDirection(); // 현재 프레임 조준 방향 계산

                if (Physics.SphereCast(origin, Mathf.Max(0.05f, itemData.EffectRadius), direction, out RaycastHit hit, itemData.EffectRange, effectCollisionLayers, QueryTriggerInteraction.Ignore)) // 직선 물줄기 첫 충돌 검사
                { // 물줄기 충돌 처리
                    ExternalForceReceiver receiver = hit.collider == null ? null : hit.collider.GetComponentInParent<ExternalForceReceiver>(); // 충돌 대상 외부 힘 수신기 조회

                    if (receiver != null && receiver.transform != transform && receiver.CanReceivePush) // 사용자 자신이 아닌 유효 대상 여부 확인
                    { // 물줄기 밀치기 적용 처리
                        receiver.TryReceiveExternalForce(direction, itemData.PrimaryValue); // 조준 방향 연속 공통 밀치기 힘 적용
                    } // 물줄기 밀치기 적용 처리 종료
                } // 물줄기 충돌 처리 종료

                yield return new WaitForSeconds(interval); // 다음 물줄기 판정 간격 대기
                remainingDuration -= interval; // 대기한 시간만큼 남은 사용 시간 감소
            } // 현재 물줄기 판정 처리 종료
        } // 물총 지속 효과 처리 종료

        private IEnumerator UseFirework(ItemDataDefinition itemData) // 준비 시간 뒤 전방 넓은 범위 폭발
        { // 폭죽 지연 효과 처리
            Vector3 explosionPosition = GetUseOriginPosition() + GetUseDirection() * Mathf.Max(1f, itemData.EffectRange); // 사용 순간 전방 폭발 위치 저장
            yield return new WaitForSeconds(itemData.EffectDuration); // 데이터 기반 폭죽 준비 시간 대기
            ApplyRadialPush(explosionPosition, itemData.EffectRadius, itemData.PrimaryValue); // 저장된 위치에서 범위 밀치기 적용
        } // 폭죽 지연 효과 처리 종료

        private void ApplyRadialPush(Vector3 center, float radius, float force) // 지정 위치 주변 대상 방사형 밀치기 적용
        { // 방사형 밀치기 처리
            Collider[] overlaps = Physics.OverlapSphere(center, radius, effectCollisionLayers, QueryTriggerInteraction.Ignore); // 폭발 범위 Collider 수집
            HashSet<int> affectedIds = new HashSet<int>(); // 같은 대상 중복 적용 방지 목록 생성

            for (int colliderIndex = 0; colliderIndex < overlaps.Length; colliderIndex++) // 폭발 범위 Collider 전체 순회
            { // 현재 폭발 대상 처리
                ExternalForceReceiver receiver = overlaps[colliderIndex] == null ? null : overlaps[colliderIndex].GetComponentInParent<ExternalForceReceiver>(); // 현재 외부 힘 대상 조회

                if (!IsValidTarget(receiver, affectedIds)) // 대상 유효성과 중복 여부 확인
                { // 무효 폭발 대상 처리
                    continue; // 현재 대상 생략
                } // 무효 폭발 대상 처리 종료

                Vector3 direction = receiver.ForceReceiverTransform.position - center; // 폭발 중심에서 대상 방향 계산
                direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized + Vector3.up * 0.25f; // 수평과 약한 위쪽 혼합 방향 계산
                receiver.TryReceiveExternalForce(direction.normalized, force); // 폭발 바깥쪽 공통 밀치기 힘 적용
                affectedIds.Add(receiver.GetInstanceID()); // 현재 대상 적용 완료 등록
            } // 현재 폭발 대상 처리 종료
        } // 방사형 밀치기 처리 종료

        private void SpawnStraightProjectile(ItemDataDefinition itemData) // 눈덩이와 풀 공과 먹물과 비눗방울 직선 투사체 생성
        { // 직선 투사체 생성 처리
            GameObject projectileObject = new GameObject($"Projectile_{itemData.DataId}_{itemData.DisplayName}"); // 아이템 ID 기반 투사체 루트 생성
            projectileObject.transform.position = GetUseOriginPosition(); // 현재 아이템 사용 시작 위치 적용
            projectileObject.transform.rotation = Quaternion.LookRotation(GetUseDirection(), Vector3.up); // 현재 조준 방향 회전 적용
            ItemProjectileEffect projectile = projectileObject.AddComponent<ItemProjectileEffect>(); // 투사체 이동과 적중 효과 기능 추가
            float projectileRadius = Mathf.Max(0.1f, itemData.EffectRadius); // 데이터 기반 안전 충돌 반지름 계산
            float lifeTime = P1ItemRules.CalculateProjectileLifeTime(itemData.EffectRange, itemData.ProjectileSpeed, 3f); // 최대 거리 기반 투사체 수명 계산
            projectile.Configure(itemData.EffectType, transform, GetUseDirection(), itemData.ProjectileSpeed, itemData.PrimaryValue, itemData.EffectDuration, lifeTime, projectileRadius, itemData.PickupColor, effectCollisionLayers); // 이동과 적중 효과 데이터 연결
        } // 직선 투사체 생성 처리 종료

        private void SpawnThrownItem(ItemDataDefinition itemData, float fuseTime) // 폭탄 또는 연막탄 곡선 투척물 생성
        { // 투척 아이템 생성 처리
            GameObject thrownObject = new GameObject($"Thrown_{itemData.DataId}_{itemData.DisplayName}"); // 아이템 ID 기반 투척물 루트 생성
            ThrownItemEffect thrownEffect = thrownObject.AddComponent<ThrownItemEffect>(); // 곡선 이동과 작동 효과 기능 추가
            float effectDuration = itemData.EffectType == ItemEffectType.SmokeGrenade ? itemData.EffectDuration : 0.1f; // 연막 유지 시간 또는 폭탄 안전 기본값 선택
            thrownEffect.Configure(itemData.EffectType, transform, GetUseOriginPosition(), GetUseDirection(), itemData.ProjectileSpeed, 3.5f, -18f, fuseTime, effectDuration, itemData.EffectRadius, itemData.PrimaryValue, 0.3f, itemData.PickupColor, effectCollisionLayers); // 투척 이동과 폭발 또는 연막 데이터 연결
        } // 투척 아이템 생성 처리 종료

        private bool TryUseFishingRod(ItemDataDefinition itemData) // 최대 거리 안 첫 대상 사용자 방향 끌어당기기
        { // 낚시대 사용 처리
            if (!TryFindFirstAimedHit(itemData.EffectRange, 0.2f, out RaycastHit hit)) // 조준선 첫 충돌 검색 여부 확인
            { // 낚시대 대상 없음 처리
                ShowMessage("낚시대로 당길 대상을 찾지 못했습니다."); // 대상 없음 실패 문구 표시
                return false; // 아이템 보존 사용 실패 반환
            } // 낚시대 대상 없음 처리 종료

            ExternalForceReceiver receiver = hit.collider == null ? null : hit.collider.GetComponentInParent<ExternalForceReceiver>(); // 첫 충돌 외부 힘 대상 조회

            if (receiver == null || receiver.transform == transform || !receiver.CanReceivePush) // 당길 수 있는 상대 여부 확인
            { // 낚시대 무효 대상 처리
                ShowMessage("조준한 물체는 낚시대로 당길 수 없습니다."); // 무효 대상 실패 문구 표시
                return false; // 아이템 보존 사용 실패 반환
            } // 낚시대 무효 대상 처리 종료

            Vector3 pullDirection = transform.position - receiver.ForceReceiverTransform.position; // 대상에서 사용자 방향 계산
            pullDirection = Vector3.ProjectOnPlane(pullDirection, Vector3.up); // 수평 끌어당기기 방향으로 보정

            if (!receiver.TryReceiveExternalForce(pullDirection, itemData.PrimaryValue)) // 데이터 기반 끌어당기기 적용 시도
            { // 낚시대 힘 적용 실패 처리
                ShowMessage("현재 대상은 끌어당길 수 없습니다."); // 대상 상태 실패 문구 표시
                return false; // 아이템 보존 사용 실패 반환
            } // 낚시대 힘 적용 실패 처리 종료

            return true; // 낚시대 사용 성공 반환
        } // 낚시대 사용 처리 종료

        private bool TryUseGrapplingHook(ItemDataDefinition itemData) // 최대 거리 안 구조물 목표 이동 시작
        { // 갈고리 사용 처리
            if (!TryFindFirstAimedHit(itemData.EffectRange, 0.15f, out RaycastHit hit)) // 갈고리 조준선 첫 충돌 검색 여부 확인
            { // 갈고리 구조물 없음 처리
                ShowMessage("갈고리를 걸 구조물을 찾지 못했습니다."); // 구조물 없음 실패 문구 표시
                return false; // 아이템 보존 사용 실패 반환
            } // 갈고리 구조물 없음 처리 종료

            ExternalForceReceiver receiver = hit.collider == null ? null : hit.collider.GetComponentInParent<ExternalForceReceiver>(); // 첫 충돌 이동 대상 여부 조회

            if (receiver != null) // 구조물이 아닌 플레이어 또는 더미 여부 확인
            { // 갈고리 대상 종류 차단 처리
                ShowMessage("갈고리는 구조물에만 걸 수 있습니다."); // 잘못된 대상 실패 문구 표시
                return false; // 아이템 보존 사용 실패 반환
            } // 갈고리 대상 종류 차단 처리 종료

            effectController.ActivateGrapplingHook(hit.point, itemData.PrimaryValue); // 구조물 충돌 지점 방향 이동 활성화
            return true; // 갈고리 사용 성공 반환
        } // 갈고리 사용 처리 종료

        private bool TryFindFirstAimedHit(float range, float radius, out RaycastHit closestHit) // 사용자 자신을 제외한 조준선 첫 충돌 검색
        { // 조준선 첫 충돌 검색 처리
            RaycastHit[] hits = Physics.SphereCastAll(GetUseOriginPosition(), Mathf.Max(0.01f, radius), GetUseDirection(), Mathf.Max(0f, range), effectCollisionLayers, QueryTriggerInteraction.Ignore); // 조준 거리 안 모든 구체 충돌 수집
            float closestDistance = float.PositiveInfinity; // 가장 가까운 충돌 거리 초기화
            closestHit = default; // 충돌 없음 기본 결과 설정

            for (int index = 0; index < hits.Length; index++) // 모든 조준 충돌 순회
            { // 현재 조준 충돌 확인
                RaycastHit hit = hits[index]; // 현재 충돌 결과 조회

                if (hit.collider == null || hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)) // 누락 충돌과 사용자 자신 여부 확인
                { // 무시할 조준 충돌 처리
                    continue; // 현재 충돌 제외
                } // 무시할 조준 충돌 처리 종료

                if (hit.distance < closestDistance) // 더 가까운 충돌 여부 확인
                { // 가장 가까운 조준 충돌 갱신
                    closestDistance = hit.distance; // 새 가장 가까운 거리 저장
                    closestHit = hit; // 새 가장 가까운 충돌 저장
                } // 가장 가까운 조준 충돌 갱신 종료
            } // 현재 조준 충돌 확인 종료

            return closestDistance < float.PositiveInfinity; // 유효한 첫 충돌 존재 여부 반환
        } // 조준선 첫 충돌 검색 처리 종료

        private bool IsValidTarget(ExternalForceReceiver receiver, HashSet<int> affectedIds) // 범위 효과 대상 유효성과 중복 여부 확인
        { // 범위 효과 대상 검사 처리
            if (receiver == null || receiver.transform == transform || !receiver.CanReceivePush) // 누락 대상과 사용자 자신과 수신 불가 여부 확인
            { // 범위 효과 대상 불가 처리
                return false; // 대상 불가 반환
            } // 범위 효과 대상 불가 처리 종료

            return !affectedIds.Contains(receiver.GetInstanceID()); // 아직 효과를 받지 않은 대상 여부 반환
        } // 범위 효과 대상 검사 처리 종료

        private Vector3 GetUseOriginPosition() // 투사체와 전방 효과 시작 위치 계산
        { // 아이템 사용 시작 위치 계산 처리
            Transform origin = itemUseOrigin == null ? transform : itemUseOrigin; // 설정된 시작 Transform 또는 플레이어 선택
            return origin.position + origin.forward * 0.6f; // 사용자 충돌체 앞쪽 시작 위치 반환
        } // 아이템 사용 시작 위치 계산 처리 종료

        private Vector3 GetUseDirection() // 현재 아이템 조준 방향 계산
        { // 아이템 조준 방향 계산 처리
            Transform origin = itemUseOrigin == null ? transform : itemUseOrigin; // 설정된 시작 Transform 또는 플레이어 선택
            return origin.forward.sqrMagnitude <= 0.0001f ? transform.forward : origin.forward.normalized; // 안전하게 보정한 전방 방향 반환
        } // 아이템 조준 방향 계산 처리 종료

        private static string GetPlacementFailureMessage(ItemPlacementFailureReason failureReason) // 설치 검사 실패 원인별 HUD 문구 반환
        { // 설치 실패 문구 선택 처리
            switch (failureReason) // 설치 실패 원인 선택
            { // 설치 실패 원인 분기
                case ItemPlacementFailureReason.NoGround: // 지면 없음 원인 확인
                    return "아래에 설치 가능한 지면이 없습니다."; // 지면 없음 문구 반환
                case ItemPlacementFailureReason.SlopeTooSteep: // 급경사 원인 확인
                    return "경사가 너무 가파릅니다."; // 급경사 문구 반환
                case ItemPlacementFailureReason.Blocked: // 장애물 겹침 원인 확인
                    return "설치 공간이 다른 물체와 겹칩니다."; // 장애물 겹침 문구 반환
                case ItemPlacementFailureReason.OutsideAllowedArea: // 허용 영역 이탈 원인 확인
                    return "허용된 설치 영역 밖입니다."; // 영역 이탈 문구 반환
                default: // 알 수 없는 설치 실패 처리
                    return "현재 위치에 설치할 수 없습니다."; // 공통 설치 실패 문구 반환
            } // 설치 실패 원인 분기 종료
        } // 설치 실패 문구 선택 처리 종료

        private void ShowMessage(string message) // 아이템 사용 결과 HUD 문구 갱신
        { // 사용 결과 문구 갱신 처리
            currentMessage = message ?? string.Empty; // 빈 값을 허용한 새 문구 저장
            messageRemaining = messageDuration; // 문구 유지 시간 초기화
            UseMessageChanged?.Invoke(currentMessage); // HUD 표시 컴포넌트에 새 문구 전달
        } // 사용 결과 문구 갱신 처리 종료

        private void UpdateMessageTime() // 아이템 사용 결과 문구 유지 시간 갱신
        { // 사용 결과 문구 시간 처리
            if (messageRemaining <= 0f) // 표시 중인 문구 없음 여부 확인
            { // 문구 시간 처리 생략
                return; // 문구 갱신 종료
            } // 문구 시간 처리 생략 종료

            messageRemaining = Mathf.Max(0f, messageRemaining - Time.unscaledDeltaTime); // 메뉴와 무관한 프레임 시간으로 남은 시간 감소

            if (messageRemaining <= 0f) // 문구 표시 시간 종료 여부 확인
            { // 문구 자동 숨김 처리
                currentMessage = string.Empty; // 현재 문구 제거
                UseMessageChanged?.Invoke(currentMessage); // HUD 문구 숨김 요청 전달
            } // 문구 자동 숨김 처리 종료
        } // 사용 결과 문구 시간 처리 종료

        private void ResolveReferences() // 플레이어와 Scene 기반 누락 참조 자동 연결
        { // 누락 참조 자동 연결 처리
            inventory = inventory == null ? GetComponent<PlayerItemInventory>() : inventory; // 같은 오브젝트 인벤토리 저장
            effectController = effectController == null ? GetComponent<PlayerItemEffectController>() : effectController; // 같은 오브젝트 지속 효과 관리자 저장
            p2EffectController = p2EffectController == null ? GetComponent<PlayerP2ItemEffectController>() : p2EffectController; // 같은 오브젝트 P2 효과 관리자 저장
            sniperController = sniperController == null ? GetComponent<PlayerSniperWaterGunController>() : sniperController; // 같은 오브젝트 저격 조준 관리자 저장
            playerStateController = playerStateController == null ? GetComponent<PlayerStateController>() : playerStateController; // 같은 오브젝트 플레이어 상태 관리자 저장
            matchController = matchController == null ? FindFirstObjectByType<PrototypeMatchController>() : matchController; // 현재 Scene 경기 관리자 저장
            placementValidator = placementValidator == null ? FindFirstObjectByType<ItemPlacementValidator>() : placementValidator; // 현재 Scene 설치 검사기 저장
            itemUseOrigin = itemUseOrigin == null ? transform : itemUseOrigin; // 시작 Transform 누락 시 플레이어 Transform 저장
        } // 누락 참조 자동 연결 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(PlayerItemInventory newInventory, PlayerItemEffectController newEffectController, PlayerStateController newPlayerStateController, PrototypeMatchController newMatchController, ItemPlacementValidator newPlacementValidator, Transform newItemUseOrigin, LayerMask newEffectCollisionLayers, float newPlacementForwardDistance, float newMessageDuration) // 자동 설정 도구용 아이템 사용 참조 연결
        { // 자동 설정 도구용 아이템 사용 참조 연결 처리
            inventory = newInventory; // 인벤토리 저장
            effectController = newEffectController; // 지속 효과 관리자 저장
            playerStateController = newPlayerStateController; // 플레이어 상태 관리자 저장
            matchController = newMatchController; // 경기 관리자 저장
            placementValidator = newPlacementValidator; // 설치 검사기 저장
            itemUseOrigin = newItemUseOrigin; // 사용 시작 Transform 저장
            effectCollisionLayers = newEffectCollisionLayers; // 효과 충돌 Layer 저장
            placementForwardDistance = Mathf.Max(0.1f, newPlacementForwardDistance); // 설치 거리 보정 후 저장
            messageDuration = Mathf.Max(0.1f, newMessageDuration); // 문구 시간 보정 후 저장
        } // 자동 설정 도구용 아이템 사용 참조 연결 처리 종료

        public void ConfigureP2ForEditor(PlayerP2ItemEffectController newP2EffectController, PlayerSniperWaterGunController newSniperController) // 44일차 자동 설정 도구용 P2 실행 참조 연결
        { // P2 실행 참조 연결 처리
            p2EffectController = newP2EffectController; // P2 지속 효과와 추적 효과 관리자 저장
            sniperController = newSniperController; // 저격 물총 조준 관리자 저장
        } // P2 실행 참조 연결 처리 종료
#endif // Editor 전용 설정 종료
    } // 플레이어 아이템 사용 관리자 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
