using ProjectJ.Gameplay; // 경기 결과와 순위 데이터 형식 참조
using UnityEngine; // 수치 보정과 올림 계산 기능 참조

namespace ProjectJ.UI // 프로젝트 Canvas UI 네임스페이스 선언
{ // 프로젝트 Canvas UI 기능 묶음
    public static class CanvasUiTextRules // Canvas UI 공통 표시 문구 규칙 선언
    { // Canvas UI 공통 표시 문구 묶음
        public static string FormatTimer(float remainingTime) // 남은 시간을 분과 초 문구로 변환
        { // 남은 시간 문구 변환 처리
            int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, remainingTime)); // 음수가 없는 남은 전체 초 계산
            int minutes = totalSeconds / 60; // 남은 분 계산
            int seconds = totalSeconds % 60; // 남은 초 계산
            return $"{minutes:00}:{seconds:00}"; // 두 자리 분과 초 문구 반환
        } // 남은 시간 문구 변환 처리 종료

        public static string FormatRank(int rank, int participantCount) // 현재 순위와 참가자 수 문구 생성
        { // 순위 문구 생성 처리
            int safeRank = Mathf.Max(1, rank); // 최소 1위 순위 보정
            int safeParticipantCount = Mathf.Max(1, participantCount); // 최소 한 명 참가자 수 보정
            return $"{safeRank} / {safeParticipantCount}"; // 순위와 참가자 수 문구 반환
        } // 순위 문구 생성 처리 종료

        public static string GetOutcomeText(PrototypeMatchOutcome outcome) // 경기 승패 결과 문구 선택
        { // 경기 승패 결과 문구 선택 처리
            switch (outcome) // 경기 승패 결과 분기
            { // 경기 승패 결과 분기 처리
                case PrototypeMatchOutcome.Victory: // 단독 승리 결과 확인
                    return "승리"; // 단독 승리 문구 반환
                case PrototypeMatchOutcome.SharedVictory: // 공동 승리 결과 확인
                    return "공동 승리"; // 공동 승리 문구 반환
                case PrototypeMatchOutcome.Defeat: // 패배 결과 확인
                    return "패배"; // 패배 문구 반환
                default: // 미확정 경기 결과 확인
                    return "경기 종료"; // 기본 경기 종료 문구 반환
            } // 경기 승패 결과 분기 처리 종료
        } // 경기 승패 결과 문구 선택 처리 종료

        public static string GetEndReasonText(PrototypeMatchEndReason endReason) // 경기 종료 원인 문구 선택
        { // 경기 종료 원인 문구 선택 처리
            switch (endReason) // 경기 종료 원인 분기
            { // 경기 종료 원인 분기 처리
                case PrototypeMatchEndReason.CourseTopReached: // 정상 도달 종료 확인
                    return "종료 원인 : 정상 지점 도달"; // 정상 도달 종료 문구 반환
                case PrototypeMatchEndReason.TimeExpired: // 제한 시간 종료 확인
                    return "종료 원인 : 제한 시간 만료"; // 제한 시간 종료 문구 반환
                default: // 미확정 종료 원인 확인
                    return "종료 원인 : 미정"; // 기본 종료 원인 문구 반환
            } // 경기 종료 원인 분기 처리 종료
        } // 경기 종료 원인 문구 선택 처리 종료

        public static string FormatRankingEntry(PrototypeRankEntry entry, bool isSharedRank, bool includeCourseTop) // 순위 한 줄 문구 생성
        { // 순위 한 줄 문구 생성 처리
            string rankText = isSharedRank ? $"공동 {entry.Rank}위" : $"{entry.Rank}위"; // 공동 여부 기반 순위 문구 선택
            string localMarker = entry.IsLocalPlayer ? "▶ " : string.Empty; // 로컬 플레이어 표시 문구 선택
            string courseTopMarker = includeCourseTop && entry.HasReachedCourseTop ? "  |  정상 도달" : string.Empty; // 정상 도달 표시 문구 선택
            return $"{localMarker}{rankText}  {entry.DisplayName}  |  {entry.Height:0.0} m{courseTopMarker}"; // 완성된 순위 한 줄 문구 반환
        } // 순위 한 줄 문구 생성 처리 종료
    } // Canvas UI 공통 표시 문구 묶음 종료
} // 프로젝트 Canvas UI 기능 묶음 종료
