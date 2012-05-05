using System;

namespace SignalR.Hosting.WebApi
{
    public class PersistentConnectionMessageHandler : PersistentConnectionDispatcher
    {
        private readonly Type _connectionType;

        public PersistentConnectionMessageHandler(Type connectionType, IDependencyResolver resolver)
            : base(resolver)
        {
            _connectionType = connectionType;
        }

        protected override PersistentConnection ResolveConnection()
        {
            var factory = new PersistentConnectionFactory(_resolver);
            return factory.CreateInstance(_connectionType);
        }
    }
}
