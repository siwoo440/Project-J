using System; // Unity 직렬화 가능한 값 형식 기능 참조
using UnityEngine; // Unity Inspector 속성과 직렬화 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    [Serializable] // Unity Inspector 직렬화 지정
    public struct PlayerSprintSettings // 달리기 설정 값 형식 선언
    {
        [SerializeField, Min(0.01f)] private float sprintSpeed; // 달리기 중 목표 이동 속도 저장
        [SerializeField, Min(0.01f)] private float sprintAcceleration; // 달리기 속도까지 도달하는 가속도 저장

        public float SprintSpeed => sprintSpeed; // 달리기 목표 이동 속도 반환
        public float SprintAcceleration => sprintAcceleration; // 달리기 가속도 반환

        public PlayerSprintSettings(float sprintSpeed, float sprintAcceleration) // 달리기 설정 값 생성
        {
            this.sprintSpeed = sprintSpeed; // 전달된 달리기 속도 저장
            this.sprintAcceleration = sprintAcceleration; // 전달된 달리기 가속도 저장
        }

        public static PlayerSprintSettings CreateDefault() // 7일차 기본 달리기 설정 생성
        {
            return new PlayerSprintSettings(8f, 30f); // 프로토타입용 달리기 초기값 반환
        }

        public bool IsValid(float moveSpeed, out string reason) // 기본 이동 속도와 비교한 달리기 설정 유효 여부 검사
        {
            if (sprintSpeed <= moveSpeed) // 달리기 속도가 기본 이동 속도보다 빠른지 확인
            {
                reason = "달리기 속도는 기본 이동 속도보다 커야 합니다."; // 달리기 속도 관계 오류 사유 저장
                return false; // 달리기 설정 검사 실패 반환
            }

            if (sprintAcceleration <= 0f) // 달리기 가속도가 양수인지 확인
            {
                reason = "달리기 가속도는 0보다 커야 합니다."; // 달리기 가속도 오류 사유 저장
                return false; // 달리기 설정 검사 실패 반환
            }

            reason = string.Empty; // 검사 성공 결과로 오류 사유 초기화
            return true; // 달리기 설정 검사 성공 반환
        }
    }
}
