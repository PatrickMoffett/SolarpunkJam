﻿using System.Collections.Generic;
using Constants;
using StateManager;

namespace Services
{
    public class GameOverState : BaseApplicationState
    {
        public readonly string UI_PREFAB = UIPrefabs.UIGameOver;
        public readonly int SCENE_INDEX = (int)SceneIndexes.INITIAL_SCENE;
        private UIWidget _uiWidget;

        public GameOverState()
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
        }

        protected override void SetupState(BaseState prevState, Dictionary<string, object> options)
        {
            _uiWidget = ServiceLocator.Instance.Get<UIManager>().LoadUI(UI_PREFAB);
            ServiceLocator.Instance.Get<LevelSceneManager>().LoadLevel(SCENE_INDEX);
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
