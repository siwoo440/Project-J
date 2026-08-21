using UnityEngine;

namespace ProjectJ.Items.Effects
{
    public sealed class SpringShoesEffect :
        IItemUseEffect
    {
        public ItemUseResult TryUse(
            ItemUseContext context
        )
        {
            if (
                context.User == null ||
                context.Definition == null
            )
            {
                return ItemUseResult.Fail(
                    ItemUseStatus.InvalidItem,
                    "스프링 신발 사용 정보를 찾을 수 없습니다."
                );
            }

            SpringShoesBuffState state =
                context.User.GetComponent<
                    SpringShoesBuffState
                >();

            if (state == null)
            {
                state =
                    context.User.AddComponent<
                        SpringShoesBuffState
                    >();
            }

            float duration =
                context.Definition.Duration > 0f
                    ? context.Definition.Duration
                    : 8f;

            state.Activate(duration);

            return ItemUseResult.Success();
        }
    }
}
