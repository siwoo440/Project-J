using Fusion;
using UnityEngine;

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJNetworkPlayer :
        NetworkBehaviour,
        IBeforeAllTicks,
        IAfterAllTicks
    {
        private const float InputPulseDuration =
            0.35f;

        private const float BaseMoveSpeed =
            5f;

        private Renderer visualRenderer;
        private Camera authorityCamera;

        private Material runtimeMaterial;
        private RenderTexture authorityCameraTexture;

        private float inputPulseUntil;

        private bool hasForwardPosition;
        private Vector3 lastForwardPosition;
        private Vector3 predictedPositionBeforeResimulation;

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

        public bool HasReceivedInput
        {
            get;
            private set;
        }

        public Vector2 LastReceivedMove
        {
            get;
            private set;
        }

        public bool LastReceivedJump
        {
            get;
            private set;
        }

        public bool LastReceivedSprint
        {
            get;
            private set;
        }

        public bool LastReceivedCrouch
        {
            get;
            private set;
        }

        public string LastReceivedTick
        {
            get;
            private set;
        } =
            "-";

        public Vector3 CurrentPosition =>
            transform.position;

        public float MovementSpeed =>
            BaseMoveSpeed;

        public bool InputSeenRecently =>
            Time.unscaledTime <
            inputPulseUntil;

        public int ResimulationBatchCount
        {
            get;
            private set;
        }

        public int ResimulationTickCount
        {
            get;
            private set;
        }

        public int LastResimulationTickCount
        {
            get;
            private set;
        }

        public int LastForwardTickCount
        {
            get;
            private set;
        }

        public float LastRollbackDistance
        {
            get;
            private set;
        }

        public float LastCorrectionDistance
        {
            get;
            private set;
        }

        public float MaxCorrectionDistance
        {
            get;
            private set;
        }

        public Vector3 PredictionPositionBeforeResimulation
        {
            get;
            private set;
        }

        public Vector3 RollbackPosition
        {
            get;
            private set;
        }

        public Vector3 CorrectedPositionAfterResimulation
        {
            get;
            private set;
        }

        public override void Spawned()
        {
            CachePresentation();
            ApplyAuthorityPresentation();

            lastForwardPosition =
                transform.position;

            hasForwardPosition =
                true;

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

        public override void FixedUpdateNetwork()
        {
            if (
                !GetInput<ProjectJNetworkInput>(
                    out ProjectJNetworkInput input
                )
            )
            {
                return;
            }

            HasReceivedInput =
                true;

            Vector2 moveInput =
                input.Move;

            if (
                moveInput.sqrMagnitude >
                1f
            )
            {
                moveInput.Normalize();
            }

            LastReceivedMove =
                moveInput;

            LastReceivedJump =
                input.Buttons.IsSet(
                    ProjectJNetworkButton.Jump
                );

            LastReceivedSprint =
                input.Buttons.IsSet(
                    ProjectJNetworkButton.Sprint
                );

            LastReceivedCrouch =
                input.Buttons.IsSet(
                    ProjectJNetworkButton.Crouch
                );

            LastReceivedTick =
                Runner.Tick.ToString();

            Vector3 moveDirection =
                new Vector3(
                    moveInput.x,
                    0f,
                    moveInput.y
                );

            transform.position +=
                moveDirection *
                BaseMoveSpeed *
                Runner.DeltaTime;

            bool hasActivity =
                moveInput.sqrMagnitude >
                    0.0001f ||
                LastReceivedJump ||
                LastReceivedSprint ||
                LastReceivedCrouch;

            if (hasActivity)
            {
                inputPulseUntil =
                    Time.unscaledTime +
                    InputPulseDuration;
            }
        }

        public void BeforeAllTicks(
            bool resimulation,
            int tickCount
        )
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasInputAuthority
            )
            {
                return;
            }

            if (!resimulation)
            {
                LastForwardTickCount =
                    tickCount;

                return;
            }

            ResimulationBatchCount++;
            ResimulationTickCount +=
                tickCount;

            LastResimulationTickCount =
                tickCount;

            predictedPositionBeforeResimulation =
                hasForwardPosition
                    ? lastForwardPosition
                    : transform.position;

            PredictionPositionBeforeResimulation =
                predictedPositionBeforeResimulation;

            RollbackPosition =
                transform.position;

            LastRollbackDistance =
                Vector3.Distance(
                    predictedPositionBeforeResimulation,
                    RollbackPosition
                );
        }

        public void AfterAllTicks(
            bool resimulation,
            int tickCount
        )
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasInputAuthority
            )
            {
                return;
            }

            if (resimulation)
            {
                CorrectedPositionAfterResimulation =
                    transform.position;

                LastCorrectionDistance =
                    Vector3.Distance(
                        predictedPositionBeforeResimulation,
                        CorrectedPositionAfterResimulation
                    );

                if (
                    LastCorrectionDistance >
                    MaxCorrectionDistance
                )
                {
                    MaxCorrectionDistance =
                        LastCorrectionDistance;
                }

                return;
            }

            LastForwardTickCount =
                tickCount;

            lastForwardPosition =
                transform.position;

            hasForwardPosition =
                true;
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
