using System.Collections.Generic;
using StateManager;
using UnityEngine;

namespace Services
{
    public class MainMenuState : BaseApplicationState
    {
        public readonly string UI_PREFAB = "UIMainMenu";

        private AudioClip _mainMenuMusic = Resources.Load<AudioClip>("MainMenu-LegendOfCurseV2");

        //public readonly int SCENE_NAME = (int)SceneIndexes.INITIAL_SCENE;
        private UIWidget _uiWidget;

        public MainMenuState()
        {

        }

        protected override void SetToBackgroundStateFromActive(BaseState prevState, Dictionary<string, object> options)
        {
            if (_uiWidget != null)
            {
                _uiWidget.UIObject.SetActive(false);
            }
        }

        protected override void SetToActiveStateFromBackground(BaseState prevState, Dictionary<string, object> options)
        {
            if (_uiWidget != null)
            {
                _uiWidget.UIObject.SetActive(true);
            }

            //ServiceLocator.Instance.Get<LevelSceneManager>().LoadLevel(SCENE_NAME);
        }

        protected override void SetupState(BaseState prevState, Dictionary<string, object> options)
        {
            //ServiceLocator.Instance.Get<LevelSceneManager>().LoadLevel(SCENE_NAME);
            _uiWidget = ServiceLocator.Instance.Get<UIManager>().LoadUI(UI_PREFAB);
            _uiWidget.UIObject.GetComponent<UIMainMenu>()?.Setup();
            ServiceLocator.Instance.Get<MusicManager>().StartSong(_mainMenuMusic, 1f);

        }

        protected override void SetupStateInBackground(BaseState prevState, Dictionary<string, object> options)
        {
            SetupState(prevState, options);
            SetToBackgroundStateFromActive(prevState, options);
        }

        protected override void TeardownState(BaseState prevState, Dictionary<string, object> options)
        {
            if (_uiWidget != null)
            {
                ServiceLocator.Instance.Get<UIManager>().RemoveUIByGuid(_uiWidget.GUID);
            }
        }
    }
}
