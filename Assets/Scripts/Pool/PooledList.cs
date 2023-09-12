using System.Collections.Generic;

namespace ToolBox.Pool
{
    public class PooledList<T> : List<T>, IPoolable
    {
        public PooledList() : base(10)
        {}

        private IPool _pool;
        
        public bool HasPool { get; private set; }
        public bool IsPooled => HasPool && _pool == null;
        public string PoolName { get; private set; }
        
        public void Spawn(IPool pool, string poolName)
        {
            _pool = pool;
            PoolName = poolName;
            HasPool = _pool != null;
        }

        public void Despawn()
        {
            _pool = null;
            PoolName = null;
        }

        public void DetachFromPool()
        {
            if (_pool != null)
            {
                _pool.Detach(this);
                _pool = null;
                PoolName = null;
                HasPool = false;
            }
        }
        
        public void ReturnToPool()
        {
            Clear();
            
            if (HasPool)
            {
                _pool?.Return(this);
            }
        }

    }
}