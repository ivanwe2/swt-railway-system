namespace RailwaySystem.Console.Domain;

public class Passenger
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public RailcardType Railcard { get; set; }

    public bool IsChild => Age < 18;
}
