using System.Collections.Generic;
using System.ComponentModel;
using ToolBox.Pool;

namespace ToolBox.Utils
{
    public static class PooledListUtils
    {
        public static PooledList<T> ToPooledList<T>(this IEnumerable<T> container)
        {
            PooledList<T> temp = ServiceLocator.AppPool.Get<PooledList<T>>();
            foreach (var variable in container)
            {
                temp.Add(variable);
            }

            return temp;
        }
    }
}