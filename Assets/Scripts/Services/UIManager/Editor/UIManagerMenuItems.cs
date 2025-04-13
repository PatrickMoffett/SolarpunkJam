using UnityEditor;
using UnityEngine;

namespace Services
{
    public static class UIManagerMenuItems
    {
        [MenuItem("Assets/UI/Add To UI")]
        private static void AddToUI()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("AddToUI is only for testing UI during Play Mode");
            }
            ServiceLocator.Instance.Get<UIManager>().LoadUI((GameObject)Selection.activeObject);
        }
    }
}