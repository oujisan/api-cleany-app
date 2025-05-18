using api_cleany_app.src.Helpers;
using api_cleany_app.src.Models;
using Npgsql;

namespace api_cleany_app.src.Services
{
    public class AreaService
    {
        private string _connectionString;
        private string _errorMessage = string.Empty;

        public AreaService()
        {
            _connectionString = DbConfig.ConnectionString;
        }

        public List<Area> getAllArea()
        {
            List<Area> areas = new List<Area>();
            string query = @"SELECT * FROM areas ORDER BY area_id ASC";

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            areas.Add(new Area()
                            {
                                AreaId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Floor = reader.GetInt32(2),
                                Building = reader.GetString(3),
                                CreateAt = reader.GetDateTime(4).ToString(),
                                UpdateAt = reader.GetDateTime(5).ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }
            return areas;
        }

        public string getError() => _errorMessage;
    }
}
