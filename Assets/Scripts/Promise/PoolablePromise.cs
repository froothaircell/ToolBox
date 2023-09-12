using System;
using ToolBox.Pool;
using ToolBox.Utils;
using UnityEngine;

namespace ToolBox.Promise
{
    public class PoolablePromise : DelayedPoolable
    {
        #region Private Fields
        private float _progressPerc;

        private PooledList<Action<float>> _progressHandlers;
        private PooledList<Action<Exception>> _rejectHandlers;
        private int _rejectHandlerTotal;
        #endregion

        #region Public Properties
        public PromiseState State { get; protected set; }
        public Exception RejectException { get; private set; }
        #endregion

        #region Overrides
        protected override void OnSpawn()
        {
            base.OnSpawn();
            State = PromiseState.Pending;
            RejectException = null;
            _progressPerc = 0f;
            _rejectHandlerTotal = 0;
        }

        protected override void OnDespawn()
        {
            if (State == PromiseState.Rejected && _rejectHandlerTotal <= 0)
            {
                if (RejectException == null)
                {
                    Debug.Log("Promise rejected with null");
                }
                else
                {
                    Debug.LogWarning(RejectException.ToString());
                }
            }

            State = PromiseState.Pooled;

            RejectException = null;
            
            _progressPerc = 0f;

            if (_rejectHandlers != null)
            {
                _rejectHandlers.ReturnToPool();
                _rejectHandlers = null;
            }

            if (_progressHandlers != null)
            {
                _progressHandlers.ReturnToPool();
                _progressHandlers = null;
            }
        }

        public override void ReturnToPool()
        {
            if (State != PromiseState.Pending)
            {
                base.ReturnToPool();
            }
        }
        #endregion

        #region Public Methods
        public void ReportProgress(float progressPerc)
        {
            if (State != PromiseState.Pending) throw RPromiseException.NonPending("ReportProgress", State);
            
            if (progressPerc > 1f) progressPerc = 1f;
            
            InternalReportProgress(progressPerc);
        }

        public void Reject(Exception exception)
        {
            if (State != PromiseState.Pending) throw RPromiseException.NonPending("Reject", State);

            State = PromiseState.Rejected;
            RejectException = exception;

            if (_rejectHandlers == null)
            {
                ClearHandlers();
                ReturnToPool();
                return;
            }

            for (int i = 0; i < _rejectHandlers.Count; i++)
            {
                try
                {
                    _rejectHandlers[i].Invoke(RejectException);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            ClearHandlers();
            ReturnToPool();
        }
        #endregion

        #region Internal Methods
        protected void InternalReportProgress(float progressPerc)
        {
            if (progressPerc <= _progressPerc) return;

            _progressPerc = progressPerc;

            if (_progressHandlers == null) return;

            for (int i = 0; i < _progressHandlers.Count; i++)
            {
                try
                {
                    _progressHandlers[i].Invoke(_progressPerc);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        protected void AddProgressHandler(Action<float> onProgress)
        {
            switch (State)
            {
                case PromiseState.Pending:
                    if (_progressHandlers == null)
                    {
                        _progressHandlers = ServiceLocator.AppPool.Get<PooledList<Action<float>>>();
                    }
                    _progressHandlers.Add(onProgress);
                    onProgress.Invoke(_progressPerc);
                    break;
                case PromiseState.Resolved: 
                case PromiseState.Rejected:
                    onProgress.Invoke(1f);
                    break;
                case PromiseState.Pooled:
                    throw RPromiseException.PooledInteraction("AddProgressHandler", this);
            }
        }

        protected void AddRejectHandler(Action<Exception> onRejected)
        {
            _rejectHandlerTotal++;

            switch (State)
            {
                case PromiseState.Pending:
                    if (_rejectHandlers == null)
                    {
                        _rejectHandlers = ServiceLocator.AppPool.Get<PooledList<Action<Exception>>>();
                    }
                    _rejectHandlers.Add(onRejected);
                    break;
                case PromiseState.Resolved:
                    break;
                case PromiseState.Rejected:
                    onRejected.Invoke(RejectException);
                    break;
                case PromiseState.Pooled:
                    throw RPromiseException.PooledInteraction("AddRejectHandler", this);
            }
        }

        protected virtual void ClearHandlers()
        {
            // Do not clear _rejectHandlerTotal here, it represents the promise having at any point handled the rejection
            
            _progressHandlers?.Clear();
            _rejectHandlers?.Clear();
        }
        #endregion
    }
}