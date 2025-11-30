using RailwaySystem.Console.Application.Contracts;
using RailwaySystem.Console.Domain;

namespace RailwaySystem.Console.Application.Services;

public class RailcardPricingStrategy : IPricingStrategy
{
    public decimal Calculate(decimal currentPrice, Train train, Passenger passenger)
    {
        if (passenger.Age >= 60 && passenger.Railcard == RailcardType.Over60s)
        {
            return currentPrice * 0.66m; // 34% discount
        }

        if (passenger.Age < 16)
        {
            if (passenger.Railcard == RailcardType.Family)
            {
                return currentPrice * 0.50m; // 50% discount
            }
            else
            {
                return currentPrice * 0.90m; // 10% discount
            }
        }

        return currentPrice;
    }
}

public class TimePricingStrategy : IPricingStrategy
{
    public decimal Calculate(decimal currentPrice, Train train, Passenger passenger)
    {
        var time = train.DepartureTime.TimeOfDay;

        // Lab 01: Rush Hour Definition
        bool isMorningRush = time < new TimeSpan(9, 30, 0);
        bool isEveningRush = time >= new TimeSpan(16, 0, 0) && time <= new TimeSpan(19, 30, 0);

        if (isMorningRush || isEveningRush)
        {
            return currentPrice;
        }

        // Lab 01: Evening Discount (5%)
        if (time > new TimeSpan(19, 30, 0))
        {
            return currentPrice * 0.95m;
        }

        // Default: Saver Time (Standard Price)
        return currentPrice;
    }
}
