using System.Collections.Generic; // Runner별 로컬 관찰자 저장
using Fusion; // NetworkRunner 사용
using ProjectJ.Items; // 투명 망토 표시 정책 사용
using UnityEngine; // Renderer와 Transform 사용

namespace ProjectJ.Networking.Fusion
{
    [DefaultExecutionOrder(1000)] // Player 기본 LateUpdate 이후 은신 표시 적용
    [DisallowMultipleComponent]
    public sealed class ProjectJNetworkInvisibilityPresentation :
        MonoBehaviour
    {
        private static readonly Dictionary<NetworkRunner, ProjectJNetworkInvisibilityPresentation> LocalViewers =
            new Dictionary<NetworkRunner, ProjectJNetworkInvisibilityPresentation>();

        private ProjectJNetworkPlayer networkPlayer;
        private ProjectJNetworkItemInventory itemInventory;
        private Renderer visualRenderer;
        private Transform visualTransform;
        private float baseVisualLocalX;
        private bool baseVisualLocalXCached;
        private NetworkRunner registeredRunner;

        private void Awake()
        {
            ResolveReferences();
        }

        private void LateUpdate()
        {
            ResolveReferences();

            if (
                networkPlayer == null ||
                itemInventory == null ||
                visualRenderer == null ||
                visualTransform == null
            )
            {
                return;
            }

            bool hasLocalInputAuthority =
                networkPlayer.Object != null &&
                networkPlayer.Object.IsValid &&
                networkPlayer.Object.HasInputAuthority;

            if (
                hasLocalInputAuthority &&
                networkPlayer.Runner != null
            )
            {
                registeredRunner =
                    networkPlayer.Runner;

                LocalViewers[registeredRunner] =
                    this;
            }

            bool isLocalOwner =
                hasLocalInputAuthority;

            float viewerDistance =
                ResolveViewerDistance(
                    isLocalOwner
                );

            ProjectJInvisibilityPresentationMode mode =
                ProjectJInvisibilityCloakPolicy.ResolvePresentationMode(
                    isLocalOwner,
                    itemInventory.IsInvisibilityCloakActive,
                    viewerDistance
                );

            switch (mode)
            {
                case ProjectJInvisibilityPresentationMode.Visible:
                    visualRenderer.enabled =
                        true;

                    RestoreVisualOffset();

                    break;

                case ProjectJInvisibilityPresentationMode.Hidden:
                    visualRenderer.enabled =
                        false;

                    RestoreVisualOffset();

                    break;

                case ProjectJInvisibilityPresentationMode.ProximityShimmer:
                    visualRenderer.enabled =
                        ProjectJInvisibilityCloakPolicy.IsShimmerVisible(
                            Time.unscaledTime
                        );

                    ApplyShimmerOffset();

                    break;
            }
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

            if (visualRenderer == null)
            {
                visualRenderer =
                    GetComponentInChildren<Renderer>(
                        true
                    );
            }

            if (
                visualTransform == null &&
                visualRenderer != null
            )
            {
                visualTransform =
                    visualRenderer.transform;
            }

            if (
                !baseVisualLocalXCached &&
                visualTransform != null
            )
            {
                baseVisualLocalX =
                    visualTransform.localPosition.x;

                baseVisualLocalXCached =
                    true;
            }
        }

        private float ResolveViewerDistance(
            bool isLocalOwner
        )
        {
            if (isLocalOwner)
            {
                return 0f;
            }

            if (
                networkPlayer == null ||
                networkPlayer.Runner == null ||
                !LocalViewers.TryGetValue(
                    networkPlayer.Runner,
                    out ProjectJNetworkInvisibilityPresentation viewer
                ) ||
                viewer == null ||
                viewer == this
            )
            {
                return float.PositiveInfinity;
            }

            return
                Vector3.Distance(
                    transform.position,
                    viewer.transform.position
                );
        }

        private void ApplyShimmerOffset()
        {
            if (
                visualTransform == null ||
                !baseVisualLocalXCached
            )
            {
                return;
            }

            Vector3 localPosition =
                visualTransform.localPosition;

            localPosition.x =
                baseVisualLocalX +
                ProjectJInvisibilityCloakPolicy.CalculateShimmerOffset(
                    Time.unscaledTime
                );

            visualTransform.localPosition =
                localPosition;
        }

        private void RestoreVisualOffset()
        {
            if (
                visualTransform == null ||
                !baseVisualLocalXCached
            )
            {
                return;
            }

            Vector3 localPosition =
                visualTransform.localPosition;

            localPosition.x =
                baseVisualLocalX;

            visualTransform.localPosition =
                localPosition;
        }

        private void OnDestroy()
        {
            if (
                registeredRunner != null &&
                LocalViewers.TryGetValue(
                    registeredRunner,
                    out ProjectJNetworkInvisibilityPresentation viewer
                ) &&
                viewer == this
            )
            {
                LocalViewers.Remove(
                    registeredRunner
                );
            }

            if (visualRenderer != null)
            {
                visualRenderer.enabled =
                    true;
            }

            RestoreVisualOffset();
        }
    }
}
