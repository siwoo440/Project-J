using UnityEngine; // Unity 벡터와 시간 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스
{ // 네임스페이스 범위 시작
    [DisallowMultipleComponent] // 외부 힘 컴포넌트 중복 방지
    [RequireComponent(typeof(PlayerStateController))] // 플레이어 상태 컴포넌트 보장
    public sealed class PlayerExternalForceController : ExternalForceReceiver // 플레이어 외부 힘 관리 컴포넌트
    { // 클래스 범위 시작
        [SerializeField, Min(0f)] private float hitImmunityDuration = 0.8f; // 연속 피격 면역 시간
        [SerializeField, Min(0f)] private float horizontalDeceleration = 8f; // 수평 외부 힘 감속도

        private PlayerStateController stateController; // 플레이어 상태 관리자
        private Vector3 horizontalVelocity; // 현재 수평 외부 속도
        private float hitImmunityRemaining; // 남은 피격 면역 시간

        public Vector3 HorizontalVelocity => horizontalVelocity; // 현재 외부 수평 속도
        public bool IsForceImmune => hitImmunityRemaining > 0f; // 외부 힘 면역 여부

        private void Awake() // 외부 힘 기능 준비
        { // 메서드 범위 시작
            stateController = GetComponent<PlayerStateController>(); // 플레이어 상태 관리자 조회
        } // 메서드 범위 종료

        public void Tick(float deltaTime) // 외부 힘 시간 갱신
        { // 메서드 범위 시작
            hitImmunityRemaining = Mathf.Max(0f, hitImmunityRemaining - deltaTime); // 남은 피격 면역 시간 감소

            if (!stateController.CanMove) // 현재 이동 허용 상태 확인
            { // 조건 범위 시작
                horizontalVelocity = Vector3.zero; // 차단 상태 외부 속도 제거
                return; // 외부 힘 갱신 종료
            } // 조건 범위 종료

            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, horizontalDeceleration * deltaTime); // 수평 외부 속도 감속
        } // 메서드 범위 종료

        public override bool TryReceiveExternalForce(Vector3 direction, float force) // 플레이어 외부 힘 적용 시도
        { // 메서드 범위 시작
            if (!enabled || !stateController.CanMove || IsForceImmune) // 적용 가능 상태 확인
            { // 조건 범위 시작
                return false; // 외부 힘 적용 실패 반환
            } // 조건 범위 종료

            Vector3 horizontalDirection = Vector3.ProjectOnPlane(direction, Vector3.up); // 수평 힘 방향 계산

            if (horizontalDirection.sqrMagnitude <= 0.0001f) // 힘 방향 유효성 확인
            { // 조건 범위 시작
                return false; // 외부 힘 적용 실패 반환
            } // 조건 범위 종료

            horizontalVelocity = horizontalDirection.normalized * Mathf.Max(0f, force); // 수평 외부 속도 적용
            hitImmunityRemaining = hitImmunityDuration; // 연속 피격 면역 적용
            return true; // 외부 힘 적용 성공 반환
        } // 메서드 범위 종료

        public void ResetExternalForce() // 외부 힘 상태 초기화
        { // 메서드 범위 시작
            horizontalVelocity = Vector3.zero; // 외부 수평 속도 제거
            hitImmunityRemaining = 0f; // 피격 면역 시간 제거
        } // 메서드 범위 종료
    } // 클래스 범위 종료
} // 네임스페이스 범위 종료
