using Services;
using UnityEngine;

public class UIMainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _quitButton;
    public void Setup()
    {
        // Disable quit button on WebGL builds
#if UNITY_WEBGL
        //_quitButton?.SetActive(false);
#endif
    }
    
    public void OnStartClicked()
    {
        ServiceLocator.Instance.Get<LevelSceneManager>().LoadNextLevel();
        ServiceLocator.Instance.Get<ApplicationStateManager>().PushState<CutsceneState>();
    }

    public void OnSettingsClicked()
    {
        ServiceLocator.Instance.Get<ApplicationStateManager>().PushState<SettingsState>();
    }

    public void OnCreditsClicked()
    {
        ServiceLocator.Instance.Get<ApplicationStateManager>().PushState<CreditsState>();
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
