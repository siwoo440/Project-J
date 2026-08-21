namespace ProjectJ.Items // 아이템 시스템 네임스페이스
{
    public enum ItemCategory // 아이템 역할 분류
    {
        Mobility, // 이동 보조
        Defense, // 방어
        Offensive, // 공격
        Trap, // 설치형 방해
        Utility // 기타 유틸리티
    }

    public enum ItemUseMode // 아이템 사용 방식
    {
        Instant, // 한 번 입력으로 즉시 사용
        Hold, // 입력을 유지하는 동안 사용
        Toggle, // 상태를 켜고 끄는 방식
        Place // 월드에 설치하는 방식
    }

    public enum ItemTargetType // 아이템 Target 종류
    {
        Self, // 자기 자신
        OtherPlayer, // 다른 플레이어
        Area, // 일정 범위
        WorldPosition // 월드 위치
    }
}
