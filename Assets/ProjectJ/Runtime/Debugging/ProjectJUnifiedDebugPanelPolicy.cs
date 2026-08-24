using System; // 문자열 비교 기능

namespace ProjectJ.Debugging // 디버그 공통 네임스페이스
{
    public enum ProjectJDebugPanelCategory // 통합 패널 탭 분류
    {
        Overview = 0, // 개요 탭
        Network = 1, // 네트워크 탭
        Player = 2, // 플레이어 탭
        Session = 3, // 세션 탭
        Gameplay = 4 // 게임 상태 탭
    }

    public static class ProjectJUnifiedDebugPanelPolicy // 통합 패널 표시 정책
    {
        public const bool DefaultVisibility = // 기본 표시 상태
            false; // 시작 시 숨김

        public static bool ToggleVisibility( // 표시 상태 전환
            bool currentState // 현재 표시 상태
        )
        {
            return !currentState; // 반대 상태 반환
        }

        public static ProjectJDebugPanelCategory GetCategory( // 타입별 탭 분류
            string typeName // 진단창 타입 이름
        )
        {
            if (ContainsAny( // 세션 관련 이름 확인
                typeName, // 검사할 타입 이름
                "Steam", // Steam 진단 키워드
                "Invite", // 초대 진단 키워드
                "SceneFlow", // Scene 흐름 키워드
                "ConnectionRecovery", // 연결 복구 키워드
                "Lobby" // Lobby 흐름 키워드
            ))
            {
                return ProjectJDebugPanelCategory.Session; // 세션 탭 반환
            }

            if (ContainsAny( // 플레이어 관련 이름 확인
                typeName, // 검사할 타입 이름
                "FourPlayer", // 4인 진단 키워드
                "EightPlayer", // 8인 진단 키워드
                "PlayerMatchResult", // 플레이어 결과 키워드
                "Checkpoint", // 체크포인트 키워드
                "Respawn", // 부활 키워드
                "Spectator", // 관전 키워드
                "LocalPlayer" // 로컬 플레이어 키워드
            ))
            {
                return ProjectJDebugPanelCategory.Player; // 플레이어 탭 반환
            }

            if (ContainsAny( // 네트워크 관련 이름 확인
                typeName, // 검사할 타입 이름
                "NetworkCondition", // 네트워크 상태 키워드
                "NetworkTransform", // NetworkTransform 키워드
                "Prediction", // Prediction 키워드
                "Interpolation" // Interpolation 키워드
            ))
            {
                return ProjectJDebugPanelCategory.Network; // 네트워크 탭 반환
            }

            if (ContainsAny( // 게임 상태 관련 이름 확인
                typeName, // 검사할 타입 이름
                "Match", // 경기 상태 키워드
                "Finish", // 완주 상태 키워드
                "Fall", // 낙하 상태 키워드
                "Inventory", // 아이템 상태 키워드
                "ExternalGameplay" // 네트워크 경기 키워드
            ))
            {
                return ProjectJDebugPanelCategory.Gameplay; // 게임 상태 탭 반환
            }

            return ProjectJDebugPanelCategory.Overview; // 기본 개요 탭 반환
        }

        public static bool IsKnownDiagnosticWindow( // 기능 Component 내부 진단창 판정
            string typeName // 검사할 타입 이름
        )
        {
            return ContainsAny( // 알려진 진단창 이름 확인
                typeName, // 검사할 타입 이름
                "ProjectJDay76TestFlow", // 멀티플레이 테스트 창
                "ProjectJNetworkExternalGameplay", // 네트워크 경기 상태 창
                "ProjectJNetworkItemInventory", // 네트워크 아이템 상태 창
                "ProjectJLocalPlayerPresentationController", // 로컬 플레이어 상태 창
                "ProjectJNetworkLobbyFlow" // Lobby 진행 상태 창
            );
        }

        public static string GetCategoryLabel( // 탭 한글 이름 조회
            ProjectJDebugPanelCategory category // 조회할 탭 분류
        )
        {
            switch (category) // 탭 분류별 이름 선택
            {
                case ProjectJDebugPanelCategory.Network: // 네트워크 탭 확인
                    return "네트워크"; // 네트워크 이름 반환

                case ProjectJDebugPanelCategory.Player: // 플레이어 탭 확인
                    return "플레이어"; // 플레이어 이름 반환

                case ProjectJDebugPanelCategory.Session: // 세션 탭 확인
                    return "세션·Steam"; // 세션 이름 반환

                case ProjectJDebugPanelCategory.Gameplay: // 게임 상태 탭 확인
                    return "게임 상태"; // 게임 상태 이름 반환

                default: // 기본 탭 처리
                    return "개요"; // 개요 이름 반환
            }
        }

        private static bool ContainsAny( // 여러 키워드 포함 확인
            string source, // 검사할 원본 문자열
            params string[] values // 찾을 키워드 목록
        )
        {
            if (string.IsNullOrEmpty(source)) // 빈 문자열 확인
            {
                return false; // 키워드 없음 반환
            }

            for (int index = 0; index < values.Length; index++) // 키워드 순회
            {
                if (source.IndexOf( // 키워드 위치 검색
                    values[index], // 현재 키워드
                    StringComparison.OrdinalIgnoreCase // 대소문자 무시 비교
                ) >= 0) // 포함 여부 확인
                {
                    return true; // 키워드 포함 반환
                }
            }

            return false; // 모든 키워드 미포함 반환
        }
    }
}
