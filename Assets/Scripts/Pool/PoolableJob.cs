using System;
using ToolBox.Promise;

namespace ToolBox.Pool
{
    public abstract class PoolableJob : Poolable
    {
        public RPromise ExecutePromise { get; private set; }

        public bool Finished => ExecutePromise == null || ExecutePromise.State != PromiseState.Pending;

        private Action _onExecute;
        private Action<Exception> _onCancelExecute;

        protected override void OnSpawn()
        {
            
        }

        protected void OnSpawnInit()
        {
            ExecutePromise = RPromise.Get();

            if (_onExecute == null)
            {
                _onExecute = InternalExecute;
            }

            if (_onCancelExecute == null)
            {
                _onCancelExecute = InternalCancelExecute;
            }

            ExecutePromise.Then(
                _onExecute,
                _onCancelExecute);
        }

        protected override void OnDespawn()
        {
            if (ExecutePromise != null)
            {
                if (ExecutePromise.State == PromiseState.Pending)
                {
                    ExecutePromise.Resolve();
                }

                ExecutePromise = null;
            }
        }

        private void InternalExecute()
        {
            ExecutePromise = null;
            Execute();
        }

        private void InternalCancelExecute(Exception exception)
        {
            ExecutePromise = null;
            CancelExecute(exception);
        }

        protected abstract void Execute();
        protected abstract void CancelExecute(Exception exception);
    }
}