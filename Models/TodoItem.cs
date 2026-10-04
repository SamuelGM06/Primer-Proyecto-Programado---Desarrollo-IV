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
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public bool isCompleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? CompletedAt { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

    }

}