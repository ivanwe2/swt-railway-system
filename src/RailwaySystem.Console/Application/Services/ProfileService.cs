using RailwaySystem.Console.Application.Contracts;
using RailwaySystem.Console.Domain;

namespace RailwaySystem.Console.Application.Services;

public class ProfileService
{
    private readonly IRepository<UserProfile> _repo;

    public ProfileService(IRepository<UserProfile> repo)
    {
        _repo = repo;
    }

    public void CreateProfile(string username, int age, RailcardType railcard)
    {
        var profile = new UserProfile
        {
            Username = username,
            DefaultPassengerDetails = new Passenger { Age = age, Railcard = railcard }
        };
        _repo.Add(profile);
    }

    public UserProfile? GetProfileByName(string username)
    {
        return _repo.GetAll().FirstOrDefault(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public void UpdateAddress(Guid id, string newAddress)
    {
        var profile = _repo.GetById(id);
        if (profile != null)
        {
            profile.Address = newAddress;
            _repo.Update(profile);
        }
    }

    public List<UserProfile> GetAllProfiles() => _repo.GetAll();
}