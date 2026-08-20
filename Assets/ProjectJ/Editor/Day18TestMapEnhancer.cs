using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day18TestMapEnhancer
    {
        private const string UnifiedRootName = "=== Day18 Unified Test Map ===";

        [MenuItem("ProjectJ/Day18/Enhance Current Test Map")]
        public static void EnhanceCurrentTestMap()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            if (!activeScene.IsValid())
            {
                Debug.LogWarning("유효한 씬이 열려 있지 않습니다.");
                return;
            }

            RemoveOldTestMapRoots(activeScene);

            Material floorMaterial = CreateMaterial(
                "Day18_Generated_Floor",
                new Color(0.78f, 0.84f, 0.90f)
            );

            Material wallMaterial = CreateMaterial(
                "Day18_Generated_Wall",
                new Color(0.30f, 0.36f, 0.44f)
            );

            Material lightMaterial = CreateMaterial(
                "Day18_Generated_Light",
                new Color(0.92f, 0.94f, 0.96f)
            );

            Material accentMaterial = CreateMaterial(
                "Day18_Generated_Accent",
                new Color(0.95f, 0.58f, 0.10f)
            );

            Material routeMaterial = CreateMaterial(
                "Day18_Generated_Route",
                new Color(0.48f, 0.57f, 0.67f)
            );

            GameObject root = new GameObject(UnifiedRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Unified Test Map");

            BuildMainFloor(root.transform, floorMaterial, wallMaterial);
            BuildZone01OpenPlaza(root.transform, lightMaterial, accentMaterial);
            BuildZone02SprintLane(root.transform, routeMaterial, wallMaterial, accentMaterial);
            BuildZone03DiagonalSlalom(root.transform, lightMaterial, accentMaterial);
            BuildZone04CollisionCorridor(root.transform, wallMaterial, lightMaterial);
            BuildZone05CrouchTunnel(root.transform, wallMaterial, accentMaterial);
            BuildZone06StandingSpaceLab(root.transform, lightMaterial, wallMaterial, accentMaterial);
            BuildZone07RampAndStairs(root.transform, lightMaterial, wallMaterial, accentMaterial);
            BuildZone08JumpBlocks(root.transform, lightMaterial, wallMaterial, accentMaterial);
            BuildZone09CentralBuilding(root.transform, lightMaterial, wallMaterial, accentMaterial);
            BuildZone10UpperDeck(root.transform, lightMaterial, wallMaterial, accentMaterial);
            BuildConnections(root.transform, routeMaterial, wallMaterial);
            BuildDecorations(root.transform, accentMaterial, wallMaterial);

            EditorSceneManager.MarkSceneDirty(activeScene);
            Selection.activeGameObject = root;

            Debug.Log(
                "Day18 통합 테스트 맵 리뉴얼 완료: Zone 01~10이 하나의 맵 안에 재배치되었습니다."
            );
        }

        private static void RemoveOldTestMapRoots(Scene scene)
        {
            string[] oldRootNames =
            {
                "=== Day11 Test Map ===",
                "Day18_MapEnhancement",
                "Day18_UnifiedTestMap",
                UnifiedRootName
            };

            GameObject[] sceneRoots = scene.GetRootGameObjects();

            for (int i = 0; i < sceneRoots.Length; i++)
            {
                GameObject root = sceneRoots[i];

                for (int j = 0; j < oldRootNames.Length; j++)
                {
                    if (root.name != oldRootNames[j])
                    {
                        continue;
                    }

                    Undo.DestroyObjectImmediate(root);
                    break;
                }
            }
        }

        private static void BuildMainFloor(
            Transform root,
            Material floorMaterial,
            Material wallMaterial
        )
        {
            Transform mapRoot = CreateRoot(
                root,
                "Map_Base",
                Vector3.zero
            );

            CreateBlock(
                mapRoot,
                "MainFloor",
                new Vector3(0f, -0.5f, 0f),
                new Vector3(60f, 1f, 60f),
                floorMaterial
            );

            CreateBlock(
                mapRoot,
                "NorthBorder_Left",
                new Vector3(-18f, 1f, 29f),
                new Vector3(24f, 2f, 1f),
                wallMaterial
            );

            CreateBlock(
                mapRoot,
                "NorthBorder_Right",
                new Vector3(18f, 1f, 29f),
                new Vector3(24f, 2f, 1f),
                wallMaterial
            );

            CreateBlock(
                mapRoot,
                "SouthBorder_Left",
                new Vector3(-18f, 1f, -29f),
                new Vector3(24f, 2f, 1f),
                wallMaterial
            );

            CreateBlock(
                mapRoot,
                "SouthBorder_Right",
                new Vector3(18f, 1f, -29f),
                new Vector3(24f, 2f, 1f),
                wallMaterial
            );

            CreateBlock(
                mapRoot,
                "WestBorder_Top",
                new Vector3(-29f, 1f, 18f),
                new Vector3(1f, 2f, 24f),
                wallMaterial
            );

            CreateBlock(
                mapRoot,
                "WestBorder_Bottom",
                new Vector3(-29f, 1f, -18f),
                new Vector3(1f, 2f, 24f),
                wallMaterial
            );

            CreateBlock(
                mapRoot,
                "EastBorder_Top",
                new Vector3(29f, 1f, 18f),
                new Vector3(1f, 2f, 24f),
                wallMaterial
            );

            CreateBlock(
                mapRoot,
                "EastBorder_Bottom",
                new Vector3(29f, 1f, -18f),
                new Vector3(1f, 2f, 24f),
                wallMaterial
            );
        }

        private static void BuildZone01OpenPlaza(
            Transform root,
            Material lightMaterial,
            Material accentMaterial
        )
        {
            Transform zone = CreateRoot(
                root,
                "Zone_01_OpenPlaza",
                new Vector3(-19f, 0f, -19f)
            );

            CreateBlock(
                zone,
                "PlazaPad",
                new Vector3(0f, 0.08f, 0f),
                new Vector3(14f, 0.16f, 14f),
                lightMaterial
            );

            CreateBlock(
                zone,
                "SpawnMarker",
                new Vector3(0f, 0.25f, 0f),
                new Vector3(4f, 0.5f, 4f),
                accentMaterial
            );

            CreateBlock(
                zone,
                "CornerMarker_A",
                new Vector3(-5f, 1f, -5f),
                new Vector3(1.5f, 2f, 1.5f),
                accentMaterial
            );

            CreateBlock(
                zone,
                "CornerMarker_B",
                new Vector3(5f, 1f, 5f),
                new Vector3(1.5f, 2f, 1.5f),
                accentMaterial
            );
        }

        private static void BuildZone02SprintLane(
            Transform root,
            Material routeMaterial,
            Material wallMaterial,
            Material accentMaterial
        )
        {
            Transform zone = CreateRoot(
                root,
                "Zone_02_SprintLane",
                new Vector3(0f, 0f, -20f)
            );

            CreateBlock(
                zone,
                "SprintLane",
                new Vector3(0f, 0.09f, 0f),
                new Vector3(20f, 0.18f, 6f),
                routeMaterial
            );

            CreateBlock(
                zone,
                "LaneWall_Left",
                new Vector3(-10f, 1f, 0f),
                new Vector3(0.6f, 2f, 6f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "LaneWall_Right",
                new Vector3(10f, 1f, 0f),
                new Vector3(0.6f, 2f, 6f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "SprintMarker",
                new Vector3(0f, 0.3f, 0f),
                new Vector3(1f, 0.6f, 5f),
                accentMaterial
            );
        }

        private static void BuildZone03DiagonalSlalom(
            Transform root,
            Material lightMaterial,
            Material accentMaterial
        )
        {
            Transform zone = CreateRoot(
                root,
                "Zone_03_DiagonalSlalom",
                new Vector3(20f, 0f, -18f)
            );

            CreateBlock(
                zone,
                "SlalomPad",
                new Vector3(0f, 0.08f, 0f),
                new Vector3(14f, 0.16f, 14f),
                lightMaterial
            );

            Vector3[] markers =
            {
                new Vector3(-4f, 1f, -4f),
                new Vector3(0f, 1f, -1f),
                new Vector3(4f, 1f, 2f),
                new Vector3(0f, 1f, 5f)
            };

            for (int i = 0; i < markers.Length; i++)
            {
                CreateBlock(
                    zone,
                    "SlalomMarker_" + i.ToString("00"),
                    markers[i],
                    new Vector3(1.5f, 2f, 1.5f),
                    accentMaterial
                );
            }
        }

        private static void BuildZone04CollisionCorridor(
            Transform root,
            Material wallMaterial,
            Material lightMaterial
        )
        {
            Transform zone = CreateRoot(
                root,
                "Zone_04_CollisionCorridor",
                new Vector3(22f, 0f, 0f)
            );

            CreateBlock(
                zone,
                "CorridorFloor",
                new Vector3(0f, 0.08f, 0f),
                new Vector3(12f, 0.16f, 16f),
                lightMaterial
            );

            CreateBlock(
                zone,
                "CorridorWall_Left",
                new Vector3(-5.5f, 2f, 0f),
                new Vector3(1f, 4f, 16f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "CorridorWall_Right",
                new Vector3(5.5f, 2f, 0f),
                new Vector3(1f, 4f, 16f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "InnerCover_A",
                new Vector3(-2f, 1f, -4f),
                new Vector3(2f, 2f, 3f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "InnerCover_B",
                new Vector3(2f, 1f, 1f),
                new Vector3(2f, 2f, 3f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "InnerCover_C",
                new Vector3(-2f, 1f, 6f),
                new Vector3(2f, 2f, 3f),
                wallMaterial
            );
        }

        private static void BuildZone05CrouchTunnel(
            Transform root,
            Material wallMaterial,
            Material accentMaterial
        )
        {
            Transform zone = CreateRoot(
                root,
                "Zone_05_CrouchTunnel",
                new Vector3(20f, 0f, 18f)
            );

            CreateBlock(
                zone,
                "TunnelFloor",
                new Vector3(0f, 0.08f, 0f),
                new Vector3(14f, 0.16f, 10f),
                accentMaterial
            );

            CreateBlock(
                zone,
                "TunnelWall_Left",
                new Vector3(-6.5f, 1.5f, 0f),
                new Vector3(1f, 3f, 10f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "TunnelWall_Right",
                new Vector3(6.5f, 1.5f, 0f),
                new Vector3(1f, 3f, 10f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "LowCeiling",
                new Vector3(0f, 1.75f, 0f),
                new Vector3(12f, 0.5f, 8f),
                wallMaterial
            );
        }

        private static void BuildZone06StandingSpaceLab(
            Transform root,
            Material lightMaterial,
            Material wallMaterial,
            Material accentMaterial
        )
        {
            Transform zone = CreateRoot(
                root,
                "Zone_06_StandingSpaceLab",
                new Vector3(0f, 0f, 20f)
            );

            CreateBlock(
                zone,
                "LabFloor",
                new Vector3(0f, 0.08f, 0f),
                new Vector3(16f, 0.16f, 12f),
                lightMaterial
            );

            CreateBlock(
                zone,
                "LowRoof",
                new Vector3(-4f, 1.75f, 0f),
                new Vector3(7f, 0.5f, 8f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "HighRoof",
                new Vector3(4f, 3.1f, 0f),
                new Vector3(7f, 0.5f, 8f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "CenterDivider",
                new Vector3(0f, 1.5f, 0f),
                new Vector3(0.5f, 3f, 8f),
                accentMaterial
            );
        }

        private static void BuildZone07RampAndStairs(
            Transform root,
            Material lightMaterial,
            Material wallMaterial,
            Material accentMaterial
        )
        {
            Transform zone = CreateRoot(
                root,
                "Zone_07_RampAndStairs",
                new Vector3(-20f, 0f, 18f)
            );

            CreateBlock(
                zone,
                "ZoneFloor",
                new Vector3(0f, 0.08f, 0f),
                new Vector3(15f, 0.16f, 14f),
                lightMaterial
            );

            CreateRamp(
                zone,
                "PlayableRamp",
                new Vector3(-2.5f, 1.1f, 0f),
                new Vector3(5f, 0.5f, 11f),
                -12f,
                wallMaterial
            );

            CreateStairSet(
                zone,
                "FutureStepTest",
                new Vector3(4f, 0.25f, -4f),
                7,
                new Vector3(2.4f, 0.5f, 1.2f),
                StairDirection.Forward,
                wallMaterial
            );

            CreateBlock(
                zone,
                "TopMarker",
                new Vector3(-2.5f, 2.1f, 5f),
                new Vector3(2f, 1f, 2f),
                accentMaterial
            );
        }

        private static void BuildZone08JumpBlocks(
            Transform root,
            Material lightMaterial,
            Material wallMaterial,
            Material accentMaterial
        )
        {
            Transform zone = CreateRoot(
                root,
                "Zone_08_JumpBlocks",
                new Vector3(-22f, 0f, 0f)
            );

            CreateBlock(
                zone,
                "JumpFloor",
                new Vector3(0f, 0.08f, 0f),
                new Vector3(12f, 0.16f, 16f),
                lightMaterial
            );

            Vector3[] positions =
            {
                new Vector3(0f, 0.75f, -5f),
                new Vector3(2.5f, 1.15f, -1.5f),
                new Vector3(-1.5f, 1.55f, 2f),
                new Vector3(2f, 2f, 5.5f)
            };

            Vector3[] scales =
            {
                new Vector3(3f, 1.5f, 3f),
                new Vector3(3f, 2.3f, 3f),
                new Vector3(3f, 3.1f, 3f),
                new Vector3(4f, 4f, 3f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                CreateBlock(
                    zone,
                    "JumpBlock_" + i.ToString("00"),
                    positions[i],
                    scales[i],
                    i == positions.Length - 1
                        ? accentMaterial
                        : wallMaterial
                );
            }
        }

        private static void BuildZone09CentralBuilding(
            Transform root,
            Material lightMaterial,
            Material wallMaterial,
            Material accentMaterial
        )
        {
            Transform zone = CreateRoot(
                root,
                "Zone_09_CentralBuilding",
                Vector3.zero
            );

            CreateBlock(
                zone,
                "BuildingFoundation",
                new Vector3(0f, 0.3f, 0f),
                new Vector3(16f, 0.6f, 16f),
                lightMaterial
            );

            CreateBlock(
                zone,
                "BackWall_Left",
                new Vector3(-5.5f, 4f, 6.5f),
                new Vector3(5f, 8f, 1f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "BackWall_Right",
                new Vector3(5.5f, 4f, 6.5f),
                new Vector3(5f, 8f, 1f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "BackWall_Header",
                new Vector3(0f, 7f, 6.5f),
                new Vector3(6f, 2f, 1f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "SideWall_Left",
                new Vector3(-7.5f, 3f, 1f),
                new Vector3(1f, 6f, 10f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "SideWall_Right",
                new Vector3(7.5f, 3f, 1f),
                new Vector3(1f, 6f, 10f),
                wallMaterial
            );

            CreateBlock(
                zone,
                "UpperFloor",
                new Vector3(0f, 4.2f, 0.5f),
                new Vector3(14f, 0.6f, 11f),
                lightMaterial
            );

            CreateRamp(
                zone,
                "BuildingRamp",
                new Vector3(-10f, 2f, 0f),
                new Vector3(8f, 0.5f, 4f),
                25f,
                wallMaterial
            );

            CreateBlock(
                zone,
                "BuildingAccent",
                new Vector3(0f, 1f, -6.5f),
                new Vector3(3f, 2f, 1f),
                accentMaterial
            );
        }

        private static void BuildZone10UpperDeck(
            Transform root,
            Material lightMaterial,
            Material wallMaterial,
            Material accentMaterial
        )
        {
            Transform zone = CreateRoot(
                root,
                "Zone_10_UpperDeck",
                new Vector3(0f, 0f, 0f)
            );

            CreateBlock(
                zone,
                "RoofDeck",
                new Vector3(0f, 8.3f, 2f),
                new Vector3(11f, 0.6f, 8f),
                lightMaterial
            );

            CreateRamp(
                zone,
                "UpperAccessRamp",
                new Vector3(9.5f, 6.25f, 2f),
                new Vector3(8f, 0.5f, 3.5f),
                -28f,
                wallMaterial
            );

            CreateBlock(
                zone,
                "GoalMarker",
                new Vector3(0f, 9.25f, 2f),
                new Vector3(3f, 1.4f, 3f),
                accentMaterial
            );

            CreateBlock(
                zone,
                "DeckWall_Back",
                new Vector3(0f, 9.2f, 5.7f),
                new Vector3(11f, 1.8f, 0.5f),
                wallMaterial
            );
        }

        private static void BuildConnections(
            Transform root,
            Material routeMaterial,
            Material wallMaterial
        )
        {
            Transform connections = CreateRoot(
                root,
                "Map_Connections",
                Vector3.zero
            );

            CreateBlock(
                connections,
                "SouthConnection_Left",
                new Vector3(-9.5f, 0.07f, -19f),
                new Vector3(7f, 0.14f, 5f),
                routeMaterial
            );

            CreateBlock(
                connections,
                "SouthConnection_Right",
                new Vector3(10f, 0.07f, -19f),
                new Vector3(7f, 0.14f, 5f),
                routeMaterial
            );

            CreateBlock(
                connections,
                "EastConnection_Bottom",
                new Vector3(21f, 0.07f, -9f),
                new Vector3(5f, 0.14f, 7f),
                routeMaterial
            );

            CreateBlock(
                connections,
                "EastConnection_Top",
                new Vector3(20f, 0.07f, 9f),
                new Vector3(5f, 0.14f, 7f),
                routeMaterial
            );

            CreateBlock(
                connections,
                "NorthConnection_Right",
                new Vector3(10f, 0.07f, 20f),
                new Vector3(7f, 0.14f, 5f),
                routeMaterial
            );

            CreateBlock(
                connections,
                "NorthConnection_Left",
                new Vector3(-10f, 0.07f, 19f),
                new Vector3(7f, 0.14f, 5f),
                routeMaterial
            );

            CreateBlock(
                connections,
                "WestConnection_Top",
                new Vector3(-21f, 0.07f, 9f),
                new Vector3(5f, 0.14f, 7f),
                routeMaterial
            );

            CreateBlock(
                connections,
                "WestConnection_Bottom",
                new Vector3(-21f, 0.07f, -9f),
                new Vector3(5f, 0.14f, 7f),
                routeMaterial
            );

            CreateBlock(
                connections,
                "CentralCross_NS",
                new Vector3(0f, 0.06f, 0f),
                new Vector3(5f, 0.12f, 38f),
                routeMaterial
            );

            CreateBlock(
                connections,
                "CentralCross_EW",
                new Vector3(0f, 0.06f, 0f),
                new Vector3(38f, 0.12f, 5f),
                routeMaterial
            );

            CreateBlock(
                connections,
                "CentralBarrier_Left",
                new Vector3(-11f, 1f, 10f),
                new Vector3(0.5f, 2f, 5f),
                wallMaterial
            );

            CreateBlock(
                connections,
                "CentralBarrier_Right",
                new Vector3(11f, 1f, -10f),
                new Vector3(0.5f, 2f, 5f),
                wallMaterial
            );
        }

        private static void BuildDecorations(
            Transform root,
            Material accentMaterial,
            Material wallMaterial
        )
        {
            Transform decorations = CreateRoot(
                root,
                "Map_Decoration",
                Vector3.zero
            );

            Vector3[] accentPositions =
            {
                new Vector3(-27f, 1f, -27f),
                new Vector3(27f, 1f, -27f),
                new Vector3(27f, 1f, 27f),
                new Vector3(-27f, 1f, 27f),
                new Vector3(-12f, 1f, 12f),
                new Vector3(12f, 1f, 12f),
                new Vector3(-12f, 1f, -12f),
                new Vector3(12f, 1f, -12f)
            };

            for (int i = 0; i < accentPositions.Length; i++)
            {
                CreateBlock(
                    decorations,
                    "Accent_" + i.ToString("00"),
                    accentPositions[i],
                    new Vector3(1.5f, 2f, 1.5f),
                    accentMaterial
                );
            }

            CreateBlock(
                decorations,
                "Tower_Left",
                new Vector3(-27f, 4f, 0f),
                new Vector3(2f, 8f, 4f),
                wallMaterial
            );

            CreateBlock(
                decorations,
                "Tower_Right",
                new Vector3(27f, 4f, 0f),
                new Vector3(2f, 8f, 4f),
                wallMaterial
            );
        }

        private static Material CreateMaterial(
            string assetName,
            Color color
        )
        {
            const string artFolder = "Assets/ProjectJ/Art";
            const string generatedFolder = "Assets/ProjectJ/Art/Generated";

            if (!AssetDatabase.IsValidFolder(artFolder))
            {
                AssetDatabase.CreateFolder(
                    "Assets/ProjectJ",
                    "Art"
                );
            }

            if (!AssetDatabase.IsValidFolder(generatedFolder))
            {
                AssetDatabase.CreateFolder(
                    artFolder,
                    "Generated"
                );
            }

            string materialPath =
                generatedFolder + "/" + assetName + ".mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    materialPath
                );

            if (material == null)
            {
                Shader shader = Shader.Find(
                    "Universal Render Pipeline/Lit"
                );

                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(
                    material,
                    materialPath
                );
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();

            return material;
        }

        private static Transform CreateRoot(
            Transform parent,
            string name,
            Vector3 localPosition
        )
        {
            GameObject root = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(
                root,
                "Create Map Root"
            );

            root.transform.SetParent(parent);
            root.transform.localPosition = localPosition;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

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
                "Create Block"
            );

            block.name = name;
            block.transform.SetParent(parent);
            block.transform.localPosition = localPosition;
            block.transform.localRotation = Quaternion.identity;
            block.transform.localScale = localScale;

            int worldLayer =
                LayerMask.NameToLayer("World");

            if (worldLayer >= 0)
            {
                block.layer = worldLayer;
            }

            Renderer renderer =
                block.GetComponent<Renderer>();

            if (
                renderer != null &&
                material != null
            )
            {
                renderer.sharedMaterial = material;
            }

            return block;
        }

        private static void CreateRamp(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            float xRotation,
            Material material
        )
        {
            GameObject ramp = CreateBlock(
                parent,
                name,
                localPosition,
                localScale,
                material
            );

            ramp.transform.localRotation =
                Quaternion.Euler(
                    xRotation,
                    0f,
                    0f
                );
        }

        private static void CreateStairSet(
            Transform parent,
            string name,
            Vector3 startPosition,
            int stepCount,
            Vector3 stepScale,
            StairDirection direction,
            Material material
        )
        {
            Transform stairsRoot =
                CreateRoot(
                    parent,
                    name,
                    Vector3.zero
                );

            for (int i = 0; i < stepCount; i++)
            {
                Vector3 offset =
                    GetStairOffset(
                        stepScale,
                        direction,
                        i
                    );

                CreateBlock(
                    stairsRoot,
                    "Step_" + i.ToString("00"),
                    startPosition + offset,
                    stepScale,
                    material
                );
            }
        }

        private static Vector3 GetStairOffset(
            Vector3 stepScale,
            StairDirection direction,
            int stepIndex
        )
        {
            Vector3 offset = Vector3.zero;
            offset.y =
                stepScale.y * stepIndex;

            switch (direction)
            {
                case StairDirection.Forward:
                    offset.z =
                        stepScale.z * stepIndex;
                    break;

                case StairDirection.Backward:
                    offset.z =
                        -stepScale.z * stepIndex;
                    break;

                case StairDirection.Right:
                    offset.x =
                        stepScale.x * stepIndex;
                    break;

                case StairDirection.Left:
                    offset.x =
                        -stepScale.x * stepIndex;
                    break;
            }

            return offset;
        }

        private enum StairDirection
        {
            Forward,
            Backward,
            Right,
            Left
        }
    }
}
