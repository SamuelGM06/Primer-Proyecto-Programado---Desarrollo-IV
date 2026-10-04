using System.Dynamic;
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

        [HttpGet]
        public async Task<ActionResult<TodoItem>> getAllItems([FromQuery] bool? completed, [FromQuery] int? categoryId)
        {
            var query =  _context.TodoItems.Include(t => t.Category).AsQueryable();

            if (completed.HasValue)
            {
                query = query.Where(t => t.Status == TodoStatus.Completada);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            

            return Ok(query);
        }

        [HttpGet("{id:int}")]


        public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
        {
            var todoItem = await _context.TodoItems
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);

            if(todoItem == null) return NotFound();

            return Ok(todoItem);
        }

        [HttpPost]
        public async Task<ActionResult<TodoItem>> CreateTodoItem(TodoItem todoItem)
        {

            if (todoItem.CategoryId.HasValue)
            {
                bool categoriaExiste = await _context.Categories
                .AnyAsync(c => c.Id == todoItem.CategoryId.Value);
                if (!categoriaExiste)
                {
                    return BadRequest();
                }
            }
            
            _context.TodoItems.Add(todoItem);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof (GetTodoItem), new {id = todoItem.Id}, todoItem);

        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateTodoItem(int id, TodoItem updated)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if(todoItem == null) return NotFound();

            if (updated.CategoryId.HasValue)
            {
                bool categoriaExiste = await _context.Categories
                .AnyAsync(c => c.Id == updated.CategoryId.Value);
                if (!categoriaExiste)
                {
                    return BadRequest();
                }
            }

            todoItem.Title = updated.Title;
            todoItem.Description = updated.Description;

            todoItem.CompletedAt = updated.Status == TodoStatus.Completada ? DateTime.Now : null;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id:int}/toggle")]
        public async Task<ActionResult<TodoItem>> ToggleTodoItem(int id)
        {
            var todoItem = _context.TodoItems.Find(id);

            if(todoItem == null) return NotFound();

            todoItem.Status = todoItem.Status == TodoStatus.Completada ? TodoStatus.Pendiente : TodoStatus.Completada;
            todoItem.CompletedAt = todoItem.Status == TodoStatus.Completada ? DateTime.Now : null;

            await _context.SaveChangesAsync();

            return Ok(todoItem);
        }

        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<TodoItem>> UpdateTodoItemStatus(int id, TodoStatus status)
        {
            var todoItem = _context.TodoItems.Find(id);

            if(todoItem == null) return NotFound();

            todoItem.Status = status;
            todoItem.CompletedAt = status == TodoStatus.Completada ? DateTime.Now : null;

            await _context.SaveChangesAsync();

            return Ok(todoItem);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<TodoItem>> DeleteTodoItem(int id)
        {
            var todoItem = _context.TodoItems.Find(id);

            if(todoItem == null) return NotFound();

            _context.TodoItems.Remove(todoItem);

            await _context.SaveChangesAsync();

            return NoContent();
        }



       


        
    }
}