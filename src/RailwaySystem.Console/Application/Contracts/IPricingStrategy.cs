using RailwaySystem.Console.Domain;

namespace RailwaySystem.Console.Application.Contracts;

public interface IPricingStrategy
{
    decimal Calculate(decimal currentPrice, Train train, Passenger passenger);
}
