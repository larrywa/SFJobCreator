
namespace WorkService
{
    using Common;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class WorkService : StatefulService
    {
        private readonly IList<Task> runningJobs;

        public WorkService(StatefulServiceContext serviceContext)
            : base(serviceContext)
        {
            this.runningJobs = new List<Task>();
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(context =>
                {
                    return new WebHostCommunicationListener(context, "ServiceEndpoint", uri =>
                        new WebHostBuilder().UseWebListener()
                                           .UseContentRoot(Directory.GetCurrentDirectory())
                                           .ConfigureServices(services => services
                                                .AddSingleton<IReliableStateManager>(this.StateManager)
                                                .AddSingleton<StatefulServiceContext>(context))
                                           .UseStartup<Startup>()
                                           .UseUrls(uri)
                                           .Build());
                })
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            IReliableQueue<string> queue = await this.StateManager.GetOrAddAsync<IReliableQueue<string>>("jobQueue");
            IReliableDictionary<string, Job> dictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Job>>("jobs");

            try
            {

                // need to restart any existing jobs after failover
                using (ITransaction tx = this.StateManager.CreateTransaction())
                {
                    var enumerable = await dictionary.CreateEnumerableAsync(tx);
                    var enumerator = enumerable.GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(cancellationToken))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Job job = enumerator.Current.Value;
                        
                        this.runningJobs.Add(this.StartJob(job, cancellationToken));
                    }
                }

                // start processing new jobs from the queue.
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        using (ITransaction tx = this.StateManager.CreateTransaction())
                        {
                            ConditionalValue<string> dequeueResult = await queue.TryDequeueAsync(tx);

                            if (!dequeueResult.HasValue)
                            {
                                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
                                continue;
                            }

                            string jobName = dequeueResult.Value;

                            ConditionalValue<Job> getResult = await dictionary.TryGetValueAsync(tx, jobName, LockMode.Update);

                            if (getResult.HasValue)
                            {
                                Job job = getResult.Value;
                                
                                this.runningJobs.Add(this.StartJob(job, cancellationToken));

                                await dictionary.SetAsync(tx, jobName, new Job(job.Name, job.Parameters, job.Running));
                            }

                            await tx.CommitAsync();
                        }
                    }
                    catch (FabricTransientException)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
                    }
                    catch (TimeoutException)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
                    }
                }

            }
            catch (OperationCanceledException)
            {
                await Task.WhenAll(this.runningJobs);

                throw;
            }
        }

        private Task StartJob (Job job, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("store://state/jobstate");

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using (var tx = this.StateManager.CreateTransaction())
                    {
                        var result = await myDictionary.TryGetValueAsync(tx, job.Name, LockMode.Update);

                        string iteration = result.HasValue ? result.Value.ToString() : "0";
                        ServiceEventSource.Current.ServiceMessage(this, $"Work {job.Name}: {job.Parameters}. Iteration: {iteration}");

                        await myDictionary.AddOrUpdateAsync(tx, job.Name, 0, (key, value) => ++value);

                        // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                        // discarded, and nothing is saved to the secondary replicas.
                        await tx.CommitAsync();
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }, cancellationToken);
        }
    }
}
