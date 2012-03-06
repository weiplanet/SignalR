using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SignalR.AspNetWebApi.Hosting.AspNet.Samples
{
    public static class TaskFactoryExtensions
    {
        private static readonly Task _doneTask;

        static TaskFactoryExtensions()
        {
            _doneTask = FromResult(Task.Factory, new object());
        }

        public static Task Done(this TaskFactory factory)
        {
            return _doneTask;
        }

        public static Task<T> FromResult<T>(this TaskFactory factory, T result)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(result);
            return tcs.Task;
        }
    }
}