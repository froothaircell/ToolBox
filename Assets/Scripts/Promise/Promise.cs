using System;
using System.Collections.Generic;
using UnityEngine;

namespace ToolBox.Promise
{
    public partial class RPromise : PoolablePromise, IPromise
    {
        private List<Action> _resolveHandlers;

        public void Resolve()
        {
            if (State != PromiseState.Pending)
                throw RPromiseException.NonPending($"Resolve {typeof(RPromise).FullName}", State);
            
            InternalReportProgress(1f);

            State = PromiseState.Resolved;

            if (_resolveHandlers == null)
            {
                ClearHandlers();
                ReturnToPool();
                return;
            }

            for (int i = 0; i < _resolveHandlers.Count; i++)
            {
                try
                {
                    _resolveHandlers[i].Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            ClearHandlers();
            ReturnToPool();
        }
        
        public IPromise Then(Action onResolved)
        {
            if (onResolved == null) throw new ArgumentNullException(nameof(onResolved));
            
            AddResolveHandler(onResolved);

            return this;
        }

        public IPromise Then(Action<Exception> onRejected)
        {
            if (onRejected == null) throw new ArgumentNullException(nameof(onRejected));
            
            AddRejectHandler(onRejected);

            return this;
        }

        public IPromise Then(Action onResolved, Action<Exception> onRejected)
        {
            if (onResolved == null) throw new ArgumentNullException(nameof(onResolved));
            if (onRejected == null) throw new ArgumentNullException(nameof(onRejected));
            
            AddResolveHandler(onResolved);
            AddRejectHandler(onRejected);

            return this;
        }

        public IPromise Then(Action onResolved, Action<Exception> onRejected, Action<float> onProgress)
        {
            if (onResolved == null) throw new ArgumentNullException(nameof(onResolved));
            if (onRejected == null) throw new ArgumentNullException(nameof(onRejected));
            if (onProgress == null) throw new ArgumentNullException(nameof(onProgress));
            
            // Add progress handler first to
            // avoid registering progress of
            // chained promises after the
            // chained resolution
            AddProgressHandler(onProgress);
            AddResolveHandler(onResolved);
            AddRejectHandler(onRejected);

            return this;
        }

        public IPromise Then(RPromise onReactPromise)
        {
            if (onReactPromise == null) throw new ArgumentNullException(nameof(onReactPromise));
            
            // Add progress handler first to
            // avoid registering progress of
            // chained promises after the
            // chained resolution
            AddProgressHandler(onReactPromise.ReportProgress);
            AddResolveHandler(onReactPromise.Resolve);
            AddRejectHandler(onReactPromise.Reject);

            return this;
        }

        private void AddResolveHandler(Action onResolved)
        {
            switch (State)
            {
                case PromiseState.Pending:
                    if (_resolveHandlers == null)
                    {
                        _resolveHandlers = new List<Action>();
                    }

                    _resolveHandlers.Add(onResolved);
                    break;
                case PromiseState.Resolved:
                    onResolved.Invoke();
                    break;
                case PromiseState.Rejected:
                    break;
                case PromiseState.Pooled:
                    throw RPromiseException.PooledInteraction("AddResolveHandler", this);
                
            }
        }

        protected override void ClearHandlers()
        {
            base.ClearHandlers();
            _resolveHandlers?.Clear();
        }
    }

    public class RPromise<T> : PoolablePromise, IPromise<T>
    {
        private T _resolveValue;
        
        private List<Action<T>> _resolveHandlers;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            _resolveValue = default(T);
        }

        public void Resolve(T resolveValue)
        {
            if (State != PromiseState.Pending) throw RPromiseException.NonPending("Resolve", State);
            
            InternalReportProgress(1f);

            State = PromiseState.Resolved;
            _resolveValue = resolveValue;

            if (_resolveHandlers == null)
            {
                ClearHandlers();
                ReturnToPool();
                return;
            }

            for (int i = 0; i < _resolveHandlers.Count; i++)
            {
                try
                {
                    _resolveHandlers[i].Invoke(_resolveValue);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            
            ClearHandlers();
            ReturnToPool();
        }
        
        public T GetResolveValue()
        {
            return _resolveValue;
        }
        
        public IPromise Then(Action onResolved)
        {
            if (onResolved == null) throw new ArgumentNullException(nameof(onResolved));

            AddResolveHandler((resolveValue) => onResolved());

            return this;
        }

        public IPromise Then(Action<Exception> onRejected)
        {
            if (onRejected == null) throw new ArgumentNullException(nameof(onRejected));

            AddRejectHandler(onRejected);

            return this;
        }

        public IPromise Then(Action onResolved, Action<Exception> onRejected)
        {
            if (onResolved == null) throw new ArgumentNullException(nameof(onResolved));
            if (onRejected == null) throw new ArgumentNullException(nameof(onRejected));

            AddResolveHandler((resolveValue) => onResolved());
            AddRejectHandler(onRejected);

            return this;
        }

        public IPromise Then(Action onResolved, Action<Exception> onRejected, Action<float> onProgress)
        {
            if (onResolved == null) throw new ArgumentNullException(nameof(onResolved));
            if (onRejected == null) throw new ArgumentNullException(nameof(onRejected));
            if (onProgress == null) throw new ArgumentNullException(nameof(onProgress));

            // Add progress handler first to
            // avoid registering progress of
            // chained promises after the
            // chained resolution
            AddProgressHandler(onProgress);
            AddResolveHandler((resolveValue) => onResolved());
            AddRejectHandler(onRejected);

            return this;
        }

        public IPromise Then(RPromise onReactPromise)
        {
            if (onReactPromise == null) throw new ArgumentNullException(nameof(onReactPromise));

            // Add progress handler first to
            // avoid registering progress of
            // chained promises after the
            // chained resolution
            AddProgressHandler(onReactPromise.ReportProgress);
            AddResolveHandler((resolveValue) => onReactPromise.Resolve());
            AddRejectHandler(onReactPromise.Reject);

            return this;
        }

        public IPromise<T> Then(Action<T> onResolved)
        {
            if (onResolved == null) throw new ArgumentNullException(nameof(onResolved));

            AddResolveHandler(onResolved);

            return this;
        }

        public IPromise<T> Then(Action<T> onResolved, Action<Exception> onRejected)
        {
            if (onResolved == null) throw new ArgumentNullException(nameof(onResolved));
            if (onRejected == null) throw new ArgumentNullException(nameof(onRejected));

            AddResolveHandler(onResolved);
            AddRejectHandler(onRejected);

            return this;
        }

        public IPromise<T> Then(Action<T> onResolved, Action<Exception> onRejected, Action<float> onProgress)
        {
            if (onResolved == null) throw new ArgumentNullException(nameof(onResolved));
            if (onRejected == null) throw new ArgumentNullException(nameof(onRejected));
            if (onProgress == null) throw new ArgumentNullException(nameof(onProgress));

            // Add progress handler first to
            // avoid registering progress of
            // chained promises after the
            // chained resolution
            AddProgressHandler(onProgress);
            AddResolveHandler(onResolved);
            AddRejectHandler(onRejected);

            return this;
        }

        public IPromise<T> Then(RPromise<T> onReactPromise)
        {
            if (onReactPromise == null) throw new ArgumentNullException(nameof(onReactPromise));

            // Add progress handler first to
            // avoid registering progress of
            // chained promises after the
            // chained resolution
            AddProgressHandler(onReactPromise.ReportProgress);
            AddResolveHandler(onReactPromise.Resolve);
            AddRejectHandler(onReactPromise.Reject);

            return this;
        }
        
        private void AddResolveHandler(Action<T> onResolved)
        {
            switch (State)
            {
                case PromiseState.Pending:
                    if (_resolveHandlers == null)
                    {
                        _resolveHandlers = new List<Action<T>>();
                    }
                    _resolveHandlers.Add(onResolved);
                    break;
                case PromiseState.Resolved:
                    onResolved.Invoke(_resolveValue);
                    break;
                case PromiseState.Rejected:
                    break;
                case PromiseState.Pooled:
                    throw RPromiseException.PooledInteraction("AddResolveHandler", this);
            }
        }

        protected override void ClearHandlers()
        {
            base.ClearHandlers();
            _resolveHandlers?.Clear();
        }
    }
}