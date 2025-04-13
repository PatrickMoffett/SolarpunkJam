using Services;
using UnityEngine;

public class UIPauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject _quitButton;
    public void Setup()
    {
#if UNITY_WEBGL
        _quitButton?.SetActive(false);
#endif
    }
    
    public void ResumeClicked()
    {
        //remove pause menu state
        ServiceLocator.Instance.Get<ApplicationStateManager>().PopState();
    }
    
    public void SettingsClicked()
    {
        ServiceLocator.Instance.Get<ApplicationStateManager>().PushState<SettingsState>();
    }
    public void ExitToMenuClicked()
    {
        //remove pause menu state
        ServiceLocator.Instance.Get<ApplicationStateManager>().PopState();
        //remove game state
        ServiceLocator.Instance.Get<ApplicationStateManager>().PopState();
    }

    public void ExitGameClicked()
    {
        Application.Quit();
    }
}
