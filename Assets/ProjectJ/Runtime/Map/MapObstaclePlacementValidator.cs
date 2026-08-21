using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Map // 맵 시스템 네임스페이스
{
    public static class MapObstaclePlacementValidator // 장애물 배치 검증 도구
    {
        public static MapObstaclePlacementResult Validate(MapModule module, GameObject obstacleRoot) // GameObject 기준 배치 검사
        {
            if (module == null) // Module 누락 검사
            {
                return MapObstaclePlacementResult.Rejected(MapObstaclePlacementRejectReason.MissingModule); // Module 누락 결과 반환
            }

            if (!TryCalculateObjectBounds(obstacleRoot, out Bounds obstacleBounds)) // 장애물 Bounds 계산 검사
            {
                return MapObstaclePlacementResult.Rejected(MapObstaclePlacementRejectReason.MissingBounds); // Bounds 누락 결과 반환
            }

            return Validate(module, obstacleBounds); // Bounds 기준 검사 실행
        }

        public static MapObstaclePlacementResult Validate(MapModule module, Bounds obstacleBounds) // Bounds 기준 배치 검사
        {
            if (module == null) // Module 누락 검사
            {
                return MapObstaclePlacementResult.Rejected(MapObstaclePlacementRejectReason.MissingModule); // Module 누락 결과 반환
            }

            MapObstaclePlacementVolume[] volumes = module.GetComponentsInChildren<MapObstaclePlacementVolume>(true); // Module 배치 영역 수집
            MapObstaclePlacementVolume containingSafeVolume = null; // 포함된 설치 가능 영역 저장
            bool hasSafeVolume = false; // 설치 가능 영역 존재 여부

            for (int i = 0; i < volumes.Length; i++) // 모든 영역 반복
            {
                MapObstaclePlacementVolume volume = volumes[i]; // 현재 영역 저장

                if (volume == null || volume.VolumeType != MapObstacleVolumeType.Safe) // 설치 가능 영역 여부 검사
                {
                    continue; // 다음 영역 진행
                }

                hasSafeVolume = true; // 설치 가능 영역 존재 기록

                if (volume.ContainsBounds(obstacleBounds)) // 후보 전체 포함 검사
                {
                    containingSafeVolume = volume; // 포함 영역 저장
                    break; // 설치 가능 영역 검색 종료
                }
            }

            if (!hasSafeVolume) // 설치 가능 영역 누락 검사
            {
                return MapObstaclePlacementResult.Rejected(MapObstaclePlacementRejectReason.MissingSafeVolume); // 영역 누락 결과 반환
            }

            if (containingSafeVolume == null) // 설치 가능 영역 이탈 검사
            {
                return MapObstaclePlacementResult.Rejected(MapObstaclePlacementRejectReason.OutsideSafeVolume); // 영역 이탈 결과 반환
            }

            for (int i = 0; i < volumes.Length; i++) // 모든 영역 반복
            {
                MapObstaclePlacementVolume volume = volumes[i]; // 현재 영역 저장

                if (volume == null || volume.VolumeType != MapObstacleVolumeType.NoSpawn) // 설치 금지 영역 여부 검사
                {
                    continue; // 다음 영역 진행
                }

                if (volume.IntersectsBounds(obstacleBounds)) // 금지 영역 침범 검사
                {
                    return MapObstaclePlacementResult.Rejected(MapObstaclePlacementRejectReason.IntersectsNoSpawnVolume, volume); // 금지 영역 결과 반환
                }
            }

            return MapObstaclePlacementResult.Allowed(); // 최종 허용 결과 반환
        }

        public static bool TryCalculateObjectBounds(GameObject obstacleRoot, out Bounds bounds) // 장애물 전체 Bounds 계산
        {
            bounds = default; // 기본 Bounds 초기화

            if (obstacleRoot == null) // 장애물 누락 검사
            {
                return false; // 계산 실패 반환
            }

            Collider[] colliders = obstacleRoot.GetComponentsInChildren<Collider>(true); // 하위 Collider 수집
            bool hasBounds = false; // Bounds 존재 여부

            for (int i = 0; i < colliders.Length; i++) // Collider 반복
            {
                Collider currentCollider = colliders[i]; // 현재 Collider 저장

                if (currentCollider == null || !currentCollider.enabled) // 유효 Collider 검사
                {
                    continue; // 다음 Collider 진행
                }

                if (!hasBounds) // 첫 Bounds 검사
                {
                    bounds = currentCollider.bounds; // 첫 Bounds 저장
                    hasBounds = true; // Bounds 존재 기록
                    continue; // 다음 Collider 진행
                }

                bounds.Encapsulate(currentCollider.bounds); // Collider Bounds 합치기
            }

            if (hasBounds) // Collider Bounds 존재 검사
            {
                return true; // 계산 성공 반환
            }

            Renderer[] renderers = obstacleRoot.GetComponentsInChildren<Renderer>(true); // 하위 Renderer 수집

            for (int i = 0; i < renderers.Length; i++) // Renderer 반복
            {
                Renderer currentRenderer = renderers[i]; // 현재 Renderer 저장

                if (currentRenderer == null || !currentRenderer.enabled) // 유효 Renderer 검사
                {
                    continue; // 다음 Renderer 진행
                }

                if (!hasBounds) // 첫 Bounds 검사
                {
                    bounds = currentRenderer.bounds; // 첫 Bounds 저장
                    hasBounds = true; // Bounds 존재 기록
                    continue; // 다음 Renderer 진행
                }

                bounds.Encapsulate(currentRenderer.bounds); // Renderer Bounds 합치기
            }

            return hasBounds; // 최종 계산 결과 반환
        }
    }
}
