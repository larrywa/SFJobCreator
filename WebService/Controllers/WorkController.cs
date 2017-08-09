using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Fabric;
using System.Fabric.Description;
using System.Fabric.Query;
using System.Text;
using System.Net.Http;
using Common;
using Newtonsoft.Json.Linq;

namespace WebService.Controllers
{
    [Route("api/workservice")]
    public class ApiController : Controller
    {
        private readonly FabricClient fabricClient;
        private readonly StatelessServiceContext context;

        public ApiController(FabricClient fabricClient, StatelessServiceContext context)
        {
            this.fabricClient = fabricClient;
            this.context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            ServicePartitionList partitions = await fabricClient.QueryManager.GetPartitionListAsync(new ServiceUriBuilder("WorkService").Build());
            List<string> result = new List<string>();

            foreach (Partition partition in partitions)
            {
                HttpClient client = new HttpClient(new HttpServiceClientHandler());

                Int64RangePartitionInformation partitionInfo = (Int64RangePartitionInformation)partition.PartitionInformation;

                Uri serviceUri = new HttpServiceUriBuilder()
                    .SetServiceName(new ServiceUriBuilder("WorkService").Build())
                    .SetPartitionKey(partitionInfo.LowKey)
                    .SetServicePathAndQuery($"api/work/")
                    .Build();

                HttpResponseMessage response = await client.GetAsync(serviceUri);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (responseContent != null)
                {
                    JArray responseJson = JArray.Parse(responseContent);
                    result.AddRange(responseJson.Select(x => x.Value<string>()));
                }
            }

            return result;
        }

        [HttpPost]
        [Route("{jobName}/{parameters}")]
        public async Task Post(string jobName, string parameters)
        {
            FnvHash fnv = new FnvHash();
            long partitionKey = (long)fnv.Hash(Encoding.UTF8.GetBytes(jobName));
            
            HttpClient client = new HttpClient(new HttpServiceClientHandler());

            Uri serviceUri = new HttpServiceUriBuilder()
                .SetServiceName(new ServiceUriBuilder("WorkService").Build())
                .SetPartitionKey(partitionKey)
                .SetServicePathAndQuery($"api/work/{jobName}/{parameters}")
                .Build();

            await client.PostAsync(serviceUri, new StringContent(String.Empty));
        }
    }
}
