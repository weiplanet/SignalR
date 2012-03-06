using System.Collections.Generic;
using System.Collections.Specialized;
using System;
using System.Net.Http.Headers;

namespace SignalR.AspNetWebApi
{
    internal static class HttpRequestHeadersExtensions
    {
        public static NameValueCollection ParseCookies(this HttpRequestHeaders headers)
        {
            var cookies = new NameValueCollection();

            IEnumerable<string> cookieValues;
            if (headers.TryGetValues("Cookie", out cookieValues))
            {
                // TODO: Parse cookies from cookie header

            }

            return cookies;
        }

        public static NameValueCollection ParseHeaders(this HttpRequestHeaders headers)
        {
            var headerValues = new NameValueCollection();
            foreach (var header in headers)
            {
                foreach (var value in header.Value)
                {
                    headerValues.Add(header.Key, value);
                }
            }
            return headerValues;
        }
    }
}
