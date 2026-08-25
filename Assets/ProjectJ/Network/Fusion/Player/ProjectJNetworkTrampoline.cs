using System.Collections.Generic; // Owner Player 조회용 목록
using Fusion; // NetworkBehaviour, PlayerRef, TickTimer 사용
using ProjectJ.Items; // 트램폴린 정책 사용
using UnityEngine; // 위치, 시각화 사용
using UnityEngine.Rendering; // 그림자 비활성화 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public sealed class ProjectJNetworkTrampoline :
        NetworkBehaviour
    {
        private readonly List<ProjectJNetworkExternalGameplay> playerBuffer =
            new List<ProjectJNetworkExternalGameplay>(8);

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
        private int NetworkUseCount
        {
            get;
            set;
        }

        [Networked]
        private NetworkBool NetworkOwnerInsideActivationArea
        {
            get;
            set;
        }

        [Networked]
        private float NetworkLastLaunchSpeed
        {
            get;
            set;
        }

        public bool IsInitialized =>
            NetworkInitialized;

        public PlayerRef Owner =>
            NetworkOwner;

        public int UseCount =>
            NetworkUseCount;

        public float LastLaunchSpeed =>
            NetworkLastLaunchSpeed;

        public bool IsLifetimeActive =>
            NetworkInitialized &&
            Runner != null &&
            !NetworkLifetimeTimer.ExpiredOrNotRunning(Runner);

        public override void Spawned()
        {
            DisablePrefabRendererLocal();
            EnsureWorldVisualLocal();
        }

        public override void Despawned(
            NetworkRunner runner,
            bool hasState
        )
        {
            DestroyWorldVisualLocal();
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

            NetworkOwner =
                owner;

            NetworkLifetimeTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    ProjectJTrampolinePolicy.LifetimeSeconds
                );

            NetworkUseCount =
                0;

            NetworkOwnerInsideActivationArea =
                false;

            NetworkLastLaunchSpeed =
                0f;

            NetworkInitialized =
                true;

            return true;
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority)
            {
                return;
            }

            if (
                !NetworkInitialized ||
                Runner == null
            )
            {
                DespawnAuthority();
                return;
            }

            ProjectJNetworkExternalGameplay owner =
                FindOwner();

            bool ownerMissing =
                owner == null ||
                owner.Object == null ||
                !owner.Object.IsValid;

            bool lifetimeActive =
                !NetworkLifetimeTimer.ExpiredOrNotRunning(
                    Runner
                );

            if (
                ProjectJTrampolinePolicy.ShouldDespawn(
                    lifetimeActive,
                    ownerMissing,
                    NetworkUseCount
                )
            )
            {
                DespawnAuthority();
                return;
            }

            if (!owner.GameplayInputAllowed)
            {
                DespawnAuthority();
                return;
            }

            ProjectJNetworkPlayer ownerPlayer =
                owner.GetComponent<ProjectJNetworkPlayer>();

            if (ownerPlayer == null)
            {
                DespawnAuthority();
                return;
            }

            float horizontalDistance =
                GetHorizontalDistance(
                    transform.position,
                    owner.transform.position
                );

            float verticalOffset =
                owner.transform.position.y -
                transform.position.y;

            bool insideActivationArea =
                ProjectJTrampolinePolicy.IsWithinActivationArea(
                    horizontalDistance,
                    verticalOffset
                );

            if (!insideActivationArea)
            {
                NetworkOwnerInsideActivationArea =
                    false;
                return;
            }

            if (NetworkOwnerInsideActivationArea)
            {
                return;
            }

            bool canActivate =
                ProjectJTrampolinePolicy.CanActivateOwner(
                    true,
                    owner.GameplayInputAllowed,
                    NetworkUseCount,
                    ownerPlayer.IsGrounded,
                    ownerPlayer.VerticalVelocity,
                    horizontalDistance,
                    verticalOffset
                );

            if (!canActivate)
            {
                return;
            }

            NetworkOwnerInsideActivationArea =
                true;

            float launchSpeed =
                ProjectJTrampolinePolicy.GetLaunchSpeed(
                    NetworkUseCount
                );

            if (
                !owner.TrySetTrampolineLaunchAuthority(
                    launchSpeed
                )
            )
            {
                return;
            }

            NetworkLastLaunchSpeed =
                launchSpeed;

            NetworkUseCount =
                ProjectJTrampolinePolicy.GetNextUseCount(
                    NetworkUseCount
                );

            if (
                ProjectJTrampolinePolicy.HasConsumedAllUses(
                    NetworkUseCount
                )
            )
            {
                DespawnAuthority();
            }
        }

        public override void Render()
        {
            EnsureWorldVisualLocal();
            UpdateWorldVisualLocal();
            DrawDebugActivationArea();
        }

        internal void DespawnForReplacementAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return;
            }

            DespawnAuthority();
        }

        private ProjectJNetworkExternalGameplay FindOwner()
        {
            ProjectJNetworkExternalGameplay.CollectActivePlayers(
                Runner,
                playerBuffer
            );

            for (
                int index = 0;
                index < playerBuffer.Count;
                index++
            )
            {
                ProjectJNetworkExternalGameplay player =
                    playerBuffer[index];

                if (
                    player == null ||
                    player.Object == null ||
                    !player.Object.IsValid ||
                    player.Object.InputAuthority !=
                    NetworkOwner
                )
                {
                    continue;
                }

                return player;
            }

            return null;
        }

        private static float GetHorizontalDistance(
            Vector3 a,
            Vector3 b
        )
        {
            Vector2 horizontalA =
                new Vector2(
                    a.x,
                    a.z
                );

            Vector2 horizontalB =
                new Vector2(
                    b.x,
                    b.z
                );

            return Vector2.Distance(
                horizontalA,
                horizontalB
            );
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

            worldVisualObject =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder
                );

            worldVisualObject.name =
                "Trampoline Prototype Visual";

            worldVisualObject.transform.SetParent(
                transform,
                false
            );

            Collider generatedCollider =
                worldVisualObject.GetComponent<Collider>();

            if (generatedCollider != null)
            {
                generatedCollider.enabled =
                    false;

                UnityEngine.Object.Destroy(
                    generatedCollider
                );
            }

            Renderer renderer =
                worldVisualObject.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.shadowCastingMode =
                    ShadowCastingMode.Off;

                renderer.receiveShadows =
                    false;
            }

            UpdateWorldVisualLocal();
        }

        private void UpdateWorldVisualLocal()
        {
            if (worldVisualObject == null)
            {
                return;
            }

            float stageHeight =
                0.12f +
                Mathf.Clamp(
                    NetworkUseCount,
                    0,
                    ProjectJTrampolinePolicy.MaximumUseCount
                ) * 0.025f;

            worldVisualObject.transform.localPosition =
                Vector3.up *
                stageHeight;

            worldVisualObject.transform.localScale =
                new Vector3(
                    ProjectJTrampolinePolicy.ActivationRadius * 2f,
                    stageHeight,
                    ProjectJTrampolinePolicy.ActivationRadius * 2f
                );
        }

        private void DestroyWorldVisualLocal()
        {
            if (worldVisualObject != null)
            {
                UnityEngine.Object.Destroy(
                    worldVisualObject
                );
            }

            worldVisualObject =
                null;
        }

        private void DrawDebugActivationArea()
        {
            Vector3 center =
                transform.position;

            Debug.DrawLine(
                center + Vector3.left *
                ProjectJTrampolinePolicy.ActivationRadius,
                center + Vector3.right *
                ProjectJTrampolinePolicy.ActivationRadius
            );

            Debug.DrawLine(
                center + Vector3.back *
                ProjectJTrampolinePolicy.ActivationRadius,
                center + Vector3.forward *
                ProjectJTrampolinePolicy.ActivationRadius
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

            Runner.Despawn(
                Object
            );
        }
    }
}
