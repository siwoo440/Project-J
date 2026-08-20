using NUnit.Framework;
using ProjectJ.CameraSystem;
using ProjectJ.Finish;
using ProjectJ.Player;
using ProjectJ.Ranking;
using ProjectJ.Spectator;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Tests.EditMode
{
    public sealed class SpectatorControllerTests
    {
        private GameObject localPlayerObject;
        private PlayerFinishState localFinishState;
        private PlayerInput localInput;
        private PlayerHeightTracker localGameplayController;

        private GameObject gameplayRigObject;
        private PlayerThirdPersonCamera gameplayRig;
        private Camera gameplayCamera;

        private GameObject spectatorRigObject;
        private PlayerThirdPersonCamera spectatorRig;
        private Transform spectatorPitchPivot;
        private Camera spectatorCamera;

        private GameObject controllerObject;
        private SpectatorController controller;

        [SetUp]
        public void SetUp()
        {
            localPlayerObject =
                CreatePlayer(
                    "LocalPlayer",
                    out localFinishState
                );

            localInput =
                localPlayerObject.AddComponent<
                    PlayerInput
                >();

            localGameplayController =
                localPlayerObject.GetComponent<
                    PlayerHeightTracker
                >();

            gameplayRig =
                CreateCameraRig(
                    "GameplayRig",
                    localPlayerObject.transform,
                    localInput,
                    out gameplayRigObject,
                    out _,
                    out gameplayCamera
                );

            spectatorRig =
                CreateCameraRig(
                    "SpectatorRig",
                    localPlayerObject.transform,
                    localInput,
                    out spectatorRigObject,
                    out spectatorPitchPivot,
                    out spectatorCamera
                );

            spectatorRig.enabled =
                false;

            spectatorCamera.enabled =
                false;

            controllerObject =
                new GameObject(
                    "Spectator Controller"
                );

            controller =
                controllerObject.AddComponent<
                    SpectatorController
                >();

            controller.Configure(
                gameplayRig,
                gameplayCamera,
                spectatorRig,
                spectatorPitchPivot,
                spectatorCamera,
                localInput,
                null,
                localFinishState,
                localGameplayController
            );
        }

        [TearDown]
        public void TearDown()
        {
            PlayerFinishState[] players =
                Object.FindObjectsByType<
                    PlayerFinishState
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < players.Length;
                i++
            )
            {
                if (players[i] != null)
                {
                    Object.DestroyImmediate(
                        players[i].gameObject
                    );
                }
            }

            if (controllerObject != null)
            {
                Object.DestroyImmediate(
                    controllerObject
                );
            }

            if (gameplayRigObject != null)
            {
                Object.DestroyImmediate(
                    gameplayRigObject
                );
            }

            if (spectatorRigObject != null)
            {
                Object.DestroyImmediate(
                    spectatorRigObject
                );
            }
        }

        [Test]
        public void BeginSpectating_SelectsActiveUnfinishedPlayer()
        {
            PlayerFinishState target =
                CreatePlayer(
                    "Player_B",
                    out _
                )
                .GetComponent<
                    PlayerFinishState
                >();

            bool started =
                controller.BeginSpectating();

            Assert.IsTrue(
                started
            );

            Assert.IsTrue(
                controller.IsSpectating
            );

            Assert.AreSame(
                target,
                controller.CurrentTarget
            );
        }

        [Test]
        public void FinishedPlayer_IsExcludedFromTargets()
        {
            PlayerFinishState finished =
                CreatePlayer(
                    "Player_A",
                    out _
                )
                .GetComponent<
                    PlayerFinishState
                >();

            finished.TryConfirmFinish(
                1,
                10d
            );

            PlayerFinishState active =
                CreatePlayer(
                    "Player_B",
                    out _
                )
                .GetComponent<
                    PlayerFinishState
                >();

            bool started =
                controller.BeginSpectating();

            Assert.IsTrue(
                started
            );

            Assert.AreSame(
                active,
                controller.CurrentTarget
            );
        }

        [Test]
        public void NextTarget_ChangesTargetAndKeepsLocalInputOwner()
        {
            PlayerFinishState playerB =
                CreatePlayer(
                    "Player_B",
                    out _
                )
                .GetComponent<
                    PlayerFinishState
                >();

            PlayerFinishState playerC =
                CreatePlayer(
                    "Player_C",
                    out _
                )
                .GetComponent<
                    PlayerFinishState
                >();

            controller.BeginSpectating();

            Assert.AreSame(
                playerB,
                controller.CurrentTarget
            );

            bool changed =
                controller.NextTarget();

            Assert.IsTrue(
                changed
            );

            Assert.AreSame(
                playerC,
                controller.CurrentTarget
            );

            Assert.AreSame(
                localInput,
                controller.LocalInputSource
            );
        }

        [Test]
        public void PreviousTarget_WrapsToLastTarget()
        {
            CreatePlayer(
                "Player_B",
                out _
            );

            PlayerFinishState playerC =
                CreatePlayer(
                    "Player_C",
                    out _
                )
                .GetComponent<
                    PlayerFinishState
                >();

            controller.BeginSpectating();

            bool changed =
                controller.PreviousTarget();

            Assert.IsTrue(
                changed
            );

            Assert.AreSame(
                playerC,
                controller.CurrentTarget
            );
        }

        [Test]
        public void Spectating_DisablesOnlyLocalGameplayController()
        {
            GameObject remoteObject =
                CreatePlayer(
                    "Player_B",
                    out _
                );

            PlayerHeightTracker remoteHeight =
                remoteObject.GetComponent<
                    PlayerHeightTracker
                >();

            Assert.IsTrue(
                localGameplayController.enabled
            );

            Assert.IsTrue(
                remoteHeight.enabled
            );

            controller.BeginSpectating();

            Assert.IsFalse(
                localGameplayController.enabled
            );

            Assert.IsTrue(
                remoteHeight.enabled
            );

            Assert.IsTrue(
                localInput.enabled
            );
        }

        [Test]
        public void ExitSpectating_RestoresPreSpectatorCameraAndControl()
        {
            CreatePlayer(
                "Player_B",
                out _
            );

            controller.BeginSpectating();

            Assert.IsFalse(
                gameplayRig.enabled
            );

            Assert.IsFalse(
                gameplayCamera.enabled
            );

            Assert.IsTrue(
                spectatorRig.enabled
            );

            Assert.IsTrue(
                spectatorCamera.enabled
            );

            controller.ExitSpectating();

            Assert.IsFalse(
                controller.IsSpectating
            );

            Assert.IsTrue(
                gameplayRig.enabled
            );

            Assert.IsTrue(
                gameplayCamera.enabled
            );

            Assert.IsFalse(
                spectatorRig.enabled
            );

            Assert.IsFalse(
                spectatorCamera.enabled
            );

            Assert.IsTrue(
                localGameplayController.enabled
            );
        }

        [Test]
        public void CurrentTargetFinish_AutomaticallyMovesToNextPlayer()
        {
            PlayerFinishState playerB =
                CreatePlayer(
                    "Player_B",
                    out _
                )
                .GetComponent<
                    PlayerFinishState
                >();

            PlayerFinishState playerC =
                CreatePlayer(
                    "Player_C",
                    out _
                )
                .GetComponent<
                    PlayerFinishState
                >();

            controller.BeginSpectating();

            Assert.AreSame(
                playerB,
                controller.CurrentTarget
            );

            playerB.TryConfirmFinish(
                1,
                20d
            );

            Assert.IsTrue(
                controller.IsSpectating
            );

            Assert.AreSame(
                playerC,
                controller.CurrentTarget
            );
        }

        [Test]
        public void NoValidTarget_DoesNotEnterSpectatorMode()
        {
            bool started =
                controller.BeginSpectating();

            Assert.IsFalse(
                started
            );

            Assert.IsFalse(
                controller.IsSpectating
            );

            Assert.IsTrue(
                localGameplayController.enabled
            );
        }

        private static GameObject CreatePlayer(
            string objectName,
            out PlayerFinishState finishState
        )
        {
            GameObject player =
                new GameObject(
                    objectName
                );

            PlayerHeightTracker height =
                player.AddComponent<
                    PlayerHeightTracker
                >();

            PlayerRankingParticipant ranking =
                player.AddComponent<
                    PlayerRankingParticipant
                >();

            ranking.Configure(
                -1,
                height
            );

            finishState =
                player.AddComponent<
                    PlayerFinishState
                >();

            finishState.Configure(
                ranking
            );

            return player;
        }

        private static PlayerThirdPersonCamera
            CreateCameraRig(
                string objectName,
                Transform target,
                PlayerInput inputSource,
                out GameObject rigObject,
                out Transform pitchPivot,
                out Camera targetCamera
            )
        {
            rigObject =
                new GameObject(
                    objectName
                );

            pitchPivot =
                new GameObject(
                    "PitchPivot"
                ).transform;

            pitchPivot.SetParent(
                rigObject.transform,
                false
            );

            GameObject cameraObject =
                new GameObject(
                    "Camera"
                );

            cameraObject.transform.SetParent(
                pitchPivot,
                false
            );

            targetCamera =
                cameraObject.AddComponent<
                    Camera
                >();

            PlayerThirdPersonCamera rig =
                rigObject.AddComponent<
                    PlayerThirdPersonCamera
                >();

            rig.Configure(
                target,
                inputSource,
                pitchPivot,
                targetCamera
            );

            return rig;
        }
    }
}
