namespace api_cleany_app.src.Models
{
    public class Verification
    {
        public int Verification_id { get; set; }
        public int Assignment_id { get; set; }
        public UserCard? Verification_by { get; set; }
        public string Status { get; set; }
        public string? Feedback { get; set; }
        public string? Verification_at { get; set; }
    }
}
