using api_cleany_app.src.Helpers;
using api_cleany_app.src.Models;
using Npgsql;
using System.Data;

namespace api_cleany_app.src.Services
{
    public class TaskService
    {
        private string _connectionString;
        private string _errorMessage = string.Empty;

        public TaskService()
        {
            _connectionString = DbConfig.ConnectionString;
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

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
            try
            {
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
                            Routine = reader.GetString(5).ToLower() != "routine" ? null : new RoutineTask
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
                                Routine = new RoutineTask
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
                            CreatedAt = reader.GetDateTime(14).ToString(),
                            AssigmentAt = reader.IsDBNull(15) ? null : reader.GetDateTime(15).ToString(),
                            CompleteAt = reader.IsDBNull(16) ? null : reader.GetDateTime(16).ToString()
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

        public string getError() => _errorMessage;
    }
}