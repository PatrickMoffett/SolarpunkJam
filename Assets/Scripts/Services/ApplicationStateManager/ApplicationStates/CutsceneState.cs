using System.Collections.Generic;
using Constants;
using StackStateMachine;
using UnityEngine;

namespace Services
{
    public class CutsceneState : BaseApplicationState
    {
        public CutsceneState()
        {

        }

        protected override void SetToActiveStateFromBackground(StackStateMachineBaseState prevState, Dictionary<string, object> options)
        {

        }

        protected override void SetToBackgroundStateFromActive(StackStateMachineBaseState prevState, Dictionary<string, object> options)
        {

        }

        protected override void SetupState(StackStateMachineBaseState prevState, Dictionary<string, object> options)
        {
            ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueEnd += OnDialogueEnded;
        }

        private void OnDialogueEnded()
        {
            ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueEnd -= OnDialogueEnded;
            ServiceLocator.Instance.Get<LevelSceneManager>().LoadNextLevel();
            ServiceLocator.Instance.Get<ApplicationStateManager>().PushState<GameState>(true);
        }

        protected override void SetupStateInBackground(StackStateMachineBaseState prevState, Dictionary<string, object> options)
        {
            SetupState(prevState, options);
            SetToBackgroundStateFromActive(prevState, options);
        }

        protected override void TeardownState(StackStateMachineBaseState prevState, Dictionary<string, object> options)
        {

        }
    }
}
