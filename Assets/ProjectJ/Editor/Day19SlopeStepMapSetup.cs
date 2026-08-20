using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day19SlopeStepMapSetup
    {
        private const string UnifiedMapRoot =
            "=== Day18 Unified Test Map ===";

        private const string ZoneName =
            "Zone_07_RampAndStairs";

        private const string Day19RootName =
            "Day19_SlopeStepTests";

        [MenuItem("ProjectJ/Day19/Upgrade Slope Step Test Zone")]
        public static void UpgradeSlopeStepTestZone()
        {
            GameObject mapRoot =
                GameObject.Find(UnifiedMapRoot);

            if (mapRoot == null)
            {
                Debug.LogWarning(
                    "먼저 ProjectJ > Day18 > Enhance Current Test Map을 실행하세요."
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
                    "Zone_07_RampAndStairs를 찾을 수 없습니다."
                );

                return;
            }

            Transform previous =
                zone.Find(Day19RootName);

            if (previous != null)
            {
                Undo.DestroyObjectImmediate(
                    previous.gameObject
                );
            }

            Transform day19Root =
                CreateRoot(
                    zone,
                    Day19RootName
                );

            Material wallMaterial =
                AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/ProjectJ/Art/Generated/Day18_Generated_Wall.mat"
                );

            Material accentMaterial =
                AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/ProjectJ/Art/Generated/Day18_Generated_Accent.mat"
                );

            CreateStepCourse(
                day19Root,
                wallMaterial,
                accentMaterial
            );

            CreateSlopeMarkers(
                day19Root,
                wallMaterial,
                accentMaterial
            );

            Scene scene =
                SceneManager.GetActiveScene();

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            Selection.activeGameObject =
                day19Root.gameObject;

            Debug.Log(
                "Day19 Slope / Step 테스트 구역을 Zone 07에 추가했습니다."
            );
        }

        private static void CreateStepCourse(
            Transform parent,
            Material wallMaterial,
            Material accentMaterial
        )
        {
            Transform root =
                CreateRoot(
                    parent,
                    "StepCourse"
                );

            float[] topHeights =
            {
                0.2f,
                0.4f,
                0.7f,
                1.1f,
                1.7f
            };

            for (
                int i = 0;
                i < topHeights.Length;
                i++
            )
            {
                float topHeight =
                    topHeights[i];

                bool isBlockedStep =
                    i == topHeights.Length - 1;

                CreateBlock(
                    root,
                    isBlockedStep
                        ? "Step_04_Block_0.6"
                        : "Step_" +
                            i.ToString("00"),
                    new Vector3(
                        4.4f,
                        topHeight * 0.5f,
                        -4f + i * 1.5f
                    ),
                    new Vector3(
                        2.2f,
                        topHeight,
                        1.5f
                    ),
                    isBlockedStep
                        ? accentMaterial
                        : wallMaterial
                );
            }
        }

        private static void CreateSlopeMarkers(
            Transform parent,
            Material wallMaterial,
            Material accentMaterial
        )
        {
            Transform root =
                CreateRoot(
                    parent,
                    "SlopeMarkers"
                );

            GameObject walkableRamp =
                CreateBlock(
                    root,
                    "WalkableSlope_30deg",
                    new Vector3(
                        -5.2f,
                        1.1f,
                        3.2f
                    ),
                    new Vector3(
                        3f,
                        0.5f,
                        5f
                    ),
                    wallMaterial
                );

            walkableRamp.transform.localRotation =
                Quaternion.Euler(
                    -30f,
                    0f,
                    0f
                );

            GameObject steepRamp =
                CreateBlock(
                    root,
                    "BlockedSlope_55deg",
                    new Vector3(
                        5.2f,
                        1.1f,
                        4.2f
                    ),
                    new Vector3(
                        3f,
                        0.5f,
                        4f
                    ),
                    accentMaterial
                );

            steepRamp.transform.localRotation =
                Quaternion.Euler(
                    -55f,
                    0f,
                    0f
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
                "Create Day19 Test Root"
            );

            root.transform.SetParent(parent);
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
                "Create Day19 Test Block"
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
