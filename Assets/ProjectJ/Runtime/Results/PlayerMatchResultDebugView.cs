using ProjectJ.Debugging;
using UnityEngine;

namespace ProjectJ.Results
{
    [DisallowMultipleComponent]
    public sealed class PlayerMatchResultDebugView :
        MonoBehaviour
    {
        [SerializeField]
        private PlayerMatchResultCollector
            collector;

        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;

        public void Configure(
            PlayerMatchResultCollector
                targetCollector
        )
        {
            collector =
                targetCollector;
        }

        private void OnGUI()
        {
            if (
                !ProjectJDebugOverlayController
                    .IsVisible
            )
            {
                return;
            }

            ResolveCollector();

            if (collector == null)
            {
                return;
            }

            EnsureStyles();

            string text =
                BuildResultText();

            GUI.Label(
                new Rect(
                    470f,
                    100f,
                    500f,
                    180f
                ),
                text,
                labelStyle
            );

            if (
                !collector.HasResult &&
                GUI.Button(
                    new Rect(
                        470f,
                        285f,
                        220f,
                        38f
                    ),
                    "시간 종료 결과 생성",
                    buttonStyle
                )
            )
            {
                collector
                    .TryCreateTimeExpiredResult();
            }
        }

        private string BuildResultText()
        {
            if (!collector.HasResult)
            {
                return
                    "개인 결과 : 대기 중" +
                    "\n완주 또는 시간 종료 시 생성";
            }

            PlayerMatchResult result =
                collector.CurrentResult;

            if (result == null)
            {
                return
                    "개인 결과 : 없음";
            }

            string finishTime =
                result.HasFinishTime
                    ? result.FinishTime
                        .ToString("0.000") +
                        "초"
                    : "--";

            string finishedText =
                result.IsFinished
                    ? "완주"
                    : "미완주";

            return
                "개인 결과 : 생성됨" +
                "\n플레이어 ID : " +
                result.PlayerId +
                "\n최종 순위 : " +
                result.FinalRank +
                "\n완주 여부 : " +
                finishedText +
                "\n완주 순서 : " +
                result.FinishOrder +
                "\n완주 기록 : " +
                finishTime +
                "\n최고 높이 : " +
                result.HighestHeight
                    .ToString("0.00") +
                "m" +
                "\n최고 체크포인트 : " +
                GetCheckpointLabel(
                    result.HighestCheckpoint
                );
        }

        private void ResolveCollector()
        {
            if (collector != null)
            {
                return;
            }

            collector =
                FindFirstObjectByType<
                    PlayerMatchResultCollector
                >();
        }

        private static string GetCheckpointLabel(
            ProjectJ.Checkpoint.CheckpointId checkpointId
        )
        {
            if (
                checkpointId ==
                ProjectJ.Checkpoint.CheckpointId.Start
            )
            {
                return "시작 지점";
            }

            string value =
                checkpointId.ToString();

            if (
                value.StartsWith("CP")
            )
            {
                return
                    "체크포인트 " +
                    value.Substring(2);
            }

            return value;
        }

        private void EnsureStyles()
        {
            if (labelStyle == null)
            {
                labelStyle =
                    new GUIStyle(
                        GUI.skin.label
                    );

                labelStyle.fontSize = 18;
                labelStyle.fontStyle =
                    FontStyle.Bold;

                labelStyle
                    .normal
                    .textColor =
                        Color.black;
            }

            if (buttonStyle == null)
            {
                buttonStyle =
                    new GUIStyle(
                        GUI.skin.button
                    );

                buttonStyle.fontSize = 15;
                buttonStyle.fontStyle =
                    FontStyle.Bold;

                buttonStyle
                    .normal
                    .textColor =
                        Color.black;
            }
        }
    }
}
