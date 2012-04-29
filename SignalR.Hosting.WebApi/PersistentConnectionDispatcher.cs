using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace SignalR.Hosting.WebApi
{
    public class PersistentConnectionDispatcher : DelegatingHandler
    {
        private readonly HttpConfiguration _config;
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
            var routeData = _config.Routes.GetRouteData(request);

            if (routeData == null)
            {
                return TaskAsyncHelper.FromResult(request.CreateResponse(HttpStatusCode.NotFound));
            }

            object connectionType;
            if (!routeData.Route.Defaults.TryGetValue(HttpRouteExtensions.RouteKeys.ConnectionType, out connectionType))
            {
                return base.SendAsync(request, cancellationToken);
            }

            var type = (Type)connectionType;

            var factory = new PersistentConnectionFactory(_resolver);
            PersistentConnection connection = factory.CreateInstance(type);

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
    }
}
