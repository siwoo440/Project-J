using ProjectJ.Push;
using UnityEditor;
using UnityEngine;

namespace ProjectJ.Editor
{
    public static class Day42ExternalForceSetup
    {
        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/" +
            "Player.prefab";

        [MenuItem(
            "ProjectJ/Day42/Setup External Force Accumulator"
        )]
        public static void SetupExternalForceAccumulator()
        {
            GameObject prefabRoot =
                PrefabUtility.LoadPrefabContents(
                    PlayerPrefabPath
                );

            if (prefabRoot == null)
            {
                Debug.LogError(
                    "Player.prefab을 찾을 수 없습니다: " +
                    PlayerPrefabPath
                );

                return;
            }

            try
            {
                Rigidbody body =
                    prefabRoot.GetComponent<
                        Rigidbody
                    >();

                if (body == null)
                {
                    Debug.LogError(
                        "Player.prefab에서 Rigidbody를 " +
                        "찾을 수 없습니다."
                    );

                    return;
                }

                PlayerExternalForceAccumulator
                    accumulator =
                        prefabRoot.GetComponent<
                            PlayerExternalForceAccumulator
                        >();

                if (accumulator == null)
                {
                    accumulator =
                        prefabRoot.AddComponent<
                            PlayerExternalForceAccumulator
                        >();
                }

                accumulator.Configure(
                    body,
                    12f,
                    0.05f
                );

                PlayerPushController pushController =
                    prefabRoot.GetComponent<
                        PlayerPushController
                    >();

                if (pushController != null)
                {
                    SerializedObject serializedController =
                        new SerializedObject(
                            pushController
                        );

                    SerializedProperty horizontalProperty =
                        serializedController
                            .FindProperty(
                                "horizontalVelocityChange"
                            );

                    if (horizontalProperty != null)
                    {
                        horizontalProperty.floatValue =
                            12f;
                    }

                    SerializedProperty upwardProperty =
                        serializedController
                            .FindProperty(
                                "upwardVelocityChange"
                            );

                    if (upwardProperty != null)
                    {
                        upwardProperty.floatValue =
                            0f;
                    }

                    serializedController
                        .ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(
                    prefabRoot,
                    PlayerPrefabPath
                );
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(
                    prefabRoot
                );
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Day42 External Force Accumulator 설정 완료. " +
                "Player.prefab에 밀치기 수평 힘 12와 " +
                "수평 감속 기반 외부 힘 처리를 적용했습니다."
            );
        }
    }
}
