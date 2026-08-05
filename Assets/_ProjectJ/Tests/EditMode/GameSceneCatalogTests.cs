using System.Collections.Generic; // 중복 씬 이름 검사 컬렉션 기능 참조
using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Core.SceneFlow; // 프로젝트 씬 흐름 형식 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{
    public sealed class GameSceneCatalogTests // 씬 카탈로그 검증 테스트 형식 선언
    {
        [Test] // Unity Test Runner 테스트 지정
        public void BuildOrderContainsExpectedScenes() // 빌드 순서에 기본 씬이 모두 포함되는지 검증
        {
            GameSceneId[] buildOrder = GameSceneCatalog.GetBuildOrder(); // 빌드 순서 배열 조회

            Assert.AreEqual(6, buildOrder.Length); // 기본 씬 수가 여섯 개인지 비교
            Assert.AreEqual(GameSceneId.Bootstrap, buildOrder[0]); // 첫 번째 씬이 Bootstrap인지 비교
            Assert.AreEqual(GameSceneId.MainMenu, buildOrder[1]); // 두 번째 씬이 MainMenu인지 비교
            Assert.AreEqual(GameSceneId.Lobby, buildOrder[2]); // 세 번째 씬이 Lobby인지 비교
            Assert.AreEqual(GameSceneId.MatchLoading, buildOrder[3]); // 네 번째 씬이 MatchLoading인지 비교
            Assert.AreEqual(GameSceneId.Game, buildOrder[4]); // 다섯 번째 씬이 Game인지 비교
            Assert.AreEqual(GameSceneId.Tests, buildOrder[5]); // 여섯 번째 씬이 Tests인지 비교
        }

        [Test] // Unity Test Runner 테스트 지정
        public void EverySceneIdHasUniqueSceneName() // 모든 씬 식별자의 씬 이름 중복 여부 검증
        {
            GameSceneId[] buildOrder = GameSceneCatalog.GetBuildOrder(); // 빌드 순서 배열 조회
            HashSet<string> sceneNames = new HashSet<string>(); // 중복 검사용 씬 이름 집합 생성

            foreach (GameSceneId sceneId in buildOrder) // 모든 씬 식별자 순회
            {
                string sceneName = GameSceneCatalog.GetSceneName(sceneId); // 씬 식별자에서 씬 이름 조회
                Assert.IsTrue(sceneNames.Add(sceneName), $"중복된 씬 이름이 존재합니다: {sceneName}"); // 씬 이름이 고유한지 검증
            }
        }

        [Test] // Unity Test Runner 테스트 지정
        public void BootstrapScenePathMatchesExpectedValue() // Bootstrap 씬 에셋 경로 일치 여부 검증
        {
            string scenePath = GameSceneCatalog.GetScenePath(GameSceneId.Bootstrap); // Bootstrap 씬 경로 조회
            Assert.AreEqual("Assets/_ProjectJ/Scenes/Game/Bootstrap.unity", scenePath); // 예상 경로와 실제 경로 비교
        }
    }
}
