using Fusion; // NetworkProjectConfig와 Network Conditions 사용
using UnityEditor; // Editor Window와 Asset 저장 사용
using UnityEngine; // Editor GUI 사용

namespace ProjectJ.EditorTools
{
    public sealed class ProjectJDay79NetworkConditionPresetWindow :
        EditorWindow
    {
        private Vector2 scrollPosition;

        [MenuItem(
            "Project J/Day79/Network Condition Presets"
        )]
        private static void OpenWindow()
        {
            ProjectJDay79NetworkConditionPresetWindow window =
                GetWindow<
                    ProjectJDay79NetworkConditionPresetWindow
                >(
                    "Day79 Network Conditions"
                );

            window.minSize =
                new Vector2(
                    520f,
                    540f
                );

            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition =
                EditorGUILayout.BeginScrollView(
                    scrollPosition
                );

            EditorGUILayout.Space(
                8f
            );

            EditorGUILayout.LabelField(
                "Project J - Day79 Network Condition Presets",
                EditorStyles.boldLabel
            );

            EditorGUILayout.Space(
                6f
            );

            EditorGUILayout.HelpBox(
                "Photon Fusion 내장 Network Conditions는 Debug fusion.dll에서만 실제 적용됩니다. " +
                "Network Conditions가 보이지 않거나 효과가 없다면 Fusion > Toggle Debug Dlls를 실행하고 Unity를 재시작하세요.",
                MessageType.Warning
            );

            EditorGUILayout.Space(
                8f
            );

            if (
                GUILayout.Button(
                    "Fusion Network Project Config 열기",
                    GUILayout.Height(
                        30f
                    )
                )
            )
            {
                OpenNetworkProjectConfig();
            }

            if (
                GUILayout.Button(
                    "Fusion Debug DLL Toggle 메뉴 실행",
                    GUILayout.Height(
                        30f
                    )
                )
            )
            {
                ToggleDebugDlls();
            }

            EditorGUILayout.Space(
                12f
            );

            DrawCurrentConfig();

            EditorGUILayout.Space(
                12f
            );

            EditorGUILayout.LabelField(
                "79일차 Preset",
                EditorStyles.boldLabel
            );

            DrawPresetButton(
                "A. NORMAL / Simulation OFF",
                false,
                0d,
                0d,
                0d
            );

            DrawPresetButton(
                "B. 100ms / 0% Loss",
                true,
                0.100d,
                0d,
                0d
            );

            DrawPresetButton(
                "C. 약 150ms + Jitter / 1% Loss",
                true,
                0.130d,
                0.040d,
                0.010d
            );

            DrawPresetButton(
                "D. 약 200ms + Jitter / 3% Loss",
                true,
                0.180d,
                0.040d,
                0.030d
            );

            DrawPresetButton(
                "E. 약 250ms + Jitter / 5% Loss",
                true,
                0.220d,
                0.060d,
                0.050d
            );

            EditorGUILayout.Space(
                12f
            );

            EditorGUILayout.HelpBox(
                "Preset 적용 후 이미 실행 중인 NetworkRunner가 있다면 테스트를 종료하고 Host/Client를 다시 시작하세요. " +
                "각 실행 프로그램이 동일한 NetworkProjectConfig를 사용해야 비교가 쉽습니다.",
                MessageType.Info
            );

            EditorGUILayout.EndScrollView();
        }

        private static void DrawPresetButton(
            string label,
            bool enabled,
            double baseDelay,
            double additionalJitter,
            double lossChance
        )
        {
            if (
                !GUILayout.Button(
                    label,
                    GUILayout.Height(
                        34f
                    )
                )
            )
            {
                return;
            }

            ApplyPreset(
                enabled,
                baseDelay,
                additionalJitter,
                lossChance
            );
        }

        private static void ApplyPreset(
            bool enabled,
            double baseDelay,
            double additionalJitter,
            double lossChance
        )
        {
            NetworkProjectConfigAsset asset =
                NetworkProjectConfigAsset.Global;

            if (
                asset == null ||
                asset.Config == null ||
                asset.Config.NetworkConditions == null
            )
            {
                Debug.LogError(
                    "[Project J/Day79] NetworkProjectConfigAsset을 찾지 못했습니다."
                );

                return;
            }

            Undo.RecordObject(
                asset,
                "Day79 Network Condition Preset"
            );

            NetworkSimulationConfiguration conditions =
                asset.Config.NetworkConditions;

            conditions.Enabled =
                enabled;

            conditions.DelayMin =
                baseDelay;

            conditions.DelayMax =
                baseDelay;

            conditions.DelayPeriod =
                0d;

            conditions.DelayThreshold =
                0d;

            conditions.AdditionalJitter =
                additionalJitter;

            conditions.LossChanceMin =
                lossChance;

            conditions.LossChanceMax =
                lossChance;

            conditions.LossChancePeriod =
                0d;

            conditions.LossChanceThreshold =
                0d;

            conditions.AdditionalLoss =
                0d;

            EditorUtility.SetDirty(
                asset
            );

            AssetDatabase.SaveAssets();

            Debug.Log(
                "[Project J/Day79] Network Condition Preset 적용 / " +
                "Enabled: " +
                enabled +
                " / Delay: " +
                (
                    baseDelay *
                    1000d
                ).ToString("F0") +
                "ms / Jitter: " +
                (
                    additionalJitter *
                    1000d
                ).ToString("F0") +
                "ms / Loss: " +
                (
                    lossChance *
                    100d
                ).ToString("F1") +
                "%"
            );
        }

        private static void DrawCurrentConfig()
        {
            NetworkProjectConfigAsset asset =
                NetworkProjectConfigAsset.Global;

            if (
                asset == null ||
                asset.Config == null ||
                asset.Config.NetworkConditions == null
            )
            {
                EditorGUILayout.HelpBox(
                    "NetworkProjectConfig를 불러오지 못했습니다.",
                    MessageType.Error
                );

                return;
            }

            NetworkSimulationConfiguration conditions =
                asset.Config.NetworkConditions;

            EditorGUILayout.LabelField(
                "현재 설정",
                EditorStyles.boldLabel
            );

            EditorGUILayout.LabelField(
                "Enabled",
                conditions.Enabled.ToString()
            );

            EditorGUILayout.LabelField(
                "Delay",
                (
                    (
                        conditions.DelayMin +
                        conditions.DelayMax
                    ) *
                    500d
                ).ToString("F0") +
                " ms"
            );

            EditorGUILayout.LabelField(
                "Additional Jitter",
                (
                    conditions.AdditionalJitter *
                    1000d
                ).ToString("F0") +
                " ms"
            );

            EditorGUILayout.LabelField(
                "Loss",
                (
                    (
                        conditions.LossChanceMin +
                        conditions.LossChanceMax
                    ) *
                    50d
                ).ToString("F1") +
                " %"
            );
        }

        private static void OpenNetworkProjectConfig()
        {
            bool executed =
                EditorApplication.ExecuteMenuItem(
                    "Fusion/Network Project Config"
                );

            if (executed)
            {
                return;
            }

            NetworkProjectConfigAsset asset =
                NetworkProjectConfigAsset.Global;

            if (asset != null)
            {
                Selection.activeObject =
                    asset;

                EditorGUIUtility.PingObject(
                    asset
                );

                return;
            }

            Debug.LogWarning(
                "[Project J/Day79] Fusion Network Project Config 메뉴를 자동으로 열지 못했습니다."
            );
        }

        private static void ToggleDebugDlls()
        {
            bool executed =
                EditorApplication.ExecuteMenuItem(
                    "Fusion/Toggle Debug Dlls"
                );

            if (!executed)
            {
                Debug.LogWarning(
                    "[Project J/Day79] Fusion > Toggle Debug Dlls 메뉴를 자동으로 실행하지 못했습니다. 상단 Fusion 메뉴에서 직접 실행하세요."
                );
            }
        }
    }
}
