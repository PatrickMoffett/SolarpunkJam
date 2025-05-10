using Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class UIGameOver : MonoBehaviour
    {
        public void ExitToMenuClicked()
        {
            // pop current state
            ServiceLocator.Instance.Get<ApplicationStateManager>().PopState();
        }
        public void RetryClicked()
        {
            ServiceLocator.Instance.Get<ApplicationStateManager>().PushState<GameState>(true);
        }
    }
}
