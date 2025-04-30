using System.Collections.Generic;
using UnityEngine;

namespace StackStateMachine
{
    /// <summary>
    /// Abstract class for states to be used in a StackStateMachine
    /// </summary>
    public abstract class StackStateMachineBaseState
    {
        /// <summary>
        /// State Representing the current stack state of an State
        /// <param name="Inactive">means the state isn't in the StackStateMachine</param>
        /// <param name="Background">means the state is not the Active/Top State of the StackStateMachine</param>
        /// <param name="Active">means the state is the currently Active/Top State of the StackStateMachine</param>
        /// </summary>
        public enum StateStatus
        {
            Inactive, // means the state isn't in the StackStateMachine
            Active, // means the state is the currently Active/Top State of the StackStateMachine
            Background, // means the state is not the Active/Top State of the StackStateMachine
        }
        /// <summary>
        /// Current status of the state
        /// </summary>
        public StateStatus CurrentStateStatus { get; protected set; } = StateStatus.Inactive;
    
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Transition into a new state
        /// </summary>
        /// <param name="toStateStatus">new state to set</param>
        /// <param name="prevState">previous application state</param>
        /// <param name="options">optional dictionary options</param>
        public void Transition(StateStatus toStateStatus, StackStateMachineBaseState prevState = null, Dictionary<string, object> options = null){
        
            // Guard against same state transition
            if (toStateStatus == CurrentStateStatus)
            {
                return;
            }

            switch (toStateStatus)
            {
                case StateStatus.Inactive:
                    TeardownState(prevState,options);
                    break;
            
                case StateStatus.Active:
                    if (CurrentStateStatus == StateStatus.Inactive)
                    {
                        SetupState(prevState,options);
                    }
                    else
                    {
                        SetToActiveStateFromBackground(prevState,options);
                    }
                    break;
            
                case StateStatus.Background:
                    if (CurrentStateStatus == StateStatus.Active)
                    {
                        SetToBackgroundStateFromActive(prevState,options);
                    }
                    else
                    {
                        SetupStateInBackground(prevState,options);
                    }
                    break;
            
                default:
                    Debug.LogError("Unsupported state found in BaseApplicationState Transition.");
                    break;
            }
            //update the current state
            CurrentStateStatus = toStateStatus;
        }

    
        /// <summary>
        /// Set state to background from active state
        /// </summary>
        /// <param name="prevState">previous application state</param>
        /// <param name="options"> optional dictionary options</param>
        protected abstract void SetToBackgroundStateFromActive(StackStateMachineBaseState prevState,Dictionary<string, object> options);

        /// <summary>
        /// Set state to active from background state
        /// </summary>
        /// <param name="prevState">previous application state</param>
        /// <param name="options"> optional dictionary options</param>
        protected abstract void SetToActiveStateFromBackground(StackStateMachineBaseState prevState,Dictionary<string, object> options);

        /// <summary>
        /// Teardown state
        /// </summary>
        /// <param name="prevState">previous application state</param>
        /// <param name="options"> optional dictionary options</param>
        protected abstract void TeardownState(StackStateMachineBaseState prevState,Dictionary<string, object> options);

        /// <summary>
        /// Setup state
        /// </summary>
        /// <param name="prevState">optional previous application state</param>
        /// <param name="options"> optional dictionary options</param>
        protected abstract void SetupState(StackStateMachineBaseState prevState,Dictionary<string, object> options);

        /// <summary>
        /// Setup state and immediately put into background state
        /// </summary>
        /// <param name="prevState">previous application state</param>
        /// <param name="options"> optional dictionary options</param>
        protected abstract void SetupStateInBackground(StackStateMachineBaseState prevState,Dictionary<string, object> options);

    }
}
