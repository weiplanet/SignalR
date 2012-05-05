using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using SignalR.Hubs;

namespace SignalR.Hosting.WebApi
{
    public class WebApiAssemblyLocator : IAssemblyLocator
    {
        private readonly HttpConfiguration _config;

        public WebApiAssemblyLocator(HttpConfiguration config)
        {
            _config = config;
        }

        public IEnumerable<Assembly> GetAssemblies()
        {
            return _config.Services.GetAssembliesResolver().GetAssemblies();
        }
    }
}
