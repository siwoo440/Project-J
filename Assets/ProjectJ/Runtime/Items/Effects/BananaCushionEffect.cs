using ProjectJ.Items.Placement;
using UnityEngine;

namespace ProjectJ.Items.Effects
{
    public sealed class BananaCushionEffect :
        IItemUseEffect
    {
        private const float ForwardDistance =
            1.5f;

        private const float RayStartHeight =
            1.5f;

        private const float RayDistance =
            4f;

        private const float MinimumGroundDot =
            0.65f;

        private static readonly Vector3
            PlacementSize =
                new Vector3(
                    1.3f,
                    0.3f,
                    1.3f
                );

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
                userTransform.forward *
                ForwardDistance +
                Vector3.up *
                RayStartHeight;

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
                return
                    CreateInvalidPositionResult();
            }

            if (
                Vector3.Dot(
                    hit.normal,
                    Vector3.up
                ) <
                MinimumGroundDot
            )
            {
                return
                    CreateInvalidPositionResult();
            }

            Bounds placementBounds =
                new Bounds(
                    hit.point +
                    Vector3.up *
                    (
                        PlacementSize.y *
                        0.5f
                    ),
                    PlacementSize
                );

            if (
                !ItemPlacementValidator.CanPlace(
                    placementBounds
                )
            )
            {
                return
                    CreateInvalidPositionResult();
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

        private static ItemUseResult
            CreateInvalidPositionResult()
        {
            return ItemUseResult.Fail(
                ItemUseStatus.InvalidPosition,
                "해당 위치는 설치할 수 없습니다."
            );
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
