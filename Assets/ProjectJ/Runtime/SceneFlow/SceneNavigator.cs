using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ
{
    public sealed class SceneNavigator : MonoBehaviour
    {
        private bool isLoading;

        public void OpenMainMenu()
        {
            LoadScene(SceneNames.MainMenu);
        }

        public void OpenGame()
        {
            LoadScene(SceneNames.Game);
        }

        public void OpenLobby()
        {
            LoadScene(SceneNames.Lobby);
        }

        private void LoadScene(string sceneName)
        {
            if (isLoading)
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"Build Settings에서 Scene을 찾을 수 없습니다: {sceneName}");
                return;
            }

            isLoading = true;
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
    }
}
