using System.Collections.Generic; // 아이템 목록 구성
using NUnit.Framework; // EditMode 검증 기능
using ProjectJ.Items; // ItemDefinition 형식 사용
using UnityEditor; // 프로젝트 자산 조회

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJItemDefinitionCatalogTests // 초기 출시 아이템 데이터 검증
    {
        private const string ItemDirectory = "Assets/ProjectJ/Data/Items"; // 아이템 데이터 폴더

        [TestCase("SpringShoes", "spring_shoes", "스프링 신발", ItemCategory.Mobility, ItemUseMode.Instant, ItemTargetType.Self, 8f, false)] // 스프링 신발 기준
        [TestCase("JellyShield", "jelly_shield", "젤리 보호막", ItemCategory.Defense, ItemUseMode.Instant, ItemTargetType.Self, 4f, false)] // 젤리 보호막 기준
        [TestCase("BananaCushion", "banana_cushion", "바나나 쿠션", ItemCategory.Trap, ItemUseMode.Place, ItemTargetType.WorldPosition, 20f, true)] // 바나나 쿠션 기준
        [TestCase("BalloonHorn", "balloon_horn", "풍선 나팔", ItemCategory.Offensive, ItemUseMode.Instant, ItemTargetType.Area, 0f, false)] // 풍선 나팔 기준
        [TestCase("Jetpack", "jetpack", "제트팩", ItemCategory.Mobility, ItemUseMode.Hold, ItemTargetType.Self, 5f, false)] // 제트팩 기준
        [TestCase("WaterGun", "water_gun", "물총", ItemCategory.Offensive, ItemUseMode.Hold, ItemTargetType.OtherPlayer, 2.5f, false)] // 물총 기준
        [TestCase("Firework", "firework", "폭죽", ItemCategory.Offensive, ItemUseMode.Instant, ItemTargetType.Area, 0.9f, false)] // 폭죽 기준
        [TestCase("RewindClock", "rewind_clock", "되감기 시계", ItemCategory.Mobility, ItemUseMode.Instant, ItemTargetType.Self, 0.8f, false)] // 되감기 시계 기준
        [TestCase("Hammer", "hammer", "망치", ItemCategory.Offensive, ItemUseMode.Instant, ItemTargetType.Self, 6f, false)] // 망치 기준
        [TestCase("HomingMissile", "homing_missile", "유도탄", ItemCategory.Offensive, ItemUseMode.Instant, ItemTargetType.OtherPlayer, 10f, false)] // 유도탄 기준
        [TestCase("FeatherShoes", "feather_shoes", "깃털 신발", ItemCategory.Mobility, ItemUseMode.Instant, ItemTargetType.Self, 7f, false)] // 깃털 신발 기준
        [TestCase("ShrinkPotion", "shrink_potion", "소형화 물약", ItemCategory.Defense, ItemUseMode.Instant, ItemTargetType.Self, 6f, false)] // 소형화 물약 기준
        [TestCase("Bomb", "bomb", "폭탄", ItemCategory.Offensive, ItemUseMode.Instant, ItemTargetType.Area, 2.5f, false)] // 폭탄 기준
        [TestCase("PufferBalloonSuit", "puffer_balloon_suit", "복어 풍선옷", ItemCategory.Defense, ItemUseMode.Instant, ItemTargetType.Self, 5f, false)] // 복어 풍선옷 기준
        [TestCase("InkOctopus", "ink_octopus", "먹물 문어", ItemCategory.Offensive, ItemUseMode.Instant, ItemTargetType.OtherPlayer, 3.5f, false)] // 먹물 문어 기준
        [TestCase("FishingRod", "fishing_rod", "낚시대", ItemCategory.Offensive, ItemUseMode.Instant, ItemTargetType.OtherPlayer, 0.6f, false)] // 낚시대 기준
        [TestCase("GrapplingHook", "grappling_hook", "갈고리", ItemCategory.Mobility, ItemUseMode.Instant, ItemTargetType.WorldPosition, 1.5f, false)] // 갈고리 기준
        [TestCase("Drone", "drone", "드론", ItemCategory.Offensive, ItemUseMode.Instant, ItemTargetType.OtherPlayer, 12f, false)] // 드론 기준
        [TestCase("Snowball", "snowball", "눈덩이", ItemCategory.Offensive, ItemUseMode.Instant, ItemTargetType.OtherPlayer, 3f, false)] // 눈덩이 기준
        [TestCase("SoapBubble", "soap_bubble", "비눗방울", ItemCategory.Offensive, ItemUseMode.Instant, ItemTargetType.OtherPlayer, 2.5f, false)] // 비눗방울 기준
        [TestCase("SmokeGrenade", "smoke_grenade", "연막탄", ItemCategory.Offensive, ItemUseMode.Instant, ItemTargetType.Area, 6f, false)] // 연막탄 기준
        [TestCase("Mine", "mine", "지뢰", ItemCategory.Trap, ItemUseMode.Place, ItemTargetType.WorldPosition, 25f, true)] // 지뢰 기준
        [TestCase("InvisibilityCloak", "invisibility_cloak", "투명 망토", ItemCategory.Defense, ItemUseMode.Instant, ItemTargetType.Self, 5f, false)] // 투명 망토 기준
        [TestCase("Trampoline", "trampoline", "트램폴린", ItemCategory.Mobility, ItemUseMode.Place, ItemTargetType.WorldPosition, 12f, true)] // 트램폴린 기준
        [TestCase("PoolBall", "pool_ball", "풀 공", ItemCategory.Offensive, ItemUseMode.Instant, ItemTargetType.OtherPlayer, 0f, false)] // 풀 공 기준
        [TestCase("SniperWaterGun", "sniper_water_gun", "저격 물총", ItemCategory.Offensive, ItemUseMode.Instant, ItemTargetType.OtherPlayer, 0.8f, false)] // 저격 물총 기준
        [TestCase("GiantBalloon", "giant_balloon", "거대 풍선", ItemCategory.Mobility, ItemUseMode.Instant, ItemTargetType.Self, 6f, false)] // 거대 풍선 기준
        [TestCase("Cart", "cart", "카트", ItemCategory.Mobility, ItemUseMode.Instant, ItemTargetType.Self, 8f, false)] // 카트 기준
        [TestCase("HandMirror", "hand_mirror", "손거울", ItemCategory.Defense, ItemUseMode.Instant, ItemTargetType.Self, 4f, false)] // 손거울 기준
        public void InitialReleaseItem_WithPlannedValues_IsValid( // 개별 기획 데이터 검증
            string assetName, // 자산 파일 이름
            string itemId, // 고유 ID
            string displayName, // 한글 표시 이름
            ItemCategory category, // 아이템 분류
            ItemUseMode useMode, // 사용 방식
            ItemTargetType targetType, // 대상 방식
            float duration, // 지속 시간
            bool isPlaceable // 설치 가능 여부
        )
        {
            string assetPath = $"{ItemDirectory}/Item_{assetName}.asset"; // 예상 자산 경로
            ItemDefinition definition = AssetDatabase.LoadAssetAtPath<ItemDefinition>(assetPath); // Definition 자산 로드

            Assert.IsNotNull(definition, assetPath); // 자산 존재 확인
            Assert.AreEqual(itemId, definition.ItemId, assetPath); // 고유 ID 확인
            Assert.AreEqual(displayName, definition.DisplayName, assetPath); // 표시 이름 확인
            Assert.AreEqual(category, definition.Category, assetPath); // 아이템 분류 확인
            Assert.AreEqual(useMode, definition.UseMode, assetPath); // 사용 방식 확인
            Assert.AreEqual(targetType, definition.TargetType, assetPath); // 대상 방식 확인
            Assert.AreEqual(duration, definition.Duration, 0.0001f, assetPath); // 지속 시간 확인
            Assert.AreEqual(0f, definition.Cooldown, 0.0001f, assetPath); // 단일 사용 재사용 시간 확인
            Assert.AreEqual(isPlaceable, definition.IsPlaceable, assetPath); // 설치 가능 여부 확인
            Assert.IsNotNull(definition.Icon, assetPath); // 임시 아이콘 포함 확인
        }

        [Test] // 전체 목록 검증
        public void InitialReleaseCatalog_HasTwentyNineUniqueValidItems() // 누락·중복 데이터 방지
        {
            string[] itemGuids = AssetDatabase.FindAssets("t:ItemDefinition", new[] { ItemDirectory }); // Definition GUID 조회
            List<ItemDefinition> definitions = new List<ItemDefinition>(); // 전체 Definition 목록 생성

            for (int index = 0; index < itemGuids.Length; index++) // 모든 Definition 순회
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(itemGuids[index]); // GUID 경로 변환
                ItemDefinition definition = AssetDatabase.LoadAssetAtPath<ItemDefinition>(assetPath); // Definition 자산 로드
                definitions.Add(definition); // 검증 목록 추가
            }

            List<string> errors = ItemDefinitionValidator.ValidateCatalog(definitions); // 공통 유효성 검사

            Assert.AreEqual(29, definitions.Count); // 초기 출시 29종 확인
            Assert.IsEmpty(errors, string.Join("\n", errors)); // 누락·중복 오류 확인
        }

        [Test] // 손거울 임시 이미지 검증
        public void HandMirror_UsesJellyShieldIconAsTemporaryImage() // 손거울 이미지 누락 방지
        {
            ItemDefinition handMirror = AssetDatabase.LoadAssetAtPath<ItemDefinition>( // 손거울 Definition 로드
                $"{ItemDirectory}/Item_HandMirror.asset" // 손거울 자산 경로
            );
            ItemDefinition jellyShield = AssetDatabase.LoadAssetAtPath<ItemDefinition>( // 젤리 보호막 Definition 로드
                $"{ItemDirectory}/Item_JellyShield.asset" // 젤리 보호막 자산 경로
            );

            Assert.IsNotNull(handMirror); // 손거울 자산 확인
            Assert.IsNotNull(jellyShield); // 젤리 보호막 자산 확인
            Assert.AreSame(jellyShield.Icon, handMirror.Icon); // 임시 이미지 재사용 확인
        }
    }
}
