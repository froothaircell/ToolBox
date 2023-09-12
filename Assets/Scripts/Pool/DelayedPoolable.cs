using System;
using ToolBox.Promise;
using ToolBox.Utils;
using ToolBox.Utils.Jobs;
using UnityEditor;
using UnityEngine;

namespace ToolBox.Pool
{
    public class DelayedPoolable : Poolable
    {
        private int _delayDespawnCount;

        private Action _onRemoveDelayDespawn;
        private Action<Exception> _onRemoveDelayDespawnException;
        private Action _finalReturnToPool;

#if UNITY_EDITOR
        private int _delayedReturnCount = -1;
#endif
        
        protected override void OnSpawn()
        {
            _delayDespawnCount = 0;
        }

        protected override void OnDespawn()
        {
            _delayDespawnCount = 0;
        }

        public void AddDelayDespawn()
        {
            _delayDespawnCount++;
        }

        public virtual void DelayDespawn(IPromise promise)
        {
            if (promise == null || promise.State == PromiseState.Pending)
            {
                return;
            }

            _delayDespawnCount++;

            if (_onRemoveDelayDespawn == null)
            {
                _onRemoveDelayDespawn = RemoveDelayDespawn;
            }
            
            if (_onRemoveDelayDespawnException == null)
            {
                _onRemoveDelayDespawnException = RemoveDelayDespawn;
            }

            promise.Then(_onRemoveDelayDespawn, _onRemoveDelayDespawnException);
        }

        private void RemoveDelayDespawn(Exception exception)
        {
            RemoveDelayDespawn();
        }

        private void RemoveDelayDespawn()
        {
            _delayDespawnCount--;
            ReturnToPool();
        }

        public override void ReturnToPool()
        {
            if (_delayDespawnCount <= 0)
            {
                if (HasPool || !IsPooled)
                {
                    if (_finalReturnToPool == null)
                    {
                        _finalReturnToPool = FinalReturnToPool;
                    }
#if UNITY_EDITOR
                    if (Application.isPlaying)
                    {
                        ServiceLocator.Get<JobManager>().ExecuteMainThread(_finalReturnToPool);
                    }
                    else
                    {
                        if (_delayedReturnCount < 0)
                        {
                            _delayedReturnCount = 1;
                            EditorApplication.update += DelayedEditorPoolReturn;
                        }
                    }

#else
                    AppHandler.JobHandler.ExecuteMainThread(_finalReturnToPool);
#endif
                }
            }
        }
#if UNITY_EDITOR
        private void DelayedEditorPoolReturn()
        {
            _delayedReturnCount--;
            if (_delayedReturnCount < 0)
            {
                FinalReturnToPool();

                EditorApplication.update -= DelayedEditorPoolReturn;
            }
        }
#endif
        private void FinalReturnToPool()
        {
            if (_delayDespawnCount > 0)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"DelayedPoolable | {GetType().Name} attempted to return itself but something delayed it");
#endif
                return;
            }
            
            base.ReturnToPool();
        }
    }
}