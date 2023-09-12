using System;
using ToolBox.Pool;

namespace ToolBox.Promise
{
    public class JobPromise : DelayedPoolable
    {
        protected RPromise _jobPromise;

        private Action _promiseResolveDelegate;
        private Action<Exception> _promiseRejectedDelegate;

        internal void SetPromise(RPromise jobPromise)
        {
            _jobPromise = jobPromise;

            if (_jobPromise != null)
            {
                if (_promiseResolveDelegate == null)
                {
                    _promiseResolveDelegate = CleanOnPromiseFinished;
                }

                if (_promiseRejectedDelegate == null)
                {
                    _promiseRejectedDelegate = CleanOnPromiseFinished;
                }
                
                base.DelayDespawn(_jobPromise);
                _jobPromise.Then(
                    _promiseResolveDelegate,
                    _promiseRejectedDelegate);
            }
        }

        private void CleanOnPromiseFinished(Exception e)
        {
            CleanOnPromiseFinished();
        }

        private void CleanOnPromiseFinished()
        {
            _jobPromise = null;
            ReturnToPool();
        }

        public override void DelayDespawn(IPromise promise)
        {
            base.DelayDespawn(promise);

            _jobPromise?.DelayDespawn(promise);
        }
    }
}