using UnityEngine; // MonoBehaviour와 Gizmos 사용

namespace ProjectJ.Items
{
    [DisallowMultipleComponent]
    public sealed class ProjectJCartRouteNode :
        MonoBehaviour
    {
        [SerializeField]
        private ProjectJCartRouteNode nextNode; // 다음 연결 노드

        public ProjectJCartRouteNode NextNode =>
            nextNode;

        public static ProjectJCartRouteNode FindNearest(Vector3 worldPosition)
        {
            ProjectJCartRouteNode[] nodes =
                Object.FindObjectsByType<ProjectJCartRouteNode>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            ProjectJCartRouteNode nearest = null;
            float nearestDistance = float.MaxValue;

            for (int index = 0; index < nodes.Length; index++)
            {
                ProjectJCartRouteNode node = nodes[index];

                if (node == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(
                    worldPosition,
                    node.transform.position
                );

                if (
                    !ProjectJCartPolicy.IsWithinStartNodeSearchRadius(distance) ||
                    distance >= nearestDistance
                )
                {
                    continue;
                }

                nearest = node;
                nearestDistance = distance;
            }

            return nearest;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, 0.25f);

            if (nextNode == null)
            {
                return;
            }

            Gizmos.DrawLine(
                transform.position,
                nextNode.transform.position
            );
        }
    }
}
