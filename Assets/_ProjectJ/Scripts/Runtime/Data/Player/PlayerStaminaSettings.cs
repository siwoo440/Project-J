using System; // Unity 직렬화 가능한 값 형식 기능 참조
using UnityEngine; // Unity Inspector 속성과 직렬화 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    [Serializable] // Unity Inspector 직렬화 지정
    public struct PlayerStaminaSettings // 달리기 스태미나 설정 값 형식 선언
    {
        [SerializeField, Min(0.01f)] private float maximumStamina; // 최대 스태미나 저장
        [SerializeField, Min(0.01f)] private float sprintDrainPerSecond; // 달리기 중 초당 스태미나 소비량 저장
        [SerializeField, Min(0.01f)] private float recoveryPerSecond; // 회복 시작 후 초당 스태미나 회복량 저장
        [SerializeField, Min(0f)] private float recoveryDelay; // 달리기 종료 후 회복 시작까지 대기 시간 저장
        [SerializeField, Min(0f)] private float minimumStaminaToStartSprint; // 달리기 시작에 필요한 최소 스태미나 저장

        public float MaximumStamina => maximumStamina; // 최대 스태미나 반환
        public float SprintDrainPerSecond => sprintDrainPerSecond; // 초당 달리기 소비량 반환
        public float RecoveryPerSecond => recoveryPerSecond; // 초당 스태미나 회복량 반환
        public float RecoveryDelay => recoveryDelay; // 스태미나 회복 대기 시간 반환
        public float MinimumStaminaToStartSprint => minimumStaminaToStartSprint; // 달리기 시작 최소 스태미나 반환

        public PlayerStaminaSettings( // 스태미나 설정 값 생성
            float maximumStamina, // 최대 스태미나 입력
            float sprintDrainPerSecond, // 초당 소비량 입력
            float recoveryPerSecond, // 초당 회복량 입력
            float recoveryDelay, // 회복 대기 시간 입력
            float minimumStaminaToStartSprint) // 달리기 시작 최소 스태미나 입력
        {
            this.maximumStamina = maximumStamina; // 전달된 최대 스태미나 저장
            this.sprintDrainPerSecond = sprintDrainPerSecond; // 전달된 초당 소비량 저장
            this.recoveryPerSecond = recoveryPerSecond; // 전달된 초당 회복량 저장
            this.recoveryDelay = recoveryDelay; // 전달된 회복 대기 시간 저장
            this.minimumStaminaToStartSprint = minimumStaminaToStartSprint; // 전달된 달리기 시작 최소 스태미나 저장
        }

        public static PlayerStaminaSettings CreateDefault() // 7일차 기본 스태미나 설정 생성
        {
            return new PlayerStaminaSettings(100f, 20f, 25f, 0.75f, 5f); // 프로토타입용 스태미나 초기값 반환
        }

        public bool IsValid(out string reason) // 스태미나 설정 값 유효 여부 검사
        {
            if (maximumStamina <= 0f) // 최대 스태미나가 양수인지 확인
            {
                reason = "최대 스태미나는 0보다 커야 합니다."; // 최대 스태미나 오류 사유 저장
                return false; // 스태미나 설정 검사 실패 반환
            }

            if (sprintDrainPerSecond <= 0f) // 초당 달리기 소비량이 양수인지 확인
            {
                reason = "달리기 초당 스태미나 소비량은 0보다 커야 합니다."; // 달리기 소비량 오류 사유 저장
                return false; // 스태미나 설정 검사 실패 반환
            }

            if (recoveryPerSecond <= 0f) // 초당 스태미나 회복량이 양수인지 확인
            {
                reason = "초당 스태미나 회복량은 0보다 커야 합니다."; // 스태미나 회복량 오류 사유 저장
                return false; // 스태미나 설정 검사 실패 반환
            }

            if (recoveryDelay < 0f) // 스태미나 회복 대기 시간이 음수인지 확인
            {
                reason = "스태미나 회복 대기 시간은 0 이상이어야 합니다."; // 스태미나 회복 대기 시간 오류 사유 저장
                return false; // 스태미나 설정 검사 실패 반환
            }

            if (minimumStaminaToStartSprint < 0f || minimumStaminaToStartSprint > maximumStamina) // 달리기 시작 최소 스태미나가 유효 범위인지 확인
            {
                reason = "달리기 시작 최소 스태미나는 0 이상이고 최대 스태미나 이하여야 합니다."; // 달리기 시작 최소값 오류 사유 저장
                return false; // 스태미나 설정 검사 실패 반환
            }

            reason = string.Empty; // 검사 성공 결과로 오류 사유 초기화
            return true; // 스태미나 설정 검사 성공 반환
        }
    }
}
