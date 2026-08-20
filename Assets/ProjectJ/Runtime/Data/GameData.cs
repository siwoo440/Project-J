using UnityEngine;

namespace ProjectJ.Data
{
    [CreateAssetMenu(
        fileName = "NewGameData",
        menuName = "Project J/Data/Game Data"
    )]
    public class GameData : ScriptableObject
    {
        [SerializeField]
        private string id;

        [SerializeField]
        private string displayName;

        [SerializeField]
        [TextArea(2, 5)]
        private string description;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public bool HasValidId => GameDataId.IsValid(id);

        private void OnValidate()
        {
            if (!GameDataId.IsValid(id))
            {
                id = GameDataId.Create();
            }

            displayName = displayName?.Trim();
        }
    }
}
