using UnityEngine;

namespace ProjectJ.Networking.Fusion
{
    public static class
        ProjectJDay97DedicatedServerDiagnostics
    {
#if UNITY_SERVER
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad
        )]
        private static void LogServerBuildStart()
        {
            Debug.Log(
                "[Project J/Day97] " +
                "Dedicated Server Build 시작" +
                " / UNITY_SERVER=True" +
                " / BatchMode=" +
                Application.isBatchMode
            );
        }
#endif
    }
}
