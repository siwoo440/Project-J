using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEngine; // Unity 컴포넌트와 좌표 기능 참조

namespace ProjectJ.Player // 플레이어 진행 기능 네임스페이스 선언
{ // 플레이어 높이 진행 범위
    [DisallowMultipleComponent] // 동일 높이 진행 컴포넌트 중복 방지
    public sealed class PlayerHeightProgressController : MonoBehaviour // 현재 높이와 최고 높이와 수직 구간 관리 컴포넌트 선언
    { // 플레이어 높이 진행 기능 범위
        [Header("Height Reference")] // 높이 기준 설정 구역 제목
        [SerializeField] private Transform heightOrigin; // 높이 0미터 기준점
        [SerializeField, Min(0.001f)] private float metersPerUnityUnit = 1f; // 유니티 한 단위의 실제 미터 값
        [Header("Course Sections")] // 전체 코스 구간 설정 구역 제목
        [SerializeField, Min(0.001f)] private float totalCourseHeight = 1000f; // 전체 코스 정상 높이
        [SerializeField, Min(1)] private int sectionCount = 5; // 전체 수직 구간 개수

        private float fallbackOriginY; // 기준점 누락 시 사용할 시작 Y 좌표

        public float CurrentHeight { get; private set; } // 현재 월드 높이 미터
        public float HighestHeight { get; private set; } // 경기 중 최고 높이 미터
        public int CurrentSectionIndex { get; private set; } = 1; // 현재 수직 구간 번호
        public float CourseProgress01 { get; private set; } // 전체 코스 진행 비율
        public float SectionProgress01 { get; private set; } // 현재 구간 내부 진행 비율
        public float TotalCourseHeight => totalCourseHeight; // 전체 코스 정상 높이 반환
        public int SectionCount => sectionCount; // 전체 수직 구간 개수 반환
        public float SectionHeight => WorldHeightProgressMath.CalculateSectionHeight(totalCourseHeight, sectionCount); // 한 구간 높이 반환
        public float CurrentSectionStartHeight => WorldHeightProgressMath.CalculateSectionStartHeight(CurrentSectionIndex, totalCourseHeight, sectionCount); // 현재 구간 시작 높이 반환
        public float CurrentSectionEndHeight => WorldHeightProgressMath.CalculateSectionEndHeight(CurrentSectionIndex, totalCourseHeight, sectionCount); // 현재 구간 종료 높이 반환
        public bool HasReachedCourseTop => WorldHeightProgressMath.HasReachedCourseTop(CurrentHeight, totalCourseHeight); // 전체 코스 정상 도달 여부 반환

        private void Awake() // 높이 기준과 최초 진행 상태 준비
        { // 높이 진행 준비 범위
            fallbackOriginY = transform.position.y; // 기준점 누락 대비 시작 Y 좌표 저장
            ValidateConfiguration(); // 전체 코스 설정값 보정

            if (heightOrigin == null) // 높이 기준점 누락 여부 확인
            { // 높이 기준점 누락 범위
                ProjectLog.Warning(ProjectLogCategory.Gameplay, "HeightOrigin이 연결되지 않아 플레이어 시작 위치를 0미터로 사용합니다.", "HEIGHT_ORIGIN_MISSING", this); // 대체 기준점 사용 경고 출력
            } // 높이 기준점 누락 범위 종료

            RefreshProgress(); // 최초 현재 높이와 구간 계산
            HighestHeight = CurrentHeight; // 최초 최고 높이 기록
        } // 높이 진행 준비 범위 종료

        private void Update() // 현재 높이와 최고 높이와 구간 상태 갱신
        { // 높이 진행 프레임 갱신 범위
            RefreshProgress(); // 현재 프레임 높이와 구간 계산
            HighestHeight = WorldHeightProgressMath.CalculateHighestHeight(HighestHeight, CurrentHeight); // 기존 기록보다 높은 현재 높이 저장
        } // 높이 진행 프레임 갱신 범위 종료

        private void OnValidate() // Inspector 전체 코스 설정값 보정
        { // Inspector 설정 보정 범위
            ValidateConfiguration(); // 잘못된 높이와 구간 수 보정
        } // Inspector 설정 보정 범위 종료

        public void RefreshProgress() // 현재 Transform 위치에서 높이와 구간 즉시 갱신
        { // 높이와 구간 즉시 갱신 범위
            float originY = heightOrigin != null ? heightOrigin.position.y : fallbackOriginY; // 사용할 높이 기준 Y 좌표 선택
            CurrentHeight = WorldHeightProgressMath.CalculateHeight(transform.position.y, originY, metersPerUnityUnit); // 현재 월드 높이 미터 계산
            CurrentSectionIndex = WorldHeightProgressMath.CalculateSectionIndex(CurrentHeight, totalCourseHeight, sectionCount); // 현재 수직 구간 번호 계산
            CourseProgress01 = WorldHeightProgressMath.CalculateCourseProgress01(CurrentHeight, totalCourseHeight); // 전체 코스 진행 비율 계산
            SectionProgress01 = WorldHeightProgressMath.CalculateSectionProgress01(CurrentHeight, totalCourseHeight, sectionCount); // 현재 구간 진행 비율 계산
        } // 높이와 구간 즉시 갱신 범위 종료

        public void ResetHighestHeight() // 새 경기용 최고 높이 기록 초기화
        { // 최고 높이 초기화 범위
            RefreshProgress(); // 초기화 시점의 현재 진행 상태 갱신
            HighestHeight = CurrentHeight; // 현재 높이를 새 최고 높이 기준으로 저장
        } // 최고 높이 초기화 범위 종료

        public void SetHeightOrigin(Transform newHeightOrigin, bool resetHighestHeight) // 절차 생성 코스의 새 높이 기준점 적용
        { // 높이 기준점 변경 범위
            heightOrigin = newHeightOrigin; // 새 높이 기준 Transform 저장
            fallbackOriginY = newHeightOrigin != null ? newHeightOrigin.position.y : transform.position.y; // 새 대체 기준 Y 좌표 저장
            RefreshProgress(); // 새 기준점 기반 진행 상태 갱신

            if (resetHighestHeight) // 최고 높이 기록 초기화 요청 확인
            { // 최고 높이 기록 초기화 범위
                HighestHeight = CurrentHeight; // 새 기준점 기반 최고 높이 저장
            } // 최고 높이 기록 초기화 범위 종료
        } // 높이 기준점 변경 범위 종료

        private void ValidateConfiguration() // 높이 변환과 코스 구간 설정 유효성 보정
        { // 설정값 유효성 보정 범위
            metersPerUnityUnit = Mathf.Max(0.001f, metersPerUnityUnit); // 유니티 단위당 최소 미터 보장
            totalCourseHeight = Mathf.Max(0.001f, totalCourseHeight); // 전체 코스 최소 높이 보장
            sectionCount = Mathf.Max(1, sectionCount); // 최소 한 개의 수직 구간 보장
        } // 설정값 유효성 보정 범위 종료
    } // 플레이어 높이 진행 기능 범위 종료
} // 플레이어 높이 진행 범위 종료
