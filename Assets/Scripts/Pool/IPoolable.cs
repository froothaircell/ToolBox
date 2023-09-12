using System;
using System.Collections.Generic;

namespace ToolBox.Pool
{
    public interface IPoolable
    {
        bool HasPool { get; }
        bool IsPooled { get; }
        string PoolName { get; }

        void Spawn(IPool pool, string poolName);
        void Despawn();
        void ReturnToPool();
        void DetachFromPool();
    }

    public class TypePoolData
    {
        public Stack<IPoolable> Stack { get; private set; }
        public Type Type { get; private set; }
        public int InstantiatedCount;

        public static TypePoolData Get(Type type)
        {
            return new TypePoolData
            {
                Type = type,
                Stack = new Stack<IPoolable>(16),
                InstantiatedCount = 0
            };
        }

        private TypePoolData()
        {

        }
    }
}