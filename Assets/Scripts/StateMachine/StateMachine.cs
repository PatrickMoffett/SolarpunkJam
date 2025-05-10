using UnityEngine;
using System;
using System.Diagnostics;

namespace StateMachine
{
    public abstract class BaseState<TContext>
    {
        protected TContext Context { get; private set; }
        public void Initialize(TContext ctx) => Context = ctx;
        public abstract void EnterState(BaseState<TContext> previousState);
        public abstract void ExitState(BaseState<TContext> nextState);
    }
    public class StateMachine<TContext, TState>
        where TState : BaseState<TContext>
    {
        public event Action<BaseState<TContext>> OnStateChanged;

        private TContext _context;
        private TState _currentState;

        public TState CurrentState {  get { return _currentState; } }

        public StateMachine(TContext context)
        {
            _context = context;
        }

        public void TransitionTo<TNewState>()
            where TNewState : TState, new()
        {
            // create & init the new
            var newState = new TNewState();
            newState.Initialize(_context);

            /*if(_currentState != null)
            {
                UnityEngine.Debug.Log($"Transitioning to {newState.GetType().Name} from {_currentState.GetType().Name}");
            }*/

            // exit old
            _currentState?.ExitState(newState);

            // enter it
            newState.EnterState(_currentState);
            _currentState = newState;

            OnStateChanged?.Invoke(_currentState);
        }
    }
}