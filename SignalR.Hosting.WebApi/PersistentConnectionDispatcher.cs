using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;

namespace SignalR.Hosting.WebApi
{
    public class PersistentConnectionDispatcher : DelegatingHandler
    {
        protected readonly HttpConfiguration _config;
        private readonly IDependencyResolver _resolver;

        public PersistentConnectionDispatcher(HttpConfiguration config)
            : this(config, GlobalHost.DependencyResolver)
        {
        }

        public PersistentConnectionDispatcher(HttpConfiguration config, IDependencyResolver resolver)
        {
            _config = config;
            _resolver = resolver;
        }

        public IConnectionManager ConnectionManager
        {
            get
            {
                return _resolver.Resolve<IConnectionManager>();
            }
        }

        public IConfigurationManager Configuration
        {
            get
            {
                return _resolver.Resolve<IConfigurationManager>();
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            PersistentConnection connection;
            if (!TryGetConnection(request, cancellationToken, out connection))
            {
                return base.SendAsync(request, cancellationToken);
            }

            var tcs = new TaskCompletionSource<HttpResponseMessage>();
            var req = new WebApiRequest(request);
            var response = new HttpResponseMessage();
            var resp = new WebApiResponse(cancellationToken, response, () => tcs.TrySetResult(response));
            var host = new HostContext(req, resp, Thread.CurrentPrincipal);

            try
            {
                connection.Initialize(_resolver);
                connection.ProcessRequestAsync(host).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        tcs.TrySetException(task.Exception);
                    }
                    else if (task.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(response);
                    }

                    resp.End();
                });
            }
            catch (Exception ex)
            {
                return TaskAsyncHelper.FromError<HttpResponseMessage>(ex);
            }

            return tcs.Task;
        }

        protected virtual bool TryGetConnection(HttpRequestMessage request, CancellationToken cancellationToken, out PersistentConnection connection)
        {
            connection = null;
            IHttpRouteData routeData = _config.Routes.GetRouteData(request);

            if (routeData == null)
            {
                return false;
            }

            object connectionType;
            if (!routeData.Route.Defaults.TryGetValue(HttpRouteExtensions.RouteKeys.ConnectionType, out connectionType))
            {
                return false;
            }

            var type = (Type)connectionType;

            var factory = new PersistentConnectionFactory(_resolver);
            connection = factory.CreateInstance(type);

            return true;
        }
    }
}
