using Microsoft.ServiceFabric.Services.Runtime;
using System.Threading;

namespace WorkService
{
    public class Program
    {
        // Entry point for the application.
        public static void Main(string[] args)
        {
            ServiceRuntime.RegisterServiceAsync("WorkServiceType", context => new WorkService(context)).GetAwaiter().GetResult();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
