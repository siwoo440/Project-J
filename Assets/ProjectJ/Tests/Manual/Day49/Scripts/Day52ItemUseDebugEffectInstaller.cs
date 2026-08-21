using ProjectJ.Items; // 아이템 공통 사용 시스템 사용
using UnityEngine; // 유니티 기능 사용
using UnityEngine.SceneManagement; // Scene 이름 확인

namespace ProjectJ.Tests.Manual // 수동 테스트 네임스페이스
{
    public sealed class Day52ItemUseDebugEffectInstaller : MonoBehaviour // Day49 테스트맵 전용 임시 Effect 등록기
    {
        private const string TargetSceneName = "Day49_AllSystemsTest"; // 테스트 대상 Scene

        private static readonly string[] TestItemIds = // 51일차 대표 아이템 ID
        {
            "spring_shoes",
            "jelly_shield",
            "banana_cushion",
            "balloon_horn",
            "water_gun"
        };

        private DebugConsumeEffect debugEffect; // 공통 임시 성공 Effect

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] // Scene 로드 후 자동 실행
        private static void CreateForDay49Scene() // Day49 Scene에서만 등록기 생성
        {
            if (SceneManager.GetActiveScene().name != TargetSceneName) // 대상 Scene 검사
            {
                return; // 다른 Scene에서는 테스트 Effect를 등록하지 않음
            }

            Day52ItemUseDebugEffectInstaller existing =
                FindFirstObjectByType<Day52ItemUseDebugEffectInstaller>(); // 중복 Installer 탐색

            if (existing != null) // 이미 존재하는 경우
            {
                return; // 중복 생성 방지
            }

            GameObject installerObject =
                new GameObject("=== Day52 Item Use Debug Effects ==="); // 테스트 전용 Root 생성

            installerObject.AddComponent<Day52ItemUseDebugEffectInstaller>(); // 등록기 추가
        }

        private void Awake() // 테스트 Effect 등록
        {
            debugEffect = new DebugConsumeEffect(); // 공통 성공 Effect 생성

            for (int i = 0; i < TestItemIds.Length; i++) // 대표 5종 반복
            {
                ItemUseEffectRegistry.Register(
                    TestItemIds[i],
                    debugEffect
                ); // 각 Item ID에 임시 Effect 등록
            }
        }

        private void OnDestroy() // Scene 종료 시 테스트 Effect 해제
        {
            if (debugEffect == null) // Effect 생성 여부 검사
            {
                return; // 해제할 내용 없음
            }

            for (int i = 0; i < TestItemIds.Length; i++) // 대표 5종 반복
            {
                ItemUseEffectRegistry.Unregister(
                    TestItemIds[i],
                    debugEffect
                ); // 테스트 Effect 등록 해제
            }
        }

        private sealed class DebugConsumeEffect : IItemUseEffect // 52일차 공통 사용 흐름 확인용 Effect
        {
            public ItemUseResult TryUse(ItemUseContext context) // 사용 성공 처리
            {
                if (context.Definition == null) // 잘못된 Context 검사
                {
                    return ItemUseResult.Fail(
                        ItemUseStatus.InvalidItem,
                        "ItemDefinition이 없습니다."
                    ); // 데이터 오류 반환
                }

                Debug.Log(
                    $"[Day52 Test] {context.Definition.DisplayName} 사용 성공",
                    context.User
                ); // 실제 입력과 Effect 실행 여부 확인

                return ItemUseResult.Success(); // 성공을 반환해 선택 슬롯 소비 허용
            }
        }
    }
}
