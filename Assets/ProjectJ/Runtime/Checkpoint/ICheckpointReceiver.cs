namespace ProjectJ.Checkpoint
{
    public interface ICheckpointReceiver // 체크포인트 수신 공통 계약
    {
        void ReceiveCheckpoint(global::ProjectJ.Checkpoint.Checkpoint checkpoint); // 체크포인트 접촉 전달
    }
}
