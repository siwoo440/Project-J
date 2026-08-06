using System; // 열거형 플래그 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    public enum MapModuleKind // 맵 모듈 종류 선언
    { // 맵 모듈 종류 묶음
        FixedPlatform, // 고정 발판 모듈
        LowPassage, // 낮은 통로 모듈
        JumpGap, // 점프 간격 모듈
        Branch, // 경로 분기 모듈
        Merge, // 경로 합류 모듈
        Checkpoint, // 체크포인트 모듈
        CourseTop // 정상 도착 모듈
    } // 맵 모듈 종류 묶음 종료

    public enum MapConnectionRole // 연결 지점 역할 선언
    { // 연결 지점 역할 묶음
        Entrance, // 모듈 입구
        Exit // 모듈 출구
    } // 연결 지점 역할 묶음 종료

    public enum MapConnectionDirection // 연결 지점 방향 선언
    { // 연결 지점 방향 묶음
        North, // 로컬 앞쪽 방향
        East, // 로컬 오른쪽 방향
        South, // 로컬 뒤쪽 방향
        West // 로컬 왼쪽 방향
    } // 연결 지점 방향 묶음 종료

    public enum MapTraversalRequirement // 모듈 통과 조건 선언
    { // 모듈 통과 조건 묶음
        Walk, // 걷기 통과
        Crouch, // 앉기 통과
        Jump, // 점프 통과
        LedgeClimb // 끝자락 올라오기 통과
    } // 모듈 통과 조건 묶음 종료

    [Flags] // 여러 회전 선택 허용
    public enum MapRotationOptions // 모듈 회전 허용값 선언
    { // 모듈 회전 허용값 묶음
        None = 0, // 회전값 없음
        Degrees0 = 1 << 0, // 0도 회전
        Degrees90 = 1 << 1, // 90도 회전
        Degrees180 = 1 << 2, // 180도 회전
        Degrees270 = 1 << 3, // 270도 회전
        All = Degrees0 | Degrees90 | Degrees180 | Degrees270 // 모든 직각 회전
    } // 모듈 회전 허용값 묶음 종료
} // 맵 생성 기능 묶음 종료
