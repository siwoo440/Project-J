using NUnit.Framework; // Unity EditMode 테스트 기능 참조
using ProjectJ.Core.Services; // 설정 데이터 기능 참조
using UnityEngine; // Unity 화면 모드 형식 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 51일차 설정 UI 작업 복사본 회귀 테스트 정의
    public sealed class SettingsMenuWorkingCopyTests // 설정 메뉴 적용 전 데이터 독립성 테스트 선언
    { // 50일차 작업 복사본 기반 UI 요구사항 검증
        [Test] // Unity Test Runner 테스트 지정
        public void WorkingCopyCanChangeWithoutMutatingOriginalSettings() // UI 작업값 변경이 원본 설정을 바꾸지 않는지 검증
        { // 취소 버튼 기반 데이터 독립성 확인
            ProjectUserSettings original = ProjectUserSettings.CreateDefault(); // 원본 설정 생성
            original.MasterVolume = 0.8f; // 원본 마스터 음량 테스트값 적용
            original.FullScreenModeValue = (int)FullScreenMode.FullScreenWindow; // 원본 화면 모드 테스트값 적용
            ProjectUserSettings workingCopy = original.Clone(); // UI 작업 복사본 생성
            workingCopy.MasterVolume = 0.2f; // 작업 복사본 마스터 음량 변경
            workingCopy.FullScreenModeValue = (int)FullScreenMode.Windowed; // 작업 복사본 화면 모드 변경

            Assert.AreEqual(0.8f, original.MasterVolume, 0.0001f); // 원본 마스터 음량 유지 검증
            Assert.AreEqual((int)FullScreenMode.FullScreenWindow, original.FullScreenModeValue); // 원본 화면 모드 유지 검증
            Assert.IsFalse(original.ContentEquals(workingCopy)); // 변경된 작업 복사본과 원본 차이 검증
        } // UI 작업값 변경이 원본 설정을 바꾸지 않는지 검증 완료

        [Test] // Unity Test Runner 테스트 지정
        public void DefaultWorkingCopyRemainsValidForSettingsMenu() // 기본값 미리보기가 유효 설정 범위를 유지하는지 검증
        { // 기본값 버튼 UI 기반 데이터 확인
            ProjectUserSettings defaults = ProjectUserSettings.CreateDefault(); // 현재 환경 기본 설정 생성
            defaults.Validate(); // 기본 설정 안전 범위 검증

            Assert.AreEqual(ProjectUserSettings.CurrentVersion, defaults.Version); // 설정 버전 검증
            Assert.GreaterOrEqual(defaults.ResolutionWidth, 640); // 최소 가로 해상도 검증
            Assert.GreaterOrEqual(defaults.ResolutionHeight, 360); // 최소 세로 해상도 검증
            Assert.That(defaults.MasterVolume, Is.InRange(0f, 1f)); // 마스터 음량 범위 검증
        } // 기본값 미리보기가 유효 설정 범위를 유지하는지 검증 완료
    } // 50일차 작업 복사본 기반 UI 요구사항 검증 완료
} // 51일차 설정 UI 작업 복사본 회귀 테스트 정의 완료
