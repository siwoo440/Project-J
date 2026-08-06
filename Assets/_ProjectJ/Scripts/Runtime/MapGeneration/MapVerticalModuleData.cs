using System; // 직렬화 기능 참조
using UnityEngine; // Unity 컴포넌트와 수학 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    public enum MapVerticalLayoutKind // 수직 모듈 형태 선언
    { // 수직 모듈 형태 묶음
        Flat, // 높이 변화 없는 모듈
        StepRise, // 낮은 계단 연속 상승
        ZigzagRise, // 좌우 지그재그 상승
        JumpRise // 점프 구간 연속 상승
    } // 수직 모듈 형태 묶음 종료

    [Serializable] // Unity 직렬화 대상 표시
    public sealed class MapVerticalTraversalSegment // 단일 수직 이동 구간 선언
    { // 단일 수직 이동 구간 묶음
        [SerializeField] private string segmentId = "Segment"; // 수직 이동 구간 ID
        [SerializeField] private MapTraversalRequirement traversalRequirement = MapTraversalRequirement.Walk; // 구간 통과 방식
        [SerializeField, Min(0f)] private float heightGain; // 구간 상승 높이
        [SerializeField, Min(0f)] private float horizontalDistance; // 구간 수평 이동 거리

        public string SegmentId => segmentId; // 수직 이동 구간 ID 반환
        public MapTraversalRequirement TraversalRequirement => traversalRequirement; // 구간 통과 방식 반환
        public float HeightGain => heightGain; // 구간 상승 높이 반환
        public float HorizontalDistance => horizontalDistance; // 구간 수평 이동 거리 반환

        public MapVerticalTraversalSegment(string newSegmentId, MapTraversalRequirement newTraversalRequirement, float newHeightGain, float newHorizontalDistance) // 수직 이동 구간 생성
        { // 수직 이동 구간 생성 처리
            segmentId = newSegmentId; // 새 구간 ID 저장
            traversalRequirement = newTraversalRequirement; // 새 통과 방식 저장
            heightGain = Mathf.Max(0f, newHeightGain); // 새 상승 높이 저장
            horizontalDistance = Mathf.Max(0f, newHorizontalDistance); // 새 수평 거리 저장
        } // 수직 이동 구간 생성 처리 종료
    } // 단일 수직 이동 구간 묶음 종료

    [DisallowMultipleComponent] // 수직 모듈 데이터 중복 방지
    [RequireComponent(typeof(MapModuleDefinition))] // 기본 모듈 정의 필수 지정
    public sealed class MapVerticalModuleData : MonoBehaviour // 수직 맵 모듈 데이터 선언
    { // 수직 맵 모듈 데이터 묶음
        [SerializeField] private MapVerticalLayoutKind layoutKind = MapVerticalLayoutKind.Flat; // 수직 모듈 형태
        [SerializeField] private string entranceConnectionId = "Entrance"; // 기준 입구 연결 ID
        [SerializeField] private string exitConnectionId = "Exit"; // 기준 출구 연결 ID
        [SerializeField, Min(0f)] private float expectedHeightGain; // 입구부터 출구까지 예상 상승량
        [SerializeField] private MapVerticalTraversalSegment[] traversalSegments = Array.Empty<MapVerticalTraversalSegment>(); // 내부 수직 이동 구간 목록
        [SerializeField] private MapTraversalProfile traversalProfile; // 플레이어 이동 능력 기준

        public MapVerticalLayoutKind LayoutKind => layoutKind; // 수직 모듈 형태 반환
        public string EntranceConnectionId => entranceConnectionId; // 기준 입구 연결 ID 반환
        public string ExitConnectionId => exitConnectionId; // 기준 출구 연결 ID 반환
        public float ExpectedHeightGain => expectedHeightGain; // 예상 상승량 반환
        public MapVerticalTraversalSegment[] TraversalSegments => traversalSegments; // 수직 이동 구간 목록 반환
        public MapTraversalProfile TraversalProfile => traversalProfile; // 이동 능력 기준 반환
        public bool IsAscending => expectedHeightGain > 0.01f; // 상승 모듈 여부 반환

        private void OnValidate() // Inspector 수직 모듈 데이터 보정
        { // 수직 모듈 데이터 보정 처리
            entranceConnectionId = string.IsNullOrWhiteSpace(entranceConnectionId) ? "Entrance" : entranceConnectionId.Trim(); // 빈 입구 ID 보완
            exitConnectionId = string.IsNullOrWhiteSpace(exitConnectionId) ? "Exit" : exitConnectionId.Trim(); // 빈 출구 ID 보완
            expectedHeightGain = Mathf.Max(0f, expectedHeightGain); // 예상 상승량 음수 방지
            traversalSegments = traversalSegments ?? Array.Empty<MapVerticalTraversalSegment>(); // 빈 구간 배열 보완
        } // 수직 모듈 데이터 보정 종료

        public bool TryValidate(out string reason) // 현재 수직 모듈 데이터 유효성 검사
        { // 수직 모듈 데이터 검사 처리
            MapModuleDefinition module = GetComponent<MapModuleDefinition>(); // 같은 오브젝트의 기본 모듈 정의 조회
            return MapVerticalModuleValidationRules.TryValidateVerticalModule(module, this, out reason); // 공통 규칙 기반 검사 결과 반환
        } // 수직 모듈 데이터 검사 종료

#if UNITY_EDITOR // Unity Editor 전용 설정 시작
        public void ConfigureForEditor(MapVerticalLayoutKind newLayoutKind, string newEntranceConnectionId, string newExitConnectionId, float newExpectedHeightGain, MapVerticalTraversalSegment[] newTraversalSegments, MapTraversalProfile newTraversalProfile) // Editor 도구용 수직 데이터 설정
        { // Editor 수직 데이터 설정 처리
            layoutKind = newLayoutKind; // 새 수직 형태 저장
            entranceConnectionId = newEntranceConnectionId; // 새 입구 ID 저장
            exitConnectionId = newExitConnectionId; // 새 출구 ID 저장
            expectedHeightGain = newExpectedHeightGain; // 새 예상 상승량 저장
            traversalSegments = newTraversalSegments; // 새 이동 구간 목록 저장
            traversalProfile = newTraversalProfile; // 새 이동 능력 기준 저장
            OnValidate(); // 설정값 즉시 보정
        } // Editor 수직 데이터 설정 종료
#endif // Unity Editor 전용 설정 종료
    } // 수직 맵 모듈 데이터 묶음 종료
} // 맵 생성 기능 묶음 종료
