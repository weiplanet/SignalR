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

        public class RouteKeys
        {
            public static string ConnectionType = "SIGNALR_ConnectionType";
        }
    }
}
