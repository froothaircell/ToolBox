using ToolBox.Pool;

namespace ToolBox.Handlers.EventHandler
{
    // Class is pooled and can have its own implementation for invoke
    // Correction : definition of the invoke/dispatch function is necessary
    public class REvent : Poolable
    {
        public string Name => this.Name;

        public static T Get<T>() where T : REvent, new()
        {
            var temp = REventHandler.EventPool.Get<T>();
            return temp;
        }

        protected override void OnSpawn()
        {
            
        }

        protected override void OnDespawn()
        {
            
        }
    }
}