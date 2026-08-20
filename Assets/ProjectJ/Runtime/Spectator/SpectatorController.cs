using System;
using System.Collections.Generic;
using ProjectJ.CameraSystem;
using ProjectJ.Finish;
using ProjectJ.Results;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Spectator
{
    [DisallowMultipleComponent]
    public sealed class SpectatorController :
        MonoBehaviour
    {
        [SerializeField]
        private PlayerThirdPersonCamera
            gameplayCameraRig;

        [SerializeField]
        private Camera gameplayCamera;

        [SerializeField]
        private PlayerThirdPersonCamera
            spectatorCameraRig;

        [SerializeField]
        private Transform spectatorPitchPivot;

        [SerializeField]
        private Camera spectatorCamera;

        [SerializeField]
        private PlayerInput localInputSource;

        [SerializeField]
        private PlayerMatchResultCollector
            localResultCollector;

        [SerializeField]
        private PlayerFinishState
            localFinishState;

        [SerializeField]
        private Behaviour localGameplayController;

        [SerializeField]
        private bool isSpectating;

        [SerializeField]
        private PlayerFinishState
            currentTarget;

        private readonly List<PlayerFinishState>
            validTargets =
                new List<PlayerFinishState>();

        private bool gameplayRigWasEnabled;
        private bool gameplayCameraWasEnabled;
        private bool localGameplayWasEnabled;
        private bool resultSubscribed;
        private bool targetSubscribed;

        public event Action<PlayerFinishState>
            SpectatorTargetChanged;

        public event Action SpectatingStarted;
        public event Action SpectatingEnded;

        public bool IsSpectating
        {
            get
            {
                return isSpectating;
            }
        }

        public PlayerFinishState CurrentTarget
        {
            get
            {
                return currentTarget;
            }
        }

        public PlayerInput LocalInputSource
        {
            get
            {
                return localInputSource;
            }
        }

        public Behaviour LocalGameplayController
        {
            get
            {
                return localGameplayController;
            }
        }

        public int ValidTargetCount
        {
            get
            {
                RefreshValidTargets();

                return validTargets.Count;
            }
        }

        private void OnEnable()
        {
            SubscribeResult();
        }

        private void OnDisable()
        {
            UnsubscribeResult();
            UnsubscribeCurrentTarget();
        }

        private void Update()
        {
            if (!isSpectating)
            {
                return;
            }

            if (!IsValidTarget(currentTarget))
            {
                SelectFirstValidTargetOrExit();
            }
        }

        public void Configure(
            PlayerThirdPersonCamera
                newGameplayCameraRig,
            Camera newGameplayCamera,
            PlayerThirdPersonCamera
                newSpectatorCameraRig,
            Transform newSpectatorPitchPivot,
            Camera newSpectatorCamera,
            PlayerInput newLocalInputSource,
            PlayerMatchResultCollector
                newLocalResultCollector,
            PlayerFinishState
                newLocalFinishState,
            Behaviour newLocalGameplayController
        )
        {
            UnsubscribeResult();
            UnsubscribeCurrentTarget();

            gameplayCameraRig =
                newGameplayCameraRig;

            gameplayCamera =
                newGameplayCamera;

            spectatorCameraRig =
                newSpectatorCameraRig;

            spectatorPitchPivot =
                newSpectatorPitchPivot;

            spectatorCamera =
                newSpectatorCamera;

            localInputSource =
                newLocalInputSource;

            localResultCollector =
                newLocalResultCollector;

            localFinishState =
                newLocalFinishState;

            localGameplayController =
                newLocalGameplayController;

            if (isActiveAndEnabled)
            {
                SubscribeResult();
            }
        }

        public bool BeginSpectating()
        {
            if (isSpectating)
            {
                return true;
            }

            if (
                spectatorCameraRig == null ||
                spectatorPitchPivot == null ||
                spectatorCamera == null ||
                localInputSource == null
            )
            {
                return false;
            }

            RefreshValidTargets();

            if (validTargets.Count == 0)
            {
                return false;
            }

            gameplayRigWasEnabled =
                gameplayCameraRig != null &&
                gameplayCameraRig.enabled;

            gameplayCameraWasEnabled =
                gameplayCamera != null &&
                gameplayCamera.enabled;

            localGameplayWasEnabled =
                localGameplayController != null &&
                localGameplayController.enabled;

            if (localGameplayController != null)
            {
                localGameplayController.enabled =
                    false;
            }

            if (gameplayCameraRig != null)
            {
                gameplayCameraRig.enabled =
                    false;
            }

            if (gameplayCamera != null)
            {
                gameplayCamera.enabled =
                    false;
            }

            isSpectating = true;

            ApplyTarget(
                validTargets[0]
            );

            spectatorCamera.enabled =
                true;

            spectatorCameraRig.enabled =
                true;

            SpectatingStarted?.Invoke();

            return true;
        }

        public bool NextTarget()
        {
            return MoveTarget(
                1
            );
        }

        public bool PreviousTarget()
        {
            return MoveTarget(
                -1
            );
        }

        public void ExitSpectating()
        {
            if (!isSpectating)
            {
                return;
            }

            UnsubscribeCurrentTarget();

            currentTarget = null;
            isSpectating = false;

            if (spectatorCameraRig != null)
            {
                spectatorCameraRig.enabled =
                    false;
            }

            if (spectatorCamera != null)
            {
                spectatorCamera.enabled =
                    false;
            }

            if (gameplayCameraRig != null)
            {
                gameplayCameraRig.enabled =
                    gameplayRigWasEnabled;
            }

            if (gameplayCamera != null)
            {
                gameplayCamera.enabled =
                    gameplayCameraWasEnabled;
            }

            if (
                localGameplayController != null &&
                ShouldRestoreLocalGameplay()
            )
            {
                localGameplayController.enabled =
                    localGameplayWasEnabled;
            }

            SpectatingEnded?.Invoke();
        }

        public void RefreshCurrentTarget()
        {
            if (!isSpectating)
            {
                return;
            }

            if (IsValidTarget(currentTarget))
            {
                return;
            }

            SelectFirstValidTargetOrExit();
        }

        private bool MoveTarget(
            int direction
        )
        {
            if (!isSpectating)
            {
                return false;
            }

            RefreshValidTargets();

            if (validTargets.Count == 0)
            {
                ExitSpectating();
                return false;
            }

            int currentIndex =
                validTargets.IndexOf(
                    currentTarget
                );

            if (currentIndex < 0)
            {
                ApplyTarget(
                    validTargets[0]
                );

                return true;
            }

            int count =
                validTargets.Count;

            int nextIndex =
                (
                    currentIndex +
                    direction
                ) %
                count;

            if (nextIndex < 0)
            {
                nextIndex +=
                    count;
            }

            ApplyTarget(
                validTargets[nextIndex]
            );

            return true;
        }

        private void ApplyTarget(
            PlayerFinishState target
        )
        {
            if (!IsValidTarget(target))
            {
                return;
            }

            UnsubscribeCurrentTarget();

            currentTarget =
                target;

            spectatorCameraRig.enabled =
                false;

            spectatorCameraRig.Configure(
                currentTarget.transform,
                localInputSource,
                spectatorPitchPivot,
                spectatorCamera
            );

            spectatorCameraRig.enabled =
                true;

            SubscribeCurrentTarget();

            SpectatorTargetChanged?.Invoke(
                currentTarget
            );
        }

        private void RefreshValidTargets()
        {
            validTargets.Clear();

            PlayerFinishState[] allPlayers =
                FindObjectsByType<
                    PlayerFinishState
                >(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < allPlayers.Length;
                i++
            )
            {
                PlayerFinishState candidate =
                    allPlayers[i];

                if (!IsValidTarget(candidate))
                {
                    continue;
                }

                validTargets.Add(
                    candidate
                );
            }

            validTargets.Sort(
                CompareTargets
            );
        }

        private bool IsValidTarget(
            PlayerFinishState candidate
        )
        {
            if (
                candidate == null ||
                candidate == localFinishState ||
                candidate.IsFinished ||
                !candidate.gameObject
                    .activeInHierarchy
            )
            {
                return false;
            }

            return true;
        }

        private static int CompareTargets(
            PlayerFinishState left,
            PlayerFinishState right
        )
        {
            if (left == right)
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            return string.CompareOrdinal(
                left.gameObject.name,
                right.gameObject.name
            );
        }

        private void SelectFirstValidTargetOrExit()
        {
            RefreshValidTargets();

            if (validTargets.Count == 0)
            {
                ExitSpectating();
                return;
            }

            ApplyTarget(
                validTargets[0]
            );
        }

        private bool ShouldRestoreLocalGameplay()
        {
            if (!localGameplayWasEnabled)
            {
                return false;
            }

            if (
                localResultCollector == null ||
                !localResultCollector.HasResult ||
                localResultCollector
                    .CurrentResult == null
            )
            {
                return true;
            }

            return
                !localResultCollector
                    .CurrentResult
                    .IsFinished;
        }

        private void HandleResultCreated(
            PlayerMatchResult result
        )
        {
            if (
                result == null ||
                !result.IsFinished
            )
            {
                return;
            }

            BeginSpectating();
        }

        private void SubscribeResult()
        {
            if (
                resultSubscribed ||
                localResultCollector == null
            )
            {
                return;
            }

            localResultCollector.ResultCreated +=
                HandleResultCreated;

            resultSubscribed = true;
        }

        private void UnsubscribeResult()
        {
            if (
                !resultSubscribed ||
                localResultCollector == null
            )
            {
                resultSubscribed = false;
                return;
            }

            localResultCollector.ResultCreated -=
                HandleResultCreated;

            resultSubscribed = false;
        }

        private void SubscribeCurrentTarget()
        {
            if (
                targetSubscribed ||
                currentTarget == null
            )
            {
                return;
            }

            currentTarget.Finished +=
                HandleCurrentTargetFinished;

            targetSubscribed = true;
        }

        private void UnsubscribeCurrentTarget()
        {
            if (
                !targetSubscribed ||
                currentTarget == null
            )
            {
                targetSubscribed = false;
                return;
            }

            currentTarget.Finished -=
                HandleCurrentTargetFinished;

            targetSubscribed = false;
        }

        private void HandleCurrentTargetFinished(
            PlayerFinishState player
        )
        {
            SelectFirstValidTargetOrExit();
        }
    }
}
