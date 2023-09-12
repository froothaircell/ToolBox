﻿namespace ToolBox.StateMachine
{
    public interface IState
    {
        void OnEnter();
        void OnUpdate();  // Think about adding fixed update if you want that functionality
        void OnExit();
    }
}