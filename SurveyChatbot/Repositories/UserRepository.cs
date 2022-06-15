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

    public async Task<User?> GetByIdAsync(long id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task AddAsync(User data)
    {
        await _context.Users.AddAsync(data);
    }

    public async Task<bool> ContainsAsync(User data)
    {
        return await _context.Users.ContainsAsync(data);
    }
}