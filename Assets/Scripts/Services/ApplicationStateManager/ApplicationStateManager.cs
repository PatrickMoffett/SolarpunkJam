using System;
using StateManager;
using UnityEditor;


namespace Services
{
    //For implementation details see StateManager
    
    /// <summary>
    /// Used to manage the overall state of the application
    /// </summary>
    public class ApplicationStateManager : StateManager<BaseApplicationState>, IService { }
}
