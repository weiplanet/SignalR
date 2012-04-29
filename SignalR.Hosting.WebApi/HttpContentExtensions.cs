using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace SignalR.Hosting.WebApi
{
    public static class HttpContentExtensions
    {
        public static NameValueCollection ReadAsNameValueCollection(this HttpContent content)
        {
            var form = new NameValueCollection();
            var collection = content.ReadAsAsync<FormDataCollection>().Result;

            foreach (var kvp in collection)
            {
                form.Add(kvp.Key, kvp.Value);
            }

            return form;
        }
    }
}
