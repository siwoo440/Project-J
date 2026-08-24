using UnityEngine;

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

        public static void SetVisible( // 통합 패널의 레거시 표시 상태 적용
            bool isVisible // 적용할 표시 상태
        )
        {
            IsVisible =
                isVisible; // 전달된 표시 상태 저장
        }
    }
}
