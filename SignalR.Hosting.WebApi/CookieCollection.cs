using System.Linq;
using System.Net.Http.Headers;
using WebApiRequestHeadersExtensions = System.Net.Http.HttpRequestHeadersExtensions;

namespace SignalR.Hosting.WebApi
{
    internal class CookieCollection : IRequestCookieCollection
    {
        private HttpRequestHeaders _headers;

        public CookieCollection(HttpRequestHeaders headers)
        {
            _headers = headers;
            Count = WebApiRequestHeadersExtensions.GetCookies(headers).Count;
        }

        public Cookie this[string name]
        {
            get
            {
                var cookieHeaderValue = WebApiRequestHeadersExtensions.GetCookies(_headers, name).FirstOrDefault();
                if (cookieHeaderValue == null)
                {
                    return null;
                }

                CookieState state = cookieHeaderValue.Cookies.FirstOrDefault();
                string value = state != null ? state.Value : null;
                return new Cookie(name, value, cookieHeaderValue.Domain, cookieHeaderValue.Path);
            }
        }

        public int Count
        {
            get;
            private set;
        }
    }
}
