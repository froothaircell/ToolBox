using System;
using ToolBox.Utils;

namespace ToolBox.Promise
{
    public partial class RPromise
    {
        public static RPromise Get()
        {
            RPromise promise = ServiceLocator.AppPool.Get<RPromise>();
            return promise;
        }

        public static RPromise<T> Get<T>()
        {
            RPromise<T> promise = ServiceLocator.AppPool.Get<RPromise<T>>();
            return promise;
        }

        public static RPromise Resolved()
        {
            RPromise promise = ServiceLocator.AppPool.Get<RPromise>();
            promise.Resolve();
            return promise;
        }

        public static RPromise Rejected(Exception exception)
        {
            RPromise promise = ServiceLocator.AppPool.Get<RPromise>();
            promise.Reject(exception);
            return promise;
        }

        public static IPromise<T> Resolved<T>(T resolveValue)
        {
            RPromise<T> promise = ServiceLocator.AppPool.Get<RPromise<T>>();
            promise.Resolve(resolveValue);
            return promise;
        }

        public static IPromise<T> Rejected<T>(Exception exception)
        {
            RPromise<T> promise = ServiceLocator.AppPool.Get<RPromise<T>>();
            promise.Reject(exception);
            return promise;
        }

        public static void SafeResolve(ref RPromise promise)
        {
            InternalSafePromiseBehaviour(ref promise)?.Resolve();
        }

        public static void SafeReject(ref RPromise promise, Exception exception)
        {
            InternalSafePromiseBehaviour(ref promise)?.Reject(exception);
        }
        
        public static void SafeResolve<T>(ref RPromise<T> promise, T resolveValue)
        {
            InternalSafePromiseBehaviour(ref promise)?.Resolve(resolveValue);
        }
        
        public static void SafeReject<T>(ref RPromise<T> promise, Exception exception)
        {
            InternalSafePromiseBehaviour(ref promise)?.Reject(exception);
        }

        private static RPromise InternalSafePromiseBehaviour(ref RPromise promise)
        {
            if (promise == null) return null;

            if (promise.State != PromiseState.Pending)
            {
                promise = null;
                return null;
            }

            RPromise tmpPromise = promise;
            promise = null;
            return tmpPromise;
        }

        private static RPromise<T> InternalSafePromiseBehaviour<T>(ref RPromise<T> promise)
        {
            if (promise == null) return null;

            if (promise.State != PromiseState.Pending)
            {
                promise = null;
                return null;
            }

            RPromise<T> tmpPromise = promise;
            promise = null;
            return tmpPromise;
        }
    }
}