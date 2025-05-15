using api_cleany_app.src.Helpers;
using Npgsql;

namespace api_cleany_app.src.Services
{
    public class RoleService
    {
        private string _connectionString;
        private string _errorMessage = string.Empty;

        public RoleService()
        {
            _connectionString = DbConfig.ConnectionString;
        }

        public int? getRoleIdByName(string roleName)
        {
            string query = @"SELECT role_id FROM roles WHERE LOWER(name) = LOWER(@roleName) LIMIT 1";

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
            try
            {
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("roleName", roleName);
                    var result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : null;
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            return null;
        }
    }
}
