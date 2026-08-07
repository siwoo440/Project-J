using System; // 데이터 분류와 예외 기능 참조
using System.Collections.Generic; // CSV 행과 오류 목록 기능 참조
using System.IO; // CSV 파일 읽기와 파일명 문자 기능 참조
using System.Text; // CSV 값 조립 기능 참조
using ProjectJ.Data; // 프로젝트 데이터 에셋과 ID 규칙 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEditor; // Unity 에셋 생성과 메뉴 기능 참조
using UnityEngine; // Unity ScriptableObject와 변경 표시 기능 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal static class ProjectDataCsvImporter // Google Sheets 내보내기 CSV 가져오기 도구 선언
    {
        private const string ImportDirectoryPath = "Assets/_ProjectJ/Data/Imports"; // CSV 가져오기 폴더 경로 선언
        private const string ImportFilePath = ImportDirectoryPath + "/ProjectData.csv"; // 통합 CSV 파일 경로 선언
        private const int ExpectedColumnCount = 4; // CSV 필수 열 개수 선언
        private static readonly string[] ExpectedHeaders = { "Category", "DataId", "DisplayName", "Version" }; // CSV 필수 머리글 순서 선언

        [MenuItem(ProjectJEditorMenuPaths.DataCsv + "/데이터 CSV 템플릿 생성 (Day 17일차)")] // 17일차 CSV 템플릿 생성 메뉴 등록
        private static void CreateTemplate() // 기본 데이터가 포함된 CSV 템플릿 생성
        {
            EnsureImportDirectory(); // CSV 가져오기 폴더 준비

            if (File.Exists(ImportFilePath)) // 기존 CSV 파일 존재 여부 확인
            {
                bool shouldReplace = EditorUtility.DisplayDialog("Project J 데이터 CSV", "기존 ProjectData.csv 파일을 기본 템플릿으로 덮어씁니다.", "덮어쓰기", "취소"); // 기존 CSV 교체 여부 확인

                if (!shouldReplace) // CSV 교체 취소 여부 확인
                {
                    return; // 기존 CSV 유지 후 메뉴 처리 종료
                }
            }

            StringBuilder builder = new StringBuilder(); // CSV 템플릿 문자열 생성기 준비
            builder.AppendLine("Category,DataId,DisplayName,Version"); // CSV 머리글 추가
            builder.AppendLine("Player,PLY-001,Default Player,1.0.0"); // 기본 플레이어 행 추가
            builder.AppendLine("Map,MAP-001,Default Map,1.0.0"); // 기본 맵 행 추가
            builder.AppendLine("Obstacle,OBS-001,Default Obstacle,1.0.0"); // 기본 장애물 행 추가
            builder.AppendLine("Item,ITM-001,Spring Shoes,1.0.0"); // 기본 아이템 행 추가
            builder.AppendLine("Cosmetic,COS-001,Default Costume,1.0.0"); // 기본 꾸미기 행 추가
            builder.AppendLine("Audio,AUD-001,Default Audio,1.0.0"); // 기본 오디오 행 추가
            File.WriteAllText(ImportFilePath, builder.ToString(), new UTF8Encoding(true)); // UTF-8 BOM 형식 CSV 템플릿 저장
            AssetDatabase.Refresh(); // 새 CSV 파일 Unity 프로젝트에 반영
            ProjectLog.Info(ProjectLogCategory.Data, $"CSV 템플릿을 생성했습니다: {ImportFilePath}", "DATA_CSV_TEMPLATE_CREATED"); // CSV 템플릿 생성 완료 로그 출력
        }

        [MenuItem(ProjectJEditorMenuPaths.DataCsv + "/프로젝트 데이터 CSV 가져오기 (Day 17일차)")] // 17일차 CSV 데이터 가져오기 메뉴 등록
        private static void ImportCsv() // 통합 CSV를 ScriptableObject 데이터 에셋으로 가져오기
        {
            if (!File.Exists(ImportFilePath)) // 통합 CSV 파일 존재 여부 확인
            {
                ProjectLog.Error(ProjectLogCategory.Data, $"CSV 파일이 없습니다: {ImportFilePath}", "DATA_CSV_NOT_FOUND"); // CSV 파일 누락 오류 출력
                return; // CSV 가져오기 중단
            }

            string[] lines = File.ReadAllLines(ImportFilePath, Encoding.UTF8); // 통합 CSV 전체 행 읽기
            List<string> errors = new List<string>(); // CSV 사전 검증 오류 목록 생성
            List<ImportRow> rows = ParseRows(lines, errors); // CSV 행 해석과 사전 검증 실행

            if (errors.Count > 0) // CSV 사전 검증 오류 존재 여부 확인
            {
                WriteImportErrors(errors); // 발견된 CSV 오류 전체 출력
                return; // 잘못된 CSV의 부분 적용 차단
            }

            int createdCount = 0; // 새로 생성한 데이터 에셋 수 저장
            int updatedCount = 0; // 갱신한 기존 데이터 에셋 수 저장
            ApplyRows(rows, ref createdCount, ref updatedCount); // 검증된 CSV 행 전체 적용
            AssetDatabase.SaveAssets(); // 생성과 갱신된 데이터 에셋 저장
            AssetDatabase.Refresh(); // Unity 에셋 데이터베이스 새로고침
            ProjectDataValidationReport report = ProjectDataCatalogBuilder.RebuildAndValidate(true); // 런타임 카탈로그 갱신과 전체 데이터 검증 실행

            if (report.HasErrors) // 적용 후 전체 데이터 오류 존재 여부 확인
            {
                ProjectLog.Error(ProjectLogCategory.Data, $"CSV 적용 후 데이터 오류 {report.ErrorCount}개가 남았습니다.", "DATA_CSV_IMPORT_INVALID"); // 적용 후 데이터 오류 요약 출력
                return; // 성공 로그 없이 가져오기 종료
            }

            ProjectLog.Info(ProjectLogCategory.Data, $"CSV 가져오기를 완료했습니다. 생성 {createdCount}개, 갱신 {updatedCount}개.", "DATA_CSV_IMPORTED"); // CSV 가져오기 성공 로그 출력
        }

        private static List<ImportRow> ParseRows(IReadOnlyList<string> lines, List<string> errors) // CSV 전체 행 해석과 변경 전 검증
        {
            List<ImportRow> rows = new List<ImportRow>(); // 해석된 CSV 데이터 행 목록 생성
            HashSet<string> seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // CSV 내부 중복 ID 검사 집합 생성

            if (lines.Count == 0) // CSV 빈 파일 여부 확인
            {
                errors.Add("1행: CSV 파일이 비어 있습니다."); // 빈 CSV 오류 추가
                return rows; // 빈 데이터 행 목록 반환
            }

            List<string> headers = ParseCsvLine(lines[0], 1, errors); // 첫 행 CSV 머리글 해석
            ValidateHeaders(headers, errors); // CSV 머리글 이름과 순서 검증

            for (int lineIndex = 1; lineIndex < lines.Count; lineIndex++) // 머리글 다음 CSV 행 전체 순회
            {
                string line = lines[lineIndex]; // 현재 CSV 원본 행 조회

                if (string.IsNullOrWhiteSpace(line)) // 현재 CSV 행이 빈 줄인지 확인
                {
                    continue; // 빈 CSV 행 건너뛰기
                }

                int displayLineNumber = lineIndex + 1; // 사용자 안내용 1부터 시작하는 행 번호 계산
                List<string> columns = ParseCsvLine(line, displayLineNumber, errors); // 현재 CSV 행 열 값 해석

                if (columns.Count != ExpectedColumnCount) // 현재 행 필수 열 개수 일치 여부 확인
                {
                    errors.Add($"{displayLineNumber}행: 열은 {ExpectedColumnCount}개여야 하지만 {columns.Count}개입니다."); // CSV 열 개수 오류 추가
                    continue; // 현재 행의 세부 검증 생략
                }

                if (!TryCreateRow(columns, displayLineNumber, seenIds, errors, out ImportRow row)) // 현재 CSV 행 값 유효 여부 확인
                {
                    continue; // 잘못된 CSV 행 적용 목록에서 제외
                }

                rows.Add(row); // 검증된 CSV 데이터 행 목록 추가
            }

            if (rows.Count == 0 && errors.Count == 0) // 유효 데이터 행이 하나도 없는지 확인
            {
                errors.Add("2행 이후: 가져올 데이터 행이 없습니다."); // 빈 데이터 목록 오류 추가
            }

            return rows; // 해석과 검증이 완료된 CSV 행 목록 반환
        }

        private static List<string> ParseCsvLine(string line, int lineNumber, List<string> errors) // 따옴표와 쉼표를 고려한 단일 CSV 행 해석
        {
            List<string> values = new List<string>(); // 해석된 열 값 목록 생성
            StringBuilder currentValue = new StringBuilder(); // 현재 열 값 문자열 생성기 준비
            bool insideQuotes = false; // 현재 따옴표 내부 여부 저장

            for (int index = 0; index < line.Length; index++) // 현재 CSV 행의 모든 문자 순회
            {
                char currentCharacter = line[index]; // 현재 CSV 문자 조회

                if (currentCharacter == '"') // 현재 문자의 따옴표 여부 확인
                {
                    bool escapedQuote = insideQuotes && index + 1 < line.Length && line[index + 1] == '"'; // 따옴표 내부 이중 따옴표 여부 확인

                    if (escapedQuote) // 이스케이프된 따옴표 여부 확인
                    {
                        currentValue.Append('"'); // 실제 따옴표 문자 값 추가
                        index++; // 두 번째 따옴표 문자 건너뛰기
                    }
                    else // CSV 값 경계 따옴표 처리
                    {
                        insideQuotes = !insideQuotes; // 따옴표 내부 상태 전환
                    }

                    continue; // 현재 따옴표 문자 처리 완료
                }

                if (currentCharacter == ',' && !insideQuotes) // 따옴표 밖 열 구분자 여부 확인
                {
                    values.Add(currentValue.ToString().Trim()); // 완성된 현재 열 값 목록 추가
                    currentValue.Clear(); // 다음 열 값 문자열 초기화
                    continue; // 현재 쉼표 문자 처리 완료
                }

                currentValue.Append(currentCharacter); // 현재 문자를 열 값에 추가
            }

            if (insideQuotes) // 닫히지 않은 따옴표 존재 여부 확인
            {
                errors.Add($"{lineNumber}행: 닫히지 않은 큰따옴표가 있습니다."); // CSV 따옴표 오류 추가
            }

            values.Add(currentValue.ToString().Trim()); // 마지막 열 값 목록 추가
            return values; // 해석된 CSV 열 값 목록 반환
        }

        private static void ValidateHeaders(IReadOnlyList<string> headers, List<string> errors) // CSV 머리글 개수와 순서 검증
        {
            if (headers.Count != ExpectedColumnCount) // CSV 머리글 열 개수 일치 여부 확인
            {
                errors.Add($"1행: 머리글은 {ExpectedColumnCount}개여야 합니다."); // CSV 머리글 개수 오류 추가
                return; // 머리글 이름 비교 생략
            }

            for (int index = 0; index < ExpectedHeaders.Length; index++) // 필수 CSV 머리글 전체 순회
            {
                string actualHeader = headers[index].TrimStart('\uFEFF'); // UTF-8 BOM이 제거된 실제 머리글 조회

                if (!string.Equals(actualHeader, ExpectedHeaders[index], StringComparison.OrdinalIgnoreCase)) // 현재 머리글 이름과 순서 일치 여부 확인
                {
                    errors.Add($"1행 {index + 1}열: 머리글은 {ExpectedHeaders[index]}이어야 합니다."); // CSV 머리글 이름 오류 추가
                }
            }
        }

        private static bool TryCreateRow(IReadOnlyList<string> columns, int lineNumber, HashSet<string> seenIds, List<string> errors, out ImportRow row) // CSV 열 값을 검증된 가져오기 행으로 변환
        {
            row = null; // 변환 실패 기본 결과 저장
            bool isValid = true; // 현재 CSV 행 유효 상태 초기화

            if (!Enum.TryParse(columns[0], true, out ProjectDataCategory category)) // 데이터 분류 문자열 변환 성공 여부 확인
            {
                errors.Add($"{lineNumber}행 Category: {columns[0]}은 지원하지 않는 분류입니다."); // 데이터 분류 오류 추가
                return false; // 데이터 분류 없는 행 변환 실패 반환
            }

            string dataId = columns[1]; // CSV 데이터 ID 값 조회
            string displayName = columns[2]; // CSV 표시 이름 값 조회
            string versionText = columns[3]; // CSV 버전 문자열 조회

            if (!ProjectDataIdRules.IsValid(dataId, category, out string idReason)) // 데이터 ID 형식과 분류 일치 여부 확인
            {
                errors.Add($"{lineNumber}행 DataId: {idReason}"); // 데이터 ID 검증 오류 추가
                isValid = false; // 현재 CSV 행 오류 상태 적용
            }

            if (!seenIds.Add(dataId)) // CSV 내부 데이터 ID 중복 여부 확인
            {
                errors.Add($"{lineNumber}행 DataId: {dataId}가 CSV 안에서 중복되었습니다."); // CSV 내부 중복 ID 오류 추가
                isValid = false; // 현재 CSV 행 오류 상태 적용
            }

            if (string.IsNullOrWhiteSpace(displayName)) // 표시 이름 누락 여부 확인
            {
                errors.Add($"{lineNumber}행 DisplayName: 표시 이름이 비어 있습니다."); // 표시 이름 누락 오류 추가
                isValid = false; // 현재 CSV 행 오류 상태 적용
            }

            if (!TryParseVersion(versionText, out ProjectDataVersion version)) // 데이터 버전 문자열 변환 성공 여부 확인
            {
                errors.Add($"{lineNumber}행 Version: {versionText}는 1.0.0 형식이 아닙니다."); // 데이터 버전 형식 오류 추가
                isValid = false; // 현재 CSV 행 오류 상태 적용
            }

            if (!isValid) // 현재 CSV 행의 최종 오류 여부 확인
            {
                return false; // 잘못된 CSV 행 변환 실패 반환
            }

            row = new ImportRow(category, dataId, displayName, version); // 검증된 가져오기 행 생성
            return true; // CSV 행 변환 성공 반환
        }

        private static bool TryParseVersion(string versionText, out ProjectDataVersion version) // 점으로 구분된 데이터 버전 문자열 변환
        {
            version = default; // 데이터 버전 변환 실패 기본값 저장
            string[] parts = versionText.Split('.'); // 데이터 버전 문자열 세 부분 분리

            if (parts.Length != 3) // 데이터 버전 부분 개수 일치 여부 확인
            {
                return false; // 데이터 버전 변환 실패 반환
            }

            if (!int.TryParse(parts[0], out int major) // 주 버전 정수 변환 성공 여부 확인
                || !int.TryParse(parts[1], out int minor) // 부 버전 정수 변환 성공 여부 확인
                || !int.TryParse(parts[2], out int patch)) // 패치 버전 정수 변환 성공 여부 확인
            {
                return false; // 데이터 버전 숫자 변환 실패 반환
            }

            version = new ProjectDataVersion(major, minor, patch); // 변환된 세 숫자로 데이터 버전 생성
            return version.IsValid; // 데이터 버전 허용 범위 결과 반환
        }

        private static void ApplyRows(IReadOnlyList<ImportRow> rows, ref int createdCount, ref int updatedCount) // 검증된 CSV 행을 Unity 데이터 에셋에 적용
        {
            List<ProjectDataAsset> existingAssets = ProjectDataAssetDatabase.LoadAll(); // 적용 전 기존 프로젝트 데이터 에셋 조회

            for (int index = 0; index < rows.Count; index++) // 검증된 CSV 행 전체 순회
            {
                ImportRow row = rows[index]; // 현재 가져오기 행 조회
                ProjectDataAsset asset = ProjectDataAssetDatabase.FindById(existingAssets, row.DataId); // 같은 데이터 ID의 기존 에셋 검색

                if (asset == null) // 새 데이터 ID 여부 확인
                {
                    asset = CreateAsset(row); // 현재 CSV 행에 맞는 새 데이터 에셋 생성
                    existingAssets.Add(asset); // 이후 행 중복 검색용 기존 에셋 목록 추가
                    createdCount++; // 새 데이터 에셋 생성 수 증가
                }
                else // 기존 데이터 에셋 갱신 처리
                {
                    if (asset.Category != row.Category) // 기존 에셋과 CSV 데이터 분류 일치 여부 확인
                    {
                        throw new InvalidOperationException($"{row.DataId}의 기존 분류 {asset.Category}와 CSV 분류 {row.Category}가 다릅니다."); // 데이터 ID 분류 변경 차단 예외 발생
                    }

                    updatedCount++; // 기존 데이터 에셋 갱신 수 증가
                }

                asset.SetEditorIdentity(row.DataId, row.DisplayName, row.Version); // CSV 식별 정보와 버전 에셋 적용
                EditorUtility.SetDirty(asset); // 현재 데이터 에셋 변경 상태 표시
            }
        }

        private static ProjectDataAsset CreateAsset(ImportRow row) // 데이터 분류에 맞는 새 ScriptableObject 에셋 생성
        {
            ProjectDataAsset asset = CreateInstance(row.Category); // 데이터 분류에 맞는 ScriptableObject 인스턴스 생성
            string categoryFolderPath = ProjectDataAssetDatabase.DefinitionsRootPath + "/" + row.Category; // 데이터 분류별 저장 폴더 경로 생성
            EnsureFolder(categoryFolderPath); // 데이터 분류별 저장 폴더 준비
            string safeDisplayName = CreateSafeFileName(row.DisplayName); // 표시 이름 기반 안전한 파일명 생성
            string desiredPath = categoryFolderPath + "/" + row.DataId + "_" + safeDisplayName + ".asset"; // 새 데이터 에셋 기본 경로 생성
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(desiredPath); // 기존 에셋과 겹치지 않는 경로 생성
            AssetDatabase.CreateAsset(asset, uniquePath); // 새 ScriptableObject 데이터 에셋 저장
            return asset; // 생성된 데이터 에셋 반환
        }

        private static ProjectDataAsset CreateInstance(ProjectDataCategory category) // 데이터 분류별 ScriptableObject 인스턴스 생성
        {
            switch (category) // 데이터 분류별 생성 형식 분기
            {
                case ProjectDataCategory.Player: // 플레이어 데이터 분류 처리
                    return ScriptableObject.CreateInstance<PlayerDataDefinition>(); // 플레이어 데이터 에셋 생성

                case ProjectDataCategory.Map: // 맵 데이터 분류 처리
                    return ScriptableObject.CreateInstance<MapDataDefinition>(); // 맵 데이터 에셋 생성

                case ProjectDataCategory.Obstacle: // 장애물 데이터 분류 처리
                    return ScriptableObject.CreateInstance<ObstacleDataDefinition>(); // 장애물 데이터 에셋 생성

                case ProjectDataCategory.Item: // 아이템 데이터 분류 처리
                    return ScriptableObject.CreateInstance<ItemDataDefinition>(); // 아이템 데이터 에셋 생성

                case ProjectDataCategory.Cosmetic: // 꾸미기 데이터 분류 처리
                    return ScriptableObject.CreateInstance<CosmeticDataDefinition>(); // 꾸미기 데이터 에셋 생성

                case ProjectDataCategory.Audio: // 오디오 데이터 분류 처리
                    return ScriptableObject.CreateInstance<AudioDataDefinition>(); // 오디오 데이터 에셋 생성

                default: // 정의되지 않은 데이터 분류 처리
                    throw new ArgumentOutOfRangeException(nameof(category), category, "지원하지 않는 데이터 분류입니다."); // 알 수 없는 데이터 분류 예외 발생
            }
        }

        private static string CreateSafeFileName(string displayName) // 표시 이름을 Unity 에셋 파일명으로 정리
        {
            string safeName = displayName.Trim().Replace(' ', '_'); // 앞뒤 공백 제거와 내부 공백 밑줄 변경

            foreach (char invalidCharacter in Path.GetInvalidFileNameChars()) // 운영체제 파일명 금지 문자 전체 순회
            {
                safeName = safeName.Replace(invalidCharacter, '_'); // 금지 문자를 밑줄로 변경
            }

            return string.IsNullOrWhiteSpace(safeName) ? "Data" : safeName; // 비어 있지 않은 안전한 파일명 반환
        }

        private static void WriteImportErrors(IReadOnlyList<string> errors) // CSV 사전 검증 오류 전체 로그 출력
        {
            for (int index = 0; index < errors.Count; index++) // 발견된 CSV 오류 전체 순회
            {
                ProjectLog.Error(ProjectLogCategory.Data, errors[index], "DATA_CSV_INVALID"); // 현재 CSV 오류 로그 출력
            }

            ProjectLog.Error(ProjectLogCategory.Data, $"CSV 오류 {errors.Count}개로 가져오기를 취소했습니다. 데이터 에셋은 변경되지 않았습니다.", "DATA_CSV_IMPORT_CANCELLED"); // CSV 가져오기 취소 요약 로그 출력
        }

        private static void EnsureImportDirectory() // CSV 가져오기 폴더 존재 보장
        {
            EnsureFolder("Assets/_ProjectJ/Data"); // 프로젝트 데이터 루트 폴더 준비
            EnsureFolder(ImportDirectoryPath); // CSV 가져오기 폴더 준비
        }

        private static void EnsureFolder(string folderPath) // 지정된 Unity 폴더 경로 단계별 생성
        {
            if (AssetDatabase.IsValidFolder(folderPath)) // 지정 폴더의 기존 존재 여부 확인
            {
                return; // 기존 폴더 재사용
            }

            string parentPath = Path.GetDirectoryName(folderPath)?.Replace('\\', '/'); // 상위 Unity 폴더 경로 계산
            string folderName = Path.GetFileName(folderPath); // 생성할 마지막 폴더 이름 조회

            if (!string.IsNullOrEmpty(parentPath) && !AssetDatabase.IsValidFolder(parentPath)) // 상위 폴더 추가 생성 필요 여부 확인
            {
                EnsureFolder(parentPath); // 상위 폴더부터 재귀 생성
            }

            AssetDatabase.CreateFolder(parentPath, folderName); // 현재 Unity 폴더 생성
        }

        private sealed class ImportRow // 검증을 통과한 단일 CSV 데이터 행 선언
        {
            internal ProjectDataCategory Category { get; } // 데이터 분류 반환
            internal string DataId { get; } // 데이터 영구 ID 반환
            internal string DisplayName { get; } // 데이터 표시 이름 반환
            internal ProjectDataVersion Version { get; } // 데이터 버전 반환

            internal ImportRow(ProjectDataCategory category, string dataId, string displayName, ProjectDataVersion version) // 검증된 CSV 데이터 행 생성
            {
                Category = category; // 데이터 분류 저장
                DataId = dataId; // 데이터 영구 ID 저장
                DisplayName = displayName; // 데이터 표시 이름 저장
                Version = version; // 데이터 버전 저장
            }
        }
    }
}
