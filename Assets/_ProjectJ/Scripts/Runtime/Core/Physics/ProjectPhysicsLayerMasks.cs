namespace ProjectJ.Core.Physics // 프로젝트 물리 레이어 네임스페이스 선언
{
    public static class ProjectPhysicsLayerMasks // 자주 사용하는 Project J 물리 레이어 마스크 관리 형식 선언
    {
        public static int Player => ProjectPhysicsLayers.GetMask(ProjectPhysicsLayer.Player); // 플레이어 단일 레이어 마스크 반환
        public static int Ground => ProjectPhysicsLayers.GetMask(ProjectPhysicsLayer.Ground); // 지면 단일 레이어 마스크 반환
        public static int Obstacle => ProjectPhysicsLayers.GetMask(ProjectPhysicsLayer.Obstacle); // 장애물 단일 레이어 마스크 반환
        public static int Checkpoint => ProjectPhysicsLayers.GetMask(ProjectPhysicsLayer.Checkpoint); // 체크포인트 단일 레이어 마스크 반환
        public static int ItemBox => ProjectPhysicsLayers.GetMask(ProjectPhysicsLayer.ItemBox); // 아이템 상자 단일 레이어 마스크 반환
        public static int Interactable => ProjectPhysicsLayers.GetMask(ProjectPhysicsLayer.Interactable); // 상호작용 오브젝트 단일 레이어 마스크 반환
        public static int PushHitbox => ProjectPhysicsLayers.GetMask(ProjectPhysicsLayer.PushHitbox); // 밀치기 판정 단일 레이어 마스크 반환
        public static int RespawnProtection => ProjectPhysicsLayers.GetMask(ProjectPhysicsLayer.RespawnProtection); // 부활 보호 단일 레이어 마스크 반환

        public static int World => Ground | Obstacle | Interactable; // 플레이어 이동과 지면 검사에 사용할 월드 레이어 마스크 반환
        public static int ProgressTriggers => Checkpoint | ItemBox; // 진행과 보상 Trigger 검사에 사용할 레이어 마스크 반환
        public static int InteractionTargets => Interactable | ItemBox; // 상호작용 대상 검색에 사용할 레이어 마스크 반환
        public static int PushTargets => Player; // 밀치기 판정이 영향을 줄 수 있는 플레이어 마스크 반환
        public static int AllProjectLayers => Player | Ground | Obstacle | Checkpoint | ItemBox | Interactable | PushHitbox | RespawnProtection; // 모든 Project J 전용 레이어 마스크 반환
    }
}
