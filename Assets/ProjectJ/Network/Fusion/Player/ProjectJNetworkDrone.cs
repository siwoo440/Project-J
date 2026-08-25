using System.Collections.Generic; // 경로 탐색 컬렉션 사용
using Fusion; // NetworkBehaviour, PlayerRef, TickTimer 사용
using ProjectJ.Items; // 드론 정책과 유도탄 Route Node 재사용
using UnityEngine; // 물리 판정과 Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public sealed class ProjectJNetworkDrone :
        NetworkBehaviour
    {
        private const int CastBufferSize = 32;

        private readonly RaycastHit[] castBuffer =
            new RaycastHit[CastBufferSize];

        private readonly List<ProjectJNetworkExternalGameplay> playerBuffer =
            new List<ProjectJNetworkExternalGameplay>(8);

        private readonly List<ProjectJHomingMissileRouteNode> routeNodes =
            new List<ProjectJHomingMissileRouteNode>(32);

        private readonly List<ProjectJHomingMissileRouteNode> routePath =
            new List<ProjectJHomingMissileRouteNode>(16);

        private readonly List<ProjectJHomingMissileRouteNode> adjacentBuffer =
            new List<ProjectJHomingMissileRouteNode>(8);

        private readonly Queue<ProjectJHomingMissileRouteNode> routeQueue =
            new Queue<ProjectJHomingMissileRouteNode>(32);

        private readonly Dictionary<ProjectJHomingMissileRouteNode, ProjectJHomingMissileRouteNode> routeParents =
            new Dictionary<ProjectJHomingMissileRouteNode, ProjectJHomingMissileRouteNode>(32);

        private readonly HashSet<ProjectJHomingMissileRouteNode> visitedNodes =
            new HashSet<ProjectJHomingMissileRouteNode>();

        private int routePathIndex;

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
        private PlayerRef NetworkTarget
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
        private int NetworkReacquireCount
        {
            get;
            set;
        }

        [Networked]
        private int NetworkTargetRevision
        {
            get;
            set;
        }

        public PlayerRef Owner =>
            NetworkOwner;

        public PlayerRef Target =>
            NetworkTarget;

        public int ReacquireCount =>
            NetworkReacquireCount;

        public int TargetRevision =>
            NetworkTargetRevision;

        public bool ConfigureAuthority(
            PlayerRef owner,
            PlayerRef target
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

            ProjectJNetworkExternalGameplay resolvedTarget =
                ResolvePlayerByRef(
                    target
                );

            if (
                resolvedTarget == null ||
                !IsEligibleTarget(
                    resolvedTarget,
                    owner
                )
            )
            {
                return false;
            }

            NetworkOwner =
                owner;

            NetworkTarget =
                target;

            NetworkLifetimeTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    ProjectJDronePolicy.LifetimeSeconds
                );

            NetworkReacquireCount =
                0;

            NetworkTargetRevision =
                1;

            NetworkInitialized =
                true;

            ClearRouteAuthority();

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

            if (
                NetworkLifetimeTimer.ExpiredOrNotRunning(
                    Runner
                )
            )
            {
                DespawnAuthority();
                return;
            }

            ProjectJNetworkExternalGameplay owner =
                ResolvePlayerByRef(
                    NetworkOwner
                );

            if (
                owner == null ||
                !owner.GameplayInputAllowed
            )
            {
                DespawnAuthority();
                return;
            }

            ProjectJNetworkExternalGameplay target =
                ResolvePlayerByRef(
                    NetworkTarget
                );

            if (
                target == null ||
                !IsEligibleTarget(
                    target,
                    NetworkOwner
                )
            )
            {
                if (!TryReacquireAuthority())
                {
                    DespawnAuthority();
                }

                return;
            }

            Vector3 targetPoint =
                target.transform.position +
                Vector3.up *
                ProjectJDronePolicy.TargetHeightOffset;

            float targetDistance =
                Vector3.Distance(
                    transform.position,
                    targetPoint
                );

            bool directPathClear =
                HasClearWorldPath(
                    transform.position,
                    targetPoint
                );

            if (
                directPathClear &&
                ProjectJDronePolicy.HasReachedAttackDistance(
                    targetDistance
                )
            )
            {
                AttackAndDespawnAuthority(
                    target,
                    targetPoint -
                    transform.position
                );

                return;
            }

            Vector3 moveTarget;

            if (directPathClear)
            {
                ClearRouteAuthority();

                moveTarget =
                    targetPoint;
            }
            else
            {
                if (
                    routePath.Count == 0 ||
                    routePathIndex >= routePath.Count
                )
                {
                    if (
                        !TryBuildRouteAuthority(
                            targetPoint
                        )
                    )
                    {
                        if (!TryReacquireAuthority())
                        {
                            DespawnAuthority();
                        }

                        return;
                    }
                }

                ProjectJHomingMissileRouteNode currentNode =
                    routePath[routePathIndex];

                if (currentNode == null)
                {
                    ClearRouteAuthority();
                    return;
                }

                float nodeDistance =
                    Vector3.Distance(
                        transform.position,
                        currentNode.transform.position
                    );

                if (
                    ProjectJDronePolicy.HasReachedRouteNode(
                        nodeDistance
                    )
                )
                {
                    routePathIndex++;

                    if (
                        routePathIndex >=
                        routePath.Count
                    )
                    {
                        ClearRouteAuthority();
                        return;
                    }

                    currentNode =
                        routePath[routePathIndex];
                }

                moveTarget =
                    currentNode.transform.position;
            }

            Vector3 movementDirection =
                moveTarget -
                transform.position;

            if (
                movementDirection.sqrMagnitude <=
                0.0001f
            )
            {
                return;
            }

            movementDirection.Normalize();

            float stepDistance =
                ProjectJDronePolicy.CalculateStepDistance(
                    Runner.DeltaTime
                );

            float remainingDistance =
                Vector3.Distance(
                    transform.position,
                    moveTarget
                );

            stepDistance =
                Mathf.Min(
                    stepDistance,
                    remainingDistance
                );

            if (stepDistance <= 0f)
            {
                return;
            }

            if (
                HasWorldCollisionAhead(
                    movementDirection,
                    stepDistance
                )
            )
            {
                ClearRouteAuthority();

                if (
                    !TryBuildRouteAuthority(
                        targetPoint
                    ) &&
                    !TryReacquireAuthority()
                )
                {
                    DespawnAuthority();
                }

                return;
            }

            transform.position +=
                movementDirection *
                stepDistance;

            transform.rotation =
                Quaternion.LookRotation(
                    movementDirection,
                    Vector3.up
                );
        }

        internal static ProjectJNetworkExternalGameplay FindInitialLeaderAuthority(
            NetworkRunner runner,
            PlayerRef owner
        )
        {
            if (runner == null)
            {
                return null;
            }

            List<ProjectJNetworkExternalGameplay> results =
                new List<ProjectJNetworkExternalGameplay>(8);

            ProjectJNetworkExternalGameplay.CollectActivePlayers(
                runner,
                results
            );

            ProjectJNetworkExternalGameplay best =
                null;

            int bestPlayerIndex =
                int.MaxValue;

            for (
                int index = 0;
                index < results.Count;
                index++
            )
            {
                ProjectJNetworkExternalGameplay candidate =
                    results[index];

                if (
                    candidate == null ||
                    candidate.Object == null ||
                    !candidate.Object.IsValid
                )
                {
                    continue;
                }

                if (
                    !ProjectJDronePolicy.IsInitialLeaderRank(
                        candidate.RaceRank
                    ) ||
                    !ProjectJDronePolicy.CanTarget(
                        true,
                        candidate.Object.InputAuthority == owner,
                        candidate.GameplayInputAllowed,
                        IsTrackableByDrone(
                            candidate
                        )
                    )
                )
                {
                    continue;
                }

                int candidateIndex =
                    candidate.Object.InputAuthority.AsIndex;

                if (
                    best == null ||
                    candidateIndex <
                    bestPlayerIndex
                )
                {
                    best =
                        candidate;

                    bestPlayerIndex =
                        candidateIndex;
                }
            }

            return best;
        }

        internal static ProjectJNetworkExternalGameplay FindBestReacquireTargetAuthority(
            NetworkRunner runner,
            PlayerRef owner,
            int excludedTargetIndex
        )
        {
            if (runner == null)
            {
                return null;
            }

            List<ProjectJNetworkExternalGameplay> results =
                new List<ProjectJNetworkExternalGameplay>(8);

            ProjectJNetworkExternalGameplay.CollectActivePlayers(
                runner,
                results
            );

            ProjectJNetworkExternalGameplay best =
                null;

            int bestRank =
                int.MaxValue;

            int bestPlayerIndex =
                int.MaxValue;

            for (
                int index = 0;
                index < results.Count;
                index++
            )
            {
                ProjectJNetworkExternalGameplay candidate =
                    results[index];

                if (
                    candidate == null ||
                    candidate.Object == null ||
                    !candidate.Object.IsValid
                )
                {
                    continue;
                }

                int candidateIndex =
                    candidate.Object.InputAuthority.AsIndex;

                if (
                    candidateIndex ==
                    excludedTargetIndex ||
                    !ProjectJDronePolicy.CanTarget(
                        true,
                        candidate.Object.InputAuthority == owner,
                        candidate.GameplayInputAllowed,
                        IsTrackableByDrone(
                            candidate
                        )
                    ) ||
                    !ProjectJDronePolicy.IsBetterReacquireCandidate(
                        candidate.RaceRank,
                        bestRank,
                        candidateIndex,
                        bestPlayerIndex
                    )
                )
                {
                    continue;
                }

                best =
                    candidate;

                bestRank =
                    candidate.RaceRank;

                bestPlayerIndex =
                    candidateIndex;
            }

            return best;
        }

        private bool TryReacquireAuthority()
        {
            if (
                !ProjectJDronePolicy.CanReacquire(
                    NetworkReacquireCount
                )
            )
            {
                return false;
            }

            int excludedTargetIndex =
                NetworkTarget.AsIndex;

            ProjectJNetworkExternalGameplay nextTarget =
                FindBestReacquireTargetAuthority(
                    Runner,
                    NetworkOwner,
                    excludedTargetIndex
                );

            NetworkReacquireCount++;

            if (
                nextTarget == null ||
                nextTarget.Object == null ||
                !nextTarget.Object.IsValid
            )
            {
                return false;
            }

            NetworkTarget =
                nextTarget.Object.InputAuthority;

            NetworkTargetRevision++;

            ClearRouteAuthority();

            return true;
        }

        private ProjectJNetworkExternalGameplay ResolvePlayerByRef(
            PlayerRef playerRef
        )
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
                ProjectJNetworkExternalGameplay candidate =
                    playerBuffer[index];

                if (
                    candidate != null &&
                    candidate.Object != null &&
                    candidate.Object.IsValid &&
                    candidate.Object.InputAuthority ==
                    playerRef
                )
                {
                    return candidate;
                }
            }

            return null;
        }

        private bool IsEligibleTarget(
            ProjectJNetworkExternalGameplay candidate,
            PlayerRef owner
        )
        {
            if (
                candidate == null ||
                candidate.Object == null
            )
            {
                return false;
            }

            return
                ProjectJDronePolicy.CanTarget(
                    candidate.Object.IsValid,
                    candidate.Object.InputAuthority == owner,
                    candidate.GameplayInputAllowed,
                    IsTrackableByDrone(
                        candidate
                    )
                );
        }

        private static bool IsTrackableByDrone(
            ProjectJNetworkExternalGameplay candidate
        )
        {
            if (
                candidate == null ||
                candidate.Object == null ||
                !candidate.Object.IsValid
            )
            {
                return false;
            }

            return true; // 129일차 투명 망토에서 추적 제외 상태 연결
        }

        private void AttackAndDespawnAuthority(
            ProjectJNetworkExternalGameplay target,
            Vector3 direction
        )
        {
            if (
                target != null &&
                target.Object != null &&
                target.Object.IsValid
            )
            {
                target.TryApplyExternalVelocityChange(
                    ProjectJExternalForceSource.Item,
                    ProjectJDronePolicy.ResolveAttackVelocity(
                        direction
                    )
                );
            }

            DespawnAuthority();
        }

        private bool HasClearWorldPath(
            Vector3 origin,
            Vector3 destination
        )
        {
            Vector3 offset =
                destination -
                origin;

            float distance =
                offset.magnitude;

            if (distance <= 0.0001f)
            {
                return true;
            }

            Vector3 direction =
                offset /
                distance;

            int hitCount =
                Physics.SphereCastNonAlloc(
                    origin,
                    ProjectJDronePolicy.CollisionRadius,
                    direction,
                    castBuffer,
                    distance,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore
                );

            for (
                int index = 0;
                index < hitCount;
                index++
            )
            {
                Collider collider =
                    castBuffer[index].collider;

                if (collider == null)
                {
                    continue;
                }

                ProjectJNetworkExternalGameplay player =
                    collider.GetComponentInParent<ProjectJNetworkExternalGameplay>();

                if (player != null)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private bool HasWorldCollisionAhead(
            Vector3 direction,
            float distance
        )
        {
            int hitCount =
                Physics.SphereCastNonAlloc(
                    transform.position,
                    ProjectJDronePolicy.CollisionRadius,
                    direction,
                    castBuffer,
                    distance,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore
                );

            for (
                int index = 0;
                index < hitCount;
                index++
            )
            {
                Collider collider =
                    castBuffer[index].collider;

                if (collider == null)
                {
                    continue;
                }

                if (
                    collider.GetComponentInParent<ProjectJNetworkExternalGameplay>() !=
                    null
                )
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private bool TryBuildRouteAuthority(
            Vector3 targetPoint
        )
        {
            CollectRouteNodes();

            if (routeNodes.Count == 0)
            {
                return false;
            }

            ProjectJHomingMissileRouteNode startNode =
                FindNearestReachableRouteNode(
                    transform.position,
                    true
                );

            ProjectJHomingMissileRouteNode endNode =
                FindNearestReachableRouteNode(
                    targetPoint,
                    false
                );

            if (
                startNode == null ||
                endNode == null
            )
            {
                return false;
            }

            routeQueue.Clear();
            routeParents.Clear();
            visitedNodes.Clear();

            routeQueue.Enqueue(
                startNode
            );

            visitedNodes.Add(
                startNode
            );

            bool found =
                startNode ==
                endNode;

            while (
                routeQueue.Count > 0 &&
                !found
            )
            {
                ProjectJHomingMissileRouteNode current =
                    routeQueue.Dequeue();

                CollectAdjacentNodes(
                    current
                );

                for (
                    int index = 0;
                    index < adjacentBuffer.Count;
                    index++
                )
                {
                    ProjectJHomingMissileRouteNode next =
                        adjacentBuffer[index];

                    if (
                        next == null ||
                        visitedNodes.Contains(
                            next
                        ) ||
                        !HasClearWorldPath(
                            current.transform.position,
                            next.transform.position
                        )
                    )
                    {
                        continue;
                    }

                    visitedNodes.Add(
                        next
                    );

                    routeParents[next] =
                        current;

                    if (next == endNode)
                    {
                        found =
                            true;

                        break;
                    }

                    routeQueue.Enqueue(
                        next
                    );
                }
            }

            if (!found)
            {
                return false;
            }

            routePath.Clear();

            ProjectJHomingMissileRouteNode cursor =
                endNode;

            routePath.Add(
                cursor
            );

            while (cursor != startNode)
            {
                if (
                    !routeParents.TryGetValue(
                        cursor,
                        out ProjectJHomingMissileRouteNode parent
                    ) ||
                    parent == null
                )
                {
                    routePath.Clear();
                    return false;
                }

                cursor =
                    parent;

                routePath.Add(
                    cursor
                );
            }

            routePath.Reverse();

            routePathIndex =
                0;

            return
                routePath.Count >
                0;
        }

        private void CollectRouteNodes()
        {
            routeNodes.Clear();

            ProjectJHomingMissileRouteNode[] nodes =
                UnityEngine.Object.FindObjectsByType<ProjectJHomingMissileRouteNode>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            for (
                int index = 0;
                index < nodes.Length;
                index++
            )
            {
                if (nodes[index] != null)
                {
                    routeNodes.Add(
                        nodes[index]
                    );
                }
            }
        }

        private ProjectJHomingMissileRouteNode FindNearestReachableRouteNode(
            Vector3 origin,
            bool fromDrone
        )
        {
            ProjectJHomingMissileRouteNode nearest =
                null;

            float nearestDistanceSquared =
                float.PositiveInfinity;

            for (
                int index = 0;
                index < routeNodes.Count;
                index++
            )
            {
                ProjectJHomingMissileRouteNode node =
                    routeNodes[index];

                if (node == null)
                {
                    continue;
                }

                float distanceSquared =
                    (
                        node.transform.position -
                        origin
                    ).sqrMagnitude;

                float distance =
                    Mathf.Sqrt(
                        distanceSquared
                    );

                if (
                    !ProjectJDronePolicy.IsWithinRouteNodeSearchRadius(
                        distance
                    ) ||
                    distanceSquared >=
                    nearestDistanceSquared
                )
                {
                    continue;
                }

                bool clear =
                    fromDrone
                        ? HasClearWorldPath(
                            transform.position,
                            node.transform.position
                        )
                        : HasClearWorldPath(
                            node.transform.position,
                            origin
                        );

                if (!clear)
                {
                    continue;
                }

                nearest =
                    node;

                nearestDistanceSquared =
                    distanceSquared;
            }

            return nearest;
        }

        private void CollectAdjacentNodes(
            ProjectJHomingMissileRouteNode current
        )
        {
            adjacentBuffer.Clear();

            if (current == null)
            {
                return;
            }

            IReadOnlyList<ProjectJHomingMissileRouteNode> direct =
                current.Neighbours;

            if (direct != null)
            {
                for (
                    int index = 0;
                    index < direct.Count;
                    index++
                )
                {
                    ProjectJHomingMissileRouteNode node =
                        direct[index];

                    if (
                        node != null &&
                        !adjacentBuffer.Contains(
                            node
                        )
                    )
                    {
                        adjacentBuffer.Add(
                            node
                        );
                    }
                }
            }

            for (
                int index = 0;
                index < routeNodes.Count;
                index++
            )
            {
                ProjectJHomingMissileRouteNode node =
                    routeNodes[index];

                if (
                    node == null ||
                    node == current ||
                    !node.ContainsNeighbour(
                        current
                    ) ||
                    adjacentBuffer.Contains(
                        node
                    )
                )
                {
                    continue;
                }

                adjacentBuffer.Add(
                    node
                );
            }
        }

        private void ClearRouteAuthority()
        {
            routePath.Clear();

            routePathIndex =
                0;
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
