using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Map // 맵 시스템 네임스페이스
{
    public enum MapObstacleVolumeType // 장애물 배치 영역 종류
    {
        Safe = 0, // 설치 가능 영역
        NoSpawn = 1 // 설치 금지 영역
    }

    [DisallowMultipleComponent] // 중복 컴포넌트 방지
    public sealed class MapObstaclePlacementVolume : MonoBehaviour // 장애물 배치 영역 컴포넌트
    {
        [SerializeField] // 인스펙터 직렬화
        private MapObstacleVolumeType volumeType = MapObstacleVolumeType.Safe; // 영역 종류

        [SerializeField] // 인스펙터 직렬화
        private bool showVolume = true; // 영역 선 표시 여부

        [SerializeField] // 인스펙터 직렬화
        [Min(0.0001f)] // 최소 허용 오차 제한
        private float boundsEpsilon = 0.001f; // 경계 판정 여유값

        public MapObstacleVolumeType VolumeType => volumeType; // 영역 종류 반환
        public bool ShowVolume => showVolume; // 선 표시 여부 반환

        public void Configure(MapObstacleVolumeType newVolumeType, bool newShowVolume) // 영역 설정 적용
        {
            volumeType = newVolumeType; // 영역 종류 저장
            showVolume = newShowVolume; // 선 표시 여부 저장
        }

        public bool ContainsBounds(Bounds candidateBounds) // 후보 Bounds 전체 포함 검사
        {
            Vector3[] corners = CreateBoundsCorners(candidateBounds); // 후보 모서리 생성

            for (int i = 0; i < corners.Length; i++) // 모든 모서리 반복
            {
                Vector3 localPoint = transform.InverseTransformPoint(corners[i]); // 영역 로컬 좌표 변환

                if (Mathf.Abs(localPoint.x) > 0.5f + boundsEpsilon) // X축 범위 초과 검사
                {
                    return false; // 포함 실패 반환
                }

                if (Mathf.Abs(localPoint.y) > 0.5f + boundsEpsilon) // Y축 범위 초과 검사
                {
                    return false; // 포함 실패 반환
                }

                if (Mathf.Abs(localPoint.z) > 0.5f + boundsEpsilon) // Z축 범위 초과 검사
                {
                    return false; // 포함 실패 반환
                }
            }

            return true; // 전체 포함 성공 반환
        }

        public bool IntersectsBounds(Bounds candidateBounds) // 후보 Bounds 겹침 검사
        {
            Vector3[] corners = CreateBoundsCorners(candidateBounds); // 후보 모서리 생성
            Vector3 localMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity); // 로컬 최소값 초기화
            Vector3 localMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity); // 로컬 최대값 초기화

            for (int i = 0; i < corners.Length; i++) // 모든 모서리 반복
            {
                Vector3 localPoint = transform.InverseTransformPoint(corners[i]); // 영역 로컬 좌표 변환
                localMin = Vector3.Min(localMin, localPoint); // 최소 좌표 갱신
                localMax = Vector3.Max(localMax, localPoint); // 최대 좌표 갱신
            }

            bool overlapsX = localMin.x <= 0.5f + boundsEpsilon && localMax.x >= -0.5f - boundsEpsilon; // X축 겹침 판정
            bool overlapsY = localMin.y <= 0.5f + boundsEpsilon && localMax.y >= -0.5f - boundsEpsilon; // Y축 겹침 판정
            bool overlapsZ = localMin.z <= 0.5f + boundsEpsilon && localMax.z >= -0.5f - boundsEpsilon; // Z축 겹침 판정

            return overlapsX && overlapsY && overlapsZ; // 전체 축 겹침 결과 반환
        }

        private static Vector3[] CreateBoundsCorners(Bounds bounds) // Bounds 모서리 배열 생성
        {
            Vector3 center = bounds.center; // Bounds 중심 저장
            Vector3 extents = bounds.extents; // Bounds 반크기 저장

            return new Vector3[] // 8개 모서리 반환
            {
                center + new Vector3(-extents.x, -extents.y, -extents.z), // 좌하후 모서리
                center + new Vector3(-extents.x, -extents.y, extents.z), // 좌하전 모서리
                center + new Vector3(-extents.x, extents.y, -extents.z), // 좌상후 모서리
                center + new Vector3(-extents.x, extents.y, extents.z), // 좌상전 모서리
                center + new Vector3(extents.x, -extents.y, -extents.z), // 우하후 모서리
                center + new Vector3(extents.x, -extents.y, extents.z), // 우하전 모서리
                center + new Vector3(extents.x, extents.y, -extents.z), // 우상후 모서리
                center + new Vector3(extents.x, extents.y, extents.z) // 우상전 모서리
            };
        }

        private void OnDrawGizmos() // Scene 영역 선 표시
        {
            if (!showVolume) // 표시 비활성 검사
            {
                return; // 표시 중단
            }

            Matrix4x4 previousMatrix = Gizmos.matrix; // 기존 Gizmo 행렬 저장
            Color previousColor = Gizmos.color; // 기존 Gizmo 색상 저장
            Gizmos.matrix = transform.localToWorldMatrix; // 영역 Transform 행렬 적용
            Gizmos.color = volumeType == MapObstacleVolumeType.Safe ? new Color(0.2f, 1f, 0.3f, 1f) : new Color(1f, 0.25f, 0.25f, 1f); // 영역 종류별 선 색상 적용
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one); // 단위 큐브 선 표시
            Gizmos.matrix = previousMatrix; // 기존 Gizmo 행렬 복원
            Gizmos.color = previousColor; // 기존 Gizmo 색상 복원
        }
    }
}
