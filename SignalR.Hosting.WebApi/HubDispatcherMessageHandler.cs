using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Routing;
using SignalR.Hubs;

namespace SignalR.Hosting.WebApi
{
    public class HubDispatcherMessageHandler : PersistentConnectionDispatcher
    {
        public HubDispatcherMessageHandler(HttpConfiguration config)
            : this(config, GlobalHost.DependencyResolver)
        {

        }

        public HubDispatcherMessageHandler(HttpConfiguration config, IDependencyResolver resolver)
            : base(config, resolver)
        {
        }

        protected override bool TryGetConnection(HttpRequestMessage request, CancellationToken cancellationToken, out PersistentConnection connection)
        {
            IHttpRouteData routeData = _config.Routes.GetRouteData(request);

            if (routeData == null)
            {
                connection = null;
                return false;
            }

            object hubsUrlValue;
            if (routeData.Route.Defaults.TryGetValue(HttpRouteExtensions.RouteKeys.HubsBaseUrl, out hubsUrlValue))
            {
                var hubsUrl = (string)hubsUrlValue;
                string fullUrl = hubsUrl.Replace("~/", _config.VirtualPathRoot);
                connection = new HubDispatcher(fullUrl);
                return true;
            }

            return base.TryGetConnection(request, cancellationToken, out connection);
        }
    }
}
