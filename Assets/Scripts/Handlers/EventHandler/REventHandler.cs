using System;
using System.Collections.Generic;
using ToolBox.Pool;
using ToolBox.Utils.Disposables;
using ToolBox.Utils.Singleton;

namespace ToolBox.Handlers.EventHandler
{
    public class REventHandler : NonMonoSingleton<REventHandler>
    {
        private Dictionary<Type, Delegate> _listeners = new Dictionary<Type, Delegate>();

        public static TypePool EventPool = new TypePool("EventPool");

        #region Overrides
        public override void InitSingleton()
        {
            
        }

        public override void CleanSingleton()
        {
            
        }
        #endregion

        #region Public Methods
        // Assigns the dispose function to a collection of IDisposables
        public void Subscribe<T>(Action<T> callback, ICollection<IDisposable> disposableContainer) where T : REvent
        {
            if (disposableContainer == null) throw new ArgumentNullException(nameof(disposableContainer));
            
            disposableContainer.Add(Subscribe(callback));
        }
        
        // Requires you to call the dispose function yourself
        public IDisposable Subscribe<T>(Action<T> callback) where T : REvent
        {
            var type = typeof(T);
            Delegate observer;
            
            if (_listeners.TryGetValue(type, out observer))
            {
                var action = observer as Action<T>;
                action += callback;
                _listeners[type] = action;
            }
            else
            {
                _listeners.Add(type, callback);
            }

            return Disposables.CreateWithState(callback, this.RemoveListener);
        }

        public void Dispatch(REvent rEvent, bool returnToPool = true)
        {
            var type = rEvent.GetType();

            if (_listeners.TryGetValue(type, out Delegate observer))
            {
                observer.DynamicInvoke(rEvent);
            }

            if (returnToPool)
            {
                rEvent.ReturnToPool();
            }
        }
        #endregion

        #region Internal Methods
        private void RemoveListener<T>(Action<T> callback)
        {
            var type = typeof(T);
            Delegate observer;

            if (_listeners != null && _listeners.TryGetValue(type, out observer))
            {
                var action = observer as Action<T>;
                action -= callback;

                if (action != null)
                {
                    _listeners[type] = action;
                }
                else
                {
                    _listeners.Remove(type);
                }
            }
        }
        #endregion
    }
}
