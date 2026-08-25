using Fusion; // NetworkObject와 PlayerRef 사용
using ProjectJ.Items; // 유도탄 정책 사용
using UnityEngine; // Resources와 Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        private const string HomingMissileResourcePath =
            "ProjectJNetworkHomingMissile"; // Resources Prefab 이름

        private NetworkObject homingMissilePrefab;

        private bool UseHomingMissileAuthority()
        {
            bool runnerReady =
                Runner != null &&
                Runner.IsServer &&
                Object != null &&
                Object.IsValid &&
                Object.HasStateAuthority &&
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            ProjectJNetworkExternalGameplay target =
                runnerReady
                    ? ProjectJNetworkHomingMissile.FindNearestTargetAuthority(
                        Runner,
                        Object.InputAuthority,
                        transform.position,
                        -1
                    )
                    : null;

            if (
                !ProjectJHomingMissilePolicy.CanSpawn(
                    runnerReady,
                    target != null
                )
            )
            {
                return false;
            }

            NetworkObject prefab =
                ResolveHomingMissilePrefab();

            if (prefab == null)
            {
                Debug.LogError(
                    "[Project J/Fusion] 125일차 유도탄 Prefab을 찾을 수 없음",
                    this
                );

                return false;
            }

            Vector3 forward =
                target.transform.position -
                transform.position;

            forward.y =
                0f;

            if (
                forward.sqrMagnitude <=
                0.0001f
            )
            {
                forward =
                    transform.forward;
            }

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
                Vector3.up * 1.15f +
                forward * 0.85f;

            Vector3 targetPoint =
                target.transform.position +
                Vector3.up *
                ProjectJHomingMissilePolicy.TargetHeightOffset;

            Vector3 initialDirection =
                targetPoint -
                spawnPosition;

            if (
                initialDirection.sqrMagnitude <=
                0.0001f
            )
            {
                initialDirection =
                    forward;
            }

            NetworkObject missileObject =
                Runner.Spawn(
                    prefab,
                    spawnPosition,
                    Quaternion.LookRotation(
                        initialDirection.normalized
                    ),
                    Object.InputAuthority
                );

            if (missileObject == null)
            {
                return false;
            }

            ProjectJNetworkHomingMissile missile =
                missileObject.GetComponent<ProjectJNetworkHomingMissile>();

            if (
                missile == null ||
                !missile.ConfigureAuthority(
                    Object.InputAuthority,
                    target.Object.InputAuthority
                )
            )
            {
                Runner.Despawn(
                    missileObject
                );

                return false;
            }

            return true;
        }

        private NetworkObject ResolveHomingMissilePrefab()
        {
            if (homingMissilePrefab == null)
            {
                GameObject prefabObject =
                    Resources.Load<GameObject>(
                        HomingMissileResourcePath
                    );

                homingMissilePrefab =
                    prefabObject != null
                        ? prefabObject.GetComponent<NetworkObject>()
                        : null;
            }

            return homingMissilePrefab;
        }
    }
}
