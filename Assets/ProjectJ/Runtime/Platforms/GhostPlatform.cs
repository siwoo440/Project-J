using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectJ.Platforms
{
    public enum GhostPlatformState
    {
        Active = 0,
        Warning = 1,
        Hidden = 2
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class GhostPlatform :
        MonoBehaviour
    {
        private const int MaxOverlapResults =
            16;

        [SerializeField]
        [Min(0.1f)]
        private float activeDuration =
            3f;

        [SerializeField]
        [Min(0.1f)]
        private float warningDuration =
            1f;

        [SerializeField]
        [Min(0.1f)]
        private float hiddenDuration =
            2f;

        [SerializeField]
        [Min(0f)]
        private float cycleOffset;

        [SerializeField]
        private Renderer platformRenderer;

        [SerializeField]
        private BoxCollider platformCollider;

        [SerializeField]
        private LayerMask playerLayers =
            1 << 8;

        private readonly Collider[] overlapResults =
            new Collider[
                MaxOverlapResults
            ];

        private GhostPlatformState
            currentState =
                GhostPlatformState.Active;

        private Material runtimeMaterial;
        private Color originalColor =
            Color.white;

        public GhostPlatformState CurrentState
        {
            get
            {
                return currentState;
            }
        }

        private void Awake()
        {
            ResolveReferences();
            PrepareTransparentMaterial();

            float initialCycleTime =
                cycleOffset;

            ApplyState(
                EvaluateState(
                    initialCycleTime,
                    activeDuration,
                    warningDuration,
                    hiddenDuration
                ),
                true
            );

            ApplyVisualAlpha(
                EvaluateVisibilityAlpha(
                    initialCycleTime,
                    activeDuration,
                    warningDuration,
                    hiddenDuration
                )
            );
        }

        private void Update()
        {
            float cycleTime =
                Time.time +
                cycleOffset;

            GhostPlatformState nextState =
                EvaluateState(
                    cycleTime,
                    activeDuration,
                    warningDuration,
                    hiddenDuration
                );

            if (
                nextState !=
                currentState
            )
            {
                ApplyState(
                    nextState,
                    false
                );
            }

            float alpha =
                EvaluateVisibilityAlpha(
                    cycleTime,
                    activeDuration,
                    warningDuration,
                    hiddenDuration
                );

            ApplyVisualAlpha(
                alpha
            );
        }

        private void OnDestroy()
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            Destroy(
                runtimeMaterial
            );

            runtimeMaterial =
                null;
        }

        public void Configure(
            float newActiveDuration,
            float newWarningDuration,
            float newHiddenDuration,
            float newCycleOffset,
            LayerMask newPlayerLayers
        )
        {
            activeDuration =
                Mathf.Max(
                    0.1f,
                    newActiveDuration
                );

            warningDuration =
                Mathf.Max(
                    0.1f,
                    newWarningDuration
                );

            hiddenDuration =
                Mathf.Max(
                    0.1f,
                    newHiddenDuration
                );

            cycleOffset =
                Mathf.Max(
                    0f,
                    newCycleOffset
                );

            playerLayers =
                newPlayerLayers;

            ResolveReferences();
        }

        public static GhostPlatformState
            EvaluateState(
                float time,
                float activeTime,
                float warningTime,
                float hiddenTime
            )
        {
            activeTime =
                Mathf.Max(
                    0.1f,
                    activeTime
                );

            warningTime =
                Mathf.Max(
                    0.1f,
                    warningTime
                );

            hiddenTime =
                Mathf.Max(
                    0.1f,
                    hiddenTime
                );

            float total =
                activeTime +
                warningTime +
                hiddenTime;

            float localTime =
                Mathf.Repeat(
                    Mathf.Max(
                        0f,
                        time
                    ),
                    total
                );

            if (localTime < activeTime)
            {
                return
                    GhostPlatformState.Active;
            }

            if (
                localTime <
                activeTime +
                warningTime
            )
            {
                return
                    GhostPlatformState.Warning;
            }

            return
                GhostPlatformState.Hidden;
        }

        public static float
            EvaluateVisibilityAlpha(
                float time,
                float activeTime,
                float warningTime,
                float hiddenTime
            )
        {
            activeTime =
                Mathf.Max(
                    0.1f,
                    activeTime
                );

            warningTime =
                Mathf.Max(
                    0.1f,
                    warningTime
                );

            hiddenTime =
                Mathf.Max(
                    0.1f,
                    hiddenTime
                );

            float total =
                activeTime +
                warningTime +
                hiddenTime;

            float localTime =
                Mathf.Repeat(
                    Mathf.Max(
                        0f,
                        time
                    ),
                    total
                );

            if (localTime < activeTime)
            {
                return 1f;
            }

            if (
                localTime <
                activeTime +
                warningTime
            )
            {
                float fadeProgress =
                    (
                        localTime -
                        activeTime
                    ) /
                    warningTime;

                return
                    1f -
                    Mathf.Clamp01(
                        fadeProgress
                    );
            }

            return 0f;
        }

        private void ApplyState(
            GhostPlatformState nextState,
            bool initialApply
        )
        {
            GhostPlatformState previousState =
                currentState;

            currentState =
                nextState;

            if (
                currentState ==
                GhostPlatformState.Hidden
            )
            {
                if (platformCollider != null)
                {
                    platformCollider.enabled =
                        false;
                }

                return;
            }

            if (platformCollider != null)
            {
                platformCollider.enabled =
                    true;
            }

            if (platformRenderer != null)
            {
                platformRenderer.enabled =
                    true;
            }

            if (
                !initialApply &&
                previousState ==
                    GhostPlatformState.Hidden
            )
            {
                ResolvePlayerOverlap();
            }
        }

        private void ApplyVisualAlpha(
            float alpha
        )
        {
            if (
                platformRenderer == null ||
                runtimeMaterial == null
            )
            {
                return;
            }

            float clampedAlpha =
                Mathf.Clamp01(
                    alpha
                );

            Color color =
                originalColor;

            color.a =
                originalColor.a *
                clampedAlpha;

            if (
                runtimeMaterial.HasProperty(
                    "_BaseColor"
                )
            )
            {
                runtimeMaterial.SetColor(
                    "_BaseColor",
                    color
                );
            }
            else if (
                runtimeMaterial.HasProperty(
                    "_Color"
                )
            )
            {
                runtimeMaterial.SetColor(
                    "_Color",
                    color
                );
            }

            platformRenderer.enabled =
                clampedAlpha >
                0.001f;
        }

        private void PrepareTransparentMaterial()
        {
            if (
                platformRenderer == null ||
                platformRenderer.sharedMaterial ==
                    null
            )
            {
                return;
            }

            runtimeMaterial =
                new Material(
                    platformRenderer.sharedMaterial
                );

            runtimeMaterial.name =
                platformRenderer
                    .sharedMaterial
                    .name +
                " (Ghost Runtime)";

            platformRenderer.sharedMaterial =
                runtimeMaterial;

            if (
                runtimeMaterial.HasProperty(
                    "_BaseColor"
                )
            )
            {
                originalColor =
                    runtimeMaterial.GetColor(
                        "_BaseColor"
                    );
            }
            else if (
                runtimeMaterial.HasProperty(
                    "_Color"
                )
            )
            {
                originalColor =
                    runtimeMaterial.GetColor(
                        "_Color"
                    );
            }

            ConfigureMaterialForTransparency(
                runtimeMaterial
            );
        }

        private static void
            ConfigureMaterialForTransparency(
                Material material
            )
        {
            if (material == null)
            {
                return;
            }

            material.SetOverrideTag(
                "RenderType",
                "Transparent"
            );

            if (
                material.HasProperty(
                    "_Surface"
                )
            )
            {
                material.SetFloat(
                    "_Surface",
                    1f
                );
            }

            if (
                material.HasProperty(
                    "_Blend"
                )
            )
            {
                material.SetFloat(
                    "_Blend",
                    0f
                );
            }

            if (
                material.HasProperty(
                    "_SrcBlend"
                )
            )
            {
                material.SetFloat(
                    "_SrcBlend",
                    (float)BlendMode.SrcAlpha
                );
            }

            if (
                material.HasProperty(
                    "_DstBlend"
                )
            )
            {
                material.SetFloat(
                    "_DstBlend",
                    (float)BlendMode
                        .OneMinusSrcAlpha
                );
            }

            if (
                material.HasProperty(
                    "_ZWrite"
                )
            )
            {
                material.SetFloat(
                    "_ZWrite",
                    0f
                );
            }

            material.EnableKeyword(
                "_SURFACE_TYPE_TRANSPARENT"
            );

            material.EnableKeyword(
                "_ALPHABLEND_ON"
            );

            material.DisableKeyword(
                "_ALPHAPREMULTIPLY_ON"
            );

            material.DisableKeyword(
                "_ALPHATEST_ON"
            );

            material.renderQueue =
                (int)RenderQueue.Transparent;
        }

        private void ResolvePlayerOverlap()
        {
            if (
                platformCollider == null ||
                !platformCollider.enabled
            )
            {
                return;
            }

            Bounds bounds =
                platformCollider.bounds;

            int hitCount =
                Physics.OverlapBoxNonAlloc(
                    bounds.center,
                    bounds.extents,
                    overlapResults,
                    transform.rotation,
                    playerLayers,
                    QueryTriggerInteraction.Ignore
                );

            for (
                int i = 0;
                i < hitCount;
                i++
            )
            {
                Collider playerCollider =
                    overlapResults[i];

                overlapResults[i] =
                    null;

                if (playerCollider == null)
                {
                    continue;
                }

                Rigidbody body =
                    playerCollider.attachedRigidbody;

                if (
                    body == null ||
                    body.isKinematic
                )
                {
                    continue;
                }

                float correctionY =
                    bounds.max.y -
                    playerCollider.bounds.min.y +
                    0.05f;

                if (correctionY <= 0f)
                {
                    continue;
                }

                body.position +=
                    Vector3.up *
                    correctionY;

                Physics.SyncTransforms();
            }
        }

        private void ResolveReferences()
        {
            if (platformCollider == null)
            {
                platformCollider =
                    GetComponent<
                        BoxCollider
                    >();
            }

            if (platformRenderer == null)
            {
                platformRenderer =
                    GetComponent<Renderer>();
            }
        }

        private void OnValidate()
        {
            activeDuration =
                Mathf.Max(
                    0.1f,
                    activeDuration
                );

            warningDuration =
                Mathf.Max(
                    0.1f,
                    warningDuration
                );

            hiddenDuration =
                Mathf.Max(
                    0.1f,
                    hiddenDuration
                );

            cycleOffset =
                Mathf.Max(
                    0f,
                    cycleOffset
                );
        }
    }
}
