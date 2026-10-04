namespace TodoApi.Models
{
    public class StatusUpdateDto
    {
        public TodoStatus Status { get; set; }
        public bool Force { get; set; } = false;
    }
}