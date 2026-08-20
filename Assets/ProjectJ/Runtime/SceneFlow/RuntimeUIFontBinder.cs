using UnityEngine;
using UnityEngine.UI;

namespace ProjectJ
{
    public sealed class RuntimeUIFontBinder : MonoBehaviour
    {
        private void Awake()
        {
            Font runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (runtimeFont == null)
            {
                Debug.LogError("Unity 기본 런타임 폰트를 불러오지 못했습니다.");
                return;
            }

            Text[] texts = GetComponentsInChildren<Text>(true);

            foreach (Text text in texts)
            {
                text.font = runtimeFont;
            }
        }
    }
}
