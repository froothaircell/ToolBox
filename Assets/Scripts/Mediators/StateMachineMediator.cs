using ToolBox.StateMachine;

namespace ToolBox.Mediators
{
    public class StateMachineMediator<TMediator, TStateMachine> : Mediator<TMediator> 
        where TMediator : StateMachineMediator<TMediator, TStateMachine>
        where TStateMachine : StateMachineHistory, new()
    {
        private TStateMachine _fsm;

        public TStateMachine FSM
        {
            get { return _fsm ?? (_fsm = new TStateMachine()); }
        }
    }
}