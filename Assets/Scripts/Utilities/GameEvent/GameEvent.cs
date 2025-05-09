// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   10/04/17
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameEvent : ScriptableObject
{
    /// <summary>
    /// The list of listeners that this event will notify if it is raised.
    /// </summary>
    private readonly List<GameEventListener> _eventListeners = 
        new List<GameEventListener>();
    public event Action OnGameEvent;
    public void Raise()
    {
        OnGameEvent?.Invoke();
        for (int i = _eventListeners.Count -1; i >= 0; i--)
            _eventListeners[i].OnEventRaised();
    }

    public void RegisterListener(GameEventListener listener)
    {
        if (!_eventListeners.Contains(listener))
            _eventListeners.Add(listener);
    }

    public void UnregisterListener(GameEventListener listener)
    {
        if (_eventListeners.Contains(listener))
            _eventListeners.Remove(listener);
    }
}

