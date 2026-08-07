using ProjectJ.Data; // 아이템 효과 종류 참조
using ProjectJ.Player; // 외부 힘 수신 대상 기능 참조
using UnityEngine; // Unity 추적 이동과 오브젝트 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 추적 오브젝트당 효과 한 개만 허용
    public sealed class HomingItemEffect : MonoBehaviour // 유도탄과 드론 공통 추적 효과 선언
    { // 추적 효과 묶음
        private ItemEffectType effectType; // 유도탄 또는 드론 효과 종류 저장
        private Transform ownerRoot; // 발사한 플레이어 루트 저장
        private PlayerP2ItemEffectController ownerEffectController; // 소유자 상태와 정리 관리자 저장
        private ExternalForceReceiver currentTarget; // 현재 추적 대상 저장
        private float movementSpeed; // 초당 추적 이동 속도 저장
        private float pushForce; // 적중 시 밀치기 힘 저장
        private float lifeTimeRemaining; // 남은 추적 유지 시간 저장
        private float hitRadius; // 목표 도달 판정 반경 저장
        private int retargetsRemaining; // 남은 목표 재선정 횟수 저장
        private Vector3 lastDirection = Vector3.forward; // 목표 누락 시 유지할 마지막 방향 저장

        public ExternalForceReceiver CurrentTarget => currentTarget; // 현재 추적 대상 반환
        public int RetargetsRemaining => retargetsRemaining; // 남은 재선정 횟수 반환

        public bool Configure(ItemEffectType newEffectType, Transform newOwnerRoot, PlayerP2ItemEffectController newOwnerEffectController, float newMovementSpeed, float newPushForce, float newLifeTime, float newHitRadius, int maximumRetargetCount, Color visualColor) // 유도탄 또는 드론 추적 효과 구성
        { // 추적 효과 구성 처리
            effectType = newEffectType; // 추적 효과 종류 저장
            ownerRoot = newOwnerRoot; // 발사자 루트 저장
            ownerEffectController = newOwnerEffectController; // 발사자 P2 효과 관리자 저장
            movementSpeed = Mathf.Max(0.1f, newMovementSpeed); // 최소 추적 이동 속도 저장
            pushForce = Mathf.Max(0f, newPushForce); // 음수가 없는 밀치기 힘 저장
            lifeTimeRemaining = Mathf.Max(0.1f, newLifeTime); // 최소 유지 시간 저장
            hitRadius = Mathf.Max(0.1f, newHitRadius); // 최소 적중 반경 저장
            retargetsRemaining = Mathf.Max(0, maximumRetargetCount); // 음수가 없는 재선정 횟수 저장
            CreateVisual(visualColor); // 임시 추적 물체 표시 생성

            if (!TrySelectTarget(false)) // 최초 추적 대상 검색 여부 확인
            { // 최초 대상 없음 처리
                Destroy(gameObject); // 사용할 수 없는 추적 오브젝트 제거
                return false; // 추적 효과 구성 실패 반환
            } // 최초 대상 없음 처리 종료

            return true; // 추적 효과 구성 성공 반환
        } // 추적 효과 구성 처리 종료

        private void Update() // 목표 추적과 적중 상태 갱신
        { // 추적 효과 프레임 처리
            float deltaTime = Mathf.Max(0f, Time.deltaTime); // 음수가 없는 프레임 시간 계산
            lifeTimeRemaining = Mathf.Max(0f, lifeTimeRemaining - deltaTime); // 남은 유지 시간 감소

            if (lifeTimeRemaining <= 0f || ownerRoot == null || ownerEffectController == null || !ownerEffectController.CanOwnedEffectsContinue) // 유지 시간과 소유자 상태 확인
            { // 추적 효과 종료 처리
                Destroy(gameObject); // 추적 오브젝트 제거
                return; // 현재 프레임 추적 종료
            } // 추적 효과 종료 처리 종료

            if (!IsValidTarget(currentTarget)) // 현재 목표 유효성 확인
            { // 현재 목표 무효 처리
                if (retargetsRemaining <= 0 || !TrySelectTarget(true)) // 재선정 횟수와 새 목표 존재 여부 확인
                { // 재선정 실패 처리
                    Destroy(gameObject); // 목표 없는 추적 오브젝트 제거
                    return; // 현재 프레임 추적 종료
                } // 재선정 실패 처리 종료
            } // 현재 목표 무효 처리 종료

            Vector3 targetPosition = currentTarget.ForceReceiverTransform.position + Vector3.up * 0.75f; // 목표 몸통 높이 위치 계산
            Vector3 toTarget = targetPosition - transform.position; // 현재 위치에서 목표 방향 계산

            if (toTarget.sqrMagnitude <= hitRadius * hitRadius) // 목표 적중 반경 도달 여부 확인
            { // 목표 적중 처리
                ApplyHit(); // 목표 밀치기 효과 적용
                return; // 적중 후 추적 종료
            } // 목표 적중 처리 종료

            lastDirection = toTarget.sqrMagnitude <= 0.0001f ? lastDirection : toTarget.normalized; // 현재 유효 추적 방향 저장
            transform.position += lastDirection * movementSpeed * deltaTime; // 장애물 충돌 없이 목표 방향 이동

            if (lastDirection.sqrMagnitude > 0.0001f) // 유효한 진행 방향 여부 확인
            { // 추적 오브젝트 회전 처리
                transform.rotation = Quaternion.LookRotation(lastDirection, Vector3.up); // 진행 방향을 바라보도록 회전
            } // 추적 오브젝트 회전 처리 종료
        } // 추적 효과 프레임 처리 종료

        private bool TrySelectTarget(bool consumesRetarget) // 효과 종류에 맞는 새 추적 대상 검색
        { // 새 추적 대상 검색 처리
            ExternalForceReceiver[] receivers = FindObjectsByType<ExternalForceReceiver>(FindObjectsSortMode.None); // 현재 Scene 전체 외부 힘 수신 대상 조회
            ExternalForceReceiver bestTarget = null; // 검색된 최적 대상 저장
            float bestDistance = float.PositiveInfinity; // 최적 대상까지 거리 초기화
            float bestHeight = float.NegativeInfinity; // 드론 최적 대상 높이 초기화

            for (int index = 0; index < receivers.Length; index++) // 전체 외부 힘 대상 순회
            { // 현재 추적 후보 확인
                ExternalForceReceiver candidate = receivers[index]; // 현재 후보 대상 조회

                if (!IsValidTarget(candidate) || candidate == currentTarget) // 후보 유효성과 기존 무효 목표 반복 여부 확인
                { // 사용할 수 없는 후보 처리
                    continue; // 다음 후보로 이동
                } // 사용할 수 없는 후보 처리 종료

                float candidateDistance = Vector3.Distance(transform.position, candidate.ForceReceiverTransform.position); // 후보까지 현재 거리 계산

                if (effectType == ItemEffectType.HomingMissile) // 유도탄의 가까운 대상 규칙 확인
                { // 유도탄 후보 비교 처리
                    if (candidateDistance < bestDistance) // 더 가까운 후보 여부 확인
                    { // 유도탄 최적 후보 갱신
                        bestTarget = candidate; // 새 가까운 목표 저장
                        bestDistance = candidateDistance; // 새 가까운 거리 저장
                    } // 유도탄 최적 후보 갱신 종료

                    continue; // 다음 후보로 이동
                } // 유도탄 후보 비교 처리 종료

                float candidateHeight = candidate.ForceReceiverTransform.position.y; // 드론 후보 발 높이 조회

                if (bestTarget == null || P2ItemRules.IsHigherPriorityTarget(candidateHeight, bestHeight, candidateDistance, bestDistance)) // 더 높은 1위 후보 또는 같은 높이 가까운 후보 여부 확인
                { // 드론 최적 후보 갱신
                    bestTarget = candidate; // 새 1위 목표 저장
                    bestHeight = candidateHeight; // 새 1위 높이 저장
                    bestDistance = candidateDistance; // 같은 높이 비교용 거리 저장
                } // 드론 최적 후보 갱신 종료
            } // 현재 추적 후보 확인 종료

            currentTarget = bestTarget; // 검색된 최적 목표 적용

            if (currentTarget != null && consumesRetarget) // 새 목표와 재선정 소비 여부 확인
            { // 재선정 횟수 소비 처리
                retargetsRemaining = Mathf.Max(0, retargetsRemaining - 1); // 남은 재선정 횟수 한 번 감소
            } // 재선정 횟수 소비 처리 종료

            return currentTarget != null; // 새 목표 검색 성공 여부 반환
        } // 새 추적 대상 검색 처리 종료

        private bool IsValidTarget(ExternalForceReceiver receiver) // 추적 대상 유효성과 투명 상태 확인
        { // 추적 대상 유효성 검사 처리
            if (receiver == null || !receiver.isActiveAndEnabled || !receiver.CanReceivePush || ownerRoot == null) // 누락 대상과 비활성 대상과 수신 불가와 소유자 누락 여부 확인
            { // 기본 무효 대상 처리
                return false; // 추적 불가 반환
            } // 기본 무효 대상 처리 종료

            Transform receiverTransform = receiver.ForceReceiverTransform; // 대상 위치 Transform 조회

            if (receiverTransform == ownerRoot || receiverTransform.IsChildOf(ownerRoot)) // 소유자 자신 또는 자식 여부 확인
            { // 자기 자신 추적 차단 처리
                return false; // 추적 불가 반환
            } // 자기 자신 추적 차단 처리 종료

            PlayerP2ItemEffectController targetEffects = receiver.GetComponentInParent<PlayerP2ItemEffectController>(); // 대상 플레이어 P2 효과 관리자 조회
            return targetEffects == null || !targetEffects.IsInvisible; // 투명 망토 대상 제외 결과 반환
        } // 추적 대상 유효성 검사 처리 종료

        private void ApplyHit() // 현재 목표에 밀치기 적용 후 제거
        { // 추적 적중 효과 처리
            if (IsValidTarget(currentTarget)) // 적중 순간 목표 유효성 재확인
            { // 유효 목표 밀치기 처리
                Vector3 pushDirection = currentTarget.ForceReceiverTransform.position - ownerRoot.position; // 소유자에서 목표 방향 계산
                pushDirection = Vector3.ProjectOnPlane(pushDirection, Vector3.up).normalized + Vector3.up * 0.2f; // 수평과 약한 위쪽 혼합 방향 계산
                currentTarget.TryReceiveExternalForce(pushDirection.normalized, pushForce); // 목표에 추적 아이템 밀치기 힘 적용
            } // 유효 목표 밀치기 처리 종료

            Destroy(gameObject); // 적중한 추적 오브젝트 제거
        } // 추적 적중 효과 처리 종료

        private void OnDestroy() // 추적 오브젝트 제거 시 소유자 목록 정리
        { // 소유자 추적 목록 정리 처리
            ownerEffectController?.UnregisterOwnedEffect(this); // 소유자 효과 관리자에서 현재 효과 제거
        } // 소유자 추적 목록 정리 처리 종료

        private void CreateVisual(Color visualColor) // 유도탄 또는 드론 임시 표시 생성
        { // 임시 추적 표시 생성 처리
            PrimitiveType primitiveType = effectType == ItemEffectType.Drone ? PrimitiveType.Cube : PrimitiveType.Sphere; // 드론과 유도탄별 임시 모양 선택
            GameObject visualObject = GameObject.CreatePrimitive(primitiveType); // 선택한 기본 도형 생성
            visualObject.name = "Visual"; // 임시 표시 오브젝트 이름 지정
            visualObject.transform.SetParent(transform, false); // 추적 루트 아래 표시 배치
            visualObject.transform.localPosition = Vector3.zero; // 추적 중심에 표시 배치
            visualObject.transform.localScale = effectType == ItemEffectType.Drone ? new Vector3(0.8f, 0.25f, 0.8f) : Vector3.one * hitRadius * 2f; // 효과 종류에 맞는 표시 크기 적용
            Collider visualCollider = visualObject.GetComponent<Collider>(); // 기본 도형 Collider 조회

            if (visualCollider != null) // 자동 생성 Collider 존재 여부 확인
            { // 자동 Collider 제거 처리
                Destroy(visualCollider); // 장애물과 충돌하지 않는 추적 이동을 위해 Collider 제거
            } // 자동 Collider 제거 처리 종료

            Renderer visualRenderer = visualObject.GetComponent<Renderer>(); // 임시 표시 Renderer 조회

            if (visualRenderer != null) // Renderer 존재 여부 확인
            { // 대표 색상 적용 처리
                visualRenderer.material.color = visualColor; // 아이템 데이터 대표 색상 적용
            } // 대표 색상 적용 처리 종료
        } // 임시 추적 표시 생성 처리 종료
    } // 추적 효과 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
