using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using SignalR.Hubs;

namespace SignalR.AspNetWebApi.Hosting.AspNet.Samples.Api.Controllers
{
    [HubName("values")]
    public class ValuesConnectedController : ConnectedApiController, IConnected
    {
        private static int _getCalls = 0;
        private static int _postCalls = 0;
        private static int _putCalls = 0;
        private static int _deleteCalls = 0;
        private static int _patchCalls = 0;

        // GET /api/<controller>
        public IEnumerable<string> Get()
        {
            Clients.called(HubContext.ConnectionId, "get", Interlocked.Increment(ref _getCalls));
            return new string[] { "value1", "value2" };
        }

        // GET /api/<controller>/5
        public string Get(int id)
        {
            Clients.called(HubContext.ConnectionId, "get", Interlocked.Increment(ref _getCalls));
            return "value";
        }

        // POST /api/<controller>
        public void Post(string value)
        {
            Clients.called(HubContext.ConnectionId, "post", Interlocked.Increment(ref _postCalls));
        }

        // PUT /api/<controller>/5
        public void Put(int id, string value)
        {
            Clients.called(HubContext.ConnectionId, "put", Interlocked.Increment(ref _putCalls));
        }

        // DELETE /api/<controller>/5
        public void Delete(int id)
        {
            Clients.called(HubContext.ConnectionId, "del", Interlocked.Increment(ref _deleteCalls));
        }

        // PATCH /api/<controller>/5
        [AcceptVerbs("PATCH")]
        public void Patch(int id, string value)
        {
            Clients.called(HubContext.ConnectionId, "patch", Interlocked.Increment(ref _patchCalls));
        }

        Task IConnected.Connect(IEnumerable<string> groups)
        {
            Caller.setCallStats(new { get = _getCalls, post = _postCalls, put = _putCalls, del = _deleteCalls, patch = _patchCalls });
            return Task.Factory.Done();
        }

        Task IConnected.Reconnect(IEnumerable<string> groups)
        {
            return Task.Factory.Done();
        }
    }
}