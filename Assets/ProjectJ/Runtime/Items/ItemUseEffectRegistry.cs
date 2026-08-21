using System; // StringComparer 사용
using System.Collections.Generic; // Dictionary 사용
using UnityEngine; // Runtime 초기화 Attribute 사용

namespace ProjectJ.Items // 아이템 시스템 네임스페이스
{
    public static class ItemUseEffectRegistry // Item ID와 실제 Effect를 연결하는 공통 Registry
    {
        private static readonly Dictionary<string, IItemUseEffect> Effects =
            new Dictionary<string, IItemUseEffect>(
                StringComparer.OrdinalIgnoreCase
            ); // Item ID 대소문자를 무시하고 Effect 저장

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // Play 시작 시 Static 상태 초기화
        private static void ResetRuntimeState() // Domain Reload 설정과 무관하게 Registry 초기화
        {
            Effects.Clear(); // 이전 Play 상태 제거
        }

        public static bool Register( // Item Effect 등록
            string itemId,
            IItemUseEffect effect
        )
        {
            if (
                string.IsNullOrWhiteSpace(itemId) ||
                effect == null
            ) // 잘못된 등록 요청 검사
            {
                return false; // 등록 실패
            }

            Effects[itemId.Trim()] = effect; // 같은 ID는 최신 Effect로 교체
            return true; // 등록 성공
        }

        public static bool Unregister( // Item Effect 등록 해제
            string itemId,
            IItemUseEffect effect
        )
        {
            if (
                string.IsNullOrWhiteSpace(itemId) ||
                effect == null
            ) // 잘못된 해제 요청 검사
            {
                return false; // 해제 실패
            }

            string normalizedId = itemId.Trim(); // 공통 ID 정리

            if (!Effects.TryGetValue(normalizedId, out IItemUseEffect current)) // 등록 여부 검사
            {
                return false; // 등록되지 않은 Effect
            }

            if (!ReferenceEquals(current, effect)) // 다른 Effect가 현재 등록되어 있는지 검사
            {
                return false; // 다른 Effect는 제거하지 않음
            }

            return Effects.Remove(normalizedId); // 현재 Effect 제거 결과 반환
        }

        public static bool TryResolve( // ItemDefinition에 대응하는 Effect 탐색
            ItemDefinition definition,
            out IItemUseEffect effect
        )
        {
            effect = null; // 실패 기본값

            if (
                definition == null ||
                string.IsNullOrWhiteSpace(definition.ItemId)
            ) // ItemDefinition 유효성 기본 검사
            {
                return false; // 탐색 실패
            }

            return Effects.TryGetValue(
                definition.ItemId.Trim(),
                out effect
            ); // 등록된 Effect 반환
        }

        public static void Clear() // 테스트와 개발용 전체 초기화
        {
            Effects.Clear(); // 모든 Effect 제거
        }
    }
}
