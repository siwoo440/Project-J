using UnityEngine; // Unity 이동과 물리 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스
{ // 네임스페이스 범위 시작
    [DisallowMultipleComponent] // 더미 컴포넌트 중복 방지
    [RequireComponent(typeof(CharacterController))] // 캐릭터 충돌 제어기 보장
    public sealed class PushableDummy : ExternalForceReceiver // 밀치기와 높이 측정용 더미 컴포넌트
    { // 클래스 범위 시작
        [SerializeField] private string competitorName = "DUMMY"; // 순위표 표시 이름
        [SerializeField] private Transform heightOrigin; // 높이 측정 기준점
        [SerializeField, Min(0f)] private float hitImmunityDuration = 0.8f; // 연속 피격 면역 시간
        [SerializeField, Min(0f)] private float horizontalDeceleration = 8f; // 수평 외부 힘 감속도
        [SerializeField] private float gravityAcceleration = -30f; // 더미 중력 가속도
        [SerializeField, Min(0f)] private float maximumFallSpeed = 25f; // 더미 최대 낙하 속도
        [SerializeField] private float groundedGravity = -2f; // 접지 유지 중력
        [SerializeField] private float fallLimitY = -5f; // 더미 추락 판정 높이
        [SerializeField, Min(0f)] private float respawnVerticalOffset = 0.05f; // 더미 부활 수직 보정값

        private CharacterController characterController; // 더미 충돌 제어기
        private Vector3 spawnPosition; // 더미 최초 위치
        private Quaternion spawnRotation; // 더미 최초 회전
        private Vector3 horizontalVelocity; // 현재 수평 외부 속도
        private float verticalVelocity; // 현재 수직 속도
        private float hitImmunityRemaining; // 남은 연속 피격 면역 시간
        private float heightOriginY; // 높이 기준 Y 좌표

        public string CompetitorName => string.IsNullOrWhiteSpace(competitorName) ? gameObject.name : competitorName; // 유효한 참가자 이름 반환
        public float CurrentHeight => Mathf.Max(0f, transform.position.y - heightOriginY); // 현재 높이 반환
        public float HighestHeight { get; private set; } // 최고 높이 반환
        public float HighestHeightReachedAt { get; private set; } // 최고 높이 최초 도달 시간
        public bool IsPushImmune => hitImmunityRemaining > 0f; // 연속 피격 면역 상태

        private void Awake() // 더미 이동과 높이 정보 준비
        { // 메서드 범위 시작
            characterController = GetComponent<CharacterController>(); // 캐릭터 충돌 제어기 조회
            spawnPosition = transform.position; // 최초 부활 위치 저장
            spawnRotation = transform.rotation; // 최초 부활 회전 저장
            heightOriginY = heightOrigin != null ? heightOrigin.position.y : 0f; // 높이 기준 좌표 저장
            HighestHeight = CurrentHeight; // 최초 최고 높이 저장
            HighestHeightReachedAt = 0f; // 최초 도달 시간 저장
        } // 메서드 범위 종료

        private void Update() // 더미 외부 힘 이동과 높이 갱신
        { // 메서드 범위 시작
            float deltaTime = Time.deltaTime; // 현재 프레임 시간
            hitImmunityRemaining = Mathf.Max(0f, hitImmunityRemaining - deltaTime); // 연속 피격 면역 시간 감소
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, horizontalDeceleration * deltaTime); // 수평 외부 속도 감속

            if (characterController.isGrounded && verticalVelocity < 0f) // 접지 중 하강 상태 확인
            { // 조건 범위 시작
                verticalVelocity = groundedGravity; // 접지 유지 중력 적용
            } // 조건 범위 종료
            else // 공중 상태 분기
            { // 분기 범위 시작
                verticalVelocity += gravityAcceleration * deltaTime; // 중력 가속도 적용
                verticalVelocity = Mathf.Max(verticalVelocity, -maximumFallSpeed); // 최대 낙하 속도 제한
            } // 분기 범위 종료

            Vector3 frameVelocity = horizontalVelocity + Vector3.up * verticalVelocity; // 현재 전체 이동 속도 계산
            CollisionFlags collisionFlags = characterController.Move(frameVelocity * deltaTime); // 더미 이동 실행

            if ((collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) // 천장 상승 충돌 확인
            { // 조건 범위 시작
                verticalVelocity = 0f; // 상승 속도 제거
            } // 조건 범위 종료

            if (CurrentHeight > HighestHeight + 0.001f) // 새 최고 높이 확인
            { // 조건 범위 시작
                HighestHeight = CurrentHeight; // 최고 높이 갱신
                HighestHeightReachedAt = Time.timeSinceLevelLoad; // 최고 높이 도달 시간 저장
            } // 조건 범위 종료

            if (transform.position.y <= fallLimitY) // 추락 한계 통과 확인
            { // 조건 범위 시작
                RespawnAtStart(); // 최초 위치에서 더미 부활
            } // 조건 범위 종료
        } // 메서드 범위 종료

        public override bool TryReceiveExternalForce(Vector3 direction, float force) // 더미 외부 힘 적용 시도
        { // 메서드 범위 시작
            if (IsPushImmune || !enabled || !characterController.enabled) // 면역과 동작 가능 상태 확인
            { // 조건 범위 시작
                return false; // 외부 힘 적용 실패 반환
            } // 조건 범위 종료

            Vector3 horizontalDirection = Vector3.ProjectOnPlane(direction, Vector3.up); // 외부 힘 수평 방향 계산

            if (horizontalDirection.sqrMagnitude <= 0.0001f) // 외부 힘 방향 유효성 확인
            { // 조건 범위 시작
                return false; // 외부 힘 적용 실패 반환
            } // 조건 범위 종료

            horizontalVelocity = horizontalDirection.normalized * Mathf.Max(0f, force); // 수평 외부 속도 적용
            hitImmunityRemaining = hitImmunityDuration; // 연속 피격 면역 적용
            return true; // 외부 힘 적용 성공 반환
        } // 메서드 범위 종료

        public bool TryReceivePush(Vector3 direction, float force) // 기존 밀치기 호출 호환 처리
        { // 메서드 범위 시작
            return TryReceiveExternalForce(direction, force); // 공통 외부 힘 처리 반환
        } // 메서드 범위 종료

        private void RespawnAtStart() // 더미 최초 위치 복귀
        { // 메서드 범위 시작
            characterController.enabled = false; // 위치 이동 전 충돌 비활성화
            transform.SetPositionAndRotation(spawnPosition + Vector3.up * respawnVerticalOffset, spawnRotation); // 최초 위치와 회전 적용
            characterController.enabled = true; // 충돌 다시 활성화
            horizontalVelocity = Vector3.zero; // 수평 속도 초기화
            verticalVelocity = groundedGravity; // 수직 속도 초기화
            hitImmunityRemaining = 0f; // 피격 면역 초기화
        } // 메서드 범위 종료
    } // 클래스 범위 종료
} // 네임스페이스 범위 종료
