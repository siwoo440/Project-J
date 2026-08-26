using System; // 문자열 비교 사용

namespace ProjectJ.Items
{
    public static class ProjectJNetworkItemMaterialPolicy
    {
        public static bool IsKnownIncompatibleShaderName(
            string shaderName
        )
        {
            if (string.IsNullOrWhiteSpace(shaderName))
            {
                return true; // Shader 정보 누락 교체
            }

            if (
                string.Equals(
                    shaderName,
                    "Standard",
                    StringComparison.Ordinal
                ) ||
                string.Equals(
                    shaderName,
                    "Hidden/InternalErrorShader",
                    StringComparison.Ordinal
                )
            )
            {
                return true; // Built-in Standard와 Error Shader 교체
            }

            return shaderName.StartsWith(
                "Legacy Shaders/",
                StringComparison.Ordinal
            ); // Legacy Shader 계열 교체
        }

        public static bool IsBuiltinDefaultMaterialPath(
            string assetPath
        )
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return false; // 경로 없는 Runtime Material은 별도 Shader 검사 사용
            }

            string normalizedPath =
                assetPath.Replace(
                    '\\',
                    '/'
                ); // Unity Asset 경로 형식 통일

            return string.Equals(
                normalizedPath,
                "Resources/unity_builtin_extra",
                StringComparison.OrdinalIgnoreCase
            ); // Unity Built-in Default Material 경로 확인
        }
    }
}
