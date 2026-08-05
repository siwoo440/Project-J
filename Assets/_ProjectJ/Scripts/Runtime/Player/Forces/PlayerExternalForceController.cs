using UnityEngine; // Unity 벡터와 시간 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 외부 힘 범위
    [DisallowMultipleComponent] // 외부 힘 컴포넌트 중복 방지
    [RequireComponent(typeof(PlayerStateController))] // 플레이어 상태 컴포넌트 보장
    public sealed class PlayerExternalForceController : ExternalForceReceiver // 플레이어 외부 힘 통합 관리 컴포넌트 선언
    { // 외부 힘 통합 관리 범위
        private const float ExternalForceThreshold = 0.0001f; // 외부 힘 활성 판정 기준

        [SerializeField, Min(0f)] private float hitImmunityDuration = 0.8f; // 연속 밀치기 면역 시간
        [SerializeField, Min(0f)] private float impulseDeceleration = 8f; // 순간 외부 힘 감속도
        [SerializeField, Min(0.01f)] private float platformVelocityGraceTime = 0.1f; // 발판 속도 갱신 유예 시간

        private PlayerStateController stateController; // 플레이어 상태 관리자
        private Vector3 impulseVelocity; // 밀치기와 장애물 순간 속도
        private Vector3 carrierVelocity; // 이동 발판 전달 속도
        private float hitImmunityRemaining; // 남은 밀치기 면역 시간
        private float carrierVelocityRemaining; // 남은 발판 속도 유지 시간

        public Vector3 Velocity => impulseVelocity + carrierVelocity; // 최종 외부 속도 반환
        public Vector3 HorizontalVelocity => Vector3.ProjectOnPlane(Velocity, Vector3.up); // 최종 외부 수평 속도 반환
        public float VerticalVelocity => Velocity.y; // 최종 외부 수직 속도 반환
        public Vector3 ImpulseVelocity => impulseVelocity; // 현재 순간 외부 속도 반환
        public Vector3 CarrierVelocity => carrierVelocity; // 현재 발판 전달 속도 반환
        public bool IsForceImmune => hitImmunityRemaining > 0f; // 밀치기 면역 여부 반환
        public bool IsReceivingImpulse => impulseVelocity.sqrMagnitude > ExternalForceThreshold; // 밀치기 또는 장애물 순간 힘 적용 여부 반환
        public bool IsReceivingExternalForce => Velocity.sqrMagnitude > ExternalForceThreshold; // 모든 외부 힘 적용 여부 반환

        private void Awake() // 외부 힘 기능 준비
        { // 외부 힘 준비 범위
            stateController = GetComponent<PlayerStateController>(); // 플레이어 상태 관리자 조회
        } // 외부 힘 준비 범위 종료

        public void Tick(float deltaTime) // 외부 힘 시간 갱신
        { // 외부 힘 갱신 범위
            float safeDeltaTime = Mathf.Max(0f, deltaTime); // 음수가 아닌 프레임 시간 보정
            hitImmunityRemaining = Mathf.Max(0f, hitImmunityRemaining - safeDeltaTime); // 남은 밀치기 면역 시간 감소
            carrierVelocityRemaining = Mathf.Max(0f, carrierVelocityRemaining - safeDeltaTime); // 남은 발판 속도 유지 시간 감소

            if (carrierVelocityRemaining <= 0f) // 발판 속도 갱신 만료 확인
            { // 발판 속도 만료 범위
                carrierVelocity = Vector3.zero; // 오래된 발판 전달 속도 제거
            } // 발판 속도 만료 범위 종료

            if (!stateController.CanMove) // 현재 이동 허용 상태 확인
            { // 이동 차단 범위
                ClearVelocity(); // 차단 상태의 모든 외부 속도 제거
                return; // 외부 힘 갱신 종료
            } // 이동 차단 범위 종료

            impulseVelocity = Vector3.MoveTowards(impulseVelocity, Vector3.zero, impulseDeceleration * safeDeltaTime); // 순간 외부 속도 감속
        } // 외부 힘 갱신 범위 종료

        public override bool TryReceiveExternalForce(Vector3 direction, float force) // 기존 밀치기 외부 힘 적용 시도
        { // 기존 외부 힘 적용 범위
            return TryReceiveExternalForce(ExternalForceRequest.CreatePush(direction, force)); // 기존 요청을 밀치기 요청으로 변환
        } // 기존 외부 힘 적용 범위 종료

        public override bool TryReceiveExternalForce(ExternalForceRequest request) // 통합 외부 힘 요청 적용 시도
        { // 통합 외부 힘 적용 범위
            if (!enabled || stateController == null || !stateController.CanMove) // 적용 가능 상태 확인
            { // 적용 불가 범위
                return false; // 외부 힘 적용 실패 반환
            } // 적용 불가 범위 종료

            if (request.Velocity.sqrMagnitude <= ExternalForceThreshold) // 요청 속도 유효성 확인
            { // 빈 요청 범위
                return false; // 외부 힘 적용 실패 반환
            } // 빈 요청 범위 종료

            if (request.Source == ExternalForceSource.Push && IsForceImmune) // 밀치기 요청의 피격 면역 확인
            { // 밀치기 면역 범위
                return false; // 면역 중 밀치기 적용 실패 반환
            } // 밀치기 면역 범위 종료

            ApplyRequestVelocity(request); // 요청 결합 방식에 따른 외부 속도 적용

            if (request.StartsHitImmunity) // 피격 면역 시작 요청 확인
            { // 피격 면역 시작 범위
                hitImmunityRemaining = hitImmunityDuration; // 연속 밀치기 면역 시간 적용
            } // 피격 면역 시작 범위 종료

            return true; // 외부 힘 적용 성공 반환
        } // 통합 외부 힘 적용 범위 종료

        public bool ApplyPlatformVelocity(Vector3 velocity) // 이동 발판 속도 전달
        { // 발판 속도 적용 범위
            return TryReceiveExternalForce(ExternalForceRequest.CreatePlatform(velocity)); // 발판 요청 생성과 적용 결과 반환
        } // 발판 속도 적용 범위 종료

        public bool ApplyObstacleImpulse(Vector3 direction, float force) // 장애물 순간 힘 전달
        { // 장애물 힘 적용 범위
            return TryReceiveExternalForce(ExternalForceRequest.CreateObstacle(direction, force)); // 장애물 요청 생성과 적용 결과 반환
        } // 장애물 힘 적용 범위 종료

        public void ClearVelocity() // 모든 외부 속도 제거
        { // 외부 속도 제거 범위
            impulseVelocity = Vector3.zero; // 순간 외부 속도 제거
            carrierVelocity = Vector3.zero; // 발판 전달 속도 제거
            carrierVelocityRemaining = 0f; // 발판 속도 유지 시간 제거
        } // 외부 속도 제거 범위 종료

        public void ResetExternalForce() // 외부 힘 전체 상태 초기화
        { // 외부 힘 초기화 범위
            ClearVelocity(); // 모든 외부 속도 제거
            hitImmunityRemaining = 0f; // 밀치기 면역 시간 제거
        } // 외부 힘 초기화 범위 종료

        private void ApplyRequestVelocity(ExternalForceRequest request) // 결합 방식 기반 요청 속도 적용
        { // 요청 속도 적용 범위
            switch (request.Application) // 외부 힘 결합 방식 선택
            { // 결합 방식 분기 범위
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
            } // 결합 방식 분기 범위 종료
        } // 요청 속도 적용 범위 종료
    } // 외부 힘 통합 관리 범위 종료
} // 플레이어 외부 힘 범위 종료
