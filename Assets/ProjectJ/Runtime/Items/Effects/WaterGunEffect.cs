namespace ProjectJ.Items.Effects
{
    public sealed class WaterGunEffect :
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
                    "물총 사용 정보를 찾을 수 없습니다."
                );
            }

            WaterGunRuntime runtime =
                context.User.GetComponent<
                    WaterGunRuntime
                >();

            if (runtime == null)
            {
                runtime =
                    context.User.AddComponent<
                        WaterGunRuntime
                    >();
            }

            runtime.Begin(
                context.Definition
            );

            return ItemUseResult.Success();
        }
    }
}
