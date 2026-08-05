using System; // Unity 직렬화 가능한 값 형식 기능 참조
using UnityEngine; // Unity Inspector 속성과 직렬화 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    [Serializable] // Unity Inspector 직렬화 지정
    public struct PlayerMovementSettings // 기본 지상 이동 설정 값 형식 선언
    {
        [SerializeField, Min(0.01f)] private float moveSpeed; // 기본 지상 이동 속도 저장
        [SerializeField, Min(0.01f)] private float acceleration; // 목표 속도까지 도달하는 지상 가속도 저장
        [SerializeField, Min(0.01f)] private float deceleration; // 입력 해제 시 사용하는 지상 감속도 저장
        [SerializeField, Min(0.01f)] private float rotationSpeed; // 이동 방향을 향하는 초당 회전 속도 저장

        public float MoveSpeed => moveSpeed; // 기본 지상 이동 속도 반환
        public float Acceleration => acceleration; // 지상 가속도 반환
        public float Deceleration => deceleration; // 지상 감속도 반환
        public float RotationSpeed => rotationSpeed; // 초당 회전 속도 반환

        public PlayerMovementSettings(float moveSpeed, float acceleration, float deceleration, float rotationSpeed) // 지상 이동 설정 값 생성
        {
            this.moveSpeed = moveSpeed; // 전달된 기본 이동 속도 저장
            this.acceleration = acceleration; // 전달된 지상 가속도 저장
            this.deceleration = deceleration; // 전달된 지상 감속도 저장
            this.rotationSpeed = rotationSpeed; // 전달된 초당 회전 속도 저장
        }

        public static PlayerMovementSettings CreateDefault() // 7일차 기본 지상 이동 설정 생성
        {
            return new PlayerMovementSettings(6f, 24f, 30f, 720f); // 데이터 시트 이동 속도와 초기 반응값을 사용하는 설정 반환
        }

        public bool IsValid(out string reason) // 지상 이동 설정 값 유효 여부 검사
        {
            if (moveSpeed <= 0f) // 기본 이동 속도가 양수인지 확인
            {
                reason = "기본 이동 속도는 0보다 커야 합니다."; // 이동 속도 오류 사유 저장
                return false; // 이동 설정 검사 실패 반환
            }

            if (acceleration <= 0f) // 지상 가속도가 양수인지 확인
            {
                reason = "지상 가속도는 0보다 커야 합니다."; // 가속도 오류 사유 저장
                return false; // 이동 설정 검사 실패 반환
            }

            if (deceleration <= 0f) // 지상 감속도가 양수인지 확인
            {
                reason = "지상 감속도는 0보다 커야 합니다."; // 감속도 오류 사유 저장
                return false; // 이동 설정 검사 실패 반환
            }

            if (rotationSpeed <= 0f) // 초당 회전 속도가 양수인지 확인
            {
                reason = "회전 속도는 0보다 커야 합니다."; // 회전 속도 오류 사유 저장
                return false; // 이동 설정 검사 실패 반환
            }

            reason = string.Empty; // 검사 성공 결과로 오류 사유 초기화
            return true; // 이동 설정 검사 성공 반환
        }
    }
}
