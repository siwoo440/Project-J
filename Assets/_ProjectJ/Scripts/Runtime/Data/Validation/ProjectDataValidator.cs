using System; // 문자열 비교 기능 참조
using System.Collections.Generic; // 데이터 목록과 사전 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    public static class ProjectDataValidator // 프로젝트 공통 데이터 검증 담당 형식 선언
    {
        public const string MissingAssetCode = "DATA_NULL"; // null 데이터 에셋 오류 코드 선언
        public const string MissingIdCode = "DATA_ID_MISSING"; // 데이터 ID 누락 오류 코드 선언
        public const string InvalidIdCode = "DATA_ID_INVALID"; // 데이터 ID 형식 오류 코드 선언
        public const string DuplicateIdCode = "DATA_ID_DUPLICATE"; // 데이터 ID 중복 오류 코드 선언
        public const string MissingNameCode = "DATA_NAME_MISSING"; // 데이터 표시 이름 누락 오류 코드 선언
        public const string InvalidVersionCode = "DATA_VERSION_INVALID"; // 데이터 버전 오류 코드 선언

        public static ProjectDataValidationReport Validate(IEnumerable<ProjectDataAsset> assets) // 전달된 모든 데이터 에셋 검증
        {
            ProjectDataValidationReport report = new ProjectDataValidationReport(); // 전체 검증 결과 생성
            Dictionary<string, List<ProjectDataAsset>> assetsById = new Dictionary<string, List<ProjectDataAsset>>(StringComparer.OrdinalIgnoreCase); // ID별 데이터 에셋 목록 생성

            if (assets == null) // 전달된 데이터 에셋 모음의 null 여부 확인
            {
                report.AddError(null, MissingAssetCode, "검증할 데이터 에셋 모음이 null입니다."); // 데이터 모음 누락 오류 추가
                return report; // 누락 오류가 포함된 검증 결과 반환
            }

            foreach (ProjectDataAsset asset in assets) // 모든 데이터 에셋 순회
            {
                ValidateAsset(asset, report, assetsById); // 현재 데이터 에셋의 공통 값과 분류별 설정 검사
            }

            ValidateDuplicateIds(assetsById, report); // 수집된 데이터 ID 중복 여부 검사
            return report; // 전체 데이터 검증 결과 반환
        }

        private static void ValidateAsset(ProjectDataAsset asset, ProjectDataValidationReport report, Dictionary<string, List<ProjectDataAsset>> assetsById) // 단일 데이터 에셋 공통 값과 분류별 설정 검사
        {
            if (asset == null) // 현재 데이터 에셋의 null 여부 확인
            {
                report.AddError(null, MissingAssetCode, "데이터 목록에 null 에셋이 포함되어 있습니다."); // null 데이터 에셋 오류 추가
                return; // 현재 데이터 에셋 검사 중단
            }

            if (string.IsNullOrWhiteSpace(asset.DataId)) // 데이터 ID 누락 여부 확인
            {
                report.AddError(asset, MissingIdCode, $"{asset.Category} 데이터의 ID가 비어 있습니다."); // 데이터 ID 누락 오류 추가
            }
            else // 데이터 ID가 존재하는 경우 처리
            {
                if (!ProjectDataIdRules.IsValid(asset.DataId, asset.Category, out string reason)) // 데이터 ID 형식과 분류 일치 여부 확인
                {
                    report.AddError(asset, InvalidIdCode, reason); // 데이터 ID 형식 오류 추가
                }

                if (!assetsById.TryGetValue(asset.DataId, out List<ProjectDataAsset> matchingAssets)) // 현재 ID의 에셋 목록 존재 여부 확인
                {
                    matchingAssets = new List<ProjectDataAsset>(); // 현재 ID용 새 에셋 목록 생성
                    assetsById.Add(asset.DataId, matchingAssets); // ID와 에셋 목록 등록
                }

                matchingAssets.Add(asset); // 현재 ID 목록에 데이터 에셋 추가
            }

            if (string.IsNullOrWhiteSpace(asset.DisplayName)) // 데이터 표시 이름 누락 여부 확인
            {
                report.AddError(asset, MissingNameCode, $"{asset.Category} 데이터의 표시 이름이 비어 있습니다."); // 데이터 표시 이름 누락 오류 추가
            }

            if (!asset.Version.IsValid) // 데이터 버전 유효 여부 확인
            {
                report.AddError(asset, InvalidVersionCode, $"데이터 버전은 1.0.0 이상이어야 합니다. 현재 값: {asset.Version}"); // 데이터 버전 오류 추가
            }

            ValidateCategorySpecificSettings(asset, report); // 현재 데이터 분류의 세부 설정 검사
        }

        private static void ValidateCategorySpecificSettings(ProjectDataAsset asset, ProjectDataValidationReport report) // 데이터 분류별 세부 설정 검사
        {
            if (asset is PlayerDataDefinition playerData) // 현재 에셋이 플레이어 데이터인지 확인
            {
                PlayerSettingsValidationRules.Validate(playerData, report); // 플레이어 이동 관련 모든 설정 검사
            }
        }

        private static void ValidateDuplicateIds(Dictionary<string, List<ProjectDataAsset>> assetsById, ProjectDataValidationReport report) // 수집된 데이터 ID 중복 여부 검사
        {
            foreach (KeyValuePair<string, List<ProjectDataAsset>> pair in assetsById) // 모든 ID별 에셋 목록 순회
            {
                if (pair.Value.Count <= 1) // 현재 ID가 한 에셋에서만 사용되는지 확인
                {
                    continue; // 중복이 없으면 다음 ID 검사로 이동
                }

                foreach (ProjectDataAsset asset in pair.Value) // 중복 ID를 사용하는 모든 에셋 순회
                {
                    report.AddError(asset, DuplicateIdCode, $"데이터 ID {pair.Key}가 {pair.Value.Count}개 에셋에서 중복 사용되고 있습니다."); // 현재 에셋에 ID 중복 오류 추가
                }
            }
        }
    }
}
