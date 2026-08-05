using ProjectJ.Gameplay; // 경기 관리자 기능 참조
using UnityEngine; // Unity 화면 UI 기능 참조

namespace ProjectJ.UI // 사용자 인터페이스 네임스페이스
{
    [DisallowMultipleComponent] // HUD 컴포넌트 중복 방지
    public sealed class MinimalPlayerHud : MonoBehaviour // 플레이어 최소 HUD 컴포넌트
    {
        [SerializeField] private ProjectJ.Player.PlayerRespawnController respawnController; // 높이와 체크포인트 정보 제공자
        [SerializeField] private ProjectJ.Player.PlayerMovementController movementController; // 스태미나 정보 제공자
        [SerializeField] private PrototypeMatchController matchController; // 경기 시간과 순위 제공자
        [SerializeField] private Vector2 panelPosition = new Vector2(20f, 20f); // HUD 패널 화면 위치
        [SerializeField, Min(200f)] private float panelWidth = 300f; // HUD 패널 너비
        [SerializeField] private Color staminaColor = new Color(0.2f, 0.8f, 0.35f, 1f); // 스태미나 막대 색상
        [SerializeField] private Color staminaBackgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.9f); // 스태미나 배경 색상

        private GUIStyle titleStyle; // HUD 제목 스타일
        private GUIStyle labelStyle; // HUD 정보 스타일
        private GUIStyle noticeStyle; // 부활 안내 스타일

        private void Awake() // HUD 필수 참조 검증
        {
            if (respawnController == null || movementController == null || matchController == null) // HUD 참조 누락 확인
            {
                Debug.LogError("[ProjectJ][UI][PLAYER_HUD_SOURCE_MISSING] 부활, 이동, 경기 관리자 연결을 확인합니다.", this); // HUD 참조 누락 오류
                enabled = false; // HUD 표시 비활성화
            }
        }

        private void OnGUI() // 최소 HUD 화면 출력
        {
            PrepareStyles(); // GUI 스타일 준비

            Rect panelRect = new Rect(panelPosition.x, panelPosition.y, panelWidth, 176f); // HUD 패널 영역 계산
            GUI.Box(panelRect, GUIContent.none); // HUD 패널 배경 출력

            Rect titleRect = new Rect(panelRect.x + 12f, panelRect.y + 8f, panelRect.width - 24f, 24f); // 제목 영역 계산
            GUI.Label(titleRect, "PROJECT J", titleStyle); // HUD 제목 출력

            Rect heightRect = new Rect(panelRect.x + 12f, panelRect.y + 36f, panelRect.width - 24f, 22f); // 높이 영역 계산
            GUI.Label(heightRect, $"높이 {respawnController.CurrentHeight:0.0} m  |  최고 {respawnController.HighestHeight:0.0} m", labelStyle); // 높이 정보 출력

            Rect checkpointRect = new Rect(panelRect.x + 12f, panelRect.y + 62f, panelRect.width - 24f, 22f); // 체크포인트 영역 계산
            GUI.Label(checkpointRect, $"체크포인트 : {respawnController.CurrentCheckpointId}", labelStyle); // 체크포인트 정보 출력

            int remainingTotalSeconds = Mathf.CeilToInt(matchController.RemainingTime); // 남은 전체 초 계산
            int remainingMinutes = remainingTotalSeconds / 60; // 남은 분 계산
            int remainingSeconds = remainingTotalSeconds % 60; // 남은 초 계산
            int displayedRank = matchController.IsMatchFinished ? matchController.FinalPlayerRank : matchController.PlayerRank; // 표시할 순위 선택

            Rect matchRect = new Rect(panelRect.x + 12f, panelRect.y + 88f, panelRect.width - 24f, 22f); // 경기 정보 영역 계산
            GUI.Label(matchRect, $"남은 시간 {remainingMinutes:00}:{remainingSeconds:00}  |  순위 {displayedRank}/{matchController.ParticipantCount}", labelStyle); // 시간과 순위 출력

            Rect staminaLabelRect = new Rect(panelRect.x + 12f, panelRect.y + 114f, panelRect.width - 24f, 22f); // 스태미나 글자 영역 계산
            int staminaPercent = Mathf.RoundToInt(movementController.StaminaNormalized * 100f); // 스태미나 백분율 계산
            GUI.Label(staminaLabelRect, $"스태미나 : {staminaPercent}%", labelStyle); // 스태미나 수치 출력

            Rect staminaBarRect = new Rect(panelRect.x + 12f, panelRect.y + 144f, panelRect.width - 24f, 18f); // 스태미나 막대 영역 계산
            DrawStaminaBar(staminaBarRect, movementController.StaminaNormalized); // 스태미나 막대 출력

            if (respawnController.IsRespawning) // 부활 진행 상태 확인
            {
                Rect noticeRect = new Rect(Screen.width * 0.5f - 180f, Screen.height * 0.5f - 35f, 360f, 70f); // 부활 안내 영역 계산
                GUI.Box(noticeRect, "추락\n체크포인트로 복귀 중", noticeStyle); // 부활 안내 출력
            }

            if (matchController.IsMatchFinished) // 경기 종료 상태 확인
            {
                DrawFinalResult(); // 최종 결과 화면 출력
            }
        }

        private void DrawFinalResult() // 최종 경기 결과 출력
        {
            int participantCount = matchController.ParticipantCount; // 전체 참가자 수 조회
            float resultHeight = 90f + participantCount * 24f; // 참가자 수 기반 결과창 높이 계산
            Rect resultRect = new Rect(Screen.width * 0.5f - 210f, Screen.height * 0.5f - resultHeight * 0.5f, 420f, resultHeight); // 결과창 영역 계산
            GUI.Box(resultRect, GUIContent.none); // 결과창 배경 출력

            Rect titleRect = new Rect(resultRect.x + 20f, resultRect.y + 12f, resultRect.width - 40f, 26f); // 결과 제목 영역 계산
            GUI.Label(titleRect, "경기 종료", titleStyle); // 경기 종료 제목 출력

            Rect rankRect = new Rect(resultRect.x + 20f, resultRect.y + 42f, resultRect.width - 40f, 24f); // 최종 순위 영역 계산
            GUI.Label(rankRect, $"최종 순위 : {matchController.FinalPlayerRank}/{participantCount}", labelStyle); // 플레이어 최종 순위 출력

            for (int index = 0; index < participantCount; index++) // 최종 순위 항목 순회
            {
                PrototypeRankEntry entry = matchController.GetFinalRankEntry(index); // 현재 최종 순위 데이터 조회
                string localMarker = entry.IsLocalPlayer ? "▶ " : string.Empty; // 로컬 플레이어 표시 준비
                Rect entryRect = new Rect(resultRect.x + 20f, resultRect.y + 70f + index * 24f, resultRect.width - 40f, 22f); // 순위 항목 영역 계산
                GUI.Label(entryRect, $"{localMarker}{index + 1}위  {entry.DisplayName}  |  최고 {entry.Height:0.0} m", labelStyle); // 순위 항목 출력
            }
        }

        private void PrepareStyles() // GUI 스타일 최초 생성
        {
            if (titleStyle != null) // 스타일 생성 완료 확인
            {
                return; // 중복 생성 생략
            }

            titleStyle = new GUIStyle(GUI.skin.label); // 기본 제목 스타일 복사
            titleStyle.fontSize = 18; // 제목 글자 크기 적용
            titleStyle.fontStyle = FontStyle.Bold; // 제목 굵기 적용
            titleStyle.normal.textColor = Color.white; // 제목 글자 색상 적용

            labelStyle = new GUIStyle(GUI.skin.label); // 기본 정보 스타일 복사
            labelStyle.fontSize = 15; // 정보 글자 크기 적용
            labelStyle.normal.textColor = Color.white; // 정보 글자 색상 적용

            noticeStyle = new GUIStyle(GUI.skin.box); // 기본 안내 스타일 복사
            noticeStyle.fontSize = 20; // 안내 글자 크기 적용
            noticeStyle.fontStyle = FontStyle.Bold; // 안내 글자 굵기 적용
            noticeStyle.alignment = TextAnchor.MiddleCenter; // 안내 글자 중앙 정렬
            noticeStyle.normal.textColor = Color.white; // 안내 글자 색상 적용
        }

        private void DrawStaminaBar(Rect barRect, float normalizedValue) // 스태미나 막대 배경과 채움 출력
        {
            Color previousColor = GUI.color; // 기존 GUI 색상 저장
            GUI.color = staminaBackgroundColor; // 스태미나 배경 색상 적용
            GUI.DrawTexture(barRect, Texture2D.whiteTexture); // 스태미나 배경 출력

            float clampedValue = Mathf.Clamp01(normalizedValue); // 스태미나 비율 범위 제한
            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * clampedValue, barRect.height); // 스태미나 채움 영역 계산
            GUI.color = staminaColor; // 스태미나 채움 색상 적용
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture); // 스태미나 채움 출력
            GUI.color = previousColor; // 기존 GUI 색상 복원
        }
    }
}
