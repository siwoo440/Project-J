using System.Linq; // 검증 문제 코드 검색 기능 참조
using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Data; // 플레이어 데이터 설정과 검증 형식 참조
using UnityEngine; // ScriptableObject 생성과 제거 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{
    public sealed class PlayerSettingsTests // 플레이어 설정 기본값과 오류 검증 테스트 형식 선언
    {
        private PlayerDataDefinition playerData; // 테스트에서 사용할 임시 플레이어 데이터 에셋 저장

        [SetUp] // 각 테스트 실행 전 준비 메서드 지정
        public void SetUp() // 기본 플레이어 설정을 가진 임시 에셋 생성
        {
            playerData = ScriptableObject.CreateInstance<PlayerDataDefinition>(); // 임시 플레이어 데이터 에셋 인스턴스 생성
            playerData.SetEditorIdentity("PLY-001", "Default Player", new ProjectDataVersion(1, 1, 0)); // 임시 플레이어 데이터 식별 정보 설정
            playerData.ResetEditorSettingsToDefaults(); // 임시 플레이어 데이터에 7일차 기본값 적용
        }

        [TearDown] // 각 테스트 실행 후 정리 메서드 지정
        public void TearDown() // 임시 플레이어 데이터 에셋 제거
        {
            if (playerData != null) // 임시 플레이어 데이터 에셋 존재 여부 확인
            {
                Object.DestroyImmediate(playerData); // 임시 플레이어 데이터 에셋 즉시 제거
                playerData = null; // 임시 플레이어 데이터 에셋 참조 초기화
            }
        }

        [Test] // Unity Test Runner 테스트 지정
        public void DefaultPlayerSettingsAreValid() // 7일차 전체 기본 플레이어 설정 유효 여부 검증
        {
            ProjectDataValidationReport report = ProjectDataValidator.Validate(new[] { playerData }); // 기본 플레이어 데이터 전체 검증 실행

            Assert.IsTrue(report.IsValid); // 기본 플레이어 데이터 검증 성공 여부 확인
            Assert.AreEqual(0, report.ErrorCount); // 기본 플레이어 데이터 오류가 없는지 확인
        }

        [Test] // Unity Test Runner 테스트 지정
        public void SheetBackedValuesMatchInitialDefaults() // 데이터 시트 기반 세 가지 핵심 수치 일치 여부 검증
        {
            Assert.AreEqual(6f, playerData.Movement.MoveSpeed); // 데이터 시트 기본 이동 속도 6m/s 일치 여부 검증
            Assert.AreEqual(2.4f, playerData.Jump.JumpHeight); // 데이터 시트 기본 점프 높이 2.4m 일치 여부 검증
            Assert.AreEqual(0.65f, playerData.AirControl.ControlRatio); // 데이터 시트 공중 제어 비율 0.65 일치 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void SprintSpeedMustExceedMoveSpeed() // 달리기 속도와 기본 이동 속도 관계 검증
        {
            SetSettings(sprint: new PlayerSprintSettings(6f, 30f)); // 기본 이동 속도와 같은 잘못된 달리기 속도 적용
            ProjectDataValidationReport report = ProjectDataValidator.Validate(new[] { playerData }); // 잘못된 달리기 설정 검증 실행

            AssertIssue(report, PlayerSettingsValidationRules.SprintCode); // 달리기 설정 오류 코드 존재 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void CrouchingHeightMustBeLowerThanStandingHeight() // 앉기 높이와 서기 높이 관계 검증
        {
            SetSettings(crouch: new PlayerCrouchSettings(3.5f, 2f, 2f, 0.45f, 8f, 0.05f)); // 서기 높이와 같은 잘못된 앉기 높이 적용
            ProjectDataValidationReport report = ProjectDataValidator.Validate(new[] { playerData }); // 잘못된 앉기 설정 검증 실행

            AssertIssue(report, PlayerSettingsValidationRules.CrouchCode); // 앉기 설정 오류 코드 존재 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void GravityMustPointDownward() // 중력 가속도 아래 방향 규칙 검증
        {
            SetSettings(gravity: new PlayerGravitySettings(25f, -2f, 35f)); // 위 방향의 잘못된 중력 가속도 적용
            ProjectDataValidationReport report = ProjectDataValidator.Validate(new[] { playerData }); // 잘못된 중력 설정 검증 실행

            AssertIssue(report, PlayerSettingsValidationRules.GravityCode); // 중력 설정 오류 코드 존재 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void AirControlRatioCannotExceedOne() // 공중 제어 비율 상한 규칙 검증
        {
            SetSettings(airControl: new PlayerAirControlSettings(1.2f, 12f)); // 1을 초과하는 잘못된 공중 제어 비율 적용
            ProjectDataValidationReport report = ProjectDataValidator.Validate(new[] { playerData }); // 잘못된 공중 제어 설정 검증 실행

            AssertIssue(report, PlayerSettingsValidationRules.AirControlCode); // 공중 제어 설정 오류 코드 존재 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void MinimumSprintStaminaCannotExceedMaximum() // 달리기 시작 최소 스태미나 상한 규칙 검증
        {
            SetSettings(stamina: new PlayerStaminaSettings(100f, 20f, 25f, 0.75f, 120f)); // 최대값을 초과하는 달리기 시작 최소 스태미나 적용
            ProjectDataValidationReport report = ProjectDataValidator.Validate(new[] { playerData }); // 잘못된 스태미나 설정 검증 실행

            AssertIssue(report, PlayerSettingsValidationRules.StaminaCode); // 스태미나 설정 오류 코드 존재 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void ProjectDataValidatorReportsMultiplePlayerSettingErrors() // 공통 검증기의 여러 플레이어 설정 오류 수집 여부 검증
        {
            playerData.SetEditorSettings( // 여러 구역에 잘못된 플레이어 설정 일괄 적용
                new PlayerMovementSettings(0f, 24f, 30f, 720f), // 잘못된 이동 속도 적용
                new PlayerSprintSettings(0f, 30f), // 잘못된 달리기 속도 적용
                PlayerCrouchSettings.CreateDefault(), // 정상 앉기 설정 유지
                PlayerJumpSettings.CreateDefault(), // 정상 점프 설정 유지
                new PlayerGravitySettings(10f, 1f, 35f), // 잘못된 중력 설정 적용
                PlayerAirControlSettings.CreateDefault(), // 정상 공중 제어 설정 유지
                new PlayerStaminaSettings(0f, 0f, 0f, -1f, 1f)); // 잘못된 스태미나 설정 적용

            ProjectDataValidationReport report = ProjectDataValidator.Validate(new[] { playerData }); // 여러 잘못된 플레이어 설정 검증 실행

            AssertIssue(report, PlayerSettingsValidationRules.MovementCode); // 이동 설정 오류 수집 여부 검증
            AssertIssue(report, PlayerSettingsValidationRules.SprintCode); // 달리기 설정 오류 수집 여부 검증
            AssertIssue(report, PlayerSettingsValidationRules.GravityCode); // 중력 설정 오류 수집 여부 검증
            AssertIssue(report, PlayerSettingsValidationRules.StaminaCode); // 스태미나 설정 오류 수집 여부 검증
        }

        private void SetSettings( // 일부 설정만 교체하기 위한 테스트 보조 메서드
            PlayerMovementSettings? movement = null, // 선택적 새 지상 이동 설정 입력
            PlayerSprintSettings? sprint = null, // 선택적 새 달리기 설정 입력
            PlayerCrouchSettings? crouch = null, // 선택적 새 앉기 설정 입력
            PlayerJumpSettings? jump = null, // 선택적 새 점프 설정 입력
            PlayerGravitySettings? gravity = null, // 선택적 새 중력 설정 입력
            PlayerAirControlSettings? airControl = null, // 선택적 새 공중 제어 설정 입력
            PlayerStaminaSettings? stamina = null) // 선택적 새 스태미나 설정 입력
        {
            playerData.SetEditorSettings( // 현재값 또는 전달된 새 값 일괄 적용
                movement ?? playerData.Movement, // 새 이동 설정 또는 현재 이동 설정 적용
                sprint ?? playerData.Sprint, // 새 달리기 설정 또는 현재 달리기 설정 적용
                crouch ?? playerData.Crouch, // 새 앉기 설정 또는 현재 앉기 설정 적용
                jump ?? playerData.Jump, // 새 점프 설정 또는 현재 점프 설정 적용
                gravity ?? playerData.Gravity, // 새 중력 설정 또는 현재 중력 설정 적용
                airControl ?? playerData.AirControl, // 새 공중 제어 설정 또는 현재 공중 제어 설정 적용
                stamina ?? playerData.Stamina); // 새 스태미나 설정 또는 현재 스태미나 설정 적용
        }

        private static void AssertIssue(ProjectDataValidationReport report, string expectedCode) // 지정 오류 코드가 검증 결과에 포함되는지 확인
        {
            Assert.IsTrue(report.Issues.Any(issue => issue.Code == expectedCode), $"{expectedCode} 오류가 검증 결과에 없습니다."); // 지정 오류 코드 존재 여부 검증
        }
    }
}
