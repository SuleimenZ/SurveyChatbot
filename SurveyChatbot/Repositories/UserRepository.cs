using Microsoft.EntityFrameworkCore;
using SurveyChatbot.Database;
using SurveyChatbot.Models;

namespace SurveyChatbot.Repositories;

public class UserRepository : IDataRepository<User>
{
    private readonly DatabaseContext _context;

    public UserRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<User[]> GetAllAsync()
    {
        return await _context.Users.ToArrayAsync();
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task AddAsync(User data)
    {
        await _context.Users.AddAsync(data);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ContainsAsync(User data)
    {
        return await _context.Users.ContainsAsync(data);
    }

    public void Add(User data)
    {
        _context.Users.Add(data);
        _context.SaveChanges();
    }
}