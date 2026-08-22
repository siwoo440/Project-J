#if UNITY_EDITOR
using System.IO;
using Fusion;
using UnityEditor;
using UnityEngine;

namespace ProjectJ.Networking.Fusion.Editor
{
    [InitializeOnLoad]
    public static class
        ProjectJNetworkPlayerPrefabBuilder
    {
        private const string PrefabPath =
            "Assets/ProjectJ/Network/Fusion/Player/" +
            "Resources/ProjectJNetworkPlayer.prefab";

        static
            ProjectJNetworkPlayerPrefabBuilder()
        {
            EditorApplication.delayCall +=
                EnsurePrefab;
        }

        [MenuItem(
            "Tools/Project J/Fusion/" +
            "60일차 Network Player Prefab 재생성"
        )]
        private static void RebuildPrefab()
        {
            if (
                AssetDatabase.LoadAssetAtPath<
                    GameObject
                >(
                    PrefabPath
                ) != null
            )
            {
                AssetDatabase.DeleteAsset(
                    PrefabPath
                );
            }

            EnsurePrefab();

            Selection.activeObject =
                AssetDatabase.LoadAssetAtPath<
                    GameObject
                >(
                    PrefabPath
                );
        }

        private static void EnsurePrefab()
        {
            if (
                EditorApplication
                    .isPlayingOrWillChangePlaymode
            )
            {
                return;
            }

            GameObject existing =
                AssetDatabase.LoadAssetAtPath<
                    GameObject
                >(
                    PrefabPath
                );

            if (existing != null)
            {
                return;
            }

            string directory =
                Path.GetDirectoryName(
                    PrefabPath
                );

            if (
                !Directory.Exists(
                    directory
                )
            )
            {
                Directory.CreateDirectory(
                    directory
                );
            }

            GameObject root =
                new GameObject(
                    "ProjectJNetworkPlayer"
                );

            root.AddComponent<
                NetworkObject
            >();

            root.AddComponent<
                NetworkTransform
            >();

            root.AddComponent<
                ProjectJNetworkPlayer
            >();

            GameObject visual =
                GameObject.CreatePrimitive(
                    PrimitiveType.Capsule
                );

            visual.name =
                "Visual";

            visual.transform.SetParent(
                root.transform,
                false
            );

            visual.transform.localPosition =
                new Vector3(
                    0f,
                    1f,
                    0f
                );

            visual.transform.localScale =
                new Vector3(
                    0.8f,
                    1f,
                    0.8f
                );

            Collider visualCollider =
                visual.GetComponent<
                    Collider
                >();

            if (visualCollider != null)
            {
                Object.DestroyImmediate(
                    visualCollider
                );
            }

            GameObject cameraMarker =
                new GameObject(
                    "AuthorityCameraMarker"
                );

            cameraMarker.transform.SetParent(
                root.transform,
                false
            );

            cameraMarker.transform.localPosition =
                new Vector3(
                    0f,
                    1.6f,
                    -2f
                );

            Camera markerCamera =
                cameraMarker.AddComponent<
                    Camera
                >();

            markerCamera.enabled =
                false;

            markerCamera.cullingMask =
                0;

            PrefabUtility.SaveAsPrefabAsset(
                root,
                PrefabPath
            );

            Object.DestroyImmediate(
                root
            );

            AssetDatabase.ImportAsset(
                PrefabPath,
                ImportAssetOptions.ForceUpdate
            );

            AssetDatabase.SaveAssets();

            Debug.Log(
                "[Project J/Fusion] " +
                "60일차 Network Player Prefab 생성 완료: " +
                PrefabPath
            );
        }
    }
}
#endif
