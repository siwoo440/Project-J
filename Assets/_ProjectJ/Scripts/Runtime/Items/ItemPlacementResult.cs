using UnityEngine; // Unity 위치와 방향 자료형 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    public enum ItemPlacementFailureReason // 설치 위치 실패 원인 선언
    { // 설치 위치 실패 원인 묶음
        None, // 설치 가능 상태
        MissingValidator, // 공통 검사기 누락
        NoGround, // 지면 미검출
        SlopeTooSteep, // 허용 경사 초과
        Blocked, // 장애물 공간 점유
        OutsideAllowedArea // 허용 영역 이탈
    } // 설치 위치 실패 원인 묶음 종료

    public readonly struct ItemPlacementResult // 설치 위치 검사 결과 선언
    { // 설치 위치 검사 결과 묶음
        public ItemPlacementResult(bool isValid, ItemPlacementFailureReason failureReason, Vector3 position, Vector3 surfaceNormal, Collider groundCollider) // 설치 위치 검사 결과 생성
        { // 설치 위치 검사 결과 저장 처리
            IsValid = isValid; // 설치 가능 여부 저장
            FailureReason = failureReason; // 실패 원인 저장
            Position = position; // 지면 보정 위치 저장
            SurfaceNormal = surfaceNormal; // 지면 법선 저장
            GroundCollider = groundCollider; // 감지 지면 Collider 저장
        } // 설치 위치 검사 결과 저장 처리 종료

        public bool IsValid { get; } // 설치 가능 여부 반환
        public ItemPlacementFailureReason FailureReason { get; } // 실패 원인 반환
        public Vector3 Position { get; } // 지면 보정 위치 반환
        public Vector3 SurfaceNormal { get; } // 지면 법선 반환
        public Collider GroundCollider { get; } // 감지 지면 Collider 반환

        public static ItemPlacementResult CreateFailure(ItemPlacementFailureReason failureReason, Vector3 requestedPosition) // 설치 실패 결과 생성
        { // 설치 실패 결과 생성 처리
            return new ItemPlacementResult(false, failureReason, requestedPosition, Vector3.up, null); // 실패 원인 포함 결과 반환
        } // 설치 실패 결과 생성 처리 종료

        public static ItemPlacementResult CreateSuccess(Vector3 position, Vector3 surfaceNormal, Collider groundCollider) // 설치 성공 결과 생성
        { // 설치 성공 결과 생성 처리
            return new ItemPlacementResult(true, ItemPlacementFailureReason.None, position, surfaceNormal, groundCollider); // 설치 가능 결과 반환
        } // 설치 성공 결과 생성 처리 종료
    } // 설치 위치 검사 결과 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
