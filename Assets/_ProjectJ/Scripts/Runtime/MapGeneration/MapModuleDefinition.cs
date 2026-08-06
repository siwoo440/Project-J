using UnityEngine; // Unity 컴포넌트와 기즈모 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    [DisallowMultipleComponent] // 모듈 정의 컴포넌트 중복 방지
    public sealed class MapModuleDefinition : MonoBehaviour // 맵 모듈 공통 데이터 선언
    { // 맵 모듈 공통 데이터 묶음
        [SerializeField] private string moduleId = "MAP-000"; // 맵 모듈 고유 ID
        [SerializeField] private MapModuleKind moduleKind = MapModuleKind.FixedPlatform; // 맵 모듈 종류
        [SerializeField] private MapTraversalRequirement traversalRequirement = MapTraversalRequirement.Walk; // 모듈 내부 통과 조건
        [SerializeField] private MapRotationOptions allowedRotations = MapRotationOptions.All; // 허용 회전값
        [SerializeField] private Vector3 boundsCenter = Vector3.up; // 모듈 배치 영역 중심
        [SerializeField] private Vector3 boundsSize = new Vector3(4f, 2f, 8f); // 모듈 배치 영역 크기
        [SerializeField, Min(0f)] private float requiredClearanceHeight = 2.2f; // 내부 통로 유효 높이
        [SerializeField, Min(0f)] private float requiredJumpDistance; // 내부 점프 수평 거리
        [SerializeField] private float requiredJumpRise; // 내부 점프 도착 높이 차이
        [SerializeField] private MapTraversalProfile traversalProfile; // 플레이어 이동 능력 기준
        [SerializeField] private MapModuleConnectionPoint[] connectionPoints; // 입구와 출구 연결 지점 목록
        [SerializeField] private bool drawBoundsGizmo = true; // 모듈 영역 기즈모 표시 여부

        public string ModuleId => moduleId; // 모듈 고유 ID 반환
        public MapModuleKind ModuleKind => moduleKind; // 모듈 종류 반환
        public MapTraversalRequirement TraversalRequirement => traversalRequirement; // 모듈 통과 조건 반환
        public MapRotationOptions AllowedRotations => allowedRotations; // 허용 회전값 반환
        public Vector3 BoundsCenter => boundsCenter; // 모듈 영역 중심 반환
        public Vector3 BoundsSize => boundsSize; // 모듈 영역 크기 반환
        public float RequiredClearanceHeight => requiredClearanceHeight; // 내부 통로 높이 반환
        public float RequiredJumpDistance => requiredJumpDistance; // 점프 수평 거리 반환
        public float RequiredJumpRise => requiredJumpRise; // 점프 도착 높이 차이 반환
        public MapTraversalProfile TraversalProfile => traversalProfile; // 이동 능력 기준 반환
        public MapModuleConnectionPoint[] ConnectionPoints => connectionPoints; // 연결 지점 목록 반환
        public Bounds WorldBounds // 회전과 배율이 적용된 월드 영역 반환
        { // 월드 영역 계산 범위
            get // 월드 영역 조회
            { // 월드 영역 조회 범위
                Vector3 localExtents = boundsSize * 0.5f; // 로컬 영역 절반 크기 계산
                Vector3 firstLocalCorner = boundsCenter - localExtents; // 첫 번째 로컬 모서리 계산
                Vector3 firstWorldCorner = transform.TransformPoint(firstLocalCorner); // 첫 번째 모서리 월드 좌표 변환
                Bounds worldBounds = new Bounds(firstWorldCorner, Vector3.zero); // 월드 영역 초기화

                for (int xIndex = 0; xIndex < 2; xIndex++) // X축 양쪽 모서리 순회
                { // X축 모서리 순회 범위
                    for (int yIndex = 0; yIndex < 2; yIndex++) // Y축 양쪽 모서리 순회
                    { // Y축 모서리 순회 범위
                        for (int zIndex = 0; zIndex < 2; zIndex++) // Z축 양쪽 모서리 순회
                        { // Z축 모서리 순회 범위
                            float xOffset = xIndex == 0 ? -localExtents.x : localExtents.x; // 현재 X축 모서리 위치 계산
                            float yOffset = yIndex == 0 ? -localExtents.y : localExtents.y; // 현재 Y축 모서리 위치 계산
                            float zOffset = zIndex == 0 ? -localExtents.z : localExtents.z; // 현재 Z축 모서리 위치 계산
                            Vector3 localCorner = boundsCenter + new Vector3(xOffset, yOffset, zOffset); // 현재 로컬 모서리 계산
                            Vector3 worldCorner = transform.TransformPoint(localCorner); // 현재 모서리 월드 좌표 변환
                            worldBounds.Encapsulate(worldCorner); // 현재 모서리를 월드 영역에 포함
                        } // Z축 모서리 순회 종료
                    } // Y축 모서리 순회 종료
                } // X축 모서리 순회 종료

                return worldBounds; // 회전이 반영된 월드 영역 반환
            } // 월드 영역 조회 종료
        } // 월드 영역 계산 종료

        private void Reset() // 컴포넌트 최초 추가 기본값 구성
        { // 최초 기본값 구성 처리
            RefreshConnectionPoints(); // 자식 연결 지점 자동 수집
        } // 최초 기본값 구성 종료

        private void OnValidate() // Inspector 모듈 데이터 보정
        { // 모듈 데이터 보정 처리
            moduleId = string.IsNullOrWhiteSpace(moduleId) ? gameObject.name : moduleId.Trim(); // 빈 모듈 ID 자동 보완
            boundsSize = MapModuleValidationRules.ClampBoundsSize(boundsSize); // 배치 영역 양수 크기 보장
            requiredClearanceHeight = Mathf.Max(0f, requiredClearanceHeight); // 통로 높이 음수 방지
            requiredJumpDistance = Mathf.Max(0f, requiredJumpDistance); // 점프 거리 음수 방지
            RefreshConnectionPoints(); // 자식 연결 지점 목록 최신화
        } // 모듈 데이터 보정 종료

        public void RefreshConnectionPoints() // 자식 입구와 출구 목록 다시 수집
        { // 연결 지점 수집 처리
            connectionPoints = GetComponentsInChildren<MapModuleConnectionPoint>(true); // 비활성 자식을 포함한 연결 지점 수집
        } // 연결 지점 수집 종료

        public bool TryValidate(out string reason) // 현재 모듈 데이터 유효성 검사
        { // 모듈 데이터 검사 처리
            return MapModuleValidationRules.TryValidateModule(this, traversalProfile, out reason); // 공통 규칙 기반 검사 결과 반환
        } // 모듈 데이터 검사 종료

        private void OnDrawGizmosSelected() // 선택된 모듈 배치 영역 기즈모 표시
        { // 배치 영역 기즈모 표시 처리
            if (!drawBoundsGizmo) // 영역 기즈모 비활성 상태 확인
            { // 영역 기즈모 비활성 처리
                return; // 영역 기즈모 표시 생략
            } // 영역 기즈모 비활성 처리 종료

            Matrix4x4 previousMatrix = Gizmos.matrix; // 기존 기즈모 행렬 저장
            Gizmos.matrix = transform.localToWorldMatrix; // 모듈 로컬 좌표 행렬 적용
            Gizmos.color = TryValidate(out string unusedReason) ? new Color(0.2f, 1f, 0.35f, 0.8f) : new Color(1f, 0.2f, 0.2f, 0.8f); // 유효성에 따른 영역 색상 적용
            Gizmos.DrawWireCube(boundsCenter, boundsSize); // 모듈 배치 영역 표시
            Gizmos.matrix = previousMatrix; // 기존 기즈모 행렬 복원
        } // 배치 영역 기즈모 표시 종료

#if UNITY_EDITOR // Unity Editor 전용 설정 시작
        public void ConfigureForEditor(string newModuleId, MapModuleKind newModuleKind, MapTraversalRequirement newTraversalRequirement, MapRotationOptions newAllowedRotations, Vector3 newBoundsCenter, Vector3 newBoundsSize, float newRequiredClearanceHeight, float newRequiredJumpDistance, float newRequiredJumpRise, MapTraversalProfile newTraversalProfile) // Editor 도구용 모듈 데이터 설정
        { // Editor 모듈 데이터 설정 처리
            moduleId = newModuleId; // 새 모듈 ID 저장
            moduleKind = newModuleKind; // 새 모듈 종류 저장
            traversalRequirement = newTraversalRequirement; // 새 통과 조건 저장
            allowedRotations = newAllowedRotations; // 새 허용 회전 저장
            boundsCenter = newBoundsCenter; // 새 영역 중심 저장
            boundsSize = newBoundsSize; // 새 영역 크기 저장
            requiredClearanceHeight = newRequiredClearanceHeight; // 새 통로 높이 저장
            requiredJumpDistance = newRequiredJumpDistance; // 새 점프 거리 저장
            requiredJumpRise = newRequiredJumpRise; // 새 점프 높이 차이 저장
            traversalProfile = newTraversalProfile; // 새 이동 능력 기준 저장
            OnValidate(); // 설정값 즉시 보정
        } // Editor 모듈 데이터 설정 종료
#endif // Unity Editor 전용 설정 종료
    } // 맵 모듈 공통 데이터 묶음 종료
} // 맵 생성 기능 묶음 종료

