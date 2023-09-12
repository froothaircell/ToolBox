using ToolBox.Utils.Singleton;
using UnityEngine;

namespace ToolBox.Mediators
{
    public class Mediator<T> : DestroyableMonoSingleton<T> where T : Mediator<T>
    {
        public virtual string Name
        {
            get { return GetType().Name; }
        }
        
        public static T Create(Transform parent = null)
        {
            T mediator = CreateInstance(parent);
            if (mediator == null)
            {
                mediator = Instance;
            }

            return mediator;
        }

        public virtual void Initialize()
        {
            Debug.Log($"{Name} Initialized!");
        }

        public virtual void Clean()
        {
            Debug.Log($"{Name} Cleaned!");
        }
    }
}