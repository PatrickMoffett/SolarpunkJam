using Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class UIGameWon : MonoBehaviour
    {
        public void ExitToMenuClicked()
        {
            // pop current state
            ServiceLocator.Instance.Get<ApplicationStateManager>().PopState();
        }
    }
}
