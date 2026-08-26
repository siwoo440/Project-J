using NUnit.Framework; // EditMode Test 사용
using ProjectJ.Items; // Network Item Material 정책 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJNetworkItemMaterialPolicyTests
    {
        [Test]
        public void IsKnownIncompatibleShaderName_RejectsStandardShader()
        {
            bool result =
                ProjectJNetworkItemMaterialPolicy.IsKnownIncompatibleShaderName(
                    "Standard"
                ); // Built-in Standard Shader 판정

            Assert.That(
                result,
                Is.True
            ); // URP 교체 대상 검증
        }

        [Test]
        public void IsKnownIncompatibleShaderName_RejectsLegacyShader()
        {
            bool result =
                ProjectJNetworkItemMaterialPolicy.IsKnownIncompatibleShaderName(
                    "Legacy Shaders/Diffuse"
                ); // Legacy Shader 판정

            Assert.That(
                result,
                Is.True
            ); // Legacy Shader 교체 대상 검증
        }

        [Test]
        public void IsKnownIncompatibleShaderName_AllowsUrpLitShader()
        {
            bool result =
                ProjectJNetworkItemMaterialPolicy.IsKnownIncompatibleShaderName(
                    "Universal Render Pipeline/Lit"
                ); // URP Lit Shader 판정

            Assert.That(
                result,
                Is.False
            ); // URP Lit 유지 검증
        }

        [Test]
        public void IsKnownIncompatibleShaderName_RejectsMissingName()
        {
            bool result =
                ProjectJNetworkItemMaterialPolicy.IsKnownIncompatibleShaderName(
                    string.Empty
                ); // Shader 이름 누락 판정

            Assert.That(
                result,
                Is.True
            ); // 누락 Shader 교체 대상 검증
        }

        [Test]
        public void IsBuiltinDefaultMaterialPath_DetectsUnityBuiltinExtra()
        {
            bool result =
                ProjectJNetworkItemMaterialPolicy.IsBuiltinDefaultMaterialPath(
                    "Resources/unity_builtin_extra"
                ); // Unity Built-in Material 경로 판정

            Assert.That(
                result,
                Is.True
            ); // Built-in Default Material 교체 대상 검증
        }

        [Test]
        public void IsBuiltinDefaultMaterialPath_AllowsProjectMaterial()
        {
            bool result =
                ProjectJNetworkItemMaterialPolicy.IsBuiltinDefaultMaterialPath(
                    "Assets/ProjectJ/Art/Materials/Player.mat"
                ); // Project Material 경로 판정

            Assert.That(
                result,
                Is.False
            ); // Project Material 유지 검증
        }
    }
}
