using System.Collections.Generic; // Owner Player 조회용 목록
using Fusion; // NetworkBehaviour와 PlayerRef 사용
using ProjectJ.Items; // 연막탄 정책 사용
using UnityEngine; // 포물선 이동과 물리 판정 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public sealed class ProjectJNetworkSmokeGrenadeProjectile :
        NetworkBehaviour
    {
        private const string SmokeZoneResourcePath =
            "ProjectJNetworkSmokeZone";

        private static NetworkObject smokeZonePrefab;

        private readonly RaycastHit[] hitBuffer =
            new RaycastHit[24];

        private readonly List<ProjectJNetworkExternalGameplay> ownerBuffer =
            new List<ProjectJNetworkExternalGameplay>(8);

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
        private Vector3 NetworkThrowOrigin
        {
            get;
            set;
        }

        [Networked]
        private Vector3 NetworkVelocity
        {
            get;
            set;
        }

        [Networked]
        private float NetworkFallLimitY
        {
            get;
            set;
        }

        public PlayerRef Owner =>
            NetworkOwner;

        public bool IsInitialized =>
            NetworkInitialized;

        public bool ConfigureAuthority(
            PlayerRef owner,
            Vector3 forward,
            float fallLimitY
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
            NetworkThrowOrigin =
                transform.position;
            NetworkVelocity =
                ProjectJSmokeGrenadePolicy.CreateInitialVelocity(
                    forward
                );
            NetworkFallLimitY =
                fallLimitY;
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

            if (!IsOwnerGameplayActive())
            {
                DespawnAuthority();
                return;
            }

            if (
                ProjectJSmokeGrenadePolicy.IsBelowFallLimit(
                    transform.position.y,
                    NetworkFallLimitY
                )
            )
            {
                DespawnAuthority();
                return;
            }

            SimulateThrowAuthority();
        }

        private void SimulateThrowAuthority()
        {
            float deltaTime =
                Runner.DeltaTime;

            Vector3 velocity =
                NetworkVelocity;

            velocity +=
                Vector3.up *
                ProjectJSmokeGrenadePolicy.PrototypeGravity *
                deltaTime;

            float horizontalDistance =
                ProjectJSmokeGrenadePolicy.GetHorizontalDistance(
                    NetworkThrowOrigin,
                    transform.position
                );

            if (
                horizontalDistance >=
                ProjectJSmokeGrenadePolicy.MaximumThrowDistance
            )
            {
                velocity.x =
                    0f;
                velocity.z =
                    0f;
            }

            Vector3 step =
                velocity * deltaTime;

            float stepDistance =
                step.magnitude;

            if (
                stepDistance > 0.0001f &&
                TryResolveTerrainCollision(
                    step / stepDistance,
                    stepDistance,
                    out RaycastHit hit
                )
            )
            {
                Vector3 smokePosition =
                    hit.point +
                    hit.normal * 0.05f;

                SpawnSmokeZoneAuthority(
                    smokePosition
                );

                DespawnAuthority();
                return;
            }

            Vector3 nextPosition =
                transform.position +
                step;

            float nextHorizontalDistance =
                ProjectJSmokeGrenadePolicy.GetHorizontalDistance(
                    NetworkThrowOrigin,
                    nextPosition
                );

            if (
                nextHorizontalDistance >
                ProjectJSmokeGrenadePolicy.MaximumThrowDistance
            )
            {
                Vector3 horizontal =
                    nextPosition -
                    NetworkThrowOrigin;

                horizontal.y =
                    0f;

                if (
                    horizontal.sqrMagnitude >
                    0.0001f
                )
                {
                    horizontal =
                        horizontal.normalized *
                        ProjectJSmokeGrenadePolicy.MaximumThrowDistance;

                    nextPosition.x =
                        NetworkThrowOrigin.x +
                        horizontal.x;

                    nextPosition.z =
                        NetworkThrowOrigin.z +
                        horizontal.z;
                }

                velocity.x =
                    0f;
                velocity.z =
                    0f;
            }

            transform.position =
                nextPosition;
            NetworkVelocity =
                velocity;
        }

        private bool TryResolveTerrainCollision(
            Vector3 direction,
            float stepDistance,
            out RaycastHit nearestHit
        )
        {
            nearestHit =
                default;

            int hitCount =
                Physics.SphereCastNonAlloc(
                    transform.position,
                    ProjectJSmokeGrenadePolicy.CollisionRadius,
                    direction,
                    hitBuffer,
                    stepDistance,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore
                );

            int nearestIndex =
                -1;

            float nearestDistance =
                float.MaxValue;

            for (
                int index = 0;
                index < hitCount;
                index++
            )
            {
                Collider hitCollider =
                    hitBuffer[index].collider;

                if (hitCollider == null)
                {
                    continue;
                }

                ProjectJNetworkExternalGameplay player =
                    hitCollider.GetComponentInParent<ProjectJNetworkExternalGameplay>();

                if (player != null)
                {
                    continue;
                }

                if (
                    hitBuffer[index].distance >=
                    nearestDistance
                )
                {
                    continue;
                }

                nearestIndex =
                    index;
                nearestDistance =
                    hitBuffer[index].distance;
            }

            if (nearestIndex < 0)
            {
                return false;
            }

            nearestHit =
                hitBuffer[nearestIndex];

            return true;
        }

        private bool IsOwnerGameplayActive()
        {
            ProjectJNetworkExternalGameplay.CollectActivePlayers(
                Runner,
                ownerBuffer
            );

            for (
                int index = 0;
                index < ownerBuffer.Count;
                index++
            )
            {
                ProjectJNetworkExternalGameplay player =
                    ownerBuffer[index];

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

                return
                    player.GameplayInputAllowed;
            }

            return false;
        }

        private void SpawnSmokeZoneAuthority(
            Vector3 position
        )
        {
            NetworkObject resolvedPrefab =
                ResolveSmokeZonePrefab();

            if (resolvedPrefab == null)
            {
                Debug.LogError(
                    "[Project J/Fusion] 120일차 Smoke Zone Prefab을 찾을 수 없음",
                    this
                );

                return;
            }

            NetworkObject zoneObject =
                Runner.Spawn(
                    resolvedPrefab,
                    position,
                    Quaternion.identity,
                    NetworkOwner
                );

            if (zoneObject == null)
            {
                return;
            }

            ProjectJNetworkSmokeZone zone =
                zoneObject.GetComponent<ProjectJNetworkSmokeZone>();

            if (
                zone == null ||
                !zone.ConfigureAuthority(
                    NetworkOwner
                )
            )
            {
                Runner.Despawn(
                    zoneObject
                );
            }
        }

        private NetworkObject ResolveSmokeZonePrefab()
        {
            if (smokeZonePrefab == null)
            {
                GameObject prefabObject =
                    Resources.Load<GameObject>(
                        SmokeZoneResourcePath
                    );

                smokeZonePrefab =
                    prefabObject != null
                        ? prefabObject.GetComponent<NetworkObject>()
                        : null;
            }

            return
                smokeZonePrefab;
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
