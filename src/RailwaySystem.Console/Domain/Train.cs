namespace RailwaySystem.Console.Domain;

public class Train
{
    public int Id { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public decimal BasePrice { get; set; }
}
