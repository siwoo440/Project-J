using UnityEngine; // Unity ScriptableObject 생성 메뉴와 Inspector 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Project J/Data/Player")] // Project 창 플레이어 데이터 에셋 생성 메뉴 등록
    public sealed class PlayerDataDefinition : ProjectDataAsset // Player 데이터 정의 에셋 선언
    {
        [Header("Movement")] // 지상 이동 설정 구역 제목 표시
        [SerializeField] private PlayerMovementSettings movement = PlayerMovementSettings.CreateDefault(); // 기본 지상 이동 설정 저장

        [Header("Sprint")] // 달리기 설정 구역 제목 표시
        [SerializeField] private PlayerSprintSettings sprint = PlayerSprintSettings.CreateDefault(); // 달리기 설정 저장

        [Header("Crouch")] // 앉기 설정 구역 제목 표시
        [SerializeField] private PlayerCrouchSettings crouch = PlayerCrouchSettings.CreateDefault(); // 앉기와 충돌체 설정 저장

        [Header("Jump")] // 점프 설정 구역 제목 표시
        [SerializeField] private PlayerJumpSettings jump = PlayerJumpSettings.CreateDefault(); // 기본 점프 설정 저장

        [Header("Gravity")] // 중력 설정 구역 제목 표시
        [SerializeField] private PlayerGravitySettings gravity = PlayerGravitySettings.CreateDefault(); // 중력과 낙하 설정 저장

        [Header("Air Control")] // 공중 제어 설정 구역 제목 표시
        [SerializeField] private PlayerAirControlSettings airControl = PlayerAirControlSettings.CreateDefault(); // 공중 방향 제어 설정 저장

        [Header("Stamina")] // 스태미나 설정 구역 제목 표시
        [SerializeField] private PlayerStaminaSettings stamina = PlayerStaminaSettings.CreateDefault(); // 달리기 스태미나 설정 저장

        public override ProjectDataCategory Category => ProjectDataCategory.Player; // Player 데이터 분류 반환
        public PlayerMovementSettings Movement => movement; // 기본 지상 이동 설정 반환
        public PlayerSprintSettings Sprint => sprint; // 달리기 설정 반환
        public PlayerCrouchSettings Crouch => crouch; // 앉기 설정 반환
        public PlayerJumpSettings Jump => jump; // 기본 점프 설정 반환
        public PlayerGravitySettings Gravity => gravity; // 중력 설정 반환
        public PlayerAirControlSettings AirControl => airControl; // 공중 제어 설정 반환
        public PlayerStaminaSettings Stamina => stamina; // 스태미나 설정 반환

#if UNITY_EDITOR
        public void SetEditorSettings( // Editor 도구와 EditMode 테스트용 플레이어 설정 변경
            PlayerMovementSettings newMovement, // 새 지상 이동 설정 입력
            PlayerSprintSettings newSprint, // 새 달리기 설정 입력
            PlayerCrouchSettings newCrouch, // 새 앉기 설정 입력
            PlayerJumpSettings newJump, // 새 점프 설정 입력
            PlayerGravitySettings newGravity, // 새 중력 설정 입력
            PlayerAirControlSettings newAirControl, // 새 공중 제어 설정 입력
            PlayerStaminaSettings newStamina) // 새 스태미나 설정 입력
        {
            movement = newMovement; // 새 지상 이동 설정 저장
            sprint = newSprint; // 새 달리기 설정 저장
            crouch = newCrouch; // 새 앉기 설정 저장
            jump = newJump; // 새 점프 설정 저장
            gravity = newGravity; // 새 중력 설정 저장
            airControl = newAirControl; // 새 공중 제어 설정 저장
            stamina = newStamina; // 새 스태미나 설정 저장
        }

        public void ResetEditorSettingsToDefaults() // Editor에서 모든 플레이어 설정을 7일차 기본값으로 초기화
        {
            SetEditorSettings( // 기본 설정 값 일괄 적용
                PlayerMovementSettings.CreateDefault(), // 기본 지상 이동 설정 적용
                PlayerSprintSettings.CreateDefault(), // 기본 달리기 설정 적용
                PlayerCrouchSettings.CreateDefault(), // 기본 앉기 설정 적용
                PlayerJumpSettings.CreateDefault(), // 기본 점프 설정 적용
                PlayerGravitySettings.CreateDefault(), // 기본 중력 설정 적용
                PlayerAirControlSettings.CreateDefault(), // 기본 공중 제어 설정 적용
                PlayerStaminaSettings.CreateDefault()); // 기본 스태미나 설정 적용
        }
#endif
    }
}
