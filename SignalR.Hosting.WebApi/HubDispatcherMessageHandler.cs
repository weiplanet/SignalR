using SignalR.Hubs;

namespace SignalR.Hosting.WebApi
{
    public class HubDispatcherMessageHandler : PersistentConnectionDispatcher
    {
        private readonly string _url;

        public HubDispatcherMessageHandler(string url, IDependencyResolver resolver)
            : base(resolver)
        {
            _url = url;
        }

        protected override PersistentConnection ResolveConnection()
        {
            return new HubDispatcher(_url);
        }
    }
}
