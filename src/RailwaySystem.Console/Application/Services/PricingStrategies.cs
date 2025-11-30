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

        // Rule: Rush Hour (Full Fare)
        // Lab 01: < 09:30 OR 16:00-19:30
        bool isMorningRush = time < new TimeSpan(9, 30, 0);
        bool isEveningRush = time >= new TimeSpan(16, 0, 0) && time <= new TimeSpan(19, 30, 0);

        if (isMorningRush || isEveningRush)
        {
            return train.BasePrice;
        }

        // Rule: Evening Discount (5%)
        // Lab 01: After 19:30
        if (time > new TimeSpan(19, 30, 0))
        {
            return currentPrice * 0.95m;
        }

        return currentPrice;
    }
}
