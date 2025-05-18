namespace api_cleany_app.src.Models
{
    public class UserProfileDto
    {
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? ImageUrl { get; set; }
        public string? Shift { get; set; }
    }
}
