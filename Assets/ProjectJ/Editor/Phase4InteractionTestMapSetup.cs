using System;
using ProjectJ.Checkpoint;
using ProjectJ.Finish;
using ProjectJ.Player;
using ProjectJ.Push;
using ProjectJ.Ranking;
using ProjectJ.Results;
using ProjectJ.Tests.Manual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Phase4InteractionTestMapSetup
    {
        private const string SourceScenePath =
            "Assets/ProjectJ/Tests/Manual/Day37/" +
            "Day37_SpectatorTest.unity";

        private const string Phase4Folder =
            "Assets/ProjectJ/Tests/Manual/Phase4";

        private const string MaterialFolder =
            Phase4Folder + "/Materials";

        private const string Phase4ScenePath =
            Phase4Folder +
            "/Phase4_InteractionTest.unity";

        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/" +
            "Player.prefab";

        private const string Phase4RootName =
            "=== Phase4 Interaction Test Area ===";

        private const string OldSpectatorTargetsName =
            "=== Spectator Test Targets ===";

        private const float BridgeLength = 6f;
        private const float BridgeWidth = 4f;
        private const float AnnexWidth = 32f;
        private const float AnnexDepth = 24f;
        private const float FloorThickness = 0.2f;
        private const float WallThickness = 0.25f;
        private const float WallHeight = 3f;

        [MenuItem(
            "ProjectJ/Phase4/Build Interaction Test Map"
        )]
        public static void BuildInteractionTestMap()
        {
            bool canContinue =
                EditorSceneManager
                    .SaveCurrentModifiedScenesIfUserWantsTo();

            if (!canContinue)
            {
                return;
            }

            EnsureFolder(
                Phase4Folder
            );

            EnsureFolder(
                MaterialFolder
            );

            SceneAsset sourceScene =
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    SourceScenePath
                );

            if (sourceScene == null)
            {
                Debug.LogError(
                    "Phase4 테스트맵의 원본 Day37 Scene을 " +
                    "찾을 수 없습니다: " +
                    SourceScenePath
                );

                return;
            }

            if (
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    Phase4ScenePath
                ) != null
            )
            {
                AssetDatabase.DeleteAsset(
                    Phase4ScenePath
                );
            }

            bool copied =
                AssetDatabase.CopyAsset(
                    SourceScenePath,
                    Phase4ScenePath
                );

            if (!copied)
            {
                Debug.LogError(
                    "Phase4 테스트 Scene 복사에 실패했습니다."
                );

                return;
            }

            AssetDatabase.Refresh();

            Scene scene =
                EditorSceneManager.OpenScene(
                    Phase4ScenePath,
                    OpenSceneMode.Single
                );

            Transform localPlayer =
                FindLocalPlayer();

            if (localPlayer == null)
            {
                Debug.LogError(
                    "Phase4 테스트맵에 사용할 Local Player를 " +
                    "찾을 수 없습니다."
                );

                return;
            }

            BoxCollider sourceFloor =
                FindFloorUnderPlayer(
                    localPlayer
                );

            if (sourceFloor == null)
            {
                Debug.LogError(
                    "Local Player 아래의 기존 테스트 바닥을 " +
                    "찾을 수 없습니다."
                );

                return;
            }

            RemoveObjectIfExists(
                Phase4RootName
            );

            RemoveObjectIfExists(
                OldSpectatorTargetsName
            );

            Material floorMaterial =
                LoadOrCreateMaterial(
                    MaterialFolder +
                    "/Phase4_Floor.mat",
                    new Color(
                        0.23f,
                        0.32f,
                        0.42f,
                        1f
                    )
                );

            Material wallMaterial =
                LoadOrCreateMaterial(
                    MaterialFolder +
                    "/Phase4_Wall.mat",
                    new Color(
                        0.18f,
                        0.20f,
                        0.24f,
                        1f
                    )
                );

            Material markerMaterial =
                LoadOrCreateMaterial(
                    MaterialFolder +
                    "/Phase4_Marker.mat",
                    new Color(
                        0.35f,
                        0.75f,
                        0.95f,
                        1f
                    )
                );

            Material dangerMaterial =
                LoadOrCreateMaterial(
                    MaterialFolder +
                    "/Phase4_Danger.mat",
                    new Color(
                        0.88f,
                        0.34f,
                        0.25f,
                        1f
                    )
                );

            BuildArea(
                scene,
                localPlayer,
                sourceFloor,
                floorMaterial,
                wallMaterial,
                markerMaterial,
                dangerMaterial
            );

            PrepareLocalPlayer(
                localPlayer.gameObject
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameObject phase4Root =
                GameObject.Find(
                    Phase4RootName
                );

            Selection.activeGameObject =
                phase4Root;

            Debug.Log(
                "Phase4 상호작용 테스트맵 생성 완료: " +
                Phase4ScenePath
            );
        }

        private static void BuildArea(
            Scene scene,
            Transform localPlayer,
            BoxCollider sourceFloor,
            Material floorMaterial,
            Material wallMaterial,
            Material markerMaterial,
            Material dangerMaterial
        )
        {
            GameObject root =
                new GameObject(
                    Phase4RootName
                );

            SceneManager.MoveGameObjectToScene(
                root,
                scene
            );

            Bounds floorBounds =
                sourceFloor.bounds;

            float oldFloorTopY =
                floorBounds.max.y;

            float playerRootOffset =
                Mathf.Clamp(
                    localPlayer.position.y -
                    oldFloorTopY,
                    0.5f,
                    2f
                );

            float targetRootY =
                oldFloorTopY +
                playerRootOffset;

            float oldEastEdge =
                floorBounds.max.x;

            float annexWest =
                oldEastEdge +
                BridgeLength;

            float annexCenterX =
                annexWest +
                AnnexWidth * 0.5f;

            float annexCenterZ =
                localPlayer.position.z;

            float newFloorCenterY =
                oldFloorTopY -
                FloorThickness * 0.5f;

            bool removedWall =
                RemoveEastConnectorWall(
                    sourceFloor,
                    localPlayer.position.z
                );

            if (!removedWall)
            {
                Debug.LogWarning(
                    "기존 Day37 Scene에서 연결부에 해당하는 " +
                    "물리 벽을 찾지 못했습니다. " +
                    "새 구간은 기존 바닥의 동쪽 경계에서 " +
                    "바로 연결되도록 생성했습니다."
                );
            }

            GameObject bridge =
                CreateCube(
                    "Phase4_Connector_Bridge",
                    new Vector3(
                        oldEastEdge +
                        BridgeLength * 0.5f,
                        newFloorCenterY,
                        annexCenterZ
                    ),
                    new Vector3(
                        BridgeLength,
                        FloorThickness,
                        BridgeWidth
                    ),
                    floorMaterial,
                    WorldLayer()
                );

            bridge.transform.SetParent(
                root.transform,
                true
            );

            GameObject floor =
                CreateCube(
                    "Phase4_Main_Floor",
                    new Vector3(
                        annexCenterX,
                        newFloorCenterY,
                        annexCenterZ
                    ),
                    new Vector3(
                        AnnexWidth,
                        FloorThickness,
                        AnnexDepth
                    ),
                    floorMaterial,
                    WorldLayer()
                );

            floor.transform.SetParent(
                root.transform,
                true
            );

            BuildOuterWalls(
                root.transform,
                annexWest,
                annexCenterX,
                annexCenterZ,
                oldFloorTopY,
                wallMaterial
            );

            CreateLabel(
                root.transform,
                "PHASE 4 TEST AREA",
                new Vector3(
                    annexWest + 1.5f,
                    oldFloorTopY + 2.5f,
                    annexCenterZ + 2.5f
                ),
                0.42f
            );

            BuildBridgeMarkers(
                root.transform,
                oldEastEdge,
                annexWest,
                annexCenterZ,
                oldFloorTopY,
                markerMaterial
            );

            BuildPassThroughLane(
                scene,
                root.transform,
                annexCenterX,
                annexCenterZ,
                targetRootY,
                oldFloorTopY,
                markerMaterial
            );

            BuildNearestTargetLane(
                scene,
                root.transform,
                annexCenterX,
                annexCenterZ,
                targetRootY,
                oldFloorTopY,
                markerMaterial
            );

            BuildCooldownLane(
                scene,
                root.transform,
                annexCenterX,
                annexCenterZ,
                targetRootY,
                oldFloorTopY,
                markerMaterial
            );

            BuildProtectedLane(
                scene,
                root.transform,
                annexCenterX,
                annexCenterZ,
                targetRootY,
                oldFloorTopY,
                dangerMaterial
            );

            BuildPushEdgeDeck(
                scene,
                root.transform,
                annexCenterX,
                annexCenterZ,
                targetRootY,
                oldFloorTopY,
                floorMaterial,
                dangerMaterial
            );
        }

        private static void BuildOuterWalls(
            Transform parent,
            float annexWest,
            float annexCenterX,
            float annexCenterZ,
            float floorTopY,
            Material wallMaterial
        )
        {
            float east =
                annexWest +
                AnnexWidth;

            float south =
                annexCenterZ -
                AnnexDepth * 0.5f;

            float north =
                annexCenterZ +
                AnnexDepth * 0.5f;

            float wallCenterY =
                floorTopY +
                WallHeight * 0.5f;

            CreateWall(
                parent,
                "Phase4_East_Wall",
                new Vector3(
                    east,
                    wallCenterY,
                    annexCenterZ
                ),
                new Vector3(
                    WallThickness,
                    WallHeight,
                    AnnexDepth
                ),
                wallMaterial
            );

            CreateWall(
                parent,
                "Phase4_South_Wall",
                new Vector3(
                    annexCenterX,
                    wallCenterY,
                    south
                ),
                new Vector3(
                    AnnexWidth,
                    WallHeight,
                    WallThickness
                ),
                wallMaterial
            );

            float westGapHalf =
                BridgeWidth * 0.5f;

            float westSegmentDepth =
                (
                    AnnexDepth -
                    BridgeWidth
                ) * 0.5f;

            CreateWall(
                parent,
                "Phase4_West_Wall_South",
                new Vector3(
                    annexWest,
                    wallCenterY,
                    annexCenterZ -
                    westGapHalf -
                    westSegmentDepth * 0.5f
                ),
                new Vector3(
                    WallThickness,
                    WallHeight,
                    westSegmentDepth
                ),
                wallMaterial
            );

            CreateWall(
                parent,
                "Phase4_West_Wall_North",
                new Vector3(
                    annexWest,
                    wallCenterY,
                    annexCenterZ +
                    westGapHalf +
                    westSegmentDepth * 0.5f
                ),
                new Vector3(
                    WallThickness,
                    WallHeight,
                    westSegmentDepth
                ),
                wallMaterial
            );

            float deckCenterX =
                annexCenterX +
                8f;

            float deckGapWidth =
                8f;

            float deckGapMin =
                deckCenterX -
                deckGapWidth * 0.5f;

            float deckGapMax =
                deckCenterX +
                deckGapWidth * 0.5f;

            float leftLength =
                deckGapMin -
                annexWest;

            if (leftLength > 0.5f)
            {
                CreateWall(
                    parent,
                    "Phase4_North_Wall_Left",
                    new Vector3(
                        annexWest +
                        leftLength * 0.5f,
                        wallCenterY,
                        north
                    ),
                    new Vector3(
                        leftLength,
                        WallHeight,
                        WallThickness
                    ),
                    wallMaterial
                );
            }

            float rightLength =
                east -
                deckGapMax;

            if (rightLength > 0.5f)
            {
                CreateWall(
                    parent,
                    "Phase4_North_Wall_Right",
                    new Vector3(
                        deckGapMax +
                        rightLength * 0.5f,
                        wallCenterY,
                        north
                    ),
                    new Vector3(
                        rightLength,
                        WallHeight,
                        WallThickness
                    ),
                    wallMaterial
                );
            }
        }

        private static void BuildBridgeMarkers(
            Transform parent,
            float oldEastEdge,
            float annexWest,
            float centerZ,
            float floorTopY,
            Material markerMaterial
        )
        {
            for (int i = 0; i < 3; i++)
            {
                float t =
                    (i + 1f) / 4f;

                CreateVisualMarker(
                    parent,
                    "Phase4_BridgeMarker_" +
                    (i + 1),
                    new Vector3(
                        Mathf.Lerp(
                            oldEastEdge,
                            annexWest,
                            t
                        ),
                        floorTopY + 0.02f,
                        centerZ
                    ),
                    new Vector3(
                        0.9f,
                        0.04f,
                        1.4f
                    ),
                    markerMaterial
                );
            }
        }

        private static void BuildPassThroughLane(
            Scene scene,
            Transform parent,
            float centerX,
            float centerZ,
            float targetRootY,
            float floorTopY,
            Material markerMaterial
        )
        {
            float laneX =
                centerX - 10f;

            float startZ =
                centerZ - 8f;

            CreateLabel(
                parent,
                "PASS THROUGH",
                new Vector3(
                    laneX,
                    floorTopY + 1.8f,
                    startZ - 1f
                ),
                0.28f
            );

            CreateVisualMarker(
                parent,
                "PassThrough_Start",
                new Vector3(
                    laneX,
                    floorTopY + 0.02f,
                    startZ
                ),
                new Vector3(
                    2f,
                    0.04f,
                    1.2f
                ),
                markerMaterial
            );

            CreateDummyPlayer(
                scene,
                parent,
                "Phase4_PassTarget_A",
                new Vector3(
                    laneX,
                    targetRootY,
                    startZ + 3f
                ),
                false
            );

            CreateDummyPlayer(
                scene,
                parent,
                "Phase4_PassTarget_B",
                new Vector3(
                    laneX,
                    targetRootY,
                    startZ + 5.5f
                ),
                false
            );
        }

        private static void BuildNearestTargetLane(
            Scene scene,
            Transform parent,
            float centerX,
            float centerZ,
            float targetRootY,
            float floorTopY,
            Material markerMaterial
        )
        {
            float laneX =
                centerX;

            float startZ =
                centerZ - 8f;

            CreateLabel(
                parent,
                "NEAREST TARGET",
                new Vector3(
                    laneX,
                    floorTopY + 1.8f,
                    startZ - 1f
                ),
                0.28f
            );

            CreateVisualMarker(
                parent,
                "NearestTarget_Start",
                new Vector3(
                    laneX,
                    floorTopY + 0.02f,
                    startZ
                ),
                new Vector3(
                    2f,
                    0.04f,
                    1.2f
                ),
                markerMaterial
            );

            CreateDummyPlayer(
                scene,
                parent,
                "Phase4_Target_Near",
                new Vector3(
                    laneX,
                    targetRootY,
                    startZ + 1.5f
                ),
                false
            );

            CreateDummyPlayer(
                scene,
                parent,
                "Phase4_Target_Far",
                new Vector3(
                    laneX,
                    targetRootY,
                    startZ + 2.25f
                ),
                false
            );

            CreateDummyPlayer(
                scene,
                parent,
                "Phase4_Target_AngleOutside",
                new Vector3(
                    laneX + 2.1f,
                    targetRootY,
                    startZ + 1.2f
                ),
                false
            );

            CreateDummyPlayer(
                scene,
                parent,
                "Phase4_Target_RangeOutside",
                new Vector3(
                    laneX,
                    targetRootY,
                    startZ + 3.5f
                ),
                false
            );
        }

        private static void BuildCooldownLane(
            Scene scene,
            Transform parent,
            float centerX,
            float centerZ,
            float targetRootY,
            float floorTopY,
            Material markerMaterial
        )
        {
            float laneX =
                centerX + 10f;

            float startZ =
                centerZ - 8f;

            CreateLabel(
                parent,
                "PUSH / COOLDOWN",
                new Vector3(
                    laneX,
                    floorTopY + 1.8f,
                    startZ - 1f
                ),
                0.28f
            );

            CreateVisualMarker(
                parent,
                "PushCooldown_Start",
                new Vector3(
                    laneX,
                    floorTopY + 0.02f,
                    startZ
                ),
                new Vector3(
                    2f,
                    0.04f,
                    1.2f
                ),
                markerMaterial
            );

            CreateDummyPlayer(
                scene,
                parent,
                "Phase4_PushCooldown_Target",
                new Vector3(
                    laneX,
                    targetRootY,
                    startZ + 1.6f
                ),
                false
            );
        }

        private static void BuildProtectedLane(
            Scene scene,
            Transform parent,
            float centerX,
            float centerZ,
            float targetRootY,
            float floorTopY,
            Material dangerMaterial
        )
        {
            float laneX =
                centerX - 8f;

            float startZ =
                centerZ + 4f;

            CreateLabel(
                parent,
                "RESPAWN PROTECTED",
                new Vector3(
                    laneX,
                    floorTopY + 1.8f,
                    startZ - 1f
                ),
                0.26f
            );

            CreateVisualMarker(
                parent,
                "ProtectedTarget_Start",
                new Vector3(
                    laneX,
                    floorTopY + 0.02f,
                    startZ
                ),
                new Vector3(
                    2f,
                    0.04f,
                    1.2f
                ),
                dangerMaterial
            );

            CreateDummyPlayer(
                scene,
                parent,
                "Phase4_Protected_Target",
                new Vector3(
                    laneX,
                    targetRootY,
                    startZ + 1.6f
                ),
                true
            );
        }

        private static void BuildPushEdgeDeck(
            Scene scene,
            Transform parent,
            float centerX,
            float centerZ,
            float targetRootY,
            float floorTopY,
            Material floorMaterial,
            Material dangerMaterial
        )
        {
            float deckCenterX =
                centerX + 8f;

            float mainNorth =
                centerZ +
                AnnexDepth * 0.5f;

            float deckDepth =
                6f;

            float deckCenterZ =
                mainNorth +
                deckDepth * 0.5f;

            GameObject deck =
                CreateCube(
                    "Phase4_PushEdge_Deck",
                    new Vector3(
                        deckCenterX,
                        floorTopY -
                        FloorThickness * 0.5f,
                        deckCenterZ
                    ),
                    new Vector3(
                        8f,
                        FloorThickness,
                        deckDepth
                    ),
                    floorMaterial,
                    WorldLayer()
                );

            deck.transform.SetParent(
                parent,
                true
            );

            CreateLabel(
                parent,
                "PUSH EDGE / DROP",
                new Vector3(
                    deckCenterX,
                    floorTopY + 1.8f,
                    mainNorth + 0.7f
                ),
                0.28f
            );

            CreateVisualMarker(
                parent,
                "PushEdge_Start",
                new Vector3(
                    deckCenterX,
                    floorTopY + 0.02f,
                    mainNorth + 1.3f
                ),
                new Vector3(
                    2f,
                    0.04f,
                    1.2f
                ),
                dangerMaterial
            );

            CreateDummyPlayer(
                scene,
                parent,
                "Phase4_PushEdge_Target",
                new Vector3(
                    deckCenterX,
                    targetRootY,
                    mainNorth + 3.2f
                ),
                false
            );

            CreateVisualMarker(
                parent,
                "PushEdge_DangerLine",
                new Vector3(
                    deckCenterX,
                    floorTopY + 0.025f,
                    mainNorth + deckDepth - 0.25f
                ),
                new Vector3(
                    7.5f,
                    0.05f,
                    0.25f
                ),
                dangerMaterial
            );
        }

        private static GameObject CreateDummyPlayer(
            Scene scene,
            Transform parent,
            string objectName,
            Vector3 position,
            bool alwaysProtected
        )
        {
            GameObject playerPrefab =
                AssetDatabase.LoadAssetAtPath<
                    GameObject
                >(
                    PlayerPrefabPath
                );

            if (playerPrefab == null)
            {
                Debug.LogError(
                    "Player.prefab을 찾을 수 없습니다: " +
                    PlayerPrefabPath
                );

                return null;
            }

            GameObject dummy =
                PrefabUtility.InstantiatePrefab(
                    playerPrefab,
                    scene
                ) as GameObject;

            if (dummy == null)
            {
                return null;
            }

            dummy.name =
                objectName;

            dummy.transform.SetParent(
                parent,
                true
            );

            dummy.transform.position =
                position;

            dummy.transform.rotation =
                Quaternion.identity;

            PlayerInput input =
                dummy.GetComponent<
                    PlayerInput
                >();

            if (input != null)
            {
                input.enabled =
                    false;
            }

            PlayerCameraRelativeMovement movement =
                dummy.GetComponent<
                    PlayerCameraRelativeMovement
                >();

            if (movement != null)
            {
                movement.enabled =
                    false;
            }

            PlayerLedgeClimber ledgeClimber =
                dummy.GetComponent<
                    PlayerLedgeClimber
                >();

            if (ledgeClimber != null)
            {
                ledgeClimber.enabled =
                    false;
            }

            PlayerRankingParticipant ranking =
                dummy.GetComponent<
                    PlayerRankingParticipant
                >();

            if (ranking != null)
            {
                ranking.enabled =
                    false;
            }

            PlayerHeightTracker heightTracker =
                dummy.GetComponent<
                    PlayerHeightTracker
                >();

            if (heightTracker != null)
            {
                heightTracker.enabled =
                    false;
            }

            PlayerMatchResultCollector resultCollector =
                dummy.GetComponent<
                    PlayerMatchResultCollector
                >();

            if (resultCollector != null)
            {
                resultCollector.enabled =
                    false;
            }

            PlayerFallTracker fallTracker =
                dummy.GetComponent<
                    PlayerFallTracker
                >();

            if (fallTracker != null)
            {
                fallTracker.enabled =
                    false;
            }

            PlayerRespawnController respawnController =
                dummy.GetComponent<
                    PlayerRespawnController
                >();

            if (respawnController != null)
            {
                respawnController.enabled =
                    false;
            }

            PlayerRespawnProtection protection =
                dummy.GetComponent<
                    PlayerRespawnProtection
                >();

            if (protection != null)
            {
                protection.enabled =
                    alwaysProtected;
            }

            Rigidbody body =
                dummy.GetComponent<
                    Rigidbody
                >();

            if (body != null)
            {
                body.isKinematic =
                    false;

                body.useGravity =
                    true;

                body.linearVelocity =
                    Vector3.zero;

                body.angularVelocity =
                    Vector3.zero;
            }

            AddComponentByRuntimeType(
                dummy,
                "ProjectJ.Push.PlayerPushReceiver"
            );

            if (
                alwaysProtected &&
                protection != null
            )
            {
                Phase4ProtectedTargetLoop loop =
                    dummy.GetComponent<
                        Phase4ProtectedTargetLoop
                    >();

                if (loop == null)
                {
                    loop =
                        dummy.AddComponent<
                            Phase4ProtectedTargetLoop
                        >();
                }

                loop.Configure(
                    protection
                );
            }

            return dummy;
        }

        private static void PrepareLocalPlayer(
            GameObject localPlayer
        )
        {
            if (localPlayer == null)
            {
                return;
            }

            AddComponentByRuntimeType(
                localPlayer,
                "ProjectJ.Push.PlayerPushReceiver"
            );

            AddComponentByRuntimeType(
                localPlayer,
                "ProjectJ.Push.PlayerPushController"
            );

            PlayerPushTargetSelector selector =
                localPlayer.GetComponent<
                    PlayerPushTargetSelector
                >();

            if (selector == null)
            {
                selector =
                    localPlayer.AddComponent<
                        PlayerPushTargetSelector
                    >();
            }

            EditorUtility.SetDirty(
                localPlayer
            );
        }

        private static Component AddComponentByRuntimeType(
            GameObject target,
            string fullTypeName
        )
        {
            Type type =
                Type.GetType(
                    fullTypeName +
                    ", ProjectJ.Runtime"
                );

            if (type == null)
            {
                Debug.LogWarning(
                    "아직 구현되지 않은 Runtime Type을 " +
                    "찾지 못했습니다: " +
                    fullTypeName
                );

                return null;
            }

            Component existing =
                target.GetComponent(
                    type
                );

            if (existing != null)
            {
                return existing;
            }

            return
                target.AddComponent(
                    type
                );
        }

        private static Transform FindLocalPlayer()
        {
            PlayerMatchResultCollector[] collectors =
                UnityEngine.Object
                    .FindObjectsByType<
                        PlayerMatchResultCollector
                    >(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None
                    );

            for (
                int i = 0;
                i < collectors.Length;
                i++
            )
            {
                PlayerMatchResultCollector collector =
                    collectors[i];

                if (collector == null)
                {
                    continue;
                }

                string objectName =
                    collector.gameObject.name;

                if (
                    objectName.StartsWith(
                        "SpectatorDummy_"
                    ) ||
                    objectName.StartsWith(
                        "Phase4_"
                    )
                )
                {
                    continue;
                }

                return
                    collector.transform;
            }

            PlayerCameraRelativeMovement movement =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        PlayerCameraRelativeMovement
                    >(
                        FindObjectsInactive.Include
                    );

            return
                movement != null
                    ? movement.transform
                    : null;
        }

        private static BoxCollider FindFloorUnderPlayer(
            Transform player
        )
        {
            BoxCollider[] colliders =
                UnityEngine.Object
                    .FindObjectsByType<
                        BoxCollider
                    >(
                        FindObjectsInactive.Exclude,
                        FindObjectsSortMode.None
                    );

            BoxCollider best =
                null;

            float bestTopY =
                float.NegativeInfinity;

            Vector3 position =
                player.position;

            for (
                int i = 0;
                i < colliders.Length;
                i++
            )
            {
                BoxCollider collider =
                    colliders[i];

                if (
                    collider == null ||
                    collider.isTrigger
                )
                {
                    continue;
                }

                Bounds bounds =
                    collider.bounds;

                bool containsXZ =
                    position.x >=
                        bounds.min.x - 0.05f &&
                    position.x <=
                        bounds.max.x + 0.05f &&
                    position.z >=
                        bounds.min.z - 0.05f &&
                    position.z <=
                        bounds.max.z + 0.05f;

                if (!containsXZ)
                {
                    continue;
                }

                if (
                    bounds.max.y >
                    position.y + 0.25f
                )
                {
                    continue;
                }

                float verticalGap =
                    position.y -
                    bounds.max.y;

                if (
                    verticalGap < 0f ||
                    verticalGap > 2.5f
                )
                {
                    continue;
                }

                if (
                    bounds.size.x < 2f ||
                    bounds.size.z < 2f
                )
                {
                    continue;
                }

                if (
                    bounds.max.y >
                    bestTopY
                )
                {
                    best =
                        collider;

                    bestTopY =
                        bounds.max.y;
                }
            }

            return best;
        }

        private static bool RemoveEastConnectorWall(
            BoxCollider sourceFloor,
            float playerZ
        )
        {
            Bounds floorBounds =
                sourceFloor.bounds;

            BoxCollider[] colliders =
                UnityEngine.Object
                    .FindObjectsByType<
                        BoxCollider
                    >(
                        FindObjectsInactive.Exclude,
                        FindObjectsSortMode.None
                    );

            BoxCollider best =
                null;

            float bestScore =
                float.PositiveInfinity;

            for (
                int i = 0;
                i < colliders.Length;
                i++
            )
            {
                BoxCollider collider =
                    colliders[i];

                if (
                    collider == null ||
                    collider == sourceFloor ||
                    collider.isTrigger
                )
                {
                    continue;
                }

                if (
                    collider.GetComponent<
                        Renderer
                    >() == null
                )
                {
                    continue;
                }

                Bounds bounds =
                    collider.bounds;

                bool wallShape =
                    bounds.size.x <= 1.5f &&
                    bounds.size.y >= 1.5f &&
                    bounds.size.z >= 1.5f;

                if (!wallShape)
                {
                    continue;
                }

                float xDistance =
                    Mathf.Abs(
                        bounds.center.x -
                        floorBounds.max.x
                    );

                if (xDistance > 1.5f)
                {
                    continue;
                }

                float zDistance =
                    Mathf.Abs(
                        bounds.center.z -
                        playerZ
                    );

                if (
                    zDistance >
                    BridgeWidth
                )
                {
                    continue;
                }

                float score =
                    xDistance +
                    zDistance * 0.25f;

                if (
                    score <
                    bestScore
                )
                {
                    best =
                        collider;

                    bestScore =
                        score;
                }
            }

            if (best == null)
            {
                return false;
            }

            Debug.Log(
                "Phase4 연결을 위해 기존 벽 제거: " +
                best.gameObject.name
            );

            UnityEngine.Object
                .DestroyImmediate(
                    best.gameObject
                );

            return true;
        }

        private static void CreateWall(
            Transform parent,
            string objectName,
            Vector3 position,
            Vector3 scale,
            Material material
        )
        {
            GameObject wall =
                CreateCube(
                    objectName,
                    position,
                    scale,
                    material,
                    WorldLayer()
                );

            wall.transform.SetParent(
                parent,
                true
            );
        }

        private static GameObject CreateCube(
            string objectName,
            Vector3 position,
            Vector3 scale,
            Material material,
            int layer
        )
        {
            GameObject cube =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            cube.name =
                objectName;

            cube.transform.position =
                position;

            cube.transform.localScale =
                scale;

            cube.layer =
                layer;

            Renderer renderer =
                cube.GetComponent<
                    Renderer
                >();

            if (
                renderer != null &&
                material != null
            )
            {
                renderer.sharedMaterial =
                    material;
            }

            return cube;
        }

        private static void CreateVisualMarker(
            Transform parent,
            string objectName,
            Vector3 position,
            Vector3 scale,
            Material material
        )
        {
            GameObject marker =
                CreateCube(
                    objectName,
                    position,
                    scale,
                    material,
                    0
                );

            marker.transform.SetParent(
                parent,
                true
            );

            Collider collider =
                marker.GetComponent<
                    Collider
                >();

            if (collider != null)
            {
                UnityEngine.Object
                    .DestroyImmediate(
                        collider
                    );
            }
        }

        private static void CreateLabel(
            Transform parent,
            string text,
            Vector3 position,
            float characterSize
        )
        {
            GameObject labelObject =
                new GameObject(
                    "Label_" +
                    text.Replace(
                        " ",
                        "_"
                    )
                );

            labelObject.transform.SetParent(
                parent,
                true
            );

            labelObject.transform.position =
                position;

            labelObject.transform.rotation =
                Quaternion.Euler(
                    0f,
                    180f,
                    0f
                );

            TextMesh textMesh =
                labelObject.AddComponent<
                    TextMesh
                >();

            textMesh.text =
                text;

            textMesh.anchor =
                TextAnchor.MiddleCenter;

            textMesh.alignment =
                TextAlignment.Center;

            textMesh.characterSize =
                characterSize;

            textMesh.fontSize =
                48;
        }

        private static Material LoadOrCreateMaterial(
            string assetPath,
            Color color
        )
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<
                    Material
                >(
                    assetPath
                );

            if (material == null)
            {
                Shader shader =
                    Shader.Find(
                        "Universal Render Pipeline/Lit"
                    );

                if (shader == null)
                {
                    shader =
                        Shader.Find(
                            "Standard"
                        );
                }

                material =
                    new Material(
                        shader
                    );

                AssetDatabase.CreateAsset(
                    material,
                    assetPath
                );
            }

            material.color =
                color;

            EditorUtility.SetDirty(
                material
            );

            return material;
        }

        private static int WorldLayer()
        {
            int worldLayer =
                LayerMask.NameToLayer(
                    "World"
                );

            if (worldLayer >= 0)
            {
                return worldLayer;
            }

            return 9;
        }

        private static void RemoveObjectIfExists(
            string objectName
        )
        {
            GameObject existing =
                GameObject.Find(
                    objectName
                );

            if (existing == null)
            {
                return;
            }

            UnityEngine.Object
                .DestroyImmediate(
                    existing
                );
        }

        private static void EnsureFolder(
            string folderPath
        )
        {
            string[] parts =
                folderPath.Split(
                    '/'
                );

            string current =
                parts[0];

            for (
                int i = 1;
                i < parts.Length;
                i++
            )
            {
                string next =
                    current +
                    "/" +
                    parts[i];

                if (
                    !AssetDatabase.IsValidFolder(
                        next
                    )
                )
                {
                    AssetDatabase.CreateFolder(
                        current,
                        parts[i]
                    );
                }

                current =
                    next;
            }
        }
    }
}
