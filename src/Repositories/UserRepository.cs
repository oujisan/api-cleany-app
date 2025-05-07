using api_cleany_app.src.Helpers;

namespace api_cleany_app.src.Repositories
{
    public class UserRepository
    {
        private string _connectionString;
        private string _errorMessage;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = DbConfig.ConnectionString;
        }
    }
}