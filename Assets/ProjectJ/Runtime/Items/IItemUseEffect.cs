namespace ProjectJ.Items // 아이템 시스템 네임스페이스
{
    public interface IItemUseEffect // 모든 실제 아이템 효과가 구현할 공통 인터페이스
    {
        ItemUseResult TryUse(ItemUseContext context); // 효과 실행 후 성공 또는 실패 반환
    }
}
