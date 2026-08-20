using ProjectJ.CameraSystem;
using ProjectJ.Checkpoint;
using ProjectJ.Finish;
using ProjectJ.Player;
using ProjectJ.Ranking;
using ProjectJ.Results;
using ProjectJ.Spectator;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day37SpectatorSetup
    {
        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/" +
            "Player.prefab";

        private const string Day36ScenePath =
            "Assets/ProjectJ/Tests/Manual/Day36/" +
            "Day36_PersonalResultTest.unity";

        private const string Day37Folder =
            "Assets/ProjectJ/Tests/Manual/Day37";

        private const string Day37ScenePath =
            Day37Folder +
            "/Day37_SpectatorTest.unity";

        private const string GameplayCameraRigName =
            "=== Day37 Gameplay Camera Rig ===";

        private const string SpectatorManagerName =
            "=== Spectator Manager ===";

        private const string SpectatorCameraName =
            "=== Spectator Camera Rig ===";

        private const string DummyRootName =
            "=== Spectator Test Targets ===";

        private const string PitchPivotName =
            "CameraPivot";

        private const string MainCameraName =
            "Main Camera";

        [MenuItem(
            "ProjectJ/Day37/Setup Basic Spectator"
        )]
        public static void SetupBasicSpectator()
        {
            bool canContinue =
                EditorSceneManager
                    .SaveCurrentModifiedScenesIfUserWantsTo();

            if (!canContinue)
            {
                return;
            }

            EnsureFolder(
                Day37Folder
            );

            CreateManualTestScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Day37 기본 관전 전환 설정 완료."
            );
        }

        private static void CreateManualTestScene()
        {
            if (
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    Day36ScenePath
                ) == null
            )
            {
                Debug.LogError(
                    "Day36 테스트 Scene을 찾을 수 없습니다: " +
                    Day36ScenePath
                );

                return;
            }

            if (
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    Day37ScenePath
                ) != null
            )
            {
                AssetDatabase.DeleteAsset(
                    Day37ScenePath
                );
            }

            bool copied =
                AssetDatabase.CopyAsset(
                    Day36ScenePath,
                    Day37ScenePath
                );

            if (!copied)
            {
                Debug.LogError(
                    "Day37 테스트 Scene 복사에 실패했습니다."
                );

                return;
            }

            AssetDatabase.Refresh();

            Scene scene =
                EditorSceneManager.OpenScene(
                    Day37ScenePath,
                    OpenSceneMode.Single
                );

            PlayerMatchResultCollector localCollector =
                FindLocalCollector();

            if (localCollector == null)
            {
                Debug.LogError(
                    "Day37 설정에 필요한 Local Player를 " +
                    "찾을 수 없습니다."
                );

                return;
            }

            PlayerInput localInput =
                localCollector.GetComponent<
                    PlayerInput
                >();

            PlayerCameraRelativeMovement localMovement =
                localCollector.GetComponent<
                    PlayerCameraRelativeMovement
                >();

            if (
                localInput == null ||
                localMovement == null
            )
            {
                Debug.LogError(
                    "Local Player에서 PlayerInput 또는 " +
                    "PlayerCameraRelativeMovement를 찾을 수 없습니다."
                );

                return;
            }

            PlayerThirdPersonCamera gameplayRig =
                FindOrCreateGameplayCameraRig(
                    localCollector.transform,
                    localInput,
                    out Camera gameplayCamera
                );

            if (
                gameplayRig == null ||
                gameplayCamera == null
            )
            {
                Debug.LogError(
                    "Day37 Gameplay Camera Rig 생성에 실패했습니다."
                );

                return;
            }

            RemoveExistingObject(
                SpectatorManagerName
            );

            RemoveExistingObject(
                SpectatorCameraName
            );

            RemoveExistingObject(
                DummyRootName
            );

            PlayerThirdPersonCamera spectatorRig =
                CreateSpectatorCameraRig(
                    gameplayRig,
                    localCollector.transform,
                    localInput
                );

            if (spectatorRig == null)
            {
                return;
            }

            Camera spectatorCamera =
                spectatorRig
                    .GetComponentInChildren<
                        Camera
                    >(
                        true
                    );

            Transform spectatorPitchPivot =
                spectatorCamera != null
                    ? spectatorCamera
                        .transform
                        .parent
                    : null;

            if (
                spectatorCamera == null ||
                spectatorPitchPivot == null
            )
            {
                Debug.LogError(
                    "Spectator Camera Rig 구조 생성에 실패했습니다."
                );

                return;
            }

            CreateSpectatorTargets(
                scene,
                localCollector.transform
            );

            GameObject managerObject =
                new GameObject(
                    SpectatorManagerName
                );

            SpectatorController controller =
                managerObject.AddComponent<
                    SpectatorController
                >();

            controller.Configure(
                gameplayRig,
                gameplayCamera,
                spectatorRig,
                spectatorPitchPivot,
                spectatorCamera,
                localInput,
                localCollector,
                localCollector.FinishState,
                localMovement
            );

            SpectatorDebugView debugView =
                managerObject.AddComponent<
                    SpectatorDebugView
                >();

            debugView.Configure(
                controller
            );

            EditorUtility.SetDirty(
                controller
            );

            EditorUtility.SetDirty(
                debugView
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            );

            Selection.activeGameObject =
                managerObject;
        }

        private static PlayerMatchResultCollector
            FindLocalCollector()
        {
            PlayerMatchResultCollector[] collectors =
                Object.FindObjectsByType<
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
                if (
                    collectors[i] == null ||
                    collectors[i]
                        .gameObject
                        .name
                        .StartsWith(
                            "SpectatorDummy_"
                        )
                )
                {
                    continue;
                }

                return collectors[i];
            }

            return null;
        }

        private static PlayerThirdPersonCamera
            FindOrCreateGameplayCameraRig(
                Transform localTarget,
                PlayerInput localInput,
                out Camera gameplayCamera
            )
        {
            gameplayCamera = null;

            PlayerThirdPersonCamera[] existingRigs =
                Object.FindObjectsByType<
                    PlayerThirdPersonCamera
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < existingRigs.Length;
                i++
            )
            {
                PlayerThirdPersonCamera existingRig =
                    existingRigs[i];

                if (existingRig == null)
                {
                    continue;
                }

                Camera existingCamera =
                    existingRig
                        .GetComponentInChildren<
                            Camera
                        >(
                            true
                        );

                if (existingCamera == null)
                {
                    continue;
                }

                Transform existingPivot =
                    existingCamera
                        .transform
                        .parent;

                if (existingPivot == null)
                {
                    continue;
                }

                existingRig.gameObject
                    .SetActive(
                        true
                    );

                existingCamera.gameObject
                    .SetActive(
                        true
                    );

                existingRig.enabled =
                    true;

                existingCamera.enabled =
                    true;

                TryAssignMainCameraTag(
                    existingCamera
                );

                existingRig.Configure(
                    localTarget,
                    localInput,
                    existingPivot,
                    existingCamera
                );

                gameplayCamera =
                    existingCamera;

                return existingRig;
            }

            GameObject existingRigObject =
                GameObject.Find(
                    GameplayCameraRigName
                );

            if (existingRigObject != null)
            {
                Object.DestroyImmediate(
                    existingRigObject
                );
            }

            GameObject rigObject =
                new GameObject(
                    GameplayCameraRigName
                );

            Transform pitchPivot =
                new GameObject(
                    PitchPivotName
                ).transform;

            pitchPivot.SetParent(
                rigObject.transform,
                false
            );

            Camera cameraToUse =
                FindExistingSceneCamera();

            if (cameraToUse == null)
            {
                GameObject cameraObject =
                    new GameObject(
                        MainCameraName
                    );

                cameraToUse =
                    cameraObject.AddComponent<
                        Camera
                    >();

                if (
                    Object.FindObjectsByType<
                        AudioListener
                    >(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None
                    ).Length == 0
                )
                {
                    cameraObject.AddComponent<
                        AudioListener
                    >();
                }
            }

            cameraToUse.gameObject.SetActive(
                true
            );

            cameraToUse.transform.SetParent(
                pitchPivot,
                false
            );

            cameraToUse.transform.localPosition =
                new Vector3(
                    0f,
                    0f,
                    -7.5f
                );

            cameraToUse.transform.localRotation =
                Quaternion.identity;

            cameraToUse.enabled =
                true;

            TryAssignMainCameraTag(
                cameraToUse
            );

            PlayerThirdPersonCamera newRig =
                rigObject.AddComponent<
                    PlayerThirdPersonCamera
                >();

            newRig.Configure(
                localTarget,
                localInput,
                pitchPivot,
                cameraToUse
            );

            newRig.enabled =
                true;

            gameplayCamera =
                cameraToUse;

            EditorUtility.SetDirty(
                newRig
            );

            EditorUtility.SetDirty(
                cameraToUse
            );

            return newRig;
        }

        private static Camera FindExistingSceneCamera()
        {
            Camera mainCamera =
                Camera.main;

            if (mainCamera != null)
            {
                return mainCamera;
            }

            Camera[] cameras =
                Object.FindObjectsByType<
                    Camera
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            if (cameras.Length > 0)
            {
                return cameras[0];
            }

            return null;
        }

        private static void TryAssignMainCameraTag(
            Camera targetCamera
        )
        {
            if (targetCamera == null)
            {
                return;
            }

            Camera currentMain =
                Camera.main;

            if (
                currentMain == null ||
                currentMain ==
                    targetCamera
            )
            {
                targetCamera.gameObject.tag =
                    "MainCamera";
            }
        }

        private static PlayerThirdPersonCamera
            CreateSpectatorCameraRig(
                PlayerThirdPersonCamera gameplayRig,
                Transform localTarget,
                PlayerInput localInput
            )
        {
            GameObject cloned =
                Object.Instantiate(
                    gameplayRig.gameObject
                );

            cloned.name =
                SpectatorCameraName;

            PlayerThirdPersonCamera spectatorRig =
                cloned.GetComponent<
                    PlayerThirdPersonCamera
                >();

            Camera spectatorCamera =
                cloned.GetComponentInChildren<
                    Camera
                >(
                    true
                );

            if (
                spectatorRig == null ||
                spectatorCamera == null
            )
            {
                Object.DestroyImmediate(
                    cloned
                );

                Debug.LogError(
                    "기존 3인칭 Camera Rig 복제에 실패했습니다."
                );

                return null;
            }

            spectatorCamera.gameObject.tag =
                "Untagged";

            AudioListener spectatorListener =
                spectatorCamera.GetComponent<
                    AudioListener
                >();

            if (spectatorListener != null)
            {
                Object.DestroyImmediate(
                    spectatorListener
                );
            }

            Transform pitchPivot =
                spectatorCamera
                    .transform
                    .parent;

            spectatorRig.Configure(
                localTarget,
                localInput,
                pitchPivot,
                spectatorCamera
            );

            spectatorRig.enabled =
                false;

            spectatorCamera.enabled =
                false;

            return spectatorRig;
        }

        private static void CreateSpectatorTargets(
            Scene scene,
            Transform localPlayer
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

                return;
            }

            GameObject root =
                new GameObject(
                    DummyRootName
                );

            CreateDummyPlayer(
                scene,
                playerPrefab,
                root.transform,
                "SpectatorDummy_B",
                localPlayer.position +
                    new Vector3(
                        6f,
                        0f,
                        10f
                    )
            );

            CreateDummyPlayer(
                scene,
                playerPrefab,
                root.transform,
                "SpectatorDummy_C",
                localPlayer.position +
                    new Vector3(
                        -6f,
                        0f,
                        15f
                    )
            );
        }

        private static void CreateDummyPlayer(
            Scene scene,
            GameObject playerPrefab,
            Transform parent,
            string objectName,
            Vector3 position
        )
        {
            GameObject dummy =
                PrefabUtility.InstantiatePrefab(
                    playerPrefab,
                    scene
                ) as GameObject;

            if (dummy == null)
            {
                return;
            }

            dummy.name =
                objectName;

            dummy.transform.SetParent(
                parent,
                true
            );

            dummy.transform.position =
                position;

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

            PlayerRankingParticipant ranking =
                dummy.GetComponent<
                    PlayerRankingParticipant
                >();

            if (ranking != null)
            {
                ranking.enabled =
                    false;
            }

            PlayerHeightTracker height =
                dummy.GetComponent<
                    PlayerHeightTracker
                >();

            if (height != null)
            {
                height.enabled =
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

            PlayerRespawnController respawn =
                dummy.GetComponent<
                    PlayerRespawnController
                >();

            if (respawn != null)
            {
                respawn.enabled =
                    false;
            }

            PlayerRespawnProtection protection =
                dummy.GetComponent<
                    PlayerRespawnProtection
                >();

            if (protection != null)
            {
                protection.enabled =
                    false;
            }

            Rigidbody body =
                dummy.GetComponent<
                    Rigidbody
                >();

            if (body != null)
            {
                body.linearVelocity =
                    Vector3.zero;

                body.angularVelocity =
                    Vector3.zero;

                body.isKinematic =
                    true;
            }
        }

        private static void RemoveExistingObject(
            string objectName
        )
        {
            GameObject existing =
                GameObject.Find(
                    objectName
                );

            if (existing != null)
            {
                Object.DestroyImmediate(
                    existing
                );
            }
        }

        private static void EnsureFolder(
            string fullPath
        )
        {
            string[] parts =
                fullPath.Split('/');

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

                current = next;
            }
        }
    }
}
