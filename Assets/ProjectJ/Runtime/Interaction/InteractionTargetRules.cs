using System.Collections.Generic; // 목록 기능 사용
using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Interaction // 상호작용 시스템 네임스페이스
{
    public static class InteractionTargetRules // 상호작용 Target 선택 규칙
    {
        private const float DistanceTieEpsilon = 0.0001f; // 거리 동점 허용 오차

        public static bool IsWithinRange(Vector3 originPosition, IInteractable target, float range) // 상호작용 거리 검사
        {
            if (target == null) // 대상 누락 검사
            {
                return false; // 거리 검사 실패 반환
            }

            Transform targetTransform = target.InteractionTransform; // 대상 기준 위치 저장

            if (targetTransform == null) // 기준 위치 누락 검사
            {
                return false; // 거리 검사 실패 반환
            }

            float safeRange = Mathf.Max(0f, range); // 음수 거리 보정
            float sqrDistance = (targetTransform.position - originPosition).sqrMagnitude; // 제곱 거리 계산
            float sqrRange = safeRange * safeRange; // 제곱 범위 계산

            return sqrDistance <= sqrRange; // 범위 포함 결과 반환
        }

        public static IInteractable SelectNearest( // 최근접 유효 Target 선택
            GameObject interactor, // 상호작용 실행자
            Vector3 originPosition, // 상호작용 기준 위치
            float range, // 최대 상호작용 거리
            IReadOnlyList<IInteractable> candidates // 후보 목록
        )
        {
            if (interactor == null || candidates == null) // 필수 데이터 누락 검사
            {
                return null; // Target 없음 반환
            }

            float safeRange = Mathf.Max(0f, range); // 음수 거리 보정
            float sqrRange = safeRange * safeRange; // 제곱 범위 계산
            float bestSqrDistance = float.PositiveInfinity; // 최단 거리 초기화
            int bestInstanceId = int.MaxValue; // 동점 정렬 ID 초기화
            IInteractable bestTarget = null; // 최종 Target 초기화

            for (int i = 0; i < candidates.Count; i++) // 모든 후보 반복
            {
                IInteractable candidate = candidates[i]; // 현재 후보 저장
                Component candidateComponent = candidate as Component; // 유니티 컴포넌트 변환

                if (candidate == null || candidateComponent == null) // 유효 후보 검사
                {
                    continue; // 다음 후보 진행
                }

                if (!candidateComponent.gameObject.activeInHierarchy) // 활성 상태 검사
                {
                    continue; // 비활성 후보 제외
                }

                if (!candidate.CanInteract(interactor)) // 사용 가능 상태 검사
                {
                    continue; // 사용 불가 후보 제외
                }

                Transform candidateTransform = candidate.InteractionTransform; // 후보 기준 위치 저장

                if (candidateTransform == null) // 기준 위치 누락 검사
                {
                    continue; // 잘못된 후보 제외
                }

                float sqrDistance = (candidateTransform.position - originPosition).sqrMagnitude; // 후보 거리 계산

                if (sqrDistance > sqrRange) // 최대 거리 초과 검사
                {
                    continue; // 범위 밖 후보 제외
                }

                int instanceId = candidateComponent.GetInstanceID(); // 동점 정렬용 ID 저장
                bool isCloser = sqrDistance < bestSqrDistance - DistanceTieEpsilon; // 더 가까운 후보 검사
                bool isSameDistance = Mathf.Abs(sqrDistance - bestSqrDistance) <= DistanceTieEpsilon; // 거리 동점 검사
                bool winsTie = isSameDistance && instanceId < bestInstanceId; // 동점 우선순위 검사

                if (!isCloser && !winsTie) // 교체 조건 검사
                {
                    continue; // 기존 Target 유지
                }

                bestTarget = candidate; // 최근접 Target 갱신
                bestSqrDistance = sqrDistance; // 최근접 거리 갱신
                bestInstanceId = instanceId; // 동점 정렬 ID 갱신
            }

            return bestTarget; // 최종 Target 반환
        }
    }
}
