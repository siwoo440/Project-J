using UnityEngine; // Unity 이동과 물리 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 기능 묶음
    [DisallowMultipleComponent] // 더미 컴포넌트 중복 방지
    [RequireComponent(typeof(CharacterController))] // 캐릭터 충돌 제어기 보장
    public sealed class PushableDummy : ExternalForceReceiver // 밀치기와 높이 측정용 더미 컴포넌트 선언
    { // 밀치기 더미 기능 묶음
        private const float HeightComparisonTolerance = 0.05f; // 현재 높이 도달 시간 갱신 허용 오차

        [SerializeField] private string competitorName = "DUMMY"; // 순위표 표시 이름
        [SerializeField] private Transform heightOrigin; // 높이 측정 기준점
        [SerializeField, Min(0f)] private float hitImmunityDuration = 0.8f; // 연속 피격 면역 시간
        [SerializeField, Min(0f)] private float maximumCombinedPushSpeed = 10f; // 동시와 연속 밀치기 최대 합산 속도
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
        private Vector3 pendingPushVelocity; // 같은 프레임에 모인 밀치기 속도
        private float verticalVelocity; // 현재 수직 속도
        private float hitImmunityRemaining; // 남은 연속 피격 면역 시간
        private float heightOriginY; // 높이 기준 Y 좌표
        private float observedCurrentHeight; // 마지막으로 기록한 현재 높이
        private bool hasPendingPush; // 적용 대기 중인 밀치기 존재 여부

        public string CompetitorName => string.IsNullOrWhiteSpace(competitorName) ? gameObject.name : competitorName; // 유효한 참가자 이름 반환
        public float CurrentHeight => Mathf.Max(0f, transform.position.y - heightOriginY); // 현재 높이 반환
        public float CurrentHeightReachedAt { get; private set; } // 현재 표시 높이 도달 시간 반환
        public float HighestHeight { get; private set; } // 최고 높이 반환
        public float HighestHeightReachedAt { get; private set; } // 최고 높이 최초 도달 시간 반환
        public float HitImmunityRemaining => hitImmunityRemaining; // 남은 피격 면역 시간 반환
        public bool IsPushImmune => hitImmunityRemaining > 0f; // 연속 피격 면역 상태 반환
        public override bool CanReceivePush => enabled && characterController != null && characterController.enabled; // 현재 더미 밀치기 대상 가능 여부 반환

        private void Awake() // 더미 이동과 높이 정보 준비
        { // 더미 기능 준비 처리
            characterController = GetComponent<CharacterController>(); // 캐릭터 충돌 제어기 조회
            spawnPosition = transform.position; // 최초 부활 위치 저장
            spawnRotation = transform.rotation; // 최초 부활 회전 저장
            heightOriginY = heightOrigin != null ? heightOrigin.position.y : 0f; // 높이 기준 좌표 저장
            observedCurrentHeight = CurrentHeight; // 최초 현재 높이 저장
            CurrentHeightReachedAt = 0f; // 최초 현재 높이 도달 시간 저장
            HighestHeight = CurrentHeight; // 최초 최고 높이 저장
            HighestHeightReachedAt = 0f; // 최초 최고 높이 도달 시간 저장
        } // 더미 기능 준비 종료

        private void OnValidate() // Inspector 더미 수치 보정
        { // 더미 수치 보정 처리
            hitImmunityDuration = Mathf.Max(0f, hitImmunityDuration); // 음수가 없는 면역 시간 보장
            maximumCombinedPushSpeed = Mathf.Max(0f, maximumCombinedPushSpeed); // 음수가 없는 합산 속도 보장
            horizontalDeceleration = Mathf.Max(0f, horizontalDeceleration); // 음수가 없는 감속도 보장
            maximumFallSpeed = Mathf.Max(0f, maximumFallSpeed); // 음수가 없는 최대 낙하 속도 보장
            respawnVerticalOffset = Mathf.Max(0f, respawnVerticalOffset); // 음수가 없는 부활 보정값 보장
        } // 더미 수치 보정 종료

        private void Update() // 더미 외부 힘 이동과 높이 갱신
        { // 더미 프레임 처리
            float deltaTime = Time.deltaTime; // 현재 프레임 시간 조회
            hitImmunityRemaining = PushForceRules.CalculateImmunityRemaining(hitImmunityRemaining, deltaTime); // 연속 피격 면역 시간 감소
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, horizontalDeceleration * deltaTime); // 수평 외부 속도 감속

            if (characterController.isGrounded && verticalVelocity < 0f) // 접지 중 하강 상태 확인
            { // 접지 중력 처리
                verticalVelocity = groundedGravity; // 접지 유지 중력 적용
            } // 접지 중력 처리 종료
            else // 공중 상태 확인
            { // 공중 중력 처리
                verticalVelocity += gravityAcceleration * deltaTime; // 중력 가속도 적용
                verticalVelocity = Mathf.Max(verticalVelocity, -maximumFallSpeed); // 최대 낙하 속도 제한
            } // 공중 중력 처리 종료

            Vector3 frameVelocity = horizontalVelocity + Vector3.up * verticalVelocity; // 현재 전체 이동 속도 계산
            CollisionFlags collisionFlags = characterController.Move(frameVelocity * deltaTime); // 더미 이동 실행

            if ((collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) // 천장 상승 충돌 확인
            { // 천장 충돌 처리
                verticalVelocity = 0f; // 상승 속도 제거
            } // 천장 충돌 처리 종료

            UpdateHeightRecords(); // 현재 높이와 최고 높이 기록 갱신

            if (transform.position.y <= fallLimitY) // 추락 한계 통과 확인
            { // 더미 추락 처리
                RespawnAtStart(); // 최초 위치에서 더미 부활
            } // 더미 추락 처리 종료
        } // 더미 프레임 처리 종료

        private void LateUpdate() // 같은 프레임 밀치기 일괄 적용
        { // 밀치기 일괄 적용 처리
            ApplyPendingPush(); // 모인 밀치기 벡터 합산 적용
        } // 밀치기 일괄 적용 종료

        public override bool TryReceiveExternalForce(Vector3 direction, float force) // 더미 외부 힘 적용 시도
        { // 더미 외부 힘 처리
            Vector3 pushVelocity = PushForceRules.CreateHorizontalVelocity(direction, force); // 방향과 세기 기반 수평 밀치기 생성
            return TryQueuePush(pushVelocity); // 같은 프레임 밀치기 대기열 등록 결과 반환
        } // 더미 외부 힘 처리 종료

        public override bool TryReceiveExternalForce(ExternalForceRequest request) // 통합 외부 힘 요청 적용 시도
        { // 통합 외부 힘 처리
            if (request.Source != ExternalForceSource.Push) // 플레이어 밀치기 외 요청 확인
            { // 지원하지 않는 힘 처리
                return false; // 더미 외 외부 힘 적용 실패 반환
            } // 지원하지 않는 힘 처리 종료

            return TryQueuePush(request.Velocity); // 밀치기 속도 대기열 등록 결과 반환
        } // 통합 외부 힘 처리 종료

        public bool TryReceivePush(Vector3 direction, float force) // 기존 밀치기 호출 호환 처리
        { // 기존 밀치기 호출 처리
            return TryReceiveExternalForce(direction, force); // 공통 외부 힘 처리 결과 반환
        } // 기존 밀치기 호출 처리 종료

        public void StopForMatchEnd() // 경기 종료 순간 더미 이동과 밀치기 중단
        { // 더미 경기 종료 처리
            horizontalVelocity = Vector3.zero; // 남은 수평 속도 제거
            pendingPushVelocity = Vector3.zero; // 적용 대기 밀치기 제거
            verticalVelocity = 0f; // 남은 수직 속도 제거
            hitImmunityRemaining = 0f; // 남은 피격 면역 제거
            hasPendingPush = false; // 적용 대기 밀치기 상태 제거
            enabled = false; // 경기 종료 뒤 더미 갱신 비활성화
        } // 더미 경기 종료 처리 종료

        private bool TryQueuePush(Vector3 pushVelocity) // 같은 프레임 밀치기 합산 대기열 추가
        { // 밀치기 대기열 처리
            if (!CanReceivePush || !PushForceRules.CanAcceptPush(false, hitImmunityRemaining) || pushVelocity.sqrMagnitude <= 0.0001f) // 면역과 동작과 유효 속도 확인
            { // 밀치기 수신 차단 처리
                return false; // 밀치기 적용 실패 반환
            } // 밀치기 수신 차단 처리 종료

            pendingPushVelocity = PushForceRules.CombineHorizontalVelocity(pendingPushVelocity, pushVelocity, maximumCombinedPushSpeed); // 같은 프레임 밀치기 벡터 합산
            hasPendingPush = pendingPushVelocity.sqrMagnitude > 0.0001f; // 유효한 합산 밀치기 존재 여부 저장
            return hasPendingPush; // 합산 밀치기 등록 결과 반환
        } // 밀치기 대기열 처리 종료

        private void ApplyPendingPush() // 같은 프레임에 모인 밀치기 일괄 적용
        { // 대기 밀치기 적용 처리
            if (!hasPendingPush) // 적용 대기 밀치기 존재 확인
            { // 대기 밀치기 없음 처리
                return; // 밀치기 적용 생략
            } // 대기 밀치기 없음 처리 종료

            horizontalVelocity = PushForceRules.CombineHorizontalVelocity(horizontalVelocity, pendingPushVelocity, maximumCombinedPushSpeed); // 기존 잔여 힘과 새 합산 힘 누적
            pendingPushVelocity = Vector3.zero; // 적용한 대기 밀치기 속도 제거
            hasPendingPush = false; // 대기 밀치기 상태 제거
            hitImmunityRemaining = hitImmunityDuration; // 일괄 밀치기 적용 뒤 연속 피격 면역 시작
        } // 대기 밀치기 적용 종료

        private void UpdateHeightRecords() // 현재 높이 도달 시간과 최고 높이 갱신
        { // 높이 기록 갱신 처리
            float currentHeight = CurrentHeight; // 현재 더미 높이 조회

            if (Mathf.Abs(currentHeight - observedCurrentHeight) > HeightComparisonTolerance) // 표시 높이 변경 여부 확인
            { // 현재 높이 도달 시간 갱신 처리
                observedCurrentHeight = currentHeight; // 관찰한 현재 높이 갱신
                CurrentHeightReachedAt = Time.timeSinceLevelLoad; // 현재 표시 높이 도달 시간 저장
            } // 현재 높이 도달 시간 갱신 종료

            if (currentHeight > HighestHeight + 0.001f) // 새 최고 높이 확인
            { // 최고 높이 갱신 처리
                HighestHeight = currentHeight; // 최고 높이 갱신
                HighestHeightReachedAt = Time.timeSinceLevelLoad; // 최고 높이 도달 시간 저장
            } // 최고 높이 갱신 종료
        } // 높이 기록 갱신 종료

        private void RespawnAtStart() // 더미 최초 위치 복귀
        { // 더미 부활 처리
            characterController.enabled = false; // 위치 이동 전 충돌 비활성화
            transform.SetPositionAndRotation(spawnPosition + Vector3.up * respawnVerticalOffset, spawnRotation); // 최초 위치와 회전 적용
            characterController.enabled = true; // 충돌 다시 활성화
            horizontalVelocity = Vector3.zero; // 수평 속도 초기화
            pendingPushVelocity = Vector3.zero; // 대기 밀치기 속도 초기화
            verticalVelocity = groundedGravity; // 수직 속도 초기화
            hitImmunityRemaining = 0f; // 피격 면역 초기화
            hasPendingPush = false; // 대기 밀치기 상태 초기화
            observedCurrentHeight = CurrentHeight; // 부활 위치 높이 저장
            CurrentHeightReachedAt = Time.timeSinceLevelLoad; // 부활 위치 도달 시간 저장
        } // 더미 부활 처리 종료
    } // 밀치기 더미 기능 묶음 종료
} // 플레이어 기능 묶음 종료
