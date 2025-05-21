namespace api_cleany_app.src.Models
{
    public class RoutineTaskDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public List<string?> ImageUrl { get; set; }
        public int AreaId { get; set; }
        public string? Time { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public List<int?> DaysOfWeek { get; set; }
    }
}
