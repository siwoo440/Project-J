using ProjectJ.Items; // 저격 물총 Zoom 정책 사용
using UnityEngine; // Camera와 GUI 사용
using UnityEngine.InputSystem; // 마우스 휠 사용

namespace ProjectJ.Networking.Fusion
{
    [DefaultExecutionOrder(1100)] // 기본 카메라 LateUpdate 이후 저격 FOV 적용
    [DisallowMultipleComponent]
    public sealed class ProjectJSniperWaterGunLocalPresentation :
        MonoBehaviour
    {
        private ProjectJNetworkPlayer networkPlayer;
        private ProjectJNetworkItemInventory itemInventory;
        private Camera gameplayCamera;
        private bool localAimPresentationActive;
        private float baseFieldOfView = 60f;
        private int zoomMultiplier =
            ProjectJSniperWaterGunPolicy.Zoom2X;

        private void Awake()
        {
            ResolveReferences();
        }

        private void LateUpdate()
        {
            ResolveReferences();

            bool shouldPresent =
                networkPlayer != null &&
                networkPlayer.HasLocalInputAuthority &&
                itemInventory != null &&
                itemInventory.IsSniperWaterGunAiming;

            if (!shouldPresent)
            {
                RestoreCameraIfNeeded();
                return;
            }

            ResolveCamera();

            if (gameplayCamera == null)
            {
                return;
            }

            if (!localAimPresentationActive)
            {
                localAimPresentationActive =
                    true;

                baseFieldOfView =
                    Mathf.Clamp(
                        gameplayCamera.fieldOfView,
                        1f,
                        179f
                    );

                zoomMultiplier =
                    ProjectJSniperWaterGunPolicy.Zoom2X;
            }

            UpdateZoomMultiplier();

            gameplayCamera.fieldOfView =
                ProjectJSniperWaterGunPolicy.CalculateZoomedFieldOfView(
                    baseFieldOfView,
                    zoomMultiplier
                );
        }

        private void OnGUI()
        {
            if (
                !localAimPresentationActive ||
                itemInventory == null
            )
            {
                return;
            }

            float centerX =
                Screen.width *
                0.5f;

            float centerY =
                Screen.height *
                0.5f;

            GUI.Label(
                new Rect(
                    centerX - 8f,
                    centerY - 12f,
                    32f,
                    24f
                ),
                "+"
            );

            int progressPercent =
                Mathf.RoundToInt(
                    itemInventory.SniperWaterGunPreparationProgress01 *
                    100f
                );

            GUI.Label(
                new Rect(
                    centerX - 70f,
                    centerY + 24f,
                    180f,
                    24f
                ),
                "AIM " +
                progressPercent +
                "%  /  " +
                zoomMultiplier +
                "x"
            );
        }

        private void ResolveReferences()
        {
            if (networkPlayer == null)
            {
                networkPlayer =
                    GetComponent<ProjectJNetworkPlayer>();
            }

            if (itemInventory == null)
            {
                itemInventory =
                    GetComponent<ProjectJNetworkItemInventory>();
            }
        }

        private void ResolveCamera()
        {
            if (
                gameplayCamera != null &&
                gameplayCamera.enabled
            )
            {
                return;
            }

            gameplayCamera =
                Camera.main;
        }

        private void UpdateZoomMultiplier()
        {
            if (Mouse.current == null)
            {
                return;
            }

            float scrollY =
                Mouse.current.scroll.ReadValue().y;

            zoomMultiplier =
                ProjectJSniperWaterGunPolicy.ResolveZoomMultiplier(
                    zoomMultiplier,
                    scrollY
                );
        }

        private void RestoreCameraIfNeeded()
        {
            if (!localAimPresentationActive)
            {
                return;
            }

            ResolveCamera();

            if (gameplayCamera != null)
            {
                gameplayCamera.fieldOfView =
                    baseFieldOfView;
            }

            localAimPresentationActive =
                false;

            zoomMultiplier =
                ProjectJSniperWaterGunPolicy.Zoom2X;
        }

        private void OnDisable()
        {
            RestoreCameraIfNeeded();
        }

        private void OnDestroy()
        {
            RestoreCameraIfNeeded();
        }

        public static Vector3 ResolveLocalAimDirection()
        {
            Camera mainCamera =
                Camera.main;

            if (mainCamera == null)
            {
                return Vector3.forward;
            }

            return
                ProjectJSniperWaterGunPolicy.ResolveAimDirection(
                    mainCamera.transform.forward,
                    Vector3.forward
                );
        }

        public static bool ShouldReserveScrollForSniper(
            ProjectJNetworkPlayer player
        )
        {
            if (
                player == null ||
                !player.HasLocalInputAuthority
            )
            {
                return false;
            }

            ProjectJNetworkItemInventory inventory =
                player.GetComponent<ProjectJNetworkItemInventory>();

            return
                inventory != null &&
                inventory.IsSniperWaterGunAiming;
        }
    }
}
