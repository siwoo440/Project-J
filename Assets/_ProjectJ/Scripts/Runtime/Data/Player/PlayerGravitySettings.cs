using System; // Unity 직렬화 가능한 값 형식 기능 참조
using UnityEngine; // Unity Inspector 속성과 직렬화 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    [Serializable] // Unity Inspector 직렬화 지정
    public struct PlayerGravitySettings // 중력과 낙하 속도 설정 값 형식 선언
    {
        [SerializeField] private float gravityAcceleration; // 공중에서 적용할 초당 중력 가속도 저장
        [SerializeField] private float groundedGravity; // 접지 상태를 유지하기 위한 작은 하향 속도 저장
        [SerializeField, Min(0.01f)] private float maximumFallSpeed; // 최대 낙하 속도의 절댓값 저장

        public float GravityAcceleration => gravityAcceleration; // 중력 가속도 반환
        public float GroundedGravity => groundedGravity; // 접지 유지용 하향 속도 반환
        public float MaximumFallSpeed => maximumFallSpeed; // 최대 낙하 속도의 절댓값 반환

        public PlayerGravitySettings(float gravityAcceleration, float groundedGravity, float maximumFallSpeed) // 중력 설정 값 생성
        {
            this.gravityAcceleration = gravityAcceleration; // 전달된 중력 가속도 저장
            this.groundedGravity = groundedGravity; // 전달된 접지 유지용 하향 속도 저장
            this.maximumFallSpeed = maximumFallSpeed; // 전달된 최대 낙하 속도 저장
        }

        public static PlayerGravitySettings CreateDefault() // 7일차 기본 중력 설정 생성
        {
            return new PlayerGravitySettings(-25f, -2f, 35f); // CharacterController 프로토타입용 중력 초기값 반환
        }

        public bool IsValid(out string reason) // 중력 설정 값 유효 여부 검사
        {
            if (gravityAcceleration >= 0f) // 중력 가속도가 아래 방향인지 확인
            {
                reason = "중력 가속도는 0보다 작은 아래 방향 값이어야 합니다."; // 중력 방향 오류 사유 저장
                return false; // 중력 설정 검사 실패 반환
            }

            if (groundedGravity > 0f) // 접지 유지용 중력이 위 방향인지 확인
            {
                reason = "접지 유지용 중력은 0 이하의 값이어야 합니다."; // 접지 중력 방향 오류 사유 저장
                return false; // 중력 설정 검사 실패 반환
            }

            if (maximumFallSpeed <= 0f) // 최대 낙하 속도의 절댓값이 양수인지 확인
            {
                reason = "최대 낙하 속도는 0보다 커야 합니다."; // 최대 낙하 속도 오류 사유 저장
                return false; // 중력 설정 검사 실패 반환
            }

            reason = string.Empty; // 검사 성공 결과로 오류 사유 초기화
            return true; // 중력 설정 검사 성공 반환
        }
    }
}
