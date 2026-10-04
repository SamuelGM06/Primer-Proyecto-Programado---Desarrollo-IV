using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoDbContext _context;

        public TodoItemsController(TodoDbContext context)
        {
            _context = context;
        }

        private string? CurrentUserID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> getAllItems(
            [FromQuery] TodoStatus? status, 
            [FromQuery] bool? overdue, 
            [FromQuery] int? categoryId)
        {
            var userId = CurrentUserID;
            if (userId == null) return Unauthorized();

            var query = _context.TodoItems
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (overdue.HasValue && overdue.Value)
            {
                var now = DateTime.UtcNow;
                query = query.Where(t => t.DueDate.HasValue 
                                         && t.DueDate < now 
                                         && t.Status != TodoStatus.Completada 
                                         && t.Status != TodoStatus.Cancelada);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            return Ok(await query.ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
        {
            var userId = CurrentUserID;
            if (userId == null) return Unauthorized();

            var todoItem = await _context.TodoItems
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todoItem == null) return NotFound();

            return Ok(todoItem);
        }

        [HttpGet("stats")]
        public async Task<ActionResult> GetStats()
        {
            var userId = CurrentUserID;
            if (userId == null) return Unauthorized();

            var tasks = _context.TodoItems.Where(t => t.UserId == userId);

            var total = await tasks.CountAsync();

            var byStatus = await tasks
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var now = DateTime.UtcNow;
            var overdue = await tasks.CountAsync(t =>
                t.Status != TodoStatus.Completada && 
                t.Status != TodoStatus.Cancelada && 
                t.DueDate != null && 
                t.DueDate < now);

            var completedDates = await tasks
                .Where(t => t.Status == TodoStatus.Completada && t.CompletedAt != null)
                .Select(t => new { t.CreatedAt, CompletedAt = t.CompletedAt!.Value })
                .ToListAsync();

            double? averageDays = completedDates.Count == 0
                ? null
                : Math.Round(completedDates.Average(t => (t.CompletedAt - t.CreatedAt).TotalDays), 2);

            return Ok(new
            {
                total,
                byStatus,
                overdue,
                averageCompletionDays = averageDays
            });
        }

        [HttpPost]
        public async Task<ActionResult<TodoItem>> CreateTodoItem(TodoItem todoItem)
        {
            var userId = CurrentUserID;
            if (userId == null) return Unauthorized();

            if (todoItem.CategoryId.HasValue)
            {
                bool categoriaExiste = await _context.Categories
                    .AnyAsync(c => c.Id == todoItem.CategoryId.Value);
                if (!categoriaExiste) return BadRequest("La categoría especificada no existe.");
            }

            todoItem.Id = 0;
            todoItem.UserId = userId;
            todoItem.Status = TodoStatus.Pendiente;
            todoItem.CreatedAt = DateTime.UtcNow;

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateTodoItem(int id, TodoItem updated)
        {
            var userId = CurrentUserID;
            if (userId == null) return Unauthorized();

            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todoItem == null) return NotFound();

            if (updated.CategoryId.HasValue)
            {
                bool categoriaExiste = await _context.Categories
                    .AnyAsync(c => c.Id == updated.CategoryId.Value);
                if (!categoriaExiste) return BadRequest("La categoría especificada no existe.");
            }

            todoItem.Title = updated.Title;
            todoItem.Description = updated.Description;
            todoItem.CategoryId = updated.CategoryId;
            todoItem.DueDate = updated.DueDate;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<TodoItem>> UpdateTodoItemStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            var userId = CurrentUserID;
            if (userId == null) return Unauthorized();

            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todoItem == null) return NotFound();

            bool isValidTransition = (todoItem.Status, dto.Status) switch
            {
                (TodoStatus.Pendiente, TodoStatus.EnProgreso) => true,
                (TodoStatus.Pendiente, TodoStatus.Cancelada) => true,
                (TodoStatus.EnProgreso, TodoStatus.Completada) => true,
                (TodoStatus.EnProgreso, TodoStatus.Cancelada) => true,
                _ => false
            };

            if (!isValidTransition)
            {
                return BadRequest($"Transición no permitida de '{todoItem.Status}' a '{dto.Status}'.");
            }

            if (dto.Status == TodoStatus.Completada && todoItem.DueDate.HasValue && todoItem.DueDate < DateTime.UtcNow)
            {
                if (!dto.Force)
                {
                    return BadRequest("La tarea está vencida. Se requiere confirmación explícita para completarla.");
                }
            }

            todoItem.Status = dto.Status;
            todoItem.CompletedAt = dto.Status == TodoStatus.Completada ? DateTime.UtcNow : null;

            await _context.SaveChangesAsync();
            return Ok(todoItem);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteTodoItem(int id)
        {
            var userId = CurrentUserID;
            if (userId == null) return Unauthorized();

            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todoItem == null) return NotFound();

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}