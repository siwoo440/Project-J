using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using ProjectJ.Gameplay; // 경기 관리자 기능 참조
using ProjectJ.Player; // 플레이어 진행과 이동 기능 참조
using UnityEngine; // Unity 화면 UI 기능 참조

namespace ProjectJ.UI // 사용자 인터페이스 네임스페이스 선언
{ // 최소 플레이어 HUD 범위
    [DisallowMultipleComponent] // HUD 컴포넌트 중복 방지
    public sealed class MinimalPlayerHud : MonoBehaviour // 높이와 체크포인트와 경기 정보를 표시하는 최소 HUD 선언
    { // 최소 플레이어 HUD 기능 범위
        [SerializeField] private PlayerRespawnController respawnController; // 체크포인트와 정상과 부활 정보 제공자
        [SerializeField] private PlayerHeightProgressController heightProgressController; // 높이와 수직 구간 정보 제공자
        [SerializeField] private PlayerMovementController movementController; // 스태미나 정보 제공자
        [SerializeField] private PrototypeMatchController matchController; // 경기 시간과 순위 제공자
        [SerializeField] private Vector2 panelPosition = new Vector2(20f, 20f); // HUD 패널 화면 위치
        [SerializeField, Min(260f)] private float panelWidth = 340f; // HUD 패널 너비
        [SerializeField] private Color courseProgressColor = new Color(1f, 0.55f, 0.1f, 1f); // 전체 코스 진행 막대 색상
        [SerializeField] private Color staminaColor = new Color(0.2f, 0.8f, 0.35f, 1f); // 스태미나 막대 색상
        [SerializeField] private Color barBackgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.9f); // 진행 막대 공통 배경 색상

        private GUIStyle titleStyle; // HUD 제목 스타일
        private GUIStyle labelStyle; // HUD 정보 스타일
        private GUIStyle noticeStyle; // 부활 안내 스타일

        private void Awake() // HUD 필수 참조 자동 연결과 검증
        { // HUD 준비 범위
            if (heightProgressController == null && respawnController != null) // 높이 진행 참조 자동 연결 조건 확인
            { // 높이 진행 참조 자동 연결 범위
                heightProgressController = respawnController.GetComponent<PlayerHeightProgressController>(); // 플레이어 오브젝트에서 높이 진행 참조 조회
            } // 높이 진행 참조 자동 연결 범위 종료

            if (respawnController == null || heightProgressController == null || movementController == null || matchController == null) // HUD 참조 누락 확인
            { // HUD 참조 누락 범위
                ProjectLog.Error(ProjectLogCategory.Gameplay, "부활, 높이 진행, 이동, 경기 관리자 연결을 확인합니다.", "PLAYER_HUD_SOURCE_MISSING", this); // 필수 HUD 참조 누락 오류 출력
                enabled = false; // HUD 표시 비활성화
            } // HUD 참조 누락 범위 종료
        } // HUD 준비 범위 종료

        private void OnGUI() // 최소 HUD 화면 출력
        { // HUD 화면 출력 범위
            PrepareStyles(); // GUI 스타일 준비

            Rect panelRect = new Rect(panelPosition.x, panelPosition.y, panelWidth, 258f); // HUD 패널 영역 계산
            GUI.Box(panelRect, GUIContent.none); // HUD 패널 배경 출력

            Rect titleRect = new Rect(panelRect.x + 12f, panelRect.y + 8f, panelRect.width - 24f, 24f); // 제목 영역 계산
            GUI.Label(titleRect, "PROJECT J", titleStyle); // HUD 제목 출력

            Rect heightRect = new Rect(panelRect.x + 12f, panelRect.y + 36f, panelRect.width - 24f, 22f); // 높이 영역 계산
            GUI.Label(heightRect, $"현재 {heightProgressController.CurrentHeight:0.0} m  |  최고 {heightProgressController.HighestHeight:0.0} m", labelStyle); // 현재와 최고 높이 출력

            int courseProgressPercent = Mathf.RoundToInt(heightProgressController.CourseProgress01 * 100f); // 전체 코스 진행 백분율 계산
            Rect sectionRect = new Rect(panelRect.x + 12f, panelRect.y + 62f, panelRect.width - 24f, 22f); // 수직 구간 영역 계산
            GUI.Label(sectionRect, $"구간 {heightProgressController.CurrentSectionIndex}/{heightProgressController.SectionCount}  |  전체 {courseProgressPercent}%", labelStyle); // 현재 구간과 전체 진행률 출력

            Rect courseBarRect = new Rect(panelRect.x + 12f, panelRect.y + 88f, panelRect.width - 24f, 14f); // 전체 코스 진행 막대 영역 계산
            DrawProgressBar(courseBarRect, heightProgressController.CourseProgress01, courseProgressColor); // 전체 코스 진행 막대 출력

            Rect checkpointRect = new Rect(panelRect.x + 12f, panelRect.y + 110f, panelRect.width - 24f, 22f); // 체크포인트 영역 계산
            GUI.Label(checkpointRect, $"체크포인트 {respawnController.CurrentCheckpointIndex}/{respawnController.CheckpointCount}  |  {respawnController.CurrentCheckpointId}", labelStyle); // 체크포인트 순서와 식별자 출력

            string courseTopState = respawnController.HasReachedCourseTop ? "도달" : "미도달"; // 정상 지점 도달 표시 문구 선택
            Rect courseTopRect = new Rect(panelRect.x + 12f, panelRect.y + 136f, panelRect.width - 24f, 22f); // 정상 지점 상태 영역 계산
            GUI.Label(courseTopRect, $"정상 지점 : {courseTopState}", labelStyle); // 정상 지점 도달 상태 출력

            int remainingTotalSeconds = Mathf.CeilToInt(matchController.RemainingTime); // 남은 전체 초 계산
            int remainingMinutes = remainingTotalSeconds / 60; // 남은 분 계산
            int remainingSeconds = remainingTotalSeconds % 60; // 남은 초 계산
            int displayedRank = matchController.IsMatchFinished ? matchController.FinalPlayerRank : matchController.PlayerRank; // 표시할 순위 선택
            Rect matchRect = new Rect(panelRect.x + 12f, panelRect.y + 162f, panelRect.width - 24f, 22f); // 경기 정보 영역 계산
            GUI.Label(matchRect, $"남은 시간 {remainingMinutes:00}:{remainingSeconds:00}  |  순위 {displayedRank}/{matchController.ParticipantCount}", labelStyle); // 시간과 순위 출력

            Rect staminaLabelRect = new Rect(panelRect.x + 12f, panelRect.y + 188f, panelRect.width - 24f, 22f); // 스태미나 글자 영역 계산
            int staminaPercent = Mathf.RoundToInt(movementController.StaminaNormalized * 100f); // 스태미나 백분율 계산
            GUI.Label(staminaLabelRect, $"스태미나 : {staminaPercent}%", labelStyle); // 스태미나 수치 출력

            Rect staminaBarRect = new Rect(panelRect.x + 12f, panelRect.y + 218f, panelRect.width - 24f, 18f); // 스태미나 막대 영역 계산
            DrawProgressBar(staminaBarRect, movementController.StaminaNormalized, staminaColor); // 스태미나 막대 출력

            if (respawnController.IsRespawning) // 부활 진행 상태 확인
            { // 부활 안내 범위
                Rect noticeRect = new Rect(Screen.width * 0.5f - 180f, Screen.height * 0.5f - 35f, 360f, 70f); // 부활 안내 영역 계산
                GUI.Box(noticeRect, "추락\n체크포인트로 복귀 중", noticeStyle); // 부활 안내 출력
            } // 부활 안내 범위 종료

            if (matchController.IsMatchFinished) // 경기 종료 상태 확인
            { // 경기 결과 표시 범위
                DrawFinalResult(); // 최종 결과 화면 출력
            } // 경기 결과 표시 범위 종료
        } // HUD 화면 출력 범위 종료

        private void DrawFinalResult() // 최종 경기 결과 출력
        { // 최종 경기 결과 범위
            int participantCount = matchController.ParticipantCount; // 전체 참가자 수 조회
            float resultHeight = 90f + participantCount * 24f; // 참가자 수 기반 결과창 높이 계산
            Rect resultRect = new Rect(Screen.width * 0.5f - 210f, Screen.height * 0.5f - resultHeight * 0.5f, 420f, resultHeight); // 결과창 영역 계산
            GUI.Box(resultRect, GUIContent.none); // 결과창 배경 출력

            Rect titleRect = new Rect(resultRect.x + 20f, resultRect.y + 12f, resultRect.width - 40f, 26f); // 결과 제목 영역 계산
            GUI.Label(titleRect, "경기 종료", titleStyle); // 경기 종료 제목 출력

            Rect rankRect = new Rect(resultRect.x + 20f, resultRect.y + 42f, resultRect.width - 40f, 24f); // 최종 순위 영역 계산
            GUI.Label(rankRect, $"최종 순위 : {matchController.FinalPlayerRank}/{participantCount}", labelStyle); // 플레이어 최종 순위 출력

            for (int index = 0; index < participantCount; index++) // 최종 순위 항목 순회
            { // 최종 순위 항목 반복 범위
                PrototypeRankEntry entry = matchController.GetFinalRankEntry(index); // 현재 최종 순위 데이터 조회
                string localMarker = entry.IsLocalPlayer ? "▶ " : string.Empty; // 로컬 플레이어 표시 준비
                Rect entryRect = new Rect(resultRect.x + 20f, resultRect.y + 70f + index * 24f, resultRect.width - 40f, 22f); // 순위 항목 영역 계산
                GUI.Label(entryRect, $"{localMarker}{index + 1}위  {entry.DisplayName}  |  최고 {entry.Height:0.0} m", labelStyle); // 순위 항목 출력
            } // 최종 순위 항목 반복 범위 종료
        } // 최종 경기 결과 범위 종료

        private void PrepareStyles() // GUI 스타일 최초 생성
        { // GUI 스타일 준비 범위
            if (titleStyle != null) // 스타일 생성 완료 확인
            { // 스타일 생성 완료 범위
                return; // 중복 생성 생략
            } // 스타일 생성 완료 범위 종료

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
        } // GUI 스타일 준비 범위 종료

        private void DrawProgressBar(Rect barRect, float normalizedValue, Color fillColor) // 공통 진행 막대 배경과 채움 출력
        { // 공통 진행 막대 출력 범위
            Color previousColor = GUI.color; // 기존 GUI 색상 저장
            GUI.color = barBackgroundColor; // 진행 막대 배경 색상 적용
            GUI.DrawTexture(barRect, Texture2D.whiteTexture); // 진행 막대 배경 출력
            float clampedValue = Mathf.Clamp01(normalizedValue); // 진행 비율 범위 제한
            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * clampedValue, barRect.height); // 진행 막대 채움 영역 계산
            GUI.color = fillColor; // 전달된 진행 막대 채움 색상 적용
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture); // 진행 막대 채움 출력
            GUI.color = previousColor; // 기존 GUI 색상 복원
        } // 공통 진행 막대 출력 범위 종료
    } // 최소 플레이어 HUD 기능 범위 종료
} // 최소 플레이어 HUD 범위 종료
