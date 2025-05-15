using api_cleany_app.src.Helpers;
using Npgsql;

namespace api_cleany_app.src.Services
{
    public class ShiftService
    {
        private string _connectionString;
        private string _errorMessage = string.Empty;

        public ShiftService()
        {
            _connectionString = DbConfig.ConnectionString;
        }

        public int? getShiftIdByName(string shiftName)
        {
            string query = @"SELECT shift_id FROM shifts WHERE LOWER(name) = LOWER(@shiftName) LIMIT 1";

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("shiftName", shiftName);
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
