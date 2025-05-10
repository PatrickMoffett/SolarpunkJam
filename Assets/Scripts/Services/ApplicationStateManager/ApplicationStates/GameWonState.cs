using System.Collections.Generic;
using Constants;
using StackStateMachine;

namespace Services
{
    public class GameWonState : BaseApplicationState
    {
        public readonly string UI_PREFAB = UIPrefabs.UIGameWon;
        public readonly int SCENE_INDEX = (int)SceneIndexes.GAME_WON;
        private UIWidget _uiWidget;

        public GameWonState()
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
        }

        protected override void SetupState(StackStateMachineBaseState prevState, Dictionary<string, object> options)
        {
            ServiceLocator.Instance.Get<LevelSceneManager>().LoadLevel(SCENE_INDEX);
            _uiWidget = ServiceLocator.Instance.Get<UIManager>().LoadUI(UI_PREFAB);
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