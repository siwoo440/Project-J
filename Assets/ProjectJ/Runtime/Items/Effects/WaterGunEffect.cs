namespace ProjectJ.Items.Effects
{
    public sealed class WaterGunEffect :
        IItemUseEffect
    {
        public ItemUseResult TryUse(
            ItemUseContext context
        )
        {
            if (context.User == null)
            {
                return ItemUseResult.Fail(
                    ItemUseStatus.InvalidItem,
                    "물총 사용자를 찾을 수 없습니다."
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

            runtime.Begin();

            return ItemUseResult.Success();
        }
    }
}
