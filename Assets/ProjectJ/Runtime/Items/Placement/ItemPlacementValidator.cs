using System.Collections.Generic;
using ProjectJ.Checkpoint;
using ProjectJ.Map;
using UnityEngine;
using CheckpointComponent =
    ProjectJ.Checkpoint.Checkpoint;

namespace ProjectJ.Items.Placement
{
    public static class ItemPlacementValidator
    {
        private const float CheckpointPadding =
            1.25f;

        private const float RespawnBlockRadius =
            2.5f;

        private const float RespawnVerticalTolerance =
            3f;

        private static readonly List<Vector3>
            startPositions =
                new List<Vector3>();

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void ResetSceneCache()
        {
            startPositions.Clear();
            CaptureStartPositions();
        }

        public static bool CanPlace(
            Bounds candidateBounds
        )
        {
            CaptureStartPositions();

            if (
                IntersectsNoSpawnVolume(
                    candidateBounds
                )
            )
            {
                return false;
            }

            if (
                IntersectsCheckpoint(
                    candidateBounds
                )
            )
            {
                return false;
            }

            if (
                IsNearCheckpointRespawn(
                    candidateBounds.center
                )
            )
            {
                return false;
            }

            if (
                IsNearStartPosition(
                    candidateBounds.center
                )
            )
            {
                return false;
            }

            return true;
        }

        private static bool
            IntersectsNoSpawnVolume(
                Bounds candidateBounds
            )
        {
            MapObstaclePlacementVolume[] volumes =
                Object.FindObjectsByType<
                    MapObstaclePlacementVolume
                >(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            for (int i = 0; i < volumes.Length; i++)
            {
                MapObstaclePlacementVolume volume =
                    volumes[i];

                if (
                    volume == null ||
                    volume.VolumeType !=
                    MapObstacleVolumeType.NoSpawn
                )
                {
                    continue;
                }

                if (
                    volume.IntersectsBounds(
                        candidateBounds
                    )
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IntersectsCheckpoint(
            Bounds candidateBounds
        )
        {
            CheckpointComponent[] checkpoints =
                Object.FindObjectsByType<
                    CheckpointComponent
                >(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < checkpoints.Length;
                i++
            )
            {
                CheckpointComponent checkpoint =
                    checkpoints[i];

                if (checkpoint == null)
                {
                    continue;
                }

                Collider checkpointCollider =
                    checkpoint.GetComponent<
                        Collider
                    >();

                if (checkpointCollider == null)
                {
                    continue;
                }

                Bounds protectedBounds =
                    checkpointCollider.bounds;

                protectedBounds.Expand(
                    new Vector3(
                        CheckpointPadding * 2f,
                        1f,
                        CheckpointPadding * 2f
                    )
                );

                if (
                    protectedBounds.Intersects(
                        candidateBounds
                    )
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static bool
            IsNearCheckpointRespawn(
                Vector3 candidatePosition
            )
        {
            CheckpointComponent[] checkpoints =
                Object.FindObjectsByType<
                    CheckpointComponent
                >(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < checkpoints.Length;
                i++
            )
            {
                CheckpointComponent checkpoint =
                    checkpoints[i];

                if (checkpoint == null)
                {
                    continue;
                }

                if (
                    IsInsideProtectedRadius(
                        candidatePosition,
                        checkpoint.RespawnPosition
                    )
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static void CaptureStartPositions()
        {
            PlayerCheckpointTracker[] trackers =
                Object.FindObjectsByType<
                    PlayerCheckpointTracker
                >(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < trackers.Length;
                i++
            )
            {
                PlayerCheckpointTracker tracker =
                    trackers[i];

                if (
                    tracker == null ||
                    tracker.CurrentCheckpointId !=
                    CheckpointId.Start
                )
                {
                    continue;
                }

                Vector3 startPosition =
                    tracker.RespawnPosition;

                if (
                    !ContainsStartPosition(
                        startPosition
                    )
                )
                {
                    startPositions.Add(
                        startPosition
                    );
                }
            }
        }

        private static bool IsNearStartPosition(
            Vector3 candidatePosition
        )
        {
            for (
                int i = 0;
                i < startPositions.Count;
                i++
            )
            {
                if (
                    IsInsideProtectedRadius(
                        candidatePosition,
                        startPositions[i]
                    )
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsStartPosition(
            Vector3 position
        )
        {
            const float DuplicateDistanceSqr =
                0.01f;

            for (
                int i = 0;
                i < startPositions.Count;
                i++
            )
            {
                if (
                    (
                        startPositions[i] -
                        position
                    ).sqrMagnitude <=
                    DuplicateDistanceSqr
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static bool
            IsInsideProtectedRadius(
                Vector3 candidatePosition,
                Vector3 protectedPosition
            )
        {
            if (
                Mathf.Abs(
                    candidatePosition.y -
                    protectedPosition.y
                ) >
                RespawnVerticalTolerance
            )
            {
                return false;
            }

            Vector2 candidateXZ =
                new Vector2(
                    candidatePosition.x,
                    candidatePosition.z
                );

            Vector2 protectedXZ =
                new Vector2(
                    protectedPosition.x,
                    protectedPosition.z
                );

            return
                (
                    candidateXZ -
                    protectedXZ
                ).sqrMagnitude <=
                RespawnBlockRadius *
                RespawnBlockRadius;
        }
    }
}
