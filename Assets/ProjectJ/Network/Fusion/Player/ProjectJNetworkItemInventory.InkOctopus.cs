using Fusion; // NetworkObject와 TickTimer 사용
using ProjectJ.Items; // 먹물 문어 정책 사용
using UnityEngine; // Vector3와 GameObject 사용
using UnityEngine.UI; // 로컬 화면 Overlay 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        private const string InkOctopusProjectileResourcePath =
            "ProjectJNetworkInkOctopusProjectile";

        private NetworkObject inkOctopusProjectilePrefab;
        private Canvas inkOctopusOverlayCanvas;
        private Image inkOctopusOverlayImage;

        [Networked]
        private TickTimer NetworkInkOctopusTimer
        {
            get;
            set;
        }

        public bool IsInkOctopusActive =>
            IsTimerActive(NetworkInkOctopusTimer);

        public float InkOctopusRemaining =>
            GetRemainingTime(NetworkInkOctopusTimer);

        private void InitializeInkOctopusAuthority()
        {
            NetworkInkOctopusTimer = TickTimer.None;
        }

        private bool UseInkOctopusAuthority()
        {
            if (
                Runner == null ||
                !Runner.IsServer ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed
            )
            {
                return false;
            }

            NetworkObject projectilePrefab =
                ResolveInkOctopusProjectilePrefab();

            if (projectilePrefab == null)
            {
                Debug.LogError(
                    "[Project J/Fusion] 116일차 먹물 문어 Prefab을 찾을 수 없음",
                    this
                );

                return false;
            }

            Vector3 forward = transform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();

            Vector3 spawnPosition =
                transform.position +
                Vector3.up * 1.2f +
                forward * 0.9f;

            NetworkObject projectileObject = Runner.Spawn(
                projectilePrefab,
                spawnPosition,
                Quaternion.LookRotation(forward),
                Object.InputAuthority
            );

            if (projectileObject == null)
            {
                return false;
            }

            ProjectJNetworkInkOctopusProjectile projectile =
                projectileObject.GetComponent<ProjectJNetworkInkOctopusProjectile>();

            if (
                projectile == null ||
                !projectile.ConfigureAuthority(
                    Object.InputAuthority,
                    forward
                )
            )
            {
                Runner.Despawn(projectileObject);
                return false;
            }

            return true;
        }

        internal bool ApplyInkOctopusAuthority()
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return false;
            }

            float duration = ProjectJInkOctopusPolicy.GetRefreshedDuration(
                InkOctopusRemaining
            );

            NetworkInkOctopusTimer = TickTimer.CreateFromSeconds(
                Runner,
                duration
            );

            return true;
        }

        private void ClearInkOctopusAuthority()
        {
            NetworkInkOctopusTimer = TickTimer.None;
        }

        private NetworkObject ResolveInkOctopusProjectilePrefab()
        {
            if (inkOctopusProjectilePrefab == null)
            {
                GameObject projectilePrefabObject = Resources.Load<GameObject>(
                    InkOctopusProjectileResourcePath
                );

                inkOctopusProjectilePrefab =
                    projectilePrefabObject != null
                        ? projectilePrefabObject.GetComponent<NetworkObject>()
                        : null;
            }

            return inkOctopusProjectilePrefab;
        }

        public override void Render()
        {
            UpdateInkOctopusOverlayLocal();
        }

        private void UpdateInkOctopusOverlayLocal()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasInputAuthority
            )
            {
                DestroyInkOctopusOverlayLocal();
                return;
            }

            bool shouldShow =
                IsInkOctopusActive &&
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            if (!shouldShow)
            {
                if (inkOctopusOverlayImage != null)
                {
                    inkOctopusOverlayImage.enabled = false;
                }

                return;
            }

            EnsureInkOctopusOverlayLocal();

            if (inkOctopusOverlayImage != null)
            {
                inkOctopusOverlayImage.enabled = true;
            }
        }

        private void EnsureInkOctopusOverlayLocal()
        {
            if (
                inkOctopusOverlayCanvas != null &&
                inkOctopusOverlayImage != null
            )
            {
                return;
            }

            GameObject canvasObject = new GameObject(
                "Ink Octopus Overlay Canvas"
            );
            canvasObject.transform.SetParent(transform, false);

            inkOctopusOverlayCanvas =
                canvasObject.AddComponent<Canvas>();
            inkOctopusOverlayCanvas.renderMode =
                RenderMode.ScreenSpaceOverlay;
            inkOctopusOverlayCanvas.sortingOrder = 30000;

            CanvasScaler scaler =
                canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution =
                new Vector2(1920f, 1080f);
            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject imageObject = new GameObject(
                "Ink Overlay"
            );
            imageObject.transform.SetParent(
                canvasObject.transform,
                false
            );

            inkOctopusOverlayImage =
                imageObject.AddComponent<Image>();
            inkOctopusOverlayImage.raycastTarget = false;
            inkOctopusOverlayImage.color = new Color(
                0.015f,
                0.015f,
                0.02f,
                ProjectJInkOctopusPolicy.OverlayAlpha
            );

            RectTransform rect =
                inkOctopusOverlayImage.rectTransform;

            float horizontalMargin =
                (1f - ProjectJInkOctopusPolicy.OverlayWidthNormalized) *
                0.5f;
            float verticalMargin =
                (1f - ProjectJInkOctopusPolicy.OverlayHeightNormalized) *
                0.5f;

            rect.anchorMin = new Vector2(
                horizontalMargin,
                verticalMargin
            );
            rect.anchorMax = new Vector2(
                1f - horizontalMargin,
                1f - verticalMargin
            );
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void DestroyInkOctopusOverlayLocal()
        {
            if (inkOctopusOverlayCanvas != null)
            {
                UnityEngine.Object.Destroy(
                    inkOctopusOverlayCanvas.gameObject
                );
            }

            inkOctopusOverlayCanvas = null;
            inkOctopusOverlayImage = null;
        }
    }
}
