using UnityEngine;
using Services;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;


public static class Bootstrapper
{
    
    /// <summary>
    /// This function is used to initialize a variety of services and setup the initial state of the game.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        ServiceLocator.Initialize();
        
        //Setup services that must be attached to a GameObject
        GameObject singletonObject = new GameObject("singletonObject",
            typeof(EventSystem),
            typeof(InputSystemUIInputModule),
            typeof(MonoBehaviorService));
        ServiceLocator.Instance.Register(singletonObject.GetComponent<MonoBehaviorService>());
        Object.DontDestroyOnLoad(singletonObject);
        
        //Setup Services
        ServiceLocator.Instance.Register(new ApplicationStateManager());
        ServiceLocator.Instance.Register(new AudioManager());
        ServiceLocator.Instance.Register(new MusicManager());
        ServiceLocator.Instance.Register(new LevelSceneManager());
        ServiceLocator.Instance.Register(new UIManager());
        ServiceLocator.Instance.Register(new TileMapManager());

        //Start MainMenuState
        ServiceLocator.Instance.Get<ApplicationStateManager>().PushState<MainMenuState>();
#if UNITY_EDITOR
        int currentLevelIndex = ServiceLocator.Instance.Get<LevelSceneManager>().GetLevelIndex();
        if (currentLevelIndex == 0) return;
        ServiceLocator.Instance.Get<ApplicationStateManager>().PushState<GameState>();
#endif
    }
}


