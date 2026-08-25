using System.Collections.Generic; // IReadOnlyList 사용
using UnityEngine; // MonoBehaviour와 Gizmos 사용

namespace ProjectJ.Items
{
    [DisallowMultipleComponent]
    public sealed class ProjectJHomingMissileRouteNode :
        MonoBehaviour
    {
        [SerializeField]
        private ProjectJHomingMissileRouteNode[] neighbours =
            new ProjectJHomingMissileRouteNode[0]; // 연결된 경로 노드

        public IReadOnlyList<ProjectJHomingMissileRouteNode> Neighbours =>
            neighbours;

        public bool ContainsNeighbour(
            ProjectJHomingMissileRouteNode node
        )
        {
            if (
                node == null ||
                neighbours == null
            )
            {
                return false;
            }

            for (
                int index = 0;
                index < neighbours.Length;
                index++
            )
            {
                if (neighbours[index] == node)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(
                transform.position,
                0.3f
            );

            if (neighbours == null)
            {
                return;
            }

            for (
                int index = 0;
                index < neighbours.Length;
                index++
            )
            {
                ProjectJHomingMissileRouteNode neighbour =
                    neighbours[index];

                if (neighbour == null)
                {
                    continue;
                }

                Gizmos.DrawLine(
                    transform.position,
                    neighbour.transform.position
                );
            }
        }
    }
}
