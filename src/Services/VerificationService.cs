﻿using api_cleany_app.src.Helpers;
using api_cleany_app.src.Models;
using Npgsql;

namespace api_cleany_app.src.Services
{
    public class VerificationService
    {
        private string _connectionString;
        private string _errorMessage = string.Empty;

        public VerificationService()
        {
            _connectionString = DbConfig.ConnectionString;
        }

        public List<Verification> getAllVerifications()
        {
            List<Verification> verifications = new List<Verification>();

            string query = @"
            SELECT 
                verification_id,
                assignment_id,
                u.user_id,
                u.username,
                r.name,
                status,
                feedback,
                verification_at
            FROM verifications v
                LEFT JOIN users u ON v.verification_by = u.user_id
                LEFT JOIN roles r ON r.role_id = u.role_id
            ORDER BY verification_id";

            try
            {
                using (SqlDbHelper sqlHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlHelper.NpgsqlCommand(query))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            verifications.Add(new Verification
                            {
                                Verification_id = reader.GetInt32(0),
                                Assignment_id = reader.GetInt32(1),
                                Verification_by = reader.IsDBNull(2) ? null : new UserCard
                                {
                                    UserId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                    Username = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    Role = reader.IsDBNull(4) ? null : reader.GetString(4),
                                },
                                Status = reader.GetString(5),
                                Feedback = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Verification_at = reader.IsDBNull(7) ? null : reader.GetDateTime(7).ToString(),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            return verifications;
        }

        public List<Verification> getAllVerificationByAssignmentId(int assignmentId)
        {
            List<Verification> verifications = new List<Verification>();

            string query = @"
            SELECT 
                verification_id,
                assignment_id,
                u.user_id,
                u.username,
                r.name,
                status,
                feedback,
                verification_at
            FROM verifications v
                LEFT JOIN users u ON v.verification_by = u.user_id
                LEFT JOIN roles r ON r.role_id = u.role_id
            WHERE assignment_id = @AssignmentId
            ORDER BY verification_id";

            try
            {
                using (SqlDbHelper sqlHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("@AssignmentId", assignmentId);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            verifications.Add(new Verification
                            {
                                Verification_id = reader.GetInt32(0),
                                Assignment_id = reader.GetInt32(1),
                                Verification_by = reader.IsDBNull(2) ? null : new UserCard
                                {
                                    UserId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                    Username = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    Role = reader.IsDBNull(4) ? null : reader.GetString(4),
                                },
                                Status = reader.GetString(5),
                                Feedback = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Verification_at = reader.IsDBNull(7) ? null : reader.GetDateTime(7).ToString(),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            return verifications;
        }

        public Verification getAllVerificationById(int verificationId)
        {
            Verification verification = null;

            string query = @"
            SELECT 
                verification_id,
                assignment_id,
                u.user_id,
                u.username,
                r.name,
                status,
                feedback,
                verification_at
            FROM verifications v
                LEFT JOIN users u ON v.verification_by = u.user_id
                LEFT JOIN roles r ON r.role_id = u.role_id
            WHERE verification_id = @VerificationId
            ORDER BY verification_id";

            try
            {
                using (SqlDbHelper sqlHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("@VerificationId", verificationId);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            verification = new Verification()
                            {
                                Verification_id = reader.GetInt32(0),
                                Assignment_id = reader.GetInt32(1),
                                Verification_by = new UserCard
                                {
                                    UserId = reader.GetInt32(2),
                                    Username = reader.GetString(3),
                                    Role = reader.GetString(4),
                                },
                                Status = reader.GetString(5),
                                Feedback = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Verification_at = reader.IsDBNull(7) ? null : reader.GetDateTime(7).ToString(),
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            return verification;
        }

        public List<Verification> getVerificationByTask(int taskId)
        {
            List<Verification> verifications = new List<Verification>();
            string query = @"SELECT
	            verification_id,
	            v.assignment_id,
                u.user_id,
	            u.username AS verification_by,
                r.name,
	            v.status,
	            v.feedback,
	            v.verification_at
            FROM verifications v
	            LEFT JOIN users u ON u.user_id = v.verification_by
	            LEFT JOIN assignments a ON a.assignment_id = v.assignment_id
	            LEFT JOIN tasks t ON t.task_id = a.assignment_id
                LEFT JOIN roles r ON u.role_id = r.role_id
            WHERE
	            t.task_id = @TaskId";

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("@TaskId", taskId);
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                verifications.Add(new Verification
                                {
                                    Verification_id = reader.GetInt32(0),
                                    Assignment_id = reader.GetInt32(1),
                                    Verification_by = reader.IsDBNull(2) ? null : new UserCard
                                    {
                                        UserId = reader.GetInt32(2),
                                        Username = reader.GetString(3),
                                        Role = reader.GetString(4),
                                    },
                                    Status = reader.GetString(5),
                                    Feedback = reader.IsDBNull(6) ? null : reader.GetString(6),
                                    Verification_at = reader.IsDBNull(7) ? null : reader.GetDateTime(7).ToString(),
                                });
                            }
                        }
                    }
                }   
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            return verifications;
        }
        public bool addVerification(int assignmentId)
        {
            string query = "INSERT INTO verifications (assignment_id) VALUES (@AssignmentId)";
            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("@AssignmentId", assignmentId);
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

        public bool updateVerification(VerificationStatusRequest request, int assignmentId, int userId)
        {
            string query = @"UPDATE verifications v
            SET 
                status = CAST(@Status AS verification_status), 
                verification_at = NOW(), 
                verification_by = @UserId, 
                feedback = @Feedback
            FROM (
                SELECT verification_id 
                FROM verifications 
                WHERE assignment_id = @AssignmentId 
                ORDER BY verification_id DESC 
                LIMIT 1
            ) sub
            WHERE v.verification_id = sub.verification_id;";

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("@AssignmentId", assignmentId);
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@Status", request.Status);
                        command.Parameters.AddWithValue("@Feedback", string.IsNullOrEmpty(request.Feedback) ? DBNull.Value : request.Feedback);
                        command.ExecuteNonQuery();
                    }

                    if (request.Status == "rejected")
                    {
                        string queryAddAssignment = @"INSERT INTO assignments (task_id, worked_by, status) VALUES (@TaskId, NULL, 'pending')";
                        string queryGetTaskId = "SELECT task_id FROM assignments WHERE assignment_id = @AssignmentId";
                        int taskId = 0;

                        using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(queryGetTaskId))
                        {
                            command.Parameters.AddWithValue("@AssignmentId", assignmentId);
                            var result = command.ExecuteScalar();
                            taskId = Convert.ToInt32(result);
                        }

                        if (taskId > 0)
                        {
                            using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(queryAddAssignment))
                            {
                                command.Parameters.AddWithValue("@TaskId", taskId);
                                command.ExecuteNonQuery();
                            }
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }

            return false;
        }

        public bool deleteVerification(int verificationId)
        {
            string query = @"DELETE FROM verifications WHERE verification_id = @VerificationId";
            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("@VerificationId", verificationId);
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
