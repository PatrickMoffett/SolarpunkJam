using System;
using UnityEngine;

namespace Services
{
    /// <summary>
    /// The MonoBehaviourService class provides a convenient way to expose the behavior of Unity's MonoBehaviour class
    /// to classes that do not inherit from it. Since many of Unity's features are built around the MonoBehaviour class,
    /// it can be limiting for certain classes that need access to its functionality to be forced to inherit from it.
    /// <br/><br/>
    /// By using the MonoBehaviourService class, classes can subscribe to events that will be invoked whenever a
    /// MonoBehaviour function is called, such as Awake(), Start(), Update(), LateUpdate(), FixedUpdate(),
    /// and OnDestroy(). This allows those classes to respond to these events and utilize the functionality of
    /// Unity's MonoBehaviour class without being forced to inherit from it.
    /// <br/><br/>
    ///For example, a class that manages the game's audio might want to respond to the OnDestroy() event to properly
    /// dispose of its resources when the game is closed. Without the MonoBehaviourService class, this class would
    /// need to inherit from MonoBehaviour to get access to the OnDestroy() function. By using the MonoBehaviourService
    /// class, the audio manager can subscribe to the OnDestroyEvent and properly dispose of its resources without
    /// having to inherit from MonoBehaviour.
    /// <br/><br/>
    /// Overall, the MonoBehaviourService class provides a useful tool for allowing classes to utilize the
    /// functionality of Unity's MonoBehaviour class without being forced to inherit from it. This can make code more
    /// flexible and easier to manage, while still providing access to the powerful features of Unity's game engine.
    /// 
    /// </summary>
    public class MonoBehaviorService : MonoBehaviour, IService
    {
        public event Action AwakeEvent;
        public event Action StartEvent;
        public event Action UpdateEvent;
        public event Action LateUpdateEvent;
        public event Action FixedUpdateEvent;
        private void Awake()
        {
            AwakeEvent?.Invoke();
        }

        private void Start()
        {
            StartEvent?.Invoke();
        }

        private void Update()
        {
            UpdateEvent?.Invoke();
        }

        private void LateUpdate()
        {
            LateUpdateEvent?.Invoke();
        }

        private void FixedUpdate()
        {
            FixedUpdateEvent?.Invoke();
        }
        
    }
}
