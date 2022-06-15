using Microsoft.EntityFrameworkCore;
using SurveyChatbot.Database;
using SurveyChatbot.Models;

namespace SurveyChatbot.Repositories;

public class QuestionRepository : IDataRepository<Question>
{
    private readonly DatabaseContext _context;

    public QuestionRepository(DatabaseContext databaseContext)
    {
        _context = databaseContext;
    }

    public async Task<Question?> GetByIdAsync(long id)
    {
        return await _context.Questions.FindAsync(id);
    }

    public async Task AddAsync(Question data)
    {
        await _context.Questions.AddAsync(data);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ContainsAsync(Question data)
    {
        return await _context.Questions.ContainsAsync(data);
    }
}