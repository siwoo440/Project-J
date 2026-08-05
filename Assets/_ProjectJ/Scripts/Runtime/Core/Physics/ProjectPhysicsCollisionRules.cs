namespace ProjectJ.Core.Physics // 프로젝트 물리 레이어 네임스페이스 선언
{
    public static class ProjectPhysicsCollisionRules // Project J 전용 레이어 충돌 규칙 관리 형식 선언
    {
        public static bool ShouldCollide(ProjectPhysicsLayer firstLayer, ProjectPhysicsLayer secondLayer) // 두 프로젝트 물리 레이어의 충돌 허용 여부 반환
        {
            ProjectPhysicsLayer lowerLayer = firstLayer <= secondLayer ? firstLayer : secondLayer; // 번호가 작은 레이어를 첫 비교 값으로 정렬
            ProjectPhysicsLayer higherLayer = firstLayer <= secondLayer ? secondLayer : firstLayer; // 번호가 큰 레이어를 두 번째 비교 값으로 정렬

            switch (lowerLayer) // 번호가 작은 레이어 기준 충돌 규칙 분기
            {
                case ProjectPhysicsLayer.Player: // 플레이어와 다른 레이어의 충돌 규칙 처리
                    return higherLayer != ProjectPhysicsLayer.RespawnProtection; // 부활 보호 레이어를 제외한 모든 전용 레이어와 충돌 허용

                case ProjectPhysicsLayer.Ground: // 지면과 번호가 큰 레이어의 충돌 규칙 처리
                    return higherLayer == ProjectPhysicsLayer.Ground // 지면끼리 충돌 허용
                        || higherLayer == ProjectPhysicsLayer.Obstacle // 지면과 장애물 충돌 허용
                        || higherLayer == ProjectPhysicsLayer.Interactable // 지면과 상호작용 오브젝트 충돌 허용
                        || higherLayer == ProjectPhysicsLayer.RespawnProtection; // 지면과 부활 보호 플레이어 충돌 허용

                case ProjectPhysicsLayer.Obstacle: // 장애물과 번호가 큰 레이어의 충돌 규칙 처리
                    return higherLayer == ProjectPhysicsLayer.Obstacle // 장애물끼리 충돌 허용
                        || higherLayer == ProjectPhysicsLayer.Interactable // 장애물과 상호작용 오브젝트 충돌 허용
                        || higherLayer == ProjectPhysicsLayer.RespawnProtection; // 장애물과 부활 보호 플레이어 충돌 허용

                case ProjectPhysicsLayer.Checkpoint: // 체크포인트와 번호가 큰 레이어의 충돌 규칙 처리
                    return higherLayer == ProjectPhysicsLayer.RespawnProtection; // 부활 보호 플레이어의 체크포인트 Trigger 진입 허용

                case ProjectPhysicsLayer.ItemBox: // 아이템 상자와 번호가 큰 레이어의 충돌 규칙 처리
                    return higherLayer == ProjectPhysicsLayer.RespawnProtection; // 부활 보호 플레이어의 아이템 상자 Trigger 진입 허용

                case ProjectPhysicsLayer.Interactable: // 상호작용 오브젝트와 번호가 큰 레이어의 충돌 규칙 처리
                    return higherLayer == ProjectPhysicsLayer.Interactable // 상호작용 오브젝트끼리 충돌 허용
                        || higherLayer == ProjectPhysicsLayer.RespawnProtection; // 부활 보호 플레이어와 상호작용 오브젝트 충돌 허용

                case ProjectPhysicsLayer.PushHitbox: // 밀치기 판정과 번호가 큰 레이어의 충돌 규칙 처리
                    return false; // 밀치기 판정끼리와 부활 보호 플레이어 충돌 차단

                case ProjectPhysicsLayer.RespawnProtection: // 부활 보호 레이어 자기 충돌 규칙 처리
                    return false; // 부활 보호 플레이어끼리 충돌 차단

                default: // 정의되지 않은 프로젝트 물리 레이어 처리
                    return false; // 알 수 없는 레이어 조합의 충돌 차단
            }
        }

        public static int GetCollisionMask(ProjectPhysicsLayer sourceLayer) // 지정 레이어와 충돌해야 하는 전용 레이어 마스크 반환
        {
            int collisionMask = 0; // 충돌 허용 레이어 마스크 초기화

            foreach (ProjectPhysicsLayer targetLayer in ProjectPhysicsLayers.All) // 모든 Project J 전용 레이어 순회
            {
                if (!ShouldCollide(sourceLayer, targetLayer)) // 현재 대상 레이어와 충돌하지 않는지 확인
                {
                    continue; // 충돌하지 않는 레이어를 마스크에서 제외
                }

                collisionMask |= ProjectPhysicsLayers.GetMask(targetLayer); // 충돌 허용 대상 레이어 비트를 마스크에 추가
            }

            return collisionMask; // 완성된 충돌 허용 레이어 마스크 반환
        }
    }
}
