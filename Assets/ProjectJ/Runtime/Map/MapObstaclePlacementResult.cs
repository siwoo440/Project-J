namespace ProjectJ.Map // 맵 시스템 네임스페이스
{
    public enum MapObstaclePlacementRejectReason // 장애물 배치 거부 사유
    {
        None = 0, // 배치 허용
        MissingModule = 1, // Module 누락
        MissingBounds = 2, // 장애물 Bounds 누락
        MissingSafeVolume = 3, // 설치 가능 영역 누락
        OutsideSafeVolume = 4, // 설치 가능 영역 이탈
        IntersectsNoSpawnVolume = 5 // 설치 금지 영역 침범
    }

    public readonly struct MapObstaclePlacementResult // 장애물 배치 검사 결과
    {
        public bool IsAllowed { get; } // 배치 허용 여부
        public MapObstaclePlacementRejectReason RejectReason { get; } // 배치 거부 사유
        public MapObstaclePlacementVolume RelatedVolume { get; } // 관련 영역 정보

        public MapObstaclePlacementResult(bool isAllowed, MapObstaclePlacementRejectReason rejectReason, MapObstaclePlacementVolume relatedVolume) // 결과 생성자
        {
            IsAllowed = isAllowed; // 허용 여부 저장
            RejectReason = rejectReason; // 거부 사유 저장
            RelatedVolume = relatedVolume; // 관련 영역 저장
        }

        public static MapObstaclePlacementResult Allowed() // 허용 결과 생성
        {
            return new MapObstaclePlacementResult(true, MapObstaclePlacementRejectReason.None, null); // 허용 결과 반환
        }

        public static MapObstaclePlacementResult Rejected(MapObstaclePlacementRejectReason reason, MapObstaclePlacementVolume volume = null) // 거부 결과 생성
        {
            return new MapObstaclePlacementResult(false, reason, volume); // 거부 결과 반환
        }
    }
}
