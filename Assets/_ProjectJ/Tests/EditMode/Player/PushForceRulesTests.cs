using NUnit.Framework; // NUnit 테스트 기능 참조
using ProjectJ.Player; // 밀치기 힘 규칙 참조
using UnityEngine; // Unity 벡터 기능 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class PushForceRulesTests // 밀치기 힘 합산 규칙 테스트 선언
    { // 밀치기 힘 테스트 묶음
        [Test] // 자동 테스트 항목 표시
        public void VerticalDirectionCreatesNoPushVelocity() // 수직 방향의 수평 밀치기 제거 확인
        { // 수직 방향 테스트 처리
            Vector3 result = PushForceRules.CreateHorizontalVelocity(Vector3.up, 6f); // 수직 방향 밀치기 속도 생성
            Assert.AreEqual(Vector3.zero, result); // 수평 힘 없는 결과 확인
        } // 수직 방향 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void SameDirectionPushesAreAdded() // 같은 방향 동시 밀치기 합산 확인
        { // 같은 방향 합산 테스트 처리
            Vector3 result = PushForceRules.CombineHorizontalVelocity(Vector3.right * 3f, Vector3.right * 4f, 10f); // 같은 방향 밀치기 합산
            Assert.AreEqual(7f, result.magnitude, 0.0001f); // 합산 속도 크기 확인
        } // 같은 방향 합산 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void OppositePushesCancelEachOther() // 반대 방향 동시 밀치기 상쇄 확인
        { // 반대 방향 합산 테스트 처리
            Vector3 result = PushForceRules.CombineHorizontalVelocity(Vector3.right * 6f, Vector3.left * 6f, 10f); // 반대 방향 밀치기 합산
            Assert.AreEqual(Vector3.zero, result); // 완전 상쇄 결과 확인
        } // 반대 방향 합산 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void CombinedPushNeverExceedsMaximumSpeed() // 합산 밀치기 최대 속도 제한 확인
        { // 최대 속도 테스트 처리
            Vector3 result = PushForceRules.CombineHorizontalVelocity(Vector3.forward * 8f, Vector3.forward * 8f, 10f); // 최대값을 넘는 밀치기 합산
            Assert.AreEqual(10f, result.magnitude, 0.0001f); // 최대 합산 속도 확인
        } // 최대 속도 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void RespawnProtectionRejectsPush() // 부활 보호 중 밀치기 차단 확인
        { // 부활 보호 테스트 처리
            bool result = PushForceRules.CanAcceptPush(true, 0f); // 부활 보호 상태 밀치기 판정
            Assert.IsFalse(result); // 밀치기 차단 결과 확인
        } // 부활 보호 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void HitImmunityRejectsSequentialPush() // 연속 피격 면역 중 밀치기 차단 확인
        { // 연속 피격 면역 테스트 처리
            bool result = PushForceRules.CanAcceptPush(false, 0.4f); // 남은 면역 시간 기반 밀치기 판정
            Assert.IsFalse(result); // 연속 밀치기 차단 결과 확인
        } // 연속 피격 면역 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void ExpiredImmunityAllowsNextPush() // 면역 종료 후 다음 밀치기 허용 확인
        { // 면역 종료 테스트 처리
            bool result = PushForceRules.CanAcceptPush(false, 0f); // 면역 종료 상태 밀치기 판정
            Assert.IsTrue(result); // 다음 밀치기 허용 결과 확인
        } // 면역 종료 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void ImmunityRemainingNeverBecomesNegative() // 남은 면역 시간 음수 방지 확인
        { // 면역 시간 하한 테스트 처리
            float result = PushForceRules.CalculateImmunityRemaining(0.2f, 1f); // 전체 면역 시간을 넘긴 감소 계산
            Assert.AreEqual(0f, result, 0.0001f); // 영초 하한 확인
        } // 면역 시간 하한 테스트 종료
    } // 밀치기 힘 테스트 묶음 종료
} // EditMode 테스트 묶음 종료
