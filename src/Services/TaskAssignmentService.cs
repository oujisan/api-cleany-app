using api_cleany_app.src.Helpers;
using api_cleany_app.src.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace api_cleany_app.src.Services
{
    public class TaskAssignmentService
    {
        private string _connectionString;
        private string _errorMessage = string.Empty;
        private UserService _userService;

        public TaskAssignmentService(UserService userService, VerificationService verificationService)
        {
            _connectionString = DbConfig.ConnectionString;
            _userService = userService;
        }

        public List<TaskAssignment> getTaskAssignmentRoutine()
        {
            List<TaskAssignment> routineTasks = new List<TaskAssignment>();

            string query = @"
            WITH image_agg AS (
                SELECT 
                    task_id,
                    STRING_AGG(i.image_url, ',' ORDER BY i.image_id) AS images_url
                FROM task_images ti
                JOIN images i ON i.image_id = ti.image_id
                GROUP BY task_id
            ),
            imageAsn_agg AS (
                SELECT 
                    assignment_id,
                    STRING_AGG(i.image_url, ',' ORDER BY i.image_id) AS images_url
                FROM assignment_images ai
                JOIN images i ON i.image_id = ai.image_id
                GROUP BY assignment_id
            ),
            day_agg AS (
                SELECT 
                    task_id,
                    STRING_AGG(d.name, ',' ORDER BY d.day_id) AS days
                FROM task_days td
                JOIN days d ON d.day_id = td.day_id
                GROUP BY task_id
            )
            SELECT 
                asn.assignment_id,
                t.task_id, 
                t.title, 
                t.description, 
                COALESCE(ia.images_url, '') AS task_images_url, 
                u2.username as created_by, 
                tt.name as task_type,
                a.area_id,
                a.name as area, 
                a.floor, 
                a.building,
                t.time,
                t.start_date, 
                t.end_date,
                COALESCE(da.days, '') AS days,
                asn.date, 
                COALESCE(iaa.images_url, '') AS proof_images_url,
                u1.username as worked_by, 
                asn.status,
                t.created_at,
                asn.assignment_at, 
                asn.complete_at
            FROM assignments asn
            LEFT JOIN tasks t ON t.task_id = asn.task_id
            LEFT JOIN image_agg ia ON ia.task_id = t.task_id
            LEFT JOIN imageAsn_agg iaa ON iaa.assignment_id = asn.assignment_id
            LEFT JOIN day_agg da ON da.task_id = t.task_id
            LEFT JOIN users u1 ON u1.user_id = asn.worked_by
            LEFT JOIN users u2 ON u2.user_id = t.created_by
            LEFT JOIN areas a ON a.area_id = t.area_id
            LEFT JOIN task_types tt ON tt.task_type_id = t.task_type_id
            WHERE LOWER(tt.name) = 'routine'
            ORDER BY asn.date DESC;";

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
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
                                TaskImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4).Split(',').ToList(),
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
                            Date = reader.IsDBNull(15) ? null : reader.GetDateTime(15).ToString("dd-MM-yyyy"),
                            ProofImageUrl = reader.IsDBNull(16) ? null : reader.GetString(16).Split(',').ToList(),
                            WorkedBy = reader.IsDBNull(17) ? null : reader.GetString(17),
                            Status = reader.GetString(18),
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
            string query = @"
            WITH image_agg AS (
                SELECT 
                    task_id,
                    STRING_AGG(i.image_url, ',' ORDER BY i.image_id) AS images_url
                FROM task_images ti
                JOIN images i ON i.image_id = ti.image_id
                GROUP BY task_id
            ),
            image_asn_agg AS (
                SELECT 
                    assignment_id,
                    STRING_AGG(i.image_url, ',' ORDER BY i.image_id) AS images_url
                FROM assignment_images ai
                JOIN images i ON i.image_id = ai.image_id
                GROUP BY assignment_id
            )
            SELECT 
                asn.assignment_id,
                t.task_id, 
                t.title, 
                t.description, 
                COALESCE(img_task.images_url, '') AS task_images_url,
                u2.username AS created_by,
                tt.name AS task_type,
                a.area_id,
                a.name AS area, 
                a.floor, 
                a.building,
                u1.username AS worked_by, 
                asn.status,
                COALESCE(img_asn.images_url, '') AS proof_images_url, 
                t.created_at,
                asn.assignment_at, 
                asn.complete_at,
                asn.latitude,
                asn.longitude
            FROM assignments asn
            LEFT JOIN tasks t ON t.task_id = asn.task_id
            LEFT JOIN users u1 ON u1.user_id = asn.worked_by
            LEFT JOIN users u2 ON u2.user_id = t.created_by
            LEFT JOIN areas a ON a.area_id = t.area_id
            LEFT JOIN task_types tt ON tt.task_type_id = t.task_type_id
            LEFT JOIN image_agg img_task ON img_task.task_id = t.task_id
            LEFT JOIN image_asn_agg img_asn ON img_asn.assignment_id = asn.assignment_id
            WHERE 
                LOWER(tt.name) = 'report'
            ORDER BY 
                asn.assignment_at DESC;";

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
                                    TaskImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4).Split(',').ToList(),
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
                                ProofImageUrl = reader.IsDBNull(13) ? null : reader.GetString(13).Split(',').ToList(),
                                CreatedAt = reader.GetDateTime(14).ToString("dd-MM-yyyy HH:mm:ss"),
                                AssigmentAt = reader.IsDBNull(15) ? null : reader.GetDateTime(15).ToString("dd-MM-yyyy HH:mm:ss"),
                                CompleteAt = reader.IsDBNull(16) ? null : reader.GetDateTime(16).ToString("dd-MM-yyyy HH:mm:ss"),
                                Latitude = reader.IsDBNull(17) ? null : reader.GetString(17),
                                Longitude = reader.IsDBNull(18) ? null : reader.GetString(18)
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
            List<TaskAssignment> reportTasks = new List<TaskAssignment>();
            string query = @"
            WITH image_agg AS (
                SELECT 
                    task_id,
                    STRING_AGG(i.image_url, ',' ORDER BY i.image_id) AS images_url
                FROM task_images ti
                JOIN images i ON i.image_id = ti.image_id
                GROUP BY task_id
            ),
            image_asn_agg AS (
                SELECT 
                    assignment_id,
                    STRING_AGG(i.image_url, ',' ORDER BY i.image_id) AS images_url
                FROM assignment_images ai
                JOIN images i ON i.image_id = ai.image_id
                GROUP BY assignment_id
            )
            SELECT 
                asn.assignment_id,
                t.task_id, 
                t.title, 
                t.description, 
                COALESCE(img_task.images_url, '') AS task_images_url,
                u2.username AS created_by,
                tt.name AS task_type,
                a.area_id,
                a.name AS area, 
                a.floor, 
                a.building,
                u1.username AS worked_by, 
                asn.status,
                COALESCE(img_asn.images_url, '') AS proof_images_url, 
                t.created_at,
                asn.assignment_at, 
                asn.complete_at,
                asn.latitude,
                asn.longitude
            FROM assignments asn
                LEFT JOIN tasks t ON t.task_id = asn.task_id
                LEFT JOIN users u1 ON u1.user_id = asn.worked_by
                LEFT JOIN users u2 ON u2.user_id = t.created_by
                LEFT JOIN areas a ON a.area_id = t.area_id
                LEFT JOIN task_types tt ON tt.task_type_id = t.task_type_id
                LEFT JOIN image_agg img_task ON img_task.task_id = t.task_id
                LEFT JOIN image_asn_agg img_asn ON img_asn.assignment_id = asn.assignment_id
            WHERE 
                LOWER(tt.name) = 'report' AND u2.user_id = @UserId
            ORDER BY 
                asn.assignment_at DESC;";

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

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
                                        TaskImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4).Split(',').ToList(),
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
                                    ProofImageUrl = reader.IsDBNull(13) ? null : reader.GetString(13).Split(',').ToList(),
                                    CreatedAt = reader.GetDateTime(14).ToString("dd-MM-yyyy HH:mm:ss"),
                                    AssigmentAt = reader.IsDBNull(15) ? null : reader.GetDateTime(15).ToString("dd-MM-yyyy HH:mm:ss"),
                                    CompleteAt = reader.IsDBNull(16) ? null : reader.GetDateTime(16).ToString("dd-MM-yyyy HH:mm:ss"),
                                    Latitude = reader.IsDBNull(17) ? null : reader.GetString(17),
                                    Longitude = reader.IsDBNull(18) ? null : reader.GetString(18)
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }

            return reportTasks;
        }

        public List<TaskAssignment> getTaskAssignmentRoutineByUserId(int userId)
        {
            List<TaskAssignment> routineTasks = new List<TaskAssignment>();

            string query = @"
            WITH image_agg AS (
                SELECT 
                    task_id,
                    STRING_AGG(i.image_url, ',' ORDER BY i.image_id) AS images_url
                FROM task_images ti
                JOIN images i ON i.image_id = ti.image_id
                GROUP BY task_id
            ),
            imageAsn_agg AS (
                SELECT 
                    assignment_id,
                    STRING_AGG(i.image_url, ',' ORDER BY i.image_id) AS images_url
                FROM assignment_images ai
                JOIN images i ON i.image_id = ai.image_id
                GROUP BY assignment_id
            ),
            day_agg AS (
                SELECT 
                    task_id,
                    STRING_AGG(d.name, ',' ORDER BY d.day_id) AS days
                FROM task_days td
                JOIN days d ON d.day_id = td.day_id
                GROUP BY task_id
            )
            SELECT 
                asn.assignment_id,
                t.task_id, 
                t.title, 
                t.description, 
                COALESCE(ia.images_url, '') AS task_images_url, 
                u2.username as created_by, 
                tt.name as task_type,
                a.area_id,
                a.name as area, 
                a.floor, 
                a.building, 
                t.time,
                t.start_date, 
                t.end_date,
                COALESCE(da.days, '') AS days,
                asn.date, 
                COALESCE(iaa.images_url, '') AS proof_images_url,
                u1.username as worked_by, 
                asn.status,
                t.created_at,
                asn.assignment_at, 
                asn.complete_at
            FROM assignments asn
                LEFT JOIN tasks t ON t.task_id = asn.task_id
                LEFT JOIN image_agg ia ON ia.task_id = t.task_id
                LEFT JOIN imageAsn_agg iaa ON iaa.assignment_id = asn.assignment_id
                LEFT JOIN day_agg da ON da.task_id = t.task_id
                LEFT JOIN users u1 ON u1.user_id = asn.worked_by
                LEFT JOIN users u2 ON u2.user_id = t.created_by
                LEFT JOIN areas a ON a.area_id = t.area_id
                LEFT JOIN task_types tt ON tt.task_type_id = t.task_type_id
            WHERE 
                LOWER(tt.name) = 'routine' AND u2.user_id = @UserId
            ORDER BY asn.date DESC;";

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

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
                                    TaskImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4).Split(',').ToList(),
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
                                ProofImageUrl = reader.IsDBNull(16) ? null : reader.GetString(16).Split(',').ToList(),
                                WorkedBy = reader.IsDBNull(17) ? null : reader.GetString(17),
                                Status = reader.GetString(18),
                                CreatedAt = reader.GetDateTime(19).ToString("dd-MM-yyyy HH:mm:ss"),
                                AssigmentAt = reader.IsDBNull(20) ? null : reader.GetDateTime(20).ToString("dd-MM-yyyy HH:mm:ss"),
                                CompleteAt = reader.IsDBNull(21) ? null : reader.GetDateTime(21).ToString("dd-MM-yyyy HH:mm:ss"),
                                Latitude = null,
                                Longitude = null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }

            return routineTasks;
        }

        public TaskAssignment getTaskAssignmentById(int assignmentId)
        {
            TaskAssignment task = null;

            string query = @"
            WITH image_agg AS (
                SELECT 
                    task_id,
                    STRING_AGG(i.image_url, ',' ORDER BY i.image_id) AS images_url
                FROM task_images ti
                JOIN images i ON i.image_id = ti.image_id
                GROUP BY task_id
            ),
            imageAsn_agg AS (
                SELECT 
                    assignment_id,
                    STRING_AGG(i.image_url, ',' ORDER BY i.image_id) AS images_url
                FROM assignment_images ai
                JOIN images i ON i.image_id = ai.image_id
                GROUP BY assignment_id
            ),
            day_agg AS (
                SELECT 
                    task_id,
                    STRING_AGG(d.name, ',' ORDER BY d.day_id) AS days
                FROM task_days td
                JOIN days d ON d.day_id = td.day_id
                GROUP BY task_id
            )
            SELECT 
                asn.assignment_id,
                t.task_id, 
                t.title, 
                t.description, 
                COALESCE(ia.images_url, '') AS task_images_url, 
                u2.username as created_by, 
                tt.name as task_type,
                a.area_id,
                a.name as area, 
                a.floor, 
                a.building, 
                t.time,
                t.start_date, 
                t.end_date,
                COALESCE(da.days, '') AS days,
                asn.date, 
                COALESCE(iaa.images_url, '') AS proof_images_url,
                u1.username as worked_by, 
                asn.status,
                t.created_at,
                asn.assignment_at, 
                asn.complete_at,
                asn.latitude,
                asn.longitude
            FROM assignments asn
                LEFT JOIN tasks t ON t.task_id = asn.task_id
                LEFT JOIN image_agg ia ON ia.task_id = t.task_id
                LEFT JOIN imageAsn_agg iaa ON iaa.assignment_id = asn.assignment_id
                LEFT JOIN day_agg da ON da.task_id = t.task_id
                LEFT JOIN users u1 ON u1.user_id = asn.worked_by
                LEFT JOIN users u2 ON u2.user_id = t.created_by
                LEFT JOIN areas a ON a.area_id = t.area_id
                LEFT JOIN task_types tt ON tt.task_type_id = t.task_type_id
            WHERE 
                asn.assignment_id = @AssignmentId
            ORDER BY asn.date DESC;";

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("@AssignmentId", assignmentId);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            task = new TaskAssignment
                            {
                                AssigmentId = reader.GetInt32(0),
                                Task = new TaskDto
                                {
                                    TaskId = reader.GetInt32(1),
                                    Title = reader.GetString(2),
                                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    TaskImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4).Split(',').ToList(),
                                    CreatedBy = reader.GetString(5),
                                    TaskType = reader.GetString(6),
                                    Area = new AreaDto
                                    {
                                        AreaId = reader.GetInt32(7),
                                        Name = reader.GetString(8),
                                        Floor = reader.GetInt32(9),
                                        Building = reader.GetString(10),
                                    },
                                    Routine = reader.IsDBNull(11) ? null : new RoutineJson
                                    {
                                        Time = reader.IsDBNull(11) ? null : reader.GetTimeSpan(11).ToString(@"hh\:mm"),
                                        StartDate = reader.IsDBNull(12) ? null : reader.GetDateTime(12).ToString("dd-MM-yyyy"),
                                        EndDate = reader.IsDBNull(13) ? null : reader.GetDateTime(13).ToString("dd-MM-yyyy"),
                                        DaysOfWeek = reader.IsDBNull(14) ? null : reader.GetString(14).Split(',').ToList()
                                    },
                                },
                                Date = reader.IsDBNull(15) ? null : reader.GetDateTime(15).ToString("dd-MM-yyyy"),
                                ProofImageUrl = reader.IsDBNull(16) ? null : reader.GetString(16).Split(',').ToList(),
                                WorkedBy = reader.IsDBNull(17) ? null : reader.GetString(17),
                                Status = reader.GetString(18),
                                CreatedAt = reader.GetDateTime(19).ToString("dd-MM-yyyy HH:mm:ss"),
                                AssigmentAt = reader.IsDBNull(20) ? null : reader.GetDateTime(20).ToString("dd-MM-yyyy HH:mm:ss"),
                                CompleteAt = reader.IsDBNull(21) ? null : reader.GetDateTime(21).ToString("dd-MM-yyyy HH:mm:ss"),
                                Latitude = reader.IsDBNull(22) ? null : reader.GetString(22),
                                Longitude = reader.IsDBNull(23) ? null : reader.GetString(23)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }

            return task;
        }

        public int getTaskIdByAssignmentId(int assignmentId)
        {
            string query = "SELECT task_id FROM assignments WHERE assignment_id = @AssignmentId";
            int taskId = 0;

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("@AssignmentId", assignmentId);
                    var result = command.ExecuteScalar();
                    taskId = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            return taskId;
        }

        public bool AddAssignmentTask(int taskId, int taskTypeId)
        {
            string query = @"INSERT INTO 
            assignments (task_id, date, worked_by, status)
            VALUES 
                (@TaskId,@Date, NULL, 'pending');";

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
                    worked_by = @WorkedBy,
                    assignment_at = NOW()
                WHERE assignment_id = (
                    SELECT assignment_id
                    FROM assignments
                    WHERE task_id = @TaskId
                    ORDER BY assignment_at DESC
                    LIMIT 1
                );";
            }
            else if (status == "completed")
            {
                query = @"UPDATE assignments
                SET
                    status = CAST(@status AS task_status),
                    complete_at = NOW()
                WHERE assignment_id = (
                    SELECT assignment_id
                    FROM assignments
                    WHERE task_id = @TaskId
                    ORDER BY assignment_at DESC
                    LIMIT 1
                );";
            }
            else
            {
                query = @"UPDATE assignments
                SET
                    status = CAST(@status AS task_status)
                WHERE assignment_id = (
                    SELECT assignment_id
                    FROM assignments
                    WHERE task_id = @TaskId
                    ORDER BY assignment_at DESC
                    LIMIT 1
                );";
            }

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
            {
                try
                {
                    int taskId = this.getTaskIdByAssignmentId(assignmentId);
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("Status", status);
                        command.Parameters.AddWithValue("AssignmentId", assignmentId);
                        command.Parameters.AddWithValue("TaskId", taskId);

                        if (status == "in_progress")
                        {
                            command.Parameters.AddWithValue("WorkedBy", workedBy);
                        }

                        command.ExecuteNonQuery();
                    }

                    bool isRoutine = false;
                    string queryCheckTaskType = @"
                    SELECT true 
                    FROM assignments a
                        LEFT JOIN tasks t ON t.task_id = a.task_id
                    WHERE t.task_type_id = 2 AND assignment_id = @AssignmentId
                    LIMIT 1";

                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(queryCheckTaskType))
                    {
                        command.Parameters.AddWithValue("@AssignmentId", assignmentId);
                        var result = command.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            isRoutine = (bool)result;
                        }
                    }


                    if (status == "completed" && isRoutine)
                    {
                        string queryInsert = "INSERT INTO verifications (assignment_id) VALUES (@AssignmentId)";
                        using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(queryInsert))
                        {
                            command.Parameters.AddWithValue("@AssignmentId", assignmentId);
                            command.ExecuteNonQuery();
                        };
                    }
                         
                    return true;
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }
            }
            return false;
        }

        public bool updateProofImage(int assignmentId, [FromBody] List<String> imageUrls)
        {
            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                {
                    string queryDelImageAsn = @"DELETE FROM assignment_images WHERE assignment_id = @AssignmentId";
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(queryDelImageAsn))
                    {
                        command.Parameters.AddWithValue("@AssignmentId", assignmentId);
                        command.ExecuteNonQuery();
                    }

                    foreach (string imageUrl in imageUrls)
                    {
                        int imageId = 0;
                        string queryInsertImage = @"INSERT INTO images (image_url) VALUES (@ImageUrl) RETURNING image_id";
                        using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(queryInsertImage))
                        {
                            command.Parameters.AddWithValue("@ImageUrl", imageUrl);
                            var result = command.ExecuteScalar();
                            imageId = Convert.ToInt32(result);
                        }

                        string queryInsertImageAsn = @"INSERT INTO assignment_images (assignment_id, image_id) VALUES (@AssignmentId, @ImageId)";
                        using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(queryInsertImageAsn))
                        {
                            command.Parameters.AddWithValue("@AssignmentId", assignmentId);
                            command.Parameters.AddWithValue("@ImageId", imageId);
                            var result = command.ExecuteNonQuery();
                            return result > 0;

                        }
                    }
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