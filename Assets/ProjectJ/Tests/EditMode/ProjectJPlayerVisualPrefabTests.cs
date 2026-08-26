using NUnit.Framework; // EditMode 테스트 기능
using ProjectJ.Player; // Player Visual Controller 사용
using UnityEditor; // Prefab 자산 로드 사용
using UnityEngine; // GameObject와 Transform 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{ // 네임스페이스 시작
    public sealed class ProjectJPlayerVisualPrefabTests // Network Player Visual 연결 테스트
    { // 테스트 클래스 시작
        private const string NetworkPlayerPrefabPath = // 실제 Spawn Prefab 경로
            "Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkPlayer.prefab"; // Resources Player 경로
        private const string NetworkBotPrefabPath = // 실제 AI Bot Spawn Prefab 경로
            "Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkBot.prefab"; // Resources Bot 경로

        [Test] // 실제 Fusion Player 연결 사례
        public void NetworkPlayerPrefab_UsesChefVisualInsteadOfLegacyCapsule() // 잘못된 Player Prefab 수정 오류 방지
        { // 테스트 시작
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>( // 실제 Network Player 로드
                NetworkPlayerPrefabPath // Resources Player 경로
            ); // Prefab 로드 종료
            Assert.IsNotNull(prefab); // Network Player Prefab 존재 확인

            ProjectJPlayerVisualController controller = // Visual Controller 조회
                prefab.GetComponent<ProjectJPlayerVisualController>(); // Root Controller 검색
            Transform visualRoot = prefab.transform.Find("VisualRoot"); // 새 Visual Root 검색
            Transform legacyVisual = prefab.transform.Find("Visual"); // 기존 캡슐 Visual 검색

            Assert.IsNotNull(controller); // 실제 Spawn Prefab Controller 확인
            Assert.IsNotNull(visualRoot); // 실제 Spawn Prefab VisualRoot 확인
            Assert.AreEqual( // 검은색 요리사 기본값 확인
                ProjectJPlayerVisualController.ChefVisualName, // 기대 기본 이름
                controller.DefaultVisualName // 실제 기본 이름
            ); // 기본값 비교 종료
            Assert.IsNull(legacyVisual); // 기존 캡슐 Visual 제거 확인
        } // 테스트 종료

        [Test] // 실제 Fusion AI Bot 연결 사례
        public void NetworkBotPrefab_UsesChefVisualInsteadOfLegacyCapsule() // AI Bot 캡슐 잔존 오류 방지
        { // 테스트 시작
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>( // 실제 Network Bot 로드
                NetworkBotPrefabPath // Resources Bot 경로
            ); // Prefab 로드 종료
            Assert.IsNotNull(prefab); // Network Bot Prefab 존재 확인

            ProjectJPlayerVisualController controller = // Visual Controller 조회
                prefab.GetComponent<ProjectJPlayerVisualController>(); // Root Controller 검색
            Transform visualRoot = prefab.transform.Find("VisualRoot"); // 새 Visual Root 검색
            Transform legacyVisual = prefab.transform.Find("Visual"); // 기존 캡슐 Visual 검색

            Assert.IsNotNull(controller); // 실제 Bot Spawn Prefab Controller 확인
            Assert.IsNotNull(visualRoot); // 실제 Bot Spawn Prefab VisualRoot 확인
            Assert.AreEqual( // 검은색 요리사 기본값 확인
                ProjectJPlayerVisualController.ChefVisualName, // 기대 기본 이름
                controller.DefaultVisualName // 실제 기본 이름
            ); // 기본값 비교 종료
            Assert.IsNull(legacyVisual); // 기존 AI Bot 캡슐 Visual 제거 확인
        } // 테스트 종료
    } // 테스트 클래스 종료
} // 네임스페이스 종료
