using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ
{
    public sealed class BootstrapSceneController : MonoBehaviour
    {
        private void Start()
        {
            if (!Application.CanStreamedLevelBeLoaded(SceneNames.MainMenu))
            {
                Debug.LogError($"Build Settings에서 Scene을 찾을 수 없습니다: {SceneNames.MainMenu}");
                return;
            }

            SceneManager.LoadSceneAsync(SceneNames.MainMenu, LoadSceneMode.Single);
        }
    }
}
