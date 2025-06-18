namespace api_cleany_app.src.Models
{
    public class Shift
    {
        public int ShiftId { get; set; }
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string? CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }
}
