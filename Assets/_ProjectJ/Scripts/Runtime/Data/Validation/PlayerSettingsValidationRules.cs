namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    public static class PlayerSettingsValidationRules // 플레이어 설정 에셋 전용 검증 규칙 선언
    {
        public const string MovementCode = "PLAYER_MOVEMENT_INVALID"; // 지상 이동 설정 오류 코드 선언
        public const string SprintCode = "PLAYER_SPRINT_INVALID"; // 달리기 설정 오류 코드 선언
        public const string CrouchCode = "PLAYER_CROUCH_INVALID"; // 앉기 설정 오류 코드 선언
        public const string JumpCode = "PLAYER_JUMP_INVALID"; // 점프 설정 오류 코드 선언
        public const string GravityCode = "PLAYER_GRAVITY_INVALID"; // 중력 설정 오류 코드 선언
        public const string AirControlCode = "PLAYER_AIR_CONTROL_INVALID"; // 공중 제어 설정 오류 코드 선언
        public const string StaminaCode = "PLAYER_STAMINA_INVALID"; // 스태미나 설정 오류 코드 선언

        public static void Validate(PlayerDataDefinition playerData, ProjectDataValidationReport report) // 플레이어 설정의 모든 구역 검사
        {
            if (playerData == null || report == null) // 플레이어 데이터 또는 결과 객체 누락 여부 확인
            {
                return; // 누락된 입력에서 플레이어 설정 검사 생략
            }

            if (!playerData.Movement.IsValid(out string movementReason)) // 지상 이동 설정 유효 여부 확인
            {
                report.AddError(playerData, MovementCode, movementReason); // 지상 이동 설정 오류 추가
            }

            if (!playerData.Sprint.IsValid(playerData.Movement.MoveSpeed, out string sprintReason)) // 달리기 설정 유효 여부 확인
            {
                report.AddError(playerData, SprintCode, sprintReason); // 달리기 설정 오류 추가
            }

            if (!playerData.Crouch.IsValid(playerData.Movement.MoveSpeed, out string crouchReason)) // 앉기 설정 유효 여부 확인
            {
                report.AddError(playerData, CrouchCode, crouchReason); // 앉기 설정 오류 추가
            }

            if (!playerData.Jump.IsValid(out string jumpReason)) // 기본 점프 설정 유효 여부 확인
            {
                report.AddError(playerData, JumpCode, jumpReason); // 기본 점프 설정 오류 추가
            }

            if (!playerData.Gravity.IsValid(out string gravityReason)) // 중력 설정 유효 여부 확인
            {
                report.AddError(playerData, GravityCode, gravityReason); // 중력 설정 오류 추가
            }

            if (!playerData.AirControl.IsValid(out string airControlReason)) // 공중 제어 설정 유효 여부 확인
            {
                report.AddError(playerData, AirControlCode, airControlReason); // 공중 제어 설정 오류 추가
            }

            if (!playerData.Stamina.IsValid(out string staminaReason)) // 스태미나 설정 유효 여부 확인
            {
                report.AddError(playerData, StaminaCode, staminaReason); // 스태미나 설정 오류 추가
            }
        }
    }
}
