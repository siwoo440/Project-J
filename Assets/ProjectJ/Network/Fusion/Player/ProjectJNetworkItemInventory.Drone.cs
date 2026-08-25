using Fusion; // NetworkObject와 PlayerRef 사용
using ProjectJ.Items; // 드론 정책 사용
using UnityEngine; // Resources와 Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        private const string DroneResourcePath =
            "ProjectJNetworkDrone"; // Resources Prefab 이름

        private NetworkObject dronePrefab;

        private bool UseDroneAuthority()
        {
            ResolveReferences();

            bool runnerReady =
                Runner != null &&
                Runner.IsServer &&
                Object != null &&
                Object.IsValid &&
                Object.HasStateAuthority;

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            int ownerRaceRank =
                externalGameplay != null
                    ? externalGameplay.RaceRank
                    : 0;

            ProjectJNetworkExternalGameplay target =
                runnerReady &&
                gameplayAllowed &&
                ownerRaceRank > 1
                    ? ProjectJNetworkDrone.FindInitialLeaderAuthority(
                        Runner,
                        Object.InputAuthority
                    )
                    : null;

            if (
                !ProjectJDronePolicy.CanUse(
                    runnerReady,
                    gameplayAllowed,
                    ownerRaceRank,
                    target != null
                )
            )
            {
                return false;
            }

            NetworkObject prefab =
                ResolveDronePrefab();

            if (prefab == null)
            {
                Debug.LogError(
                    "[Project J/Fusion] 128일차 드론 Prefab을 찾을 수 없음",
                    this
                );

                return false;
            }

            Vector3 targetPoint =
                target.transform.position +
                Vector3.up *
                ProjectJDronePolicy.TargetHeightOffset;

            Vector3 direction =
                targetPoint -
                transform.position;

            if (
                direction.sqrMagnitude <=
                0.0001f
            )
            {
                direction =
                    transform.forward;
            }

            if (
                direction.sqrMagnitude <=
                0.0001f
            )
            {
                direction =
                    Vector3.forward;
            }

            direction.Normalize();

            Vector3 spawnPosition =
                transform.position +
                Vector3.up * 1.6f +
                direction * 0.8f;

            NetworkObject droneObject =
                Runner.Spawn(
                    prefab,
                    spawnPosition,
                    Quaternion.LookRotation(
                        direction,
                        Vector3.up
                    ),
                    Object.InputAuthority
                );

            if (droneObject == null)
            {
                return false;
            }

            ProjectJNetworkDrone drone =
                droneObject.GetComponent<ProjectJNetworkDrone>();

            if (
                drone == null ||
                !drone.ConfigureAuthority(
                    Object.InputAuthority,
                    target.Object.InputAuthority
                )
            )
            {
                Runner.Despawn(
                    droneObject
                );

                return false;
            }

            return true;
        }

        private NetworkObject ResolveDronePrefab()
        {
            if (dronePrefab == null)
            {
                GameObject prefabObject =
                    Resources.Load<GameObject>(
                        DroneResourcePath
                    );

                dronePrefab =
                    prefabObject != null
                        ? prefabObject.GetComponent<NetworkObject>()
                        : null;
            }

            return dronePrefab;
        }
    }
}
