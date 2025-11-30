using RailwaySystem.Console.Application.Contracts;
using RailwaySystem.Console.Domain;
using System.Net.Sockets;

namespace RailwaySystem.Console.Application.Services;

public class PricingService
{
    private readonly IEnumerable<IPricingStrategy> _strategies;

    public PricingService()
    {
        _strategies =
        [
            new TimePricingStrategy(),
            new RailcardPricingStrategy()
        ];
    }

    public decimal CalculatePrice(Train train, Passenger passenger, TicketType ticketType = TicketType.OneWay)
    {
        decimal multiplier = (ticketType == TicketType.Return) ? 2.0m : 1.0m;
        decimal price = train.BasePrice * multiplier;

        foreach (var strategy in _strategies)
        {
            price = strategy.Calculate(price, train, passenger);
        }

        return Math.Round(price, 2);
    }
}