using UnityEngine; // Unity 벡터와 시간 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 외부 힘 기능 묶음
    [DisallowMultipleComponent] // 외부 힘 컴포넌트 중복 방지
    [RequireComponent(typeof(PlayerStateController))] // 플레이어 상태 컴포넌트 보장
    public sealed class PlayerExternalForceController : ExternalForceReceiver // 플레이어 외부 힘 통합 관리 컴포넌트 선언
    { // 외부 힘 통합 관리 기능 묶음
        private const float ExternalForceThreshold = 0.0001f; // 외부 힘 활성 판정 기준

        [SerializeField, Min(0f)] private float hitImmunityDuration = 0.8f; // 연속 밀치기 면역 시간
        [SerializeField, Min(0f)] private float maximumCombinedPushSpeed = 10f; // 동시와 연속 밀치기 최대 합산 속도
        [SerializeField, Min(0f)] private float impulseDeceleration = 8f; // 순간 외부 힘 감속도
        [SerializeField, Min(0.01f)] private float platformVelocityGraceTime = 0.1f; // 발판 속도 갱신 유예 시간

        private PlayerStateController stateController; // 플레이어 상태 관리자
        private PlayerRespawnProtectionController respawnProtectionController; // 부활 보호 관리자
        private Vector3 impulseVelocity; // 밀치기와 장애물 순간 속도
        private Vector3 carrierVelocity; // 이동 발판 전달 속도
        private Vector3 pendingPushVelocity; // 같은 프레임에 모인 밀치기 속도
        private float hitImmunityRemaining; // 남은 밀치기 면역 시간
        private float carrierVelocityRemaining; // 남은 발판 속도 유지 시간
        private bool hasPendingPush; // 적용 대기 중인 밀치기 존재 여부

        public Vector3 Velocity => impulseVelocity + carrierVelocity; // 최종 외부 속도 반환
        public Vector3 HorizontalVelocity => Vector3.ProjectOnPlane(Velocity, Vector3.up); // 최종 외부 수평 속도 반환
        public float VerticalVelocity => Velocity.y; // 최종 외부 수직 속도 반환
        public Vector3 ImpulseVelocity => impulseVelocity; // 현재 순간 외부 속도 반환
        public Vector3 CarrierVelocity => carrierVelocity; // 현재 발판 전달 속도 반환
        public float HitImmunityRemaining => hitImmunityRemaining; // 남은 밀치기 면역 시간 반환
        public bool IsForceImmune => hitImmunityRemaining > 0f; // 연속 밀치기 면역 여부 반환
        public bool IsReceivingImpulse => impulseVelocity.sqrMagnitude > ExternalForceThreshold; // 순간 외부 힘 적용 여부 반환
        public bool IsReceivingExternalForce => Velocity.sqrMagnitude > ExternalForceThreshold; // 모든 외부 힘 적용 여부 반환
        public override bool CanReceivePush => enabled && stateController != null && stateController.CanMove && !IsRespawnProtected; // 부활 보호가 아닌 현재 플레이어 대상 가능 여부 반환

        private bool IsRespawnProtected => respawnProtectionController != null && respawnProtectionController.IsProtected; // 부활 보호 활성 여부 반환

        private void Awake() // 외부 힘 기능 준비
        { // 외부 힘 참조 준비 처리
            stateController = GetComponent<PlayerStateController>(); // 플레이어 상태 관리자 조회
            respawnProtectionController = GetComponent<PlayerRespawnProtectionController>(); // 부활 보호 관리자 조회
        } // 외부 힘 참조 준비 종료

        private void OnValidate() // Inspector 외부 힘 수치 보정
        { // 외부 힘 수치 보정 처리
            hitImmunityDuration = Mathf.Max(0f, hitImmunityDuration); // 음수가 없는 면역 시간 보장
            maximumCombinedPushSpeed = Mathf.Max(0f, maximumCombinedPushSpeed); // 음수가 없는 최대 합산 속도 보장
            impulseDeceleration = Mathf.Max(0f, impulseDeceleration); // 음수가 없는 감속도 보장
            platformVelocityGraceTime = Mathf.Max(0.01f, platformVelocityGraceTime); // 최소 발판 유예 시간 보장
        } // 외부 힘 수치 보정 종료

        public void Tick(float deltaTime) // 외부 힘 시간 갱신
        { // 외부 힘 프레임 처리
            float safeDeltaTime = Mathf.Max(0f, deltaTime); // 음수가 아닌 프레임 시간 보정
            hitImmunityRemaining = PushForceRules.CalculateImmunityRemaining(hitImmunityRemaining, safeDeltaTime); // 남은 연속 밀치기 면역 시간 감소
            carrierVelocityRemaining = Mathf.Max(0f, carrierVelocityRemaining - safeDeltaTime); // 남은 발판 속도 유지 시간 감소

            if (carrierVelocityRemaining <= 0f) // 발판 속도 갱신 만료 확인
            { // 발판 속도 만료 처리
                carrierVelocity = Vector3.zero; // 오래된 발판 전달 속도 제거
            } // 발판 속도 만료 처리 종료

            if (stateController == null || !stateController.CanMove) // 현재 이동 허용 상태 확인
            { // 이동 차단 처리
                ClearVelocity(); // 차단 상태의 모든 외부 속도 제거
                return; // 외부 힘 갱신 종료
            } // 이동 차단 처리 종료

            impulseVelocity = Vector3.MoveTowards(impulseVelocity, Vector3.zero, impulseDeceleration * safeDeltaTime); // 기존 순간 외부 속도 감속
            ApplyPendingPush(); // 이전 프레임에 모인 동시 밀치기 적용
        } // 외부 힘 프레임 처리 종료

        public override bool TryReceiveExternalForce(Vector3 direction, float force) // 기존 밀치기 외부 힘 적용 시도
        { // 기존 외부 힘 적용 처리
            Vector3 pushVelocity = PushForceRules.CreateHorizontalVelocity(direction, force); // 방향과 세기 기반 수평 밀치기 속도 생성
            ExternalForceRequest request = new ExternalForceRequest(pushVelocity, ExternalForceSource.Push, ExternalForceApplication.AddImpulse, true); // 누적형 밀치기 요청 생성
            return TryReceiveExternalForce(request); // 통합 외부 힘 처리 결과 반환
        } // 기존 외부 힘 적용 종료

        public override bool TryReceiveExternalForce(ExternalForceRequest request) // 통합 외부 힘 요청 적용 시도
        { // 통합 외부 힘 적용 처리
            if (!enabled || stateController == null || !stateController.CanMove) // 적용 가능 상태 확인
            { // 적용 불가 처리
                return false; // 외부 힘 적용 실패 반환
            } // 적용 불가 처리 종료

            if (request.Velocity.sqrMagnitude <= ExternalForceThreshold) // 요청 속도 유효성 확인
            { // 빈 요청 처리
                return false; // 외부 힘 적용 실패 반환
            } // 빈 요청 처리 종료

            if (request.Source == ExternalForceSource.Push) // 플레이어 밀치기 요청 확인
            { // 플레이어 밀치기 처리
                return TryQueuePush(request.Velocity); // 같은 프레임 밀치기 합산 결과 반환
            } // 플레이어 밀치기 처리 종료

            ApplyRequestVelocity(request); // 밀치기 외 요청 결합 방식 적용

            if (request.StartsHitImmunity) // 피격 면역 시작 요청 확인
            { // 피격 면역 시작 처리
                hitImmunityRemaining = hitImmunityDuration; // 연속 밀치기 면역 시간 적용
            } // 피격 면역 시작 처리 종료

            return true; // 외부 힘 적용 성공 반환
        } // 통합 외부 힘 적용 종료

        public bool ApplyPlatformVelocity(Vector3 velocity) // 이동 발판 속도 전달
        { // 발판 속도 적용 처리
            return TryReceiveExternalForce(ExternalForceRequest.CreatePlatform(velocity)); // 발판 요청 생성과 적용 결과 반환
        } // 발판 속도 적용 종료

        public bool ApplyObstacleImpulse(Vector3 direction, float force) // 장애물 순간 힘 전달
        { // 장애물 힘 적용 처리
            return TryReceiveExternalForce(ExternalForceRequest.CreateObstacle(direction, force)); // 장애물 요청 생성과 적용 결과 반환
        } // 장애물 힘 적용 종료

        public void ClearVelocity() // 모든 외부 속도 제거
        { // 외부 속도 제거 처리
            impulseVelocity = Vector3.zero; // 순간 외부 속도 제거
            carrierVelocity = Vector3.zero; // 발판 전달 속도 제거
            pendingPushVelocity = Vector3.zero; // 적용 대기 밀치기 속도 제거
            carrierVelocityRemaining = 0f; // 발판 속도 유지 시간 제거
            hasPendingPush = false; // 적용 대기 밀치기 상태 제거
        } // 외부 속도 제거 종료

        public void ResetExternalForce() // 외부 힘 전체 상태 초기화
        { // 외부 힘 초기화 처리
            ClearVelocity(); // 모든 외부 속도 제거
            hitImmunityRemaining = 0f; // 연속 밀치기 면역 시간 제거
        } // 외부 힘 초기화 종료

        private bool TryQueuePush(Vector3 pushVelocity) // 같은 프레임 밀치기 합산 대기열 추가
        { // 밀치기 대기열 처리
            if (!PushForceRules.CanAcceptPush(IsRespawnProtected, hitImmunityRemaining)) // 부활 보호와 피격 면역 확인
            { // 밀치기 수신 차단 처리
                return false; // 밀치기 적용 실패 반환
            } // 밀치기 수신 차단 처리 종료

            pendingPushVelocity = PushForceRules.CombineHorizontalVelocity(pendingPushVelocity, pushVelocity, maximumCombinedPushSpeed); // 같은 프레임 밀치기 벡터 합산
            hasPendingPush = pendingPushVelocity.sqrMagnitude > ExternalForceThreshold; // 유효한 합산 밀치기 존재 여부 저장
            return hasPendingPush; // 합산 밀치기 등록 결과 반환
        } // 밀치기 대기열 처리 종료

        private void ApplyPendingPush() // 같은 프레임에 모인 밀치기 일괄 적용
        { // 대기 밀치기 적용 처리
            if (!hasPendingPush) // 적용 대기 밀치기 존재 확인
            { // 대기 밀치기 없음 처리
                return; // 밀치기 적용 생략
            } // 대기 밀치기 없음 처리 종료

            impulseVelocity = PushForceRules.CombineHorizontalVelocity(impulseVelocity, pendingPushVelocity, maximumCombinedPushSpeed); // 기존 잔여 힘과 새 합산 힘 누적
            pendingPushVelocity = Vector3.zero; // 적용한 대기 밀치기 속도 제거
            hasPendingPush = false; // 대기 밀치기 상태 제거
            hitImmunityRemaining = hitImmunityDuration; // 일괄 밀치기 적용 뒤 연속 피격 면역 시작
        } // 대기 밀치기 적용 종료

        private void ApplyRequestVelocity(ExternalForceRequest request) // 결합 방식 기반 요청 속도 적용
        { // 요청 속도 적용 처리
            switch (request.Application) // 외부 힘 결합 방식 선택
            { // 결합 방식 분기 처리
                case ExternalForceApplication.ReplaceImpulse: // 순간 힘 교체 방식 확인
                    impulseVelocity = request.Velocity; // 기존 순간 힘을 요청 속도로 교체
                    break; // 순간 힘 교체 분기 종료
                case ExternalForceApplication.AddImpulse: // 순간 힘 누적 방식 확인
                    impulseVelocity += request.Velocity; // 기존 순간 힘에 요청 속도 추가
                    break; // 순간 힘 누적 분기 종료
                case ExternalForceApplication.SetCarrierVelocity: // 발판 속도 갱신 방식 확인
                    carrierVelocity = request.Velocity; // 현재 발판 전달 속도 갱신
                    carrierVelocityRemaining = platformVelocityGraceTime; // 발판 속도 유지 시간 갱신
                    break; // 발판 속도 갱신 분기 종료
            } // 결합 방식 분기 처리 종료
        } // 요청 속도 적용 종료
    } // 외부 힘 통합 관리 기능 묶음 종료
} // 플레이어 외부 힘 기능 묶음 종료
