namespace api_cleany_app.src.Models
{
    public class Shift
    {
        public int ShiftId { get; set; }
        public string Name { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string? CreateAt { get; set; }
        public string UpdateAt { get; set; }
    }
}
