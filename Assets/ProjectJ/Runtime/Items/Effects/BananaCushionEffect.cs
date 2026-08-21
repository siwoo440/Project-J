using UnityEngine;

namespace ProjectJ.Items.Effects
{
    public sealed class BananaCushionEffect :
        IItemUseEffect
    {
        private const float ForwardDistance = 1.5f;
        private const float RayStartHeight = 1.5f;
        private const float RayDistance = 4f;
        private const float MinimumGroundDot = 0.65f;

        public ItemUseResult TryUse(
            ItemUseContext context
        )
        {
            if (context.User == null)
            {
                return ItemUseResult.Fail(
                    ItemUseStatus.InvalidItem,
                    "바나나 쿠션 사용자를 찾을 수 없습니다."
                );
            }

            Transform userTransform =
                context.User.transform;

            Vector3 rayOrigin =
                userTransform.position +
                userTransform.forward * ForwardDistance +
                Vector3.up * RayStartHeight;

            if (
                !Physics.Raycast(
                    rayOrigin,
                    Vector3.down,
                    out RaycastHit hit,
                    RayDistance,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore
                )
            )
            {
                return ItemUseResult.Fail(
                    ItemUseStatus.InvalidPosition,
                    "바나나 쿠션을 설치할 바닥이 없습니다."
                );
            }

            if (
                Vector3.Dot(
                    hit.normal,
                    Vector3.up
                ) < MinimumGroundDot
            )
            {
                return ItemUseResult.Fail(
                    ItemUseStatus.InvalidPosition,
                    "경사가 너무 큰 위치에는 설치할 수 없습니다."
                );
            }

            GameObject bananaObject =
                new GameObject(
                    "Banana Cushion Runtime"
                );

            bananaObject.transform.position =
                hit.point +
                hit.normal * 0.08f;

            bananaObject.transform.rotation =
                Quaternion.FromToRotation(
                    Vector3.up,
                    hit.normal
                );

            SphereCollider trigger =
                bananaObject.AddComponent<
                    SphereCollider
                >();

            trigger.isTrigger = true;
            trigger.radius = 0.65f;

            BananaCushionRuntime runtime =
                bananaObject.AddComponent<
                    BananaCushionRuntime
                >();

            runtime.Initialize(
                context.User
            );

            CreateVisual(
                bananaObject.transform
            );

            return ItemUseResult.Success();
        }

        private static void CreateVisual(
            Transform parent
        )
        {
            GameObject visual =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder
                );

            visual.name = "Visual";
            visual.transform.SetParent(
                parent,
                false
            );

            visual.transform.localPosition =
                Vector3.zero;

            visual.transform.localScale =
                new Vector3(
                    0.85f,
                    0.08f,
                    0.85f
                );

            Collider visualCollider =
                visual.GetComponent<Collider>();

            if (visualCollider != null)
            {
                Object.Destroy(
                    visualCollider
                );
            }

            Renderer renderer =
                visual.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.material.color =
                    new Color(
                        1f,
                        0.82f,
                        0.08f,
                        1f
                    );
            }
        }
    }
}
