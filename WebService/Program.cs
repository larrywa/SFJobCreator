using Microsoft.ServiceFabric.Services.Runtime;
using System.Threading;

namespace WebService
{
    public class Program
    {
        // Entry point for the application.
        public static void Main(string[] args)
        {
            ServiceRuntime.RegisterServiceAsync("WebServiceType", context => new WebService(context)).GetAwaiter().GetResult();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
