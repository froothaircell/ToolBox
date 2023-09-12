using System;
using ToolBox.Handlers.EventHandler;
using ToolBox.StateMachine;
using ToolBox.Utils;
using ToolBox.Utils.Disposables;

namespace ToolBox.Mediators
{
    public abstract class MenuMediator<TMenuMediator, TMenuStateMachine, TMenuView> : StateMachineMediator<TMenuMediator, TMenuStateMachine> 
        where TMenuMediator : MenuMediator<TMenuMediator, TMenuStateMachine, TMenuView>
        where TMenuStateMachine : StateMachineHistory, new()
        where TMenuView : MenuView<TMenuView>
    {
        public TMenuView View;

        public virtual void OnEnter(REvent evt)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            OnEnterMenu();
        }

        public virtual void OnExit(REvent evt)
        {
            OnExitMenu();
            transform.GetChild(0).gameObject.SetActive(false);
        }

        public override void CleanSingleton()
        {
            base.CleanSingleton();
            View.RemoveAllListeners();
            if (_disposables != null)
            {
                _disposables.ClearDisposables();
                _disposables.ReturnToPool();
            }
        }

        public abstract void SubscribeToEvents();
        
        public abstract void OnEnterMenu();
        public abstract void OnExitMenu();
        
    }
}