using Fusion; // NetworkBool과 NetworkObject 사용
using ProjectJ.Items; // 카트 정책과 Route Node 사용
using UnityEngine; // Resources와 Object 검색 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        private const string CartResourcePath =
            "ProjectJNetworkCart";

        private NetworkObject cartPrefab;

        [Networked]
        private NetworkBool NetworkCartRiding
        {
            get;
            set;
        }

        public bool IsCartRiding =>
            NetworkCartRiding;

        private void InitializeCartAuthority()
        {
            NetworkCartRiding = false;
        }

        private bool UseCartAuthority()
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

            ProjectJCartRouteNode startNode =
                ProjectJCartRouteNode.FindNearest(transform.position);

            bool hasExistingCart =
                HasOwnedCartAuthority();

            if (
                !ProjectJCartPolicy.CanUse(
                    runnerReady,
                    gameplayAllowed,
                    IsCartRiding,
                    startNode != null,
                    hasExistingCart
                )
            )
            {
                return false;
            }

            NetworkObject resolvedPrefab =
                ResolveCartPrefab();

            if (resolvedPrefab == null)
            {
                Debug.LogError(
                    "[Project J/Fusion] 123일차 카트 Prefab을 찾을 수 없음",
                    this
                );

                return false;
            }

            Vector3 spawnPosition = transform.position;
            Quaternion spawnRotation = transform.rotation;

            Vector3 directionToNode =
                startNode.transform.position - spawnPosition;
            directionToNode.y = 0f;

            if (directionToNode.sqrMagnitude > 0.0001f)
            {
                spawnRotation = Quaternion.LookRotation(
                    directionToNode.normalized,
                    Vector3.up
                );
            }

            NetworkObject cartObject = Runner.Spawn(
                resolvedPrefab,
                spawnPosition,
                spawnRotation,
                Object.InputAuthority
            );

            if (cartObject == null)
            {
                return false;
            }

            ProjectJNetworkCart cart =
                cartObject.GetComponent<ProjectJNetworkCart>();

            if (
                cart == null ||
                !cart.ConfigureAuthority(
                    Object.InputAuthority,
                    startNode
                )
            )
            {
                Runner.Despawn(cartObject);
                return false;
            }

            SetCartRidingAuthority(true);
            return true;
        }

        internal void SetCartRidingAuthority(bool riding)
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return;
            }

            NetworkCartRiding = riding;
        }

        private void ClearCartAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return;
            }

            NetworkCartRiding = false;

            ProjectJNetworkCart[] carts =
                UnityEngine.Object.FindObjectsByType<ProjectJNetworkCart>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            for (int index = 0; index < carts.Length; index++)
            {
                ProjectJNetworkCart cart = carts[index];

                if (
                    cart == null ||
                    cart.Runner != Runner ||
                    !cart.IsInitialized ||
                    cart.Owner != Object.InputAuthority
                )
                {
                    continue;
                }

                cart.FinishFromInventoryAuthority();
            }
        }

        private bool HasOwnedCartAuthority()
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid
            )
            {
                return false;
            }

            ProjectJNetworkCart[] carts =
                UnityEngine.Object.FindObjectsByType<ProjectJNetworkCart>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            for (int index = 0; index < carts.Length; index++)
            {
                ProjectJNetworkCart cart = carts[index];

                if (
                    cart != null &&
                    cart.Runner == Runner &&
                    cart.IsInitialized &&
                    cart.Owner == Object.InputAuthority
                )
                {
                    return true;
                }
            }

            return false;
        }

        private NetworkObject ResolveCartPrefab()
        {
            if (cartPrefab == null)
            {
                GameObject prefabObject =
                    Resources.Load<GameObject>(CartResourcePath);

                cartPrefab =
                    prefabObject != null
                        ? prefabObject.GetComponent<NetworkObject>()
                        : null;
            }

            return cartPrefab;
        }
    }
}
