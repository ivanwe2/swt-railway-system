using RailwaySystem.Console.Application.Contracts;
using RailwaySystem.Console.Domain;

namespace RailwaySystem.Console.Application.Services;

public class BookingService
{
    private readonly IRepository<Reservation> _repo;

    public BookingService(IRepository<Reservation> repo)
    {
        _repo = repo;
    }

    public void AddToCart(Reservation reservation)
    {
        reservation.Status = ReservationStatus.InCart;
        _repo.Add(reservation);
    }

    public void CancelReservation(Guid id)
    {
        var res = _repo.GetById(id);
        if (res != null && res.Status == ReservationStatus.Booked)
        {
            res.Status = ReservationStatus.Cancelled;
            _repo.Update(res);
        }
    }

    public void ModifyReservation(Guid id, TicketType newTicketType, decimal newPrice)
    {
        var res = _repo.GetById(id);
        // We can only modify active bookings or cart items
        if (res != null && (res.Status == ReservationStatus.Booked || res.Status == ReservationStatus.InCart))
        {
            res.TicketType = newTicketType;
            res.FinalPrice = newPrice;
            res.Status = ReservationStatus.Modified; // Track state change
            _repo.Update(res);
        }
    }

    public List<Reservation> GetMyReservations()
    {
        return _repo.GetAll().Where(r => !r.IsExpired()).ToList();
    }
}
