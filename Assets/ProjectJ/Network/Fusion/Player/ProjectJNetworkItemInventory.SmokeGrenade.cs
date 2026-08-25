using Fusion; // NetworkObject 사용
using ProjectJ.Checkpoint; // 현재 Checkpoint 낙하 한계 재사용
using ProjectJ.Items; // 연막탄 정책 사용
using UnityEngine; // Resources와 Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        private const string SmokeGrenadeProjectileResourcePath =
            "ProjectJNetworkSmokeGrenadeProjectile";

        private NetworkObject smokeGrenadeProjectilePrefab;
        private CheckpointFallLimitSet smokeGrenadeFallLimitSet;

        private bool UseSmokeGrenadeAuthority()
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
                !ProjectJSmokeGrenadePolicy.CanThrow(
                    runnerReady,
                    gameplayAllowed
                )
            )
            {
                return false;
            }

            NetworkObject resolvedPrefab =
                ResolveSmokeGrenadeProjectilePrefab();

            if (resolvedPrefab == null)
            {
                Debug.LogError(
                    "[Project J/Fusion] 120일차 연막탄 Prefab을 찾을 수 없음",
                    this
                );

                return false;
            }

            Vector3 forward =
                transform.forward;
            forward.y =
                0f;

            if (
                forward.sqrMagnitude <=
                0.0001f
            )
            {
                forward =
                    Vector3.forward;
            }

            forward.Normalize();

            Vector3 spawnPosition =
                transform.position +
                Vector3.up * 1.2f +
                forward * 0.9f;

            NetworkObject projectileObject =
                Runner.Spawn(
                    resolvedPrefab,
                    spawnPosition,
                    Quaternion.LookRotation(
                        forward
                    ),
                    Object.InputAuthority
                );

            if (projectileObject == null)
            {
                return false;
            }

            ProjectJNetworkSmokeGrenadeProjectile projectile =
                projectileObject.GetComponent<ProjectJNetworkSmokeGrenadeProjectile>();

            float fallLimitY =
                ResolveSmokeGrenadeFallLimitY();

            if (
                projectile == null ||
                !projectile.ConfigureAuthority(
                    Object.InputAuthority,
                    forward,
                    fallLimitY
                )
            )
            {
                Runner.Despawn(
                    projectileObject
                );
                return false;
            }

            return true;
        }

        private float ResolveSmokeGrenadeFallLimitY()
        {
            if (smokeGrenadeFallLimitSet == null)
            {
                smokeGrenadeFallLimitSet =
                    UnityEngine.Object.FindFirstObjectByType<CheckpointFallLimitSet>();
            }

            if (
                smokeGrenadeFallLimitSet == null ||
                externalGameplay == null
            )
            {
                return -20f;
            }

            return
                smokeGrenadeFallLimitSet.GetFallLimitY(
                    externalGameplay.CurrentCheckpointId
                );
        }

        private NetworkObject ResolveSmokeGrenadeProjectilePrefab()
        {
            if (smokeGrenadeProjectilePrefab == null)
            {
                GameObject prefabObject =
                    Resources.Load<GameObject>(
                        SmokeGrenadeProjectileResourcePath
                    );

                smokeGrenadeProjectilePrefab =
                    prefabObject != null
                        ? prefabObject.GetComponent<NetworkObject>()
                        : null;
            }

            return
                smokeGrenadeProjectilePrefab;
        }
    }
}
