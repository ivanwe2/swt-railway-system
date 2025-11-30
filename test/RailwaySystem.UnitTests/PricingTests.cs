using RailwaySystem.Console.Application.Services;
using RailwaySystem.Console.Domain;

namespace RailwaySystem.UnitTests;

public class PricingTests
{

    [Theory]
    [InlineData(65, RailcardType.Over60s, 66.0)] // T, T -> True (34% off)
    [InlineData(65, RailcardType.None, 100.0)]   // T, F -> False
    [InlineData(50, RailcardType.Over60s, 100.0)]// F, T -> False
    [InlineData(50, RailcardType.None, 100.0)]   // F, F -> False
    public void RailcardStrategy_SeniorCondition_Check(int age, RailcardType card, decimal expected)
    {
        // Arrange
        var strategy = new RailcardPricingStrategy();
        var passenger = new Passenger { Age = age, Railcard = card };

        // Act
        var result = strategy.Calculate(100m, null, passenger);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(15, 90.0)]  // Child (10% off default)
    [InlineData(16, 100.0)] // Adult (Full Fare)
    public void RailcardStrategy_ChildBoundary_Check(int age, decimal expected)
    {
        var strategy = new RailcardPricingStrategy();
        var passenger = new Passenger { Age = age, Railcard = RailcardType.None };

        var result = strategy.Calculate(100m, null, passenger);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void RailcardStrategy_FamilyCard_Apply50Percent()
    {
        var strategy = new RailcardPricingStrategy();
        var passenger = new Passenger { Age = 12, Railcard = RailcardType.Family };

        var result = strategy.Calculate(100m, null, passenger);

        Assert.Equal(50m, result); // 50% of 100
    }

    [Theory]
    [InlineData(8, 0, 100)]  // 08:00 (Morning Rush) -> Full Fare
    [InlineData(17, 0, 100)] // 17:00 (Evening Rush) -> Full Fare
    public void PricingService_RushHour_OverridesDiscounts(int hour, int minute, decimal expected)
    {
        var service = new PricingService();
        var train = new Train { BasePrice = 100, DepartureTime = DateTime.Today.AddHours(hour).AddMinutes(minute) };
        var passenger = new Passenger { Age = 30, Railcard = RailcardType.None };

        var result = service.CalculatePrice(train, passenger);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void PricingService_Cumulative_Discount_Check()
    {
        var service = new PricingService();
        // 20:00 is Late Night
        var train = new Train { BasePrice = 100, DepartureTime = DateTime.Today.AddHours(20) };
        var passenger = new Passenger { Age = 65, Railcard = RailcardType.Over60s };

        var result = service.CalculatePrice(train, passenger);

        // 100 * 0.95 = 95. 95 * 0.66 = 62.7
        Assert.Equal(62.7m, result);
    }

    [Fact]
    public void CalculatePrice_ReturnTicket_DoublesBasePrice()
    {
        // Arrange
        var service = new PricingService();
        var train = new Train { BasePrice = 100, DepartureTime = DateTime.Today.AddHours(12) };
        var passenger = new Passenger { Age = 30, Railcard = RailcardType.None };

        // Act
        var result = service.CalculatePrice(train, passenger, TicketType.Return);

        // Assert
        Assert.Equal(200m, result);
    }
}
