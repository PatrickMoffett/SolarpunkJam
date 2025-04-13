using Services;
using UnityEngine;

namespace UI
{
    public class UIGameOver : MonoBehaviour
    {
        public void ExitToMenuClicked()
        {
            ServiceLocator.Instance.Get<ApplicationStateManager>().PopState();
        }
        public void RetryClicked()
        {
            ServiceLocator.Instance.Get<ApplicationStateManager>().PushState<GameState>(true);
        }
    }
}
