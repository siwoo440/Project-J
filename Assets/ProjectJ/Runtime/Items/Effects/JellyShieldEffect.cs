namespace ProjectJ.Items.Effects
{
    public sealed class JellyShieldEffect :
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
                    "젤리 보호막 사용 정보를 찾을 수 없습니다."
                );
            }

            JellyShieldState state =
                context.User.GetComponent<
                    JellyShieldState
                >();

            if (state == null)
            {
                state =
                    context.User.AddComponent<
                        JellyShieldState
                    >();
            }

            float duration =
                context.Definition.Duration > 0f
                    ? context.Definition.Duration
                    : 4f;

            state.Activate(duration);

            return ItemUseResult.Success();
        }
    }
}
