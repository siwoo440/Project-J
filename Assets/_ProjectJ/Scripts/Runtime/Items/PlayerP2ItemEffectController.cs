using System.Collections; // 되감기 시간 재생 기능 참조
using System.Collections.Generic; // 되감기 표본과 소유 효과 목록 기능 참조
using ProjectJ.Data; // P2 아이템 효과 종류 참조
using ProjectJ.Gameplay; // 경기 종료 상태 기능 참조
using ProjectJ.Player; // 플레이어 이동과 부활 기능 참조
using UnityEngine; // Unity 이동과 외형과 물리 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 플레이어당 P2 효과 관리자 한 개만 허용
    [RequireComponent(typeof(PlayerMovementController))] // 플레이어 이동 관리자 필수 지정
    [RequireComponent(typeof(PlayerRewindRecorder))] // 최근 안전 위치 기록기 필수 지정
    public sealed class PlayerP2ItemEffectController : MonoBehaviour // P2 지속 효과와 소유 오브젝트 관리자 선언
    { // P2 효과 관리자 묶음
        [SerializeField] private PlayerMovementController movementController; // 기본 이동 활성 상태 제어 대상 저장
        [SerializeField] private PlayerExternalForceController externalForceController; // 외부 힘 초기화와 상태 대상 저장
        [SerializeField] private PlayerRespawnController respawnController; // 부활 상태 제공자 저장
        [SerializeField] private PrototypeMatchController matchController; // 경기 종료 상태 제공자 저장
        [SerializeField] private PlayerInputReader inputReader; // 카트와 되감기 이동 입력 제한 대상 저장
        [SerializeField] private PlayerRewindRecorder rewindRecorder; // 최근 안전 위치 제공자 저장
        [SerializeField] private CharacterController characterController; // 크기와 카트와 되감기 이동 대상 저장
        [SerializeField] private Transform visualRoot; // 소형화할 플레이어 외형 루트 저장

        private readonly List<PlayerRewindSample> rewindPath = new List<PlayerRewindSample>(); // 최신부터 과거 순서 되감기 경로 저장
        private readonly List<HomingItemEffect> ownedHomingEffects = new List<HomingItemEffect>(); // 현재 소유한 유도탄과 드론 목록 저장
        private Coroutine rewindRoutine; // 진행 중인 되감기 코루틴 저장
        private Collider[] rewindColliders; // 되감기 중 끈 추가 Collider 목록 저장
        private bool[] rewindColliderStates; // 추가 Collider 기존 활성 상태 저장
        private bool movementWasEnabledBeforeRewind; // 되감기 전 이동 컴포넌트 활성 상태 저장
        private bool externalForceWasEnabledBeforeRewind; // 되감기 전 외부 힘 컴포넌트 활성 상태 저장
        private bool rewindStatePrepared; // 되감기용 이동과 충돌 상태 변경 여부 저장
        private float miniatureRemaining; // 소형화 물약 남은 시간 저장
        private float miniatureScale = 1f; // 현재 소형화 배율 저장
        private float originalControllerHeight; // 소형화 전 충돌체 높이 저장
        private float originalControllerRadius; // 소형화 전 충돌체 반지름 저장
        private Vector3 originalControllerCenter; // 소형화 전 충돌체 중심 저장
        private Vector3 originalVisualScale = Vector3.one; // 소형화 전 외형 크기 저장
        private bool miniatureApplied; // 소형화 원본 값 저장 여부 기록
        private float invisibilityRemaining; // 투명 망토 남은 시간 저장
        private Renderer[] visibilityRenderers; // 투명 상태를 적용할 외형 Renderer 목록 저장
        private bool[] rendererEnabledStates; // 투명 전 Renderer 활성 상태 저장
        private bool invisibilityApplied; // Renderer 원본 상태 저장 여부 기록
        private CartPath activeCartPath; // 현재 자동 주행 경로 저장
        private int activeCartWaypointIndex; // 다음 카트 경로 지점 번호 저장
        private float cartRemaining; // 남은 카트 자동 주행 시간 저장
        private float cartSpeed; // 현재 카트 이동 속도 저장
        private bool movementWasEnabledBeforeCart; // 카트 전 이동 컴포넌트 활성 상태 저장

        public bool IsRewinding => rewindRoutine != null; // 되감기 진행 여부 반환
        public bool IsMiniature => miniatureRemaining > 0f; // 소형화 상태 여부 반환
        public bool IsInvisible => invisibilityRemaining > 0f; // 투명 망토 상태 여부 반환
        public bool IsCartRiding => activeCartPath != null && cartRemaining > 0f; // 카트 자동 주행 여부 반환
        public bool CanOwnedEffectsContinue => enabled && gameObject.activeInHierarchy && (matchController == null || !matchController.IsMatchFinished) && (respawnController == null || !respawnController.IsRespawning); // 소유 추적 효과 유지 가능 여부 반환

        private void Awake() // 실행 시작 시 P2 효과 참조 준비
        { // P2 효과 참조 준비 처리
            ResolveReferences(); // 같은 플레이어와 Scene 기반 누락 참조 자동 연결
        } // P2 효과 참조 준비 처리 종료

        private void Update() // P2 지속 시간과 카트 이동 갱신
        { // P2 효과 프레임 처리
            if (!CanOwnedEffectsContinue) // 부활 또는 경기 종료 여부 확인
            { // P2 효과 강제 정리 처리
                ClearAllEffects(); // 지속 효과와 소유 오브젝트 전체 제거
                return; // P2 효과 갱신 종료
            } // P2 효과 강제 정리 처리 종료

            float deltaTime = Mathf.Max(0f, Time.deltaTime); // 음수가 없는 프레임 시간 계산
            miniatureRemaining = Mathf.Max(0f, miniatureRemaining - deltaTime); // 소형화 남은 시간 감소
            invisibilityRemaining = Mathf.Max(0f, invisibilityRemaining - deltaTime); // 투명 망토 남은 시간 감소

            if (miniatureApplied && miniatureRemaining <= 0f) // 소형화 종료 시점 확인
            { // 소형화 복원 처리
                RestoreMiniature(); // 충돌체와 외형 원래 크기 복원
            } // 소형화 복원 처리 종료

            if (invisibilityApplied && invisibilityRemaining <= 0f) // 투명 망토 종료 시점 확인
            { // 투명 상태 복원 처리
                RestoreVisibility(); // 모든 Renderer 기존 상태 복원
            } // 투명 상태 복원 처리 종료

            UpdateCartRide(deltaTime); // 카트 경로 자동 주행 갱신
        } // P2 효과 프레임 처리 종료

        private void LateUpdate() // 기본 이동 이후 소형화 충돌체 크기 보정
        { // P2 외형과 충돌체 후처리
            if (!IsMiniature || characterController == null || !characterController.enabled) // 소형화와 충돌체 사용 가능 여부 확인
            { // 소형화 후처리 생략
                return; // 후처리 종료
            } // 소형화 후처리 생략 종료

            characterController.height = originalControllerHeight * miniatureScale; // 데이터 배율 기반 충돌체 높이 적용
            characterController.radius = originalControllerRadius * miniatureScale; // 데이터 배율 기반 충돌체 반지름 적용
            characterController.center = originalControllerCenter * miniatureScale; // 발 위치 유지를 위한 충돌체 중심 적용
        } // P2 외형과 충돌체 후처리 종료

        private void OnDisable() // P2 효과 관리자 비활성화 시 상태 정리
        { // 비활성화 정리 처리
            ClearAllEffects(); // 이동과 외형과 소유 효과 전체 복원
        } // 비활성화 정리 처리 종료

        public bool TryActivateRewind(float historySeconds, float playbackDuration) // 최근 안전 위치 역재생 시작 시도
        { // 되감기 활성화 처리
            if (rewindRecorder == null || IsRewinding || IsCartRiding) // 기록기와 중복 이동 효과 상태 확인
            { // 되감기 시작 불가 처리
                return false; // 되감기 시작 실패 반환
            } // 되감기 시작 불가 처리 종료

            if (!rewindRecorder.TryBuildRewindPath(historySeconds, rewindPath)) // 최근 안전 위치 경로 생성 여부 확인
            { // 안전 기록 부족 처리
                return false; // 아이템을 보존한 채 되감기 실패 반환
            } // 안전 기록 부족 처리 종료

            rewindRoutine = StartCoroutine(RewindRoutine(Mathf.Max(0.1f, playbackDuration))); // 충돌 없는 되감기 재생 시작
            return true; // 되감기 시작 성공 반환
        } // 되감기 활성화 처리 종료

        public void ActivateMiniature(float duration, float scale) // 소형화 물약 외형과 충돌체 축소 활성화
        { // 소형화 활성화 처리
            if (!miniatureApplied) // 최초 소형화 적용 여부 확인
            { // 소형화 원본 값 저장 처리
                originalControllerHeight = characterController == null ? 2f : characterController.height; // 현재 충돌체 높이 저장
                originalControllerRadius = characterController == null ? 0.5f : characterController.radius; // 현재 충돌체 반지름 저장
                originalControllerCenter = characterController == null ? Vector3.up : characterController.center; // 현재 충돌체 중심 저장
                originalVisualScale = visualRoot == null ? Vector3.one : visualRoot.localScale; // 현재 외형 크기 저장
                miniatureApplied = true; // 원본 값 저장 완료 표시
            } // 소형화 원본 값 저장 처리 종료

            miniatureScale = P2ItemRules.ClampMiniatureScale(scale); // 안전 범위 소형화 배율 저장
            miniatureRemaining = Mathf.Max(miniatureRemaining, duration); // 더 긴 소형화 시간 유지

            if (visualRoot != null) // 외형 루트 존재 여부 확인
            { // 외형 크기 축소 처리
                visualRoot.localScale = originalVisualScale * miniatureScale; // 원본 외형에 소형화 배율 적용
            } // 외형 크기 축소 처리 종료
        } // 소형화 활성화 처리 종료

        public void ActivateInvisibility(float duration) // 투명 망토 추적 제외와 외형 숨김 활성화
        { // 투명 망토 활성화 처리
            if (!invisibilityApplied) // 최초 투명 상태 적용 여부 확인
            { // Renderer 원본 상태 저장 처리
                visibilityRenderers = visualRoot == null ? GetComponentsInChildren<Renderer>(true) : visualRoot.GetComponentsInChildren<Renderer>(true); // 숨길 외형 Renderer 전체 조회
                rendererEnabledStates = new bool[visibilityRenderers.Length]; // Renderer 원본 상태 배열 생성

                for (int index = 0; index < visibilityRenderers.Length; index++) // 전체 외형 Renderer 순회
                { // 현재 Renderer 원본 상태 저장
                    rendererEnabledStates[index] = visibilityRenderers[index] != null && visibilityRenderers[index].enabled; // 기존 활성 상태 저장
                } // 현재 Renderer 원본 상태 저장 종료

                invisibilityApplied = true; // Renderer 원본 상태 저장 완료 표시
            } // Renderer 원본 상태 저장 처리 종료

            invisibilityRemaining = Mathf.Max(invisibilityRemaining, duration); // 더 긴 투명 시간 유지

            for (int index = 0; index < visibilityRenderers.Length; index++) // 전체 외형 Renderer 순회
            { // 현재 Renderer 숨김 처리
                if (visibilityRenderers[index] != null) // Renderer 존재 여부 확인
                { // 유효 Renderer 숨김 처리
                    visibilityRenderers[index].enabled = false; // 다른 화면에서 외형이 보이지 않도록 비활성화
                } // 유효 Renderer 숨김 처리 종료
            } // 현재 Renderer 숨김 처리 종료
        } // 투명 망토 활성화 처리 종료

        public bool TrySpawnHomingEffect(ItemDataDefinition itemData, Vector3 origin, Vector3 direction) // 유도탄 또는 드론 전방 소환 시도
        { // 추적 효과 소환 처리
            if (itemData == null || (itemData.EffectType != ItemEffectType.HomingMissile && itemData.EffectType != ItemEffectType.Drone)) // 데이터와 효과 종류 확인
            { // 잘못된 추적 효과 처리
                return false; // 추적 효과 생성 실패 반환
            } // 잘못된 추적 효과 처리 종료

            GameObject effectObject = new GameObject($"Homing_{itemData.DataId}_{itemData.DisplayName}"); // 추적 아이템 루트 생성
            Vector3 safeDirection = direction.sqrMagnitude <= 0.0001f ? transform.forward : direction.normalized; // 안전한 전방 방향 계산
            effectObject.transform.position = origin + safeDirection * 0.8f; // 플레이어 앞쪽 소환 위치 적용
            effectObject.transform.rotation = Quaternion.LookRotation(safeDirection, Vector3.up); // 최초 전방 회전 적용
            HomingItemEffect homingEffect = effectObject.AddComponent<HomingItemEffect>(); // 공통 추적 이동 효과 추가
            bool isMissile = itemData.EffectType == ItemEffectType.HomingMissile; // 유도탄 또는 드론 여부 계산
            float movementSpeed = itemData.ProjectileSpeed > 0f ? itemData.ProjectileSpeed : isMissile ? P2ItemRules.HomingMissileSpeed : P2ItemRules.DroneSpeed; // 데이터 또는 기본 추적 속도 선택
            float pushForce = itemData.PrimaryValue > 0f ? itemData.PrimaryValue : isMissile ? P2ItemRules.HomingMissileForce : P2ItemRules.DroneForce; // 데이터 또는 기본 밀치기 힘 선택
            float lifeTime = itemData.EffectDuration > 0f ? itemData.EffectDuration : isMissile ? P2ItemRules.HomingMissileLifeTime : P2ItemRules.DroneLifeTime; // 데이터 또는 기본 유지 시간 선택
            float hitRadius = itemData.EffectRadius > 0f ? itemData.EffectRadius : isMissile ? P2ItemRules.HomingMissileRadius : P2ItemRules.DroneRadius; // 데이터 또는 기본 적중 반경 선택
            ownedHomingEffects.Add(homingEffect); // 소유 효과 목록에 새 추적 효과 등록

            if (!homingEffect.Configure(itemData.EffectType, transform, this, movementSpeed, pushForce, lifeTime, hitRadius, P2ItemRules.MaximumRetargetCount, itemData.PickupColor)) // 목표 검색과 효과 구성 성공 여부 확인
            { // 추적 대상 없음 처리
                ownedHomingEffects.Remove(homingEffect); // 실패한 효과를 소유 목록에서 제거
                return false; // 아이템을 보존한 채 생성 실패 반환
            } // 추적 대상 없음 처리 종료

            return true; // 추적 효과 소환 성공 반환
        } // 추적 효과 소환 처리 종료

        public bool TryActivateCart(float duration, float speed, float routeSearchRange) // 가까운 연결 경로 자동 주행 시작 시도
        { // 카트 자동 주행 활성화 처리
            if (IsRewinding || IsCartRiding || characterController == null) // 중복 특수 이동과 충돌체 존재 여부 확인
            { // 카트 시작 불가 처리
                return false; // 카트 자동 주행 실패 반환
            } // 카트 시작 불가 처리 종료

            if (!CartPath.TryFindNearestPath(transform.position, routeSearchRange, out CartPath foundPath, out int foundWaypointIndex)) // 가까운 경로와 시작 지점 검색 여부 확인
            { // 가까운 카트 경로 없음 처리
                return false; // 아이템을 보존한 채 카트 시작 실패 반환
            } // 가까운 카트 경로 없음 처리 종료

            int nextWaypointIndex = foundWaypointIndex; // 최초 이동 목표 지점 번호 저장

            if (foundPath.TryGetWaypoint(foundWaypointIndex, out Vector3 foundPosition) && Vector3.Distance(transform.position, foundPosition) <= 0.35f) // 이미 가장 가까운 지점에 도달했는지 확인
            { // 다음 경로 지점 선택 처리
                nextWaypointIndex++; // 현재 지점 다음 번호 선택
            } // 다음 경로 지점 선택 처리 종료

            if (!foundPath.TryGetWaypoint(nextWaypointIndex, out Vector3 unusedWaypoint)) // 실제 이동할 다음 지점 존재 여부 확인
            { // 경로 끝 시작 처리
                return false; // 주행 가능한 다음 지점 없음 반환
            } // 경로 끝 시작 처리 종료

            activeCartPath = foundPath; // 현재 카트 경로 저장
            activeCartWaypointIndex = nextWaypointIndex; // 다음 이동 지점 번호 저장
            cartRemaining = Mathf.Max(0.1f, duration); // 카트 최대 주행 시간 저장
            cartSpeed = Mathf.Max(0.1f, speed); // 카트 이동 속도 저장
            movementWasEnabledBeforeCart = movementController != null && movementController.enabled; // 기존 이동 컴포넌트 활성 상태 저장

            if (movementController != null) // 이동 관리자 존재 여부 확인
            { // 기본 이동 차단 처리
                movementController.enabled = false; // 카트 주행 중 일반 이동 계산 비활성화
            } // 기본 이동 차단 처리 종료

            inputReader?.SetP2MovementRestricted(true); // 카트 주행 중 이동과 달리기와 앉기 입력 제한
            externalForceController?.ClearVelocity(); // 주행 시작 전 남은 외부 힘 제거
            return true; // 카트 자동 주행 시작 성공 반환
        } // 카트 자동 주행 활성화 처리 종료

        public void UnregisterOwnedEffect(HomingItemEffect effect) // 제거된 유도탄 또는 드론 소유 목록 해제
        { // 소유 효과 목록 해제 처리
            if (effect != null) // 제거 대상 참조 존재 여부 확인
            { // 유효 소유 효과 해제 처리
                ownedHomingEffects.Remove(effect); // 소유 효과 목록에서 제거
            } // 유효 소유 효과 해제 처리 종료
        } // 소유 효과 목록 해제 처리 종료

        public void ClearAllEffects() // 부활과 경기 종료용 P2 효과 전체 정리
        { // P2 효과 전체 정리 처리
            if (rewindRoutine != null) // 진행 중인 되감기 존재 여부 확인
            { // 되감기 코루틴 중단 처리
                StopCoroutine(rewindRoutine); // 진행 중인 되감기 재생 중단
                rewindRoutine = null; // 되감기 코루틴 참조 제거
            } // 되감기 코루틴 중단 처리 종료

            RestoreRewindState(); // 되감기용 충돌과 이동 상태 복원
            miniatureRemaining = 0f; // 소형화 남은 시간 제거
            invisibilityRemaining = 0f; // 투명 망토 남은 시간 제거
            RestoreMiniature(); // 충돌체와 외형 크기 복원
            RestoreVisibility(); // Renderer 기존 상태 복원
            StopCartRide(); // 카트 주행과 이동 제한 종료

            for (int index = ownedHomingEffects.Count - 1; index >= 0; index--) // 현재 소유 추적 효과 역순 순회
            { // 소유 추적 효과 제거 처리
                if (ownedHomingEffects[index] != null) // 추적 효과 존재 여부 확인
                { // 유효 추적 효과 제거 처리
                    Destroy(ownedHomingEffects[index].gameObject); // 부활 또는 경기 종료에 맞춰 제거
                } // 유효 추적 효과 제거 처리 종료
            } // 소유 추적 효과 제거 처리 종료

            ownedHomingEffects.Clear(); // 소유 추적 효과 목록 초기화
            rewindRecorder?.ClearHistory(); // 부활 전 또는 종료 전 이동 기록 제거
        } // P2 효과 전체 정리 처리 종료

        private IEnumerator RewindRoutine(float playbackDuration) // 안전 위치 경로를 충돌 없이 역재생
        { // 되감기 재생 처리
            PrepareRewindState(); // 일반 이동과 충돌을 끈 유령 상태 준비
            float elapsedTime = 0f; // 되감기 경과 시간 초기화

            while (elapsedTime < playbackDuration && rewindPath.Count > 0) // 재생 시간과 표본이 남은 동안 반복
            { // 현재 되감기 표본 적용 처리
                float progress = P2ItemRules.CalculatePlaybackProgress(elapsedTime, playbackDuration); // 전체 되감기 진행률 계산
                float scaledIndex = progress * Mathf.Max(0, rewindPath.Count - 1); // 표본 배열 내 실수 번호 계산
                int fromIndex = Mathf.Clamp(Mathf.FloorToInt(scaledIndex), 0, rewindPath.Count - 1); // 현재 앞쪽 표본 번호 계산
                int toIndex = Mathf.Clamp(fromIndex + 1, 0, rewindPath.Count - 1); // 현재 뒤쪽 표본 번호 계산
                float segmentProgress = scaledIndex - fromIndex; // 두 표본 사이 보간 진행률 계산
                PlayerRewindSample fromSample = rewindPath[fromIndex]; // 현재 최신 쪽 표본 조회
                PlayerRewindSample toSample = rewindPath[toIndex]; // 현재 과거 쪽 표본 조회
                transform.SetPositionAndRotation(Vector3.Lerp(fromSample.Position, toSample.Position, segmentProgress), Quaternion.Slerp(fromSample.Rotation, toSample.Rotation, segmentProgress)); // 장애물과 충돌하지 않는 위치와 회전 보간 적용
                elapsedTime += Mathf.Max(0f, Time.deltaTime); // 현재 프레임 시간 누적
                yield return null; // 다음 프레임까지 대기
            } // 현재 되감기 표본 적용 처리 종료

            if (rewindPath.Count > 0) // 최종 과거 표본 존재 여부 확인
            { // 최종 되감기 위치 적용 처리
                PlayerRewindSample finalSample = rewindPath[rewindPath.Count - 1]; // 가장 오래된 안전 표본 조회
                transform.SetPositionAndRotation(finalSample.Position, finalSample.Rotation); // 정확한 최종 안전 위치와 회전 적용
            } // 최종 되감기 위치 적용 처리 종료

            rewindRoutine = null; // 되감기 진행 상태 해제
            RestoreRewindState(); // 일반 이동과 충돌 상태 복원
            movementController?.ResetAfterRespawn(); // 되감기 후 낙하와 외부 힘 이동 상태 초기화
            rewindRecorder?.ClearHistory(); // 되감은 이전 미래 기록 제거
        } // 되감기 재생 처리 종료

        private void PrepareRewindState() // 되감기 중 유령 이동 상태 준비
        { // 되감기 유령 상태 준비 처리
            rewindStatePrepared = true; // 되감기 상태 변경 시작 표시
            movementWasEnabledBeforeRewind = movementController != null && movementController.enabled; // 되감기 전 기본 이동 활성 상태 저장
            externalForceWasEnabledBeforeRewind = externalForceController != null && externalForceController.enabled; // 되감기 전 외부 힘 활성 상태 저장

            if (movementController != null) // 이동 관리자 존재 여부 확인
            { // 되감기 중 이동 비활성화 처리
                movementController.enabled = false; // 일반 이동 계산 중단
            } // 되감기 중 이동 비활성화 처리 종료

            if (externalForceController != null) // 외부 힘 관리자 존재 여부 확인
            { // 되감기 중 외부 힘 비활성화 처리
                externalForceController.ResetExternalForce(); // 남은 밀치기와 발판 속도 제거
                externalForceController.enabled = false; // 새 외부 힘 수신 차단
            } // 되감기 중 외부 힘 비활성화 처리 종료

            rewindColliders = GetComponentsInChildren<Collider>(true); // 플레이어 루트 아래 모든 Collider 조회
            rewindColliderStates = new bool[rewindColliders.Length]; // Collider 원본 활성 상태 배열 생성

            for (int index = 0; index < rewindColliders.Length; index++) // 전체 플레이어 Collider 순회
            { // 현재 Collider 유령 상태 적용
                Collider currentCollider = rewindColliders[index]; // 현재 Collider 조회
                rewindColliderStates[index] = currentCollider != null && currentCollider.enabled; // 기존 활성 상태 저장

                if (currentCollider != null) // Collider 존재 여부 확인
                { // 유효 Collider 비활성화 처리
                    currentCollider.enabled = false; // 되감기 중 현재 장애물과 충돌하지 않도록 끄기
                } // 유효 Collider 비활성화 처리 종료
            } // 현재 Collider 유령 상태 적용 종료

            inputReader?.SetP2MovementRestricted(true); // 되감기 중 일반 이동 입력 제한
        } // 되감기 유령 상태 준비 처리 종료

        private void RestoreRewindState() // 되감기 전 이동과 충돌 상태 복원
        { // 되감기 상태 복원 처리
            if (!rewindStatePrepared) // 실제 되감기 상태 변경 여부 확인
            { // 되감기 복원 생략 처리
                return; // 원래 이동 컴포넌트 상태를 건드리지 않고 종료
            } // 되감기 복원 생략 처리 종료

            if (rewindColliders != null && rewindColliderStates != null) // Collider 원본 상태 배열 존재 여부 확인
            { // Collider 원본 상태 복원 처리
                int restoreCount = Mathf.Min(rewindColliders.Length, rewindColliderStates.Length); // 안전한 복원 항목 수 계산

                for (int index = 0; index < restoreCount; index++) // 저장된 Collider 전체 순회
                { // 현재 Collider 상태 복원
                    if (rewindColliders[index] != null) // Collider가 아직 존재하는지 확인
                    { // 유효 Collider 상태 복원 처리
                        bool respawnKeepsControllerDisabled = respawnController != null && respawnController.IsRespawning && rewindColliders[index] == characterController; // 부활 코루틴의 CharacterController 비활성 상태 유지 여부 계산
                        rewindColliders[index].enabled = respawnKeepsControllerDisabled ? false : rewindColliderStates[index]; // 부활 상태 또는 되감기 전 활성 상태 적용
                    } // 유효 Collider 상태 복원 처리 종료
                } // 현재 Collider 상태 복원 종료
            } // Collider 원본 상태 복원 처리 종료

            rewindColliders = null; // Collider 참조 배열 제거
            rewindColliderStates = null; // Collider 상태 배열 제거

            if (movementController != null) // 이동 관리자 존재 여부 확인
            { // 이동 활성 상태 복원 처리
                movementController.enabled = movementWasEnabledBeforeRewind; // 되감기 전 이동 활성 상태 적용
            } // 이동 활성 상태 복원 처리 종료

            if (externalForceController != null) // 외부 힘 관리자 존재 여부 확인
            { // 외부 힘 활성 상태 복원 처리
                externalForceController.enabled = externalForceWasEnabledBeforeRewind; // 되감기 전 외부 힘 활성 상태 적용
            } // 외부 힘 활성 상태 복원 처리 종료

            inputReader?.SetP2MovementRestricted(false); // 되감기 이동 입력 제한 해제
            rewindStatePrepared = false; // 되감기 상태 복원 완료 표시
        } // 되감기 상태 복원 처리 종료

        private void UpdateCartRide(float deltaTime) // 현재 카트 경로 자동 주행 갱신
        { // 카트 주행 프레임 처리
            if (!IsCartRiding) // 카트 주행 비활성 여부 확인
            { // 카트 주행 생략 처리
                return; // 카트 주행 갱신 종료
            } // 카트 주행 생략 처리 종료

            cartRemaining = Mathf.Max(0f, cartRemaining - deltaTime); // 남은 카트 주행 시간 감소

            if (cartRemaining <= 0f || !activeCartPath.TryGetWaypoint(activeCartWaypointIndex, out Vector3 targetPosition)) // 시간 종료 또는 다음 경로 지점 누락 여부 확인
            { // 카트 주행 종료 처리
                StopCartRide(); // 이동 제한과 경로 상태 정리
                return; // 카트 주행 갱신 종료
            } // 카트 주행 종료 처리 종료

            Vector3 toTarget = targetPosition - transform.position; // 현재 위치에서 다음 경로 지점 방향 계산
            float distance = toTarget.magnitude; // 다음 경로 지점까지 거리 계산

            if (distance <= 0.2f) // 현재 경로 지점 도달 여부 확인
            { // 다음 경로 지점 전환 처리
                activeCartWaypointIndex++; // 다음 경로 지점 번호로 이동

                if (!activeCartPath.TryGetWaypoint(activeCartWaypointIndex, out targetPosition)) // 다음 경로 지점 존재 여부 확인
                { // 경로 끝 도달 처리
                    StopCartRide(); // 카트 자동 주행 종료
                    return; // 카트 주행 갱신 종료
                } // 경로 끝 도달 처리 종료

                toTarget = targetPosition - transform.position; // 새 경로 지점 방향 다시 계산
                distance = toTarget.magnitude; // 새 경로 지점 거리 다시 계산
            } // 다음 경로 지점 전환 처리 종료

            Vector3 direction = distance <= 0.0001f ? Vector3.zero : toTarget / distance; // 안전한 현재 주행 방향 계산
            Vector3 movement = direction * Mathf.Min(distance, cartSpeed * deltaTime); // 이번 프레임 경로 이동량 계산
            externalForceController?.ClearVelocity(); // 카트 경로를 벗어날 수 있는 외부 힘 제거

            if (characterController != null && characterController.enabled) // CharacterController 이동 가능 여부 확인
            { // 충돌을 반영한 카트 이동 처리
                characterController.Move(movement); // 경로 방향 자동 이동 실행
            } // 충돌을 반영한 카트 이동 처리 종료
            else // CharacterController 사용 불가 처리
            { // Transform 대체 이동 처리
                transform.position += movement; // 경로 방향 위치 직접 이동
            } // Transform 대체 이동 처리 종료

            Vector3 horizontalDirection = Vector3.ProjectOnPlane(direction, Vector3.up); // 회전에 사용할 수평 주행 방향 계산

            if (horizontalDirection.sqrMagnitude > 0.0001f) // 유효한 수평 주행 방향 여부 확인
            { // 카트 진행 방향 회전 처리
                transform.rotation = Quaternion.LookRotation(horizontalDirection.normalized, Vector3.up); // 경로 진행 방향을 바라보도록 회전
            } // 카트 진행 방향 회전 처리 종료
        } // 카트 주행 프레임 처리 종료

        private void StopCartRide() // 카트 경로와 이동 제한 상태 정리
        { // 카트 주행 종료 처리
            bool wasCartRiding = activeCartPath != null || cartRemaining > 0f; // 실제 카트 상태 존재 여부 저장
            activeCartPath = null; // 현재 카트 경로 제거
            activeCartWaypointIndex = -1; // 다음 지점 번호 초기화
            cartRemaining = 0f; // 남은 주행 시간 제거

            if (movementController != null && wasCartRiding) // 이동 관리자와 카트 상태 존재 여부 확인
            { // 기본 이동 상태 복원 처리
                movementController.enabled = movementWasEnabledBeforeCart; // 카트 전 이동 활성 상태 적용
                movementController.ResetAfterRespawn(); // 주행 종료 후 낙하와 이동 상태 초기화
            } // 기본 이동 상태 복원 처리 종료

            if (wasCartRiding) // 실제 이동 제한을 적용했던 카트 상태 여부 확인
            { // 카트 입력 제한 해제 처리
                inputReader?.SetP2MovementRestricted(false); // 이동과 달리기와 앉기 입력 제한 해제
            } // 카트 입력 제한 해제 처리 종료
        } // 카트 주행 종료 처리 종료

        private void RestoreMiniature() // 소형화 전 충돌체와 외형 크기 복원
        { // 소형화 복원 처리
            if (!miniatureApplied) // 원본 값 저장 여부 확인
            { // 소형화 복원 생략 처리
                return; // 소형화 복원 종료
            } // 소형화 복원 생략 처리 종료

            if (characterController != null) // CharacterController 존재 여부 확인
            { // 충돌체 크기 복원 처리
                characterController.height = originalControllerHeight; // 원래 충돌체 높이 복원
                characterController.radius = originalControllerRadius; // 원래 충돌체 반지름 복원
                characterController.center = originalControllerCenter; // 원래 충돌체 중심 복원
            } // 충돌체 크기 복원 처리 종료

            if (visualRoot != null) // 외형 루트 존재 여부 확인
            { // 외형 크기 복원 처리
                visualRoot.localScale = originalVisualScale; // 소형화 전 외형 크기 복원
            } // 외형 크기 복원 처리 종료

            miniatureApplied = false; // 소형화 원본 상태 제거
            miniatureScale = 1f; // 기본 크기 배율 복원
        } // 소형화 복원 처리 종료

        private void RestoreVisibility() // 투명 전 Renderer 활성 상태 복원
        { // 투명 상태 복원 처리
            if (!invisibilityApplied || visibilityRenderers == null || rendererEnabledStates == null) // 저장된 Renderer 상태 존재 여부 확인
            { // 투명 상태 복원 생략 처리
                return; // Renderer 복원 종료
            } // 투명 상태 복원 생략 처리 종료

            int restoreCount = Mathf.Min(visibilityRenderers.Length, rendererEnabledStates.Length); // 안전한 Renderer 복원 항목 수 계산

            for (int index = 0; index < restoreCount; index++) // 전체 저장 Renderer 순회
            { // 현재 Renderer 활성 상태 복원
                if (visibilityRenderers[index] != null) // Renderer가 아직 존재하는지 확인
                { // 유효 Renderer 복원 처리
                    visibilityRenderers[index].enabled = rendererEnabledStates[index]; // 투명 전 활성 상태 적용
                } // 유효 Renderer 복원 처리 종료
            } // 현재 Renderer 활성 상태 복원 종료

            visibilityRenderers = null; // Renderer 참조 배열 제거
            rendererEnabledStates = null; // Renderer 상태 배열 제거
            invisibilityApplied = false; // 투명 원본 상태 제거
        } // 투명 상태 복원 처리 종료

        private void ResolveReferences() // 플레이어와 Scene 기반 누락 참조 자동 연결
        { // 누락 참조 자동 연결 처리
            movementController = movementController == null ? GetComponent<PlayerMovementController>() : movementController; // 같은 오브젝트 이동 관리자 저장
            externalForceController = externalForceController == null ? GetComponent<PlayerExternalForceController>() : externalForceController; // 같은 오브젝트 외부 힘 관리자 저장
            respawnController = respawnController == null ? GetComponent<PlayerRespawnController>() : respawnController; // 같은 오브젝트 부활 관리자 저장
            inputReader = inputReader == null ? GetComponent<PlayerInputReader>() : inputReader; // 같은 오브젝트 입력 제공자 저장
            rewindRecorder = rewindRecorder == null ? GetComponent<PlayerRewindRecorder>() : rewindRecorder; // 같은 오브젝트 되감기 기록기 저장
            characterController = characterController == null ? GetComponent<CharacterController>() : characterController; // 같은 오브젝트 CharacterController 저장
            visualRoot = visualRoot == null ? transform.Find("Visual") : visualRoot; // Visual 자식 또는 기존 외형 루트 저장
            matchController = matchController == null ? FindFirstObjectByType<PrototypeMatchController>() : matchController; // 현재 Scene 경기 관리자 저장
        } // 누락 참조 자동 연결 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(PlayerMovementController newMovementController, PlayerExternalForceController newExternalForceController, PlayerRespawnController newRespawnController, PrototypeMatchController newMatchController, PlayerInputReader newInputReader, PlayerRewindRecorder newRewindRecorder, CharacterController newCharacterController, Transform newVisualRoot) // 자동 설정 도구용 P2 효과 참조 연결
        { // Editor P2 효과 참조 연결 처리
            movementController = newMovementController; // 이동 관리자 저장
            externalForceController = newExternalForceController; // 외부 힘 관리자 저장
            respawnController = newRespawnController; // 부활 관리자 저장
            matchController = newMatchController; // 경기 관리자 저장
            inputReader = newInputReader; // 입력 제공자 저장
            rewindRecorder = newRewindRecorder; // 되감기 기록기 저장
            characterController = newCharacterController; // CharacterController 저장
            visualRoot = newVisualRoot; // 외형 루트 저장
        } // Editor P2 효과 참조 연결 처리 종료
#endif // Editor 전용 설정 종료
    } // P2 효과 관리자 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
