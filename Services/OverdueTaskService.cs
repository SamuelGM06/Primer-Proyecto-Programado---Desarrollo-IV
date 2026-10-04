using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Services;

public class OverdueTaskService
{
    private readonly TodoDbContext _context;
    private readonly INotifier _notifier;

    public OverdueTaskService(TodoDbContext context, INotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    
    public async Task<int> NotifyOverdueAsync(string? userId = null)
    {
        var now = DateTime.UtcNow;

        var query = _context.TodoItems.Where(t =>
            t.DueDate != null &&
            t.DueDate < now &&
            t.Status != TodoStatus.Completada &&
            t.Status != TodoStatus.Cancelada &&
            (t.NotifiedForDueDate == null || t.NotifiedForDueDate != t.DueDate));

        if (userId != null)
        {
            query = query.Where(t => t.UserId == userId);
        }

        var items = await query.ToListAsync();

        foreach (var item in items)
        {
            _notifier.Notify(item);
            item.NotifiedForDueDate = item.DueDate; 
        }

        await _context.SaveChangesAsync();
        return items.Count;
    }
}