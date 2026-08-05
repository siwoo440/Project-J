namespace ProjectJ.Core.Physics // 프로젝트 물리 레이어 네임스페이스 선언
{
    public enum ProjectPhysicsLayer // Project J 전용 3D 물리 레이어 번호 선언
    {
        Player = 8, // 플레이어 본체 레이어 번호 선언
        Ground = 9, // 지면과 고정 발판 레이어 번호 선언
        Obstacle = 10, // 이동·회전·낙하 장애물 레이어 번호 선언
        Checkpoint = 11, // 체크포인트 Trigger 레이어 번호 선언
        ItemBox = 12, // 아이템 상자 Trigger 레이어 번호 선언
        Interactable = 13, // 상호작용 가능한 오브젝트 레이어 번호 선언
        PushHitbox = 14, // 밀치기 판정 Trigger 레이어 번호 선언
        RespawnProtection = 15 // 부활 보호 상태 플레이어 레이어 번호 선언
    }
}
