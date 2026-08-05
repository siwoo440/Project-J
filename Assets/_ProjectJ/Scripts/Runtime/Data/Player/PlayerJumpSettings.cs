using System; // Unity 직렬화 가능한 값 형식 기능 참조
using UnityEngine; // Unity Inspector 속성과 직렬화 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    [Serializable] // Unity Inspector 직렬화 지정
    public struct PlayerJumpSettings // 기본 점프 설정 값 형식 선언
    {
        [SerializeField, Min(0.01f)] private float jumpHeight; // 기본 점프의 수직 도달 높이 저장
        [SerializeField, Range(0f, 0.5f)] private float coyoteTime; // 지면 이탈 직후 점프를 허용하는 시간 저장
        [SerializeField, Range(0f, 0.5f)] private float jumpBufferTime; // 착지 직전 점프 입력을 보관하는 시간 저장

        public float JumpHeight => jumpHeight; // 기본 점프 높이 반환
        public float CoyoteTime => coyoteTime; // 코요테 타임 반환
        public float JumpBufferTime => jumpBufferTime; // 점프 입력 버퍼 시간 반환

        public PlayerJumpSettings(float jumpHeight, float coyoteTime, float jumpBufferTime) // 기본 점프 설정 값 생성
        {
            this.jumpHeight = jumpHeight; // 전달된 기본 점프 높이 저장
            this.coyoteTime = coyoteTime; // 전달된 코요테 타임 저장
            this.jumpBufferTime = jumpBufferTime; // 전달된 점프 입력 버퍼 시간 저장
        }

        public static PlayerJumpSettings CreateDefault() // 7일차 기본 점프 설정 생성
        {
            return new PlayerJumpSettings(2.4f, 0.12f, 0.12f); // 데이터 시트 점프 높이와 입력 보정 초기값 반환
        }

        public bool IsValid(out string reason) // 기본 점프 설정 값 유효 여부 검사
        {
            if (jumpHeight <= 0f) // 기본 점프 높이가 양수인지 확인
            {
                reason = "기본 점프 높이는 0보다 커야 합니다."; // 점프 높이 오류 사유 저장
                return false; // 점프 설정 검사 실패 반환
            }

            if (coyoteTime < 0f || coyoteTime > 0.5f) // 코요테 타임이 허용 범위인지 확인
            {
                reason = "코요테 타임은 0초부터 0.5초 사이여야 합니다."; // 코요테 타임 오류 사유 저장
                return false; // 점프 설정 검사 실패 반환
            }

            if (jumpBufferTime < 0f || jumpBufferTime > 0.5f) // 점프 입력 버퍼 시간이 허용 범위인지 확인
            {
                reason = "점프 입력 버퍼 시간은 0초부터 0.5초 사이여야 합니다."; // 점프 입력 버퍼 오류 사유 저장
                return false; // 점프 설정 검사 실패 반환
            }

            reason = string.Empty; // 검사 성공 결과로 오류 사유 초기화
            return true; // 점프 설정 검사 성공 반환
        }
    }
}
