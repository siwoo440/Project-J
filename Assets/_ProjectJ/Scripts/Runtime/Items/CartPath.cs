using System.Collections.Generic; // 등록된 카트 경로 목록 기능 참조
using UnityEngine; // Unity 경로 위치와 기즈모 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 경로 오브젝트당 컴포넌트 한 개만 허용
    public sealed class CartPath : MonoBehaviour // 카트 자동 주행 경로 선언
    { // 카트 경로 묶음
        private static readonly List<CartPath> ActivePaths = new List<CartPath>(); // 현재 Scene 활성 경로 목록 저장

        [SerializeField] private Transform[] waypoints; // 순서대로 이동할 경로 지점 저장
        [SerializeField] private bool drawPathGizmos = true; // Scene 경로 선 표시 여부 저장

        public int WaypointCount => waypoints == null ? 0 : waypoints.Length; // 현재 유효 후보 경로 지점 수 반환

        private void OnEnable() // 경로 활성화 시 목록 등록
        { // 활성 경로 등록 처리
            if (!ActivePaths.Contains(this)) // 중복 등록 여부 확인
            { // 새 경로 등록 처리
                ActivePaths.Add(this); // 활성 경로 목록에 추가
            } // 새 경로 등록 처리 종료
        } // 활성 경로 등록 처리 종료

        private void OnDisable() // 경로 비활성화 시 목록 해제
        { // 활성 경로 해제 처리
            ActivePaths.Remove(this); // 활성 경로 목록에서 제거
        } // 활성 경로 해제 처리 종료

        public bool TryGetWaypoint(int index, out Vector3 position) // 지정 번호 경로 지점 조회
        { // 경로 지점 조회 처리
            position = Vector3.zero; // 조회 실패 기본 위치 설정

            if (waypoints == null || index < 0 || index >= waypoints.Length || waypoints[index] == null) // 배열과 번호와 Transform 유효성 확인
            { // 잘못된 경로 지점 처리
                return false; // 경로 지점 조회 실패 반환
            } // 잘못된 경로 지점 처리 종료

            position = waypoints[index].position; // 현재 Transform 세계 위치 저장
            return true; // 경로 지점 조회 성공 반환
        } // 경로 지점 조회 처리 종료

        public int FindClosestWaypointIndex(Vector3 position) // 지정 위치와 가장 가까운 경로 지점 번호 검색
        { // 가장 가까운 경로 지점 검색 처리
            int closestIndex = -1; // 대상 없음 기본 번호 저장
            float closestSqrDistance = float.PositiveInfinity; // 가장 가까운 거리 초기화

            for (int index = 0; index < WaypointCount; index++) // 전체 경로 지점 순회
            { // 현재 경로 지점 거리 확인
                if (!TryGetWaypoint(index, out Vector3 waypointPosition)) // 현재 경로 지점 유효성 확인
                { // 누락 경로 지점 처리
                    continue; // 다음 경로 지점으로 이동
                } // 누락 경로 지점 처리 종료

                float sqrDistance = (waypointPosition - position).sqrMagnitude; // 현재 위치와 경로 지점 제곱 거리 계산

                if (sqrDistance < closestSqrDistance) // 기존보다 가까운 경로 지점 여부 확인
                { // 가장 가까운 경로 지점 갱신
                    closestSqrDistance = sqrDistance; // 새 가장 가까운 거리 저장
                    closestIndex = index; // 새 가장 가까운 번호 저장
                } // 가장 가까운 경로 지점 갱신 종료
            } // 현재 경로 지점 거리 확인 종료

            return closestIndex; // 검색된 가장 가까운 경로 지점 번호 반환
        } // 가장 가까운 경로 지점 검색 처리 종료

        public static bool TryFindNearestPath(Vector3 position, float maximumDistance, out CartPath path, out int waypointIndex) // 가까운 카트 경로와 시작 지점 검색
        { // 가까운 카트 경로 검색 처리
            path = null; // 검색 실패 기본 경로 설정
            waypointIndex = -1; // 검색 실패 기본 지점 번호 설정
            float maximumSqrDistance = Mathf.Max(0.1f, maximumDistance) * Mathf.Max(0.1f, maximumDistance); // 허용 최대 제곱 거리 계산
            float closestSqrDistance = maximumSqrDistance; // 현재 가장 가까운 허용 거리 초기화

            for (int pathIndex = ActivePaths.Count - 1; pathIndex >= 0; pathIndex--) // 활성 경로 역순 순회
            { // 현재 활성 경로 확인
                CartPath candidatePath = ActivePaths[pathIndex]; // 현재 후보 경로 조회

                if (candidatePath == null || !candidatePath.isActiveAndEnabled) // 파괴 또는 비활성 경로 여부 확인
                { // 잘못된 활성 경로 처리
                    ActivePaths.RemoveAt(pathIndex); // 오래된 경로 목록 항목 제거
                    continue; // 다음 경로로 이동
                } // 잘못된 활성 경로 처리 종료

                int candidateIndex = candidatePath.FindClosestWaypointIndex(position); // 후보 경로의 가까운 지점 검색

                if (!candidatePath.TryGetWaypoint(candidateIndex, out Vector3 candidatePosition)) // 후보 지점 유효성 확인
                { // 사용할 수 없는 경로 처리
                    continue; // 다음 경로로 이동
                } // 사용할 수 없는 경로 처리 종료

                float candidateSqrDistance = (candidatePosition - position).sqrMagnitude; // 후보 지점까지 제곱 거리 계산

                if (candidateSqrDistance <= closestSqrDistance) // 현재 허용 범위에서 더 가까운지 확인
                { // 가까운 경로 갱신
                    closestSqrDistance = candidateSqrDistance; // 새 가까운 거리 저장
                    path = candidatePath; // 새 가까운 경로 저장
                    waypointIndex = candidateIndex; // 새 가까운 지점 번호 저장
                } // 가까운 경로 갱신 종료
            } // 현재 활성 경로 확인 종료

            return path != null && waypointIndex >= 0; // 사용할 경로와 시작 지점 존재 여부 반환
        } // 가까운 카트 경로 검색 처리 종료

        private void OnDrawGizmos() // Scene에서 경로 지점과 연결선 표시
        { // 경로 기즈모 표시 처리
            if (!drawPathGizmos || waypoints == null) // 표시 설정과 경로 배열 확인
            { // 기즈모 표시 생략 처리
                return; // 경로 기즈모 표시 종료
            } // 기즈모 표시 생략 처리 종료

            Gizmos.color = Color.red; // 카트 경로 표시 색상 지정

            for (int index = 0; index < waypoints.Length; index++) // 전체 경로 지점 순회
            { // 현재 경로 지점 표시 처리
                Transform waypoint = waypoints[index]; // 현재 경로 지점 조회

                if (waypoint == null) // 누락 경로 지점 여부 확인
                { // 누락 경로 지점 처리
                    continue; // 다음 지점으로 이동
                } // 누락 경로 지점 처리 종료

                Gizmos.DrawWireSphere(waypoint.position, 0.25f); // 현재 경로 지점 구체 표시

                if (index + 1 < waypoints.Length && waypoints[index + 1] != null) // 다음 연결 지점 존재 여부 확인
                { // 경로 연결선 표시 처리
                    Gizmos.DrawLine(waypoint.position, waypoints[index + 1].position); // 현재 지점과 다음 지점 연결선 표시
                } // 경로 연결선 표시 처리 종료
            } // 현재 경로 지점 표시 처리 종료
        } // 경로 기즈모 표시 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(Transform[] newWaypoints) // 자동 설정 도구용 경로 지점 연결
        { // Editor 경로 설정 처리
            waypoints = newWaypoints; // 새 경로 지점 배열 저장
        } // Editor 경로 설정 처리 종료
#endif // Editor 전용 설정 종료
    } // 카트 경로 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
