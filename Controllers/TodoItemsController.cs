using System.Dynamic;
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
        public async Task<ActionResult<TodoItem>> getAllItems([FromQuery] bool? completed, [FromQuery] int? categoryId)
        {
            var userId = CurrentUserID;
            //solo trae los todoitems que tengan el mismo user id del token
            var query =  _context.TodoItems.Include(t => t.Category).Where(t => t.UserId == userId).AsQueryable();

            if (completed.HasValue)
            {
                query = query.Where(t => t.isCompleted == completed.Value);
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

            var userId = CurrentUserID;

            if (userId == null)
            {
                return Unauthorized();
            }

            var todoItem = await _context.TodoItems
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);

            if(todoItem == null) return NotFound();

            return Ok(todoItem);
        }

        [HttpPost]
        public async Task<ActionResult<TodoItem>> CreateTodoItem(TodoItem todoItem)
        {
            var userId = CurrentUserID;

            if (userId == null)
            {
                return Unauthorized();
            }

            if (todoItem.CategoryId.HasValue)
            {
                bool categoriaExiste = await _context.Categories
                .AnyAsync(c => c.Id == todoItem.CategoryId.Value);
                if (!categoriaExiste)
                {
                    return BadRequest();
                }
            }

            todoItem.Id = 0;

            todoItem.UserId = userId;
            
            _context.TodoItems.Add(todoItem);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof (GetTodoItem), new {id = todoItem.Id}, todoItem);

        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateTodoItem(int id, TodoItem updated)
        {

            var userId = CurrentUserID;

            if (userId == null)
            {
                return Unauthorized();
            }

            var todoItem = await _context.TodoItems.FindAsync(id);

            if(todoItem == null) return NotFound();

            if (todoItem.CategoryId.HasValue)
            {
                bool categoriaExiste = await _context.Categories
                .AnyAsync(c => c.Id == todoItem.CategoryId.Value);
                if (!categoriaExiste)
                {
                    return BadRequest();
                }
            }

            todoItem.Title = updated.Title;
            todoItem.Description = updated.Description;
            todoItem.isCompleted = updated.isCompleted;

            todoItem.CompletedAt = updated.isCompleted ? DateTime.Now : null;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id:int}/toggle")]
        public async Task<ActionResult<TodoItem>> ToggleTodoItem(int id)
        {

            var userId = CurrentUserID;

            if (userId == null)
            {
                return Unauthorized();
            }

            var todoItem = _context.TodoItems.Find(id);

            if(todoItem == null) return NotFound();

            todoItem.isCompleted = !todoItem.isCompleted;
            todoItem.CompletedAt = todoItem.isCompleted ? DateTime.Now : null;

            await _context.SaveChangesAsync();

            return Ok(todoItem);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<TodoItem>> DeleteTodoItem(int id)
        {

            
            var userId = CurrentUserID;

            if (userId == null)
            {
                return Unauthorized();
            }

            var todoItem = _context.TodoItems.Find(id);

            if(todoItem == null) return NotFound();

            _context.TodoItems.Remove(todoItem);

            await _context.SaveChangesAsync();

            return NoContent();
        }



       


        
    }
}