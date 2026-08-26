using UnityEditor; // Editor 메뉴 사용
using UnityEngine; // 경고 로그 사용

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay136BotSetup
    {
        [MenuItem(
            "Project J/Day136/Apply Bot Foundation"
        )]
        private static void ApplyBotFoundation()
        {
            Debug.LogWarning(
                "[Project J/Day136] Day136 Bot Setup은 Day140 안정성 정리 이후 퇴역되었습니다. " +
                "현재 Bot은 Checkpoint/FINISH를 장거리 목표로 사용하고 Physics 센서로 Walk/Jump/Fall을 자율 판단합니다."
            ); // 구형 Route Setup 재실행 차단 안내
        }
    }
}
