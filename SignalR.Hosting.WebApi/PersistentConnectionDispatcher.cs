using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Hosting.WebApi
{
    public abstract class PersistentConnectionDispatcher : HttpMessageHandler
    {
        protected readonly IDependencyResolver _resolver;

        public PersistentConnectionDispatcher(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            PersistentConnection connection = ResolveConnection();

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

        protected abstract PersistentConnection ResolveConnection();
    }
}
