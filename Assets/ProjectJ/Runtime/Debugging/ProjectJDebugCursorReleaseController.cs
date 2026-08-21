using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Debugging
{
    [DisallowMultipleComponent]
    public sealed class ProjectJDebugCursorReleaseController :
        MonoBehaviour
    {
        private CursorLockMode previousLockState;
        private bool previousVisible;
        private bool isReleasedByAlt;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void Install()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectJDebugCursorReleaseController existing =
                FindFirstObjectByType<
                    ProjectJDebugCursorReleaseController
                >();

            if (existing != null)
            {
                return;
            }

            GameObject controllerObject =
                new GameObject(
                    "=== Project J Debug Cursor ==="
                );

            controllerObject.AddComponent<
                ProjectJDebugCursorReleaseController
            >();

            DontDestroyOnLoad(
                controllerObject
            );
#endif
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Keyboard keyboard =
                Keyboard.current;

            bool isAltPressed =
                keyboard != null &&
                (
                    keyboard.leftAltKey.isPressed ||
                    keyboard.rightAltKey.isPressed
                );

            if (
                isAltPressed &&
                !isReleasedByAlt
            )
            {
                ReleaseCursor();
                return;
            }

            if (
                !isAltPressed &&
                isReleasedByAlt
            )
            {
                RestoreCursor();
            }
#endif
        }

        private void OnApplicationFocus(
            bool hasFocus
        )
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (
                !hasFocus &&
                isReleasedByAlt
            )
            {
                RestoreCursor();
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (isReleasedByAlt)
            {
                RestoreCursor();
            }
#endif
        }

        private void ReleaseCursor()
        {
            previousLockState =
                Cursor.lockState;

            previousVisible =
                Cursor.visible;

            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible = true;

            isReleasedByAlt = true;
        }

        private void RestoreCursor()
        {
            Cursor.lockState =
                previousLockState;

            Cursor.visible =
                previousVisible;

            isReleasedByAlt = false;
        }
    }
}
