using ToolBox.Promise;

namespace ToolBox.Utils.Jobs
{
    public class UpdateJob : JobPromise
    {
        public void StopUpdate()
        {
            ReturnToPool();
        }

        public override void ReturnToPool()
        {
            RPromise.SafeResolve(ref _jobPromise);
            base.ReturnToPool();
        }
    }
}