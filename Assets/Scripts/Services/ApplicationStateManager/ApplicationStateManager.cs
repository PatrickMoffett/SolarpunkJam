using StackStateMachine;


namespace Services
{
    //For implementation details see StateManager
    
    /// <summary>
    /// Used to manage the overall state of the application
    /// </summary>
    public class ApplicationStateManager : StackStateMachine<BaseApplicationState>, IService { }
}
