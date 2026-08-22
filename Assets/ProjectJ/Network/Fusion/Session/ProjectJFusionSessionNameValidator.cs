namespace ProjectJ.Networking.Fusion
{
    public static class ProjectJFusionSessionNameValidator
    {
        public const int MinimumLength = 3;
        public const int MaximumLength = 24;

        public static bool TryNormalize(
            string value,
            out string normalized,
            out string errorMessage
        )
        {
            normalized =
                string.IsNullOrWhiteSpace(value)
                    ? string.Empty
                    : value.Trim();

            if (normalized.Length < MinimumLength)
            {
                errorMessage =
                    $"세션 이름은 {MinimumLength}자 이상이어야 합니다.";

                return false;
            }

            if (normalized.Length > MaximumLength)
            {
                errorMessage =
                    $"세션 이름은 {MaximumLength}자 이하여야 합니다.";

                return false;
            }

            for (
                int i = 0;
                i < normalized.Length;
                i++
            )
            {
                char character =
                    normalized[i];

                bool isAllowed =
                    character >= 'A' &&
                    character <= 'Z' ||
                    character >= 'a' &&
                    character <= 'z' ||
                    character >= '0' &&
                    character <= '9' ||
                    character == '-' ||
                    character == '_';

                if (isAllowed)
                {
                    continue;
                }

                errorMessage =
                    "세션 이름에는 영문, 숫자, - , _ 만 사용할 수 있습니다.";

                return false;
            }

            errorMessage =
                string.Empty;

            return true;
        }
    }
}
