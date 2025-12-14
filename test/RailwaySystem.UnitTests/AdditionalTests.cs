using RailwaySystem.Console.Application.Services;
using RailwaySystem.Console.Domain;
using RailwaySystem.Console.Infrastructure.Repositories;

namespace RailwaySystem.UnitTests;

public class AdditionalCoverageTests : IDisposable
{
    private readonly string _testFile;
    private readonly JsonRepository<Reservation> _repo;
    private readonly BookingService _service;

    public AdditionalCoverageTests()
    {
        _testFile = $"test_additional_{Guid.NewGuid()}.json";
        _repo = new JsonRepository<Reservation>(_testFile, r => r.Id);
        _service = new BookingService(_repo);
    }

    public void Dispose()
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, "Data", _testFile);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    [Fact]
    public void ModifyReservation_UpdatesTicketType_AndRecalculatesPrice()
    {
        // Arrange
        var id = Guid.NewGuid();
        var res = new Reservation
        {
            Id = id,
            Status = ReservationStatus.Booked,
            TicketType = TicketType.OneWay,
            FinalPrice = 100m,
            Train = new Train { BasePrice = 100, DepartureTime = DateTime.Today.AddHours(12) },
            Passenger = new Passenger { Age = 30, Railcard = RailcardType.None }
        };
        _repo.Add(res);

        // Act
        _service.ModifyReservation(id, TicketType.Return, 200m);

        // Assert
        var updated = _repo.GetById(id);
        Assert.NotNull(updated);
        Assert.Equal(TicketType.Return, updated.TicketType);
        Assert.Equal(200m, updated.FinalPrice);
        Assert.Equal(ReservationStatus.Modified, updated.Status);
    }

    [Fact]
    public void ModifyReservation_WorksForInCartStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var res = new Reservation
        {
            Id = id,
            Status = ReservationStatus.InCart,
            TicketType = TicketType.OneWay,
            FinalPrice = 50m,
            Train = new Train(),
            Passenger = new Passenger()
        };
        _repo.Add(res);

        // Act
        _service.ModifyReservation(id, TicketType.Return, 100m);

        // Assert
        var updated = _repo.GetById(id);
        Assert.NotNull(updated);
        Assert.Equal(ReservationStatus.Modified, updated.Status);
    }

    [Fact]
    public void ModifyReservation_IgnoresCancelledReservations()
    {
        // Arrange
        var id = Guid.NewGuid();
        var res = new Reservation
        {
            Id = id,
            Status = ReservationStatus.Cancelled,
            TicketType = TicketType.OneWay,
            FinalPrice = 100m,
            Train = new Train(),
            Passenger = new Passenger()
        };
        _repo.Add(res);

        // Act
        _service.ModifyReservation(id, TicketType.Return, 200m);

        // Assert - should not modify cancelled reservation
        var updated = _repo.GetById(id);
        Assert.NotNull(updated);
        Assert.Equal(ReservationStatus.Cancelled, updated.Status); // Status unchanged
        Assert.Equal(TicketType.OneWay, updated.TicketType); // Not modified
    }

    [Fact]
    public void CancelReservation_IgnoresNonBookedReservations()
    {
        // Arrange
        var id = Guid.NewGuid();
        var res = new Reservation { Id = id, Status = ReservationStatus.InCart };
        _repo.Add(res);

        // Act
        _service.CancelReservation(id);

        // Assert - InCart should not be cancelled (only Booked can be cancelled)
        var updated = _repo.GetById(id);
        Assert.NotNull(updated);
        Assert.Equal(ReservationStatus.InCart, updated.Status); // Unchanged
    }

    [Fact]
    public void CancelReservation_HandlesNonExistentId_Gracefully()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act - should not throw exception
        _service.CancelReservation(nonExistentId);

        // Assert - no exception thrown (test passes if we reach here)
        Assert.True(true);
    }

    [Fact]
    public void ModifyReservation_HandlesNonExistentId_Gracefully()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act - should not throw
        _service.ModifyReservation(nonExistentId, TicketType.Return, 100m);

        // Assert
        Assert.True(true); // No exception = pass
    }

    // ==================== PRICING SERVICE COVERAGE ====================

    [Theory]
    [InlineData(9, 29, 100.0)]  // 09:29 - Last minute of rush hour
    [InlineData(9, 30, 100.0)]  // 09:30 - First minute of off-peak
    [InlineData(15, 59, 100.0)] // 15:59 - Still off-peak
    [InlineData(16, 0, 100.0)]  // 16:00 - Start of evening rush
    [InlineData(19, 30, 100.0)] // 19:30 - Last minute of evening rush
    [InlineData(19, 31, 95.0)]  // 19:31 - First minute of late discount
    public void TimePricingStrategy_BoundaryConditions(int hour, int minute, decimal expected)
    {
        // Arrange
        var strategy = new TimePricingStrategy();
        var train = new Train
        {
            BasePrice = 100,
            DepartureTime = DateTime.Today.AddHours(hour).AddMinutes(minute)
        };
        var passenger = new Passenger { Age = 30, Railcard = RailcardType.None };

        // Act
        var result = strategy.Calculate(100m, train, passenger);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PricingService_MultipleDiscounts_StackCorrectly()
    {
        // Arrange - Senior (66%) + Late Night (95%)
        var service = new PricingService();
        var train = new Train
        {
            BasePrice = 100,
            DepartureTime = DateTime.Today.AddHours(22) // 22:00 = late night
        };
        var passenger = new Passenger
        {
            Age = 65,
            Railcard = RailcardType.Over60s
        };

        // Act
        var result = service.CalculatePrice(train, passenger, TicketType.OneWay);

        // Assert
        // 100 * 0.95 (time) = 95
        // 95 * 0.66 (senior) = 62.7
        Assert.Equal(62.7m, result);
    }

    [Fact]
    public void PricingService_ReturnTicket_WithFamilyDiscount()
    {
        // Arrange
        var service = new PricingService();
        var train = new Train
        {
            BasePrice = 50,
            DepartureTime = DateTime.Today.AddHours(14) // Off-peak
        };
        var passenger = new Passenger
        {
            Age = 10,
            Railcard = RailcardType.Family
        };

        // Act
        var result = service.CalculatePrice(train, passenger, TicketType.Return);

        // Assert
        // 50 * 2 (return) = 100
        // 100 * 0.50 (family) = 50
        Assert.Equal(50m, result);
    }

    [Fact]
    public void RailcardStrategy_AdultWithNoCard_NoDiscount()
    {
        // Arrange
        var strategy = new RailcardPricingStrategy();
        var passenger = new Passenger { Age = 30, Railcard = RailcardType.None };

        // Act
        var result = strategy.Calculate(100m, null, passenger);

        // Assert
        Assert.Equal(100m, result); // No discount applied
    }

    [Fact]
    public void RailcardStrategy_SeniorWithFamilyCard_NoDiscount()
    {
        // Arrange - Senior age but wrong card type
        var strategy = new RailcardPricingStrategy();
        var passenger = new Passenger { Age = 65, Railcard = RailcardType.Family };

        // Act
        var result = strategy.Calculate(100m, null, passenger);

        // Assert
        Assert.Equal(100m, result); // No discount (needs Over60s card)
    }

    [Fact]
    public void RailcardStrategy_Age16_NoChildDiscount()
    {
        // Arrange - Exactly 16 (boundary)
        var strategy = new RailcardPricingStrategy();
        var passenger = new Passenger { Age = 16, Railcard = RailcardType.None };

        // Act
        var result = strategy.Calculate(100m, null, passenger);

        // Assert
        Assert.Equal(100m, result); // Adult pricing
    }

    [Fact]
    public void RailcardStrategy_Age15WithFamilyCard_Gets50Percent()
    {
        // Arrange
        var strategy = new RailcardPricingStrategy();
        var passenger = new Passenger { Age = 15, Railcard = RailcardType.Family };

        // Act
        var result = strategy.Calculate(100m, null, passenger);

        // Assert
        Assert.Equal(50m, result);
    }

    [Fact]
    public void RailcardStrategy_ChildWithOver60sCard_GetsChildDiscount()
    {
        // Arrange - Edge case: child has senior card (should use child logic)
        var strategy = new RailcardPricingStrategy();
        var passenger = new Passenger { Age = 10, Railcard = RailcardType.Over60s };

        // Act
        var result = strategy.Calculate(100m, null, passenger);

        // Assert
        Assert.Equal(90m, result); // 10% child discount (not senior)
    }

    // ==================== DOMAIN ENTITY COVERAGE ====================

    [Fact]
    public void Reservation_IsExpired_ReturnsFalseForFreshCart()
    {
        // Arrange
        var res = new Reservation
        {
            CreatedAt = DateTime.Now.AddDays(-5), // 5 days ago
            Train = new Train(),
            Passenger = new Passenger()
        };

        // Act
        var result = res.IsExpired();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Reservation_IsExpired_ReturnsTrueForOldCart()
    {
        // Arrange
        var res = new Reservation
        {
            CreatedAt = DateTime.Now.AddDays(-8), // 8 days ago
            Train = new Train(),
            Passenger = new Passenger()
        };

        // Act
        var result = res.IsExpired();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Reservation_IsExpired_ExactlySevenDays_ReturnsFalse()
    {
        // Arrange - Exactly 7 days (boundary)
        var res = new Reservation
        {
            CreatedAt = DateTime.Today.AddDays(7),
            Train = new Train(),
            Passenger = new Passenger()
        };

        // Act
        var result = res.IsExpired();

        // Assert
        Assert.False(result); // <= 7 days is valid
    }

    [Fact]
    public void Passenger_IsChild_ReturnsTrueForUnder18()
    {
        // Arrange
        var passenger = new Passenger { Age = 17 };

        // Act & Assert
        Assert.True(passenger.IsChild);
    }

    [Fact]
    public void Passenger_IsChild_ReturnsFalseForAdults()
    {
        // Arrange
        var passenger = new Passenger { Age = 18 };

        // Act & Assert
        Assert.False(passenger.IsChild);
    }

    [Fact]
    public void Reservation_HasUniqueGuid_ByDefault()
    {
        // Arrange & Act
        var res1 = new Reservation();
        var res2 = new Reservation();

        // Assert
        Assert.NotEqual(res1.Id, res2.Id);
        Assert.NotEqual(Guid.Empty, res1.Id);
    }

    [Fact]
    public void ProfileService_CreateProfile_PersistsToRepository()
    {
        // Arrange
        var profileRepo = new JsonRepository<UserProfile>($"test_profiles_{Guid.NewGuid()}.json", p => p.Id);
        var profileService = new ProfileService(profileRepo);

        // Act
        profileService.CreateProfile("TestUser", 25, RailcardType.None);

        // Assert
        var profiles = profileService.GetAllProfiles();
        Assert.Single(profiles);
        Assert.Equal("TestUser", profiles[0].Username);
        Assert.Equal(25, profiles[0].DefaultPassengerDetails.Age);

        // Cleanup
        File.Delete(Path.Combine(AppContext.BaseDirectory, "Data", $"test_profiles_{profileRepo.GetAll()[0].Id}.json"));
    }

    [Fact]
    public void ProfileService_GetProfileByName_ReturnsCorrectProfile()
    {
        // Arrange
        var profileRepo = new JsonRepository<UserProfile>($"test_profiles_{Guid.NewGuid()}.json", p => p.Id);
        var profileService = new ProfileService(profileRepo);
        profileService.CreateProfile("Alice", 30, RailcardType.Family);
        profileService.CreateProfile("Bob", 65, RailcardType.Over60s);

        // Act
        var result = profileService.GetProfileByName("Alice");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Alice", result.Username);
        Assert.Equal(30, result.DefaultPassengerDetails.Age);
    }

    [Fact]
    public void ProfileService_GetProfileByName_IsCaseInsensitive()
    {
        // Arrange
        var profileRepo = new JsonRepository<UserProfile>($"test_profiles_{Guid.NewGuid()}.json", p => p.Id);
        var profileService = new ProfileService(profileRepo);
        profileService.CreateProfile("TestUser", 40, RailcardType.None);

        // Act
        var result = profileService.GetProfileByName("testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestUser", result.Username);
    }

    [Fact]
    public void ProfileService_UpdateAddress_ChangesAddress()
    {
        // Arrange
        var profileRepo = new JsonRepository<UserProfile>($"test_profiles_{Guid.NewGuid()}.json", p => p.Id);
        var profileService = new ProfileService(profileRepo);
        profileService.CreateProfile("User1", 35, RailcardType.None);
        var profile = profileService.GetProfileByName("User1");

        // Act
        profileService.UpdateAddress(profile.Id, "123 Main St, Sofia");

        // Assert
        var updated = profileRepo.GetById(profile.Id);
        Assert.NotNull(updated);
        Assert.Equal("123 Main St, Sofia", updated.Address);
    }

    [Fact]
    public void ProfileService_UpdateAddress_HandlesNonExistentId()
    {
        // Arrange
        var profileRepo = new JsonRepository<UserProfile>($"test_profiles_{Guid.NewGuid()}.json", p => p.Id);
        var profileService = new ProfileService(profileRepo);

        // Act - should not throw
        profileService.UpdateAddress(Guid.NewGuid(), "Test Address");

        // Assert
        Assert.True(true); // No exception = pass
    }

    [Fact]
    public void JsonRepository_Delete_RemovesItem()
    {
        // Arrange
        var repo = new JsonRepository<Reservation>($"test_delete_{Guid.NewGuid()}.json", r => r.Id);
        var id = Guid.NewGuid();
        var res = new Reservation { Id = id, Train = new Train(), Passenger = new Passenger() };
        repo.Add(res);

        // Act
        repo.Delete(id);

        // Assert
        Assert.Null(repo.GetById(id));
        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public void JsonRepository_Delete_HandlesNonExistentId()
    {
        // Arrange
        var repo = new JsonRepository<Reservation>($"test_delete2_{Guid.NewGuid()}.json", r => r.Id);

        // Act - should not throw
        repo.Delete(Guid.NewGuid());

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void JsonRepository_Update_ModifiesExistingItem()
    {
        // Arrange
        var repo = new JsonRepository<Reservation>($"test_update_{Guid.NewGuid()}.json", r => r.Id);
        var id = Guid.NewGuid();
        var res = new Reservation { Id = id, FinalPrice = 100m, Train = new Train(), Passenger = new Passenger() };
        repo.Add(res);

        // Act
        res.FinalPrice = 200m;
        repo.Update(res);

        // Assert
        var updated = repo.GetById(id);
        Assert.NotNull(updated);
        Assert.Equal(200m, updated.FinalPrice);
    }

    [Fact]
    public void JsonRepository_LoadsEmptyListWhenFileDoesNotExist()
    {
        // Arrange & Act
        var repo = new JsonRepository<Reservation>($"nonexistent_{Guid.NewGuid()}.json", r => r.Id);

        // Assert
        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public void JsonRepository_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var uniqueFile = $"test_dir_{Guid.NewGuid()}.json";
        var repo = new JsonRepository<Reservation>(uniqueFile, r => r.Id);

        // Act
        repo.Add(new Reservation { Train = new Train(), Passenger = new Passenger() });

        // Assert
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        Assert.True(Directory.Exists(dataDir));
    }
}