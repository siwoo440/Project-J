using System.Collections.Generic; // 활성 연막과 Player 목록 사용
using Fusion; // NetworkBehaviour, PlayerRef, TickTimer 사용
using ProjectJ.Items; // 연막탄 정책 사용
using UnityEngine; // 월드 시각화와 거리 계산 사용
using UnityEngine.Rendering; // 그림자 비활성화 사용
using UnityEngine.UI; // 로컬 화면 Overlay 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public sealed class ProjectJNetworkSmokeZone :
        NetworkBehaviour
    {
        private static readonly HashSet<ProjectJNetworkSmokeZone> ActiveZones =
            new HashSet<ProjectJNetworkSmokeZone>();

        private static readonly List<ProjectJNetworkExternalGameplay> OverlayPlayerBuffer =
            new List<ProjectJNetworkExternalGameplay>(8);

        private static readonly List<ProjectJNetworkExternalGameplay> GameplayPlayerBuffer =
            new List<ProjectJNetworkExternalGameplay>(8);

        private static int NextSpawnOrder;

        private static Canvas localOverlayCanvas;
        private static Image localOverlayImage;
        private static Transform localOverlayParent;

        private GameObject worldVisualObject;
        private Material worldVisualMaterial;

        [Networked]
        private NetworkBool NetworkInitialized
        {
            get;
            set;
        }

        [Networked]
        private PlayerRef NetworkOwner
        {
            get;
            set;
        }

        [Networked]
        private TickTimer NetworkLifetimeTimer
        {
            get;
            set;
        }

        [Networked]
        private int NetworkSpawnOrder
        {
            get;
            set;
        }

        public bool IsInitialized =>
            NetworkInitialized;

        public PlayerRef Owner =>
            NetworkOwner;

        public bool IsZoneActive =>
            NetworkInitialized &&
            Runner != null &&
            !NetworkLifetimeTimer.ExpiredOrNotRunning(Runner);

        public override void Spawned()
        {
            ActiveZones.Add(this);
            DisablePrefabRendererLocal();
            EnsureWorldVisualLocal();
        }

        public override void Despawned(
            NetworkRunner runner,
            bool hasState
        )
        {
            ActiveZones.Remove(this);
            DestroyWorldVisualLocal();
            UpdateLocalOverlay(runner);

            if (ActiveZones.Count == 0)
            {
                DestroyLocalOverlay();
            }
        }

        public bool ConfigureAuthority(
            PlayerRef owner
        )
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

            NetworkOwner = owner;
            NetworkLifetimeTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    ProjectJSmokeGrenadePolicy.SmokeDurationSeconds
                );
            NetworkSpawnOrder =
                ++NextSpawnOrder;
            NetworkInitialized = true;

            EnforceOwnerZoneLimitAuthority();

            return true;
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority)
            {
                return;
            }

            bool lifetimeActive =
                NetworkInitialized &&
                Runner != null &&
                !NetworkLifetimeTimer.ExpiredOrNotRunning(Runner);

            bool anyGameplayActive =
                HasAnyGameplayActive();

            if (
                !ProjectJSmokeGrenadePolicy.ShouldKeepSmokeZone(
                    lifetimeActive,
                    anyGameplayActive
                )
            )
            {
                DespawnAuthority();
            }
        }

        public override void Render()
        {
            EnsureWorldVisualLocal();
            UpdateLocalOverlay(Runner);
            DrawDebugRadius();
        }

        private bool HasAnyGameplayActive()
        {
            ProjectJNetworkExternalGameplay.CollectActivePlayers(
                Runner,
                GameplayPlayerBuffer
            );

            for (
                int index = 0;
                index < GameplayPlayerBuffer.Count;
                index++
            )
            {
                ProjectJNetworkExternalGameplay player =
                    GameplayPlayerBuffer[index];

                if (
                    player != null &&
                    player.GameplayInputAllowed
                )
                {
                    return true;
                }
            }

            return false;
        }

        private void EnforceOwnerZoneLimitAuthority()
        {
            ProjectJNetworkSmokeZone[] zones =
                UnityEngine.Object.FindObjectsByType<ProjectJNetworkSmokeZone>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            int ownerZoneCount = 0;
            ProjectJNetworkSmokeZone oldestZone = null;
            int oldestOrder = int.MaxValue;

            for (int index = 0; index < zones.Length; index++)
            {
                ProjectJNetworkSmokeZone zone =
                    zones[index];

                if (
                    zone == null ||
                    zone.Runner != Runner ||
                    !zone.NetworkInitialized ||
                    zone.NetworkOwner != NetworkOwner
                )
                {
                    continue;
                }

                ownerZoneCount++;

                if (
                    zone.NetworkSpawnOrder <
                    oldestOrder
                )
                {
                    oldestOrder =
                        zone.NetworkSpawnOrder;
                    oldestZone =
                        zone;
                }
            }

            if (
                ownerZoneCount <=
                ProjectJSmokeGrenadePolicy.MaximumActiveZonesPerOwner
            )
            {
                return;
            }

            if (
                oldestZone != null &&
                oldestZone != this &&
                oldestZone.Object != null &&
                oldestZone.Object.IsValid &&
                oldestZone.Object.HasStateAuthority
            )
            {
                oldestZone.DespawnAuthority();
            }
        }

        private static void UpdateLocalOverlay(
            NetworkRunner runner
        )
        {
            if (runner == null)
            {
                HideLocalOverlay();
                return;
            }

            ProjectJNetworkExternalGameplay.CollectActivePlayers(
                runner,
                OverlayPlayerBuffer
            );

            ProjectJNetworkExternalGameplay localPlayer =
                null;

            for (
                int index = 0;
                index < OverlayPlayerBuffer.Count;
                index++
            )
            {
                ProjectJNetworkExternalGameplay candidate =
                    OverlayPlayerBuffer[index];

                if (
                    candidate == null ||
                    candidate.Object == null ||
                    !candidate.Object.IsValid ||
                    !candidate.Object.HasInputAuthority
                )
                {
                    continue;
                }

                localPlayer =
                    candidate;
                break;
            }

            if (
                localPlayer == null ||
                !localPlayer.GameplayInputAllowed
            )
            {
                HideLocalOverlay();
                return;
            }

            int containingZoneCount = 0;

            foreach (
                ProjectJNetworkSmokeZone zone
                in ActiveZones
            )
            {
                if (
                    zone == null ||
                    zone.Runner != runner ||
                    zone.Object == null ||
                    !zone.Object.IsValid ||
                    !zone.IsZoneActive
                )
                {
                    continue;
                }

                float distance =
                    Vector3.Distance(
                        zone.transform.position,
                        localPlayer.transform.position
                    );

                if (
                    ProjectJSmokeGrenadePolicy.IsWithinSmokeRadius(
                        distance
                    )
                )
                {
                    containingZoneCount++;
                }
            }

            float overlayAlpha =
                ProjectJSmokeGrenadePolicy.ResolveOverlayAlpha(
                    containingZoneCount
                );

            if (overlayAlpha <= 0f)
            {
                HideLocalOverlay();
                return;
            }

            EnsureLocalOverlay(
                localPlayer.transform
            );

            if (localOverlayImage != null)
            {
                localOverlayImage.color =
                    new Color(
                        0.18f,
                        0.20f,
                        0.22f,
                        overlayAlpha
                    );
                localOverlayImage.enabled =
                    true;
            }
        }

        private static void EnsureLocalOverlay(
            Transform localPlayerTransform
        )
        {
            if (
                localOverlayCanvas != null &&
                localOverlayImage != null &&
                localOverlayParent ==
                localPlayerTransform
            )
            {
                return;
            }

            DestroyLocalOverlay();

            if (localPlayerTransform == null)
            {
                return;
            }

            GameObject canvasObject =
                new GameObject(
                    "Smoke Grenade Overlay Canvas"
                );

            canvasObject.transform.SetParent(
                localPlayerTransform,
                false
            );

            localOverlayCanvas =
                canvasObject.AddComponent<Canvas>();
            localOverlayCanvas.renderMode =
                RenderMode.ScreenSpaceOverlay;
            localOverlayCanvas.sortingOrder =
                29000;

            CanvasScaler scaler =
                canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution =
                new Vector2(1920f, 1080f);
            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight =
                0.5f;

            GameObject imageObject =
                new GameObject(
                    "Smoke Overlay"
                );
            imageObject.transform.SetParent(
                canvasObject.transform,
                false
            );

            localOverlayImage =
                imageObject.AddComponent<Image>();
            localOverlayImage.raycastTarget =
                false;

            RectTransform rect =
                localOverlayImage.rectTransform;
            rect.anchorMin =
                Vector2.zero;
            rect.anchorMax =
                Vector2.one;
            rect.offsetMin =
                Vector2.zero;
            rect.offsetMax =
                Vector2.zero;

            localOverlayParent =
                localPlayerTransform;
        }

        private static void HideLocalOverlay()
        {
            if (localOverlayImage != null)
            {
                localOverlayImage.enabled =
                    false;
            }
        }

        private static void DestroyLocalOverlay()
        {
            if (localOverlayCanvas != null)
            {
                UnityEngine.Object.Destroy(
                    localOverlayCanvas.gameObject
                );
            }

            localOverlayCanvas =
                null;
            localOverlayImage =
                null;
            localOverlayParent =
                null;
        }

        private void DisablePrefabRendererLocal()
        {
            Renderer rootRenderer =
                GetComponent<Renderer>();

            if (rootRenderer != null)
            {
                rootRenderer.enabled =
                    false;
            }
        }

        private void EnsureWorldVisualLocal()
        {
            if (worldVisualObject != null)
            {
                return;
            }

            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/Unlit"
                );

            if (shader == null)
            {
                shader =
                    Shader.Find(
                        "Sprites/Default"
                    );
            }

            if (shader == null)
            {
                return;
            }

            worldVisualObject =
                GameObject.CreatePrimitive(
                    PrimitiveType.Sphere
                );

            worldVisualObject.name =
                "Smoke Zone Prototype Visual";
            worldVisualObject.transform.SetParent(
                transform,
                false
            );
            worldVisualObject.transform.localPosition =
                Vector3.zero;
            worldVisualObject.transform.localScale =
                Vector3.one *
                ProjectJSmokeGrenadePolicy.SmokeRadius *
                2f;

            Collider generatedCollider =
                worldVisualObject.GetComponent<Collider>();

            if (generatedCollider != null)
            {
                UnityEngine.Object.Destroy(
                    generatedCollider
                );
            }

            Renderer renderer =
                worldVisualObject.GetComponent<Renderer>();

            if (renderer == null)
            {
                return;
            }

            renderer.shadowCastingMode =
                ShadowCastingMode.Off;
            renderer.receiveShadows =
                false;

            worldVisualMaterial =
                new Material(shader);

            if (
                worldVisualMaterial.HasProperty(
                    "_Surface"
                )
            )
            {
                worldVisualMaterial.SetFloat(
                    "_Surface",
                    1f
                );
                worldVisualMaterial.SetFloat(
                    "_ZWrite",
                    0f
                );
                worldVisualMaterial.EnableKeyword(
                    "_SURFACE_TYPE_TRANSPARENT"
                );
            }

            Color smokeColor =
                new Color(
                    0.28f,
                    0.30f,
                    0.32f,
                    0.18f
                );

            if (
                worldVisualMaterial.HasProperty(
                    "_BaseColor"
                )
            )
            {
                worldVisualMaterial.SetColor(
                    "_BaseColor",
                    smokeColor
                );
            }
            else
            {
                worldVisualMaterial.color =
                    smokeColor;
            }

            worldVisualMaterial.renderQueue =
                3000;

            renderer.sharedMaterial =
                worldVisualMaterial;
        }

        private void DestroyWorldVisualLocal()
        {
            if (worldVisualObject != null)
            {
                UnityEngine.Object.Destroy(
                    worldVisualObject
                );
            }

            if (worldVisualMaterial != null)
            {
                UnityEngine.Object.Destroy(
                    worldVisualMaterial
                );
            }

            worldVisualObject =
                null;
            worldVisualMaterial =
                null;
        }

        private void DrawDebugRadius()
        {
            Vector3 center =
                transform.position;

            Debug.DrawLine(
                center + Vector3.left *
                ProjectJSmokeGrenadePolicy.SmokeRadius,
                center + Vector3.right *
                ProjectJSmokeGrenadePolicy.SmokeRadius,
                Color.gray
            );

            Debug.DrawLine(
                center + Vector3.back *
                ProjectJSmokeGrenadePolicy.SmokeRadius,
                center + Vector3.forward *
                ProjectJSmokeGrenadePolicy.SmokeRadius,
                Color.gray
            );
        }

        private void DespawnAuthority()
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid
            )
            {
                return;
            }

            Runner.Despawn(Object);
        }
    }
}
