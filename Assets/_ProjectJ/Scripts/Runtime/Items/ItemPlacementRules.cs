using UnityEngine; // Unity 벡터와 각도 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    public static class ItemPlacementRules // 설치 위치 순수 계산 규칙 선언
    { // 설치 위치 순수 계산 규칙 묶음
        public static bool IsSlopeAllowed(Vector3 surfaceNormal, float maximumSlopeAngle) // 지면 경사 허용 여부 판정
        { // 지면 경사 판정 처리
            float slopeAngle = Vector3.Angle(Vector3.up, surfaceNormal.normalized); // 위쪽 방향과 지면 법선 사이 각도 계산
            return slopeAngle <= Mathf.Clamp(maximumSlopeAngle, 0f, 89f); // 허용 최대 각도 이하 여부 반환
        } // 지면 경사 판정 처리 종료

        public static bool IsInsideBounds(Vector3 position, Bounds allowedBounds, float edgePadding) // 허용 영역 내부 포함 여부 판정
        { // 허용 영역 포함 판정 처리
            float safePadding = Mathf.Max(0f, edgePadding); // 음수 가장자리 여백 제거
            float minimumX = allowedBounds.min.x + safePadding; // 여백 적용 최소 X 계산
            float maximumX = allowedBounds.max.x - safePadding; // 여백 적용 최대 X 계산
            float minimumZ = allowedBounds.min.z + safePadding; // 여백 적용 최소 Z 계산
            float maximumZ = allowedBounds.max.z - safePadding; // 여백 적용 최대 Z 계산
            return minimumX <= maximumX && minimumZ <= maximumZ && position.x >= minimumX && position.x <= maximumX && position.z >= minimumZ && position.z <= maximumZ; // XZ 평면 내부 포함 여부 반환
        } // 허용 영역 포함 판정 처리 종료
    } // 설치 위치 순수 계산 규칙 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
