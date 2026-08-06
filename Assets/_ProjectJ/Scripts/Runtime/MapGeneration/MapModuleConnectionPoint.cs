using UnityEngine; // Unity Transform과 기즈모 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    [DisallowMultipleComponent] // 연결 지점 컴포넌트 중복 방지
    public sealed class MapModuleConnectionPoint : MonoBehaviour // 모듈 연결 지점 선언
    { // 모듈 연결 지점 묶음
        [SerializeField] private string connectionId = "Connection"; // 모듈 내부 연결 지점 ID
        [SerializeField] private MapConnectionRole role = MapConnectionRole.Entrance; // 입구 또는 출구 역할
        [SerializeField] private MapConnectionDirection direction = MapConnectionDirection.North; // 로컬 연결 방향
        [SerializeField, Min(0.1f)] private float connectionWidth = 2f; // 연결 통로 너비
        [SerializeField, Min(0.1f)] private float connectionHeight = 2.2f; // 연결 통로 높이
        [SerializeField] private bool drawGizmos = true; // 연결 지점 기즈모 표시 여부

        public string ConnectionId => connectionId; // 연결 지점 ID 반환
        public MapConnectionRole Role => role; // 연결 지점 역할 반환
        public MapConnectionDirection Direction => direction; // 연결 지점 방향 반환
        public float ConnectionWidth => connectionWidth; // 연결 통로 너비 반환
        public float ConnectionHeight => connectionHeight; // 연결 통로 높이 반환
        public float LocalHeight => transform.localPosition.y; // 모듈 기준 연결 높이 반환
        public Vector3 LocalDirection => MapModuleValidationRules.ToLocalVector(direction); // 로컬 연결 방향 벡터 반환
        public Vector3 WorldDirection => transform.TransformDirection(LocalDirection).normalized; // 월드 연결 방향 벡터 반환

        private void OnValidate() // Inspector 연결 지점 수치 보정
        { // 연결 지점 수치 보정 처리
            connectionId = string.IsNullOrWhiteSpace(connectionId) ? gameObject.name : connectionId.Trim(); // 빈 연결 ID 자동 보완
            connectionWidth = Mathf.Max(0.1f, connectionWidth); // 연결 너비 양수 보장
            connectionHeight = Mathf.Max(0.1f, connectionHeight); // 연결 높이 양수 보장
        } // 연결 지점 수치 보정 종료

        private void OnDrawGizmos() // Scene 연결 지점 기즈모 표시
        { // 연결 지점 기즈모 표시 처리
            if (!drawGizmos) // 기즈모 비활성 상태 확인
            { // 기즈모 비활성 처리
                return; // 기즈모 표시 생략
            } // 기즈모 비활성 처리 종료

            Color pointColor = role == MapConnectionRole.Entrance ? new Color(0.2f, 0.8f, 1f, 1f) : new Color(1f, 0.55f, 0.15f, 1f); // 역할별 기즈모 색상 계산
            Vector3 worldDirection = WorldDirection; // 월드 연결 방향 조회
            Vector3 worldRight = Vector3.Cross(Vector3.up, worldDirection).normalized; // 연결 지점 오른쪽 방향 계산
            Vector3 arrowEnd = transform.position + worldDirection * 1.25f; // 방향 화살표 끝 위치 계산
            Gizmos.color = pointColor; // 연결 지점 색상 적용
            Gizmos.DrawSphere(transform.position, 0.15f); // 연결 중심 구체 표시
            Gizmos.DrawLine(transform.position, arrowEnd); // 연결 방향 선 표시
            Gizmos.DrawLine(arrowEnd, arrowEnd - worldDirection * 0.3f + worldRight * 0.2f); // 화살표 오른쪽 날개 표시
            Gizmos.DrawLine(arrowEnd, arrowEnd - worldDirection * 0.3f - worldRight * 0.2f); // 화살표 왼쪽 날개 표시
            Matrix4x4 previousMatrix = Gizmos.matrix; // 기존 기즈모 행렬 저장
            Vector3 gizmoCenter = transform.position + Vector3.up * connectionHeight * 0.5f; // 연결 통로 기즈모 중심 계산
            Quaternion gizmoRotation = MapModuleValidationRules.CalculateConnectionGizmoRotation(worldDirection); // 연결 방향 기반 기즈모 회전 계산
            Gizmos.matrix = Matrix4x4.TRS(gizmoCenter, gizmoRotation, Vector3.one); // 연결 위치와 방향 행렬 적용
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(connectionWidth, connectionHeight, 0.1f)); // 회전된 연결 통로 크기 표시
            Gizmos.matrix = previousMatrix; // 기존 기즈모 행렬 복원
        } // 연결 지점 기즈모 표시 종료

#if UNITY_EDITOR // Unity Editor 전용 설정 시작
        public void ConfigureForEditor(string newConnectionId, MapConnectionRole newRole, MapConnectionDirection newDirection, float newConnectionWidth, float newConnectionHeight) // Editor 도구용 연결 지점 설정
        { // Editor 연결 지점 설정 처리
            connectionId = newConnectionId; // 새 연결 ID 저장
            role = newRole; // 새 연결 역할 저장
            direction = newDirection; // 새 연결 방향 저장
            connectionWidth = newConnectionWidth; // 새 연결 너비 저장
            connectionHeight = newConnectionHeight; // 새 연결 높이 저장
            OnValidate(); // 설정값 즉시 보정
        } // Editor 연결 지점 설정 종료
#endif // Unity Editor 전용 설정 종료
    } // 모듈 연결 지점 묶음 종료
} // 맵 생성 기능 묶음 종료

