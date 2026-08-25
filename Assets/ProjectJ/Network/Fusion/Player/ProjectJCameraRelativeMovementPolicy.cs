using UnityEngine; // Vector2·Vector3·Quaternion 사용

namespace ProjectJ.Networking.Fusion
{
    public static class ProjectJCameraRelativeMovementPolicy
    {
        private const float MinimumDirectionSqrMagnitude =
            0.0001f; // 유효한 수평 방향 최소 크기

        public static Vector3 ResolveMoveDirection(
            Vector2 moveInput,
            Vector3 aimDirection,
            Vector3 fallbackForward
        )
        {
            if (moveInput.sqrMagnitude <= MinimumDirectionSqrMagnitude)
            {
                return Vector3.zero; // 이동 입력 없음 처리
            }

            Vector2 normalizedInput =
                moveInput.sqrMagnitude > 1f
                    ? moveInput.normalized
                    : moveInput; // 대각선 이동 입력 크기 제한

            Vector3 horizontalForward =
                ResolveHorizontalForward(
                    aimDirection,
                    fallbackForward
                ); // 카메라 수평 전방 계산

            Vector3 horizontalRight =
                Vector3.Cross(
                    Vector3.up,
                    horizontalForward
                ); // 카메라 기준 오른쪽 방향 계산

            if (horizontalRight.sqrMagnitude <= MinimumDirectionSqrMagnitude)
            {
                horizontalRight =
                    Vector3.right; // 비정상 전방 입력의 최종 오른쪽 보정
            }
            else
            {
                horizontalRight.Normalize(); // 오른쪽 방향 단위 벡터 변환
            }

            Vector3 moveDirection =
                horizontalForward * normalizedInput.y +
                horizontalRight * normalizedInput.x; // WASD와 카메라 축 결합

            moveDirection.y =
                0f; // 수직 이동 성분 제거

            if (moveDirection.sqrMagnitude > 1f)
            {
                moveDirection.Normalize(); // 최종 대각선 속도 증가 차단
            }

            return moveDirection; // 카메라 기준 수평 이동 방향 반환
        }

        public static Vector3 ResolveHorizontalForward(
            Vector3 aimDirection,
            Vector3 fallbackForward
        )
        {
            Vector3 horizontalForward =
                aimDirection; // 네트워크 카메라 조준 방향 복사

            horizontalForward.y =
                0f; // 카메라 Pitch 이동 영향 제거

            if (horizontalForward.sqrMagnitude <= MinimumDirectionSqrMagnitude)
            {
                horizontalForward =
                    fallbackForward; // 수직 조준 시 Player 전방 사용

                horizontalForward.y =
                    0f; // 대체 전방의 수직 성분 제거
            }

            if (horizontalForward.sqrMagnitude <= MinimumDirectionSqrMagnitude)
            {
                horizontalForward =
                    Vector3.forward; // 최종 월드 전방 안전값 사용
            }

            horizontalForward.Normalize(); // 수평 전방 단위 벡터 변환

            return horizontalForward; // 유효한 카메라 수평 전방 반환
        }

        public static Quaternion ResolveBodyRotation(
            Quaternion currentRotation,
            Vector3 moveDirection,
            float maxDegreesDelta
        )
        {
            Vector3 horizontalMoveDirection =
                moveDirection; // 실제 이동 방향 복사

            horizontalMoveDirection.y =
                0f; // 몸 회전에서 수직 성분 제거

            if (
                horizontalMoveDirection.sqrMagnitude <= MinimumDirectionSqrMagnitude ||
                maxDegreesDelta <= 0f
            )
            {
                return currentRotation; // 정지 또는 회전량 없음 처리
            }

            horizontalMoveDirection.Normalize(); // 몸 목표 전방 단위 벡터 변환

            Quaternion targetRotation =
                Quaternion.LookRotation(
                    horizontalMoveDirection,
                    Vector3.up
                ); // 실제 이동 방향 목표 회전 계산

            return Quaternion.RotateTowards(
                currentRotation,
                targetRotation,
                maxDegreesDelta
            ); // Tick당 최대 각도로 부드러운 몸 회전
        }
    }
}
