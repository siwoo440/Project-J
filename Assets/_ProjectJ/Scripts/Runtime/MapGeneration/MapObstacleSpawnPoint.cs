using ProjectJ.Data; // 장애물 데이터 정의 참조
using UnityEngine; // Unity 컴포넌트와 기즈모 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    [DisallowMultipleComponent] // 장애물 배치 지점 중복 방지
    public sealed class MapObstacleSpawnPoint : MonoBehaviour // 모듈 내부 장애물 배치 지점 선언
    { // 장애물 배치 지점 묶음
        [SerializeField] private string pointId = "ObstaclePoint"; // 모듈 안의 배치 지점 ID
        [SerializeField, Min(0.01f)] private float maximumObstacleWidth = 1.2f; // 허용 장애물 최대 폭
        [SerializeField, Min(0.01f)] private float passageWidthBeforePlacement = 3f; // 배치 전 통로 유효 폭
        [SerializeField, Min(0.01f)] private float minimumRemainingPassageWidth = 1.1f; // 배치 뒤 보존할 최소 통로 폭
        [SerializeField, Range(0.1f, 3f)] private float riskMultiplier = 1f; // 지점별 위험도 배율
        [SerializeField] private bool enabledForGeneration = true; // 절차 배치 사용 여부
        [SerializeField] private bool drawGizmo = true; // Scene 지점 기즈모 표시 여부

        public string PointId => pointId; // 배치 지점 ID 반환
        public float MaximumObstacleWidth => maximumObstacleWidth; // 허용 장애물 최대 폭 반환
        public float PassageWidthBeforePlacement => passageWidthBeforePlacement; // 배치 전 통로 폭 반환
        public float MinimumRemainingPassageWidth => minimumRemainingPassageWidth; // 배치 뒤 최소 통로 폭 반환
        public float RiskMultiplier => riskMultiplier; // 지점 위험도 배율 반환
        public bool EnabledForGeneration => enabledForGeneration; // 절차 배치 사용 여부 반환

        private void OnValidate() // Inspector 배치 지점 값 보정
        { // 배치 지점 값 보정 처리
            pointId = string.IsNullOrWhiteSpace(pointId) ? gameObject.name : pointId.Trim(); // 빈 배치 지점 ID 자동 보완
            maximumObstacleWidth = Mathf.Max(0.01f, maximumObstacleWidth); // 허용 최대 폭 양수 보장
            passageWidthBeforePlacement = Mathf.Max(0.01f, passageWidthBeforePlacement); // 배치 전 통로 폭 양수 보장
            minimumRemainingPassageWidth = Mathf.Clamp(minimumRemainingPassageWidth, 0.01f, passageWidthBeforePlacement); // 최소 통로 폭 유효 범위 보장
            riskMultiplier = Mathf.Clamp(riskMultiplier, 0.1f, 3f); // 위험도 배율 범위 보장
        } // 배치 지점 값 보정 처리 종료

        public bool CanPlace(ObstacleDataDefinition obstacle, out string reason) // 지정 장애물의 안전한 배치 가능 여부 검사
        { // 장애물 배치 가능성 검사 처리
            if (!enabledForGeneration) // 절차 배치 비활성 확인
            { // 절차 배치 비활성 처리
                reason = $"{pointId} 지점은 절차 배치가 비활성화됐습니다."; // 비활성 이유 저장
                return false; // 장애물 배치 불가 반환
            } // 절차 배치 비활성 처리 종료

            if (obstacle == null) // 장애물 데이터 누락 확인
            { // 장애물 데이터 누락 처리
                reason = $"{pointId} 지점에 연결할 장애물 데이터가 없습니다."; // 데이터 누락 이유 저장
                return false; // 장애물 배치 불가 반환
            } // 장애물 데이터 누락 처리 종료

            if (!obstacle.TryValidateObstacle(out reason)) // 장애물 자체 데이터 검사
            { // 장애물 데이터 오류 처리
                return false; // 장애물 배치 불가 반환
            } // 장애물 데이터 오류 처리 종료

            if (obstacle.FootprintSize.x > maximumObstacleWidth + 0.001f) // 장애물 폭 초과 확인
            { // 장애물 폭 초과 처리
                reason = $"{obstacle.DataId} 폭 {obstacle.FootprintSize.x:0.00}m가 {pointId} 허용 폭 {maximumObstacleWidth:0.00}m를 초과합니다."; // 폭 초과 이유 저장
                return false; // 장애물 배치 불가 반환
            } // 장애물 폭 초과 처리 종료

            float remainingPassageWidth = CalculateRemainingPassageWidth(obstacle); // 장애물 배치 뒤 통로 폭 계산

            if (remainingPassageWidth + 0.001f < minimumRemainingPassageWidth) // 최소 통로 폭 미달 확인
            { // 최소 통로 폭 미달 처리
                reason = $"{pointId} 배치 뒤 통로 폭 {remainingPassageWidth:0.00}m가 최소 {minimumRemainingPassageWidth:0.00}m보다 좁습니다."; // 통로 폭 미달 이유 저장
                return false; // 장애물 배치 불가 반환
            } // 최소 통로 폭 미달 처리 종료

            reason = string.Empty; // 성공 이유 문자열 초기화
            return true; // 장애물 배치 가능 반환
        } // 장애물 배치 가능성 검사 처리 종료

        public float CalculateRemainingPassageWidth(ObstacleDataDefinition obstacle) // 장애물 배치 뒤 유효 통로 폭 계산
        { // 배치 뒤 통로 폭 계산 처리
            if (obstacle == null || !obstacle.BlocksPassage) // 통로 폭 차감 불필요 확인
            { // 통로 폭 유지 처리
                return passageWidthBeforePlacement; // 기존 통로 폭 반환
            } // 통로 폭 유지 처리 종료

            return Mathf.Max(0f, passageWidthBeforePlacement - obstacle.FootprintSize.x); // 장애물 폭 차감 결과 반환
        } // 배치 뒤 통로 폭 계산 처리 종료

        public int CalculateRiskScore(ObstacleDataDefinition obstacle) // 지점 배율이 적용된 위험도 계산
        { // 지점 위험도 계산 처리
            if (obstacle == null) // 장애물 데이터 누락 확인
            { // 장애물 데이터 누락 처리
                return 0; // 위험도 0 반환
            } // 장애물 데이터 누락 처리 종료

            return Mathf.Max(1, Mathf.RoundToInt(obstacle.RiskScore * riskMultiplier)); // 배율 적용 위험도 반환
        } // 지점 위험도 계산 처리 종료

        private void OnDrawGizmosSelected() // 선택 중 배치 가능 영역 표시
        { // 배치 가능 영역 표시 처리
            if (!drawGizmo) // 지점 기즈모 비활성 확인
            { // 지점 기즈모 비활성 처리
                return; // 기즈모 표시 생략
            } // 지점 기즈모 비활성 처리 종료

            Gizmos.color = enabledForGeneration ? new Color(1f, 0.75f, 0.15f, 0.9f) : new Color(0.45f, 0.45f, 0.45f, 0.7f); // 사용 여부별 지점 색상 적용
            Gizmos.DrawWireCube(transform.position, new Vector3(maximumObstacleWidth, 0.2f, maximumObstacleWidth)); // 허용 장애물 폭 영역 표시
        } // 배치 가능 영역 표시 처리 종료

#if UNITY_EDITOR // Unity Editor 전용 설정 시작
        public void ConfigureForEditor(string newPointId, float newMaximumObstacleWidth, float newPassageWidthBeforePlacement, float newMinimumRemainingPassageWidth, float newRiskMultiplier, bool newEnabledForGeneration) // Editor 도구용 배치 지점 설정
        { // Editor 배치 지점 설정 처리
            pointId = newPointId; // 새 배치 지점 ID 저장
            maximumObstacleWidth = newMaximumObstacleWidth; // 새 허용 장애물 폭 저장
            passageWidthBeforePlacement = newPassageWidthBeforePlacement; // 새 배치 전 통로 폭 저장
            minimumRemainingPassageWidth = newMinimumRemainingPassageWidth; // 새 최소 남은 통로 폭 저장
            riskMultiplier = newRiskMultiplier; // 새 위험도 배율 저장
            enabledForGeneration = newEnabledForGeneration; // 새 절차 배치 사용 여부 저장
            OnValidate(); // 배치 지점 설정값 즉시 보정
        } // Editor 배치 지점 설정 처리 종료
#endif // Unity Editor 전용 설정 종료
    } // 장애물 배치 지점 묶음 종료
} // 맵 생성 기능 묶음 종료
