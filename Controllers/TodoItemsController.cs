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

        // Helper para obtener el ID del usuario autenticado desde las Claims del JWT[cite: 5, 9]
        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> getAllItems(
            [FromQuery] TodoStatus? status, 
            [FromQuery] bool? overdue, 
            [FromQuery] int? categoryId)
        {
            var userId = GetUserId();
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
                query = query.Where(t => t.DueDate.HasValue && t.DueDate < now 
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
            var userId = GetUserId();
            var todoItem = await _context.TodoItems
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todoItem == null) return NotFound();

            return Ok(todoItem);
        }

        [HttpPost]
        public async Task<ActionResult<TodoItem>> CreateTodoItem(TodoItem todoItem)
        {
            if (todoItem.CategoryId.HasValue)
            {
                bool categoriaExiste = await _context.Categories
                    .AnyAsync(c => c.Id == todoItem.CategoryId.Value);
                if (!categoriaExiste) return BadRequest("The specified category does not exist.");
            }

            todoItem.UserId = GetUserId();
            todoItem.Status = TodoStatus.Pendiente; 
            todoItem.CreatedAt = DateTime.UtcNow;

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateTodoItem(int id, TodoItem updated)
        {
            var userId = GetUserId();
            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todoItem == null) return NotFound();

            if (updated.CategoryId.HasValue)
            {
                bool categoriaExiste = await _context.Categories
                    .AnyAsync(c => c.Id == updated.CategoryId.Value);
                if (!categoriaExiste) return BadRequest("The specified category does not exist.");
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
            var userId = GetUserId();
            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todoItem == null) return NotFound();

            // Req 02: Validar la matriz de transiciones permitidas[cite: 2]
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
                return BadRequest($"Invalid status transition from '{todoItem.Status}' to '{dto.Status}'.");
            }

            // Req 03: Bloquear cierre de tareas vencidas a menos que se confirme explícitamente[cite: 3]
            if (dto.Status == TodoStatus.Completada && todoItem.DueDate.HasValue && todoItem.DueDate < DateTime.UtcNow)
            {
                if (!dto.Force)
                {
                    return BadRequest("Task is overdue. Confirm explicitly to complete it.");
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
            var userId = GetUserId();
            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todoItem == null) return NotFound();

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class StatusUpdateDto
    {
        public TodoStatus Status { get; set; }
        public bool Force { get; set; } = false;
    }
}