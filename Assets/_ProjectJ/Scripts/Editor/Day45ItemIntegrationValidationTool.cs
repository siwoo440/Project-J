using System; // Enum과 문자열 기능 참조
using System.Collections.Generic; // 검증 목록과 중복 검사 기능 참조
using System.Globalization; // CSV 숫자 형식 고정 기능 참조
using System.IO; // CSV 파일 저장 기능 참조
using System.Linq; // 아이템 정렬과 개수 집계 기능 참조
using System.Text; // CSV 문자열 생성 기능 참조
using ProjectJ.Data; // 아이템 데이터 형식 참조
using ProjectJ.Items; // 아이템 선택 가중치 규칙 참조
using UnityEditor; // Unity 메뉴와 에셋 검색 기능 참조
using UnityEngine; // Unity 로그와 수학 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 45일차 아이템 검증 도구 정의
    internal static class Day45ItemIntegrationValidationTool // 아이템 28종 통합 검증과 기준표 내보내기 도구 선언
    { // 데이터 검증과 CSV 기록 기능 정의
        private const string ValidateMenuPath = ProjectJEditorMenuPaths.ItemValidation + "/아이템 28종 통합 검증 (Day 45일차)"; // 45일차 통합 검증 메뉴 경로
        private const string ExportMenuPath = ProjectJEditorMenuPaths.ItemValidation + "/아이템 밸런스 기준 CSV 내보내기 (Day 45일차)"; // 45일차 밸런스 기준표 메뉴 경로
        private const string ItemDataFolderPath = "Assets/_ProjectJ/Data/Definitions/Item"; // 아이템 데이터 폴더 경로
        private const string DocumentationFolderPath = "Assets/_ProjectJ/Documentation"; // 프로젝트 문서 폴더 경로
        private const string BalanceCsvPath = "Assets/_ProjectJ/Documentation/Day45_ItemBalanceBaseline.csv"; // 밸런스 기준 CSV 경로
        private const int ExpectedItemCount = 28; // 현재 구현 완료 아이템 수
        private const int ExpectedP0Count = 10; // P0 구현 아이템 수
        private const int ExpectedP1Count = 11; // P1 구현 아이템 수
        private const int ExpectedP2Count = 7; // P2 구현 아이템 수

        [MenuItem(ValidateMenuPath)] // Unity 상단 메뉴에 45일차 통합 검증 등록
        private static void ValidateItemIntegration() // 아이템 28종 데이터 구조와 가중치 일관성 검증
        { // 검증 결과를 Console과 대화상자로 출력
            ItemDataDefinition[] items = LoadItems(); // 프로젝트의 아이템 데이터 전체 불러오기
            List<string> errors = CollectValidationErrors(items); // 치명 검증 오류 수집
            List<string> warnings = CollectValidationWarnings(items); // 수동 확인이 필요한 경고 수집

            for (int index = 0; index < errors.Count; index++) // 발견된 모든 오류 순회
            { // 각 오류를 개별 Console 항목으로 출력
                Debug.LogError($"[ProjectJ][Day45][ItemValidation] {errors[index]}"); // 통합 검증 오류 출력
            } // 오류 출력 완료

            for (int index = 0; index < warnings.Count; index++) // 발견된 모든 경고 순회
            { // 각 경고를 개별 Console 항목으로 출력
                Debug.LogWarning($"[ProjectJ][Day45][ItemValidation] {warnings[index]}"); // 수동 확인 경고 출력
            } // 경고 출력 완료

            if (errors.Count > 0) // 치명 검증 오류 존재 여부 확인
            { // 실패 결과를 사용자에게 명확하게 표시
                EditorUtility.DisplayDialog("Project J Day 45", $"아이템 통합 검증 실패: 오류 {errors.Count}개, 경고 {warnings.Count}개\nConsole을 확인합니다.", "확인"); // 검증 실패 대화상자 표시
                return; // 성공 로그 출력 생략
            } // 검증 실패 처리 완료

            float totalWeight = ItemSelectionRules.CalculateTotalWeight(items); // 현재 28종 전체 등장 가중치 계산
            Debug.Log($"[ProjectJ][Day45][ItemValidation] 28종 데이터 검증 통과 | P0 {ExpectedP0Count} / P1 {ExpectedP1Count} / P2 {ExpectedP2Count} | 총 가중치 {totalWeight.ToString("0.###", CultureInfo.InvariantCulture)} | 경고 {warnings.Count}개"); // 검증 통과 요약 출력
            EditorUtility.DisplayDialog("Project J Day 45", $"아이템 28종 통합 데이터 검증을 통과했습니다.\n경고 {warnings.Count}개는 Console에서 수동 확인합니다.", "확인"); // 검증 성공 대화상자 표시
        } // 통합 데이터 검증 처리 완료

        [MenuItem(ExportMenuPath)] // Unity 상단 메뉴에 밸런스 기준 CSV 내보내기 등록
        private static void ExportBalanceBaseline() // 현재 28종 수치를 CSV 기준표로 저장
        { // 수동 플레이 테스트 전후 비교용 기준 데이터 생성
            ItemDataDefinition[] items = LoadItems(); // 프로젝트의 아이템 데이터 전체 불러오기
            List<string> errors = CollectValidationErrors(items); // 내보내기 전 치명 데이터 오류 확인

            if (errors.Count > 0) // 치명 데이터 오류 존재 여부 확인
            { // 잘못된 기준표 생성 방지
                ValidateItemIntegration(); // 동일한 검증 결과와 오류 로그 출력
                return; // CSV 내보내기 중단
            } // 잘못된 데이터 내보내기 차단 완료

            EnsureDocumentationFolder(); // Documentation 폴더 존재 상태 보장

            if (File.Exists(BalanceCsvPath) && !EditorUtility.DisplayDialog("Project J Day 45", "기존 밸런스 기준 CSV가 있습니다. 기존 수동 기록을 지우고 현재 데이터로 덮어쓸지 확인합니다.", "덮어쓰기", "취소")) // 기존 수동 기록 파일 덮어쓰기 동의 여부 확인
            { // 기존 결과 보존을 선택한 경우 처리
                return; // CSV 내보내기 중단
            } // 기존 결과 보호 처리 완료

            float totalWeight = ItemSelectionRules.CalculateTotalWeight(items); // 등장 확률 계산용 전체 가중치 조회
            StringBuilder builder = new StringBuilder(); // CSV 전체 문자열 생성기 준비
            builder.AppendLine("DataId,DisplayName,Priority,UseType,EffectType,SpawnWeight,SpawnProbability,MaximumStackCount,EffectDuration,PrimaryValue,SecondaryValue,EffectRange,EffectRadius,Cooldown,ProjectileSpeed,ManualResult,Notes"); // CSV 헤더 작성

            for (int index = 0; index < items.Length; index++) // DataId 순서 아이템 전체 순회
            { // 현재 아이템의 기준 수치 한 행 생성
                ItemDataDefinition item = items[index]; // 현재 아이템 데이터 조회
                float probability = totalWeight <= 0f ? 0f : item.SpawnWeight / totalWeight; // 현재 등장 가중치 비율 계산
                builder.Append(EscapeCsv(item.DataId)); // 데이터 ID 열 작성
                builder.Append(','); // CSV 열 구분자 추가
                builder.Append(EscapeCsv(item.DisplayName)); // 표시 이름 열 작성
                builder.Append(','); // CSV 열 구분자 추가
                builder.Append(item.ImplementationPriority); // 구현 우선순위 열 작성
                builder.Append(','); // CSV 열 구분자 추가
                builder.Append(item.UseType); // 사용 방식 열 작성
                builder.Append(','); // CSV 열 구분자 추가
                builder.Append(item.EffectType); // 효과 종류 열 작성
                builder.Append(','); // CSV 열 구분자 추가
                builder.Append(ToInvariant(item.SpawnWeight)); // 등장 가중치 열 작성
                builder.Append(','); // CSV 열 구분자 추가
                builder.Append(ToInvariant(probability)); // 등장 확률 비율 열 작성
                builder.Append(','); // CSV 열 구분자 추가
                builder.Append(item.MaximumStackCount); // 최대 중첩 수 열 작성
                builder.Append(','); // CSV 열 구분자 추가
                builder.Append(ToInvariant(item.EffectDuration)); // 효과 시간 열 작성
                builder.Append(','); // CSV 열 구분자 추가
                builder.Append(ToInvariant(item.PrimaryValue)); // 핵심 수치 열 작성
                builder.Append(','); // CSV 열 구분자 추가
                builder.Append(ToInvariant(item.SecondaryValue)); // 보조 수치 열 작성
                builder.Append(','); // CSV 열 구분자 추가
                builder.Append(ToInvariant(item.EffectRange)); // 효과 거리 열 작성
                builder.Append(','); // CSV 열 구분자 추가
                builder.Append(ToInvariant(item.EffectRadius)); // 효과 반경 열 작성
                builder.Append(','); // CSV 열 구분자 추가
                builder.Append(ToInvariant(item.Cooldown)); // 반복 간격 열 작성
                builder.Append(','); // CSV 열 구분자 추가
                builder.Append(ToInvariant(item.ProjectileSpeed)); // 투사체 속도 열 작성
                builder.Append(",미검증,"); // 수동 결과와 메모 기본 열 작성
                builder.AppendLine(); // 현재 아이템 행 종료
            } // 28종 기준 행 생성 완료

            File.WriteAllText(BalanceCsvPath, builder.ToString(), new UTF8Encoding(true)); // Excel 호환 UTF-8 BOM CSV 저장
            AssetDatabase.ImportAsset(BalanceCsvPath); // 새 CSV를 Unity AssetDatabase에 즉시 반영
            AssetDatabase.Refresh(); // Project 창의 새 문서 표시 갱신
            UnityEngine.Object csvAsset = AssetDatabase.LoadMainAssetAtPath(BalanceCsvPath); // 저장된 CSV 에셋 조회
            Selection.activeObject = csvAsset; // Project 창에서 새 CSV 선택
            EditorGUIUtility.PingObject(csvAsset); // Project 창의 CSV 위치 강조
            Debug.Log($"[ProjectJ][Day45][ItemBalance] 기준표 저장 완료 | {BalanceCsvPath}"); // CSV 저장 경로 Console 출력
            EditorUtility.DisplayDialog("Project J Day 45", "아이템 28종 밸런스 기준 CSV를 저장했습니다.", "확인"); // CSV 생성 완료 대화상자 표시
        } // 밸런스 기준표 내보내기 완료

        internal static ItemDataDefinition[] LoadItems() // Item 폴더의 모든 아이템 데이터 정렬 로드
        { // 검증과 내보내기에서 동일한 데이터 집합 사용
            string[] guids = AssetDatabase.FindAssets("t:ItemDataDefinition", new[] { ItemDataFolderPath }); // 아이템 데이터 GUID 전체 검색
            List<ItemDataDefinition> items = new List<ItemDataDefinition>(); // 유효 아이템 데이터 목록 생성

            for (int index = 0; index < guids.Length; index++) // 검색된 모든 GUID 순회
            { // GUID를 실제 ItemDataDefinition으로 변환
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[index]); // GUID의 프로젝트 경로 조회
                ItemDataDefinition item = AssetDatabase.LoadAssetAtPath<ItemDataDefinition>(assetPath); // 아이템 데이터 에셋 불러오기

                if (item != null) // 유효한 아이템 데이터 여부 확인
                { // 검증 대상 목록에 정상 데이터 추가
                    items.Add(item); // 현재 아이템 데이터 목록 추가
                } // 유효 아이템 수집 완료
            } // Item 폴더 검색 결과 처리 완료

            return items.OrderBy(item => item.DataId, StringComparer.Ordinal).ToArray(); // DataId 오름차순 배열 반환
        } // 아이템 데이터 로드 완료

        internal static List<string> CollectValidationErrors(ItemDataDefinition[] items) // 자동 판정 가능한 치명 데이터 오류 수집
        { // 수량·ID·EffectType·우선순위·가중치 규칙 검사
            List<string> errors = new List<string>(); // 치명 오류 목록 생성
            ItemDataDefinition[] safeItems = items ?? Array.Empty<ItemDataDefinition>(); // null 입력을 빈 배열로 보정

            if (safeItems.Length != ExpectedItemCount) // 실제 아이템 수와 현재 구현 범위 비교
            { // 28종 누락 또는 초과 상태 기록
                errors.Add($"아이템 데이터 수가 {ExpectedItemCount}개가 아닙니다. 현재 {safeItems.Length}개"); // 아이템 수 오류 추가
            } // 아이템 수 검사 완료

            if (Enum.GetValues(typeof(ItemEffectType)).Length != ExpectedItemCount) // Runtime EffectType 열거형 수 확인
            { // 코드와 데이터의 구현 범위 불일치 기록
                errors.Add($"ItemEffectType 개수가 {ExpectedItemCount}개가 아닙니다. 현재 {Enum.GetValues(typeof(ItemEffectType)).Length}개"); // EffectType 수 오류 추가
            } // EffectType 열거형 수 검사 완료

            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal); // 중복 DataId 검사 집합 생성
            HashSet<ItemEffectType> effects = new HashSet<ItemEffectType>(); // 중복 효과 종류 검사 집합 생성

            for (int index = 0; index < safeItems.Length; index++) // 전체 아이템 데이터 순회
            { // 현재 아이템 공통 필수 값 검사
                ItemDataDefinition item = safeItems[index]; // 현재 아이템 데이터 조회

                if (item == null) // 누락 아이템 참조 여부 확인
                { // 잘못된 배열 원소 기록
                    errors.Add($"아이템 배열 {index}번 요소가 null입니다."); // null 아이템 오류 추가
                    continue; // 현재 아이템 세부 검사 생략
                } // null 아이템 처리 완료

                if (string.IsNullOrWhiteSpace(item.DataId)) // 고유 ID 누락 여부 확인
                { // 식별 불가능 아이템 기록
                    errors.Add($"{item.name}: DataId가 비어 있습니다."); // DataId 누락 오류 추가
                } // DataId 누락 검사 완료
                else if (!ids.Add(item.DataId)) // 같은 DataId 재등장 여부 확인
                { // 데이터 ID 충돌 기록
                    errors.Add($"중복 DataId: {item.DataId}"); // DataId 중복 오류 추가
                } // DataId 중복 검사 완료

                if (!effects.Add(item.EffectType)) // 같은 효과 종류 재등장 여부 확인
                { // 28종 일대일 효과 매핑 위반 기록
                    errors.Add($"중복 ItemEffectType: {item.EffectType}"); // EffectType 중복 오류 추가
                } // EffectType 중복 검사 완료

                if (item.SpawnWeight <= 0f) // 등장 가중치 양수 여부 확인
                { // 가중치 선택 불가능 데이터 기록
                    errors.Add($"{item.DataId}: SpawnWeight가 0 이하입니다."); // 등장 가중치 오류 추가
                } // 등장 가중치 검사 완료

                if (item.MaximumStackCount < 1) // 최대 중첩 수 최소값 확인
                { // 보유 수량 규칙 위반 기록
                    errors.Add($"{item.DataId}: MaximumStackCount가 1 미만입니다."); // 최대 중첩 수 오류 추가
                } // 최대 중첩 수 검사 완료
            } // 개별 아이템 필수 값 검사 완료

            for (int itemNumber = 1; itemNumber <= ExpectedItemCount; itemNumber++) // ITM-001부터 ITM-028까지 순서 확인
            { // 현재 필수 ID 존재 여부 검사
                string requiredId = $"ITM-{itemNumber:000}"; // 현재 필수 데이터 ID 생성

                if (!ids.Contains(requiredId)) // 현재 필수 ID 존재 여부 확인
                { // 번호 누락 기록
                    errors.Add($"필수 아이템 ID 누락: {requiredId}"); // ID 연속성 오류 추가
                } // 현재 필수 ID 검사 완료
            } // 28개 ID 연속성 검사 완료

            int p0Count = safeItems.Count(item => item != null && item.ImplementationPriority == ItemImplementationPriority.P0); // P0 데이터 개수 계산
            int p1Count = safeItems.Count(item => item != null && item.ImplementationPriority == ItemImplementationPriority.P1); // P1 데이터 개수 계산
            int p2Count = safeItems.Count(item => item != null && item.ImplementationPriority == ItemImplementationPriority.P2); // P2 데이터 개수 계산

            if (p0Count != ExpectedP0Count) // P0 구현 개수 확인
            { // 42일차 구현 범위 불일치 기록
                errors.Add($"P0 아이템 수가 {ExpectedP0Count}개가 아닙니다. 현재 {p0Count}개"); // P0 개수 오류 추가
            } // P0 개수 검사 완료

            if (p1Count != ExpectedP1Count) // P1 구현 개수 확인
            { // 43일차 구현 범위 불일치 기록
                errors.Add($"P1 아이템 수가 {ExpectedP1Count}개가 아닙니다. 현재 {p1Count}개"); // P1 개수 오류 추가
            } // P1 개수 검사 완료

            if (p2Count != ExpectedP2Count) // P2 구현 개수 확인
            { // 44일차 구현 범위 불일치 기록
                errors.Add($"P2 아이템 수가 {ExpectedP2Count}개가 아닙니다. 현재 {p2Count}개"); // P2 개수 오류 추가
            } // P2 개수 검사 완료

            if (effects.Count != ExpectedItemCount) // 28개 서로 다른 효과 종류 확보 여부 확인
            { // 누락 또는 중복 효과 매핑 기록
                errors.Add($"고유 ItemEffectType 수가 {ExpectedItemCount}개가 아닙니다. 현재 {effects.Count}개"); // 고유 효과 수 오류 추가
            } // 고유 효과 수 검사 완료

            if (ItemSelectionRules.CalculateTotalWeight(safeItems) <= 0f) // 전체 등장 가중치 합계 확인
            { // 확률 선택 자체가 불가능한 상태 기록
                errors.Add("전체 아이템 SpawnWeight 합계가 0 이하입니다."); // 전체 가중치 오류 추가
            } // 전체 가중치 검사 완료

            return errors; // 수집된 치명 오류 목록 반환
        } // 치명 데이터 오류 수집 완료

        internal static List<string> CollectValidationWarnings(ItemDataDefinition[] items) // 수동 확인이 필요한 비치명 항목 수집
        { // HUD 아이콘과 비정상적으로 큰 수치 후보 확인
            List<string> warnings = new List<string>(); // 경고 목록 생성
            ItemDataDefinition[] safeItems = items ?? Array.Empty<ItemDataDefinition>(); // null 입력을 빈 배열로 보정

            for (int index = 0; index < safeItems.Length; index++) // 전체 아이템 데이터 순회
            { // 현재 아이템의 수동 확인 후보 검사
                ItemDataDefinition item = safeItems[index]; // 현재 아이템 데이터 조회

                if (item == null) // 누락 아이템 참조 여부 확인
                { // 치명 오류에서 이미 처리된 항목 생략
                    continue; // 현재 경고 검사 생략
                } // null 아이템 경고 검사 완료

                if (item.InventoryIcon == null) // HUD용 아이콘 누락 여부 확인
                { // 프로토타입 텍스트 대체 가능성을 고려해 경고만 기록
                    warnings.Add($"{item.DataId}: InventoryIcon 미지정 - HUD 표시 수동 확인 필요"); // HUD 아이콘 경고 추가
                } // HUD 아이콘 검사 완료
            } // 비치명 데이터 검사 완료

            return warnings; // 수집된 경고 목록 반환
        } // 비치명 경고 수집 완료

        private static void EnsureDocumentationFolder() // Documentation 폴더 생성 상태 보장
        { // 상위 ProjectJ 폴더 아래 문서 폴더 준비
            if (AssetDatabase.IsValidFolder(DocumentationFolderPath)) // Documentation 폴더 존재 여부 확인
            { // 기존 폴더 재사용
                return; // 새 폴더 생성 생략
            } // 기존 폴더 처리 완료

            AssetDatabase.CreateFolder("Assets/_ProjectJ", "Documentation"); // 프로젝트 문서 폴더 생성
        } // Documentation 폴더 준비 완료

        private static string ToInvariant(float value) // CSV용 소수점 형식 변환
        { // 지역 설정과 무관한 점 소수점 사용
            return value.ToString("0.######", CultureInfo.InvariantCulture); // 고정 소수점 문자열 반환
        } // 숫자 문자열 변환 완료

        private static string EscapeCsv(string value) // CSV 문자열 안전 인코딩
        { // 쉼표와 따옴표와 줄바꿈 포함 값 처리
            string safeValue = value ?? string.Empty; // null 문자열을 빈 값으로 보정

            if (!safeValue.Contains(',') && !safeValue.Contains('"') && !safeValue.Contains('\n') && !safeValue.Contains('\r')) // CSV 특수문자 포함 여부 확인
            { // 따옴표가 필요 없는 값 처리
                return safeValue; // 원본 문자열 반환
            } // 단순 문자열 처리 완료

            return $"\"{safeValue.Replace("\"", "\"\"")}\""; // 내부 따옴표 이스케이프 후 CSV 따옴표 적용
        } // CSV 문자열 인코딩 완료
    } // 45일차 아이템 검증 도구 정의
} // 프로젝트 Editor 기능 정의
