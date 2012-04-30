using System;
using System.Web.Http;
using System.Web.Http.Routing;

namespace SignalR.Hosting.WebApi
{
    public static class HttpRouteExtensions
    {
        public static IHttpRoute MapConnection<T>(this HttpRouteCollection routes, string name, string url) where T : PersistentConnection
        {
            return MapConnection(routes, name, url, typeof(T));
        }

        public static IHttpRoute MapConnection(this HttpRouteCollection routes, string name, string url, Type type)
        {
            var constraints = new HttpRouteValueDictionary();

            var values = new HttpRouteValueDictionary();
            values[RouteKeys.ConnectionType] = type;

            var route = new HttpRoute(url, values, constraints);
            routes.Add(name, route);

            return route;
        }

        public static IHttpRoute MapHubs(this HttpRouteCollection routes)
        {
            return MapHubs(routes, "~/signalr");
        }

        public static IHttpRoute MapHubs(this HttpRouteCollection routes, string url)
        {
            routes.Remove("signalr.hubs");

            string routeUrl = url;
            if (!routeUrl.EndsWith("/"))
            {
                routeUrl += "/{*operation}";
            }

            routeUrl = routeUrl.TrimStart('~').TrimStart('/');

            var constraints = new HttpRouteValueDictionary();
            var values = new HttpRouteValueDictionary();

            values.Add(RouteKeys.HubsBaseUrl, url);
            var route = new HttpRoute(routeUrl, values, constraints);
            routes.Add("signalr.hubs", route);

            return route;
        }

        public class RouteKeys
        {
            public static string ConnectionType = "SIGNALR_ConnectionType";
            public static string HubsBaseUrl = "SIGNALR_HubBaseUrl";
        }
    }
}
