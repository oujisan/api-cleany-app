using api_cleany_app.src.Helpers;
using api_cleany_app.src.Models;
using Npgsql;
using NpgsqlTypes;
using System.Globalization;
using System.Threading.Tasks;

namespace api_cleany_app.src.Services
{
    public class TaskService
    {
        private string _connectionString;
        private string _errorMessage = string.Empty;
        private UserService _userService;

        public TaskService(UserService userService)
        {
            _connectionString = DbConfig.ConnectionString;
            _userService = userService;
        }

        public List<Tasks> getTasks()
        {
            List<Tasks> tasks = new List<Tasks>();

            string query = @"SELECT 
	            t.task_id, 
	            t.title, 
	            t.description, 
	            t.image_url, 
	            u.username AS created_by, 
	            tt.name AS task_type,
	            t.time ,
	            t.start_date, 
	            t.end_date,
	            a.area_id, a.name AS area_name,
	            a.floor,
	            a.building,
	            STRING_AGG(d.name, ',' ORDER BY d.day_id) AS days,
                t.created_at,
                t.updated_at
            FROM tasks t
	            LEFT JOIN task_days td ON t.task_id = td.task_id
	            LEFT JOIN days d ON td.day_id = d.day_id
	            LEFT JOIN areas a ON a.area_id = t.area_id
	            LEFT JOIN users u ON t.created_by = u.user_id
	            LEFT JOIN task_types tt ON t.task_type_id = tt.task_type_id
            GROUP BY 
	            t.task_id, t.title, t.description, t.image_url, u.username, tt.name, 
	            t.start_date, t.end_date, a.area_id, a.name, a.floor, a.building
            ORDER BY 
	            t.task_id ASC";

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new Tasks()
                        {
                            TaskId = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            ImageUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
                            CreatedBy = reader.GetString(4),
                            TaskType = reader.GetString(5),
                            Area = new AreaDto()
                            {
                                AreaId = reader.GetInt32(9),
                                Name = reader.GetString(10),
                                Floor = reader.GetInt32(11),
                                Building = reader.GetString(12),
                            },
                            Routine = reader.GetString(5).ToLower() != "routine" ? null : new RoutineJson
                            {
                                Time = reader.IsDBNull(6) ? null : reader.GetTimeSpan(6).ToString(@"hh\:mm"),
                                StartDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7).ToString("dd-MM-yyyy"),
                                EndDate = reader.IsDBNull(8) ? null : reader.GetDateTime(8).ToString("dd-MM-yyyy"),
                                DaysOfWeek = reader.IsDBNull(13) ? null : reader.GetString(13).Split(',').ToList(),
                            },
                            CreatedAt = reader.GetDateTime(14).ToString(),
                            UpdatdAt = reader.GetDateTime(15).ToString()
                        });
                    }
                }
                return tasks;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }

            return tasks;
        }

        public Tasks getTaskById(int taskId)
        {
            Tasks task = null;

            string query = @"SELECT 
	            t.task_id, 
	            t.title, 
	            t.description, 
	            t.image_url, 
	            u.username AS created_by, 
	            tt.name AS task_type,
	            t.time ,
	            t.start_date, 
	            t.end_date,
	            a.area_id, a.name AS area_name,
	            a.floor,
	            a.building,
	            STRING_AGG(d.name, ',' ORDER BY d.day_id) AS days,
                t.created_at,
                t.updated_at
            FROM tasks t
	            LEFT JOIN task_days td ON t.task_id = td.task_id
	            LEFT JOIN days d ON td.day_id = d.day_id
	            LEFT JOIN areas a ON a.area_id = t.area_id
	            LEFT JOIN users u ON t.created_by = u.user_id
	            LEFT JOIN task_types tt ON t.task_type_id = tt.task_type_id
            WHERE
                t.task_id = @TaskId
            GROUP BY 
	            t.task_id, t.title, t.description, t.image_url, u.username, tt.name, 
	            t.start_date, t.end_date, a.area_id, a.name, a.floor, a.building
            ORDER BY 
	            t.task_id ASC";

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("@TaskId", taskId);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            task = new Tasks()
                            {
                                TaskId = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                ImageUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CreatedBy = reader.GetString(4),
                                TaskType = reader.GetString(5),
                                Area = new AreaDto()
                                {
                                    AreaId = reader.GetInt32(9),
                                    Name = reader.GetString(10),
                                    Floor = reader.GetInt32(11),
                                    Building = reader.GetString(12),
                                },
                                Routine = reader.GetString(5).ToLower() != "routine" ? null : new RoutineJson
                                {
                                    Time = reader.IsDBNull(6) ? null : reader.GetTimeSpan(6).ToString(@"hh\:mm"),
                                    StartDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7).ToString("dd-MM-yyyy"),
                                    EndDate = reader.IsDBNull(8) ? null : reader.GetDateTime(8).ToString("dd-MM-yyyy"),
                                    DaysOfWeek = reader.IsDBNull(13) ? null : reader.GetString(13).Split(',').ToList(),
                                },
                                CreatedAt = reader.GetDateTime(14).ToString(),
                                UpdatdAt = reader.GetDateTime(15).ToString()
                            };
                        }
                    }
                }
                return task;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }

            return task;
        }

        public List<Tasks> getTasksByType(string taskType)
        {
            List<Tasks> tasks = new List<Tasks>();

            string query = @"SELECT 
	            t.task_id, 
	            t.title, 
	            t.description, 
	            t.image_url, 
	            u.username AS created_by, 
	            tt.name AS task_type,
	            t.time ,
	            t.start_date, 
	            t.end_date,
	            a.area_id, a.name AS area_name,
	            a.floor,
	            a.building,
	            STRING_AGG(d.name, ',' ORDER BY d.day_id) AS days,
                t.created_at,
                t.updated_at
            FROM tasks t
	            LEFT JOIN task_days td ON t.task_id = td.task_id
	            LEFT JOIN days d ON td.day_id = d.day_id
	            LEFT JOIN areas a ON a.area_id = t.area_id
	            LEFT JOIN users u ON t.created_by = u.user_id
	            LEFT JOIN task_types tt ON t.task_type_id = tt.task_type_id
            WHERE
                LOWER(tt.name) = LOWER(@TaskType)
            GROUP BY 
	            t.task_id, t.title, t.description, t.image_url, u.username, tt.name, 
	            t.start_date, t.end_date, a.area_id, a.name, a.floor, a.building
            ORDER BY 
	            t.task_id ASC";

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("@TaskType", taskType);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new Tasks()
                            {
                                TaskId = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                ImageUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CreatedBy = reader.GetString(4),
                                TaskType = reader.GetString(5),
                                Area = new AreaDto()
                                {
                                    AreaId = reader.GetInt32(9),
                                    Name = reader.GetString(10),
                                    Floor = reader.GetInt32(11),
                                    Building = reader.GetString(12),
                                },
                                Routine = reader.GetString(5).ToLower() != "routine" ? null : new RoutineJson
                                {
                                    Time = reader.IsDBNull(6) ? null : reader.GetTimeSpan(6).ToString(@"hh\:mm"),
                                    StartDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7).ToString("dd-MM-yyyy"),
                                    EndDate = reader.IsDBNull(8) ? null : reader.GetDateTime(8).ToString("dd-MM-yyyy"),
                                    DaysOfWeek = reader.IsDBNull(13) ? null : reader.GetString(13).Split(',').ToList(),
                                },
                                CreatedAt = reader.GetDateTime(14).ToString(),
                                UpdatdAt = reader.GetDateTime(15).ToString()
                            });
                        }
                    }
                }
                return tasks;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }

            return tasks;
        }

        public int addReportTask(ReportTaskDto task, int userId)
        {
            string query = @"INSERT INTO 
                tasks (title, created_by, area_id, task_type_id, description, image_url)
                VALUES 
                    (@Title, @CreatedBy, @AreaId, @TaskTypeId, @Description, @ImageUrl)
                RETURNING task_id;";
            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("@Title", task.Title);
                    command.Parameters.AddWithValue("@CreatedBy", userId);
                    command.Parameters.AddWithValue("@AreaId", task.AreaId);
                    command.Parameters.AddWithValue("@TaskTypeId", 2);
                    command.Parameters.AddWithValue("@Description", task.Description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ImageUrl", task.ImageUrl ?? (object)DBNull.Value);

                    var result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            return 0;
        }

        public int addRoutineTask(RoutineTaskDto task, int userId, out DateOnly startDate)
        {
            string queryTask = @"INSERT INTO 
                tasks (title, created_by, area_id, task_type_id, description, image_url, time, start_date, end_date)
                VALUES 
                    (@Title, @CreatedBy, @AreaId, @TaskTypeId, @Description, @ImageUrl, @time, @start_date, @end_date)
                RETURNING task_id;";
            int taskId = 0;
            var time = task.Time != null ? TimeSpan.ParseExact(task.Time, "hh\\:mm", CultureInfo.InvariantCulture) : (TimeSpan?)null;
            startDate = DateOnly.ParseExact(task.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var endDate = DateOnly.ParseExact(task.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(queryTask))
                    {
                        command.Parameters.AddWithValue("@Title", task.Title);
                        command.Parameters.AddWithValue("@CreatedBy", userId);
                        command.Parameters.AddWithValue("@AreaId", task.AreaId);
                        command.Parameters.AddWithValue("@TaskTypeId", 1);
                        command.Parameters.AddWithValue("@Description", task.Description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ImageUrl", task.ImageUrl ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@time", NpgsqlDbType.Time, time);
                        command.Parameters.AddWithValue("@start_date", NpgsqlDbType.Date, startDate);
                        command.Parameters.AddWithValue("@end_date", NpgsqlDbType.Date, endDate);

                        var result = command.ExecuteScalar();
                        taskId = Convert.ToInt32(result);
                    }
                    var data = new List<string>();
                    var parameters = new List<NpgsqlParameter>();

                    int index = 0;
                    foreach (int dayId in task.DaysOfWeek)
                    {
                        string taskParam = $"@taskId{index}";
                        string dayParam = $"@dayId{index}";
                        data.Add($"({taskParam}, {dayParam})");

                        parameters.Add(new NpgsqlParameter(taskParam, taskId));
                        parameters.Add(new NpgsqlParameter(dayParam, dayId));
                        index++;
                    }

                    string queryTaskDays = "INSERT INTO task_days (task_id, day_id) VALUES " + string.Join(", ", data) + ";";

                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(queryTaskDays))
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            return 0;
        }

        public bool updateReportTask(ReportTaskDto task, int taskId)
        {
            string query = @"UPDATE tasks
            SET
                title = @Title,
                area_id = @Area,
                description = @Description,
                image_url = @ImageUrl
            WHERE
                task_id = @TaskId";
            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("@Title", task.Title);
                    command.Parameters.AddWithValue("@AreaId", task.AreaId);
                    command.Parameters.AddWithValue("@Description", task.Description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ImageUrl", task.ImageUrl ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TaskId", taskId);

                    var result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            return false;
        }

        public bool updateRoutineTask(RoutineTaskDto task, int taskId)
        {
            string queryTask = @"UPDATE tasks
            SET
                title = @Title,
                area_id = @AreaId,
                description = @Description,
                image_url = @ImageUrl,
                time = @Time,
                start_date = @StartDate,
                end_date = @EndDate
            WHERE
                task_id = @TaskId
            RETURNING true";
            string queryDelDays = @"DELETE FROM task_days WHERE task_id = @TaskId";
            try
            {
                var time = task.Time != null ? TimeSpan.ParseExact(task.Time, "hh\\:mm", CultureInfo.InvariantCulture) : (TimeSpan?)null;
                var startDate = DateTime.ParseExact(task.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var endDate = DateTime.ParseExact(task.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(queryTask))
                    {
                        command.Parameters.AddWithValue("@TaskId", taskId);
                        command.Parameters.AddWithValue("@Title", task.Title);
                        command.Parameters.AddWithValue("@AreaId", task.AreaId);
                        command.Parameters.AddWithValue("@TaskTypeId", 1);
                        command.Parameters.AddWithValue("@Description", task.Description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ImageUrl", task.ImageUrl ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Time", NpgsqlDbType.Time, time);
                        command.Parameters.AddWithValue("@StartDate", NpgsqlDbType.Date, startDate);
                        command.Parameters.AddWithValue("@EndDate", NpgsqlDbType.Date, endDate);
                        command.ExecuteNonQuery();
                    }

                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(queryDelDays))
                    {
                        command.Parameters.AddWithValue("@TaskId", taskId);
                        command.ExecuteNonQuery();
                    }
                    var data = new List<string>();
                    var parameters = new List<NpgsqlParameter>();

                    int index = 0;
                    foreach (int dayId in task.DaysOfWeek)
                    {
                        string taskParam = $"@taskId{index}";
                        string dayParam = $"@dayId{index}";
                        data.Add($"({taskParam}, {dayParam})");

                        parameters.Add(new NpgsqlParameter(taskParam, taskId));
                        parameters.Add(new NpgsqlParameter(dayParam, dayId));
                        index++;
                    }

                    string queryTaskDays = $"INSERT INTO task_days (task_id, day_id) VALUES {string.Join(", ", data)};";

                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(queryTaskDays))
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                        var result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            return false;
        }

        public bool deleteTask(int taskId) 
        {
            string query = "DELETE FROM tasks WHERE task_id = @TaskId";

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("TaskId", taskId);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
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