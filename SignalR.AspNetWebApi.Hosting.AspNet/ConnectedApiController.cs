using SignalR.Hosting.AspNet;

namespace SignalR.AspNetWebApi.Hosting.AspNet
{
    public abstract class ConnectedApiController : ConnectedApiControllerBase
    {
        public ConnectedApiController()
            : base(AspNetHost.DependencyResolver)
        {
            
        }
    }
}
