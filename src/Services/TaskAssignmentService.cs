using api_cleany_app.src.Helpers;
using api_cleany_app.src.Models;
using Npgsql;

namespace api_cleany_app.src.Services
{
    public class TaskAssignmentService
    {
        private string _connectionString;
        private string _errorMessage = string.Empty;
        private UserService _userService;

        public TaskAssignmentService(UserService userService)
        {
            _connectionString = DbConfig.ConnectionString;
            _userService = userService;
        }

        public List<TaskAssignment> getTaskAssignmnetRoutine()
        {
            List<TaskAssignment> routineTasks = new List<TaskAssignment>();
            string query = @"SELECT 
                asn.assignment_id,
                t.task_id, 
                t.title, 
                t.description, 
                t.image_url as task_image_url, 
                u2.username as created_by, 
                tt.name as task_type,
                a.area_id,
                a.name as area, 
                a.floor, 
                a.building,
                t.time,
                t.start_date, 
                t.end_date,
                STRING_AGG(d.name, ',' ORDER BY d.day_id) AS days,
                asn.date, 
                asn.image_url as proof_image_url, 
                u1.username as worked_by, 
                asn.status,
                t.created_at,
                asn.assignment_at, 
                asn.complete_at
            FROM assignments asn
                LEFT JOIN tasks t ON t.task_id = asn.task_id
                LEFT JOIN users u1 ON u1.user_id = asn.worked_by
                LEFT JOIN users u2 ON u2.user_id = t.created_by
                LEFT JOIN areas a ON a.area_id = t.area_id
                LEFT JOIN task_days td ON t.task_id = td.task_id
                LEFT JOIN days d ON td.day_id = d.day_id
                LEFT JOIN task_types tt ON tt.task_type_id = t.task_type_id
            WHERE 
                LOWER(tt.name) = 'routine'
            GROUP BY 
                asn.assignment_id, t.task_id, t.title, t.description, t.image_url, tt.name, u2.username,a.area_id, 
                a.name, a.floor, a.building, asn.image_url, u1.username, asn.status, asn.assignment_at, asn.complete_at";

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            routineTasks.Add(new TaskAssignment
                            {
                                AssigmentId = reader.GetInt32(0),
                                Task = new TaskDto
                                {
                                    TaskId = reader.GetInt32(1),
                                    Title = reader.GetString(2),
                                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    ImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    CreatedBy = reader.GetString(5),
                                    TaskType = reader.GetString(6),
                                    Area = new AreaDto
                                    {
                                        AreaId = reader.GetInt32(7),
                                        Name = reader.GetString(8),
                                        Floor = reader.GetInt32(9),
                                        Building = reader.GetString(10),
                                    },
                                    Routine = new RoutineJson
                                    {
                                        Time = reader.IsDBNull(11) ? null : reader.GetTimeSpan(11).ToString(@"hh\:mm"),
                                        StartDate = reader.IsDBNull(12) ? null : reader.GetDateTime(12).ToString("dd-MM-yyyy"),
                                        EndDate = reader.IsDBNull(13) ? null : reader.GetDateTime(13).ToString("dd-MM-yyyy"),
                                        DaysOfWeek = reader.IsDBNull(14) ? null : reader.GetString(14).Split(',').ToList()
                                    },
                                },
                                Date = reader.GetDateTime(15).ToString("dd-MM-yyyy"),
                                WorkedBy = reader.IsDBNull(17) ? null : reader.GetString(17),
                                Status = reader.GetString(18),
                                ProofImageUrl = reader.IsDBNull(16) ? null : reader.GetString(16),
                                CreatedAt = reader.GetDateTime(19).ToString("dd-MM-yyyy HH:mm:ss"),
                                AssigmentAt = reader.IsDBNull(20) ? null : reader.GetDateTime(20).ToString("dd-MM-yyyy HH:mm:ss"),
                                CompleteAt = reader.IsDBNull(21) ? null : reader.GetDateTime(21).ToString("dd-MM-yyyy HH:mm:ss"),
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }
            return routineTasks;
        }

        public List<TaskAssignment> getTaskAssignmentReport()
        {
            List<TaskAssignment> reportTasks = new List<TaskAssignment>();
            string query = @"SELECT 
	            asn.assignment_id,
	            t.task_id, 
	            t.title, 
	            t.description, 
	            t.image_url as task_image_url, 
	            u2.username as created_by,
	            tt.name as task_type,
	            a.area_id,
	            a.name as area, 
	            a.floor, 
	            a.building,
	            u1.username as worked_by, 
	            asn.status,
	            asn.image_url as proof_image_url, 
	            t.created_at,
	            asn.assignment_at, 
	            asn.complete_at
            FROM assignments asn
                LEFT JOIN tasks t ON t.task_id = asn.task_id
                LEFT JOIN users u1 ON u1.user_id = asn.worked_by
                LEFT JOIN users u2 ON u2.user_id = t.created_by
                LEFT JOIN areas a ON a.area_id = t.area_id
                LEFT JOIN task_types tt ON tt.task_type_id = t.task_type_id
            WHERE 
                LOWER(tt.name) = 'report'
            GROUP BY 
                asn.assignment_id, t.task_id, t.title, t.description, t.image_url, tt.name, u2.username,a.area_id, a.name, a.floor,
                a.building, asn.image_url, u1.username, asn.status, t.created_at,asn.assignment_at, asn.complete_at";

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reportTasks.Add(new TaskAssignment
                            {
                                AssigmentId = reader.GetInt32(0),
                                Task = new TaskDto
                                {
                                    TaskId = reader.GetInt32(1),
                                    Title = reader.GetString(2),
                                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    ImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    CreatedBy = reader.GetString(5),
                                    TaskType = reader.GetString(6),
                                    Area = new AreaDto
                                    {
                                        AreaId = reader.GetInt32(7),
                                        Name = reader.GetString(8),
                                        Floor = reader.GetInt32(9),
                                        Building = reader.GetString(10),
                                    },
                                    Routine = null
                                },
                                Date = null,
                                WorkedBy = reader.IsDBNull(11) ? null : reader.GetString(11),
                                Status = reader.GetString(12),
                                ProofImageUrl = reader.IsDBNull(13) ? null : reader.GetString(13),
                                CreatedAt = reader.GetDateTime(14).ToString("dd-MM-yyyy HH:mm:ss"),
                                AssigmentAt = reader.IsDBNull(15) ? null : reader.GetDateTime(15).ToString("dd-MM-yyyy HH:mm:ss"),
                                CompleteAt = reader.IsDBNull(16) ? null : reader.GetDateTime(16).ToString("dd-MM-yyyy HH:mm:ss")
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }
            return reportTasks;
        }

        public List<TaskAssignment> getTaskAssignmentReportByUserId(int userId)
        {
            List<TaskAssignment> tasks = new List<TaskAssignment>();

            string query = @"SELECT 
	            asn.assignment_id,
	            t.task_id, 
	            t.title, 
	            t.description, 
	            t.image_url as task_image_url, 
	            u2.username as created_by,
	            tt.name as task_type,
	            a.area_id,
	            a.name as area, 
	            a.floor, 
	            a.building,
	            u1.username as worked_by, 
	            asn.status,
	            asn.image_url as proof_image_url, 
	            t.created_at,
	            asn.assignment_at, 
	            asn.complete_at
            FROM assignments asn
                LEFT JOIN tasks t ON t.task_id = asn.task_id
                LEFT JOIN users u1 ON u1.user_id = asn.worked_by
                LEFT JOIN users u2 ON u2.user_id = t.created_by
                LEFT JOIN areas a ON a.area_id = t.area_id
                LEFT JOIN task_types tt ON tt.task_type_id = t.task_type_id
            WHERE 
                LOWER(tt.name) = 'report' AND u2.user_id = @UserId
            GROUP BY 
                asn.assignment_id, t.task_id, t.title, t.description, t.image_url, tt.name, u2.username,a.area_id, a.name, a.floor,
                a.building, asn.image_url, u1.username, asn.status, t.created_at,asn.assignment_at, asn.complete_at";

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("UserId", userId);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new TaskAssignment
                            {
                                AssigmentId = reader.GetInt32(0),
                                Task = new TaskDto
                                {
                                    TaskId = reader.GetInt32(1),
                                    Title = reader.GetString(2),
                                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    ImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    CreatedBy = reader.GetString(5),
                                    TaskType = reader.GetString(6),
                                    Area = new AreaDto
                                    {
                                        AreaId = reader.GetInt32(7),
                                        Name = reader.GetString(8),
                                        Floor = reader.GetInt32(9),
                                        Building = reader.GetString(10),
                                    },
                                    Routine = null
                                },
                                Date = null,
                                WorkedBy = reader.IsDBNull(11) ? null : reader.GetString(11),
                                Status = reader.GetString(12),
                                ProofImageUrl = reader.IsDBNull(13) ? null : reader.GetString(13),
                                CreatedAt = reader.GetDateTime(14).ToString("dd-MM-yyyy HH:mm:ss"),
                                AssigmentAt = reader.IsDBNull(15) ? null : reader.GetDateTime(15).ToString("dd-MM-yyyy HH:mm:ss"),
                                CompleteAt = reader.IsDBNull(16) ? null : reader.GetDateTime(16).ToString("dd-MM-yyyy HH:mm:ss")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            return tasks;
        }

        public bool AddAssignmentTask(int taskId, int taskTypeId)
        {
            string query = @"INSERT INTO 
            assignments (task_id, image_url, date, worked_by, status)
            VALUES 
                (@TaskId, NULL, @Date, NULL, 'pending');";

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("@TaskId", taskId);
                    if (taskTypeId == 1)
                    {
                        command.Parameters.AddWithValue("@Date", DateTime.Today);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@Date", DBNull.Value);
                    }

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


        public bool updateStatusAssignmentTask(string status, int assignmentId, int workedBy = 0)
        {
            string query = string.Empty;
            if (status == "in_progress")
            {
                query = @"UPDATE assignments
                SET
                    status = CAST(@status AS task_status),
                    worked_by = @WorkedBy
                    assignment_at = NOW()
                WHERE
                    assignment_id = @AssignmentId";
            }
            else if (status == "completed")
            {
                query = @"UPDATE assignments
                SET
                    status = CAST(@status AS task_status),
                    complete_at = NOW()
                WHERE
                    assignment_id = @AssignmentId";
            }
            else
            {
                query = @"UPDATE assignments
                SET
                    status = CAST(@status AS task_status),
                WHERE
                    assignment_id = @AssignmentId";
            }

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("Status", status);
                        command.Parameters.AddWithValue("AssignmentId", assignmentId);

                        if (status == "in_progress")
                        {
                            command.Parameters.AddWithValue("WorkedBy", workedBy);
                        }

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

        public string getError() => _errorMessage;
    }
}
