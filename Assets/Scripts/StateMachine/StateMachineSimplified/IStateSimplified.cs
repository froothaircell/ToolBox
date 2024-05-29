namespace ToolBox.StateMachine.Simplified
{
    public interface IStateSimplified
    {
        void OnEnter();
        void OnUpdate();  // Think about adding fixed update if you want that functionality
        void OnExit();
    }
}