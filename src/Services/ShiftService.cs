using api_cleany_app.src.Helpers;
using api_cleany_app.src.Models;
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

        public List<Shift> getAllShift()
        {
            List<Shift> shifts = new List<Shift>();
            string query = @"SELECT shift_id, name, start_date, end_date, create_at, update_at FROM shifts ORDER BY shift_id";
            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
            {
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Shift shift = new Shift
                            {
                                ShiftId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                StartDate = reader.GetDateTime(2).ToString("dd-MM-yyyy"),
                                EndDate = reader.GetDateTime(3).ToString("dd-MM-yyyy"),
                                CreateAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4).ToString("dd-MM-yyy"),
                                UpdateAt = reader.GetDateTime(5).ToString("dd-MM-yyyy")
                            };
                            shifts.Add(shift);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }
                return shifts;
            }
        }

        public bool addShift(Shift shift)
        {
            string query = @"INSERT INTO shifts (name, start_date, end_date, create_at, update_at) 
                             VALUES (@name, @startDate, @endDate, @createAt, @updateAt)";
            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
            {
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("name", shift.Name);
                        command.Parameters.AddWithValue("startDate", DateTime.Parse(shift.StartDate));
                        command.Parameters.AddWithValue("endDate", DateTime.Parse(shift.EndDate));
                        command.Parameters.AddWithValue("createAt", DateTime.Now);
                        command.Parameters.AddWithValue("updateAt", DateTime.Now);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }
            }
            return false;
        }

        public bool updateShift(Shift shift)
        {
            string query = @"UPDATE shifts SET name = @name, start_date = @startDate, end_date = @endDate, 
                             update_at = @updateAt WHERE shift_id = @shiftId";
            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
            {
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("name", shift.Name);
                        command.Parameters.AddWithValue("startDate", DateTime.Parse(shift.StartDate));
                        command.Parameters.AddWithValue("endDate", DateTime.Parse(shift.EndDate));
                        command.Parameters.AddWithValue("updateAt", DateTime.Now);
                        command.Parameters.AddWithValue("shiftId", shift.ShiftId);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }
            }
            return false;
        }
        public bool deleteShift(int shiftId)
        {
            string query = @"DELETE FROM shifts WHERE shift_id = @shiftId";
            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
            {
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("shiftId", shiftId);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }
            }
            return false;
        }
    }
}
