using System;
using System.Collections.Generic;
using System.Linq;
using ToolBox.Pool;
using ToolBox.Utils.Singleton;
using UnityEngine;

namespace ToolBox.Utils
{
    /// <summary>
    /// This should definitely be the first service 
    /// to initialize in AppHandler. It will handle 
    /// all other sub services without constant need 
    /// to access the app handler or it's references 
    /// </summary>
    public class ServiceLocator : DestroyableMonoSingleton<ServiceLocator>
    {
        #region Private Fields
        private static Dictionary<Type, IGenericSingleton> _serviceList;
        #endregion
    
        // Not exactly the right place to place these but since
        // this is a toolbox without a specific app handler it's
        // the best place to add it other than making a seperate
        // class just for these instances (which is stupid)
        public static TypePool AppPool = new TypePool("AppPool");

        #region Overrides
        public override void InitSingleton()
        {
            base.InitSingleton();

            _serviceList = new Dictionary<Type, IGenericSingleton>();
        }

        public override void CleanSingleton()
        {
            var keys = _serviceList.Keys.ToList();
            for (int i = 0; i < _serviceList.Count; i++)
            {
                _serviceList[keys[i]].CleanSingleton();
            }

            _serviceList.Clear();
            _serviceList = null;

            base.CleanSingleton();
        }
        #endregion

        /// <summary>
        /// Function adds and initializes a service according to it's type. 
        /// An alternative would be to intiialize the service in the singleton and add it from there
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddService<T>(T service) where T : class, IGenericSingleton
        {
            if (_serviceList == null)
            {
                Debug.LogError("Service Locator Not Initialized");
                return;
            }
            if (typeof(T) == typeof(ServiceLocator))
            {
                Debug.LogError("Cannot add service locator class to the list of services");
                return;
            }
            if (_serviceList.ContainsKey(typeof(T)))
            {
                Debug.LogError($"Service {nameof(T)} already exists");
                return;
            }

            _serviceList.Add(typeof(T), service);
        }

        public void RemoveService<T>()
        {
            if (_serviceList == null)
            {
                Debug.LogError("Service Locator Not Initialized");
                return;
            }
            if (typeof(T) == typeof(ServiceLocator))
            {
                Debug.LogError("Cannot remove service locator class to the list of services");
                return;
            }
            if (!_serviceList.ContainsKey(typeof(T)))
            {
                Debug.LogError($"Service {nameof(T)} does not exist");
                return;
            }

            var service = _serviceList[typeof(T)];
            // We could possibly pool these game objects for later use
            switch (service)
            {
                case MonoSingleton mono:
                    Destroy(mono.gameObject);
                    break; 
                case DestroyableMonoSingleton destroyableMono:
                    Destroy(destroyableMono.gameObject);
                    break;
                case NonMonoSingleton nonMono:
                    nonMono.CleanSingleton();
                    break;
            }

            _serviceList.Remove(typeof(T));
        }

        public static  T Get<T>() where T : class, IGenericSingleton
        {
            if (_serviceList == null)
            {
                Debug.LogError("Service Locator Not Initialized");
                return null;
            }

            if (!_serviceList.TryGetValue(typeof(T), out var service) || service != null)
            {
                Debug.LogError($"Service {nameof(T)} Does Not Exist");
            }

            return service as T;
        }
    }
}
