using System; // 저장 예외 기능 참조
using System.IO; // 파일과 폴더 입출력 기능 참조
using System.Text; // UTF-8 인코딩 기능 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEngine; // Unity 영구 저장 경로 기능 참조

namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스
{ // 네임스페이스 범위
    public sealed class SaveService : GameServiceBase // 게임 저장 경로와 텍스트 파일 관리 서비스
    { // 클래스 범위
        private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false); // BOM 없는 UTF-8 인코딩

        public override string ServiceName => "Save"; // 저장 서비스 이름
        public override int InitializationOrder => 100; // 저장 서비스 초기화 순서
        public string SaveDirectoryPath { get; private set; } // 게임 진행 저장 폴더 경로
        public string SettingsDirectoryPath { get; private set; } // 사용자 설정 저장 폴더 경로

        protected override void OnInitialize() // 영구 저장 폴더 준비
        { // 메서드 범위
            string persistentDataPath = Application.persistentDataPath; // 플랫폼별 영구 저장 기본 경로 조회

            if (string.IsNullOrWhiteSpace(persistentDataPath)) // 영구 저장 경로 누락 확인
            { // 조건 범위
                throw new InvalidOperationException("영구 저장 경로를 확인할 수 없습니다."); // 저장 준비 실패 예외 발생
            } // 조건 범위

            SaveDirectoryPath = Path.Combine(persistentDataPath, "Saves"); // 게임 진행 저장 폴더 경로 생성
            SettingsDirectoryPath = Path.Combine(persistentDataPath, "Settings"); // 사용자 설정 저장 폴더 경로 생성
            Directory.CreateDirectory(SaveDirectoryPath); // 게임 진행 저장 폴더 생성
            Directory.CreateDirectory(SettingsDirectoryPath); // 사용자 설정 저장 폴더 생성
        } // 메서드 범위

        public string GetSettingsFilePath(string fileName) // 설정 파일의 전체 경로 생성
        { // 메서드 범위
            string safeFileName = Path.GetFileName(fileName); // 폴더 이동 문자가 제거된 파일명 생성

            if (string.IsNullOrWhiteSpace(safeFileName)) // 유효한 파일명 누락 확인
            { // 조건 범위
                throw new ArgumentException("설정 파일 이름이 비어 있습니다.", nameof(fileName)); // 잘못된 파일명 예외 발생
            } // 조건 범위

            return Path.Combine(SettingsDirectoryPath, safeFileName); // 설정 파일 전체 경로 반환
        } // 메서드 범위

        public bool TryLoadSettingsText(string fileName, out string content) // 설정 텍스트 파일 읽기 시도
        { // 메서드 범위
            string filePath = GetSettingsFilePath(fileName); // 설정 파일 전체 경로 조회
            content = string.Empty; // 읽기 실패 기본값 준비

            if (!File.Exists(filePath)) // 설정 파일 존재 여부 확인
            { // 조건 범위
                return false; // 최초 실행 상태 반환
            } // 조건 범위

            try // 설정 파일 읽기 예외 감시
            { // 예외 감시 범위
                content = File.ReadAllText(filePath, Utf8WithoutBom); // UTF-8 설정 텍스트 읽기
                return true; // 설정 파일 읽기 성공 반환
            } // 예외 감시 범위
            catch (Exception exception) // 파일 읽기 실패 처리
            { // 예외 처리 범위
                ProjectLog.Warning(ProjectLogCategory.Core, $"설정 파일을 읽지 못했습니다. {exception.Message}", "SETTINGS_READ_FAILED"); // 복구 가능한 읽기 경고 출력
                return false; // 설정 파일 읽기 실패 반환
            } // 예외 처리 범위
        } // 메서드 범위

        public bool SaveSettingsText(string fileName, string content) // 임시 파일을 이용한 설정 텍스트 저장
        { // 메서드 범위
            string filePath = GetSettingsFilePath(fileName); // 설정 파일 전체 경로 조회
            string temporaryPath = filePath + ".tmp"; // 임시 저장 파일 경로 생성

            try // 설정 파일 저장 예외 감시
            { // 예외 감시 범위
                Directory.CreateDirectory(SettingsDirectoryPath); // 설정 저장 폴더 존재 보장
                File.WriteAllText(temporaryPath, content ?? string.Empty, Utf8WithoutBom); // 임시 파일에 전체 내용 저장
                File.Copy(temporaryPath, filePath, true); // 임시 파일을 실제 설정 파일로 덮어쓰기
                File.Delete(temporaryPath); // 저장 완료 임시 파일 제거
                return true; // 설정 파일 저장 성공 반환
            } // 예외 감시 범위
            catch (Exception exception) // 파일 저장 실패 처리
            { // 예외 처리 범위
                ProjectLog.Error(ProjectLogCategory.Core, $"설정 파일을 저장하지 못했습니다. {exception.Message}", "SETTINGS_WRITE_FAILED"); // 설정 저장 오류 출력
                return false; // 설정 파일 저장 실패 반환
            } // 예외 처리 범위
        } // 메서드 범위
    } // 클래스 범위
} // 네임스페이스 범위
