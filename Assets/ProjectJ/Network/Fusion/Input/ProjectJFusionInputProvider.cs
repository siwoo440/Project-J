using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJFusionInputProvider :
        MonoBehaviour,
        INetworkRunnerCallbacks
    {
        private ProjectJNetworkInput cachedInput;
        private ProjectJNetworkInput lastSubmittedInput;

        private bool pendingJump;

        public Vector2 LastSubmittedMove =>
            lastSubmittedInput.Move;

        public bool LastSubmittedJump =>
            lastSubmittedInput.Buttons.IsSet(
                ProjectJNetworkButton.Jump
            );

        public bool LastSubmittedSprint =>
            lastSubmittedInput.Buttons.IsSet(
                ProjectJNetworkButton.Sprint
            );

        public bool LastSubmittedCrouch =>
            lastSubmittedInput.Buttons.IsSet(
                ProjectJNetworkButton.Crouch
            );

        public string LastSubmittedTick
        {
            get;
            private set;
        } =
            "-";

        public int SubmitCount
        {
            get;
            private set;
        }

        private void Update()
        {
            Keyboard keyboard =
                Keyboard.current;

            if (keyboard == null)
            {
                cachedInput =
                    default;

                pendingJump =
                    false;

                return;
            }

            Vector2 move =
                Vector2.zero;

            if (keyboard.wKey.isPressed)
            {
                move.y += 1f;
            }

            if (keyboard.sKey.isPressed)
            {
                move.y -= 1f;
            }

            if (keyboard.aKey.isPressed)
            {
                move.x -= 1f;
            }

            if (keyboard.dKey.isPressed)
            {
                move.x += 1f;
            }

            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            cachedInput.Move =
                move;

            if (
                keyboard.spaceKey
                    .wasPressedThisFrame
            )
            {
                pendingJump =
                    true;
            }

            cachedInput.Buttons.Set(
                ProjectJNetworkButton.Sprint,
                keyboard.leftShiftKey.isPressed ||
                keyboard.rightShiftKey.isPressed
            );

            cachedInput.Buttons.Set(
                ProjectJNetworkButton.Crouch,
                keyboard.leftCtrlKey.isPressed ||
                keyboard.rightCtrlKey.isPressed
            );
        }

        public void OnInput(
            NetworkRunner runner,
            NetworkInput input
        )
        {
            ProjectJNetworkInput networkInput =
                cachedInput;

            networkInput.Buttons.Set(
                ProjectJNetworkButton.Jump,
                pendingJump
            );

            input.Set(
                networkInput
            );

            lastSubmittedInput =
                networkInput;

            LastSubmittedTick =
                runner.InputTick.ToString();

            SubmitCount++;

            pendingJump =
                false;
        }

        public void OnObjectExitAOI(
            NetworkRunner runner,
            NetworkObject obj,
            PlayerRef player
        )
        {
        }

        public void OnObjectEnterAOI(
            NetworkRunner runner,
            NetworkObject obj,
            PlayerRef player
        )
        {
        }

        public void OnPlayerJoined(
            NetworkRunner runner,
            PlayerRef player
        )
        {
        }

        public void OnPlayerLeft(
            NetworkRunner runner,
            PlayerRef player
        )
        {
        }

        public void OnShutdown(
            NetworkRunner runner,
            ShutdownReason shutdownReason
        )
        {
        }

        public void OnDisconnectedFromServer(
            NetworkRunner runner,
            NetDisconnectReason reason
        )
        {
        }

        public void OnConnectRequest(
            NetworkRunner runner,
            NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token
        )
        {
        }

        public void OnConnectFailed(
            NetworkRunner runner,
            NetAddress remoteAddress,
            NetConnectFailedReason reason
        )
        {
        }

#pragma warning disable CS0618
        public void OnUserSimulationMessage(
            NetworkRunner runner,
            SimulationMessagePtr message
        )
        {
        }
#pragma warning restore CS0618

        public void OnReliableDataReceived(
            NetworkRunner runner,
            PlayerRef player,
            ReliableKey key,
            ReadOnlySpan<byte> data
        )
        {
        }

        public void OnReliableDataProgress(
            NetworkRunner runner,
            PlayerRef player,
            ReliableKey key,
            float progress
        )
        {
        }

        public void OnInputMissing(
            NetworkRunner runner,
            PlayerRef player,
            NetworkInput input
        )
        {
        }

        public void OnConnectedToServer(
            NetworkRunner runner
        )
        {
        }

        public void OnSessionListUpdated(
            NetworkRunner runner,
            List<SessionInfo> sessionList
        )
        {
        }

        public void OnCustomAuthenticationResponse(
            NetworkRunner runner,
            Dictionary<string, object> data
        )
        {
        }

        public void OnHostMigration(
            NetworkRunner runner,
            HostMigrationToken hostMigrationToken
        )
        {
        }

        public void OnSceneLoadDone(
            NetworkRunner runner
        )
        {
        }

        public void OnSceneLoadStart(
            NetworkRunner runner
        )
        {
        }
    }
}
