using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Company.Function.Generators;

namespace Company.Function.Handlers;

public static class OrderApiHandler
{
    private static OrderGenerator orderGenerator = new OrderGenerator();

    [FunctionName("GenerateOrders")]
    public static async Task<IActionResult> GenerateOrders(
        [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "post",
                Route = "orders")] HttpRequest req,
        [Blob("%BlobInputPath%/{DateTime}.json", FileAccess.Write, Connection = "BlobConnection")] TextWriter ordersBlob,
        ILogger log)
    {
        // Get the number of orders to generate.
        string strCount = req.Query["count"];

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        strCount = strCount ?? data?.count ?? "10";

        if (int.TryParse(strCount, out int count) && count > 0)
        {
            var orders = orderGenerator.Generate(count);
            var json = JsonConvert.SerializeObject(orders);
            await ordersBlob.WriteAsync(json);
        }

        return new OkObjectResult($"Generated {strCount} orders");
    }
}