using System;
using ToolBox.Pool;

namespace ToolBox.Utils.Disposables
{
    public static class DisposableExtensions
    {
        public static void ClearDisposables<T>(this T container) where T : PooledList<IDisposable>
        {
            if (container == null || container.Count <= 0)
            {
                return;
            }

            foreach (var disposable in container)
            {
                disposable?.Dispose();
            }
            
            container.Clear();
        }
    }
}