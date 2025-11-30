using RailwaySystem.Console.Application.Contracts;
using RailwaySystem.Console.Domain;

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

    public decimal CalculatePrice(Train train, Passenger passenger)
    {
        decimal price = train.BasePrice;

        foreach (var strategy in _strategies)
        {
            price = strategy.Calculate(price, train, passenger);
        }

        return Math.Round(price, 2);
    }
}