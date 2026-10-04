using TodoApi.Models;

namespace TodoApi.Services;

public class LogNotifier : INotifier
{
   private readonly ILogger<LogNotifier> _logger;

   public LogNotifier(ILogger<LogNotifier> logger)
   {
       _logger = logger;
   }
   public void Notify(TodoItem item)
    {
        _logger.LogWarning("Overdue task: Id={Id}, Title={Title}, DueDate={DueDate}, UserId={UserId}",
            item.Id, item.Title, item.DueDate, item.UserId);
    }
    
}