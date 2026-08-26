using NUnit.Framework; // EditMode Test 사용
using ProjectJ.AI; // Bot 경쟁 정책 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJBotCompetitionPolicyTests
    {
        [Test]
        public void ShouldAttemptPush_AllowsNearbyForwardTarget()
        {
            bool result =
                ProjectJBotCompetitionPolicy.ShouldAttemptPush(
                    1.5f,
                    2.5f,
                    0.9f,
                    0.707f,
                    false,
                    false,
                    0f
                ); // 가까운 전방 상대 Push 판정

            Assert.That(
                result,
                Is.True
            ); // 기본 Push 정책 유지 검증
        }

        [Test]
        public void ShouldAttemptProgressPush_AllowsImmediateBlockingTarget()
        {
            bool result =
                ProjectJBotCompetitionPolicy.ShouldAttemptProgressPush(
                    1f,
                    1.35f,
                    0.98f,
                    0.94f,
                    false,
                    false,
                    0f,
                    0f
                ); // 진행 방향 바로 앞 상대 Push 판정

            Assert.That(
                result,
                Is.True
            ); // 즉시 진행 방해 상대만 Push 허용 검증
        }

        [Test]
        public void ShouldAttemptProgressPush_RejectsDistantOpponent()
        {
            bool result =
                ProjectJBotCompetitionPolicy.ShouldAttemptProgressPush(
                    1.8f,
                    1.35f,
                    1f,
                    0.94f,
                    false,
                    false,
                    0f,
                    0f
                ); // 멀리 있는 전방 상대 Push 판정

            Assert.That(
                result,
                Is.False
            ); // 상대 추적형 Push 차단 검증
        }

        [Test]
        public void ShouldAttemptProgressPush_RejectsBotDecisionCooldown()
        {
            bool result =
                ProjectJBotCompetitionPolicy.ShouldAttemptProgressPush(
                    0.8f,
                    1.35f,
                    1f,
                    0.94f,
                    false,
                    false,
                    0f,
                    2f
                ); // Bot 자체 Push 판단 쿨타임 판정

            Assert.That(
                result,
                Is.False
            ); // 연속 Push 시도 차단 검증
        }

        [Test]
        public void ResolveDesiredBotCount_FillsMissingParticipants()
        {
            int result =
                ProjectJBotCompetitionPolicy.ResolveDesiredBotCount(
                    8,
                    3,
                    8
                ); // 8인 경기 3 Human 부족 인원 계산

            Assert.That(
                result,
                Is.EqualTo(
                    5
                )
            ); // 부족한 5 Bot 계산 검증
        }

        [Test]
        public void IsRosterFilled_RequiresHumansAndBotsToReachTarget()
        {
            bool result =
                ProjectJBotCompetitionPolicy.IsRosterFilled(
                    8,
                    1,
                    7
                ); // Human과 Bot 합산 참가 인원 판정

            Assert.That(
                result,
                Is.True
            ); // 8명 충원 완료 검증
        }

        [Test]
        public void IsRosterFilled_RejectsMissingBot()
        {
            bool result =
                ProjectJBotCompetitionPolicy.IsRosterFilled(
                    8,
                    1,
                    6
                ); // Bot 한 명 부족 상태 판정

            Assert.That(
                result,
                Is.False
            ); // 불완전 Roster Countdown 차단 검증
        }

        [Test]
        public void ShouldStartCountdown_RequiresStableFilledRoster()
        {
            bool result =
                ProjectJBotCompetitionPolicy.ShouldStartCountdown(
                    true,
                    1f,
                    0.75f
                ); // 안정화된 충원 상태 Countdown 판정

            Assert.That(
                result,
                Is.True
            ); // 안정화 후 Countdown 허용 검증
        }

        [Test]
        public void ShouldStartCountdown_RejectsImmediateFillFrame()
        {
            bool result =
                ProjectJBotCompetitionPolicy.ShouldStartCountdown(
                    true,
                    0f,
                    0.75f
                ); // 막 충원된 Roster Countdown 판정

            Assert.That(
                result,
                Is.False
            ); // Spawn 직후 즉시 Countdown 차단 검증
        }

        [Test]
        public void ResolvePreferredItemSlot_UsesAvailableLeftSlot()
        {
            int result =
                ProjectJBotCompetitionPolicy.ResolvePreferredItemSlot(
                    0,
                    0,
                    24,
                    0
                ); // 선택 Item 없음과 왼쪽 Item 상태 계산

            Assert.That(
                result,
                Is.EqualTo(
                    0
                )
            ); // 왼쪽 Item 선택 검증
        }

        [Test]
        public void ShouldAttemptItemUse_RequiresOpponentForAttackItem()
        {
            bool result =
                ProjectJBotCompetitionPolicy.ShouldAttemptItemUse(
                    24,
                    0f,
                    true,
                    false,
                    true
                ); // 대상 없는 공격형 Item 사용 판정

            Assert.That(
                result,
                Is.False
            ); // 공격형 Item 낭비 차단 검증
        }

        [Test]
        public void ShouldAttemptItemUse_AllowsUtilityItemWithoutOpponent()
        {
            bool result =
                ProjectJBotCompetitionPolicy.ShouldAttemptItemUse(
                    7,
                    0f,
                    false,
                    false,
                    true
                ); // Utility Item 사용 판정

            Assert.That(
                result,
                Is.True
            ); // Utility Item 단독 사용 허용 검증
        }
    }
}
