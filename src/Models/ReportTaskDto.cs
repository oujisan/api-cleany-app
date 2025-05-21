namespace api_cleany_app.src.Models
{
    public class ReportTaskDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public List<string?> ImageUrl { get; set; }
        public int AreaId { get; set; }
    }
}