using System.Collections.Generic; // Player 목록과 재적중 기록 사용
using Fusion; // NetworkBehaviour, PlayerRef, TickTimer 사용
using ProjectJ.Items; // 카트 정책과 Route Node 사용
using UnityEngine; // 이동, 충돌, 프로토타입 외형 사용
using UnityEngine.Rendering; // 그림자 비활성화 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public sealed class ProjectJNetworkCart :
        NetworkBehaviour
    {
        private const int ContactBufferSize = 32;

        private readonly List<ProjectJNetworkExternalGameplay> playerBuffer =
            new List<ProjectJNetworkExternalGameplay>(8);

        private readonly Collider[] contactBuffer =
            new Collider[ContactBufferSize];

        private readonly HashSet<int> processedTargetIndices =
            new HashSet<int>();

        private readonly Dictionary<int, float> lastHitTimeByTarget =
            new Dictionary<int, float>();

        private ProjectJCartRouteNode currentTargetNode;
        private GameObject worldVisualObject;

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
        private int NetworkVisitedNodeCount
        {
            get;
            set;
        }

        [Networked]
        private int NetworkLastPushTargetIndex
        {
            get;
            set;
        }

        [Networked]
        private int NetworkPushSuccessCount
        {
            get;
            set;
        }

        public bool IsInitialized => NetworkInitialized;
        public PlayerRef Owner => NetworkOwner;
        public int VisitedNodeCount => NetworkVisitedNodeCount;
        public int LastPushTargetIndex => NetworkLastPushTargetIndex;
        public int PushSuccessCount => NetworkPushSuccessCount;

        public override void Spawned()
        {
            DisablePrefabRendererLocal();
            EnsureWorldVisualLocal();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            DestroyWorldVisualLocal();
        }

        public bool ConfigureAuthority(
            PlayerRef owner,
            ProjectJCartRouteNode startNode
        )
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                startNode == null
            )
            {
                return false;
            }

            NetworkOwner = owner;
            NetworkLifetimeTimer = TickTimer.CreateFromSeconds(
                Runner,
                ProjectJCartPolicy.LifetimeSeconds
            );
            NetworkVisitedNodeCount = 0;
            NetworkLastPushTargetIndex = -1;
            NetworkPushSuccessCount = 0;
            currentTargetNode = startNode;
            NetworkInitialized = true;

            FaceCurrentTargetAuthority();
            return true;
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority)
            {
                return;
            }

            if (!NetworkInitialized || Runner == null)
            {
                FinishRideAuthority(false);
                return;
            }

            ResolveOwner(
                out ProjectJNetworkExternalGameplay ownerGameplay,
                out ProjectJNetworkPlayer ownerPlayer,
                out ProjectJNetworkItemInventory ownerInventory
            );

            bool ownerValid =
                ownerGameplay != null &&
                ownerPlayer != null &&
                ownerInventory != null &&
                ownerGameplay.Object != null &&
                ownerGameplay.Object.IsValid;

            bool gameplayAllowed =
                ownerGameplay != null &&
                ownerGameplay.GameplayInputAllowed;

            bool lifetimeActive =
                !NetworkLifetimeTimer.ExpiredOrNotRunning(Runner);

            bool routeEnded = currentTargetNode == null;

            if (
                ProjectJCartPolicy.ShouldFinishRide(
                    lifetimeActive,
                    gameplayAllowed,
                    ownerValid,
                    routeEnded
                )
            )
            {
                FinishRideAuthority(false);
                return;
            }

            if (!ownerInventory.IsCartRiding)
            {
                FinishRideAuthority(false);
                return;
            }

            if (ownerPlayer.LastReceivedJump)
            {
                FinishRideAuthority(true);
                return;
            }

            MoveAlongRouteAuthority();

            if (
                Object == null ||
                !Object.IsValid ||
                currentTargetNode == null
            )
            {
                return;
            }

            CarryOwnerAuthority(ownerPlayer);
            ApplyContactPushAuthority();
        }

        internal void FinishFromInventoryAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return;
            }

            FinishRideAuthority(false);
        }

        private void MoveAlongRouteAuthority()
        {
            if (currentTargetNode == null)
            {
                FinishRideAuthority(false);
                return;
            }

            Vector3 targetPosition =
                currentTargetNode.transform.position;

            Vector3 currentPosition =
                transform.position;

            float distance = Vector3.Distance(
                currentPosition,
                targetPosition
            );

            if (!ProjectJCartPolicy.HasReachedNode(distance))
            {
                float travelDistance =
                    ProjectJCartPolicy.CalculateTravelDistance(
                        Runner.DeltaTime
                    );

                Vector3 nextPosition = Vector3.MoveTowards(
                    currentPosition,
                    targetPosition,
                    travelDistance
                );

                Vector3 movementDirection =
                    nextPosition - currentPosition;

                transform.position = nextPosition;
                movementDirection.y = 0f;

                if (movementDirection.sqrMagnitude > 0.0001f)
                {
                    transform.rotation = Quaternion.LookRotation(
                        movementDirection.normalized,
                        Vector3.up
                    );
                }

                distance = Vector3.Distance(
                    nextPosition,
                    targetPosition
                );

                if (!ProjectJCartPolicy.HasReachedNode(distance))
                {
                    return;
                }
            }

            transform.position = targetPosition;
            NetworkVisitedNodeCount++;

            ProjectJCartRouteNode nextNode =
                currentTargetNode.NextNode;

            bool canAdvance =
                ProjectJCartPolicy.CanAdvanceToNextNode(
                    NetworkVisitedNodeCount,
                    nextNode != null
                );

            if (!canAdvance)
            {
                currentTargetNode = null;
                FinishRideAuthority(false);
                return;
            }

            currentTargetNode = nextNode;
            FaceCurrentTargetAuthority();
        }

        private void FaceCurrentTargetAuthority()
        {
            if (currentTargetNode == null)
            {
                return;
            }

            Vector3 direction =
                currentTargetNode.transform.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );
        }

        private void CarryOwnerAuthority(ProjectJNetworkPlayer ownerPlayer)
        {
            if (ownerPlayer == null)
            {
                return;
            }

            Vector3 riderPosition =
                transform.position +
                Vector3.up * ProjectJCartPolicy.RiderVerticalOffset;

            ownerPlayer.transform.position = riderPosition;
        }

        private void ApplyContactPushAuthority()
        {
            processedTargetIndices.Clear();

            Vector3 contactCenter =
                transform.position + Vector3.up * 0.6f;

            int overlapCount = Physics.OverlapSphereNonAlloc(
                contactCenter,
                ProjectJCartPolicy.ContactRadius,
                contactBuffer,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            );

            for (int index = 0; index < overlapCount; index++)
            {
                Collider overlap = contactBuffer[index];
                contactBuffer[index] = null;

                if (overlap == null)
                {
                    continue;
                }

                ProjectJNetworkExternalGameplay target =
                    overlap.GetComponentInParent<ProjectJNetworkExternalGameplay>();

                if (
                    target == null ||
                    target.Object == null ||
                    !target.Object.IsValid ||
                    target.Object.InputAuthority == NetworkOwner
                )
                {
                    continue;
                }

                int targetIndex =
                    target.Object.InputAuthority.AsIndex;

                if (!processedTargetIndices.Add(targetIndex))
                {
                    continue;
                }

                if (
                    lastHitTimeByTarget.TryGetValue(
                        targetIndex,
                        out float lastHitTime
                    )
                )
                {
                    float elapsed = Time.time - lastHitTime;

                    if (!ProjectJCartPolicy.IsRehitReady(elapsed))
                    {
                        continue;
                    }
                }

                Vector3 sideDirection =
                    ProjectJCartPolicy.ResolveSidePushDirection(
                        transform.right,
                        transform.position,
                        target.transform.position
                    );

                bool applied = target.TryApplyExternalVelocityChange(
                    ProjectJExternalForceSource.Item,
                    sideDirection * ProjectJCartPolicy.SidePushSpeed
                );

                if (!applied)
                {
                    continue;
                }

                lastHitTimeByTarget[targetIndex] = Time.time;
                NetworkLastPushTargetIndex = targetIndex;
                NetworkPushSuccessCount++;
            }
        }

        private void ResolveOwner(
            out ProjectJNetworkExternalGameplay ownerGameplay,
            out ProjectJNetworkPlayer ownerPlayer,
            out ProjectJNetworkItemInventory ownerInventory
        )
        {
            ownerGameplay = null;
            ownerPlayer = null;
            ownerInventory = null;

            ProjectJNetworkExternalGameplay.CollectActivePlayers(
                Runner,
                playerBuffer
            );

            for (int index = 0; index < playerBuffer.Count; index++)
            {
                ProjectJNetworkExternalGameplay candidate =
                    playerBuffer[index];

                if (
                    candidate == null ||
                    candidate.Object == null ||
                    !candidate.Object.IsValid ||
                    candidate.Object.InputAuthority != NetworkOwner
                )
                {
                    continue;
                }

                ownerGameplay = candidate;
                ownerPlayer = candidate.GetComponent<ProjectJNetworkPlayer>();
                ownerInventory = candidate.GetComponent<ProjectJNetworkItemInventory>();
                return;
            }
        }

        private void FinishRideAuthority(bool jumpDismount)
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid
            )
            {
                return;
            }

            ResolveOwner(
                out ProjectJNetworkExternalGameplay ownerGameplay,
                out ProjectJNetworkPlayer ownerPlayer,
                out ProjectJNetworkItemInventory ownerInventory
            );

            if (ownerPlayer != null)
            {
                ownerPlayer.transform.position =
                    transform.position +
                    Vector3.up * ProjectJCartPolicy.RiderVerticalOffset;
            }

            if (
                ownerInventory != null &&
                ownerInventory.Object != null &&
                ownerInventory.Object.IsValid &&
                ownerInventory.Object.HasStateAuthority
            )
            {
                ownerInventory.SetCartRidingAuthority(false);
            }

            if (
                jumpDismount &&
                ownerGameplay != null &&
                ownerGameplay.GameplayInputAllowed &&
                ownerPlayer != null
            )
            {
                ownerPlayer.transform.position += Vector3.up * 0.1f;

                ownerPlayer.TrySetItemVerticalVelocityAuthority(
                    ownerPlayer.JumpSpeed
                );
            }

            Runner.Despawn(Object);
        }

        private void DisablePrefabRendererLocal()
        {
            Renderer rootRenderer = GetComponent<Renderer>();

            if (rootRenderer != null)
            {
                rootRenderer.enabled = false;
            }
        }

        private void EnsureWorldVisualLocal()
        {
            if (worldVisualObject != null)
            {
                return;
            }

            worldVisualObject = new GameObject("Cart Prototype Visual");
            worldVisualObject.transform.SetParent(transform, false);

            CreatePrimitiveVisual(
                PrimitiveType.Cube,
                "Cart Body",
                new Vector3(0f, 0.35f, 0f),
                new Vector3(1.4f, 0.45f, 2f),
                Quaternion.identity
            );

            CreateWheelVisual(new Vector3(-0.75f, 0.15f, 0.65f));
            CreateWheelVisual(new Vector3(0.75f, 0.15f, 0.65f));
            CreateWheelVisual(new Vector3(-0.75f, 0.15f, -0.65f));
            CreateWheelVisual(new Vector3(0.75f, 0.15f, -0.65f));
        }

        private void CreateWheelVisual(Vector3 localPosition)
        {
            CreatePrimitiveVisual(
                PrimitiveType.Cylinder,
                "Cart Wheel",
                localPosition,
                new Vector3(0.4f, 0.15f, 0.4f),
                Quaternion.Euler(0f, 0f, 90f)
            );
        }

        private void CreatePrimitiveVisual(
            PrimitiveType primitiveType,
            string objectName,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation
        )
        {
            GameObject visual = GameObject.CreatePrimitive(primitiveType);
            visual.name = objectName;
            visual.transform.SetParent(worldVisualObject.transform, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localRotation = localRotation;
            visual.transform.localScale = localScale;

            Collider generatedCollider = visual.GetComponent<Collider>();

            if (generatedCollider != null)
            {
                generatedCollider.enabled = false;
                UnityEngine.Object.Destroy(generatedCollider);
            }

            Renderer renderer = visual.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private void DestroyWorldVisualLocal()
        {
            if (worldVisualObject != null)
            {
                UnityEngine.Object.Destroy(worldVisualObject);
            }

            worldVisualObject = null;
        }
    }
}
