using System; // 데이터 검증 실패 예외 기능 참조
using System.Collections.Generic; // 런타임 데이터 조회 사전 기능 참조
using ProjectJ.Audio; // 오디오 서비스 형식 참조
using ProjectJ.Core.Services; // 공통 서비스 등록과 상태 형식 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEngine; // Resources 데이터 카탈로그 로드 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    public sealed class DataValidationService : GameServiceBase // 런타임 데이터 로드와 검증 서비스 선언
    {
        public const string CatalogResourcePath = "ProjectDataCatalog"; // Resources 카탈로그 로드 경로 선언
        private readonly Dictionary<string, ProjectDataAsset> assetsById = new Dictionary<string, ProjectDataAsset>(StringComparer.OrdinalIgnoreCase); // ID별 런타임 데이터 조회 사전 저장

        public override string ServiceName => "DataValidation"; // 데이터 검증 서비스 이름 반환
        public override int InitializationOrder => 400; // 데이터 검증 서비스 초기화 순서 반환
        public bool LastValidationSucceeded { get; private set; } // 최근 전체 데이터 검증 성공 여부 저장
        public ProjectDataCatalog Catalog { get; private set; } // 로드된 런타임 데이터 카탈로그 저장
        public ProjectDataValidationReport LastReport { get; private set; } // 최근 데이터 검증 결과 저장
        public int AssetCount => assetsById.Count; // 조회 가능한 런타임 데이터 수 반환

        public bool TryGet<T>(string dataId, out T asset) where T : ProjectDataAsset // ID로 지정 형식의 런타임 데이터 조회
        {
            if (!string.IsNullOrWhiteSpace(dataId) // 데이터 ID 존재 여부 확인
                && assetsById.TryGetValue(dataId, out ProjectDataAsset foundAsset) // 등록된 데이터 에셋 조회
                && foundAsset is T typedAsset) // 요청한 데이터 형식 일치 여부 확인
            {
                asset = typedAsset; // 형식이 일치하는 데이터 에셋 반환값 저장
                return true; // 런타임 데이터 조회 성공 반환
            }

            asset = null; // 런타임 데이터 조회 실패 결과 저장
            return false; // 런타임 데이터 조회 실패 반환
        }

        protected override void OnInitialize() // 필수 서비스와 런타임 데이터 카탈로그 초기화
        {
            LastValidationSucceeded = false; // 초기화 시작 전 검증 실패 상태 적용
            assetsById.Clear(); // 이전 런타임 데이터 조회 사전 제거
            ValidateInitializedService<SettingsService>(); // 설정 서비스 초기화 완료 여부 검증
            ValidateInitializedService<SaveService>(); // 저장 서비스 초기화 완료 여부 검증
            ValidateInitializedService<AudioService>(); // 오디오 서비스 초기화 완료 여부 검증
            Catalog = Resources.Load<ProjectDataCatalog>(CatalogResourcePath); // Resources 런타임 데이터 카탈로그 로드

            if (Catalog == null) // 런타임 데이터 카탈로그 누락 여부 확인
            {
                throw new InvalidOperationException("ProjectDataCatalog이 없습니다. Project J > Day 17 > Rebuild Runtime Data Catalog를 실행하세요."); // 카탈로그 생성 안내 예외 발생
            }

            LastReport = ProjectDataValidator.ValidateCatalog(Catalog.Assets); // 카탈로그 전체 데이터와 필수 분류 검증
            WriteValidationIssues(LastReport); // 발견된 모든 데이터 문제 로그 출력

            if (LastReport.HasErrors) // 치명 데이터 오류 존재 여부 확인
            {
                ProjectDataValidationIssue firstIssue = LastReport.Issues[0]; // 첫 번째 치명 데이터 문제 조회
                throw new InvalidOperationException($"데이터 오류 {LastReport.ErrorCount}개가 발견되었습니다. 첫 오류: {firstIssue.Code} - {firstIssue.Message}"); // MainMenu 진입 차단용 예외 발생
            }

            RegisterAssets(Catalog.Assets); // 검증을 통과한 데이터 에셋 조회 사전 등록
            LastValidationSucceeded = true; // 전체 데이터 검증 성공 상태 저장
            ProjectLog.Info(ProjectLogCategory.Data, $"런타임 데이터 {AssetCount}개를 불러오고 검증했습니다.", "DATA_CATALOG_READY"); // 데이터 카탈로그 준비 완료 로그 출력
        }

        private void RegisterAssets(IReadOnlyList<ProjectDataAsset> assets) // 검증된 데이터 에셋 ID 조회 사전 등록
        {
            for (int index = 0; index < assets.Count; index++) // 전체 데이터 에셋 순회
            {
                ProjectDataAsset asset = assets[index]; // 현재 데이터 에셋 조회
                assetsById.Add(asset.DataId, asset); // 데이터 ID와 에셋 조회 사전 등록
            }
        }

        private static void WriteValidationIssues(ProjectDataValidationReport report) // 데이터 검증 문제 등급별 로그 출력
        {
            foreach (ProjectDataValidationIssue issue in report.Issues) // 발견된 모든 데이터 문제 순회
            {
                if (issue.Severity == DataValidationSeverity.Error) // 현재 문제가 오류 수준인지 확인
                {
                    ProjectLog.Error(ProjectLogCategory.Data, issue.Message, issue.Code, issue.Asset); // 치명 데이터 오류 로그 출력
                    continue; // 다음 데이터 문제 처리
                }

                ProjectLog.Warning(ProjectLogCategory.Data, issue.Message, issue.Code, issue.Asset); // 복구 가능한 데이터 경고 로그 출력
            }
        }

        private static void ValidateInitializedService<T>() where T : class, IGameService // 지정한 필수 서비스의 초기화 완료 여부 검증
        {
            T service = GameServiceRegistry.Get<T>(); // 등록된 필수 서비스 조회

            if (service.State != GameServiceState.Initialized) // 필수 서비스 초기화 완료 여부 확인
            {
                throw new InvalidOperationException($"{service.ServiceName} 서비스가 초기화되지 않았습니다."); // 필수 서비스 초기화 누락 예외 발생
            }
        }
    }
}
