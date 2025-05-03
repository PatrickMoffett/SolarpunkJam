using System;
using UnityEngine;

public class CollisionObserver2D : MonoBehaviour
{
    [SerializeField] private bool _broadcastOnCollisionEnter = true;
    [SerializeField] private bool _broadcastOnCollisionExit = true;
    [SerializeField] private bool _broadcastOnTriggerEnter = true;
    [SerializeField] private bool _broadcastOnTriggerExit = true;

    public event Action<Collision2D> OnCollisionEnter;
    public event Action<Collision2D> OnCollisionExit;
    public event Action<Collider2D> OnTriggerEnter;
    public event Action<Collider2D> OnTriggerExit;

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
            OnCollisionEnter?.Invoke(collision);
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        _collisionCount--;
        if (_broadcastOnCollisionExit)
        {
            OnCollisionExit?.Invoke(collision);
        }
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        _currentTriggerCount++;
        if (_broadcastOnTriggerEnter)
        {
            OnTriggerEnter?.Invoke(collider);
        }
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        _currentTriggerCount--;
        if (_broadcastOnTriggerExit)
        {
            OnTriggerExit?.Invoke(collider);
        }
    }
}