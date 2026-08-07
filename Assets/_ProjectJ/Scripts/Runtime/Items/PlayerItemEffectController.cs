using ProjectJ.Gameplay; // 경기 종료 상태 기능 참조
using ProjectJ.Player; // 플레이어 이동과 외부 힘 기능 참조
using UnityEngine; // Unity 시간과 컴포넌트 기능 참조
using UnityEngine.InputSystem; // 추가 점프 키 입력 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 플레이어당 효과 관리자 한 개만 허용
    [RequireComponent(typeof(PlayerMovementController))] // 플레이어 이동 관리자 필수 지정
    [RequireComponent(typeof(PlayerExternalForceController))] // 플레이어 외부 힘 관리자 필수 지정
    public sealed class PlayerItemEffectController : MonoBehaviour // 지속형 아이템 효과 관리자 선언
    { // 지속형 아이템 효과 관리자 묶음
        [SerializeField] private PlayerMovementController movementController; // 이동과 접지 상태 제공자 저장
        [SerializeField] private PlayerExternalForceController externalForceController; // 추가 이동과 방어 대상 저장
        [SerializeField] private PlayerRespawnController respawnController; // 부활 상태 제공자 저장
        [SerializeField] private PrototypeMatchController matchController; // 경기 종료 상태 제공자 저장

        private float springShoesRemaining; // 스프링 신발 남은 시간 저장
        private float springJumpVelocity; // 스프링 신발 추가 상승 속도 저장
        private bool springJumpUsed; // 현재 효과의 추가 점프 사용 여부 저장
        private float jellyShieldRemaining; // 젤리 보호막 남은 시간 저장
        private bool ownsExternalForceDisable; // 보호막이 외부 힘 컴포넌트를 끈 상태 저장
        private float featherShoesRemaining; // 깃털 신발 남은 시간 저장
        private float featherSpeedMultiplier = 1f; // 깃털 신발 이동 배율 저장
        private float slowRemaining; // 눈덩이 감속 남은 시간 저장
        private float slowMultiplier = 1f; // 눈덩이 감속 이동 배율 저장
        private bool wasRespawning; // 직전 프레임 부활 상태 저장

        public bool IsSpringShoesActive => springShoesRemaining > 0f; // 스프링 신발 활성 여부 반환
        public bool IsJellyShieldActive => jellyShieldRemaining > 0f; // 젤리 보호막 활성 여부 반환
        public bool IsFeatherShoesActive => featherShoesRemaining > 0f; // 깃털 신발 활성 여부 반환
        public bool IsSlowed => slowRemaining > 0f; // 눈덩이 감속 활성 여부 반환

        private void Awake() // 실행 시작 시 플레이어 참조 준비
        { // 플레이어 참조 준비 처리
            ResolveReferences(); // 같은 플레이어와 Scene에서 누락 참조 자동 검색
        } // 플레이어 참조 준비 처리 종료

        private void Update() // 지속형 효과 시간과 추가 점프 갱신
        { // 지속형 효과 프레임 처리
            if (matchController != null && matchController.IsMatchFinished) // 경기 종료 상태 확인
            { // 경기 종료 효과 정리 처리
                ClearAllEffects(); // 남은 아이템 효과 전체 제거
                return; // 지속형 효과 갱신 종료
            } // 경기 종료 효과 정리 처리 종료

            bool isRespawning = respawnController != null && respawnController.IsRespawning; // 현재 부활 진행 여부 조회

            if (isRespawning && !wasRespawning) // 새 부활 시작 프레임 여부 확인
            { // 부활 시작 효과 정리 처리
                ClearAllEffects(); // 사망 전 지속 효과 전체 제거
            } // 부활 시작 효과 정리 처리 종료

            wasRespawning = isRespawning; // 현재 부활 상태 저장

            if (isRespawning) // 부활 진행 중 여부 확인
            { // 부활 중 갱신 생략 처리
                return; // 지속 효과 입력 처리 중단
            } // 부활 중 갱신 생략 처리 종료

            float deltaTime = Mathf.Max(0f, Time.deltaTime); // 음수가 없는 프레임 시간 계산
            springShoesRemaining = Mathf.Max(0f, springShoesRemaining - deltaTime); // 스프링 신발 남은 시간 감소
            jellyShieldRemaining = Mathf.Max(0f, jellyShieldRemaining - deltaTime); // 젤리 보호막 남은 시간 감소
            featherShoesRemaining = Mathf.Max(0f, featherShoesRemaining - deltaTime); // 깃털 신발 남은 시간 감소
            slowRemaining = Mathf.Max(0f, slowRemaining - deltaTime); // 눈덩이 감속 남은 시간 감소
            UpdateJellyShieldState(); // 보호막 기반 외부 힘 수신 상태 갱신
            TryPerformExtraJump(); // 공중 추가 점프 입력 처리
        } // 지속형 효과 프레임 처리 종료

        private void LateUpdate() // 이동 계산 뒤 속도 배율 보정
        { // 이동 속도 배율 보정 처리
            if (externalForceController == null || !externalForceController.enabled || movementController == null) // 이동과 외부 힘 참조와 보호막 상태 확인
            { // 속도 보정 불가 처리
                return; // 이동 속도 보정 생략
            } // 속도 보정 불가 처리 종료

            float featherMultiplier = IsFeatherShoesActive ? Mathf.Max(0f, featherSpeedMultiplier) : 1f; // 현재 깃털 신발 배율 계산
            float currentSlowMultiplier = IsSlowed ? Mathf.Clamp(slowMultiplier, 0.1f, 1f) : 1f; // 현재 감속 배율 계산
            float combinedMultiplier = featherMultiplier * currentSlowMultiplier; // 강화와 감속을 곱한 최종 배율 계산
            Vector3 carrierVelocity = movementController.ControlledHorizontalVelocity * (combinedMultiplier - 1f); // 기본 이동에 더하거나 뺄 보정 속도 계산

            if (carrierVelocity.sqrMagnitude > 0.0001f) // 유효한 속도 보정 여부 확인
            { // 속도 보정 적용 처리
                externalForceController.ApplyPlatformVelocity(carrierVelocity); // 발판 전달 속도 방식으로 이동 배율 적용
            } // 속도 보정 적용 처리 종료
        } // 이동 속도 배율 보정 처리 종료

        private void OnDisable() // 효과 관리자 비활성화 시 상태 복원
        { // 효과 관리자 비활성화 정리 처리
            ClearAllEffects(); // 보호막과 지속 효과 전체 제거
        } // 효과 관리자 비활성화 정리 처리 종료

        public void ActivateSpringShoes(float duration, float extraJumpVelocity) // 스프링 신발 8초 추가 점프 활성화
        { // 스프링 신발 활성화 처리
            springShoesRemaining = Mathf.Max(springShoesRemaining, duration); // 더 긴 남은 시간 유지
            springJumpVelocity = Mathf.Max(0f, extraJumpVelocity); // 추가 상승 속도 저장
            springJumpUsed = false; // 새 효과의 추가 점프 사용 상태 초기화
        } // 스프링 신발 활성화 처리 종료

        public void ActivateJellyShield(float duration) // 젤리 보호막 밀치기 방어 활성화
        { // 젤리 보호막 활성화 처리
            jellyShieldRemaining = Mathf.Max(jellyShieldRemaining, duration); // 더 긴 남은 시간 유지
            UpdateJellyShieldState(); // 즉시 외부 힘 수신 차단 적용
        } // 젤리 보호막 활성화 처리 종료

        public void ActivateFeatherShoes(float duration, float speedMultiplier) // 깃털 신발 이동 속도 강화 활성화
        { // 깃털 신발 활성화 처리
            featherShoesRemaining = Mathf.Max(featherShoesRemaining, duration); // 더 긴 남은 시간 유지
            featherSpeedMultiplier = Mathf.Max(1f, speedMultiplier); // 최소 기본 속도 이상의 배율 저장
        } // 깃털 신발 활성화 처리 종료

        public void ApplySlow(float duration, float speedMultiplier) // 눈덩이 감속 효과 적용
        { // 눈덩이 감속 적용 처리
            slowRemaining = Mathf.Max(slowRemaining, duration); // 더 긴 감속 시간 유지
            slowMultiplier = Mathf.Clamp(speedMultiplier, 0.1f, 1f); // 안전 범위 감속 배율 저장
        } // 눈덩이 감속 적용 처리 종료

        public void ClearAllEffects() // 부활과 경기 종료 시 모든 지속 효과 제거
        { // 모든 지속 효과 제거 처리
            springShoesRemaining = 0f; // 스프링 신발 시간 제거
            springJumpUsed = false; // 추가 점프 사용 상태 제거
            jellyShieldRemaining = 0f; // 젤리 보호막 시간 제거
            featherShoesRemaining = 0f; // 깃털 신발 시간 제거
            slowRemaining = 0f; // 눈덩이 감속 시간 제거
            RestoreExternalForceController(); // 보호막이 끈 외부 힘 컴포넌트 복원
        } // 모든 지속 효과 제거 처리 종료

        private void TryPerformExtraJump() // 스프링 신발 공중 추가 점프 입력 처리
        { // 공중 추가 점프 처리
            if (!IsSpringShoesActive || springJumpUsed || movementController == null || movementController.IsGrounded) // 효과와 사용 여부와 공중 상태 확인
            { // 추가 점프 불가 처리
                return; // 추가 점프 처리 생략
            } // 추가 점프 불가 처리 종료

            if (Keyboard.current == null || !Keyboard.current.spaceKey.wasPressedThisFrame) // 현재 프레임 Space 입력 여부 확인
            { // 추가 점프 입력 없음 처리
                return; // 추가 점프 처리 생략
            } // 추가 점프 입력 없음 처리 종료

            if (externalForceController != null && externalForceController.enabled && externalForceController.ApplyObstacleImpulse(Vector3.up, springJumpVelocity)) // 위쪽 추가 상승 힘 적용 성공 여부 확인
            { // 추가 점프 성공 처리
                springJumpUsed = true; // 현재 효과 추가 점프 사용 완료 저장
            } // 추가 점프 성공 처리 종료
        } // 공중 추가 점프 처리 종료

        private void UpdateJellyShieldState() // 젤리 보호막 기반 외부 힘 수신 상태 갱신
        { // 젤리 보호막 상태 갱신 처리
            if (externalForceController == null) // 외부 힘 관리자 누락 여부 확인
            { // 보호막 적용 불가 처리
                return; // 보호막 상태 갱신 생략
            } // 보호막 적용 불가 처리 종료

            if (IsJellyShieldActive) // 보호막 활성 여부 확인
            { // 외부 힘 수신 차단 처리
                externalForceController.ClearVelocity(); // 보호막 발동 전 남은 외부 힘 제거

                if (externalForceController.enabled) // 기존 활성 상태 여부 확인
                { // 보호막 소유 비활성화 처리
                    externalForceController.enabled = false; // 새 외부 힘 수신 차단
                    ownsExternalForceDisable = true; // 보호막이 비활성화한 상태 기록
                } // 보호막 소유 비활성화 처리 종료

                return; // 보호막 해제 처리 생략
            } // 외부 힘 수신 차단 처리 종료

            RestoreExternalForceController(); // 보호막 종료 뒤 외부 힘 수신 복원
        } // 젤리 보호막 상태 갱신 처리 종료

        private void RestoreExternalForceController() // 보호막이 비활성화한 외부 힘 관리자 복원
        { // 외부 힘 관리자 복원 처리
            if (externalForceController != null && ownsExternalForceDisable) // 보호막 소유 비활성화 상태 확인
            { // 외부 힘 관리자 활성화 처리
                externalForceController.enabled = true; // 외부 힘 수신 다시 허용
            } // 외부 힘 관리자 활성화 처리 종료

            ownsExternalForceDisable = false; // 보호막 소유 상태 초기화
        } // 외부 힘 관리자 복원 처리 종료

        private void ResolveReferences() // 플레이어와 Scene 기반 누락 참조 자동 연결
        { // 누락 참조 자동 연결 처리
            movementController = movementController == null ? GetComponent<PlayerMovementController>() : movementController; // 같은 오브젝트 이동 관리자 저장
            externalForceController = externalForceController == null ? GetComponent<PlayerExternalForceController>() : externalForceController; // 같은 오브젝트 외부 힘 관리자 저장
            respawnController = respawnController == null ? GetComponent<PlayerRespawnController>() : respawnController; // 같은 오브젝트 부활 관리자 저장
            matchController = matchController == null ? FindFirstObjectByType<PrototypeMatchController>() : matchController; // 현재 Scene 경기 관리자 저장
        } // 누락 참조 자동 연결 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(PlayerMovementController newMovementController, PlayerExternalForceController newExternalForceController, PlayerRespawnController newRespawnController, PrototypeMatchController newMatchController) // 자동 설정 도구용 효과 참조 연결
        { // 자동 설정 도구용 효과 참조 연결 처리
            movementController = newMovementController; // 이동 관리자 저장
            externalForceController = newExternalForceController; // 외부 힘 관리자 저장
            respawnController = newRespawnController; // 부활 관리자 저장
            matchController = newMatchController; // 경기 관리자 저장
        } // 자동 설정 도구용 효과 참조 연결 처리 종료
#endif // Editor 전용 설정 종료
    } // 지속형 아이템 효과 관리자 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
