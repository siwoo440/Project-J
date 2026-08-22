namespace ProjectJ.Networking.Fusion
{
    public enum ProjectJExternalForceSource // 네트워크 외력 원인
    {
        None = 0, // 외력 없음
        Push = 1, // 플레이어 밀치기
        AirBag = 2, // 에어백 장애물
        Item = 3, // 아이템 효과
        Obstacle = 4 // 기타 장애물 효과
    }

    public enum ProjectJNetworkPushResult // 밀치기 처리 결과
    {
        None = 0, // 시도 없음
        Success = 1, // 밀치기 성공
        Miss = 2, // 대상 없음
        Cooldown = 3, // 쿨타임 차단
        Invalid = 4, // 잘못된 상태
        Protected = 5, // 부활 보호로 차단
        Shielded = 6 // 젤리 보호막으로 차단
    }

    public enum ProjectJNetworkRespawnReason // 네트워크 부활 원인
    {
        None = 0, // 부활 없음
        Fall = 1, // 낙하 부활
        Manual = 2 // 직접 부활
    }
}
