using System; // 문자열 비교 기능 사용
using System.Collections.Generic; // 목록과 HashSet 사용

namespace ProjectJ.Items // 아이템 시스템 네임스페이스
{
    public readonly struct ItemDefinitionValidationResult // 단일 아이템 검증 결과
    {
        public bool IsValid { get; } // 검증 성공 여부

        public string Message { get; } // 검증 메시지

        public ItemDefinitionValidationResult(bool isValid, string message) // 결과 생성
        {
            IsValid = isValid; // 성공 여부 저장
            Message = message ?? string.Empty; // 메시지 저장
        }
    }

    public static class ItemDefinitionValidator // 아이템 데이터 공통 검증기
    {
        public static ItemDefinitionValidationResult Validate(ItemDefinition definition) // 단일 데이터 검사
        {
            if (definition == null) // 데이터 누락 검사
            {
                return Invalid("ItemDefinition이 없습니다."); // 누락 결과 반환
            }

            if (string.IsNullOrWhiteSpace(definition.ItemId)) // ID 누락 검사
            {
                return Invalid("Item ID가 비어 있습니다."); // ID 오류 반환
            }

            if (string.IsNullOrWhiteSpace(definition.DisplayName)) // 표시 이름 누락 검사
            {
                return Invalid("Display Name이 비어 있습니다."); // 이름 오류 반환
            }

            if (definition.Duration < 0f) // 지속 시간 음수 검사
            {
                return Invalid("Duration은 0 이상이어야 합니다."); // 지속 시간 오류 반환
            }

            if (definition.Cooldown < 0f) // Cooldown 음수 검사
            {
                return Invalid("Cooldown은 0 이상이어야 합니다."); // Cooldown 오류 반환
            }

            if (definition.UseMode == ItemUseMode.Place && !definition.IsPlaceable) // 설치형 데이터 일관성 검사
            {
                return Invalid("Use Mode가 Place이면 Is Placeable이 활성화되어야 합니다."); // 설치형 오류 반환
            }

            return new ItemDefinitionValidationResult(true, string.Empty); // 정상 결과 반환
        }

        public static List<string> ValidateCatalog(IReadOnlyList<ItemDefinition> definitions) // 전체 아이템 ID 검사
        {
            List<string> errors = new List<string>(); // 오류 목록 생성

            if (definitions == null) // 목록 누락 검사
            {
                errors.Add("아이템 목록이 없습니다."); // 목록 오류 추가
                return errors; // 즉시 반환
            }

            HashSet<string> usedIds =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase); // 대소문자 무시 ID 중복 검사

            for (int i = 0; i < definitions.Count; i++) // 모든 아이템 반복
            {
                ItemDefinition definition = definitions[i]; // 현재 아이템 저장
                ItemDefinitionValidationResult result = Validate(definition); // 단일 아이템 검사

                if (!result.IsValid) // 단일 데이터 오류 검사
                {
                    errors.Add($"[{i}] {result.Message}"); // 오류 위치와 메시지 추가
                    continue; // ID 중복 검사를 건너뜀
                }

                if (!usedIds.Add(definition.ItemId.Trim())) // ID 중복 검사
                {
                    errors.Add($"중복 Item ID: {definition.ItemId}"); // 중복 ID 오류 추가
                }
            }

            return errors; // 전체 오류 목록 반환
        }

        private static ItemDefinitionValidationResult Invalid(string message) // 실패 결과 생성
        {
            return new ItemDefinitionValidationResult(false, message); // 실패 결과 반환
        }
    }
}
