namespace SurveyChatbot.Repositories;

public interface IDataRepository<T>
{
    public Task<T?> GetByIdAsync(long id);
    public Task AddAsync(T data);
    public Task<bool> ContainsAsync(T data);
}