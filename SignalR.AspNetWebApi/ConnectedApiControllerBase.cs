using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using SignalR.Hosting;
using SignalR.Hubs;
using SignalR.Infrastructure;

namespace SignalR.AspNetWebApi
{
    public abstract class ConnectedApiControllerBase : ApiController, IHub
    {
        private readonly IDependencyResolver _dependencyResolver;

        public ConnectedApiControllerBase(IDependencyResolver dependencyResolver)
        {
            if (dependencyResolver == null)
            {
                throw new ArgumentNullException("dependencyResolver");
            }

            _dependencyResolver = dependencyResolver;
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            var user = controllerContext.Request.GetUserPrincipal();
            // TODO: Review why the IResponse parameter is null
            var hostContext = new HostContext(new WebApiRequest(controllerContext.Request), null, user);
            var connectionId = hostContext.Request.QueryString["connectionId"];
            ((IHub)this).Context = new HubContext(hostContext, connectionId);
            var hubName = this.GetType().FullName;
            var connection = _dependencyResolver.Resolve<IConnectionManager>().GetConnection<HubDispatcher>();
            var state = new TrackingDictionary();
            var agent = new ClientAgent(connection, hubName);
            ((IHub)this).Caller = new SignalAgent(connection, connectionId, hubName, state);
            ((IHub)this).Agent = agent;
            ((IHub)this).GroupManager = agent;

            base.Initialize(controllerContext);
        }

        public dynamic Caller
        {
            get { return ((IHub)this).Caller; }
        }

        public dynamic Clients
        {
            get { return ((IHub)this).Agent; }
        }

        public HubContext HubContext
        {
            get { return ((IHub)this).Context; }
        }

        public bool IsConnectedRequest
        {
            get { return !String.IsNullOrEmpty(HubContext.ConnectionId); }
        }

        public Task AddToGroup(string connectionId, string groupName)
        {
            return ((IHub)this).GroupManager.AddToGroup(connectionId, groupName);
        }

        public Task RemoveFromGroup(string connectionId, string groupName)
        {
            return ((IHub)this).GroupManager.RemoveFromGroup(connectionId, groupName);
        }

        IClientAgent IHub.Agent
        {
            get;
            set;
        }

        dynamic IHub.Caller
        {
            get;
            set;
        }

        HubContext IHub.Context
        {
            get;
            set;
        }

        IGroupManager IHub.GroupManager
        {
            get;
            set;
        }
    }
}
