using ProjectJ.CameraSystem;
using ProjectJ.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day22CameraSetup
    {
        private const string RigName =
            "=== Day22 Camera Rig ===";

        private const string PivotName =
            "CameraPivot";

        [MenuItem("ProjectJ/Day22/Setup Third Person Camera")]
        public static void SetupThirdPersonCamera()
        {
            PlayerCameraRelativeMovement playerMovement =
                Object.FindFirstObjectByType<PlayerCameraRelativeMovement>();

            if (playerMovement == null)
            {
                Debug.LogWarning(
                    "현재 Scene에서 PlayerCameraRelativeMovement를 찾을 수 없습니다."
                );

                return;
            }

            PlayerInput playerInput =
                playerMovement.GetComponent<PlayerInput>();

            if (playerInput == null)
            {
                Debug.LogWarning(
                    "Player에 PlayerInput이 없습니다."
                );

                return;
            }

            Camera mainCamera =
                Camera.main;

            if (mainCamera == null)
            {
                mainCamera =
                    Object.FindFirstObjectByType<Camera>();
            }

            if (mainCamera == null)
            {
                Debug.LogWarning(
                    "현재 Scene에서 Camera를 찾을 수 없습니다."
                );

                return;
            }

            GameObject rig =
                GameObject.Find(
                    RigName
                );

            if (rig == null)
            {
                rig =
                    new GameObject(
                        RigName
                    );

                Undo.RegisterCreatedObjectUndo(
                    rig,
                    "Create Day22 Camera Rig"
                );
            }

            Transform pivot =
                rig.transform.Find(
                    PivotName
                );

            if (pivot == null)
            {
                GameObject pivotObject =
                    new GameObject(
                        PivotName
                    );

                Undo.RegisterCreatedObjectUndo(
                    pivotObject,
                    "Create Camera Pivot"
                );

                pivot =
                    pivotObject.transform;

                pivot.SetParent(
                    rig.transform,
                    false
                );
            }

            Undo.SetTransformParent(
                mainCamera.transform,
                pivot,
                "Parent Main Camera To Pivot"
            );

            mainCamera.transform.localPosition =
                new Vector3(
                    0f,
                    0f,
                    -7.5f
                );

            mainCamera.transform.localRotation =
                Quaternion.identity;

            rig.transform.position =
                playerMovement.transform.position +
                Vector3.up * 1.5f;

            rig.transform.rotation =
                Quaternion.identity;

            pivot.localPosition =
                Vector3.zero;

            pivot.localRotation =
                Quaternion.identity;

            PlayerThirdPersonCamera controller =
                rig.GetComponent<PlayerThirdPersonCamera>();

            if (controller == null)
            {
                controller =
                    Undo.AddComponent<PlayerThirdPersonCamera>(
                        rig
                    );
            }

            controller.Configure(
                playerMovement.transform,
                playerInput,
                pivot,
                mainCamera
            );

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
                "Day22 카메라 거리 및 벽 가림 처리 설정 완료."
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
