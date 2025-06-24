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

        public Area getAreaById(int areaId)
        {
            Area area = null;
            string query = @"SELECT * FROM areas WHERE area_id = @areaId";
            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("@areaId", areaId);
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                area = new Area()
                                {
                                    AreaId = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Floor = reader.GetInt32(2),
                                    Building = reader.GetString(3),
                                    CreateAt = reader.GetDateTime(4).ToString(),
                                    UpdateAt = reader.GetDateTime(5).ToString()
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }
            return area;
        }

        public bool addArea(AreaDto area)
        {
            string query = @"INSERT INTO areas (name, floor, building) VALUES (@name, @floor, @building)";
            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("@name", area.Name);
                        command.Parameters.AddWithValue("@floor", area.Floor);
                        command.Parameters.AddWithValue("@building", area.Building);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }
            return false;
        }

        public bool updateArea(AreaDto area)
        {
            string query = @"UPDATE areas SET name = @name, floor = @floor, building = @building, updated_at = NOW() WHERE area_id = @areaId";
            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("@name", area.Name);
                        command.Parameters.AddWithValue("@floor", area.Floor);
                        command.Parameters.AddWithValue("@building", area.Building);
                        command.Parameters.AddWithValue("@areaId", area.AreaId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }
            return false;
        }

        public bool deleteArea(int areaId)
        {
            string query = @"DELETE FROM areas WHERE area_id = @areaId";
            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("@areaId", areaId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }
            return false;
        }

        public string getError() => _errorMessage;
    }
}
