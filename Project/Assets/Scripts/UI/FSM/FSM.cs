using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI.FSM
{
    [Serializable]
    public class FSM
    {
        public readonly List<FSMState> States = new List<FSMState>();

        private bool _isSwitching;
        private int  _popQueue;
        private FSMState _nextState;
        private FSMState _nextStateToPush;
        private List<FSMTransition> _popupTransitions = new List<FSMTransition>();
        private List<FSMTransition> _transitions = new List<FSMTransition>();
        private Stack<FSMState> _queue = new Stack<FSMState>(); // pushed state
        protected FSMState _currentState;
      
        public void SpawnEvent(int eventName)
        {
            foreach (var t in _transitions)
            {
                if (t.transition == eventName)
                {
                    if (t.from == null || t.from == _currentState)
                    {
                        Switch(t.to);
                        return;
                    }
                }
            }

            foreach (var t in _popupTransitions)
            {
                if (t.transition == eventName)
                {
                    if (t.from == null || t.from == _currentState)
                    {
                        Push(t.to);
                        return;
                    }

                    if (t.to == _currentState)
                    {
                        Pop();
                        return;
                    }
                }
            }

            Debug.LogError("Unit FSM: Event " + eventName + " not handled");
        }


        public void Tick()
        {
            _currentState?.Tick();
        }

        public void RegisterState(FSMState state)
        {
            States.Add(state);
        }

        protected void AddGlobalTransition(int transition, FSMState to, FSMState[] exclude = null)
        {
            foreach (var s in States)
            {
                if (exclude == null || Array.IndexOf(exclude, s) == -1)
                    _transitions.Add(new FSMTransition(transition, s, to));
            }
        }

        protected void AddTransition(int transition, FSMState stateFrom, FSMState to)
        {
            _transitions.Add(new FSMTransition(transition, stateFrom, to));
        }

        protected void AddGlobalPopupTransition(int transition, FSMState to, FSMState[] exclude = null)
        {
            foreach (var s in States)
            {
                if (exclude == null || Array.IndexOf(exclude, s) == -1)
                    _popupTransitions.Add(new FSMTransition(transition, s, to));
            }
        }

        protected void AddPopupTransition(int transition, FSMState stateFrom, FSMState stateTo)
        {
            _popupTransitions.Add(new FSMTransition(transition, stateFrom, stateTo));
        }

        protected void FlushToTop()
        {
            _nextStateToPush = null;
            _popQueue = 0;
            while (_queue.Count > 0) 
                Pop();
        }

        protected void Switch(FSMState to)
        {
            if (_isSwitching)
            {
                if (_nextState != null)
                    Debug.LogError("Too many state switches");
                else
                    _nextState = to;
                return;
            }
        
            _nextStateToPush = null;
            _popQueue = 0;
            
            while (_queue.Count > 0)
                Pop();

            _isSwitching = true;
            _currentState?.Exit();

            _currentState = to;
            _currentState.Enter();

            SwitchingFinish();
        }

        public T Push<T>() where T : FSMState
        {
            var state = State<T>();
            Push(state);
            return state;
        }

        public T State<T>() where T : FSMState
        {
            foreach (var state in States)
            {
                if (state is T)
                    return (T)state;
            }

            var resolverState = ResolveState(typeof(T));
            if (resolverState != null)
                return (T)resolverState;

            Debug.LogError($"Can't find state {typeof(T)}");
            return null;
        }

        protected virtual FSMState ResolveState(Type type)
        {
            return null;
        }


        public void Push(FSMState to)
        {
            if (_isSwitching)
            {
                if (_nextStateToPush != null)
                    Debug.LogError("Too many state switches");
                else
                    _nextStateToPush = to;
                return;
            }
        
            _isSwitching = true;
            if (_currentState == to)
            {
                _currentState.Exit();
                _currentState.Enter();
            }
            else
            {
                _currentState?.Pause();

                _queue.Push(_currentState);
                _currentState = to;
                _currentState.Enter();
            }

            SwitchingFinish();
        }

        private void SwitchingFinish()
        {
            _isSwitching = false;

            if (_nextState != null)
            {
                var temp = _nextState;
                _nextState = null;
                Switch(temp);
                return;
            }
        
            if (_popQueue > 0)
            {
                while (_popQueue > 0)
                {
                    _popQueue--;
                    Pop();
                }
                return;
            }
        
            if (_nextStateToPush != null)
            {
                var temp = _nextStateToPush;
                _nextStateToPush = null;
                Push(temp);
            }
        }

        public void Pop()
        {
            if (_isSwitching)
            {
                if (_popQueue > 0)
                    Debug.LogError("Too many state switches");
                else
                    _popQueue++;

                return;
            }
        
            if (_queue.Count == 0)
            {
                Debug.LogError("Can't pop state. Current state is root state");
                return;
            }
        
            _isSwitching = true;
            _currentState?.Exit();

            if (_queue.Count == 0)
                throw new Exception("_queue is empty");

            _currentState = _queue.Pop();
            _currentState.Resume();
            _isSwitching = false;

            SwitchingFinish();
        }

        public void PopThis(FSMState state)
        {
            if (_currentState != state)
            {
                Debug.LogError($"pop this error {state} -> {_currentState}");
                return;
            }

            Pop();
        }

        private class FSMTransition
        {
            public readonly FSMState from;
            public readonly FSMState to;
            public readonly int transition;

            public FSMTransition(int transition, FSMState from, FSMState to)
            {
                this.transition = transition;
                this.from = from;
                this.to = to;
            }
        }
    }
}