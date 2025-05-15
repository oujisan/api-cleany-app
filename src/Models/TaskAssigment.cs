namespace api_cleany_app.src.Models
{
    public class TaskAssigment
    {
        public int AssigmentId { get; set; }
        public TaskForm TaskForm { get; set; }
        public string? ImageUrl { get; set; }
        public string Date { get; set; }
        public string WorkedBy { get; set; }
        public string Status { get; set; }
        public string? AssigmentAt { get; set; }
        public string? CompleteAt { get; set; }
    }
}
