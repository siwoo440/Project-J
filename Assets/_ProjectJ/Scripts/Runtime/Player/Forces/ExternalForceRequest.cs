using UnityEngine; // Unity 벡터 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 외부 힘 요청 범위
    public enum ExternalForceSource // 외부 힘 발생 원인 종류
    { // 외부 힘 원인 범위
        Push, // 플레이어 밀치기 힘
        Platform, // 이동 발판 전달 속도
        Obstacle // 장애물 충돌 힘
    } // 외부 힘 원인 범위 종료

    public enum ExternalForceApplication // 외부 힘 결합 방식 종류
    { // 외부 힘 결합 범위
        ReplaceImpulse, // 기존 순간 힘 교체
        AddImpulse, // 기존 순간 힘에 추가
        SetCarrierVelocity // 발판 전달 속도 갱신
    } // 외부 힘 결합 범위 종료

    public readonly struct ExternalForceRequest // 외부 힘 통합 요청 값 선언
    { // 외부 힘 요청 값 범위
        public Vector3 Velocity { get; } // 요청 외부 속도
        public ExternalForceSource Source { get; } // 요청 발생 원인
        public ExternalForceApplication Application { get; } // 요청 결합 방식
        public bool StartsHitImmunity { get; } // 적용 뒤 피격 면역 시작 여부

        public ExternalForceRequest(Vector3 velocity, ExternalForceSource source, ExternalForceApplication application, bool startsHitImmunity) // 외부 힘 요청 값 생성
        { // 외부 힘 요청 생성 범위
            Velocity = velocity; // 요청 속도 저장
            Source = source; // 발생 원인 저장
            Application = application; // 결합 방식 저장
            StartsHitImmunity = startsHitImmunity; // 피격 면역 시작 여부 저장
        } // 외부 힘 요청 생성 범위 종료

        public static ExternalForceRequest CreatePush(Vector3 direction, float force) // 플레이어 밀치기 요청 생성
        { // 밀치기 요청 생성 범위
            Vector3 horizontalDirection = Vector3.ProjectOnPlane(direction, Vector3.up); // 수평 밀치기 방향 계산
            Vector3 velocity = horizontalDirection.sqrMagnitude <= 0.0001f ? Vector3.zero : horizontalDirection.normalized * Mathf.Max(0f, force); // 유효 방향 기반 밀치기 속도 계산
            return new ExternalForceRequest(velocity, ExternalForceSource.Push, ExternalForceApplication.ReplaceImpulse, true); // 교체형 밀치기 요청 반환
        } // 밀치기 요청 생성 범위 종료

        public static ExternalForceRequest CreatePlatform(Vector3 velocity) // 이동 발판 전달 속도 요청 생성
        { // 발판 요청 생성 범위
            return new ExternalForceRequest(velocity, ExternalForceSource.Platform, ExternalForceApplication.SetCarrierVelocity, false); // 발판 속도 요청 반환
        } // 발판 요청 생성 범위 종료

        public static ExternalForceRequest CreateObstacle(Vector3 direction, float force) // 장애물 순간 힘 요청 생성
        { // 장애물 요청 생성 범위
            Vector3 velocity = direction.sqrMagnitude <= 0.0001f ? Vector3.zero : direction.normalized * Mathf.Max(0f, force); // 방향과 세기 기반 장애물 속도 계산
            return new ExternalForceRequest(velocity, ExternalForceSource.Obstacle, ExternalForceApplication.AddImpulse, false); // 누적형 장애물 요청 반환
        } // 장애물 요청 생성 범위 종료
    } // 외부 힘 요청 값 범위 종료
} // 외부 힘 요청 범위 종료
