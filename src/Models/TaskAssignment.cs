namespace api_cleany_app.src.Models
{
    public class TaskAssignment
    {
        public int AssigmentId { get; set; }
        public TaskDto Task { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public string WorkedBy { get; set; }
        public List<string?> ProofImageUrl { get; set; }
        public string? CreatedAt { get; set; }
        public string? AssigmentAt { get; set; }
        public string? CompleteAt { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
    }
}
