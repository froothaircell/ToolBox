using System;

namespace ToolBox.Pool
{
    public interface IPool
    {
        String Name { get; }

        void UpdatePoolName(string newName);
        void Detach(IPoolable poolItem);
        void Return(IPoolable poolItem);
    }

    
}
