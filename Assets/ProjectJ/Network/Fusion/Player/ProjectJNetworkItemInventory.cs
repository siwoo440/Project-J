using System; // RaycastHit 정렬 사용
using Fusion; // NetworkBehaviour와 TickTimer 사용
using ProjectJ.Debugging; // 통합 디버그 패널 표시 상태 사용
using ProjectJ.Items; // 기존 ItemDefinition 사용
using ProjectJ.Items.Placement; // 기존 설치 위치 검증 사용
using UnityEngine; // Unity 기본 타입 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent] // 동일 네트워크 인벤토리 중복 방지
    [RequireComponent(typeof(ProjectJNetworkExternalGameplay))] // 경기 상태 확인 보장
    [RequireComponent(typeof(ProjectJNetworkPlayer))] // 스프링 신발 수직 속도 적용 보장
    public sealed partial class ProjectJNetworkItemInventory :
        NetworkBehaviour
    {
        private const int EmptyItemId = 0; // 빈 슬롯 네트워크 ID
        private const int SlotCount = 2; // Project J 아이템 슬롯 수

        private const float SpringShoesDuration = 8f; // 기존 스프링 신발 지속 시간
        private const float SpringShoesExtraJumpVelocity = 8f; // 기존 추가 점프 수직 속도
        private const float SpringShoesCoyoteProtection = 0.15f; // 첫 점프 직후 오발 방지 시간
        private const float JellyShieldDuration = 4f; // 기존 젤리 보호막 지속 시간

        private const float BalloonHornRange = 6f; // 기존 풍선 나팔 범위
        private const float BalloonHornHalfAngle = 55f; // 기존 풍선 나팔 반각
        private const float BalloonHornForce = 30f; // 기본 Push 12의 2.5배

        private const float BananaForwardDistance = 1.5f; // 기존 설치 전방 거리
        private const float BananaRayStartHeight = 1.5f; // 기존 설치 Ray 시작 높이
        private const float BananaRayDistance = 4f; // 기존 설치 Ray 거리
        private const float BananaMinimumGroundDot = 0.65f; // 기존 설치 경사 제한
        private const float BananaTriggerRadius = 0.65f; // 기존 바나나 Trigger 반경
        private const float BananaLifetime = 15f; // 기존 바나나 수명
        private const float BananaSlipForce = 6.5f; // 기존 바나나 미끄러짐 힘

        private const float WaterGunRange = 12f; // 기존 물총 사거리
        private const float WaterGunCastRadius = 0.3f; // 기존 물총 판정 반경
        private const float WaterGunTickInterval = 0.1f; // 기존 물총 적용 주기
        private const float WaterGunForcePerTick = 0.55f; // 기존 물총 Tick 힘

        private static readonly Vector3 BananaPlacementSize =
            new Vector3(1.3f, 0.3f, 1.3f); // 기존 설치 공간

        private readonly Collider[] bananaOverlapBuffer =
            new Collider[24]; // 바나나 접촉 후보 버퍼

        private ProjectJNetworkExternalGameplay externalGameplay; // 경기 상태와 외력 처리
        private ProjectJNetworkPlayer networkPlayer; // 스프링 신발 추가 점프 대상
        private GameObject bananaVisual; // 각 Peer 로컬 바나나 외형

        [Networked] // 첫 번째 슬롯 Item ID 동기화
        private int NetworkSlotLeftItemId
        {
            get;
            set;
        }

        [Networked] // 두 번째 슬롯 Item ID 동기화
        private int NetworkSlotRightItemId
        {
            get;
            set;
        }

        [Networked] // 현재 선택 슬롯 동기화
        private int NetworkSelectedSlotIndex
        {
            get;
            set;
        }

        [Networked] // 인벤토리 변경 횟수 동기화
        private int NetworkInventoryRevision
        {
            get;
            set;
        }

        [Networked] // 스프링 신발 지속 시간 동기화
        private TickTimer NetworkSpringShoesTimer
        {
            get;
            set;
        }

        [Networked] // 스프링 신발 추가 점프 사용 가능 여부
        private NetworkBool NetworkSpringExtraJumpAvailable
        {
            get;
            set;
        }

        [Networked] // 스프링 신발 공중 체류 시간
        private float NetworkSpringAirborneSeconds
        {
            get;
            set;
        }

        [Networked] // 젤리 보호막 지속 시간 동기화
        private TickTimer NetworkJellyShieldTimer
        {
            get;
            set;
        }

        [Networked] // 물총 Hold 상태 동기화
        private NetworkBool NetworkWaterGunActive
        {
            get;
            set;
        }

        [Networked] // 물총 다음 힘 적용 Tick 동기화
        private TickTimer NetworkWaterGunTickTimer
        {
            get;
            set;
        }

        [Networked] // 바나나 설치 활성 상태 동기화
        private NetworkBool NetworkBananaActive
        {
            get;
            set;
        }

        [Networked] // 바나나 설치 위치 동기화
        private Vector3 NetworkBananaPosition
        {
            get;
            set;
        }

        [Networked] // 바나나 설치 바닥 Normal 동기화
        private Vector3 NetworkBananaNormal
        {
            get;
            set;
        }

        [Networked] // 바나나 수명 동기화
        private TickTimer NetworkBananaLifetimeTimer
        {
            get;
            set;
        }

        [Networked] // 바나나 설치·해제 변경 횟수
        private int NetworkBananaRevision
        {
            get;
            set;
        }

        [Networked] // 마지막 사용 성공 Item ID
        private int NetworkLastUsedItemId
        {
            get;
            set;
        }

        [Networked] // 아이템 사용 성공 횟수
        private int NetworkUseSuccessCount
        {
            get;
            set;
        }

        [Networked] // 아이템 사용 실패 횟수
        private int NetworkUseFailCount
        {
            get;
            set;
        }

        public int SlotLeftItemId => NetworkSlotLeftItemId; // 첫 번째 슬롯 조회
        public int SlotRightItemId => NetworkSlotRightItemId; // 두 번째 슬롯 조회
        public int SelectedSlotIndex => NetworkSelectedSlotIndex; // 선택 슬롯 조회
        public int SelectedItemId => GetItemId(NetworkSelectedSlotIndex); // 선택 아이템 조회
        public int InventoryRevision => NetworkInventoryRevision; // 인벤토리 Revision 조회
        public int LastUsedItemId => NetworkLastUsedItemId; // 마지막 사용 아이템 조회
        public int UseSuccessCount => NetworkUseSuccessCount; // 사용 성공 횟수 조회
        public int UseFailCount => NetworkUseFailCount; // 사용 실패 횟수 조회
        public bool IsWaterGunActive => NetworkWaterGunActive; // 물총 Hold 상태 조회
        public bool IsBananaActive => NetworkBananaActive; // 바나나 설치 상태 조회

        public int OwnerIndex =>
            Object != null && Object.IsValid
                ? Object.InputAuthority.AsIndex
                : -1; // Player Index 조회

        public bool CanReceiveWorldItem =>
            Object != null &&
            Object.IsValid &&
            Object.HasStateAuthority &&
            externalGameplay != null &&
            externalGameplay.GameplayInputAllowed; // Host 승인 가능 여부

        public bool IsSpringShoesActive =>
            IsTimerActive(NetworkSpringShoesTimer); // 스프링 신발 활성 여부

        public bool IsJellyShieldActive =>
            IsTimerActive(NetworkJellyShieldTimer); // 젤리 보호막 활성 여부

        public float SpringShoesRemaining =>
            GetRemainingTime(NetworkSpringShoesTimer); // 스프링 신발 남은 시간

        public float JellyShieldRemaining =>
            GetRemainingTime(NetworkJellyShieldTimer); // 젤리 보호막 남은 시간

        public override void Spawned()
        {
            ResolveReferences(); // 필수 참조 준비

            if (!Object.HasStateAuthority)
            {
                return; // Client 초기 Networked 쓰기 차단
            }

            NetworkSlotLeftItemId = EmptyItemId; // 첫 슬롯 초기화
            NetworkSlotRightItemId = EmptyItemId; // 두 번째 슬롯 초기화
            NetworkSelectedSlotIndex = 0; // 첫 슬롯 기본 선택
            NetworkInventoryRevision = 0; // 인벤토리 Revision 초기화
            NetworkSpringShoesTimer = TickTimer.None; // 스프링 신발 초기화
            NetworkSpringExtraJumpAvailable = false; // 추가 점프 초기화
            NetworkSpringAirborneSeconds = 0f; // 공중 시간 초기화
            NetworkJellyShieldTimer = TickTimer.None; // 보호막 초기화
            NetworkWaterGunActive = false; // 물총 Hold 초기화
            NetworkWaterGunTickTimer = TickTimer.None; // 물총 Tick 초기화
            NetworkBananaActive = false; // 바나나 초기화
            NetworkBananaPosition = Vector3.zero; // 바나나 위치 초기화
            NetworkBananaNormal = Vector3.up; // 바나나 Normal 초기화
            NetworkBananaLifetimeTimer = TickTimer.None; // 바나나 수명 초기화
            NetworkBananaRevision = 0; // 바나나 Revision 초기화
            NetworkLastUsedItemId = EmptyItemId; // 마지막 사용 초기화
            NetworkUseSuccessCount = 0; // 성공 횟수 초기화
            NetworkUseFailCount = 0; // 실패 횟수 초기화
            InitializeFireworkAuthority(); // 폭죽 준비 상태 초기화
            InitializeFeatherShoesAuthority(); // 깃털 신발 효과 초기화
            InitializeJetpackAuthority(); // 제트팩 연료 상태 초기화
            InitializeGiantBalloonAuthority(); // 거대 풍선 상승·하강 상태 초기화
            InitializeCartAuthority(); // 카트 탑승 상태 초기화
            InitializeHammerAuthority(); // 망치 강화 상태 초기화
            InitializePufferBalloonSuitAuthority(); // 복어 풍선옷 효과 초기화
            InitializeInkOctopusAuthority(); // 먹물 문어 상태 초기화
            InitializeFishingRodAuthority(); // 낚시대 연결 상태 초기화
            InitializeGrapplingHookAuthority(); // 갈고리 연결 상태 초기화
            InitializeSoapBubbleAuthority(); // 비눗방울 이동 제한 상태 초기화
            InitializeSnowballAuthority(); // 눈덩이 감속 상태 초기화
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            DestroyBananaVisual(); // Player 제거 시 로컬 바나나 외형 정리
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority)
            {
                return; // State Authority만 아이템 판정
            }

            ResolveReferences(); // 참조 유실 보정
            UpdateTimedEffectsAuthority(); // 지속 효과 상태 보정
            UpdateBananaAuthority(); // 설치 바나나 수명·접촉 판정
            UpdateFireworkAuthority(); // 폭죽 준비·취소·발동 판정
            UpdatePufferBalloonSuitAuthority(); // 복어 풍선옷 근접 자동 밀치기 판정
            UpdateFishingRodAuthority(); // 낚시대 연결·당김 상태 판정
            UpdateGrapplingHookAuthority(); // 갈고리 자기 이동 상태 판정
            UpdateSoapBubbleLifetimeAuthority(); // 비눗방울 시간·경기 상태 판정
            UpdateGiantBalloonAuthority(); // 거대 풍선 6초 상승·1.5초 하강 단계 갱신

            if (
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed
            )
            {
                StopWaterGunAuthority(); // 경기 잠금 시 Hold 효과 종료
                return; // 경기 전·완주 후·종료 후 입력 차단
            }

            if (!GetInput<ProjectJNetworkInput>(out ProjectJNetworkInput input))
            {
                return; // 입력 없음 처리
            }

            UpdateSoapBubbleJumpInputAuthority(input); // 점프 눌림 시작 6회 조기 탈출 판정

            if (input.Buttons.IsSet(ProjectJNetworkButton.ItemSlotLeft))
            {
                SelectSlotAuthority(0); // Q 첫 슬롯 선택
            }

            if (input.Buttons.IsSet(ProjectJNetworkButton.ItemSlotRight))
            {
                SelectSlotAuthority(1); // E 두 번째 슬롯 선택
            }

            UpdateSpringShoesJumpAuthority(input); // 추가 점프 입력 판정

            if (input.Buttons.IsSet(ProjectJNetworkButton.ItemUse))
            {
                TryUseSelectedItemWithStackAuthority(); // Stack 포함 선택 아이템 사용
            }

            UpdateWaterGunAuthority(
                input.Buttons.IsSet(ProjectJNetworkButton.ItemUseHeld)
            ); // 우클릭 Hold / Release 처리
        }

        private void LateUpdate()
        {
            if (Object == null || !Object.IsValid)
            {
                DestroyBananaVisual(); // Spawn 전·Despawn 후 로컬 외형 정리
                return;
            }

            UpdateBananaVisual(); // 모든 Peer에서 Networked 설치 상태 표현
        }

        public int GetItemId(int slotIndex)
        {
            if (slotIndex == 0)
            {
                return NetworkSlotLeftItemId; // 첫 슬롯 반환
            }

            if (slotIndex == 1)
            {
                return NetworkSlotRightItemId; // 두 번째 슬롯 반환
            }

            return EmptyItemId; // 잘못된 슬롯 처리
        }

        public bool TryStoreItemAuthority(
            ItemDefinition definition,
            out int storedSlotIndex
        )
        {
            storedSlotIndex = -1; // 실패 기본값

            if (
                !CanReceiveWorldItem ||
                !ProjectJNetworkItemCatalog.TryGetNetworkId(
                    definition,
                    out int networkItemId
                )
            )
            {
                return false; // 권한 또는 등록되지 않은 Item 처리
            }

            if (NetworkSlotLeftItemId == EmptyItemId)
            {
                NetworkSlotLeftItemId = networkItemId; // 첫 빈 슬롯 저장
                storedSlotIndex = 0; // 저장 위치 기록
                NetworkInventoryRevision++; // Revision 증가
                return true;
            }

            if (NetworkSlotRightItemId == EmptyItemId)
            {
                NetworkSlotRightItemId = networkItemId; // 두 번째 빈 슬롯 저장
                storedSlotIndex = 1; // 저장 위치 기록
                NetworkInventoryRevision++; // Revision 증가
                return true;
            }

            storedSlotIndex = Mathf.Clamp(
                NetworkSelectedSlotIndex,
                0,
                SlotCount - 1
            ); // 가득 찬 경우 선택 슬롯 교체

            SetSlotItemIdAuthority(storedSlotIndex, networkItemId); // 선택 슬롯 교체
            NetworkInventoryRevision++; // Revision 증가
            return true;
        }

        public bool BlocksExternalForce(ProjectJExternalForceSource source)
        {
            if (!IsJellyShieldActive)
            {
                return false; // 보호막 비활성
            }

            return
                source == ProjectJExternalForceSource.Push ||
                source == ProjectJExternalForceSource.Item; // Push·적대 아이템 외력 차단
        }

        public void ClearAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return; // Client 직접 초기화 차단
            }

            NetworkSlotLeftItemId = EmptyItemId; // 첫 슬롯 비우기
            NetworkSlotRightItemId = EmptyItemId; // 두 번째 슬롯 비우기
            NetworkSelectedSlotIndex = 0; // 첫 슬롯 선택 복원
            NetworkInventoryRevision++; // Revision 증가
            NetworkSpringShoesTimer = TickTimer.None; // 스프링 효과 제거
            NetworkSpringExtraJumpAvailable = false; // 추가 점프 제거
            NetworkSpringAirborneSeconds = 0f; // 공중 시간 제거
            NetworkJellyShieldTimer = TickTimer.None; // 보호막 제거
            StopWaterGunAuthority(); // 물총 종료
            DeactivateBananaAuthority(); // 설치 바나나 제거
            CancelFireworkPreparationAuthority(false); // 폭죽 준비 상태 제거
            ClearFeatherShoesAuthority(); // 깃털 신발 효과 제거
            ClearJetpackAuthority(); // 제트팩 효과 제거
            ClearGiantBalloonAuthority(); // 거대 풍선 상승·하강 상태 제거
            ClearCartAuthority(); // 카트 탑승 및 소유 카트 제거
            ClearHammerAuthority(); // 망치 효과 제거
            ClearPufferBalloonSuitAuthority(); // 복어 풍선옷 효과 제거
            ClearInkOctopusAuthority(); // 먹물 문어 효과 제거
            ClearFishingRodAuthority(); // 낚시대 연결 상태 제거
            ClearGrapplingHookAuthority(); // 갈고리 연결 상태 제거
            ClearSoapBubbleAuthority(); // 비눗방울 이동 제한 상태 제거
            ClearSnowballSlowAuthority(); // 눈덩이 감속 효과 제거
        }

        internal void HandleRespawnAuthority()
        {
            CancelFireworkPreparationAuthority(); // 준비 중인 폭죽 취소
            ClearFeatherShoesAuthority(); // 부활 시 깃털 신발 효과 제거
            ClearJetpackAuthority(); // 부활 시 제트팩 효과 즉시 제거
            ClearGiantBalloonAuthority(); // 부활 시 거대 풍선 상태 즉시 제거
            ClearCartAuthority(); // 부활 시 카트 탑승 및 소유 카트 제거
            ClearHammerAuthority(); // 부활 시 망치 효과 즉시 제거
            ClearPufferBalloonSuitAuthority(); // 부활 시 복어 풍선옷 효과 즉시 제거
            ClearInkOctopusAuthority(); // 부활 시 먹물 문어 효과 즉시 제거
            ClearFishingRodAuthority(); // 부활 시 낚시대 연결 즉시 제거
            ClearGrapplingHookAuthority(); // 부활 시 갈고리 연결 즉시 제거
            ClearSoapBubbleAuthority(); // 부활 시 비눗방울 이동 제한 즉시 제거
            ClearSnowballSlowAuthority(); // 부활 시 눈덩이 감속 제거
        }

        private bool TryUseSelectedItemAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed
            )
            {
                NetworkUseFailCount++; // 권한·경기 상태 실패 기록
                return false;
            }

            int slotIndex = Mathf.Clamp(
                NetworkSelectedSlotIndex,
                0,
                SlotCount - 1
            ); // 현재 선택 슬롯 보정

            int itemId = GetItemId(slotIndex); // 현재 선택 Item 조회

            if (itemId == EmptyItemId)
            {
                NetworkUseFailCount++; // 빈 슬롯 사용 실패 기록
                return false;
            }

            bool success;

            switch ((ProjectJNetworkItemId)itemId)
            {
                case ProjectJNetworkItemId.SpringShoes:
                    success = UseSpringShoesAuthority();
                    break;

                case ProjectJNetworkItemId.JellyShield:
                    success = UseJellyShieldAuthority();
                    break;

                case ProjectJNetworkItemId.BananaCushion:
                    success = TryUseBananaCushionAuthority();
                    break;

                case ProjectJNetworkItemId.BalloonHorn:
                    success = UseBalloonHornAuthority();
                    break;

                case ProjectJNetworkItemId.WaterGun:
                    success = UseWaterGunAuthority();
                    break;

                case ProjectJNetworkItemId.Firework:
                    success = UseFireworkAuthority();
                    break;

                case ProjectJNetworkItemId.FeatherShoes:
                    success = UseFeatherShoesAuthority();
                    break;

                case ProjectJNetworkItemId.Jetpack: // 제트팩 선택 상태
                    success = UseJetpackAuthority(); // 서버 권한 5초 연료 활성화
                    break; // 제트팩 분기 종료

                case ProjectJNetworkItemId.Hammer: // 망치 선택 상태
                    success = UseHammerAuthority(); // 서버 권한 6초 밀치기 강화 활성화
                    break; // 망치 분기 종료

                case ProjectJNetworkItemId.Bomb: // 폭탄 선택 상태
                    success = UseBombAuthority(); // 서버 권한 폭탄 투척
                    break; // 폭탄 분기 종료

                case ProjectJNetworkItemId.PufferBalloonSuit: // 복어 풍선옷 선택 상태
                    success = UsePufferBalloonSuitAuthority(); // 서버 권한 5초 근접 자동 밀치기 활성화
                    break; // 복어 풍선옷 분기 종료

                case ProjectJNetworkItemId.InkOctopus: // 먹물 문어 선택 상태
                    success = UseInkOctopusAuthority(); // 서버 권한 먹물 투사체 발사
                    break; // 먹물 문어 분기 종료

                case ProjectJNetworkItemId.FishingRod: // 낚시대 선택 상태
                    success = UseFishingRodAuthority(); // 서버 권한 직선 조준·당김 연결
                    break; // 낚시대 분기 종료

                case ProjectJNetworkItemId.GrapplingHook: // 갈고리 선택 상태
                    success = UseGrapplingHookAuthority(); // 서버 권한 구조물 부착·자기 이동
                    break; // 갈고리 분기 종료

                case ProjectJNetworkItemId.SoapBubble: // 비눗방울 선택 상태
                    success = UseSoapBubbleAuthority(); // 서버 권한 직선 투사체 발사
                    break; // 비눗방울 분기 종료

                case ProjectJNetworkItemId.SmokeGrenade: // 연막탄 선택 상태
                    success = UseSmokeGrenadeAuthority(); // 서버 권한 포물선 투척
                    break; // 연막탄 분기 종료

                case ProjectJNetworkItemId.Trampoline: // 트램폴린 선택 상태
                    success = UseTrampolineAuthority(); // 서버 권한 발밑 설치
                    break; // 트램폴린 분기 종료

                case ProjectJNetworkItemId.GiantBalloon: // 거대 풍선 선택 상태
                    success = UseGiantBalloonAuthority(); // 서버 권한 6초 상승 상태 시작
                    break; // 거대 풍선 분기 종료

                case ProjectJNetworkItemId.Cart: // 카트 선택 상태
                    success = UseCartAuthority(); // 서버 권한 Route Node 자동 이동 시작
                    break; // 카트 분기 종료

                case ProjectJNetworkItemId.Snowball:
                    success = UseSnowballAuthority();
                    break;

                case ProjectJNetworkItemId.Mine: // 지뢰 선택 상태
                    success = UseMineAuthority(); // 서버 권한 설치 시도
                    break; // 지뢰 분기 종료

                default:
                    success = false;
                    break;
            }

            if (!success)
            {
                NetworkUseFailCount++; // 효과 실패 시 아이템 유지
                return false;
            }

            SetSlotItemIdAuthority(slotIndex, EmptyItemId); // 성공한 슬롯만 소비
            NetworkInventoryRevision++; // 소비 Revision 증가
            NetworkLastUsedItemId = itemId; // 마지막 성공 Item 저장
            NetworkUseSuccessCount++; // 성공 횟수 증가

            Debug.Log(
                "[Project J/Fusion] 73일차 Item 사용 / P" +
                OwnerIndex +
                " / " +
                ProjectJNetworkItemCatalog.GetKey(itemId) +
                " / Slot " +
                (slotIndex + 1),
                this
            );

            return true;
        }

        private bool UseSpringShoesAuthority()
        {
            if (Runner == null)
            {
                return false; // Runner 없음 처리
            }

            NetworkSpringShoesTimer = TickTimer.CreateFromSeconds(
                Runner,
                SpringShoesDuration
            ); // 8초 추가 점프 활성

            NetworkSpringExtraJumpAvailable = true; // 공중 추가 점프 1회 허용
            NetworkSpringAirborneSeconds = 0f; // 첫 점프 직후 오발 방지 시간 초기화
            return true;
        }

        private bool UseJellyShieldAuthority()
        {
            if (Runner == null)
            {
                return false; // Runner 없음 처리
            }

            NetworkJellyShieldTimer = TickTimer.CreateFromSeconds(
                Runner,
                JellyShieldDuration
            ); // 4초 Push·Item 외력 보호

            return true;
        }

        private bool TryUseBananaCushionAuthority()
        {
            if (Runner == null || NetworkBananaActive)
            {
                return false; // 기존 바나나가 남아 있으면 새 설치 보류
            }

            Transform userTransform = transform; // 사용자 Transform 조회

            Vector3 rayOrigin =
                userTransform.position +
                userTransform.forward * BananaForwardDistance +
                Vector3.up * BananaRayStartHeight; // 기존 설치 Ray 위치

            if (
                !Physics.Raycast(
                    rayOrigin,
                    Vector3.down,
                    out RaycastHit hit,
                    BananaRayDistance,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore
                )
            )
            {
                return false; // 바닥 없음 처리
            }

            if (Vector3.Dot(hit.normal, Vector3.up) < BananaMinimumGroundDot)
            {
                return false; // 과도한 경사면 차단
            }

            Bounds placementBounds = new Bounds(
                hit.point + Vector3.up * (BananaPlacementSize.y * 0.5f),
                BananaPlacementSize
            ); // 기존 설치 공간 계산

            if (!ItemPlacementValidator.CanPlace(placementBounds))
            {
                return false; // 다른 설치물·장애물과 겹침 차단
            }

            NetworkBananaPosition = hit.point + hit.normal * 0.08f; // 설치 위치 저장
            NetworkBananaNormal = hit.normal.normalized; // 설치 Normal 저장
            NetworkBananaActive = true; // 바나나 활성
            NetworkBananaLifetimeTimer = TickTimer.CreateFromSeconds(
                Runner,
                BananaLifetime
            ); // 15초 수명 시작
            NetworkBananaRevision++; // 설치 Revision 증가
            return true;
        }

        private bool UseBalloonHornAuthority()
        {
            ProjectJNetworkExternalGameplay[] targets =
                UnityEngine.Object.FindObjectsByType<ProjectJNetworkExternalGameplay>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                ); // 현재 Network Player 후보 조회

            Vector3 origin = transform.position; // 사용자 위치
            Vector3 forward = transform.forward; // 사용 방향
            forward.y = 0f; // 수평 방향 사용

            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward; // 방향 보정
            }

            forward.Normalize(); // 각도 계산 기준 정규화

            for (int index = 0; index < targets.Length; index++)
            {
                ProjectJNetworkExternalGameplay target = targets[index];

                if (
                    target == null ||
                    target == externalGameplay ||
                    target.Object == null ||
                    !target.Object.IsValid
                )
                {
                    continue; // 자기 자신·잘못된 Player 제외
                }

                Vector3 toTarget = target.transform.position - origin;
                toTarget.y = 0f; // 수평 거리·각도 사용

                float distance = toTarget.magnitude;

                if (distance <= 0.01f || distance > BalloonHornRange)
                {
                    continue; // 범위 밖 제외
                }

                Vector3 direction = toTarget / distance; // Target 방향

                if (Vector3.Angle(forward, direction) > BalloonHornHalfAngle)
                {
                    continue; // 전방 110도 밖 제외
                }

                target.TryApplyExternalVelocityChange(
                    ProjectJExternalForceSource.Item,
                    direction * BalloonHornForce
                ); // State Authority 외력 처리에 위임
            }

            return true; // 기존 나팔과 동일하게 Target이 없어도 사용 성공
        }

        private bool UseWaterGunAuthority()
        {
            if (Runner == null)
            {
                return false; // Runner 없음 처리
            }

            NetworkWaterGunActive = true; // Hold 효과 시작
            ApplyWaterGunTickAuthority(); // 사용 순간 첫 Tick 즉시 적용
            NetworkWaterGunTickTimer = TickTimer.CreateFromSeconds(
                Runner,
                WaterGunTickInterval
            ); // 다음 Tick 예약
            return true;
        }

        private void UpdateTimedEffectsAuthority()
        {
            if (Runner == null)
            {
                return;
            }

            if (!IsSpringShoesActive)
            {
                NetworkSpringExtraJumpAvailable = false; // 종료 시 추가 점프 제거
                NetworkSpringAirborneSeconds = 0f; // 공중 시간 초기화
            }
            else if (networkPlayer != null)
            {
                if (networkPlayer.IsGrounded)
                {
                    NetworkSpringExtraJumpAvailable = true; // 착지 후 추가 점프 재장전
                    NetworkSpringAirborneSeconds = 0f; // 공중 시간 초기화
                }
                else
                {
                    NetworkSpringAirborneSeconds += Runner.DeltaTime; // 공중 체류 시간 누적
                }
            }

            if (
                externalGameplay != null &&
                externalGameplay.MatchState == ProjectJNetworkMatchState.Finished
            )
            {
                StopWaterGunAuthority(); // 경기 종료 후 Hold 정리
                DeactivateBananaAuthority(); // 경기 종료 후 설치물 정리
            }
        }

        private void UpdateSpringShoesJumpAuthority(ProjectJNetworkInput input)
        {
            if (
                !IsSpringShoesActive ||
                !NetworkSpringExtraJumpAvailable ||
                networkPlayer == null ||
                networkPlayer.IsGrounded ||
                networkPlayer.IsCrouching ||
                NetworkSpringAirborneSeconds <= SpringShoesCoyoteProtection ||
                !input.Buttons.IsSet(ProjectJNetworkButton.Jump)
            )
            {
                return; // 추가 점프 조건 미충족
            }

            if (
                networkPlayer.TrySetItemVerticalVelocityAuthority(
                    SpringShoesExtraJumpVelocity
                )
            )
            {
                NetworkSpringExtraJumpAvailable = false; // 공중 추가 점프 1회 소비
            }
        }

        private void UpdateWaterGunAuthority(bool useHeld)
        {
            if (!NetworkWaterGunActive)
            {
                return; // 물총 미사용
            }

            if (!useHeld)
            {
                StopWaterGunAuthority(); // 우클릭 해제 즉시 종료
                return;
            }

            if (
                Runner == null ||
                !NetworkWaterGunTickTimer.ExpiredOrNotRunning(Runner)
            )
            {
                return; // 다음 0.1초 Tick 대기
            }

            ApplyWaterGunTickAuthority(); // 물총 힘 적용
            NetworkWaterGunTickTimer = TickTimer.CreateFromSeconds(
                Runner,
                WaterGunTickInterval
            ); // 다음 Tick 예약
        }

        private void ApplyWaterGunTickAuthority()
        {
            Vector3 forward = transform.forward; // 발사 방향

            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward; // 방향 보정
            }

            forward.Normalize(); // SphereCast 기준 정규화

            Vector3 origin =
                transform.position +
                Vector3.up * 1.2f +
                forward * 0.4f; // 기존 물총 시작점

            RaycastHit[] hits = Physics.SphereCastAll(
                origin,
                WaterGunCastRadius,
                forward,
                WaterGunRange,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            ); // 전방 충돌 후보 조회

            Array.Sort(
                hits,
                (left, right) => left.distance.CompareTo(right.distance)
            ); // 가까운 충돌부터 처리

            for (int index = 0; index < hits.Length; index++)
            {
                Collider hitCollider = hits[index].collider;

                if (hitCollider == null)
                {
                    continue;
                }

                if (
                    hitCollider.transform == transform ||
                    hitCollider.transform.IsChildOf(transform)
                )
                {
                    continue; // 자기 Collider 제외
                }

                ProjectJNetworkExternalGameplay target =
                    hitCollider.GetComponentInParent<ProjectJNetworkExternalGameplay>();

                if (target != null)
                {
                    if (target != externalGameplay)
                    {
                        target.TryApplyExternalVelocityChange(
                            ProjectJExternalForceSource.Item,
                            forward * WaterGunForcePerTick
                        ); // 첫 Player에 물총 외력 적용
                    }

                    return; // Player 또는 보호막에 닿으면 관통하지 않음
                }

                if (!hitCollider.isTrigger)
                {
                    return; // 일반 지형에 닿으면 사거리 종료
                }
            }
        }

        private void StopWaterGunAuthority()
        {
            NetworkWaterGunActive = false; // Hold 상태 종료
            NetworkWaterGunTickTimer = TickTimer.None; // 반복 Tick 종료
        }

        private void UpdateBananaAuthority()
        {
            if (!NetworkBananaActive || Runner == null)
            {
                return; // 설치 바나나 없음
            }

            if (NetworkBananaLifetimeTimer.ExpiredOrNotRunning(Runner))
            {
                DeactivateBananaAuthority(); // 15초 수명 종료
                return;
            }

            int overlapCount = Physics.OverlapSphereNonAlloc(
                NetworkBananaPosition,
                BananaTriggerRadius,
                bananaOverlapBuffer,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            ); // Host에서 접촉 Player 검색

            for (int index = 0; index < overlapCount; index++)
            {
                Collider overlap = bananaOverlapBuffer[index];

                if (overlap == null)
                {
                    continue;
                }

                ProjectJNetworkExternalGameplay target =
                    overlap.GetComponentInParent<ProjectJNetworkExternalGameplay>();

                if (
                    target == null ||
                    target == externalGameplay ||
                    target.Object == null ||
                    !target.Object.IsValid
                )
                {
                    continue; // 소유자·비 Player 제외
                }

                int targetIndex = target.Object.InputAuthority.AsIndex;
                float sideSign = targetIndex % 2 == 0 ? 1f : -1f; // 동기화 가능한 좌우 방향 선택

                Vector3 slipDirection =
                    (
                        target.transform.right * sideSign -
                        target.transform.forward * 0.35f
                    ).normalized; // 기존 바나나 미끄러짐 방향

                bool applied = target.TryApplyExternalVelocityChange(
                    ProjectJExternalForceSource.Item,
                    slipDirection * BananaSlipForce
                ); // 보호·보호막 포함 State Authority 판정

                if (!applied)
                {
                    continue; // 보호 중이면 바나나를 유지
                }

                DeactivateBananaAuthority(); // 첫 성공 접촉 후 제거
                return;
            }
        }

        private void DeactivateBananaAuthority()
        {
            if (!NetworkBananaActive)
            {
                return;
            }

            NetworkBananaActive = false; // 설치 상태 해제
            NetworkBananaLifetimeTimer = TickTimer.None; // 수명 종료
            NetworkBananaRevision++; // 해제 Revision 증가
        }

        private void SelectSlotAuthority(int slotIndex)
        {
            if (
                slotIndex < 0 ||
                slotIndex >= SlotCount ||
                NetworkSelectedSlotIndex == slotIndex
            )
            {
                return; // 잘못된 값 또는 동일 슬롯 처리
            }

            NetworkSelectedSlotIndex = slotIndex; // 선택 슬롯 확정
            NetworkInventoryRevision++; // Revision 증가
        }

        private void SetSlotItemIdAuthority(int slotIndex, int itemId)
        {
            if (slotIndex == 0)
            {
                NetworkSlotLeftItemId = itemId; // 첫 슬롯 변경
                return;
            }

            NetworkSlotRightItemId = itemId; // 두 번째 슬롯 변경
        }

        private void ResolveReferences()
        {
            if (externalGameplay == null)
            {
                externalGameplay = GetComponent<ProjectJNetworkExternalGameplay>(); // 경기 상태 조회
            }

            if (networkPlayer == null)
            {
                networkPlayer = GetComponent<ProjectJNetworkPlayer>(); // Network Player 조회
            }
        }

        private bool IsTimerActive(TickTimer timer)
        {
            return
                Runner != null &&
                !timer.ExpiredOrNotRunning(Runner); // TickTimer 실행 여부 반환
        }

        private float GetRemainingTime(TickTimer timer)
        {
            if (Runner == null)
            {
                return 0f;
            }

            float? remaining = timer.RemainingTime(Runner); // Fusion 남은 시간 조회
            return remaining ?? 0f;
        }

        private void UpdateBananaVisual()
        {
            if (NetworkBananaActive)
            {
                EnsureBananaVisual(); // 필요 시 로컬 외형 생성

                if (bananaVisual == null)
                {
                    return;
                }

                Vector3 normal = NetworkBananaNormal.sqrMagnitude > 0.0001f
                    ? NetworkBananaNormal.normalized
                    : Vector3.up; // 잘못된 Normal 보정

                bananaVisual.transform.position = NetworkBananaPosition; // 동기화 위치 적용
                bananaVisual.transform.rotation = Quaternion.FromToRotation(
                    Vector3.up,
                    normal
                ); // 바닥 기울기 적용
                return;
            }

            DestroyBananaVisual(); // 비활성 상태 외형 정리
        }

        private void EnsureBananaVisual()
        {
            if (bananaVisual != null)
            {
                return; // 이미 생성됨
            }

            bananaVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // 테스트용 쿠션 외형 생성
            bananaVisual.name = "Network Banana Cushion P" + OwnerIndex; // 디버그 이름 지정
            bananaVisual.transform.localScale = new Vector3(0.85f, 0.08f, 0.85f); // 기존 테스트 외형 크기

            Collider visualCollider = bananaVisual.GetComponent<Collider>();

            if (visualCollider != null)
            {
                UnityEngine.Object.Destroy(visualCollider); // 실제 판정은 State Authority가 담당
            }

            Renderer renderer = bananaVisual.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.material.color = new Color(1f, 0.82f, 0.08f, 1f); // 기존 바나나 테스트 색상
            }
        }

        private void DestroyBananaVisual()
        {
            if (bananaVisual == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(bananaVisual); // 로컬 외형 제거
            bananaVisual = null;
        }

        private void OnGUI()
        {
            if (!ProjectJDebugOverlayController.IsVisible) // 통합 패널 선택 상태 확인
            {
                return; // 독립 진단창 출력 차단
            }

            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasInputAuthority
            )
            {
                return; // 로컬 소유 Player만 표시
            }

            GUILayout.BeginArea(
                new Rect(12f, 650f, 390f, 275f), // 제트팩 상태 포함 진단 영역 높이
                GUI.skin.box
            ); // 73일차 개발 확인 영역

            GUILayout.Label("DAY 73 NETWORK ITEMS");
            GUILayout.Label(
                "Slot 1 [Q] : " +
                ProjectJNetworkItemCatalog.GetDisplayName(NetworkSlotLeftItemId)
            );
            GUILayout.Label(
                "Slot 2 [E] : " +
                ProjectJNetworkItemCatalog.GetDisplayName(NetworkSlotRightItemId)
            );
            GUILayout.Label("Selected : " + (NetworkSelectedSlotIndex + 1));
            GUILayout.Label(
                "Spring Shoes : " +
                (IsSpringShoesActive ? SpringShoesRemaining.ToString("0.0") + "s" : "OFF")
            );
            GUILayout.Label(
                "Jelly Shield : " +
                (IsJellyShieldActive ? JellyShieldRemaining.ToString("0.0") + "s" : "OFF")
            );
            GUILayout.Label("Banana : " + (NetworkBananaActive ? "ACTIVE" : "OFF"));
            GUILayout.Label("Water Gun : " + (NetworkWaterGunActive ? "HOLD" : "OFF"));
            GUILayout.Label(
                "Firework : " +
                (IsFireworkPreparing ? FireworkRemaining.ToString("0.0") + "s" : "OFF") +
                " / Blast " + FireworkActivationCount +
                " / Cancel " + FireworkCancellationCount +
                " / Targets " + FireworkLastTargetCount
            );
            GUILayout.Label(
                "Feather Shoes : " +
                (IsFeatherShoesActive ? FeatherShoesRemaining.ToString("0.0") + "s" : "OFF")
            );
            GUILayout.Label( // 제트팩 Networked 연료 상태 표시
                "Jetpack : " + // 제트팩 진단 라벨
                (IsJetpackActive ? JetpackRemaining.ToString("0.0") + "s" : "OFF") // 남은 연료 또는 종료 상태
            );
            GUILayout.Label(
                "Snowball Slow : " +
                (IsSnowballSlowed ? SnowballSlowRemaining.ToString("0.0") + "s" : "OFF")
            );
            GUILayout.Label(
                "Last Use : " +
                ProjectJNetworkItemCatalog.GetDisplayName(NetworkLastUsedItemId) +
                " / Success " + NetworkUseSuccessCount +
                " / Fail " + NetworkUseFailCount
            );

            GUILayout.EndArea();
        }
    }
}
