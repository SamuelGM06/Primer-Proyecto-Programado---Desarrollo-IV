using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TodoApi.Models
{
    public class TodoItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public TodoStatus Status { get; set; } = TodoStatus.Pendiente;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public DateTime? DueDate { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;
    }
}