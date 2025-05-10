using System.Collections.Generic;
using Constants;
using StackStateMachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services
{
    public class GameState : BaseApplicationState
    {
        public readonly string UI_PREFAB = UIPrefabs.UIGame;

        private UIWidget _uiWidget;

        private int _levelIndex = (int)Constants.SceneIndexes.GAME_SCENE;

        public GameState()
        {

        }

        protected override void SetToActiveStateFromBackground(StackStateMachineBaseState prevState, Dictionary<string, object> options)
        {
            if (_uiWidget != null)
            {
                _uiWidget.UIObject.SetActive(true);
            }
            
            Time.timeScale = 1f;
        }

        protected override void SetToBackgroundStateFromActive(StackStateMachineBaseState prevState, Dictionary<string, object> options)
        {
            if (_uiWidget != null)
            {
                _uiWidget.UIObject.SetActive(false);
            }
        }

        protected override void SetupState(StackStateMachineBaseState prevState, Dictionary<string, object> options)
        {
            _uiWidget = ServiceLocator.Instance.Get<UIManager>().LoadUI(UI_PREFAB);

            ServiceLocator.Instance.Get<LevelSceneManager>().LevelLoaded += FinishStateSetup;
#if UNITY_EDITOR
            //don't load the next level if we're testing out a level in the editor
            // and we didn't launch the gamestate from the initial/menu scene (which should be index 0)
            int currentLevelIndex = ServiceLocator.Instance.Get<LevelSceneManager>().GetLevelIndex();
            if (currentLevelIndex != _levelIndex)
            {
                ServiceLocator.Instance.Get<LevelSceneManager>().LoadLevel(_levelIndex);
            }
#else
            ServiceLocator.Instance.Get<LevelSceneManager>().LoadLevel(_levelIndex);
#endif
            
        }

        private void FinishStateSetup()
        {
            ServiceLocator.Instance.Get<LevelSceneManager>().LevelLoaded -= FinishStateSetup;
        }

        protected override void SetupStateInBackground(StackStateMachineBaseState prevState, Dictionary<string, object> options)
        {
            SetupState(prevState, options);
            SetToBackgroundStateFromActive(prevState, options);
        }

        protected override void TeardownState(StackStateMachineBaseState prevState, Dictionary<string, object> options)
        {
            if (_uiWidget != null)
            {
                ServiceLocator.Instance.Get<UIManager>().RemoveUIByGuid(_uiWidget.GUID);
            }
        }
    }
}
