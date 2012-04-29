using System.Linq;
using System.Net.Http.Headers;

namespace SignalR.Hosting.WebApi
{
    internal class CookieCollection : IRequestCookieCollection
    {
        private HttpRequestHeaders _headers;

        public CookieCollection(HttpRequestHeaders headers)
        {
            _headers = headers;
            Count = System.Net.Http.HttpRequestHeadersExtensions.GetCookies(headers).Count;
        }

        public Cookie this[string name]
        {
            get
            {
                var cookieHeaderValue = System.Net.Http.HttpRequestHeadersExtensions.GetCookies(_headers, name).FirstOrDefault();
                if (cookieHeaderValue == null)
                {
                    return null;
                }

                // TODO: Figure out the correct thing to pass to value
                return new Cookie(name, null, cookieHeaderValue.Domain, cookieHeaderValue.Path);
            }
        }

        public int Count
        {
            get;
            private set;
        }
    }
}
