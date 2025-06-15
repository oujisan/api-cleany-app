using api_cleany_app.src.Models;


namespace api_cleany_app.src.Helpers
{
    public class ValidationHelper
    {
        public static bool validateRegistrationData(Registration registration)
        {
            return !string.IsNullOrEmpty(registration.FirstName) &&
                   !string.IsNullOrEmpty(registration.Username) &&
                   !string.IsNullOrEmpty(registration.Email) &&
                   !string.IsNullOrEmpty(registration.Password) &&
                   isEmailValid(registration.Email) &&
                   isPasswordValid(registration.Password) &&
                   isUsernameValid(registration.Username);
        }
        public static bool isEmailValid(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public static bool isPasswordValid(string password)
        {
            return password.Length >= 8 && password.Any(char.IsDigit);
        }
        public static bool isUsernameValid(string username)
        {
            return username.Length >= 3 && username.All(char.IsLetterOrDigit);
        }
        public static bool isImageUrlValid(string imageUrl)
        {
            return Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute) &&
                   (imageUrl.EndsWith(".jpg") || imageUrl.EndsWith(".png") || imageUrl.EndsWith(".jpeg"));
        }
    }
}