using Fusion; // Networked와 NetworkObject 사용
using ProjectJ.Items; // 풀 공 정책과 ItemDefinition 사용
using UnityEngine; // Resources와 Vector3 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 네임스페이스
{
    public sealed partial class ProjectJNetworkItemInventory // 111일차 풀 공 Stack 기능
    {
        private const string PoolBallProjectileResourcePath =
            "ProjectJNetworkPoolBallProjectile"; // 투사체 Resources 경로

        private NetworkObject poolBallProjectilePrefab; // 불러온 풀 공 투사체 Prefab

        [Networked] // 첫 번째 슬롯 풀 공 수량 동기화
        private int NetworkPoolBallLeftCount
        {
            get;
            set;
        }

        [Networked] // 두 번째 슬롯 풀 공 수량 동기화
        private int NetworkPoolBallRightCount
        {
            get;
            set;
        }

        public int SlotLeftItemCount => GetSlotStackCount(0); // 첫 번째 슬롯 수량 조회
        public int SlotRightItemCount => GetSlotStackCount(1); // 두 번째 슬롯 수량 조회

        public int GetSlotStackCount(int slotIndex) // 슬롯 표시용 수량 조회
        {
            int itemId = GetItemId(slotIndex); // 현재 슬롯 Item ID 조회

            if (itemId == 0) // 빈 슬롯 확인
            {
                return 0; // 빈 슬롯 수량 반환
            }

            if ((ProjectJNetworkItemId)itemId != ProjectJNetworkItemId.PoolBall) // 일반 아이템 확인
            {
                return 1; // 일반 아이템 단일 수량 반환
            }

            return GetPoolBallStackCount(slotIndex); // 풀 공 Networked 수량 반환
        }

        public string GetSlotDisplayName(int slotIndex) // HUD용 아이템 이름과 수량 조회
        {
            int itemId = GetItemId(slotIndex); // 슬롯 Item ID 조회
            string displayName = ProjectJNetworkItemCatalog.GetDisplayName(itemId); // 기본 표시 이름 조회

            if ((ProjectJNetworkItemId)itemId != ProjectJNetworkItemId.PoolBall) // 풀 공 여부 확인
            {
                return displayName; // 일반 아이템 이름 반환
            }

            return displayName + " ×" + GetPoolBallStackCount(slotIndex); // 풀 공 Stack 수량 표시
        }

        public bool TryStoreWorldItemAuthority( // 월드 Pickup Stack 포함 저장
            ItemDefinition definition, // 획득 Item Definition
            out int storedSlotIndex // 저장된 슬롯 번호
        )
        {
            storedSlotIndex = -1; // 실패 기본 슬롯

            if (
                definition == null || // Definition 누락 확인
                !ProjectJNetworkItemCatalog.TryGetNetworkId( // 네트워크 ID 변환 시도
                    definition, // 획득 Definition 전달
                    out int networkItemId // 변환된 네트워크 ID
                )
            )
            {
                return false; // 잘못된 Definition 획득 차단
            }

            if ((ProjectJNetworkItemId)networkItemId != ProjectJNetworkItemId.PoolBall) // 일반 아이템 확인
            {
                return TryStoreItemAuthority(definition, out storedSlotIndex); // 기존 저장 규칙 유지
            }

            return TryStorePoolBallAuthority(out storedSlotIndex); // 풀 공 Stack 저장 처리
        }

        private bool TryStorePoolBallAuthority(out int storedSlotIndex) // 풀 공 Pickup 1개 저장
        {
            storedSlotIndex = -1; // 실패 기본 슬롯

            if (!CanReceiveWorldItem) // 서버 획득 권한 확인
            {
                return false; // 경기 상태 또는 권한 실패
            }

            int selectedSlotIndex = Mathf.Clamp(SelectedSlotIndex, 0, 1); // 현재 선택 슬롯 보정
            int otherSlotIndex = selectedSlotIndex == 0 ? 1 : 0; // 반대 슬롯 계산

            if (TryAddPoolBallToSlotAuthority(selectedSlotIndex)) // 선택 슬롯 기존 Stack 합산 시도
            {
                storedSlotIndex = selectedSlotIndex; // 합산 슬롯 기록
                return true; // 획득 성공 반환
            }

            if (TryAddPoolBallToSlotAuthority(otherSlotIndex)) // 반대 슬롯 기존 Stack 합산 시도
            {
                storedSlotIndex = otherSlotIndex; // 합산 슬롯 기록
                return true; // 획득 성공 반환
            }

            if (GetItemId(0) == 0) // 첫 번째 빈 슬롯 확인
            {
                StoreNewPoolBallStackAuthority(0); // 첫 슬롯에 1개 Stack 생성
                storedSlotIndex = 0; // 저장 슬롯 기록
                return true; // 획득 성공 반환
            }

            if (GetItemId(1) == 0) // 두 번째 빈 슬롯 확인
            {
                StoreNewPoolBallStackAuthority(1); // 두 번째 슬롯에 1개 Stack 생성
                storedSlotIndex = 1; // 저장 슬롯 기록
                return true; // 획득 성공 반환
            }

            if ((ProjectJNetworkItemId)GetItemId(selectedSlotIndex) == ProjectJNetworkItemId.PoolBall) // 선택 Stack 최대 상태 확인
            {
                return false; // 최대 Stack 보존을 위해 Pickup 유지
            }

            StoreNewPoolBallStackAuthority(selectedSlotIndex); // 기존 선택 슬롯을 풀 공 1개로 교체
            storedSlotIndex = selectedSlotIndex; // 교체 슬롯 기록
            return true; // 획득 성공 반환
        }

        private bool TryAddPoolBallToSlotAuthority(int slotIndex) // 기존 풀 공 Stack 합산
        {
            if ((ProjectJNetworkItemId)GetItemId(slotIndex) != ProjectJNetworkItemId.PoolBall) // 다른 아이템 슬롯 확인
            {
                return false; // Stack 합산 불가
            }

            int currentCount = GetPoolBallStackCount(slotIndex); // 현재 Stack 수량 조회

            if (!ProjectJPoolBallPolicy.CanAddOne(currentCount)) // 최대 5개 여부 확인
            {
                return false; // 최대 Stack 합산 차단
            }

            int nextCount = ProjectJPoolBallPolicy.AddOne(currentCount); // 1개 추가 수량 계산
            SetPoolBallStackCountAuthority(slotIndex, nextCount); // Networked Stack 수량 갱신
            NetworkInventoryRevision++; // 인벤토리 변경 Revision 증가

            Debug.Log(
                "[Project J/Fusion] 111일차 풀 공 Stack 획득 / P" + // Stack 획득 로그 시작
                OwnerIndex + // 소유 Player 번호
                " / Slot " + // 슬롯 구분 문자열
                (slotIndex + 1) + // 사람이 보는 슬롯 번호
                " / Count " + // 수량 구분 문자열
                nextCount, // 갱신된 Stack 수량
                this // 로그 대상 Component
            );

            return true; // Stack 합산 성공
        }

        private void StoreNewPoolBallStackAuthority(int slotIndex) // 새 풀 공 Stack 생성
        {
            SetSlotItemIdAuthority(slotIndex, (int)ProjectJNetworkItemId.PoolBall); // 슬롯 Item ID 지정
            SetPoolBallStackCountAuthority(slotIndex, 1); // 최초 수량 1개 지정
            NetworkInventoryRevision++; // 인벤토리 변경 Revision 증가

            Debug.Log(
                "[Project J/Fusion] 111일차 풀 공 Stack 생성 / P" + // 새 Stack 로그 시작
                OwnerIndex + // 소유 Player 번호
                " / Slot " + // 슬롯 구분 문자열
                (slotIndex + 1) + // 사람이 보는 슬롯 번호
                " / Count 1", // 최초 수량 표시
                this // 로그 대상 Component
            );
        }

        private void TryUseSelectedItemWithStackAuthority() // Stack 아이템 포함 사용 진입점
        {
            if ((ProjectJNetworkItemId)SelectedItemId != ProjectJNetworkItemId.PoolBall) // 일반 아이템 선택 확인
            {
                TryUseSelectedItemAuthority(); // 기존 아이템 사용 처리 유지
                return; // 풀 공 전용 처리 종료
            }

            TryUsePoolBallStackAuthority(); // 풀 공 1개 투척 처리
        }

        private bool TryUsePoolBallStackAuthority() // 풀 공 Stack 1개 사용
        {
            if (
                Object == null || // NetworkObject 누락 확인
                !Object.IsValid || // NetworkObject 유효성 확인
                !Object.HasStateAuthority || // 서버 State Authority 확인
                externalGameplay == null || // 경기 상태 참조 확인
                !externalGameplay.GameplayInputAllowed // 경기 입력 허용 확인
            )
            {
                NetworkUseFailCount++; // 사용 실패 횟수 증가
                return false; // 권한 또는 경기 상태 실패
            }

            int slotIndex = Mathf.Clamp(SelectedSlotIndex, 0, 1); // 현재 선택 슬롯 보정
            int currentCount = GetPoolBallStackCount(slotIndex); // 현재 Stack 수량 조회

            if (!ProjectJPoolBallPolicy.CanConsumeOne(currentCount)) // 투척 가능한 수량 확인
            {
                NetworkUseFailCount++; // 사용 실패 횟수 증가
                return false; // 빈 Stack 투척 차단
            }

            if (!UsePoolBallAuthority()) // 서버 투사체 생성 시도
            {
                NetworkUseFailCount++; // Spawn 실패 횟수 증가
                return false; // 수량 소비 없이 실패
            }

            BreakInvisibilityCloakForSuccessfulItemUseAuthority((int)ProjectJNetworkItemId.PoolBall); // Stack 아이템 성공 사용 시 은신 해제
            int remainingCount = ProjectJPoolBallPolicy.ConsumeOne(currentCount); // 투척 후 남은 수량 계산
            SetPoolBallStackCountAuthority(slotIndex, remainingCount); // 남은 Networked 수량 저장

            if (remainingCount <= 0) // 마지막 1개 소비 여부 확인
            {
                SetSlotItemIdAuthority(slotIndex, 0); // 소진된 슬롯 Item ID 제거
            }

            NetworkInventoryRevision++; // 소비 Revision 증가
            NetworkLastUsedItemId = (int)ProjectJNetworkItemId.PoolBall; // 마지막 성공 Item 저장
            NetworkUseSuccessCount++; // 사용 성공 횟수 증가

            Debug.Log(
                "[Project J/Fusion] 111일차 풀 공 투척 / P" + // 투척 성공 로그 시작
                OwnerIndex + // 소유 Player 번호
                " / Slot " + // 슬롯 구분 문자열
                (slotIndex + 1) + // 사람이 보는 슬롯 번호
                " / Remaining " + // 남은 수량 구분 문자열
                remainingCount, // 투척 후 남은 수량
                this // 로그 대상 Component
            );

            return true; // 투척과 수량 소비 성공
        }

        private bool UsePoolBallAuthority() // 서버 권한 풀 공 투사체 생성
        {
            if (
                Runner == null || // Runner 누락 확인
                !Runner.IsServer || // 서버 실행 여부 확인
                Object == null || // NetworkObject 누락 확인
                !Object.IsValid || // NetworkObject 유효성 확인
                !Object.HasStateAuthority // State Authority 확인
            )
            {
                return false; // 서버 외 투척 차단
            }

            NetworkObject projectilePrefab = ResolvePoolBallProjectilePrefab(); // 투사체 Prefab 조회

            if (projectilePrefab == null) // Prefab 누락 확인
            {
                Debug.LogError(
                    "[Project J/Fusion] 111일차 풀 공 Prefab을 찾을 수 없음", // Prefab 누락 메시지
                    this // 로그 대상 Component
                );

                return false; // 수량 소비 차단
            }

            Vector3 forward = transform.forward; // 플레이어 전방 조회
            forward.y = 0f; // 수평 투척 방향 유지

            if (forward.sqrMagnitude <= 0.0001f) // 잘못된 전방 확인
            {
                forward = Vector3.forward; // 기본 전방 대체
            }

            forward.Normalize(); // 일정한 투사체 속도 유지

            Vector3 spawnPosition =
                transform.position + // 플레이어 기준 위치
                Vector3.up * 1.2f + // 몸 중앙 높이 보정
                forward * 0.9f; // 자기 Collider 앞쪽 생성

            NetworkObject projectileObject = Runner.Spawn(
                projectilePrefab, // 풀 공 NetworkObject Prefab
                spawnPosition, // 생성 위치
                Quaternion.LookRotation(forward), // 투척 방향 회전
                Object.InputAuthority // 투척 사용자 소유권
            );

            if (projectileObject == null) // Spawn 실패 확인
            {
                return false; // 수량 소비 차단
            }

            ProjectJNetworkPoolBallProjectile projectile =
                projectileObject.GetComponent<ProjectJNetworkPoolBallProjectile>(); // 풀 공 동작 Component 조회

            if (
                projectile == null || // Component 누락 확인
                !projectile.ConfigureAuthority(Object.InputAuthority, forward) // 서버 초기화 시도
            )
            {
                Runner.Despawn(projectileObject); // 잘못 생성된 NetworkObject 제거
                return false; // 수량 소비 차단
            }

            return true; // 서버 투척 성공
        }

        private int GetPoolBallStackCount(int slotIndex) // 풀 공 전용 Networked 수량 조회
        {
            int rawCount = slotIndex == 0 // 첫 번째 슬롯 여부 확인
                ? NetworkPoolBallLeftCount // 첫 번째 Networked 수량
                : NetworkPoolBallRightCount; // 두 번째 Networked 수량

            return ProjectJPoolBallPolicy.ClampStackCount(rawCount); // 0~5 범위 보정 반환
        }

        private void SetPoolBallStackCountAuthority(int slotIndex, int count) // 풀 공 Networked 수량 저장
        {
            int clampedCount = ProjectJPoolBallPolicy.ClampStackCount(count); // 저장 수량 0~5 보정

            if (slotIndex == 0) // 첫 번째 슬롯 확인
            {
                NetworkPoolBallLeftCount = clampedCount; // 첫 번째 Stack 수량 저장
                return; // 두 번째 슬롯 처리 생략
            }

            NetworkPoolBallRightCount = clampedCount; // 두 번째 Stack 수량 저장
        }

        private NetworkObject ResolvePoolBallProjectilePrefab() // Resources 풀 공 Prefab 조회
        {
            if (poolBallProjectilePrefab == null) // 캐시된 Prefab 누락 확인
            {
                GameObject projectilePrefabObject = Resources.Load<GameObject>(
                    PoolBallProjectileResourcePath // Resources 파일명 전달
                );

                poolBallProjectilePrefab = projectilePrefabObject != null // Resources 로드 성공 확인
                    ? projectilePrefabObject.GetComponent<NetworkObject>() // NetworkObject Component 조회
                    : null; // 로드 실패 상태 저장
            }

            return poolBallProjectilePrefab; // 캐시된 NetworkObject Prefab 반환
        }
    }
}
