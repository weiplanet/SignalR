using System;
using System.Web.Http;
using System.Web.Http.Routing;
using SignalR.Hosting.WebApi;

namespace SignalR
{
    public static class HttpRouteExtensions
    {
        public static IHttpRoute MapConnection<T>(this HttpConfiguration config, string name, string url) where T : PersistentConnection
        {
            return MapConnection<T>(config, name, url, GlobalHost.DependencyResolver);
        }

        public static IHttpRoute MapConnection<T>(this HttpConfiguration config, string name, string url, IDependencyResolver resolver) where T : PersistentConnection
        {
            return MapConnection(config, name, url, typeof(T), resolver);
        }

        public static IHttpRoute MapConnection(this HttpConfiguration config, string name, string url, Type type)
        {
            return MapConnection(config, name, url, type, GlobalHost.DependencyResolver);
        }

        public static IHttpRoute MapConnection(this HttpConfiguration config, string name, string url, Type type, IDependencyResolver resolver)
        {
            var constraints = new HttpRouteValueDictionary();
            var values = new HttpRouteValueDictionary();
            var dataTokens = new HttpRouteValueDictionary();

            var route = new HttpRoute(url, values, constraints, dataTokens, new PersistentConnectionMessageHandler(type, resolver));
            config.Routes.Add(name, route);

            return route;
        }

        public static IHttpRoute MapHubs(this HttpConfiguration config)
        {
            return MapHubs(config, "~/signalr", GlobalHost.DependencyResolver);
        }

        public static IHttpRoute MapHubs(this HttpConfiguration config, IDependencyResolver resolver)
        {
            return MapHubs(config, "~/signalr", resolver);
        }

        public static IHttpRoute MapHubs(this HttpConfiguration config, string url, IDependencyResolver resolver)
        {
            config.Routes.Remove("signalr.hubs");

            string routeUrl = url;
            if (!routeUrl.EndsWith("/"))
            {
                routeUrl += "/{*operation}";
            }

            routeUrl = routeUrl.TrimStart('~').TrimStart('/');

            var constraints = new HttpRouteValueDictionary();
            var values = new HttpRouteValueDictionary();
            var dataTokens = new HttpRouteValueDictionary();

            string rawUrl = url.Replace("~/", config.VirtualPathRoot);

            var route = new HttpRoute(routeUrl, values, constraints, dataTokens, new HubDispatcherMessageHandler(rawUrl, resolver));
            config.Routes.Add("signalr.hubs", route);

            return route;
        }
    }
}
