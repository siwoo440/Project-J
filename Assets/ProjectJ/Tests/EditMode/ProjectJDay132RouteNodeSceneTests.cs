using System.Collections.Generic; // BFS 검증 컬렉션 사용
using NUnit.Framework; // EditMode 테스트 사용
using ProjectJ.Items; // Route Node 타입 사용
using UnityEditor.SceneManagement; // Game Scene 로드 사용
using UnityEngine; // GameObject와 Vector3 사용
using UnityEngine.SceneManagement; // Scene 조회 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJDay132RouteNodeSceneTests
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        private const string RouteRootName =
            "=== ROUTE NODES ===";

        private const int MinimumExpectedNodeCount =
            60;

        private const float MaximumExpectedEdgeDistance =
            8.25f;

        private Scene gameScene;
        private bool openedByTest;

        [SetUp]
        public void SetUp()
        {
            gameScene =
                SceneManager.GetSceneByPath(
                    GameScenePath
                );

            if (
                !gameScene.IsValid() ||
                !gameScene.isLoaded
            )
            {
                gameScene =
                    EditorSceneManager.OpenScene(
                        GameScenePath,
                        OpenSceneMode.Additive
                    );

                openedByTest =
                    true;
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (
                openedByTest &&
                gameScene.IsValid() &&
                gameScene.isLoaded
            )
            {
                EditorSceneManager.CloseScene(
                    gameScene,
                    true
                );
            }

            openedByTest =
                false;
        }

        [Test]
        public void GameScene_HasDedicatedRouteNodeRootAndEnoughNodes()
        {
            GameObject root =
                FindRootByName(
                    gameScene,
                    RouteRootName
                );

            Assert.IsNotNull(
                root,
                RouteRootName
            );

            ProjectJHomingMissileRouteNode[] nodes =
                root.GetComponentsInChildren<ProjectJHomingMissileRouteNode>(
                    true
                );

            Assert.GreaterOrEqual(
                nodes.Length,
                MinimumExpectedNodeCount
            );
        }

        [Test]
        public void RouteNodes_DoNotContainFusionNetworkObjects()
        {
            ProjectJHomingMissileRouteNode[] nodes =
                CollectRouteNodes();

            for (
                int nodeIndex = 0;
                nodeIndex < nodes.Length;
                nodeIndex++
            )
            {
                Component[] components =
                    nodes[nodeIndex].GetComponents<Component>();

                for (
                    int componentIndex = 0;
                    componentIndex < components.Length;
                    componentIndex++
                )
                {
                    Component component =
                        components[componentIndex];

                    if (component == null)
                    {
                        continue;
                    }

                    Assert.AreNotEqual(
                        "Fusion.NetworkObject",
                        component.GetType().FullName,
                        nodes[nodeIndex].name
                    );
                }
            }
        }

        [Test]
        public void RouteGraph_IsFullyConnectedAndSymmetric()
        {
            ProjectJHomingMissileRouteNode[] nodes =
                CollectRouteNodes();

            Assert.Greater(
                nodes.Length,
                0
            );

            HashSet<ProjectJHomingMissileRouteNode> visited =
                new HashSet<ProjectJHomingMissileRouteNode>();

            Queue<ProjectJHomingMissileRouteNode> queue =
                new Queue<ProjectJHomingMissileRouteNode>();

            visited.Add(
                nodes[0]
            );

            queue.Enqueue(
                nodes[0]
            );

            while (queue.Count > 0)
            {
                ProjectJHomingMissileRouteNode current =
                    queue.Dequeue();

                Assert.IsNotNull(
                    current.Neighbours,
                    current.name
                );

                Assert.Greater(
                    current.Neighbours.Count,
                    0,
                    current.name
                );

                for (
                    int index = 0;
                    index < current.Neighbours.Count;
                    index++
                )
                {
                    ProjectJHomingMissileRouteNode neighbour =
                        current.Neighbours[index];

                    Assert.IsNotNull(
                        neighbour,
                        current.name
                    );

                    Assert.IsTrue(
                        neighbour.ContainsNeighbour(
                            current
                        ),
                        current.name +
                        " <-> " +
                        neighbour.name
                    );

                    float distance =
                        Vector3.Distance(
                            current.transform.position,
                            neighbour.transform.position
                        );

                    Assert.LessOrEqual(
                        distance,
                        MaximumExpectedEdgeDistance,
                        current.name +
                        " -> " +
                        neighbour.name
                    );

                    if (
                        visited.Add(
                            neighbour
                        )
                    )
                    {
                        queue.Enqueue(
                            neighbour
                        );
                    }
                }
            }

            Assert.AreEqual(
                nodes.Length,
                visited.Count
            );
        }

        [Test]
        public void RouteNodes_AreClearOfSolidColliders()
        {
            ProjectJHomingMissileRouteNode[] nodes =
                CollectRouteNodes();

            Physics.SyncTransforms();

            for (
                int index = 0;
                index < nodes.Length;
                index++
            )
            {
                Collider[] overlaps =
                    Physics.OverlapSphere(
                        nodes[index].transform.position,
                        0.42f,
                        Physics.AllLayers,
                        QueryTriggerInteraction.Ignore
                    );

                Assert.AreEqual(
                    0,
                    overlaps.Length,
                    nodes[index].name
                );
            }
        }

        private ProjectJHomingMissileRouteNode[] CollectRouteNodes()
        {
            GameObject root =
                FindRootByName(
                    gameScene,
                    RouteRootName
                );

            Assert.IsNotNull(
                root,
                RouteRootName
            );

            return
                root.GetComponentsInChildren<ProjectJHomingMissileRouteNode>(
                    true
                );
        }

        private static GameObject FindRootByName(
            Scene scene,
            string objectName
        )
        {
            if (
                !scene.IsValid() ||
                !scene.isLoaded
            )
            {
                return null;
            }

            GameObject[] roots =
                scene.GetRootGameObjects();

            for (
                int index = 0;
                index < roots.Length;
                index++
            )
            {
                GameObject root =
                    roots[index];

                if (
                    root != null &&
                    root.name ==
                    objectName
                )
                {
                    return root;
                }
            }

            return null;
        }
    }
}
