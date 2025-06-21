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

        public Shift getShiftById(int shiftId)
        {
            string query = @"SELECT shift_id, name, start_time, end_time, created_at, updated_at FROM shifts WHERE shift_id = @shiftId";
            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
            {
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("shiftId", shiftId);
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Shift
                                {
                                    ShiftId = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    StartTime = ((TimeSpan)reader.GetValue(2)).ToString(@"hh\:mm"),
                                    EndTime = ((TimeSpan)reader.GetValue(3)).ToString(@"hh\:mm"),
                                    CreatedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4).ToString("dd-MM-yyyy HH:mm"),
                                    UpdatedAt = reader.GetDateTime(5).ToString("dd-MM-yyyy HH:mm")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }
            }
            return null;
        }
        public List<Shift> getAllShift()
        {
            List<Shift> shifts = new List<Shift>();
            string query = @"SELECT shift_id, name, start_time, end_time, created_at, updated_at FROM shifts ORDER BY shift_id";

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
                                StartTime = ((TimeSpan)reader.GetValue(2)).ToString(@"hh\:mm"),
                                EndTime = ((TimeSpan)reader.GetValue(3)).ToString(@"hh\:mm"),
                                CreatedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4).ToString("dd-MM-yyyy HH:mm"),
                                UpdatedAt = reader.GetDateTime(5).ToString("dd-MM-yyyy HH:mm")
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

        public bool addShift(ShiftDto shift)
        {
            string query = @"INSERT INTO shifts (name, start_time, end_time, created_at, updated_at) 
                     VALUES (@name, @startTime, @endTime, @createAt, @updateAt)";

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
            {
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("name", shift.Name);
                        command.Parameters.AddWithValue("startTime", TimeSpan.Parse(shift.StartTime));
                        command.Parameters.AddWithValue("endTime", TimeSpan.Parse(shift.EndTime));
                        command.Parameters.AddWithValue("createAt", DateTime.Now);
                        command.Parameters.AddWithValue("updateAt", DateTime.Now);

                        return command.ExecuteNonQuery() > 0; 
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                    Console.WriteLine("AddShift Error: " + ex.Message);
                }
            }
            return false;
        }


        public bool updateShift(ShiftDto shift)
        {
            string selectQuery = "SELECT start_time, end_time FROM shifts WHERE shift_id = @shiftId";
            string updateQuery = @"UPDATE shifts SET name = @name, start_time = @startTime, end_time = @endTime, updated_at = @updatedAt 
                           WHERE shift_id = @shiftId";

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
            {
                try
                {
                    TimeSpan existingStartTime = TimeSpan.Zero;
                    TimeSpan existingEndTime = TimeSpan.Zero;

                    using (NpgsqlCommand selectCmd = sqlDbHelper.NpgsqlCommand(selectQuery))
                    {
                        selectCmd.Parameters.AddWithValue("shiftId", shift.ShiftId);
                        using (var reader = selectCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                existingStartTime = reader.GetTimeSpan(0);
                                existingEndTime = reader.GetTimeSpan(1);
                            }
                            else
                            {
                                _errorMessage = "Shift not found.";
                                return false;
                            }
                        }
                    }
                    TimeSpan startTime;
                    TimeSpan endTime;

                    if (!TimeSpan.TryParse(shift.StartTime, out startTime))
                    {
                        startTime = existingStartTime;
                    }

                    if (!TimeSpan.TryParse(shift.EndTime, out endTime))
                    {
                        endTime = existingEndTime;
                    }

                    using (NpgsqlCommand updateCmd = sqlDbHelper.NpgsqlCommand(updateQuery))
                    {
                        updateCmd.Parameters.AddWithValue("name", shift.Name);
                        updateCmd.Parameters.AddWithValue("startTime", startTime);
                        updateCmd.Parameters.AddWithValue("endTime", endTime);
                        updateCmd.Parameters.AddWithValue("updatedAt", DateTime.Now);
                        updateCmd.Parameters.AddWithValue("shiftId", shift.ShiftId);

                        return updateCmd.ExecuteNonQuery() > 0;
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

        public string GetError()
        {
            return _errorMessage;
        }

    }
}