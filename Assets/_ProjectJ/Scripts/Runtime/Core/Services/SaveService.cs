using System; // 저장 경로 검증 예외 기능 참조
using System.IO; // 저장 폴더 경로와 생성 기능 참조
using UnityEngine; // Unity 영구 저장 경로 기능 참조

namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스 선언
{
    public sealed class SaveService : GameServiceBase // 게임 저장 경로 준비 서비스 선언
    {
        public override string ServiceName => "Save"; // 저장 서비스 이름 반환
        public override int InitializationOrder => 200; // 저장 서비스 초기화 순서 반환
        public string SaveDirectoryPath { get; private set; } // 게임 저장 파일 폴더 경로 저장

        protected override void OnInitialize() // 저장 서비스의 기본 폴더 준비
        {
            string persistentDataPath = Application.persistentDataPath; // 플랫폼별 영구 저장 기본 경로 조회

            if (string.IsNullOrWhiteSpace(persistentDataPath)) // 영구 저장 경로가 비어 있는지 확인
            {
                throw new InvalidOperationException("영구 저장 경로를 확인할 수 없습니다."); // 저장 경로 준비 실패 예외 발생
            }

            SaveDirectoryPath = Path.Combine(persistentDataPath, "Saves"); // Project J 저장 파일 전용 폴더 경로 생성
            Directory.CreateDirectory(SaveDirectoryPath); // 저장 폴더가 없으면 새로 생성
        }
    }
}
