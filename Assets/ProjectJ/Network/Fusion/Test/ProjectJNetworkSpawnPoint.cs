using UnityEngine; // Transform과 Gizmo 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJNetworkSpawnPoint :
        MonoBehaviour
    {
        [SerializeField]
        [Range(0, 7)]
        private int slotIndex; // 0~7 Player 시작 슬롯

        public int SlotIndex =>
            slotIndex; // 현재 시작 슬롯 번호

        public void ConfigureSlot(
            int index
        )
        {
            slotIndex =
                Mathf.Clamp(
                    index,
                    0,
                    7
                ); // Editor 자동 생성용 슬롯 설정
        }

        public static bool TryGetPose(
            int index,
            out Vector3 position,
            out Quaternion rotation
        )
        {
            ProjectJNetworkSpawnPoint[] points =
                Object.FindObjectsByType<
                    ProjectJNetworkSpawnPoint
                >(
                    FindObjectsSortMode.None
                );

            for (
                int pointIndex = 0;
                pointIndex < points.Length;
                pointIndex++
            )
            {
                ProjectJNetworkSpawnPoint point =
                    points[pointIndex];

                if (
                    point == null ||
                    point.slotIndex != index
                )
                {
                    continue;
                }

                position =
                    point.transform.position;

                rotation =
                    point.transform.rotation;

                return true;
            }

            position = Vector3.zero;
            rotation = Quaternion.identity;

            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(
                transform.position,
                0.45f
            ); // Scene에서 Spawn 위치 확인
        }
    }
}
