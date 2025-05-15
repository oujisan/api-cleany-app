namespace api_cleany_app.src.Models
{
    public class Area
    {
        public int AreaId { get; set; }
        public string name { get; set; }
        public int floor { get; set; }
        public string building { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
    }
}
