using TodoApi.Models;

namespace TodoApi.Services;

public interface INotifier
{
    void Notify(TodoItem item);
}