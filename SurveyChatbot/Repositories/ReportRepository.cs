using Microsoft.EntityFrameworkCore;
using SurveyChatbot.Database;
using SurveyChatbot.Models;

namespace SurveyChatbot.Repositories;

public class ReportRepository : IDataRepository<Report>
{
    private readonly DatabaseContext _context;

    public ReportRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Report[]> GetAllAsync()
    {
        return await _context.Reports.ToArrayAsync();
    }

    public async Task<Report?> GetByIdAsync(long id)
    {
        return await _context.Reports.FindAsync(id);
    }

    public async Task AddAsync(Report data)
    {
        await _context.Reports.AddAsync(data);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ContainsAsync(Report data)
    {
        return await _context.Reports.ContainsAsync(data);
    }

    public void Add(Report data)
    {
        _context.Reports.Add(data);
        _context.SaveChanges();
    }
}