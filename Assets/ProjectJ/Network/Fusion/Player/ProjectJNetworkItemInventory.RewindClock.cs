using System.Collections.Generic; // 서버 전용 위치 기록 사용
using Fusion; // Networked 상태와 TickTimer 사용
using ProjectJ.Items; // 되감기 시계 정책 사용
using UnityEngine; // Vector3와 CapsuleCollider 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        private struct RewindHistorySample
        {
            public float Time;
            public Vector3 Position;

            public RewindHistorySample(
                float time,
                Vector3 position
            )
            {
                Time = time;
                Position = position;
            }
        }

        private readonly List<RewindHistorySample> rewindHistory =
            new List<RewindHistorySample>(384); // 약 5.5초 서버 위치 기록

        private float rewindHistoryClock;
        private float rewindPlaybackStartHistoryTime;
        private int rewindObservedRespawnCount = -1;

        private CapsuleCollider rewindBodyCollider;
        private bool rewindColliderSuppressed;
        private bool rewindColliderWasEnabled;

        [Networked] // 되감기 진행 상태 동기화
        private NetworkBool NetworkRewindActive
        {
            get;
            set;
        }

        [Networked] // 0.8초 역재생 타이머 동기화
        private TickTimer NetworkRewindTimer
        {
            get;
            set;
        }

        [Networked] // 정확한 5초 전 최종 위치 동기화
        private Vector3 NetworkRewindTargetPosition
        {
            get;
            set;
        }

        [Networked] // 되감기 시작·종료 변경 횟수 동기화
        private int NetworkRewindRevision
        {
            get;
            set;
        }

        public bool IsRewindActive =>
            NetworkRewindActive;

        public Vector3 RewindTargetPosition =>
            NetworkRewindTargetPosition;

        public int RewindRevision =>
            NetworkRewindRevision;

        public float RewindRemaining
        {
            get
            {
                if (
                    Runner == null ||
                    !NetworkRewindActive
                )
                {
                    return 0f;
                }

                float? remaining =
                    NetworkRewindTimer.RemainingTime(Runner);

                return remaining.HasValue
                    ? Mathf.Max(0f, remaining.Value)
                    : 0f;
            }
        }

        private void InitializeRewindClockAuthority()
        {
            NetworkRewindActive = false;
            NetworkRewindTimer = TickTimer.None;
            NetworkRewindTargetPosition = transform.position;
            NetworkRewindRevision = 0;
            rewindObservedRespawnCount =
                externalGameplay != null
                    ? externalGameplay.RespawnCount
                    : -1;

            ResetRewindHistoryAuthority(true);
        }

        private bool UpdateRewindClockAuthority()
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return false;
            }

            ResolveReferences();

            if (externalGameplay == null)
            {
                return false;
            }

            if (
                rewindObservedRespawnCount !=
                externalGameplay.RespawnCount
            )
            {
                ClearRewindClockAuthority(true);
                rewindObservedRespawnCount =
                    externalGameplay.RespawnCount;
            }

            bool gameplayAllowed =
                externalGameplay.GameplayInputAllowed;

            if (!gameplayAllowed)
            {
                if (
                    NetworkRewindActive ||
                    rewindHistory.Count > 0
                )
                {
                    ClearRewindClockAuthority(true);
                }

                return false;
            }

            if (NetworkRewindActive)
            {
                UpdateRewindPlaybackAuthority();
                return true;
            }

            if (
                ProjectJRewindClockPolicy.ShouldRecord(
                    gameplayAllowed,
                    false
                )
            )
            {
                RecordRewindHistoryAuthority();
            }

            return false;
        }

        private bool UseRewindClockAuthority()
        {
            ResolveReferences();

            bool runnerReady =
                Runner != null &&
                Object != null &&
                Object.IsValid &&
                Object.HasStateAuthority;

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            bool hasFullHistory =
                TryResolveRewindTargetAuthority(
                    out Vector3 targetPosition
                );

            bool targetSafe =
                hasFullHistory &&
                externalGameplay != null &&
                externalGameplay.IsRewindTargetSafeAuthority(
                    targetPosition
                );

            bool cartRiding =
                IsCartRiding;

            bool grapplingHookActive =
                IsGrapplingHookActive;

            if (
                !ProjectJRewindClockPolicy.CanUse(
                    runnerReady,
                    gameplayAllowed,
                    hasFullHistory,
                    targetSafe,
                    cartRiding,
                    grapplingHookActive,
                    NetworkRewindActive
                )
            )
            {
                return false;
            }

            StopWaterGunAuthority();
            CancelFireworkPreparationAuthority(false);
            ClearFishingRodAuthority();
            ClearJetpackAuthority();
            ClearGiantBalloonAuthority();
            ClearSnowballSlowAuthority();
            ClearInkOctopusAuthority();
            ClearSoapBubbleAuthority();

            rewindPlaybackStartHistoryTime =
                rewindHistoryClock;

            NetworkRewindTargetPosition =
                targetPosition;

            NetworkRewindTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    ProjectJRewindClockPolicy.RewindDurationSeconds
                );

            NetworkRewindActive = true;
            NetworkRewindRevision++;

            externalGameplay.BeginRewindSuppressionAuthority();

            return true;
        }

        private void UpdateRewindPlaybackAuthority()
        {
            if (
                Runner == null ||
                externalGameplay == null
            )
            {
                CancelRewindAuthority();
                return;
            }

            externalGameplay.MaintainRewindSuppressionAuthority();

            if (
                NetworkRewindTimer.ExpiredOrNotRunning(Runner)
            )
            {
                FinishRewindAuthority();
                return;
            }

            float? remainingValue =
                NetworkRewindTimer.RemainingTime(Runner);

            float remaining =
                remainingValue.HasValue
                    ? Mathf.Max(0f, remainingValue.Value)
                    : 0f;

            float elapsed =
                ProjectJRewindClockPolicy.RewindDurationSeconds -
                remaining;

            float normalized =
                ProjectJRewindClockPolicy.CalculatePlaybackNormalized(
                    elapsed
                );

            float historyTime =
                ProjectJRewindClockPolicy.CalculatePlaybackHistoryTime(
                    rewindPlaybackStartHistoryTime,
                    normalized
                );

            if (
                TryEvaluateRewindHistoryPosition(
                    historyTime,
                    out Vector3 playbackPosition
                )
            )
            {
                transform.position =
                    playbackPosition;
            }

            if (
                ProjectJRewindClockPolicy.IsPlaybackComplete(
                    elapsed
                )
            )
            {
                FinishRewindAuthority();
            }
        }

        private void FinishRewindAuthority()
        {
            transform.position =
                NetworkRewindTargetPosition;

            if (externalGameplay != null)
            {
                externalGameplay.MaintainRewindSuppressionAuthority();
            }

            NetworkRewindActive = false;
            NetworkRewindTimer = TickTimer.None;
            NetworkRewindRevision++;

            ResetRewindHistoryAuthority(true);
        }

        private void CancelRewindAuthority()
        {
            NetworkRewindActive = false;
            NetworkRewindTimer = TickTimer.None;
            NetworkRewindTargetPosition = transform.position;
            NetworkRewindRevision++;

            ResetRewindHistoryAuthority(false);
        }

        private void ClearRewindClockAuthority(
            bool clearHistory
        )
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return;
            }

            NetworkRewindActive = false;
            NetworkRewindTimer = TickTimer.None;
            NetworkRewindTargetPosition = transform.position;
            NetworkRewindRevision++;

            if (clearHistory)
            {
                ResetRewindHistoryAuthority(false);
            }

            rewindObservedRespawnCount =
                externalGameplay != null
                    ? externalGameplay.RespawnCount
                    : rewindObservedRespawnCount;
        }

        private void RecordRewindHistoryAuthority()
        {
            Vector3 currentPosition =
                transform.position;

            if (
                !ProjectJRewindClockPolicy.IsFinitePosition(
                    currentPosition
                )
            )
            {
                return;
            }

            if (rewindHistory.Count == 0)
            {
                rewindHistoryClock = 0f;

                rewindHistory.Add(
                    new RewindHistorySample(
                        rewindHistoryClock,
                        currentPosition
                    )
                );

                return;
            }

            rewindHistoryClock +=
                Mathf.Max(
                    0f,
                    Runner.DeltaTime
                );

            rewindHistory.Add(
                new RewindHistorySample(
                    rewindHistoryClock,
                    currentPosition
                )
            );

            float retentionSeconds =
                ProjectJRewindClockPolicy.HistoryDurationSeconds +
                ProjectJRewindClockPolicy.HistoryRetentionSlackSeconds;

            float cutoffTime =
                rewindHistoryClock -
                retentionSeconds;

            while (
                rewindHistory.Count > 2 &&
                rewindHistory[1].Time < cutoffTime
            )
            {
                rewindHistory.RemoveAt(0);
            }
        }

        private bool TryResolveRewindTargetAuthority(
            out Vector3 targetPosition
        )
        {
            targetPosition =
                transform.position;

            if (rewindHistory.Count < 2)
            {
                return false;
            }

            float targetHistoryTime =
                rewindHistoryClock -
                ProjectJRewindClockPolicy.HistoryDurationSeconds;

            if (
                targetHistoryTime < 0f ||
                rewindHistory[0].Time > targetHistoryTime ||
                rewindHistory[rewindHistory.Count - 1].Time <
                targetHistoryTime
            )
            {
                return false;
            }

            return
                TryEvaluateRewindHistoryPosition(
                    targetHistoryTime,
                    out targetPosition
                );
        }

        private bool TryEvaluateRewindHistoryPosition(
            float historyTime,
            out Vector3 position
        )
        {
            position =
                transform.position;

            if (rewindHistory.Count == 0)
            {
                return false;
            }

            RewindHistorySample first =
                rewindHistory[0];

            RewindHistorySample last =
                rewindHistory[rewindHistory.Count - 1];

            if (historyTime <= first.Time)
            {
                position = first.Position;
                return true;
            }

            if (historyTime >= last.Time)
            {
                position = last.Position;
                return true;
            }

            int low = 0;
            int high = rewindHistory.Count - 1;

            while (high - low > 1)
            {
                int middle =
                    (low + high) / 2;

                if (
                    rewindHistory[middle].Time <=
                    historyTime
                )
                {
                    low = middle;
                }
                else
                {
                    high = middle;
                }
            }

            RewindHistorySample from =
                rewindHistory[low];

            RewindHistorySample to =
                rewindHistory[high];

            float duration =
                to.Time - from.Time;

            float t =
                duration > 0.0001f
                    ? Mathf.Clamp01(
                        (historyTime - from.Time) /
                        duration
                    )
                    : 0f;

            position =
                Vector3.Lerp(
                    from.Position,
                    to.Position,
                    t
                );

            return
                ProjectJRewindClockPolicy.IsFinitePosition(
                    position
                );
        }

        private void ResetRewindHistoryAuthority(
            bool seedCurrentPosition
        )
        {
            rewindHistory.Clear();
            rewindHistoryClock = 0f;
            rewindPlaybackStartHistoryTime = 0f;

            if (
                !seedCurrentPosition ||
                !ProjectJRewindClockPolicy.IsFinitePosition(
                    transform.position
                )
            )
            {
                return;
            }

            rewindHistory.Add(
                new RewindHistorySample(
                    0f,
                    transform.position
                )
            );
        }

        private void Update()
        {
            UpdateRewindCollisionLocal();
        }

        private void UpdateRewindCollisionLocal()
        {
            if (rewindBodyCollider == null)
            {
                rewindBodyCollider =
                    GetComponent<CapsuleCollider>();
            }

            if (rewindBodyCollider == null)
            {
                return;
            }

            bool shouldSuppress =
                Object != null &&
                Object.IsValid &&
                IsRewindActive;

            if (
                shouldSuppress &&
                !rewindColliderSuppressed
            )
            {
                rewindColliderWasEnabled =
                    rewindBodyCollider.enabled;

                if (rewindColliderWasEnabled)
                {
                    rewindBodyCollider.enabled =
                        false;
                }

                rewindColliderSuppressed =
                    true;

                return;
            }

            if (
                !shouldSuppress &&
                rewindColliderSuppressed
            )
            {
                rewindBodyCollider.enabled =
                    rewindColliderWasEnabled;

                rewindColliderSuppressed =
                    false;
            }
        }
    }
}
