using System.Collections.Generic;
using Constants;
using StateManager;

namespace Services
{
    public class VolumeSettingsState : BaseApplicationState
    {
        public readonly string UI_PREFAB = UIPrefabs.UIVolumeSettings;
        private UIWidget _uiWidget;

        public VolumeSettingsState()
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
            _uiWidget.UIObject.GetComponent<UIVolumeSettings>().Setup();
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