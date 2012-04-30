using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Hosting.WebApi
{
    public class WebApiResponse : IResponse
    {
        private readonly CancellationToken _cancellationToken;
        private readonly HttpResponseMessage _responseMessage;
        private readonly Action _sendResponse;

        private int streamingInitialized;
        private bool _writeFailed;
        private bool _ended;
        private Stream _stream;

        public WebApiResponse(CancellationToken cancellationToken, HttpResponseMessage responseMessage, Action sendResponse)
        {
            _cancellationToken = cancellationToken;
            _sendResponse = sendResponse;
            _responseMessage = responseMessage;
        }

        public string ContentType { get; set; }

        public bool IsClientConnected
        {
            get
            {
                return !_ended && !_writeFailed && !_cancellationToken.IsCancellationRequested;
            }
        }

        public Task EndAsync(string data)
        {
            _responseMessage.Content = new StringContent(data);
            _responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
            
            return TaskAsyncHelper.Empty;
        }

        public Task WriteAsync(string data)
        {
            if (Interlocked.Exchange(ref streamingInitialized, 1) == 0)
            {
                var tcs = new TaskCompletionSource<object>();
                _responseMessage.Content = new PushStreamContent((stream, contentHeaders, context) =>
                {
                    _stream = stream;

                    tcs.TrySetResult(null);
                },
                new MediaTypeHeaderValue(ContentType));

                // Return the response back to the client
                _sendResponse();

                // Write when the stream is ready
                return tcs.Task.Then(() => WriteTaskAsync(data)).Catch();
            }

            // The stream has already been initialized so just write
            return WriteTaskAsync(data).Catch();
        }

        private Task WriteTaskAsync(string data)
        {
            if (_stream == null || !IsClientConnected)
            {
                return TaskAsyncHelper.Empty;
            }

            var buffer = Encoding.UTF8.GetBytes(data);

            return WriteAsync(buffer).Then(() => _stream.Flush())
                                     .Catch(ex =>
                                     {
                                         _writeFailed = true;
                                     });
        }

        private Task WriteAsync(byte[] buffer)
        {
            return Task.Factory.FromAsync((cb, state) => _stream.BeginWrite(buffer, 0, buffer.Length, cb, state), 
                                           ar => _stream.EndWrite(ar), null);
        }

        public void End()
        {
            if (_stream != null)
            {
                _stream.Close();
            }

            _ended = true;
        }
    }
}
