using ProjectJ.CameraSystem;
using ProjectJ.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day23CameraPolishSetup
    {
        private const string RigName =
            "=== Day22 Camera Rig ===";

        private const string PivotName =
            "CameraPivot";

        [MenuItem("ProjectJ/Day23/Apply Camera Zoom And Sprint FOV")]
        public static void ApplyCameraZoomAndSprintFov()
        {
            GameObject rig =
                GameObject.Find(
                    RigName
                );

            if (rig == null)
            {
                Debug.LogWarning(
                    "Day22 Camera Rig를 찾을 수 없습니다. 먼저 Day22 Camera Setup을 실행하세요."
                );

                return;
            }

            PlayerThirdPersonCamera controller =
                rig.GetComponent<PlayerThirdPersonCamera>();

            if (controller == null)
            {
                Debug.LogWarning(
                    "Camera Rig에 PlayerThirdPersonCamera가 없습니다."
                );

                return;
            }

            PlayerCameraRelativeMovement playerMovement =
                Object.FindFirstObjectByType<PlayerCameraRelativeMovement>();

            if (playerMovement == null)
            {
                Debug.LogWarning(
                    "현재 Scene에서 Player를 찾을 수 없습니다."
                );

                return;
            }

            PlayerInput playerInput =
                playerMovement.GetComponent<PlayerInput>();

            Transform pivot =
                rig.transform.Find(
                    PivotName
                );

            Camera mainCamera =
                Camera.main;

            if (
                playerInput == null ||
                pivot == null ||
                mainCamera == null
            )
            {
                Debug.LogWarning(
                    "PlayerInput, CameraPivot 또는 Main Camera 연결을 확인하세요."
                );

                return;
            }

            SerializedObject serializedController =
                new SerializedObject(
                    controller
                );

            SetFloat(
                serializedController,
                "cameraDistance",
                7.5f
            );

            SetFloat(
                serializedController,
                "minimumCameraDistance",
                3.5f
            );

            SetFloat(
                serializedController,
                "maximumCameraDistance",
                10f
            );

            SetFloat(
                serializedController,
                "zoomStep",
                0.75f
            );

            SetFloat(
                serializedController,
                "collisionRadius",
                0.25f
            );

            SetFloat(
                serializedController,
                "collisionPadding",
                0.15f
            );

            SetFloat(
                serializedController,
                "cameraReturnSpeed",
                12f
            );

            SetFloat(
                serializedController,
                "normalFov",
                60f
            );

            SetFloat(
                serializedController,
                "sprintFov",
                68f
            );

            SetFloat(
                serializedController,
                "fovChangeSpeed",
                8f
            );

            SerializedProperty collisionLayers =
                serializedController.FindProperty(
                    "collisionLayers"
                );

            if (collisionLayers != null)
            {
                collisionLayers.intValue =
                    LayerMask.GetMask(
                        "World",
                        "Obstacle"
                    );
            }

            serializedController.ApplyModifiedProperties();

            controller.Configure(
                playerMovement.transform,
                playerInput,
                pivot,
                mainCamera
            );

            EditorUtility.SetDirty(
                controller
            );

            EditorUtility.SetDirty(
                mainCamera
            );

            Scene activeScene =
                SceneManager.GetActiveScene();

            EditorSceneManager.MarkSceneDirty(
                activeScene
            );

            Selection.activeGameObject =
                rig;

            Debug.Log(
                "Day23 Camera Zoom / Sprint FOV 설정 완료."
            );
        }

        private static void SetFloat(
            SerializedObject serializedObject,
            string propertyName,
            float value
        )
        {
            SerializedProperty property =
                serializedObject.FindProperty(
                    propertyName
                );

            if (property != null)
            {
                property.floatValue =
                    value;
            }
        }
    }
}
