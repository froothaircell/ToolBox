namespace ToolBox.Pool
{
    public abstract class Poolable : IPoolable
    {
        protected IPool _pool;

        public bool HasPool { get; private set; }
        public bool IsPooled => HasPool && _pool == null;
        public string PoolName { get; private set; }

        public virtual bool CanSpawn<TComp>(TComp poolable) where TComp : IPoolable
        {
            return !IsPooled;
        }

        public void Spawn(IPool pool, string poolName)
        {
            _pool = pool;
            PoolName = poolName;
            HasPool = _pool != null;

            OnSpawn();
        }

        protected abstract void OnSpawn();

        public virtual void Despawn()
        {
            OnDespawn();

            _pool = null;
            PoolName = null;
            // Has{pp; remains the same despite being in the pool or outside
        }

        protected abstract void OnDespawn();

        public virtual void DetachFromPool()
        {
            if (_pool != null)
            {
                _pool.Detach(this);
                _pool = null;
                PoolName = null;
                HasPool = false;
            }
        }

        public virtual void ReturnToPool()
        {
            if (HasPool)
            {
                _pool?.Return(this);
            }
        }
    }
}