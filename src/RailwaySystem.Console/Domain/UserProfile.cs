namespace RailwaySystem.Console.Domain;

public class UserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public Passenger DefaultPassengerDetails { get; set; }

    public UserProfile()
    {
        DefaultPassengerDetails = new Passenger { Age = 30, Railcard = RailcardType.None };
        Username = string.Empty;
        Email = string.Empty;
        Address = string.Empty;
    }
}
