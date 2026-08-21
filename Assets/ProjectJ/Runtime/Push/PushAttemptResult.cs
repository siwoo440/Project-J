namespace ProjectJ.Push
{
    public enum PushAttemptResult
    {
        Success = 0,
        Miss = 1,
        Cooldown = 2,
        Protected = 3,
        InvalidState = 4,
        MissingReceiver = 5
    }
}
