using System; // Account ID 비교
using Fusion;
using UnityEngine;

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJNetworkPlayerSpawner :
        SimulationBehaviour,
        IPlayerJoined,
        IPlayerLeft
    {
        private NetworkObject playerPrefab;

        public NetworkObject PlayerPrefab =>
            playerPrefab;

        public void Configure(
            NetworkObject prefab
        )
        {
            playerPrefab =
                prefab;
        }

        public void PlayerJoined(
            PlayerRef player
        )
        {
            if (
                Runner == null ||
                !Runner.IsRunning ||
                !Runner.IsServer ||
                playerPrefab == null
            )
            {
                return;
            }

            if (
                !TryValidateProjectAccountId(
                    player,
                    out string projectAccountId
                )
            )
            {
                Runner.Disconnect(
                    player
                );

                return;
            }

            if (
                Runner.TryGetPlayerObject(
                    player,
                    out _
                )
            )
            {
                return;
            }

            Vector3 spawnPosition =
                GetSpawnPosition(
                    player
                );

            NetworkObject spawnedPlayer =
                Runner.Spawn(
                    playerPrefab,
                    spawnPosition,
                    Quaternion.identity,
                    player
                );

            if (spawnedPlayer == null)
            {
                Debug.LogError(
                    "[Project J/Fusion] " +
                    "Network Player Spawn 실패: " +
                    player
                );

                return;
            }

            Runner.SetPlayerObject(
                player,
                spawnedPlayer
            );

            spawnedPlayer.name =
                "NetworkPlayer_" +
                player.AsIndex;

            Debug.Log(
                "[Project J/Fusion] " +
                "Network Player Spawn 완료 / " +
                "PlayerRef: " +
                player.AsIndex +
                " / ProjectAccountId: " +
                projectAccountId +
                " / Input Authority: " +
                player
            );
        }

        public void PlayerLeft(
            PlayerRef player
        )
        {
            if (
                Runner == null ||
                !Runner.IsRunning ||
                !Runner.IsServer
            )
            {
                return;
            }

            if (
                !Runner.TryGetPlayerObject(
                    player,
                    out NetworkObject playerObject
                ) ||
                playerObject == null
            )
            {
                return;
            }

            Runner.Despawn(
                playerObject
            );

            Debug.Log(
                "[Project J/Fusion] " +
                "Network Player Despawn 완료 / " +
                "PlayerRef: " +
                player.AsIndex
            );
        }

        private bool TryValidateProjectAccountId(
            PlayerRef joiningPlayer,
            out string projectAccountId
        )
        {
            projectAccountId =
                Runner.GetPlayerUserId(
                    joiningPlayer
                );

            if (
                string.IsNullOrWhiteSpace(
                    projectAccountId
                )
            )
            {
                Debug.LogWarning(
                    "[Project J/Fusion] " +
                    "Project Account ID가 없는 Player 연결 거부 / P" +
                    joiningPlayer.AsIndex
                );

                return false;
            }

            foreach (
                PlayerRef activePlayer
                in Runner.ActivePlayers
            )
            {
                if (
                    activePlayer ==
                    joiningPlayer
                )
                {
                    continue;
                }

                string activeAccountId =
                    Runner.GetPlayerUserId(
                        activePlayer
                    );

                if (
                    !string.Equals(
                        projectAccountId,
                        activeAccountId,
                        StringComparison.Ordinal
                    )
                )
                {
                    continue;
                }

                Debug.LogWarning(
                    "[Project J/Fusion] " +
                    "중복 Project Account ID 연결 거부 / " +
                    projectAccountId +
                    " / Existing P" +
                    activePlayer.AsIndex +
                    " / Joining P" +
                    joiningPlayer.AsIndex
                );

                return false;
            }

            return true;
        }

        private Vector3 GetSpawnPosition(
            PlayerRef player
        )
        {
            int slot = 0;

            foreach (
                PlayerRef activePlayer
                in Runner.ActivePlayers
            )
            {
                if (
                    activePlayer == player
                )
                {
                    break;
                }

                slot++;
            }

            return new Vector3(
                slot * 3f,
                2f,
                4f
            );
        }
    }
}
