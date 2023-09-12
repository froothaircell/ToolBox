using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ToolBox.Mediators;
using ToolBox.Pool;
using ToolBox.Promise;
using JetBrains.Annotations;
using UnityEngine;
using ToolBox.Utils.Singleton;

namespace ToolBox.Utils.Jobs
{
    public class JobManager : NonMonoSingleton<JobManager>
    {
        public static TypePool JobPool = new TypePool("Job Pool");

        private JobManagerBehavior _managerBehavior;

        public override void InitSingleton()
        {
            GameObject mediatorGO = new GameObject();
            _managerBehavior = mediatorGO.AddComponent<JobManagerBehavior>();
            mediatorGO.name = _managerBehavior.Name;

            _managerBehavior.Initialize();
        }

        public override void CleanSingleton()
        {
            if (_managerBehavior != null)
            {
                _managerBehavior.Clean();
                UnityEngine.Object.Destroy(_managerBehavior.gameObject);
                _managerBehavior = null;
            }
        }

        public virtual void ExecuteMainThread(Action action)
        {
            _managerBehavior.AddMainThreadJob(action);
        }

        public virtual UpdateJob ExecuteCoroutine(IEnumerator coroutine)
        {
            return GetUpdateJob(
                _managerBehavior.CreateCoroutineJob(coroutine).ExecutePromise);
        }

        protected static UpdateJob GetUpdateJob(RPromise jobPromise)
        {
            UpdateJob instance = JobPool.Get<UpdateJob>();
            instance.SetPromise(jobPromise);
            return instance;
        }

        public static void SafeStopUpdate(ref UpdateJob updateJob)
        {
            if (updateJob != null)
            {
                UpdateJob tmpUpdate = updateJob;
                updateJob = null;
                tmpUpdate.StopUpdate();
            }
        }
    }

    public class JobManagerBehavior : Mediator<JobManagerBehavior>
    {
        public bool PauseOnNewLateAction;

        private int _threadSafeQueuePauseValue = 0;

        public bool QueuePause
        {
            get
            {
                return (Interlocked.CompareExchange(ref _threadSafeQueuePauseValue, 1, 1) == 1);
            }
            set
            {
                if (value) Interlocked.CompareExchange(ref _threadSafeQueuePauseValue, 1, 0);
                else Interlocked.CompareExchange(ref _threadSafeQueuePauseValue, 0, 1);
            }
        }
        
        private ConcurrentQueue<Action> _lateActions;
        public ConcurrentQueue<Action> LateActions => _lateActions;

        private Queue<CoroutineJob> _coroutineJobs;
        public Queue<CoroutineJob> CoroutineJobs => _coroutineJobs;

        public override void Initialize()
        {
            if (_lateActions == null)
            {
                _lateActions = new ConcurrentQueue<Action>();
            }
            
            if (_coroutineJobs == null)
            {
                _coroutineJobs = new Queue<CoroutineJob>(32);
            }
            
            Debug.Log("Initialized");
        }

        public override void Clean()
        {
            if (_coroutineJobs != null)
            {
                while (_coroutineJobs.Count > 0)
                {
                    CleanJob(_coroutineJobs.Dequeue());
                }
            }
        }
        
        private void CleanJob(PoolableJob job)
        {
            if (job == null)
            {
                return;
            }

            if (!job.Finished)
            {
                job.ExecutePromise.Reject(
                    new Exception("JobManager | Cleaning a manager that has active jobs"));
            }
            
            job.ReturnToPool();
        }

        public void AddMainThreadJob(Action action)
        {
            _lateActions.Enqueue(action);

#if UNITY_EDITOR
            if (PauseOnNewLateAction)
            {
                QueuePause = true;
            }
#endif
        }

        public CoroutineJob CreateCoroutineJob(IEnumerator coroutineEnumerator)
        {
            CoroutineJob job = CoroutineJob.Get(coroutineEnumerator);
            _coroutineJobs.Enqueue(job);
            return job;
        }

        private void LateUpdate()
        {
            int lateActionCount = _lateActions.Count;
            while (lateActionCount > 0)
            {
                if (_lateActions.TryDequeue(out Action mainThreadAction))
                {
                    mainThreadAction.Invoke();
                    lateActionCount--;
                }
                else
                {
                    break;
                }
            }
            
            while (_coroutineJobs.Count > 0)
            {
                StartCoroutineJob(_coroutineJobs.Dequeue());
            }
        }

        private void StartCoroutineJob([NotNull] CoroutineJob coroutineJob)
        {
            if (coroutineJob == null) throw new ArgumentNullException(nameof(coroutineJob));

            if (coroutineJob.Finished)
            {
                coroutineJob.ReturnToPool();
                return;
            }

            Coroutine coroutine = StartCoroutine(coroutineJob.CoroutineEnumerator);
            if (coroutine == null)
            {
                if (coroutineJob.ExecutePromise != null)
                {
                    coroutineJob.ExecutePromise.Reject(new Exception("JobManager | Failed to start coroutine"));
                }
                
                coroutineJob.ReturnToPool();
                return;
            }

            ListenCoroutineJob(coroutineJob, coroutine);
        }

        private void ListenCoroutineJob(CoroutineJob coroutineJob, Coroutine coroutine)
        {
            if (coroutineJob.ExecutePromise != null)
            {
                void OnJobResolved() => StopCoroutineJob(coroutineJob, coroutine);
                void OnJobRejected(Exception exception) => OnJobResolved();

                coroutineJob.ExecutePromise.Then(OnJobResolved, OnJobRejected);
            }
            else
            {
                StopCoroutineJob(coroutineJob, coroutine);
            }
        }

        private void StopCoroutineJob(CoroutineJob coroutineJob, Coroutine coroutine)
        {
            coroutineJob.ReturnToPool();

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
    }
}