using System;
using UnityEngine;

public class CollisionObserver2D : MonoBehaviour
{
    [SerializeField] private bool _broadcastOnCollisionEnter = true;
    [SerializeField] private bool _broadcastOnCollisionExit = true;
    [SerializeField] private bool _broadcastOnTriggerEnter = true;
    [SerializeField] private bool _broadcastOnTriggerExit = true;

    public event Action OnCollisionEnter;
    public event Action OnCollisionExit;
    public event Action OnTriggerEnter;
    public event Action OnTriggerExit;

    private int _collisionCount = 0;
    private int _currentTriggerCount = 0;
    public bool IsCurrentlyColliding()
    {
        return _collisionCount > 0;
    }
    public bool IsCurrentlyTriggering()
    {
        return _currentTriggerCount > 0;
    }
    public int GetCollisionCount()
    {
        return _collisionCount;
    }
    public int GetTriggerCount()
    {
        return _currentTriggerCount;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        _collisionCount++;
        if (_broadcastOnCollisionEnter)
        {
            OnCollisionEnter?.Invoke();
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        _collisionCount--;
        if (_broadcastOnCollisionExit)
        {
            OnCollisionExit?.Invoke();
        }
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        _currentTriggerCount++;
        if (_broadcastOnTriggerEnter)
        {
            OnTriggerEnter?.Invoke();
        }
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        _currentTriggerCount--;
        if (_broadcastOnTriggerExit)
        {
            OnTriggerExit?.Invoke();
        }
    }
}