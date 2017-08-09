using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using WorkService.Models;

namespace WorkService.Controllers
{
    [Route("api/[controller]")]
    public class WorkController : Controller
    {
        private readonly IReliableStateManager stateManager;
        private readonly StatefulServiceContext context;

        public WorkController(IReliableStateManager stateManager, StatefulServiceContext context)
        {
            this.stateManager = stateManager;
            this.context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            IReliableDictionary<string, Job> dictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, Job>>("jobs");
            List<string> result = new List<string>();

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                var enumerable = await dictionary.CreateEnumerableAsync(tx);
                var enumerator = enumerable.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(CancellationToken.None))
                {
                    result.Add(enumerator.Current.Value.Name);
                }
            }

            return result;
        }

        [HttpPost]
        [Route("{name}/{parameters}")]
        public async Task Post(string name, string parameters)
        {
            IReliableQueue<string> queue = await this.stateManager.GetOrAddAsync<IReliableQueue<string>>("jobQueue");
            IReliableDictionary<string, Job> dictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, Job>>("jobs");

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                if (await dictionary.ContainsKeyAsync(tx, name, LockMode.Update))
                {
                    throw new ArgumentException($"Job {name} already exists.");
                }

                Job job = new Job(name, parameters, false);

                await queue.EnqueueAsync(tx, name);
                await dictionary.SetAsync(tx, name, job);
                await tx.CommitAsync();
            }
        }
    }
}
