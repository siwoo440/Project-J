using System; // Type.EmptyTypes 사용
using System.Reflection; // Private Constant와 Method Reflection 사용
using NUnit.Framework; // EditMode Test 사용
using ProjectJ.AI; // Bot Spawn 정책 사용
using UnityEditor; // Prefab Asset 조회 사용
using UnityEditor.SceneManagement; // Game Scene Test 로드 사용
using UnityEngine; // GameObject와 Spawn Pose 사용
using UnityEngine.SceneManagement; // Scene 상태 조회 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJDay139BotSpawnSoloStartTests
    {
        private const string FusionNamespace =
            "ProjectJ.Networking.Fusion."; // Assembly-CSharp의 Fusion Namespace

        private const string DefaultRuntimeAssembly =
            "Assembly-CSharp"; // asmdef 외 Runtime Script 기본 Assembly

        [Test]
        public void IsSpawnSlotClear_RejectsOccupiedSlot()
        {
            bool result =
                ProjectJBotSpawnPolicy.IsSpawnSlotClear(
                    0.45f,
                    1f
                ); // 기존 참가자와 겹치는 Spawn Slot 판정

            Assert.That(
                result,
                Is.False
            ); // 겹침 Spawn 차단 검증
        }

        [Test]
        public void IsSpawnSlotClear_AllowsSeparatedSlot()
        {
            bool result =
                ProjectJBotSpawnPolicy.IsSpawnSlotClear(
                    1.25f,
                    1f
                ); // 충분히 떨어진 Spawn Slot 판정

            Assert.That(
                result,
                Is.True
            ); // 안전 Spawn 허용 검증
        }

        [Test]
        public void IsSpawnSlotClear_AllowsEmptyField()
        {
            bool result =
                ProjectJBotSpawnPolicy.IsSpawnSlotClear(
                    float.PositiveInfinity,
                    1f
                ); // 참가자가 없는 Spawn Field 판정

            Assert.That(
                result,
                Is.True
            ); // 빈 Field Spawn 허용 검증
        }

        [Test]
        public void ResolveStartDelaySeconds_StaggersBots()
        {
            float result =
                ProjectJBotSpawnPolicy.ResolveStartDelaySeconds(
                    3,
                    0.25f,
                    1.5f
                ); // 네 번째 Bot 출발 지연 계산

            Assert.That(
                result,
                Is.EqualTo(
                    0.75f
                ).Within(
                    0.0001f
                )
            ); // 순차 출발 시간 검증
        }

        [Test]
        public void ResolveStartDelaySeconds_ClampsMaximumDelay()
        {
            float result =
                ProjectJBotSpawnPolicy.ResolveStartDelaySeconds(
                    20,
                    0.25f,
                    1.5f
                ); // 많은 Bot의 최대 지연 계산

            Assert.That(
                result,
                Is.EqualTo(
                    1.5f
                ).Within(
                    0.0001f
                )
            ); // 최대 출발 지연 제한 검증
        }

        [Test]
        public void LobbyFlow_AllowsSingleReadyPlayer()
        {
            Type lobbyFlowType =
                ResolveFusionType(
                    "ProjectJNetworkLobbyFlow"
                ); // Assembly-CSharp에서 Lobby Flow Type 조회

            Assert.That(
                lobbyFlowType,
                Is.Not.Null
            ); // Lobby Flow Type 존재 검증

            FieldInfo field =
                lobbyFlowType.GetField(
                    "MinimumReadyPlayers",
                    BindingFlags.NonPublic |
                    BindingFlags.Static
                ); // Lobby 최소 Ready 인원 Constant 조회

            Assert.That(
                field,
                Is.Not.Null
            ); // 최소 인원 Constant 존재 검증

            Assert.That(
                (int)field.GetRawConstantValue(),
                Is.EqualTo(
                    1
                )
            ); // 1인 Lobby 시작 허용 검증
        }

        [Test]
        public void BotController_HasStartDelayConfiguration()
        {
            Type botControllerType =
                ResolveFusionType(
                    "ProjectJNetworkBotController"
                ); // Assembly-CSharp에서 Bot Controller Type 조회

            Assert.That(
                botControllerType,
                Is.Not.Null
            ); // Bot Controller Type 존재 검증

            MethodInfo method =
                botControllerType.GetMethod(
                    "ConfigureStartDelay",
                    BindingFlags.Public |
                    BindingFlags.Instance
                ); // Bot 순차 출발 설정 Method 조회

            Assert.That(
                method,
                Is.Not.Null
            ); // 순차 출발 Hook 적용 검증
        }

        [Test]
        public void BotRoster_UsesClearSpawnResolver()
        {
            Type botRosterType =
                ResolveFusionType(
                    "ProjectJNetworkBotRosterManager"
                ); // Assembly-CSharp에서 Bot Roster Type 조회

            Assert.That(
                botRosterType,
                Is.Not.Null
            ); // Bot Roster Type 존재 검증

            MethodInfo method =
                botRosterType.GetMethod(
                    "ResolveBotSpawnPoint",
                    BindingFlags.NonPublic |
                    BindingFlags.Instance,
                    null,
                    Type.EmptyTypes,
                    null
                ); // 인원 점유 확인형 Spawn Resolver 조회

            Assert.That(
                method,
                Is.Not.Null
            ); // 안전 Spawn Resolver 적용 검증
        }

        [Test] // Player 장면 Spawn Pose 검증
        public void PlayerSpawner_ResolvesNumberedSceneSpawnPose() // Player가 번호 Spawn 위치를 사용하는지 검증
        {
            const string gameScenePath = "Assets/ProjectJ/Scenes/Game.unity"; // 경기 Scene 경로
            Scene existingScene = SceneManager.GetSceneByPath(gameScenePath); // 기존 경기 Scene 조회
            bool openedForTest = !existingScene.IsValid() || !existingScene.isLoaded; // Test 전용 로드 필요 여부
            Scene gameScene = openedForTest // 사용할 경기 Scene 선택
                ? EditorSceneManager.OpenScene(gameScenePath, OpenSceneMode.Additive) // 경기 Scene 추가 로드
                : existingScene; // 기존 경기 Scene 사용

            try // Scene 정리 보장
            {
                Type spawnerType = ResolveFusionType("ProjectJNetworkPlayerSpawner"); // Player Spawner Type 조회
                MethodInfo method = spawnerType?.GetMethod( // 장면 Spawn Pose Resolver 조회
                    "TryGetSpawnPoseForSlot", // Resolver Method 이름
                    BindingFlags.NonPublic | BindingFlags.Static // Private Static Method 범위
                );

                Assert.That(method, Is.Not.Null); // 장면 Spawn Pose Resolver 존재 검증
                object[] arguments = { 0, Vector3.zero, Quaternion.identity }; // Spawn_00 호출 인자 생성
                bool found = (bool)method.Invoke(null, arguments); // Spawn_00 Pose 조회 실행
                Vector3 position = (Vector3)arguments[1]; // 반환 위치 조회
                Quaternion rotation = (Quaternion)arguments[2]; // 반환 회전 조회

                Assert.That(found, Is.True); // Spawn_00 발견 검증
                Assert.That(position, Is.EqualTo(new Vector3(-6f, 2f, -3f))); // Spawn_00 위치 검증
                Assert.That(rotation, Is.EqualTo(Quaternion.identity)); // Spawn_00 회전 검증
            }
            finally // 추가 Scene 정리
            {
                if (openedForTest && gameScene.IsValid()) // Test 전용 Scene 확인
                {
                    EditorSceneManager.CloseScene(gameScene, true); // Test 전용 Scene 닫기
                }
            }
        }

        [TestCase("Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkPlayer.prefab")] // 실제 Player Prefab 경로
        [TestCase("Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkBot.prefab")] // Bot Player Prefab 경로
        public void NetworkCharacterPrefab_UsesPlayerLayer( // Player 물리 Layer 적용 검증
            string prefabPath // 검사 Prefab 경로
        )
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath); // 실제 Prefab Asset 로드
            int playerLayer = LayerMask.NameToLayer("Player"); // Player Layer 번호 조회

            Assert.That(prefab, Is.Not.Null); // Prefab 존재 검증
            Assert.That(playerLayer, Is.GreaterThanOrEqualTo(0)); // Player Layer 설정 검증

            Transform[] hierarchy = prefab.GetComponentsInChildren<Transform>(true); // Prefab 전체 계층 조회

            for (int index = 0; index < hierarchy.Length; index++) // Prefab 계층 순회
            {
                Assert.That( // 현재 객체 Layer 검증
                    hierarchy[index].gameObject.layer, // 현재 객체 Layer 전달
                    Is.EqualTo(playerLayer), // Player Layer 기대
                    prefabPath + " / " + hierarchy[index].name // 실패 대상 표시
                );
            }
        }

        private static Type ResolveFusionType(
            string typeName
        )
        {
            return Type.GetType(
                FusionNamespace +
                typeName +
                ", " +
                DefaultRuntimeAssembly,
                false
            ); // asmdef Test에서 기본 Runtime Assembly Type 조회
        }
    }
}
