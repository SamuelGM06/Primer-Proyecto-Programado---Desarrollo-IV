using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}