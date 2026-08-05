using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using ProjectJ.Gameplay; // 경기 관리자 기능 참조
using ProjectJ.Player; // 플레이어 진행과 이동 기능 참조
using UnityEngine; // Unity 화면 UI 기능 참조

namespace ProjectJ.UI // 사용자 인터페이스 네임스페이스 선언
{ // 사용자 인터페이스 기능 묶음
    [DisallowMultipleComponent] // HUD 컴포넌트 중복 방지
    public sealed class MinimalPlayerHud : MonoBehaviour // 높이와 체크포인트와 경기 정보를 표시하는 최소 HUD 선언
    { // 최소 플레이어 HUD 기능 묶음
        [SerializeField] private PlayerRespawnController respawnController; // 체크포인트와 정상과 부활 정보 제공자
        [SerializeField] private PlayerHeightProgressController heightProgressController; // 높이와 수직 구간 정보 제공자
        [SerializeField] private PlayerMovementController movementController; // 스태미나 정보 제공자
        [SerializeField] private PrototypeMatchController matchController; // 경기 시간과 순위 제공자
        [SerializeField] private Vector2 panelPosition = new Vector2(20f, 20f); // HUD 패널 화면 위치
        [SerializeField, Min(260f)] private float panelWidth = 340f; // HUD 패널 너비
        [SerializeField, Min(240f)] private float rankingPanelWidth = 300f; // 실시간 순위 패널 너비
        [SerializeField] private Color courseProgressColor = new Color(1f, 0.55f, 0.1f, 1f); // 전체 코스 진행 막대 색상
        [SerializeField] private Color staminaColor = new Color(0.2f, 0.8f, 0.35f, 1f); // 스태미나 막대 색상
        [SerializeField] private Color barBackgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.9f); // 진행 막대 공통 배경 색상

        private GUIStyle titleStyle; // HUD 제목 스타일
        private GUIStyle labelStyle; // HUD 정보 스타일
        private GUIStyle localPlayerStyle; // 로컬 플레이어 강조 스타일
        private GUIStyle noticeStyle; // 부활 안내 스타일
        private GUIStyle resultStyle; // 경기 결과 강조 스타일

        private void Awake() // HUD 필수 참조 자동 연결과 검증
        { // HUD 준비 처리
            if (heightProgressController == null && respawnController != null) // 높이 진행 참조 자동 연결 조건 확인
            { // 높이 진행 참조 자동 연결 처리
                heightProgressController = respawnController.GetComponent<PlayerHeightProgressController>(); // 플레이어 오브젝트에서 높이 진행 참조 조회
            } // 높이 진행 참조 자동 연결 종료

            if (respawnController == null || heightProgressController == null || movementController == null || matchController == null) // HUD 참조 누락 확인
            { // HUD 참조 누락 처리
                ProjectLog.Error(ProjectLogCategory.Gameplay, "부활, 높이 진행, 이동, 경기 관리자 연결을 확인합니다.", "PLAYER_HUD_SOURCE_MISSING", this); // 필수 HUD 참조 누락 오류 출력
                enabled = false; // HUD 표시 비활성화
            } // HUD 참조 누락 처리 종료
        } // HUD 준비 종료

        private void OnValidate() // Inspector HUD 수치 보정
        { // HUD 수치 보정 처리
            panelWidth = Mathf.Max(260f, panelWidth); // 최소 진행 HUD 너비 보장
            rankingPanelWidth = Mathf.Max(240f, rankingPanelWidth); // 최소 순위 HUD 너비 보장
        } // HUD 수치 보정 종료

        private void OnGUI() // 최소 HUD 화면 출력
        { // HUD 화면 출력 처리
            PrepareStyles(); // GUI 스타일 준비
            DrawPlayerProgressPanel(); // 플레이어 진행 정보 패널 출력
            DrawCurrentRanking(); // 실제 높이 기반 실시간 순위 출력

            if (respawnController.IsRespawning) // 부활 진행 상태 확인
            { // 부활 안내 처리
                Rect noticeRect = new Rect(Screen.width * 0.5f - 180f, Screen.height * 0.5f - 35f, 360f, 70f); // 부활 안내 영역 계산
                GUI.Box(noticeRect, "추락\n체크포인트로 복귀 중", noticeStyle); // 부활 안내 출력
            } // 부활 안내 처리 종료

            if (matchController.IsMatchFinished) // 경기 종료 상태 확인
            { // 경기 결과 표시 처리
                DrawFinalResult(); // 최종 결과 화면 출력
            } // 경기 결과 표시 종료
        } // HUD 화면 출력 종료

        private void DrawPlayerProgressPanel() // 플레이어 진행과 경기 정보 패널 출력
        { // 진행 정보 패널 처리
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
            int displayedRank = matchController.IsMatchFinished ? matchController.FinalPlayerRank : matchController.PlayerRank; // 표시할 공동 순위 선택
            Rect matchRect = new Rect(panelRect.x + 12f, panelRect.y + 162f, panelRect.width - 24f, 22f); // 경기 정보 영역 계산
            GUI.Label(matchRect, $"남은 시간 {remainingMinutes:00}:{remainingSeconds:00}  |  순위 {displayedRank}/{matchController.ParticipantCount}", labelStyle); // 시간과 순위 출력

            Rect staminaLabelRect = new Rect(panelRect.x + 12f, panelRect.y + 188f, panelRect.width - 24f, 22f); // 스태미나 글자 영역 계산
            int staminaPercent = Mathf.RoundToInt(movementController.StaminaNormalized * 100f); // 스태미나 백분율 계산
            GUI.Label(staminaLabelRect, $"스태미나 : {staminaPercent}%", labelStyle); // 스태미나 수치 출력

            Rect staminaBarRect = new Rect(panelRect.x + 12f, panelRect.y + 218f, panelRect.width - 24f, 18f); // 스태미나 막대 영역 계산
            DrawProgressBar(staminaBarRect, movementController.StaminaNormalized, staminaColor); // 스태미나 막대 출력
        } // 진행 정보 패널 종료

        private void DrawCurrentRanking() // 실제 높이 기반 실시간 순위 패널 출력
        { // 실시간 순위 패널 처리
            int participantCount = matchController.ParticipantCount; // 전체 참가자 수 조회
            float panelHeight = 48f + participantCount * 26f; // 참가자 수 기반 순위 패널 높이 계산
            Rect rankingRect = new Rect(Screen.width - rankingPanelWidth - 20f, 20f, rankingPanelWidth, panelHeight); // 화면 오른쪽 위 순위 패널 영역 계산
            GUI.Box(rankingRect, GUIContent.none); // 실시간 순위 패널 배경 출력
            GUI.Label(new Rect(rankingRect.x + 12f, rankingRect.y + 8f, rankingRect.width - 24f, 26f), "실시간 순위", titleStyle); // 실시간 순위 제목 출력

            for (int index = 0; index < participantCount; index++) // 현재 순위 항목 순회
            { // 현재 순위 항목 처리
                PrototypeRankEntry entry = matchController.GetCurrentRankEntry(index); // 현재 순위 데이터 조회
                bool isSharedRank = CountCurrentEntriesWithRank(entry.Rank) > 1; // 현재 공동 순위 여부 계산
                string rankText = isSharedRank ? $"공동 {entry.Rank}위" : $"{entry.Rank}위"; // 단독 또는 공동 순위 문구 선택
                string localMarker = entry.IsLocalPlayer ? "▶ " : string.Empty; // 로컬 플레이어 표시 준비
                Rect entryRect = new Rect(rankingRect.x + 12f, rankingRect.y + 38f + index * 26f, rankingRect.width - 24f, 24f); // 현재 순위 한 줄 영역 계산
                GUIStyle entryStyle = entry.IsLocalPlayer ? localPlayerStyle : labelStyle; // 로컬 플레이어 강조 스타일 선택
                GUI.Label(entryRect, $"{localMarker}{rankText}  {entry.DisplayName}  |  {entry.Height:0.0} m", entryStyle); // 실제 높이와 공동 순위 출력
            } // 현재 순위 항목 처리 종료
        } // 실시간 순위 패널 종료

        private void DrawFinalResult() // 최종 경기 결과 출력
        { // 최종 경기 결과 처리
            int participantCount = matchController.ParticipantCount; // 전체 참가자 수 조회
            float resultHeight = 154f + participantCount * 26f; // 참가자 수 기반 결과창 높이 계산
            Rect resultRect = new Rect(Screen.width * 0.5f - 240f, Screen.height * 0.5f - resultHeight * 0.5f, 480f, resultHeight); // 결과창 영역 계산
            GUI.Box(resultRect, GUIContent.none); // 결과창 배경 출력

            string outcomeText = GetOutcomeText(); // 로컬 플레이어 승패 문구 조회
            string endReasonText = GetEndReasonText(); // 경기 종료 원인 문구 조회
            Rect titleRect = new Rect(resultRect.x + 20f, resultRect.y + 12f, resultRect.width - 40f, 32f); // 결과 제목 영역 계산
            GUI.Label(titleRect, outcomeText, resultStyle); // 승리와 공동 승리와 패배 제목 출력

            Rect reasonRect = new Rect(resultRect.x + 20f, resultRect.y + 46f, resultRect.width - 40f, 24f); // 종료 원인 영역 계산
            GUI.Label(reasonRect, endReasonText, labelStyle); // 정상 도달 또는 시간 종료 원인 출력

            Rect rankRect = new Rect(resultRect.x + 20f, resultRect.y + 74f, resultRect.width - 40f, 24f); // 최종 순위 영역 계산
            GUI.Label(rankRect, $"최종 순위 : {matchController.FinalPlayerRank}/{participantCount}", localPlayerStyle); // 플레이어 최종 공동 순위 출력

            for (int index = 0; index < participantCount; index++) // 최종 순위 항목 순회
            { // 최종 순위 항목 처리
                PrototypeRankEntry entry = matchController.GetFinalRankEntry(index); // 현재 최종 순위 데이터 조회
                bool isSharedRank = CountFinalEntriesWithRank(entry.Rank) > 1; // 최종 공동 순위 여부 계산
                string rankText = isSharedRank ? $"공동 {entry.Rank}위" : $"{entry.Rank}위"; // 단독 또는 공동 순위 문구 선택
                string localMarker = entry.IsLocalPlayer ? "▶ " : string.Empty; // 로컬 플레이어 표시 준비
                string summitMarker = entry.HasReachedCourseTop ? "  |  정상 도달" : string.Empty; // 정상 도달 결과 표시 준비
                Rect entryRect = new Rect(resultRect.x + 20f, resultRect.y + 108f + index * 26f, resultRect.width - 40f, 24f); // 최종 순위 항목 영역 계산
                GUIStyle entryStyle = entry.IsLocalPlayer ? localPlayerStyle : labelStyle; // 로컬 플레이어 강조 스타일 선택
                GUI.Label(entryRect, $"{localMarker}{rankText}  {entry.DisplayName}  |  {entry.Height:0.0} m{summitMarker}", entryStyle); // 최종 공동 순위와 실제 높이 출력
            } // 최종 순위 항목 처리 종료
        } // 최종 경기 결과 종료

        private int CountCurrentEntriesWithRank(int rank) // 현재 같은 공동 순위 참가자 수 계산
        { // 현재 공동 순위 집계 처리
            int count = 0; // 현재 공동 순위 수 준비

            for (int index = 0; index < matchController.ParticipantCount; index++) // 현재 순위 항목 순회
            { // 현재 공동 순위 확인 처리
                if (matchController.GetCurrentRankEntry(index).Rank == rank) // 같은 현재 순위 번호 확인
                { // 현재 공동 순위 집계 처리
                    count++; // 같은 순위 참가자 수 증가
                } // 현재 공동 순위 집계 처리 종료
            } // 현재 공동 순위 확인 처리 종료

            return count; // 같은 현재 순위 참가자 수 반환
        } // 현재 공동 순위 집계 종료

        private int CountFinalEntriesWithRank(int rank) // 최종 같은 공동 순위 참가자 수 계산
        { // 최종 공동 순위 집계 처리
            int count = 0; // 최종 공동 순위 수 준비

            for (int index = 0; index < matchController.ParticipantCount; index++) // 최종 순위 항목 순회
            { // 최종 공동 순위 확인 처리
                if (matchController.GetFinalRankEntry(index).Rank == rank) // 같은 최종 순위 번호 확인
                { // 최종 공동 순위 집계 처리
                    count++; // 같은 순위 참가자 수 증가
                } // 최종 공동 순위 집계 처리 종료
            } // 최종 공동 순위 확인 처리 종료

            return count; // 같은 최종 순위 참가자 수 반환
        } // 최종 공동 순위 집계 종료

        private string GetOutcomeText() // 로컬 플레이어 승패 문구 반환
        { // 승패 문구 선택 처리
            switch (matchController.PlayerOutcome) // 확정된 승패 결과 선택
            { // 승패 결과 분기 처리
                case PrototypeMatchOutcome.Victory: // 단독 승리 결과 확인
                    return "승리"; // 단독 승리 문구 반환
                case PrototypeMatchOutcome.SharedVictory: // 공동 승리 결과 확인
                    return "공동 승리"; // 공동 승리 문구 반환
                case PrototypeMatchOutcome.Defeat: // 패배 결과 확인
                    return "패배"; // 패배 문구 반환
                default: // 미확정 결과 확인
                    return "경기 종료"; // 기본 경기 종료 문구 반환
            } // 승패 결과 분기 처리 종료
        } // 승패 문구 선택 종료

        private string GetEndReasonText() // 경기 종료 원인 문구 반환
        { // 종료 원인 문구 선택 처리
            switch (matchController.EndReason) // 확정된 경기 종료 원인 선택
            { // 경기 종료 원인 분기 처리
                case PrototypeMatchEndReason.CourseTopReached: // 정상 도달 종료 확인
                    return "종료 원인 : 정상 지점 도달"; // 정상 도달 종료 문구 반환
                case PrototypeMatchEndReason.TimeExpired: // 제한 시간 종료 확인
                    return "종료 원인 : 제한 시간 만료"; // 시간 만료 종료 문구 반환
                default: // 미확정 종료 원인 확인
                    return "종료 원인 : 미정"; // 기본 종료 원인 문구 반환
            } // 경기 종료 원인 분기 처리 종료
        } // 종료 원인 문구 선택 종료

        private void PrepareStyles() // GUI 스타일 최초 생성
        { // GUI 스타일 준비 처리
            if (titleStyle != null) // 스타일 생성 완료 확인
            { // 스타일 생성 완료 처리
                return; // 중복 생성 생략
            } // 스타일 생성 완료 처리 종료

            titleStyle = new GUIStyle(GUI.skin.label); // 기본 제목 스타일 복사
            titleStyle.fontSize = 18; // 제목 글자 크기 적용
            titleStyle.fontStyle = FontStyle.Bold; // 제목 굵기 적용
            titleStyle.normal.textColor = Color.white; // 제목 글자 색상 적용
            labelStyle = new GUIStyle(GUI.skin.label); // 기본 정보 스타일 복사
            labelStyle.fontSize = 15; // 정보 글자 크기 적용
            labelStyle.normal.textColor = Color.white; // 정보 글자 색상 적용
            localPlayerStyle = new GUIStyle(labelStyle); // 로컬 플레이어 스타일 복사
            localPlayerStyle.fontStyle = FontStyle.Bold; // 로컬 플레이어 글자 굵기 적용
            localPlayerStyle.normal.textColor = new Color(0.2f, 1f, 0.9f, 1f); // 로컬 플레이어 강조 색상 적용
            noticeStyle = new GUIStyle(GUI.skin.box); // 기본 안내 스타일 복사
            noticeStyle.fontSize = 20; // 안내 글자 크기 적용
            noticeStyle.fontStyle = FontStyle.Bold; // 안내 글자 굵기 적용
            noticeStyle.alignment = TextAnchor.MiddleCenter; // 안내 글자 중앙 정렬
            noticeStyle.normal.textColor = Color.white; // 안내 글자 색상 적용
            resultStyle = new GUIStyle(titleStyle); // 경기 결과 제목 스타일 복사
            resultStyle.fontSize = 26; // 경기 결과 글자 크기 적용
            resultStyle.alignment = TextAnchor.MiddleCenter; // 경기 결과 중앙 정렬
        } // GUI 스타일 준비 종료

        private void DrawProgressBar(Rect barRect, float normalizedValue, Color fillColor) // 공통 진행 막대 배경과 채움 출력
        { // 공통 진행 막대 출력 처리
            Color previousColor = GUI.color; // 기존 GUI 색상 저장
            GUI.color = barBackgroundColor; // 진행 막대 배경 색상 적용
            GUI.DrawTexture(barRect, Texture2D.whiteTexture); // 진행 막대 배경 출력
            float clampedValue = Mathf.Clamp01(normalizedValue); // 진행 비율 범위 제한
            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * clampedValue, barRect.height); // 진행 막대 채움 영역 계산
            GUI.color = fillColor; // 전달된 진행 막대 채움 색상 적용
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture); // 진행 막대 채움 출력
            GUI.color = previousColor; // 기존 GUI 색상 복원
        } // 공통 진행 막대 출력 종료
    } // 최소 플레이어 HUD 기능 묶음 종료
} // 사용자 인터페이스 기능 묶음 종료
