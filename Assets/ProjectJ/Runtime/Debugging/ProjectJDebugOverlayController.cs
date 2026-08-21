using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Debugging
{
    public sealed class ProjectJDebugOverlayController :
        MonoBehaviour
    {
        public static bool IsVisible
        {
            get;
            private set;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration
        )]
        private static void ResetState()
        {
            IsVisible = false;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void CreateController()
        {
            ProjectJDebugOverlayController existing =
                FindFirstObjectByType<
                    ProjectJDebugOverlayController
                >();

            if (existing != null)
            {
                return;
            }

            GameObject controllerObject =
                new GameObject(
                    "=== Project J Debug Overlay ==="
                );

            controllerObject.AddComponent<
                ProjectJDebugOverlayController
            >();

            DontDestroyOnLoad(
                controllerObject
            );
        }

        private void Update()
        {
            Keyboard keyboard =
                Keyboard.current;

            if (
                keyboard == null ||
                !keyboard.f1Key.wasPressedThisFrame
            )
            {
                return;
            }

            IsVisible =
                !IsVisible;
        }
    }
}
