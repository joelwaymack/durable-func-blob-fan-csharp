using System;
using System.Collections.Generic;
using Bogus;
using Company.Function.Models;

namespace Company.Function.Generators;

public class OrderGenerator
{
    private readonly Faker<Order> orderFaker;

    public OrderGenerator()
    {
        orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => Guid.NewGuid())
            .RuleFor(o => o.FirstName, f => f.Name.FirstName())
            .RuleFor(o => o.LastName, f => f.Name.LastName())
            .RuleFor(o => o.Address, f => f.Address.FullAddress())
            .RuleFor(o => o.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(o => o.Total, f => f.Random.Decimal(0, 100));
    }

    public IList<Order> Generate(int count)
    {
        return orderFaker.Generate(count);
    }
}