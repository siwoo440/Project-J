using UnityEngine; // Unity 데이터 에셋 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    [CreateAssetMenu(fileName = "MAP-TRV-001_DefaultTraversal", menuName = "Project J/Map Generation/Traversal Profile")] // 이동 능력 데이터 생성 메뉴 등록
    public sealed class MapTraversalProfile : ScriptableObject // 맵 통과 능력 데이터 선언
    { // 맵 통과 능력 데이터 묶음
        [SerializeField, Min(0.01f)] private float standingHeight = 2f; // 서기 충돌체 높이
        [SerializeField, Min(0.01f)] private float crouchingHeight = 1.2f; // 앉기 충돌체 높이
        [SerializeField, Min(0.01f)] private float controllerRadius = 0.45f; // 플레이어 충돌체 반지름
        [SerializeField, Min(0.01f)] private float moveSpeed = 6f; // 기본 이동 속도
        [SerializeField, Min(0.01f)] private float jumpHeight = 2.4f; // 기본 점프 높이
        [SerializeField, Min(0.01f)] private float gravityMagnitude = 25f; // 중력 가속도 절댓값
        [SerializeField, Min(0f)] private float maximumSafeDropHeight = 3f; // 최대 안전 낙하 높이
        [SerializeField, Range(0.1f, 1f)] private float jumpDistanceSafetyRatio = 0.8f; // 점프 거리 안전 비율
        [SerializeField, Min(0f)] private float clearancePadding = 0.1f; // 통로 높이 안전 여유

        public float StandingHeight => standingHeight; // 서기 충돌체 높이 반환
        public float CrouchingHeight => crouchingHeight; // 앉기 충돌체 높이 반환
        public float ControllerRadius => controllerRadius; // 플레이어 충돌체 반지름 반환
        public float MoveSpeed => moveSpeed; // 기본 이동 속도 반환
        public float JumpHeight => jumpHeight; // 기본 점프 높이 반환
        public float GravityMagnitude => gravityMagnitude; // 중력 가속도 절댓값 반환
        public float MaximumSafeDropHeight => maximumSafeDropHeight; // 최대 안전 낙하 높이 반환
        public float JumpDistanceSafetyRatio => jumpDistanceSafetyRatio; // 점프 거리 안전 비율 반환
        public float ClearancePadding => clearancePadding; // 통로 높이 안전 여유 반환
        public float MinimumCrouchClearance => crouchingHeight + clearancePadding; // 안전한 앉기 통로 최소 높이 반환
        public float MaximumCrouchOnlyClearance => standingHeight - clearancePadding; // 앉기가 필요한 통로 최대 높이 반환
        public float MaximumSafeJumpDistance => MapModuleValidationRules.CalculateSafeJumpDistance(moveSpeed, jumpHeight, gravityMagnitude, jumpDistanceSafetyRatio); // 안전한 최대 점프 거리 반환
        public float MaximumSafeJumpRise => Mathf.Max(0f, jumpHeight - clearancePadding); // 안전한 최대 점프 상승 높이 반환

        private void OnValidate() // Inspector 이동 수치 보정
        { // 이동 수치 보정 처리
            standingHeight = Mathf.Max(0.01f, standingHeight); // 서기 높이 양수 보장
            crouchingHeight = Mathf.Clamp(crouchingHeight, 0.01f, standingHeight); // 앉기 높이 범위 보장
            controllerRadius = Mathf.Max(0.01f, controllerRadius); // 충돌체 반지름 양수 보장
            moveSpeed = Mathf.Max(0.01f, moveSpeed); // 이동 속도 양수 보장
            jumpHeight = Mathf.Max(0.01f, jumpHeight); // 점프 높이 양수 보장
            gravityMagnitude = Mathf.Max(0.01f, gravityMagnitude); // 중력 크기 양수 보장
            maximumSafeDropHeight = Mathf.Max(0f, maximumSafeDropHeight); // 안전 낙하 높이 음수 방지
            jumpDistanceSafetyRatio = Mathf.Clamp(jumpDistanceSafetyRatio, 0.1f, 1f); // 점프 안전 비율 범위 보장
            clearancePadding = Mathf.Max(0f, clearancePadding); // 통로 여유 음수 방지
        } // 이동 수치 보정 종료

#if UNITY_EDITOR // Unity Editor 전용 설정 시작
        public void ConfigureForEditor(float newStandingHeight, float newCrouchingHeight, float newControllerRadius, float newMoveSpeed, float newJumpHeight, float newGravityMagnitude, float newMaximumSafeDropHeight, float newJumpDistanceSafetyRatio, float newClearancePadding) // Editor 도구용 이동 수치 설정
        { // Editor 이동 수치 설정 처리
            standingHeight = newStandingHeight; // 새 서기 높이 저장
            crouchingHeight = newCrouchingHeight; // 새 앉기 높이 저장
            controllerRadius = newControllerRadius; // 새 충돌체 반지름 저장
            moveSpeed = newMoveSpeed; // 새 이동 속도 저장
            jumpHeight = newJumpHeight; // 새 점프 높이 저장
            gravityMagnitude = newGravityMagnitude; // 새 중력 크기 저장
            maximumSafeDropHeight = newMaximumSafeDropHeight; // 새 안전 낙하 높이 저장
            jumpDistanceSafetyRatio = newJumpDistanceSafetyRatio; // 새 점프 안전 비율 저장
            clearancePadding = newClearancePadding; // 새 통로 여유 저장
            OnValidate(); // 설정값 즉시 보정
        } // Editor 이동 수치 설정 종료
#endif // Unity Editor 전용 설정 종료
    } // 맵 통과 능력 데이터 묶음 종료
} // 맵 생성 기능 묶음 종료

