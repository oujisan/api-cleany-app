namespace api_cleany_app.src.Models
{
    public class    VerificationDto
    {
        public int? Assignment_id { get; set; }
        public int Verification_by { get; set; }
        public string? Status { get; set; }
        public string? Feedback { get; set; }
    }
}
