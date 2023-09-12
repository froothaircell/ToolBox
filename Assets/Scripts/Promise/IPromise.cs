using System;
using JetBrains.Annotations;

namespace ToolBox.Promise
{
    public enum PromiseState
    {
        Pooled = 0,
        Pending = 1,
        Rejected = 2,
        Resolved = 3
    }
    
    public interface IPromise
    {
        PromiseState State { get; }
        Exception RejectException { get; }

        IPromise Then([NotNull] Action onResolved);
        
        IPromise Then([NotNull] Action<Exception> onRejected);

        IPromise Then([NotNull] Action onResolved, [NotNull] Action<Exception> onRejected);
        
        IPromise Then([NotNull] Action onResolved, [NotNull] Action<Exception> onRejected, [NotNull] Action<float> onProgress);

        IPromise Then([NotNull] RPromise onReactPromise);
    }
    
    public interface IPromise<T> : IPromise
    {
        T GetResolveValue();
        
        IPromise<T> Then([NotNull] Action<T> onResolved);

        IPromise<T> Then([NotNull] Action<T> onResolved, [NotNull] Action<Exception> onRejected);
        
        IPromise<T> Then([NotNull] Action<T> onResolved, [NotNull] Action<Exception> onRejected, [NotNull] Action<float> onProgress);

        IPromise<T> Then([NotNull] RPromise<T> onReactPromise);
    }

    
}
