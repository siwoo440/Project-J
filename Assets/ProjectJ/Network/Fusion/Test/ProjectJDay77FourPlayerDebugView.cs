using System.Collections.Generic; // Player 정렬 사용
using Fusion; // NetworkRunner와 PlayerRef 사용
using UnityEngine; // Runtime Debug GUI 사용
using UnityEngine.InputSystem; // F4 입력 사용
using UnityEngine.SceneManagement; // Game Scene 확인

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJDay77FourPlayerDebugView :
        MonoBehaviour
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        private ProjectJFusionBootstrap bootstrap;

        private bool visible =
            false; // 78일차부터 기본 화면은 F5 8인 Gate 사용

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void Install()
        {
            ProjectJDay77FourPlayerDebugView existing =
                Object.FindFirstObjectByType<
                    ProjectJDay77FourPlayerDebugView
                >();

            if (existing != null)
            {
                return;
            }

            GameObject debugObject =
                new GameObject(
                    "=== Project J Day77 4P Debug ==="
                );

            Object.DontDestroyOnLoad(
                debugObject
            );

            debugObject.AddComponent<
                ProjectJDay77FourPlayerDebugView
            >();
        }

        private void Update()
        {
            Keyboard keyboard =
                Keyboard.current;

            if (
                keyboard != null &&
                keyboard.f4Key.wasPressedThisFrame
            )
            {
                visible =
                    !visible;
            }

            if (bootstrap == null)
            {
                bootstrap =
                    Object.FindFirstObjectByType<
                        ProjectJFusionBootstrap
                    >();
            }
        }

        private void OnGUI()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return;
#else
            if (
                !visible ||
                SceneManager.GetActiveScene().path !=
                    GameScenePath ||
                bootstrap == null ||
                bootstrap.Runner == null ||
                !bootstrap.Runner.IsRunning
            )
            {
                return;
            }

            NetworkRunner runner =
                bootstrap.Runner;

            List<PlayerRef> players =
                new List<PlayerRef>();

            foreach (
                PlayerRef player
                in runner.ActivePlayers
            )
            {
                players.Add(
                    player
                );
            }

            players.Sort(
                (
                    left,
                    right
                ) =>
                    left.AsIndex.CompareTo(
                        right.AsIndex
                    )
            );

            float width =
                430f;

            float height =
                115f +
                players.Count *
                25f;

            GUI.Box(
                new Rect(
                    12f,
                    12f,
                    width,
                    height
                ),
                string.Empty
            );

            float y =
                20f;

            DrawLine(
                ref y,
                "DAY 77 - 4 PLAYER GATE  /  F4 Toggle"
            );

            DrawLine(
                ref y,
                "Participants : " +
                bootstrap.ParticipantCount +
                " / 4    PlayerObjects : " +
                bootstrap.SpawnedPlayerCount +
                " / 4"
            );

            string gate =
                bootstrap.ParticipantCount == 4 &&
                bootstrap.SpawnedPlayerCount == 4
                    ? "4P CONNECTION GATE : PASS"
                    : "4P CONNECTION GATE : WAIT";

            DrawLine(
                ref y,
                gate
            );

            for (
                int index = 0;
                index < players.Count;
                index++
            )
            {
                PlayerRef player =
                    players[index];

                if (
                    !runner.TryGetPlayerObject(
                        player,
                        out NetworkObject playerObject
                    ) ||
                    playerObject == null
                )
                {
                    DrawLine(
                        ref y,
                        "P" +
                        player.AsIndex +
                        " : PlayerObject 없음"
                    );

                    continue;
                }

                ProjectJNetworkExternalGameplay gameplay =
                    playerObject.GetComponent<
                        ProjectJNetworkExternalGameplay
                    >();

                if (gameplay == null)
                {
                    DrawLine(
                        ref y,
                        "P" +
                        player.AsIndex +
                        " : ExternalGameplay 없음"
                    );

                    continue;
                }

                DrawLine(
                    ref y,
                    "P" +
                    player.AsIndex +
                    "  H:" +
                    gameplay.RaceHeight.ToString("F2") +
                    "  Rank:" +
                    gameplay.RaceRank +
                    "  CP:" +
                    gameplay.CurrentCheckpointId +
                    "  FIN:" +
                    gameplay.IsFinished
                );
            }
#endif
        }

        private static void DrawLine(
            ref float y,
            string text
        )
        {
            GUI.Label(
                new Rect(
                    22f,
                    y,
                    400f,
                    22f
                ),
                text
            );

            y +=
                25f;
        }
    }
}
