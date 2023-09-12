using System;
using System.Collections.Generic;
using UnityEngine;

namespace ToolBox.Pool
{
    public class TypePool : IPool
    {
        public string Name { get; private set; }
        
        private Dictionary<string, TypePoolData> _pools = new Dictionary<string, TypePoolData>();
        private static Dictionary<Type, string> _poolNames = new Dictionary<Type, string>();
        
        public int InstantiatedCount { get; private set; }
        public int InPoolCount { get; private set; }

        public TypePool(string name)
        {
            Name = name;

            InstantiatedCount = 0;
            InPoolCount = 0;
            
            // Register this pool with the manager singleton
        }
        
        public void UpdatePoolName(string newName)
        {
            Name = newName;
        }

        public virtual T Get<T>() where T : class, IPoolable, new()
        {
            var type = typeof(T);

            if (!_poolNames.TryGetValue(type, out string poolName))
            {
                poolName = type.FullName;
                _poolNames.Add(type, poolName);
            }

            T poolItem = GetFromPool<T>(poolName, out TypePoolData poolData);
            if (poolItem != null)
            {
                return poolItem;
            }

            poolItem = new T();
            poolItem.Spawn(this, poolName);

            InstantiatedCount++;

            if (poolData == null)
            {
                poolData = TypePoolData.Get(type);
                _pools.Add(poolName, poolData);
            }

            poolData.InstantiatedCount++;
            
            return poolItem;
        }

        public virtual T Get<T>(Type type) where T : class, IPoolable
        {
            string poolName = type.FullName;

            T poolItem = GetFromPool<T>(poolName, out TypePoolData poolData);
            if (poolItem != null)
            {
                return poolItem;
            }

            poolItem = (T) Activator.CreateInstance(type);
            poolItem.Spawn(this, poolName);
            
            InstantiatedCount++;

            if (poolData == null)
            {
                poolData = TypePoolData.Get(type);
                _pools.Add(poolName, poolData);
            }

            poolData.InstantiatedCount++;
            return poolItem;
        }

        private T GetFromPool<T>(string poolName, out TypePoolData poolData) where T : class, IPoolable
        {
            if (_pools.TryGetValue(poolName, out poolData) && poolData.Stack.Count > 0)
            {
                InPoolCount--;

                T poolItem = (T) poolData.Stack.Pop();
                poolItem.Spawn(this, poolName);
                return poolItem;
            }

            return null;
        }
        
        public virtual void Detach(IPoolable poolItem)
        {
            if (poolItem == null)
            {
                return;
            }

            if (!poolItem.HasPool)
            {
                Debug.LogWarning($"Trying to detach {poolItem.GetType().Name} from pool but it has no pool!");
                return;
            }

            if (poolItem.IsPooled)
            {
                Debug.LogError($"Trying to detach {poolItem.GetType().Name} from pool while it is pooled.  You can only detach a wild instance from it's pool!");
                return;
            }

            InstantiatedCount--;
        }

        public virtual void Return(IPoolable poolItem)
        {
            if (poolItem == null)
            {
                return;
            }

            if (!poolItem.HasPool)
            {
                Debug.LogWarning($"Trying to return {poolItem.GetType().Name} to pool but it has no pool!");
                return;
            }

            if (poolItem.IsPooled)
            {
                Debug.LogWarning($"Trying to return {poolItem.GetType().Name} to pool when it is already pooled!");
                return;
            }
            
            string poolName = poolItem.PoolName;
            poolItem.Despawn();
            
            if (!_pools.TryGetValue(poolName, out TypePoolData poolData))
            {
                poolData = TypePoolData.Get(poolItem.GetType());
                _pools.Add(poolName, poolData);
            }

            poolData.Stack.Push(poolItem);
        }
    }
}