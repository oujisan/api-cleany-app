namespace api_cleany_app.src.Models
{
    public class TaskForm
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string CreatedBy { get; set; }
        public List<Area> Area { get; set; }
        public string TaskType { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
    }
}
