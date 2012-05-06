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

        private Task _beginWriteTask;

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

            if (!String.IsNullOrEmpty(ContentType))
            {
                _responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
            }

            return TaskAsyncHelper.Empty;
        }

        public Task WriteAsync(string data)
        {
            // If the stream is ready then just write
            if (_stream != null)
            {
                return WriteTaskAsync(data).Catch();
            }

            // Make sure we don't miss a write here in case write is called multiple times before the 
            // stream is ready.
            var tcs = new TaskCompletionSource<object>();
            if (Interlocked.CompareExchange(ref _beginWriteTask, tcs.Task, null) != null)
            {
                // Write when the stream is ready
                return _beginWriteTask.Then(() => WriteTaskAsync(data)).Catch();
            }

            _responseMessage.Content = new PushStreamContent((stream, contentHeaders, context) =>
            {
                _stream = stream;

                tcs.TrySetResult(null);
            },
            new MediaTypeHeaderValue(ContentType));

            // Return the response back to the client
            _sendResponse();

            // Write when the stream is ready
            return _beginWriteTask.Then(() => WriteTaskAsync(data)).Catch();
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
