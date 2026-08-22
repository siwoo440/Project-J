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

        private const float StandingColliderHeight =
            2f;

        private const float BodyColliderRadius =
            0.4f;

        static
            ProjectJNetworkPlayerPrefabBuilder()
        {
            EditorApplication.delayCall +=
                EnsurePrefab;
        }

        [MenuItem(
            "Tools/Project J/Fusion/" +
            "67일차 Network Player Prefab 검증"
        )]
        private static void RebuildPrefab()
        {
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
                EnsureExistingPrefabConfiguration();

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

            CapsuleCollider bodyCollider =
                root.AddComponent<
                    CapsuleCollider
                >();

            ConfigureBodyCollider(
                bodyCollider
            );

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
                "67일차 Network Player Prefab 생성 완료: " +
                PrefabPath
            );
        }

        private static void
            EnsureExistingPrefabConfiguration()
        {
            GameObject root =
                PrefabUtility.LoadPrefabContents(
                    PrefabPath
                );

            bool changed =
                false;

            try
            {
                CapsuleCollider bodyCollider =
                    root.GetComponent<
                        CapsuleCollider
                    >();

                if (bodyCollider == null)
                {
                    bodyCollider =
                        root.AddComponent<
                            CapsuleCollider
                        >();

                    changed =
                        true;
                }

                if (
                    !Mathf.Approximately(
                        bodyCollider.height,
                        StandingColliderHeight
                    ) ||
                    !Mathf.Approximately(
                        bodyCollider.radius,
                        BodyColliderRadius
                    ) ||
                    bodyCollider.center !=
                        new Vector3(
                            0f,
                            1f,
                            0f
                        ) ||
                    bodyCollider.direction !=
                        1 ||
                    bodyCollider.isTrigger
                )
                {
                    ConfigureBodyCollider(
                        bodyCollider
                    );

                    changed =
                        true;
                }

                Transform visual =
                    root.transform.Find(
                        "Visual"
                    );

                if (visual != null)
                {
                    Vector3 targetPosition =
                        new Vector3(
                            0f,
                            1f,
                            0f
                        );

                    Vector3 targetScale =
                        new Vector3(
                            0.8f,
                            1f,
                            0.8f
                        );

                    if (
                        visual.localPosition !=
                            targetPosition
                    )
                    {
                        visual.localPosition =
                            targetPosition;

                        changed =
                            true;
                    }

                    if (
                        visual.localScale !=
                            targetScale
                    )
                    {
                        visual.localScale =
                            targetScale;

                        changed =
                            true;
                    }

                    Collider visualCollider =
                        visual.GetComponent<
                            Collider
                        >();

                    if (visualCollider != null)
                    {
                        Object.DestroyImmediate(
                            visualCollider
                        );

                        changed =
                            true;
                    }
                }

                if (!changed)
                {
                    return;
                }

                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    PrefabPath
                );
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(
                    root
                );
            }

            AssetDatabase.ImportAsset(
                PrefabPath,
                ImportAssetOptions.ForceUpdate
            );

            AssetDatabase.SaveAssets();

            Debug.Log(
                "[Project J/Fusion] " +
                "67일차 Network Player CapsuleCollider 구성 완료"
            );
        }

        private static void ConfigureBodyCollider(
            CapsuleCollider bodyCollider
        )
        {
            bodyCollider.direction =
                1;

            bodyCollider.radius =
                BodyColliderRadius;

            bodyCollider.height =
                StandingColliderHeight;

            bodyCollider.center =
                new Vector3(
                    0f,
                    StandingColliderHeight *
                    0.5f,
                    0f
                );

            bodyCollider.isTrigger =
                false;
        }
    }
}
#endif
