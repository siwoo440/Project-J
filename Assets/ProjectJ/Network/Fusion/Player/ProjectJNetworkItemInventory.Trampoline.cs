using Fusion; // NetworkObject 사용
using ProjectJ.Items; // 트램폴린 정책 사용
using UnityEngine; // Raycast와 Resources 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        private const string TrampolineResourcePath =
            "ProjectJNetworkTrampoline";

        private readonly RaycastHit[] trampolineGroundHitBuffer =
            new RaycastHit[24];

        private NetworkObject trampolinePrefab;

        private bool UseTrampolineAuthority()
        {
            bool runnerReady =
                Runner != null &&
                Runner.IsServer &&
                Object != null &&
                Object.IsValid &&
                Object.HasStateAuthority;

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            if (
                !ProjectJTrampolinePolicy.CanInstall(
                    runnerReady,
                    gameplayAllowed
                )
            )
            {
                return false;
            }

            if (
                !TryFindTrampolineGroundAuthority(
                    out RaycastHit groundHit
                )
            )
            {
                return false;
            }

            NetworkObject resolvedPrefab =
                ResolveTrampolinePrefab();

            if (resolvedPrefab == null)
            {
                Debug.LogError(
                    "[Project J/Fusion] 121일차 트램폴린 Prefab을 찾을 수 없음",
                    this
                );

                return false;
            }

            RemoveExistingTrampolineAuthority();

            Vector3 spawnPosition =
                groundHit.point +
                groundHit.normal * 0.05f;

            Quaternion spawnRotation =
                Quaternion.FromToRotation(
                    Vector3.up,
                    groundHit.normal
                );

            NetworkObject trampolineObject =
                Runner.Spawn(
                    resolvedPrefab,
                    spawnPosition,
                    spawnRotation,
                    Object.InputAuthority
                );

            if (trampolineObject == null)
            {
                return false;
            }

            ProjectJNetworkTrampoline trampoline =
                trampolineObject.GetComponent<ProjectJNetworkTrampoline>();

            if (
                trampoline == null ||
                !trampoline.ConfigureAuthority(
                    Object.InputAuthority
                )
            )
            {
                Runner.Despawn(
                    trampolineObject
                );

                return false;
            }

            return true;
        }

        private bool TryFindTrampolineGroundAuthority(
            out RaycastHit groundHit
        )
        {
            groundHit =
                default;

            Vector3 origin =
                transform.position +
                Vector3.up *
                ProjectJTrampolinePolicy.InstallRayStartHeight;

            int hitCount =
                Physics.RaycastNonAlloc(
                    origin,
                    Vector3.down,
                    trampolineGroundHitBuffer,
                    ProjectJTrampolinePolicy.InstallRayDistance,
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
                    trampolineGroundHitBuffer[index].collider;

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

                RaycastHit candidate =
                    trampolineGroundHitBuffer[index];

                if (
                    !ProjectJTrampolinePolicy.IsValidInstallSurface(
                        candidate.normal.y,
                        candidate.distance
                    )
                )
                {
                    continue;
                }

                if (
                    candidate.distance >=
                    nearestDistance
                )
                {
                    continue;
                }

                nearestIndex =
                    index;

                nearestDistance =
                    candidate.distance;
            }

            if (nearestIndex < 0)
            {
                return false;
            }

            groundHit =
                trampolineGroundHitBuffer[nearestIndex];

            return true;
        }

        private void RemoveExistingTrampolineAuthority()
        {
            ProjectJNetworkTrampoline[] trampolines =
                UnityEngine.Object.FindObjectsByType<ProjectJNetworkTrampoline>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            for (
                int index = 0;
                index < trampolines.Length;
                index++
            )
            {
                ProjectJNetworkTrampoline trampoline =
                    trampolines[index];

                if (
                    trampoline == null ||
                    trampoline.Runner != Runner ||
                    !trampoline.IsInitialized ||
                    trampoline.Owner !=
                    Object.InputAuthority
                )
                {
                    continue;
                }

                trampoline.DespawnForReplacementAuthority();
            }
        }

        private NetworkObject ResolveTrampolinePrefab()
        {
            if (trampolinePrefab == null)
            {
                GameObject prefabObject =
                    Resources.Load<GameObject>(
                        TrampolineResourcePath
                    );

                trampolinePrefab =
                    prefabObject != null
                        ? prefabObject.GetComponent<NetworkObject>()
                        : null;
            }

            return trampolinePrefab;
        }
    }
}
