using System; // 조준 상태 문구 전달 기능 참조
using ProjectJ.Data; // 저격 물총 아이템 데이터 참조
using ProjectJ.Gameplay; // 경기 종료 상태 기능 참조
using ProjectJ.Player; // 플레이어 상태와 외부 힘 기능 참조
using UnityEngine; // Unity 카메라와 물리 기능 참조
using UnityEngine.InputSystem; // 마우스와 게임패드 조준 입력 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 플레이어당 저격 조준 관리자 한 개만 허용
    [RequireComponent(typeof(PlayerItemInventory))] // 지정 슬롯 소비용 인벤토리 필수 지정
    public sealed class PlayerSniperWaterGunController : MonoBehaviour // 저격 물총 조준과 발사 관리자 선언
    { // 저격 물총 조준 관리자 묶음
        [SerializeField] private PlayerItemInventory inventory; // 조준 완료 후 아이템 소비 대상 저장
        [SerializeField] private PlayerStateController playerStateController; // 행동 가능 상태 제공자 저장
        [SerializeField] private PlayerRespawnController respawnController; // 부활 상태 제공자 저장
        [SerializeField] private PrototypeMatchController matchController; // 경기 종료 상태 제공자 저장
        [SerializeField] private Camera aimCamera; // 배율과 발사 방향 기준 카메라 저장

        private ItemDataDefinition aimingItem; // 현재 조준 중인 저격 물총 데이터 저장
        private int aimingSlotIndex = -1; // 발사 시 소비할 원래 슬롯 번호 저장
        private Transform itemUseOrigin; // 저격 물총 발사 시작 위치 저장
        private LayerMask collisionLayers = ~0; // 저격 판정 충돌 Layer 저장
        private Action<string> statusCallback; // HUD 상태 문구 전달 함수 저장
        private float originalFieldOfView; // 조준 전 카메라 시야각 저장
        private float currentZoom = P2ItemRules.MinimumSniperZoom; // 현재 저격 배율 저장
        private int aimStartedFrame = -1; // 시작 입력과 취소 입력 중복 방지 프레임 저장

        public bool IsAiming => aimingItem != null; // 현재 저격 물총 조준 여부 반환
        public float CurrentZoom => currentZoom; // 현재 저격 배율 반환

        private void Awake() // 실행 시작 시 조준 참조 준비
        { // 조준 참조 준비 처리
            ResolveReferences(); // 같은 플레이어와 Scene 기반 누락 참조 자동 연결
        } // 조준 참조 준비 처리 종료

        private void Update() // 조준 중 발사와 배율과 취소 입력 갱신
        { // 저격 조준 프레임 처리
            if (!IsAiming) // 조준 비활성 여부 확인
            { // 조준 입력 생략 처리
                return; // 저격 조준 갱신 종료
            } // 조준 입력 생략 처리 종료

            if (!CanContinueAim()) // 경기와 부활과 슬롯 상태 기반 조준 유지 가능 여부 확인
            { // 조준 강제 취소 처리
                CancelAim("저격 물총 조준이 취소되었습니다."); // 상태 복원과 취소 문구 표시
                return; // 저격 조준 갱신 종료
            } // 조준 강제 취소 처리 종료

            UpdateZoomInput(); // 마우스 휠과 게임패드 버튼 배율 변경 처리

            if (WasFirePressedThisFrame()) // 좌클릭 또는 RT 발사 입력 확인
            { // 저격 물총 발사 처리
                Fire(); // 조준선 첫 충돌에 강한 밀치기 적용
                return; // 발사 후 조준 갱신 종료
            } // 저격 물총 발사 처리 종료

            if (Time.frameCount != aimStartedFrame && WasCancelPressedThisFrame()) // 시작 프레임을 제외한 취소 입력 확인
            { // 저격 조준 취소 처리
                CancelAim("저격 물총 사용을 취소했습니다."); // 아이템을 보존하고 조준 종료
            } // 저격 조준 취소 처리 종료
        } // 저격 조준 프레임 처리 종료

        private void OnDisable() // 조준 관리자 비활성화 시 카메라 복원
        { // 조준 비활성화 정리 처리
            CancelAim(string.Empty); // 아이템을 보존하고 조준 상태 제거
        } // 조준 비활성화 정리 처리 종료

        public bool TryBeginAim(ItemDataDefinition itemData, int slotIndex, Transform newItemUseOrigin, LayerMask newCollisionLayers, Action<string> newStatusCallback) // 저격 물총 조준 모드 시작 시도
        { // 저격 조준 시작 처리
            if (IsAiming || itemData == null || itemData.EffectType != ItemEffectType.SniperWaterGun || inventory == null) // 중복 조준과 데이터와 인벤토리 상태 확인
            { // 저격 조준 시작 불가 처리
                return false; // 조준 시작 실패 반환
            } // 저격 조준 시작 불가 처리 종료

            if (inventory.GetItemAt(slotIndex) != itemData || !CanContinueGameplayAction()) // 원래 슬롯 보유와 행동 가능 상태 확인
            { // 사용할 수 없는 저격 아이템 처리
                return false; // 조준 시작 실패 반환
            } // 사용할 수 없는 저격 아이템 처리 종료

            aimingItem = itemData; // 현재 조준 아이템 저장
            aimingSlotIndex = slotIndex; // 발사 시 소비할 슬롯 번호 저장
            itemUseOrigin = newItemUseOrigin == null ? transform : newItemUseOrigin; // 발사 시작 Transform 저장
            collisionLayers = newCollisionLayers; // 저격 충돌 Layer 저장
            statusCallback = newStatusCallback; // HUD 상태 문구 함수 저장
            currentZoom = P2ItemRules.MinimumSniperZoom; // 최초 저격 배율 적용
            aimStartedFrame = Time.frameCount; // 시작 입력 프레임 저장

            if (aimCamera == null) // 조준 카메라 누락 여부 확인
            { // 메인 카메라 대체 검색 처리
                aimCamera = Camera.main; // 현재 메인 카메라 저장
            } // 메인 카메라 대체 검색 처리 종료

            if (aimCamera != null) // 조준 카메라 존재 여부 확인
            { // 최초 카메라 배율 적용 처리
                originalFieldOfView = aimCamera.fieldOfView; // 조준 전 시야각 저장
                ApplyZoom(); // 최소 배율 시야각 적용
            } // 최초 카메라 배율 적용 처리 종료

            statusCallback?.Invoke("저격 물총 조준: 좌클릭 또는 RT 발사, 휠 배율, 우클릭 또는 B 취소"); // 조준 입력 안내 표시
            return true; // 저격 조준 시작 성공 반환
        } // 저격 조준 시작 처리 종료

        public void CancelAim(string message) // 아이템을 보존하고 현재 조준 종료
        { // 저격 조준 취소 처리
            if (aimCamera != null && originalFieldOfView > 0f) // 복원할 카메라와 시야각 존재 여부 확인
            { // 카메라 시야각 복원 처리
                aimCamera.fieldOfView = originalFieldOfView; // 조준 전 시야각 적용
            } // 카메라 시야각 복원 처리 종료

            bool wasAiming = IsAiming; // 실제 조준 상태 존재 여부 저장
            aimingItem = null; // 현재 조준 아이템 제거
            aimingSlotIndex = -1; // 소비 대상 슬롯 번호 초기화
            itemUseOrigin = null; // 발사 시작 Transform 제거
            originalFieldOfView = 0f; // 저장된 시야각 초기화
            currentZoom = P2ItemRules.MinimumSniperZoom; // 기본 배율 복원
            aimStartedFrame = -1; // 시작 프레임 초기화

            if (wasAiming && !string.IsNullOrWhiteSpace(message)) // 실제 조준 상태와 표시 문구 존재 여부 확인
            { // 조준 종료 문구 표시 처리
                statusCallback?.Invoke(message); // HUD에 취소 또는 종료 문구 전달
            } // 조준 종료 문구 표시 처리 종료

            statusCallback = null; // HUD 상태 문구 함수 참조 제거
        } // 저격 조준 취소 처리 종료

        private void Fire() // 현재 조준 방향으로 저격 물총 발사
        { // 저격 물총 발사 처리
            ItemDataDefinition firedItem = aimingItem; // 취소 전 발사 아이템 데이터 저장
            int firedSlotIndex = aimingSlotIndex; // 취소 전 소비 슬롯 번호 저장
            Action<string> firedStatusCallback = statusCallback; // 취소 전 HUD 문구 함수 저장
            float firedZoom = currentZoom; // 발사 순간 조준 배율 저장

            if (inventory.GetItemAt(firedSlotIndex) != firedItem) // 발사 직전 원래 슬롯의 저격 물총 보유 여부 확인
            { // 발사 전 슬롯 변경 처리
                CancelAim(string.Empty); // 카메라와 조준 상태만 복원
                firedStatusCallback?.Invoke("저격 물총이 원래 슬롯에 없어 발사를 취소했습니다."); // 슬롯 변경 실패 문구 표시
                return; // 밀치기와 아이템 소비 없이 발사 종료
            } // 발사 전 슬롯 변경 처리 종료

            Vector3 origin = GetFireOrigin(); // 현재 카메라 기준 발사 시작 위치 계산
            Vector3 direction = GetFireDirection(); // 현재 카메라 기준 발사 방향 계산
            bool hitTarget = TryApplySniperHit(origin, direction, firedItem); // 조준선 첫 충돌 대상 밀치기 적용

            if (!inventory.TryConsumeItemAt(firedSlotIndex, out ItemDataDefinition consumedItem) || consumedItem != firedItem) // 원래 슬롯의 저격 물총 소비 여부 확인
            { // 예상하지 못한 소비 실패 처리
                CancelAim(string.Empty); // 카메라와 조준 상태만 복원
                firedStatusCallback?.Invoke("저격 물총 수량 갱신에 실패했습니다."); // 소비 실패 문구 표시
                return; // 발사 처리 종료
            } // 예상하지 못한 소비 실패 처리 종료

            CancelAim(string.Empty); // 카메라와 조준 상태 복원
            firedStatusCallback?.Invoke(hitTarget ? $"저격 물총 명중: {firedZoom:0.0}배" : "저격 물총 발사: 대상 없음"); // 명중 또는 빗나감 결과 표시
        } // 저격 물총 발사 처리 종료

        private bool TryApplySniperHit(Vector3 origin, Vector3 direction, ItemDataDefinition itemData) // 조준선 첫 충돌에 강한 밀치기 적용
        { // 저격 적중 판정 처리
            float range = itemData == null ? 50f : Mathf.Max(0.1f, itemData.EffectRange); // 데이터 기반 최대 저격 거리 계산
            float force = itemData == null || itemData.PrimaryValue <= 0f ? P2ItemRules.SniperForce : itemData.PrimaryValue; // 데이터 또는 기본 밀치기 힘 선택
            RaycastHit[] hits = Physics.RaycastAll(origin, direction, range, collisionLayers, QueryTriggerInteraction.Ignore); // 최대 거리 안 모든 조준선 충돌 수집
            Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance)); // 가까운 충돌부터 처리하도록 거리 정렬

            for (int index = 0; index < hits.Length; index++) // 거리순 조준선 충돌 순회
            { // 현재 저격 충돌 확인
                Collider hitCollider = hits[index].collider; // 현재 충돌 Collider 조회

                if (hitCollider == null || hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform)) // 누락 충돌과 사용자 자신 여부 확인
                { // 무시할 저격 충돌 처리
                    continue; // 다음 충돌로 이동
                } // 무시할 저격 충돌 처리 종료

                ExternalForceReceiver receiver = hitCollider.GetComponentInParent<ExternalForceReceiver>(); // 첫 외부 힘 대상 조회

                if (receiver == null || !receiver.CanReceivePush) // 장애물 또는 밀칠 수 없는 대상 여부 확인
                { // 저격선 장애물 차단 처리
                    return false; // 첫 장애물에서 빗나감 반환
                } // 저격선 장애물 차단 처리 종료

                Vector3 pushDirection = Vector3.ProjectOnPlane(direction, Vector3.up).normalized + Vector3.up * 0.15f; // 조준 방향과 약한 위쪽 혼합 밀치기 방향 계산
                return receiver.TryReceiveExternalForce(pushDirection.normalized, force); // 강한 저격 물총 힘 적용 결과 반환
            } // 현재 저격 충돌 확인 종료

            return false; // 아무 충돌도 없는 빗나감 반환
        } // 저격 적중 판정 처리 종료

        private void UpdateZoomInput() // 마우스 휠과 게임패드 버튼 배율 갱신
        { // 저격 배율 입력 처리
            float zoomDelta = 0f; // 현재 프레임 배율 변경량 초기화

            if (Mouse.current != null) // 마우스 연결 여부 확인
            { // 마우스 휠 배율 입력 처리
                float scrollY = Mouse.current.scroll.ReadValue().y; // 현재 휠 세로 입력 조회
                zoomDelta += scrollY > 0.01f ? P2ItemRules.SniperZoomStep : scrollY < -0.01f ? -P2ItemRules.SniperZoomStep : 0f; // 휠 방향 기반 배율 변경량 계산
            } // 마우스 휠 배율 입력 처리 종료

            if (Gamepad.current != null) // 게임패드 연결 여부 확인
            { // 게임패드 배율 입력 처리
                zoomDelta += Gamepad.current.rightShoulder.wasPressedThisFrame ? P2ItemRules.SniperZoomStep : 0f; // 오른쪽 범퍼 배율 증가
                zoomDelta -= Gamepad.current.leftShoulder.wasPressedThisFrame ? P2ItemRules.SniperZoomStep : 0f; // 왼쪽 범퍼 배율 감소
            } // 게임패드 배율 입력 처리 종료

            if (Mathf.Abs(zoomDelta) <= 0.001f) // 배율 변경 입력 없음 여부 확인
            { // 배율 갱신 생략 처리
                return; // 저격 배율 입력 처리 종료
            } // 배율 갱신 생략 처리 종료

            currentZoom = P2ItemRules.ClampSniperZoom(currentZoom + zoomDelta); // 최소와 최대 사이 새 배율 적용
            ApplyZoom(); // 새 배율 카메라 시야각 적용
            statusCallback?.Invoke($"저격 물총 조준 배율: {currentZoom:0.0}배"); // HUD에 현재 배율 표시
        } // 저격 배율 입력 처리 종료

        private void ApplyZoom() // 현재 배율을 카메라 시야각에 적용
        { // 카메라 배율 적용 처리
            if (aimCamera == null || originalFieldOfView <= 0f) // 카메라와 원본 시야각 유효성 확인
            { // 카메라 배율 적용 생략
                return; // 배율 적용 종료
            } // 카메라 배율 적용 생략 종료

            aimCamera.fieldOfView = originalFieldOfView / P2ItemRules.ClampSniperZoom(currentZoom); // 원본 시야각을 현재 배율로 나눈 값 적용
        } // 카메라 배율 적용 처리 종료

        private bool CanContinueAim() // 현재 상태와 원래 슬롯 기반 조준 유지 가능 여부 반환
        { // 조준 유지 가능 여부 검사 처리
            return CanContinueGameplayAction() && inventory != null && inventory.GetItemAt(aimingSlotIndex) == aimingItem; // 경기 행동과 원래 아이템 보유 조건 결과 반환
        } // 조준 유지 가능 여부 검사 처리 종료

        private bool CanContinueGameplayAction() // 부활과 경기 종료를 제외한 행동 가능 여부 반환
        { // 게임 행동 가능 여부 검사 처리
            bool stateAllowsAction = playerStateController == null || playerStateController.CanUseAction; // 플레이어 상태 기반 행동 가능 여부 계산
            bool matchAllowsAction = matchController == null || !matchController.IsMatchFinished; // 경기 종료 기반 행동 가능 여부 계산
            bool respawnAllowsAction = respawnController == null || !respawnController.IsRespawning; // 부활 상태 기반 행동 가능 여부 계산
            return stateAllowsAction && matchAllowsAction && respawnAllowsAction; // 세 상태를 모두 만족한 행동 가능 여부 반환
        } // 게임 행동 가능 여부 검사 처리 종료

        private static bool WasFirePressedThisFrame() // 마우스 좌클릭 또는 게임패드 RT 발사 입력 반환
        { // 발사 입력 확인 처리
            bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame; // 마우스 좌클릭 시작 여부 계산
            bool gamepadPressed = Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame; // 게임패드 RT 시작 여부 계산
            return mousePressed || gamepadPressed; // 둘 중 하나의 발사 입력 결과 반환
        } // 발사 입력 확인 처리 종료

        private static bool WasCancelPressedThisFrame() // 키보드와 마우스와 게임패드 취소 입력 반환
        { // 조준 취소 입력 확인 처리
            bool keyboardPressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame; // Escape 취소 입력 여부 계산
            bool mousePressed = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame; // 우클릭 취소 입력 여부 계산
            bool gamepadPressed = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame; // 게임패드 B 취소 입력 여부 계산
            return keyboardPressed || mousePressed || gamepadPressed; // 세 장치 중 하나의 취소 입력 결과 반환
        } // 조준 취소 입력 확인 처리 종료

        private Vector3 GetFireOrigin() // 현재 카메라와 사용 Transform 기반 발사 시작 위치 반환
        { // 발사 시작 위치 계산 처리
            Transform originTransform = aimCamera == null ? itemUseOrigin : aimCamera.transform; // 카메라 또는 사용 시작 Transform 선택
            return originTransform == null ? transform.position + Vector3.up : originTransform.position + originTransform.forward * 0.2f; // 안전한 발사 시작 위치 반환
        } // 발사 시작 위치 계산 처리 종료

        private Vector3 GetFireDirection() // 현재 카메라와 사용 Transform 기반 발사 방향 반환
        { // 발사 방향 계산 처리
            Transform originTransform = aimCamera == null ? itemUseOrigin : aimCamera.transform; // 카메라 또는 사용 시작 Transform 선택
            Vector3 direction = originTransform == null ? transform.forward : originTransform.forward; // 선택한 Transform 전방 방향 조회
            return direction.sqrMagnitude <= 0.0001f ? transform.forward : direction.normalized; // 안전하게 보정한 발사 방향 반환
        } // 발사 방향 계산 처리 종료

        private void ResolveReferences() // 플레이어와 Scene 기반 누락 참조 자동 연결
        { // 누락 참조 자동 연결 처리
            inventory = inventory == null ? GetComponent<PlayerItemInventory>() : inventory; // 같은 오브젝트 인벤토리 저장
            playerStateController = playerStateController == null ? GetComponent<PlayerStateController>() : playerStateController; // 같은 오브젝트 상태 관리자 저장
            respawnController = respawnController == null ? GetComponent<PlayerRespawnController>() : respawnController; // 같은 오브젝트 부활 관리자 저장
            matchController = matchController == null ? FindFirstObjectByType<PrototypeMatchController>() : matchController; // 현재 Scene 경기 관리자 저장
            aimCamera = aimCamera == null ? Camera.main : aimCamera; // 현재 메인 카메라 또는 기존 조준 카메라 저장
        } // 누락 참조 자동 연결 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(PlayerItemInventory newInventory, PlayerStateController newPlayerStateController, PlayerRespawnController newRespawnController, PrototypeMatchController newMatchController, Camera newAimCamera) // 자동 설정 도구용 저격 조준 참조 연결
        { // Editor 저격 참조 연결 처리
            inventory = newInventory; // 인벤토리 저장
            playerStateController = newPlayerStateController; // 플레이어 상태 관리자 저장
            respawnController = newRespawnController; // 부활 관리자 저장
            matchController = newMatchController; // 경기 관리자 저장
            aimCamera = newAimCamera; // 조준 카메라 저장
        } // Editor 저격 참조 연결 처리 종료
#endif // Editor 전용 설정 종료
    } // 저격 물총 조준 관리자 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
