using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day20LedgeMapSetup
    {
        private const string UnifiedMapRoot =
            "=== Day18 Unified Test Map ===";

        private const string ZoneName =
            "Zone_08_JumpBlocks";

        private const string Day20RootName =
            "Day20_LedgeDetectTests";

        [MenuItem("ProjectJ/Day20/Upgrade Ledge Detect Test Zone")]
        public static void UpgradeLedgeDetectTestZone()
        {
            GameObject mapRoot =
                GameObject.Find(
                    UnifiedMapRoot
                );

            if (mapRoot == null)
            {
                Debug.LogWarning(
                    "먼저 통합 테스트 맵을 생성하세요."
                );

                return;
            }

            Transform zone =
                FindChildRecursive(
                    mapRoot.transform,
                    ZoneName
                );

            if (zone == null)
            {
                Debug.LogWarning(
                    "Zone_08_JumpBlocks를 찾을 수 없습니다."
                );

                return;
            }

            Transform previous =
                zone.Find(
                    Day20RootName
                );

            if (previous != null)
            {
                Undo.DestroyObjectImmediate(
                    previous.gameObject
                );
            }

            Transform root =
                CreateRoot(
                    zone,
                    Day20RootName
                );

            Material wallMaterial =
                AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/ProjectJ/Art/Generated/Day18_Generated_Wall.mat"
                );

            Material accentMaterial =
                AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/ProjectJ/Art/Generated/Day18_Generated_Accent.mat"
                );

            Material lightMaterial =
                AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/ProjectJ/Art/Generated/Day18_Generated_Light.mat"
                );

            CreateHeightSamples(
                root,
                wallMaterial,
                accentMaterial
            );

            CreateBlockedHeadroomSample(
                root,
                wallMaterial,
                accentMaterial
            );

            CreateSteepTopSample(
                root,
                lightMaterial,
                accentMaterial
            );

            Scene scene =
                SceneManager.GetActiveScene();

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            Selection.activeGameObject =
                root.gameObject;

            Debug.Log(
                "Day20 Ledge Detect 테스트 구조를 Zone 08에 추가했습니다."
            );
        }

        private static void CreateHeightSamples(
            Transform parent,
            Material wallMaterial,
            Material accentMaterial
        )
        {
            float[] heights =
            {
                0.3f,
                0.7f,
                1.0f,
                1.4f,
                1.7f
            };

            for (
                int i = 0;
                i < heights.Length;
                i++
            )
            {
                float height =
                    heights[i];

                bool invalidHeight =
                    height < 0.45f ||
                    height > 1.4f;

                CreateBlock(
                    parent,
                    "Ledge_" +
                        height.ToString("0.0"),
                    new Vector3(
                        -4.5f + i * 2.3f,
                        0.16f + height * 0.5f,
                        -5.5f
                    ),
                    new Vector3(
                        1.8f,
                        height,
                        2.5f
                    ),
                    invalidHeight
                        ? accentMaterial
                        : wallMaterial
                );
            }
        }

        private static void CreateBlockedHeadroomSample(
            Transform parent,
            Material wallMaterial,
            Material accentMaterial
        )
        {
            Transform root =
                CreateRoot(
                    parent,
                    "BlockedHeadroom"
                );

            const float ledgeHeight =
                1.0f;

            CreateBlock(
                root,
                "Ledge_1.0",
                new Vector3(
                    -3f,
                    0.16f +
                        ledgeHeight * 0.5f,
                    4.5f
                ),
                new Vector3(
                    3f,
                    ledgeHeight,
                    2.5f
                ),
                wallMaterial
            );

            CreateBlock(
                root,
                "LowCeiling",
                new Vector3(
                    -3f,
                    2.35f,
                    4.8f
                ),
                new Vector3(
                    3f,
                    0.5f,
                    2.5f
                ),
                accentMaterial
            );
        }

        private static void CreateSteepTopSample(
            Transform parent,
            Material lightMaterial,
            Material accentMaterial
        )
        {
            GameObject steepTop =
                CreateBlock(
                    parent,
                    "SteepTop_60deg",
                    new Vector3(
                        4.2f,
                        1.0f,
                        4.5f
                    ),
                    new Vector3(
                        2.5f,
                        0.45f,
                        3f
                    ),
                    accentMaterial
                );

            steepTop.transform.localRotation =
                Quaternion.Euler(
                    60f,
                    0f,
                    0f
                );

            CreateBlock(
                parent,
                "SteepTopBase",
                new Vector3(
                    4.2f,
                    0.55f,
                    4.5f
                ),
                new Vector3(
                    2.5f,
                    1.1f,
                    3f
                ),
                lightMaterial
            );
        }

        private static Transform FindChildRecursive(
            Transform parent,
            string targetName
        )
        {
            if (parent.name == targetName)
            {
                return parent;
            }

            for (
                int i = 0;
                i < parent.childCount;
                i++
            )
            {
                Transform result =
                    FindChildRecursive(
                        parent.GetChild(i),
                        targetName
                    );

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static Transform CreateRoot(
            Transform parent,
            string name
        )
        {
            GameObject root =
                new GameObject(name);

            Undo.RegisterCreatedObjectUndo(
                root,
                "Create Day20 Test Root"
            );

            root.transform.SetParent(
                parent
            );

            root.transform.localPosition =
                Vector3.zero;

            root.transform.localRotation =
                Quaternion.identity;

            root.transform.localScale =
                Vector3.one;

            return root.transform;
        }

        private static GameObject CreateBlock(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material
        )
        {
            GameObject block =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            Undo.RegisterCreatedObjectUndo(
                block,
                "Create Day20 Test Block"
            );

            block.name = name;

            block.transform.SetParent(
                parent
            );

            block.transform.localPosition =
                localPosition;

            block.transform.localRotation =
                Quaternion.identity;

            block.transform.localScale =
                localScale;

            int worldLayer =
                LayerMask.NameToLayer(
                    "World"
                );

            if (worldLayer >= 0)
            {
                block.layer =
                    worldLayer;
            }

            Renderer renderer =
                block.GetComponent<Renderer>();

            if (
                renderer != null &&
                material != null
            )
            {
                renderer.sharedMaterial =
                    material;
            }

            return block;
        }
    }
}
