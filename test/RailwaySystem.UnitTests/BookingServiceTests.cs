using RailwaySystem.Console.Application.Services;
using RailwaySystem.Console.Domain;
using RailwaySystem.Console.Infrastructure.Repositories;

namespace RailwaySystem.UnitTests;

public class BookingServiceTests : IDisposable
{
    private readonly string _testFile;
    private readonly JsonRepository<Reservation> _repo;
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _testFile = $"test_bookings_{Guid.NewGuid()}.json";
        _repo = new JsonRepository<Reservation>(_testFile, r => r.Id);
        _service = new BookingService(_repo);
    }

    public void Dispose()
    {
        if (File.Exists(Path.Combine(AppContext.BaseDirectory, "Data", _testFile)))
        {
            File.Delete(Path.Combine(AppContext.BaseDirectory, "Data", _testFile));
        }
    }

    [Fact]
    public void AddToCart_ChangesState_To_InCart()
    {
        var res = new Reservation
        {
            Status = ReservationStatus.Booked, // Start with wrong state to prove override
            Train = new Train(),
            Passenger = new Passenger()
        };

        _service.AddToCart(res);

        Assert.Equal(ReservationStatus.InCart, res.Status);
        Assert.Single(_repo.GetAll());
    }

    [Fact]
    public void CancelReservation_ChangesState_To_Cancelled()
    {
        // Arrange
        var id = Guid.NewGuid();
        var res = new Reservation { Id = id, Status = ReservationStatus.Booked };
        _repo.Add(res);

        // Act
        _service.CancelReservation(id);

        // Assert
        var updated = _repo.GetById(id) ?? throw new ArgumentException("not found");
        Assert.Equal(ReservationStatus.Cancelled, updated.Status);
    }

    [Fact]
    public void GetMyReservations_Filters_Expired_Carts()
    {
        // Arrange
        var validCart = new Reservation { Status = ReservationStatus.InCart, CreatedAt = DateTime.Now };
        var expiredCart = new Reservation { Status = ReservationStatus.InCart, CreatedAt = DateTime.Now.AddDays(-8) };

        _repo.Add(validCart);
        _repo.Add(expiredCart);

        // Act
        var results = _service.GetMyReservations();

        // Assert
        Assert.Contains(results, r => r.Id == validCart.Id);
        Assert.DoesNotContain(results, r => r.Id == expiredCart.Id);
    }
}
