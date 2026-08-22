using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJNetworkPlayer :
        NetworkBehaviour
    {
        private const float InputPulseDuration =
            0.35f;

        private Renderer visualRenderer;
        private Camera authorityCamera;

        private Material runtimeMaterial;
        private RenderTexture authorityCameraTexture;

        private float inputPulseUntil;

        public PlayerRef Owner =>
            Object != null &&
            Object.IsValid
                ? Object.InputAuthority
                : default;

        public bool HasLocalStateAuthority =>
            Object != null &&
            Object.IsValid &&
            Object.HasStateAuthority;

        public bool HasLocalInputAuthority =>
            Object != null &&
            Object.IsValid &&
            Object.HasInputAuthority;

        public bool AuthorityCameraEnabled =>
            authorityCamera != null &&
            authorityCamera.enabled;

        public bool LocalInputSeenRecently =>
            Time.unscaledTime <
            inputPulseUntil;

        public override void Spawned()
        {
            CachePresentation();
            ApplyAuthorityPresentation();

            Debug.Log(
                "[Project J/Fusion] " +
                "Network Player 연결 / " +
                "Owner: " +
                Object.InputAuthority.AsIndex +
                " / State Authority: " +
                Object.HasStateAuthority +
                " / Input Authority: " +
                Object.HasInputAuthority
            );
        }

        private void Update()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasInputAuthority
            )
            {
                return;
            }

            Keyboard keyboard =
                Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            bool receivedLocalInput =
                keyboard.wKey.isPressed ||
                keyboard.aKey.isPressed ||
                keyboard.sKey.isPressed ||
                keyboard.dKey.isPressed ||
                keyboard.spaceKey.isPressed;

            if (receivedLocalInput)
            {
                inputPulseUntil =
                    Time.unscaledTime +
                    InputPulseDuration;
            }
        }

        private void CachePresentation()
        {
            visualRenderer =
                GetComponentInChildren<
                    Renderer
                >(
                    true
                );

            authorityCamera =
                GetComponentInChildren<
                    Camera
                >(
                    true
                );
        }

        private void ApplyAuthorityPresentation()
        {
            bool isLocalOwner =
                Object.HasInputAuthority;

            if (visualRenderer != null)
            {
                runtimeMaterial =
                    visualRenderer.material;

                runtimeMaterial.color =
                    isLocalOwner
                        ? new Color(
                            0.2f,
                            0.9f,
                            0.35f,
                            1f
                        )
                        : new Color(
                            1f,
                            0.55f,
                            0.15f,
                            1f
                        );
            }

            if (authorityCamera == null)
            {
                return;
            }

            if (!isLocalOwner)
            {
                authorityCamera.enabled =
                    false;

                authorityCamera.targetTexture =
                    null;

                return;
            }

            authorityCameraTexture =
                new RenderTexture(
                    16,
                    16,
                    24,
                    RenderTextureFormat.ARGB32
                );

            authorityCameraTexture.name =
                "ProjectJ_AuthorityCamera_" +
                Object.InputAuthority.AsIndex;

            authorityCameraTexture.Create();

            authorityCamera.cullingMask =
                0;

            authorityCamera.targetTexture =
                authorityCameraTexture;

            authorityCamera.enabled =
                true;
        }

        private void OnDestroy()
        {
            if (authorityCamera != null)
            {
                authorityCamera.enabled =
                    false;

                authorityCamera.targetTexture =
                    null;
            }

            if (authorityCameraTexture != null)
            {
                authorityCameraTexture.Release();

                Destroy(
                    authorityCameraTexture
                );

                authorityCameraTexture =
                    null;
            }

            if (runtimeMaterial != null)
            {
                Destroy(
                    runtimeMaterial
                );

                runtimeMaterial =
                    null;
            }
        }
    }
}
