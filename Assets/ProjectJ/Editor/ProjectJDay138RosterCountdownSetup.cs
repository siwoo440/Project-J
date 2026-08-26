using System.IO; // External Gameplay Source 수정 사용
using System.Text; // UTF8 저장 사용
using UnityEditor; // Editor 메뉴와 Asset 갱신 사용
using UnityEngine; // Debug 출력 사용

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay138RosterCountdownSetup
    {
        private const string ExternalGameplaySourcePath =
            "Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkExternalGameplay.cs"; // Countdown Source 경로

        private static readonly UTF8Encoding Utf8WithoutBom =
            new UTF8Encoding(
                false
            ); // BOM 없는 UTF8 저장

        [MenuItem(
            "Project J/Day138/Apply Roster Countdown And Climb Priority"
        )]
        private static void ApplyRosterCountdownAndClimbPriority()
        {
            if (!File.Exists(ExternalGameplaySourcePath))
            {
                Debug.LogError(
                    "[Project J/Day138] ProjectJNetworkExternalGameplay.cs를 찾지 못했습니다."
                ); // Countdown Source 누락 오류 출력

                return;
            }

            string source =
                File.ReadAllText(
                    ExternalGameplaySourcePath
                ); // 최신 External Gameplay Source 읽기

            string newline =
                source.Contains(
                    "\r\n"
                )
                    ? "\r\n"
                    : "\n"; // 기존 줄바꿈 형식 확인

            string rosterGate =
                "                !ProjectJNetworkBotRosterManager.IsCountdownAllowed(Runner) ||"; // Roster Countdown Gate 내용

            if (!source.Contains(rosterGate))
            {
                string oldGuard =
                    "                !Object.HasStateAuthority ||" + newline +
                    "                GetMatchCoordinator() != this ||" + newline +
                    "                (ProjectJNetworkMatchState)NetworkMatchStateValue != ProjectJNetworkMatchState.Preparing"; // 기존 BeginCountdown Guard 패턴

                string newGuard =
                    "                !Object.HasStateAuthority ||" + newline +
                    rosterGate + newline +
                    "                GetMatchCoordinator() != this ||" + newline +
                    "                (ProjectJNetworkMatchState)NetworkMatchStateValue != ProjectJNetworkMatchState.Preparing"; // 전원 충원 Gate 포함 Guard

                if (!source.Contains(oldGuard))
                {
                    Debug.LogError(
                        "[Project J/Day138] 최신 main BeginCountdownAuthority 패턴과 일치하지 않아 자동 수정을 중단했습니다."
                    ); // Source 패턴 불일치 오류 출력

                    return;
                }

                source =
                    source.Replace(
                        oldGuard,
                        newGuard
                    ); // 모든 Countdown 진입 경로에 Roster Gate 추가

                File.WriteAllText(
                    ExternalGameplaySourcePath,
                    source,
                    Utf8WithoutBom
                ); // External Gameplay Source 저장
            }

            AssetDatabase.Refresh(); // 변경 Source 재컴파일 요청

            Debug.Log(
                "[Project J/Day138] 전원 충원 Countdown Gate와 Route 진행 우선 Bot 행동 적용 완료."
            ); // 추가 수정 적용 완료 출력
        }
    }
}
