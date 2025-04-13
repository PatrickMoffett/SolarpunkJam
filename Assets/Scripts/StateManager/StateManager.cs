using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StateManager
{
    /// <summary>
    /// 
    /// </summary>
    public class StateManager<TBaseStateType> where TBaseStateType : BaseState
    {
        /// <summary>
        /// Stack holding the applications current states
        /// </summary>
        private readonly Stack<BaseState> _states = new Stack<BaseState>();

        public event Action<BaseState> StatePushed;
        public event Action<BaseState> StatePopped;

        public StateManager()
        {

        }

        /// <summary>
        /// Pushes a new state of given type, becoming the new Active state <br/>
        /// Current state will be sent to a background state, <br/>
        /// Unless popCurrentState is true, in which case the state will be removed from the stack and set to Inactive
        /// </summary>
        /// <param name="popCurrentState">Optional data to be sent to the state</param>
        /// <param name="options">Option to pop current state off the stack</param>
        /// <typeparam name="TStateType">Type of state to push</typeparam>
        public void PushState<TStateType>(bool popCurrentState = false, Dictionary<string, object> options = null) 
            where TStateType : TBaseStateType, new()
        {
            // Grab previous state if one exists
            BaseState prevState = null;
            if (_states.Count > 0)
            {
                prevState = _states.Peek();
            }

            //pop the current state or put it into a background state
            if (popCurrentState)
            {
                PopState(false);
            }
            else
            {
                prevState?.Transition(BaseState.StateStatus.Background);
            }
            
            //Create a new state of type T and add it to the stack
            BaseState newState = new TStateType();
            _states.Push(newState);
            
            // Transition new state to active
            newState.Transition(BaseState.StateStatus.Active, prevState, options);
            StatePushed?.Invoke(newState);
        }

        /// <summary>
        /// Pops the current state off of the stack
        /// </summary>
        public void PopState(bool setTopActive = true)
        {
            //Pop top state and set to inactive
            BaseState popState;
            if (_states.Count > 0)
            {
                popState = _states.Pop();
                popState.Transition(BaseState.StateStatus.Inactive);
                StatePopped?.Invoke(popState);
            }
            else
            {
                popState = null;
            }

            if (setTopActive)
            {
                //Set new top state to active if it exists
                BaseState newTopState = _states.Count == 0 ? null : _states.Peek();
                newTopState?.Transition(BaseState.StateStatus.Active, popState);
            }
        }

        /// <summary>
        /// Get the current application state 
        /// </summary>
        /// <returns>current active state</returns>
        public BaseState GetCurrentState()
        {
            return _states.Peek();
        }
    
        /// <summary>
        /// Get the Type of the current application state
        /// </summary>
        /// <returns>Type of current state</returns>
        public Type GetCurrentStateType()
        {
            return _states.Peek().GetType();
        }
        
        /// <summary>
        /// Gets all the states in the stack as an array
        /// </summary>
        /// <returns>An array of the states in the stack</returns>
        public List<BaseState> GetStates()
        {
            return new List<BaseState>(_states.ToArray());
        }
    }
}
