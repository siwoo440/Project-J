using ProjectJ.Finish;
using UnityEngine;

namespace ProjectJ.Push
{
    [DisallowMultipleComponent]
    public sealed class PlayerPushTargetSelector :
        MonoBehaviour
    {
        private const int MaxOverlapResults =
            32;

        [SerializeField]
        [Min(0.1f)]
        private float searchRange =
            2.5f;

        [SerializeField]
        [Range(1f, 180f)]
        private float searchAngle =
            90f;

        [SerializeField]
        private LayerMask playerLayers =
            1 << 8;

        [SerializeField]
        private PlayerFinishState selfFinishState;

        [SerializeField]
        private PlayerFinishState currentTarget;

        private readonly Collider[] overlapResults =
            new Collider[MaxOverlapResults];

        public float SearchRange
        {
            get
            {
                return searchRange;
            }
        }

        public float SearchAngle
        {
            get
            {
                return searchAngle;
            }
        }

        public LayerMask PlayerLayers
        {
            get
            {
                return playerLayers;
            }
        }

        public PlayerFinishState CurrentTarget
        {
            get
            {
                return currentTarget;
            }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        public void Configure(
            float newSearchRange,
            float newSearchAngle,
            LayerMask newPlayerLayers,
            PlayerFinishState newSelfFinishState
        )
        {
            searchRange =
                Mathf.Max(
                    0.1f,
                    newSearchRange
                );

            searchAngle =
                Mathf.Clamp(
                    newSearchAngle,
                    1f,
                    180f
                );

            playerLayers =
                newPlayerLayers;

            selfFinishState =
                newSelfFinishState;

            currentTarget =
                null;

            ResolveReferences();
        }

        public bool TryFindTarget(
            out PlayerFinishState target
        )
        {
            ResolveReferences();

            target =
                null;

            currentTarget =
                null;

            if (
                selfFinishState != null &&
                selfFinishState.IsFinished
            )
            {
                return false;
            }

            int hitCount =
                Physics.OverlapSphereNonAlloc(
                    transform.position,
                    searchRange,
                    overlapResults,
                    playerLayers,
                    QueryTriggerInteraction.Collide
                );

            float closestSqrDistance =
                float.PositiveInfinity;

            Vector3 forward =
                transform.forward;

            float halfAngle =
                searchAngle *
                0.5f;

            for (
                int i = 0;
                i < hitCount;
                i++
            )
            {
                Collider hit =
                    overlapResults[i];

                if (
                    !TryGetValidCandidate(
                        hit,
                        out PlayerFinishState candidate
                    )
                )
                {
                    continue;
                }

                Vector3 toCandidate =
                    candidate.transform.position -
                    transform.position;

                float sqrDistance =
                    toCandidate.sqrMagnitude;

                if (
                    sqrDistance >
                    searchRange *
                    searchRange
                )
                {
                    continue;
                }

                if (
                    toCandidate.sqrMagnitude >
                    Mathf.Epsilon
                )
                {
                    float angle =
                        Vector3.Angle(
                            forward,
                            toCandidate
                        );

                    if (angle > halfAngle)
                    {
                        continue;
                    }
                }

                if (
                    sqrDistance >=
                    closestSqrDistance
                )
                {
                    continue;
                }

                closestSqrDistance =
                    sqrDistance;

                target =
                    candidate;
            }

            currentTarget =
                target;

            return target != null;
        }

        public void ClearTarget()
        {
            currentTarget =
                null;
        }

        private bool TryGetValidCandidate(
            Collider hit,
            out PlayerFinishState candidate
        )
        {
            candidate =
                null;

            if (
                hit == null ||
                !hit.enabled ||
                !hit.gameObject.activeInHierarchy
            )
            {
                return false;
            }

            candidate =
                hit.GetComponentInParent<
                    PlayerFinishState
                >();

            if (
                candidate == null ||
                candidate ==
                    selfFinishState ||
                candidate.IsFinished ||
                !candidate.gameObject
                    .activeInHierarchy
            )
            {
                candidate =
                    null;

                return false;
            }

            return true;
        }

        private void ResolveReferences()
        {
            if (selfFinishState != null)
            {
                return;
            }

            selfFinishState =
                GetComponent<
                    PlayerFinishState
                >();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            float clampedRange =
                Mathf.Max(
                    0.1f,
                    searchRange
                );

            float halfAngle =
                Mathf.Clamp(
                    searchAngle,
                    1f,
                    180f
                ) *
                0.5f;

            Vector3 origin =
                transform.position;

            Vector3 forward =
                transform.forward;

            Quaternion leftRotation =
                Quaternion.AngleAxis(
                    -halfAngle,
                    transform.up
                );

            Quaternion rightRotation =
                Quaternion.AngleAxis(
                    halfAngle,
                    transform.up
                );

            Gizmos.DrawWireSphere(
                origin,
                clampedRange
            );

            Gizmos.DrawLine(
                origin,
                origin +
                leftRotation *
                forward *
                clampedRange
            );

            Gizmos.DrawLine(
                origin,
                origin +
                rightRotation *
                forward *
                clampedRange
            );
        }
#endif
    }
}
