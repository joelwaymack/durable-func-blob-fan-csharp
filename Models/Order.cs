using System;
using Newtonsoft.Json;

namespace Company.Function.Models;

public class Order
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("firstName")]
    public string FirstName { get; set; }

    [JsonProperty("lastName")]
    public string LastName { get; set; }

    [JsonProperty("address")]
    public string Address { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("total")]
    public decimal Total { get; set; }
}