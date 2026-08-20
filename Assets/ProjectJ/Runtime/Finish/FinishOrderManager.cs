using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectJ.Finish
{
    [DisallowMultipleComponent]
    public sealed class FinishOrderManager :
        MonoBehaviour
    {
        [SerializeField]
        private List<PlayerFinishState> finishers =
            new List<PlayerFinishState>();

        public event Action<PlayerFinishState>
            PlayerFinished;

        public IReadOnlyList<PlayerFinishState>
            Finishers
        {
            get
            {
                return finishers;
            }
        }

        public int FinishCount
        {
            get
            {
                return finishers.Count;
            }
        }

        public bool TryRegisterFinish(
            PlayerFinishState player,
            double timestamp
        )
        {
            RemoveNullFinishers();

            if (
                player == null ||
                player.IsFinished ||
                finishers.Contains(
                    player
                )
            )
            {
                return false;
            }

            int order =
                finishers.Count + 1;

            bool confirmed =
                player.TryConfirmFinish(
                    order,
                    timestamp
                );

            if (!confirmed)
            {
                return false;
            }

            finishers.Add(
                player
            );

            PlayerFinished?.Invoke(
                player
            );

            return true;
        }

        private void RemoveNullFinishers()
        {
            for (
                int i =
                    finishers.Count - 1;
                i >= 0;
                i--
            )
            {
                if (finishers[i] == null)
                {
                    finishers.RemoveAt(
                        i
                    );
                }
            }
        }
    }
}
