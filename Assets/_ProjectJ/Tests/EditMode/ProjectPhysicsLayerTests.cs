using System.Collections.Generic; // 레이어 번호 중복 검사 집합 기능 참조
using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Core.Physics; // Project J 물리 레이어와 충돌 규칙 참조
using UnityEngine; // LayerMask와 Unity 3D 물리 충돌 행렬 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{
    public sealed class ProjectPhysicsLayerTests // Project J 물리 레이어 번호와 충돌 규칙 검증 테스트 형식 선언
    {
        [Test] // Unity Test Runner 테스트 지정
        public void LayerIndicesAreUniqueAndUseUserLayerRange() // 전용 레이어 번호의 중복과 사용자 레이어 범위 검증
        {
            HashSet<int> usedIndices = new HashSet<int>(); // 이미 사용된 레이어 번호 집합 생성

            foreach (ProjectPhysicsLayer layer in ProjectPhysicsLayers.All) // 모든 Project J 전용 레이어 순회
            {
                int layerIndex = ProjectPhysicsLayers.GetIndex(layer); // 현재 Unity 레이어 번호 조회

                Assert.GreaterOrEqual(layerIndex, ProjectPhysicsLayers.MinimumUserLayerIndex); // 사용자 레이어 시작 번호 이상인지 검증
                Assert.LessOrEqual(layerIndex, ProjectPhysicsLayers.MaximumLayerIndex); // Unity 최대 레이어 번호 이하인지 검증
                Assert.IsTrue(usedIndices.Add(layerIndex), $"Layer {layerIndex}가 중복 선언되었습니다."); // 현재 레이어 번호 중복 여부 검증
            }

            Assert.AreEqual(8, usedIndices.Count); // Project J 전용 레이어가 정확히 8개인지 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void ConfiguredLayerNamesMatchExpectedIndices() // TagManager의 실제 레이어 이름과 고정 번호 일치 여부 검증
        {
            foreach (ProjectPhysicsLayer layer in ProjectPhysicsLayers.All) // 모든 Project J 전용 레이어 순회
            {
                int layerIndex = ProjectPhysicsLayers.GetIndex(layer); // 현재 Unity 레이어 번호 조회
                string expectedName = ProjectPhysicsLayers.GetName(layer); // 예상 Project J 레이어 이름 조회
                string actualName = LayerMask.LayerToName(layerIndex); // TagManager에 등록된 실제 레이어 이름 조회

                Assert.AreEqual(expectedName, actualName, $"Layer {layerIndex} 이름이 올바르지 않습니다."); // 실제 이름과 예상 이름 일치 여부 검증
            }
        }

        [Test] // Unity Test Runner 테스트 지정
        public void CollisionRulesAreSymmetric() // 모든 전용 레이어 충돌 규칙의 양방향 일치 여부 검증
        {
            foreach (ProjectPhysicsLayer firstLayer in ProjectPhysicsLayers.All) // 첫 번째 Project J 전용 레이어 순회
            {
                foreach (ProjectPhysicsLayer secondLayer in ProjectPhysicsLayers.All) // 두 번째 Project J 전용 레이어 순회
                {
                    bool forwardResult = ProjectPhysicsCollisionRules.ShouldCollide(firstLayer, secondLayer); // 첫 번째에서 두 번째 방향 충돌 규칙 조회
                    bool reverseResult = ProjectPhysicsCollisionRules.ShouldCollide(secondLayer, firstLayer); // 두 번째에서 첫 번째 방향 충돌 규칙 조회

                    Assert.AreEqual(forwardResult, reverseResult, $"{firstLayer}와 {secondLayer}의 충돌 규칙이 대칭이 아닙니다."); // 양방향 충돌 규칙 일치 여부 검증
                }
            }
        }

        [Test] // Unity Test Runner 테스트 지정
        public void PhysicsMatrixMatchesProjectCollisionRules() // Unity 3D 물리 충돌 행렬과 코드 규칙 일치 여부 검증
        {
            IReadOnlyList<ProjectPhysicsLayer> layers = ProjectPhysicsLayers.All; // Project J 전용 레이어 전체 목록 조회

            for (int firstIndex = 0; firstIndex < layers.Count; firstIndex++) // 첫 번째 충돌 레이어 순회
            {
                for (int secondIndex = firstIndex; secondIndex < layers.Count; secondIndex++) // 중복 조합을 제외한 두 번째 충돌 레이어 순회
                {
                    ProjectPhysicsLayer firstLayer = layers[firstIndex]; // 첫 번째 프로젝트 물리 레이어 조회
                    ProjectPhysicsLayer secondLayer = layers[secondIndex]; // 두 번째 프로젝트 물리 레이어 조회
                    bool expectedCollision = ProjectPhysicsCollisionRules.ShouldCollide(firstLayer, secondLayer); // 코드에 정의된 예상 충돌 여부 조회
                    bool actualCollision = !UnityEngine.Physics.GetIgnoreLayerCollision( // Unity 3D 물리 충돌 행렬의 실제 충돌 여부 조회
                        ProjectPhysicsLayers.GetIndex(firstLayer), // 첫 번째 Unity 레이어 번호 전달
                        ProjectPhysicsLayers.GetIndex(secondLayer)); // 두 번째 Unity 레이어 번호 전달

                    Assert.AreEqual(expectedCollision, actualCollision, $"{firstLayer}와 {secondLayer}의 Physics Matrix 설정이 다릅니다."); // 실제 충돌 행렬과 코드 규칙 일치 여부 검증
                }
            }
        }

        [Test] // Unity Test Runner 테스트 지정
        public void PlayerCollidesWithWorldProgressAndPushHitbox() // 일반 플레이어의 핵심 충돌 조합 검증
        {
            Assert.IsTrue(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.Player, ProjectPhysicsLayer.Player)); // 일반 플레이어끼리 몸 충돌 허용 여부 검증
            Assert.IsTrue(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.Player, ProjectPhysicsLayer.Ground)); // 플레이어와 지면 충돌 허용 여부 검증
            Assert.IsTrue(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.Player, ProjectPhysicsLayer.Obstacle)); // 플레이어와 장애물 충돌 허용 여부 검증
            Assert.IsTrue(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.Player, ProjectPhysicsLayer.Checkpoint)); // 플레이어와 체크포인트 Trigger 판정 허용 여부 검증
            Assert.IsTrue(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.Player, ProjectPhysicsLayer.ItemBox)); // 플레이어와 아이템 상자 Trigger 판정 허용 여부 검증
            Assert.IsTrue(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.Player, ProjectPhysicsLayer.Interactable)); // 플레이어와 상호작용 오브젝트 판정 허용 여부 검증
            Assert.IsTrue(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.Player, ProjectPhysicsLayer.PushHitbox)); // 플레이어와 밀치기 판정 Trigger 허용 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void TriggerLayersIgnoreUnrelatedWorldPairs() // 진행 Trigger와 월드 오브젝트 사이 불필요한 충돌 차단 여부 검증
        {
            Assert.IsFalse(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.Checkpoint, ProjectPhysicsLayer.Ground)); // 체크포인트와 지면 충돌 차단 여부 검증
            Assert.IsFalse(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.Checkpoint, ProjectPhysicsLayer.Obstacle)); // 체크포인트와 장애물 충돌 차단 여부 검증
            Assert.IsFalse(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.ItemBox, ProjectPhysicsLayer.Ground)); // 아이템 상자와 지면 충돌 차단 여부 검증
            Assert.IsFalse(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.ItemBox, ProjectPhysicsLayer.Obstacle)); // 아이템 상자와 장애물 충돌 차단 여부 검증
            Assert.IsFalse(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.Checkpoint, ProjectPhysicsLayer.ItemBox)); // 체크포인트와 아이템 상자 상호 충돌 차단 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void RespawnProtectionIgnoresPlayersAndPushHitboxesButKeepsWorld() // 부활 보호 상태의 충돌 보호와 월드 진행 유지 여부 검증
        {
            Assert.IsFalse(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.RespawnProtection, ProjectPhysicsLayer.Player)); // 부활 보호 플레이어와 일반 플레이어 몸 충돌 차단 여부 검증
            Assert.IsFalse(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.RespawnProtection, ProjectPhysicsLayer.PushHitbox)); // 부활 보호 플레이어의 밀치기 판정 차단 여부 검증
            Assert.IsFalse(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.RespawnProtection, ProjectPhysicsLayer.RespawnProtection)); // 부활 보호 플레이어끼리 몸 충돌 차단 여부 검증
            Assert.IsTrue(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.RespawnProtection, ProjectPhysicsLayer.Ground)); // 부활 보호 플레이어와 지면 충돌 유지 여부 검증
            Assert.IsTrue(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.RespawnProtection, ProjectPhysicsLayer.Obstacle)); // 부활 보호 플레이어와 장애물 충돌 유지 여부 검증
            Assert.IsTrue(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.RespawnProtection, ProjectPhysicsLayer.Checkpoint)); // 부활 보호 플레이어의 체크포인트 판정 유지 여부 검증
            Assert.IsTrue(ProjectPhysicsCollisionRules.ShouldCollide(ProjectPhysicsLayer.RespawnProtection, ProjectPhysicsLayer.ItemBox)); // 부활 보호 플레이어의 아이템 상자 판정 유지 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void CommonLayerMasksContainExpectedLayers() // 공통 레이어 마스크 구성값 검증
        {
            Assert.AreEqual( // 월드 마스크의 지면·장애물·상호작용 레이어 구성 검증
                ProjectPhysicsLayerMasks.Ground | ProjectPhysicsLayerMasks.Obstacle | ProjectPhysicsLayerMasks.Interactable, // 예상 월드 레이어 마스크 생성
                ProjectPhysicsLayerMasks.World); // 실제 월드 레이어 마스크 비교

            Assert.AreEqual( // 진행 Trigger 마스크의 체크포인트·아이템 상자 구성 검증
                ProjectPhysicsLayerMasks.Checkpoint | ProjectPhysicsLayerMasks.ItemBox, // 예상 진행 Trigger 레이어 마스크 생성
                ProjectPhysicsLayerMasks.ProgressTriggers); // 실제 진행 Trigger 레이어 마스크 비교

            Assert.AreEqual(ProjectPhysicsLayerMasks.Player, ProjectPhysicsLayerMasks.PushTargets); // 밀치기 대상 마스크가 일반 플레이어만 포함하는지 검증
        }
    }
}
