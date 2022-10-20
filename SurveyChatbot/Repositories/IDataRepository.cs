namespace SurveyChatbot.Repositories;

public interface IDataRepository<T>
{
    public Task<T[]> GetAllAsync();
    public Task<T?> GetByIdAsync(long id);
    public Task AddAsync(T data);
    public void Add(T data);
    public Task<bool> ContainsAsync(T data);
}