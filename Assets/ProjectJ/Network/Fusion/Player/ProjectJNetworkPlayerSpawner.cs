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

            GetSpawnPose( // 참가 Player의 시작 Pose 조회
                player, // 참가 Player 전달
                out Vector3 spawnPosition, // 시작 위치 수신
                out Quaternion spawnRotation // 시작 회전 수신
            );

            NetworkObject spawnedPlayer =
                Runner.Spawn(
                    playerPrefab,
                    spawnPosition,
                    spawnRotation, // 장면 Spawn 회전 적용
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

        private void GetSpawnPose( // 참가 Player 시작 Pose 계산
            PlayerRef player, // 참가 Player
            out Vector3 position, // 결과 시작 위치
            out Quaternion rotation // 결과 시작 회전
        )
        {
            int slot = 0; // 첫 번째 시작 슬롯 설정

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

                slot++; // 앞선 참가자 수만큼 슬롯 증가
            }

            if (TryGetSpawnPoseForSlot(slot, out position, out rotation)) // 번호 Spawn 지점 조회
            {
                return; // 장면 Spawn Pose 사용
            }

            position = new Vector3( // Spawn 지점 누락 시 예비 위치 생성
                slot * 3f, // 슬롯별 X 간격 적용
                2f, // 기존 시작 높이 유지
                4f // 기존 시작 Z 위치 유지
            );
            rotation = Quaternion.identity; // 예비 기본 회전 적용
        }

        private static bool TryGetSpawnPoseForSlot( // 번호 Spawn 지점 Pose 조회
            int slot, // 시작 슬롯 번호
            out Vector3 position, // 결과 시작 위치
            out Quaternion rotation // 결과 시작 회전
        )
        {
            return ProjectJNetworkSpawnPoint.TryGetPose( // 장면 Spawn Point 조회 결과 반환
                slot, // 시작 슬롯 번호 전달
                out position, // 장면 위치 수신
                out rotation // 장면 회전 수신
            );
        }
    }
}
