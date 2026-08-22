namespace ProjectJ.Networking.Fusion
{
    public enum ProjectJNetworkMatchState // 네트워크 경기 상태
    {
        Preparing = 0, // 참가자 대기
        Countdown = 1, // 3초 시작 카운트다운
        Playing = 2, // 경기 진행
        Finished = 3 // 경기 종료
    }

    public enum ProjectJNetworkMatchEndReason // 경기 종료 원인
    {
        None = 0, // 종료 전
        AllFinished = 1, // 모든 참가자 정상 도달
        TimeExpired = 2 // 제한 시간 종료
    }
}
