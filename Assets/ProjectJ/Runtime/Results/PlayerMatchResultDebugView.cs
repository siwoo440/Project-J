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
                    "Simulate Time End Result",
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
                    "Personal Result : Waiting" +
                    "\nFINISH 또는 시간 종료 시 생성";
            }

            PlayerMatchResult result =
                collector.CurrentResult;

            if (result == null)
            {
                return
                    "Personal Result : Missing";
            }

            string finishTime =
                result.HasFinishTime
                    ? result.FinishTime
                        .ToString("0.000")
                    : "--";

            return
                "Personal Result : CREATED" +
                "\nPlayer ID : " +
                result.PlayerId +
                "\nFinal Rank : " +
                result.FinalRank +
                "\nFinished : " +
                result.IsFinished +
                "\nFinish Order : " +
                result.FinishOrder +
                "\nFinish Time : " +
                finishTime +
                "\nHighest Height : " +
                result.HighestHeight
                    .ToString("0.00") +
                "m" +
                "\nHighest Checkpoint : " +
                result.HighestCheckpoint;
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
