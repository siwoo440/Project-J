using UnityEngine; // 기존 Bootstrap Scene 직행 컴포넌트 유지

namespace ProjectJ
{
    [DisallowMultipleComponent]
    public sealed class BootstrapSceneController :
        MonoBehaviour
    {
        // 85일차:
        // MainMenu 전환은 ProjectJDay82SceneFlowCoordinator가 담당한다.
        // 구형 Start() 자동 Scene 전환은 제거했다.
    }
}
