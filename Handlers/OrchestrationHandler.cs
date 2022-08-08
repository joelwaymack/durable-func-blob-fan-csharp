using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Company.Function.Models;

namespace Company.Function;

public static class OrchestrationHandler
{
    [FunctionName("OrchestrateOrders")]
    public static async Task RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        var file = context.GetInput<Models.File>();
        await context.CallActivityAsync("LogOnce", $"Orchestrator started for file {file.Path}");

        var orders = JsonConvert.DeserializeObject<List<Models.Order>>(file.Content);
        var tasks = new List<Task>();

        foreach (var order in orders)
        {
            tasks.Add(context.CallActivityAsync("ProcessOrder", order));
        }

        await Task.WhenAll(tasks);
        await context.CallActivityAsync("LogOnce", $"Orchestrator ended for file {file.Path}");
    }

    [FunctionName("ProcessOrder")]
    public static void ProcessOrder(
        [ActivityTrigger] Order order, 
        [CosmosDB(
            databaseName: "%DatabaseName%",
            collectionName: "%CollectionName%",
            ConnectionStringSetting = "CosmosConnection",
            CreateIfNotExists = true)] IAsyncCollector<Models.Order> orderCollector,
        ILogger log)
    {
        orderCollector.AddAsync(order);
        log.LogInformation($"Processed order {order.Id}");
    }

    [FunctionName("LogOnce")]
    public static void LogOnce(
        [ActivityTrigger] string message, 
        ILogger log)
    {
        log.LogInformation(message);
    }

    [FunctionName("ProcessOrderFile")]
    public static async Task ProcessOrderFile(
        [BlobTrigger("%BlobInputPath%/{name}", Connection = "BlobConnection")] string myBlob,
        string name,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        var path = $"{Environment.GetEnvironmentVariable("BlobInputPath")}/{name}";
        log.LogInformation($"New order file '{path}'");
        string instanceId = await starter.StartNewAsync("OrchestrateOrders", null, new Models.File { Path = path, Content = myBlob });
        log.LogInformation($"Started orchestration with ID = '{instanceId}'");
    }
}
