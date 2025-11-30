using RailwaySystem.Console.Application.Contracts;
using System.Text.Json;

namespace RailwaySystem.Console.Infrastructure.Repositories;

public class JsonRepository<T> : IRepository<T> where T : class
{
    private readonly string _filePath;
    private List<T> _data = [];
    private readonly Func<T, Guid> _idSelector;

    public JsonRepository(string fileName, Func<T, Guid> idSelector)
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, "Data", fileName);
        _idSelector = idSelector;
        Load();
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
        {
            _data = new List<T>();
            return;
        }
        string json = File.ReadAllText(_filePath);
        _data = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }

    public void Save()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(_data, options);

        var dir = Path.GetDirectoryName(_filePath)
            ?? throw new ArgumentNullException("directory path is null");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(_filePath, json);
    }

    public void Add(T entity)
    {
        _data.Add(entity);
        Save();
    }

    public void Delete(Guid id)
    {
        var item = GetById(id);
        if (item != null)
        {
            _data.Remove(item);
            Save();
        }
    }

    public List<T> GetAll() => _data;

    public T? GetById(Guid id)
    {
        return _data.FirstOrDefault(e => _idSelector(e) == id);
    }

    public void Update(T entity)
    {
        var id = _idSelector(entity);
        var existing = GetById(id);
        if (existing != null)
        {
            _data.Remove(existing);
            _data.Add(entity);
            Save();
        }
    }
}