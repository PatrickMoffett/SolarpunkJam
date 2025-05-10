using System.Collections.Generic;
using Constants;
using StackStateMachine;
using UnityEngine;

namespace Services
{
    public class MainMenuState : BaseApplicationState
    {
        public readonly string UI_PREFAB = UIPrefabs.UIMainMenu;

        private AudioClip _mainMenuMusic = Resources.Load<AudioClip>("MainMenu-LegendOfCurseV2");

        //public readonly int SCENE_NAME = (int)SceneIndexes.INITIAL_SCENE;
        private UIWidget _uiWidget;

        public MainMenuState()
        {

        }

        protected override void SetToBackgroundStateFromActive(StackStateMachineBaseState prevState, Dictionary<string, object> options)
        {
            if (_uiWidget != null)
            {
                _uiWidget.UIObject.SetActive(false);
            }
        }

        protected override void SetToActiveStateFromBackground(StackStateMachineBaseState prevState, Dictionary<string, object> options)
        {
            if (_uiWidget != null)
            {
                _uiWidget.UIObject.SetActive(true);
            }
            ServiceLocator.Instance.Get<LevelSceneManager>().LoadLevel(0);
        }

        protected override void SetupState(StackStateMachineBaseState prevState, Dictionary<string, object> options)
        {
            //ServiceLocator.Instance.Get<LevelSceneManager>().LoadLevel(SCENE_NAME);
            _uiWidget = ServiceLocator.Instance.Get<UIManager>().LoadUI(UI_PREFAB);
            _uiWidget.UIObject.GetComponent<UIMainMenu>()?.Setup();
            ServiceLocator.Instance.Get<MusicManager>().StartSong(_mainMenuMusic, 1f);
#if !UNITY_EDITOR
            ServiceLocator.Instance.Get<LevelSceneManager>().LoadLevel(0);
#endif
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
