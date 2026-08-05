using System; // Unity 직렬화 가능한 값 형식 기능 참조
using UnityEngine; // Unity Inspector 속성과 직렬화 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    [Serializable] // Unity Inspector 직렬화 지정
    public struct PlayerAirControlSettings // 공중 방향 제어 설정 값 형식 선언
    {
        [SerializeField, Range(0f, 1f)] private float controlRatio; // 지상 대비 공중 방향 제어 비율 저장
        [SerializeField, Min(0.01f)] private float acceleration; // 공중에서 목표 방향으로 전환하는 가속도 저장

        public float ControlRatio => controlRatio; // 지상 대비 공중 방향 제어 비율 반환
        public float Acceleration => acceleration; // 공중 방향 전환 가속도 반환

        public PlayerAirControlSettings(float controlRatio, float acceleration) // 공중 제어 설정 값 생성
        {
            this.controlRatio = controlRatio; // 전달된 공중 제어 비율 저장
            this.acceleration = acceleration; // 전달된 공중 방향 전환 가속도 저장
        }

        public static PlayerAirControlSettings CreateDefault() // 7일차 기본 공중 제어 설정 생성
        {
            return new PlayerAirControlSettings(0.65f, 12f); // 데이터 시트 공중 제어 비율과 초기 가속도 반환
        }

        public bool IsValid(out string reason) // 공중 제어 설정 값 유효 여부 검사
        {
            if (controlRatio <= 0f || controlRatio > 1f) // 공중 제어 비율이 유효 범위인지 확인
            {
                reason = "공중 제어 비율은 0보다 크고 1 이하여야 합니다."; // 공중 제어 비율 오류 사유 저장
                return false; // 공중 제어 설정 검사 실패 반환
            }

            if (acceleration <= 0f) // 공중 방향 전환 가속도가 양수인지 확인
            {
                reason = "공중 제어 가속도는 0보다 커야 합니다."; // 공중 제어 가속도 오류 사유 저장
                return false; // 공중 제어 설정 검사 실패 반환
            }

            reason = string.Empty; // 검사 성공 결과로 오류 사유 초기화
            return true; // 공중 제어 설정 검사 성공 반환
        }
    }
}
