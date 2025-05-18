namespace api_cleany_app.src.Models
{
    public class RoutineJson
    {
        public string? Time { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public List<string?> DaysOfWeek { get; set; }
    }
}
