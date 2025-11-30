namespace RailwaySystem.Console.Application.Contracts;

public interface IRepository<T>
{
    T? GetById(Guid id);
    List<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(Guid id);
}
