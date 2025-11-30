namespace RailwaySystem.Console.Domain;

public enum ReservationStatus { InCart, Booked, Cancelled, Modified }

public class Reservation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Train Train { get; set; }
    public Passenger Passenger { get; set; }
    public decimal FinalPrice { get; set; }
    public ReservationStatus Status { get; set; }
    public TicketType TicketType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsExpired() => (DateTime.Now - CreatedAt).TotalDays > 7;
}
