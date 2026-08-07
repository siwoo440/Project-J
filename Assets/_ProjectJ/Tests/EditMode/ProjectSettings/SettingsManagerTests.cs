using NUnit.Framework; // Unity EditMode 테스트 기능 참조
using ProjectJ.Core.Services; // 설정 데이터와 관리자 기능 참조
using UnityEngine; // Unity 화면 모드와 수치 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 50일차 설정 기반 회귀 테스트 정의
    public sealed class SettingsManagerTests // 설정 복사·검증·JSON·관리자 안전 접근 테스트 선언
    { // 디스크 파일을 변경하지 않는 설정 기반 단위 테스트 정의
        [SetUp] // 각 테스트 실행 전 준비 메서드 지정
        public void SetUp() // 테스트 전 공통 서비스 정적 상태 초기화
        { // 이전 테스트의 서비스 등록 상태 제거
            GameServiceRegistry.ResetForTests(); // 공통 서비스 레지스트리 초기화
        } // 테스트 전 공통 서비스 정적 상태 초기화 완료

        [TearDown] // 각 테스트 실행 후 정리 메서드 지정
        public void TearDown() // 테스트 후 공통 서비스 정적 상태 초기화
        { // 현재 테스트의 서비스 등록 상태 제거
            GameServiceRegistry.ResetForTests(); // 공통 서비스 레지스트리 초기화
        } // 테스트 후 공통 서비스 정적 상태 초기화 완료

        [Test] // Unity Test Runner 테스트 지정
        public void DefaultSettingsUseCurrentVersionAndValidRanges() // 기본 설정이 현재 버전과 유효 범위를 사용하는지 검증
        { // 기본값 생성과 검증 규칙 확인
            ProjectUserSettings settings = ProjectUserSettings.CreateDefault(); // 현재 환경 기반 기본 설정 생성

            Assert.AreEqual(ProjectUserSettings.CurrentVersion, settings.Version); // 현재 설정 버전 적용 여부 검증
            Assert.GreaterOrEqual(settings.ResolutionWidth, 640); // 최소 가로 해상도 검증
            Assert.GreaterOrEqual(settings.ResolutionHeight, 360); // 최소 세로 해상도 검증
            Assert.That(settings.MasterVolume, Is.InRange(0f, 1f)); // 마스터 음량 유효 범위 검증
            Assert.That(settings.MouseSensitivity, Is.InRange(0.01f, 2f)); // 마우스 감도 유효 범위 검증
        } // 기본 설정이 현재 버전과 유효 범위를 사용하는지 검증 완료

        [Test] // Unity Test Runner 테스트 지정
        public void CloneCreatesIndependentWorkingCopy() // 작업 복사본 수정이 원본에 영향을 주지 않는지 검증
        { // 51일차 설정 UI 취소 기능 기반 확인
            ProjectUserSettings original = ProjectUserSettings.CreateDefault(); // 원본 설정 생성
            original.MasterVolume = 0.75f; // 원본 마스터 음량 테스트값 적용
            ProjectUserSettings workingCopy = original.Clone(); // 독립 작업 복사본 생성
            workingCopy.MasterVolume = 0.25f; // 작업 복사본 마스터 음량 변경

            Assert.AreEqual(0.75f, original.MasterVolume, 0.0001f); // 원본 음량 유지 여부 검증
            Assert.AreEqual(0.25f, workingCopy.MasterVolume, 0.0001f); // 작업 복사본 변경 여부 검증
            Assert.IsFalse(original.ContentEquals(workingCopy)); // 변경 이후 설정 내용 차이 검증
        } // 작업 복사본 수정이 원본에 영향을 주지 않는지 검증 완료

        [Test] // Unity Test Runner 테스트 지정
        public void ValidateClampsUnsafeValues() // 범위를 벗어난 저장 설정이 안전값으로 보정되는지 검증
        { // 설정 파일 손상과 UI 잘못된 값 방어 확인
            ProjectUserSettings settings = ProjectUserSettings.CreateDefault(); // 검사용 기본 설정 생성
            settings.ResolutionWidth = 1; // 잘못된 가로 해상도 설정
            settings.ResolutionHeight = 1; // 잘못된 세로 해상도 설정
            settings.FullScreenModeValue = int.MaxValue; // 잘못된 화면 모드 설정
            settings.VSyncCount = 99; // 잘못된 수직 동기화 값 설정
            settings.TargetFrameRate = 9999; // 잘못된 목표 프레임 설정
            settings.MasterVolume = -5f; // 잘못된 마스터 음량 설정
            settings.MusicVolume = 5f; // 잘못된 배경 음악 음량 설정
            settings.SfxVolume = 5f; // 잘못된 효과음 음량 설정
            settings.MouseSensitivity = 0f; // 잘못된 마우스 감도 설정
            settings.GamepadLookDegreesPerSecond = 9999f; // 잘못된 게임패드 감도 설정
            settings.MinimumLogLevelValue = 99; // 잘못된 로그 등급 설정

            settings.Validate(); // 잘못된 설정 전체 보정

            Assert.AreEqual(640, settings.ResolutionWidth); // 최소 가로 해상도 보정 검증
            Assert.AreEqual(360, settings.ResolutionHeight); // 최소 세로 해상도 보정 검증
            Assert.AreEqual((int)FullScreenMode.FullScreenWindow, settings.FullScreenModeValue); // 화면 모드 기본값 보정 검증
            Assert.AreEqual(4, settings.VSyncCount); // 수직 동기화 최대값 보정 검증
            Assert.AreEqual(360, settings.TargetFrameRate); // 목표 프레임 최대값 보정 검증
            Assert.AreEqual(0f, settings.MasterVolume, 0.0001f); // 마스터 음량 최소값 보정 검증
            Assert.AreEqual(1f, settings.MusicVolume, 0.0001f); // 배경 음악 음량 최대값 보정 검증
            Assert.AreEqual(1f, settings.SfxVolume, 0.0001f); // 효과음 음량 최대값 보정 검증
            Assert.AreEqual(0.01f, settings.MouseSensitivity, 0.0001f); // 마우스 감도 최소값 보정 검증
            Assert.AreEqual(720f, settings.GamepadLookDegreesPerSecond, 0.0001f); // 게임패드 감도 최대값 보정 검증
            Assert.AreEqual(4, settings.MinimumLogLevelValue); // 로그 등급 최대값 보정 검증
        } // 범위를 벗어난 저장 설정이 안전값으로 보정되는지 검증 완료

        [Test] // Unity Test Runner 테스트 지정
        public void JsonSerializerRoundTripsAllSettings() // 설정 JSON 저장과 불러오기 결과가 동일한지 검증
        { // 파일 입출력과 분리된 직렬화 왕복 확인
            ProjectUserSettings original = ProjectUserSettings.CreateDefault(); // 직렬화 원본 설정 생성
            original.MasterVolume = 0.42f; // 마스터 음량 테스트값 설정
            original.MusicVolume = 0.31f; // 배경 음악 음량 테스트값 설정
            original.SfxVolume = 0.87f; // 효과음 음량 테스트값 설정
            original.MouseSensitivity = 0.45f; // 마우스 감도 테스트값 설정
            original.GamepadLookDegreesPerSecond = 250f; // 게임패드 감도 테스트값 설정
            original.InvertLookY = true; // 수직 시점 반전 테스트값 설정
            original.InputBindingOverridesJson = "{\"test\":true}"; // 입력 재지정 테스트 JSON 설정
            string json = SettingsJsonSerializer.Serialize(original, false); // 원본 설정 JSON 직렬화

            bool succeeded = SettingsJsonSerializer.TryDeserialize(json, out ProjectUserSettings loaded, out string failureReason); // JSON 설정 역직렬화 시도

            Assert.IsTrue(succeeded, failureReason); // JSON 역직렬화 성공 검증
            Assert.IsNotNull(loaded); // 역직렬화 결과 존재 검증
            Assert.IsTrue(original.ContentEquals(loaded)); // 저장 전후 전체 설정 내용 동일 검증
        } // 설정 JSON 저장과 불러오기 결과가 동일한지 검증 완료

        [Test] // Unity Test Runner 테스트 지정
        public void JsonSerializerRejectsUnsupportedVersion() // 지원하지 않는 설정 버전을 거부하는지 검증
        { // 버전 불일치 설정 파일 복구 기반 확인
            string json = "{\"Version\":999}"; // 지원하지 않는 버전 설정 JSON 생성

            bool succeeded = SettingsJsonSerializer.TryDeserialize(json, out ProjectUserSettings loaded, out string failureReason); // 버전 불일치 JSON 변환 시도

            Assert.IsFalse(succeeded); // 지원하지 않는 버전 거부 검증
            Assert.IsNull(loaded); // 실패 시 설정 결과 없음 검증
            StringAssert.Contains("지원하지 않습니다", failureReason); // 버전 실패 원인 안내 검증
        } // 지원하지 않는 설정 버전을 거부하는지 검증 완료

        [Test] // Unity Test Runner 테스트 지정
        public void SettingsManagerIsNotReadyWithoutRegisteredService() // 서비스 초기화 전 관리자 안전 상태를 검증
        { // Bootstrap 이전 접근 방어 확인
            Assert.IsFalse(SettingsManager.IsReady); // 설정 서비스 없는 상태의 준비 실패 검증
            Assert.IsFalse(SettingsManager.TryCreateWorkingCopy(out ProjectUserSettings settings)); // 작업 복사본 안전 생성 실패 검증
            Assert.IsNull(settings); // 실패 시 작업 복사본 없음 검증
        } // 서비스 초기화 전 관리자 안전 상태를 검증 완료
    } // 디스크 파일을 변경하지 않는 설정 기반 단위 테스트 정의 완료
} // 프로젝트 EditMode 테스트 네임스페이스 정의 완료
