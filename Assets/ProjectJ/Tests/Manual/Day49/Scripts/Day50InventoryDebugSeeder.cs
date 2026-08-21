using System.Collections; // Coroutine 사용
using ProjectJ.Items; // 아이템 시스템 사용
using UnityEngine; // 유니티 기능 사용
using UnityEngine.SceneManagement; // 현재 Scene 확인

namespace ProjectJ.Tests.Manual // 수동 테스트 네임스페이스
{
    public sealed class Day50InventoryDebugSeeder : MonoBehaviour // Day49 통합맵 UI 확인용 임시 아이템 지급
    {
        private const string TargetSceneName = "Day49_AllSystemsTest"; // 테스트 대상 Scene

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] // Scene 시작 후 자동 실행
        private static void CreateSeeder() // 테스트 Scene에서만 Seeder 생성
        {
            if (SceneManager.GetActiveScene().name != TargetSceneName) // Day49 Scene 여부 검사
            {
                return; // 다른 Scene에서는 실행하지 않음
            }

            GameObject seederObject =
                new GameObject("=== Day50 Inventory Debug Seeder ==="); // 테스트 Seeder 생성

            seederObject.AddComponent<Day50InventoryDebugSeeder>(); // Seeder 컴포넌트 추가
        }

        private IEnumerator Start() // Inventory 설치를 기다린 뒤 테스트 아이템 지급
        {
            PlayerItemInventory inventory = null; // Inventory 참조 초기화

            while (inventory == null) // Runtime Installer가 Inventory를 붙일 때까지 대기
            {
                inventory =
                    FindFirstObjectByType<PlayerItemInventory>(); // 현재 Player Inventory 탐색

                if (inventory == null) // 아직 준비되지 않은 경우
                {
                    yield return null; // 다음 Frame까지 대기
                }
            }

            if (inventory.GetItem(0) != null || inventory.GetItem(1) != null) // 기존 아이템 존재 검사
            {
                Destroy(gameObject); // 실제 테스트 데이터가 있으면 Debug Seed 생략
                yield break; // Coroutine 종료
            }

            ItemDefinition springShoes =
                ScriptableObject.CreateInstance<ItemDefinition>(); // 스프링 신발 임시 데이터 생성

            springShoes.hideFlags = HideFlags.DontSave; // 에셋으로 저장하지 않음

            springShoes.Configure(
                "spring_shoes",
                "Spring Shoes",
                ItemCategory.Mobility,
                ItemUseMode.Instant,
                ItemTargetType.Self,
                8f,
                0f,
                false
            ); // 스프링 신발 공통 데이터 설정

            ItemDefinition jellyShield =
                ScriptableObject.CreateInstance<ItemDefinition>(); // 젤리 보호막 임시 데이터 생성

            jellyShield.hideFlags = HideFlags.DontSave; // 에셋으로 저장하지 않음

            jellyShield.Configure(
                "jelly_shield",
                "Jelly Shield",
                ItemCategory.Defense,
                ItemUseMode.Instant,
                ItemTargetType.Self,
                4f,
                0f,
                false
            ); // 젤리 보호막 공통 데이터 설정

            inventory.TryAdd(springShoes, out _); // 첫 슬롯에 테스트 아이템 지급
            inventory.TryAdd(jellyShield, out _); // 두 번째 슬롯에 테스트 아이템 지급
            inventory.SelectSlot(0); // Q 슬롯 기본 선택

            Destroy(gameObject); // Seed 완료 후 Seeder 제거
        }
    }
}
