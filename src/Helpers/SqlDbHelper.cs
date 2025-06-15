using Npgsql;
using System;

namespace api_cleany_app.src.Helpers
{
    public class SqlDbHelper : IDisposable
    {
        private readonly NpgsqlConnection _connection;
        private readonly string _connectionString;
        private string _errorMessage = string.Empty;
        private bool _disposed = false;

        public SqlDbHelper(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new NpgsqlConnection(_connectionString);
        }

        public NpgsqlConnection Connection => _connection;

        public NpgsqlCommand NpgsqlCommand(string query)
        {
            try
            {
                this.OpenConnection();
                return new NpgsqlCommand(query, _connection);
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                return null;
            }
        }

        public NpgsqlCommand NpgsqlCommand(string query, NpgsqlTransaction transaction)
        {
            try
            {
                this.OpenConnection();
                return new NpgsqlCommand(query, _connection, transaction);
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                return null;
            }
        }

        public void OpenConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        public string GetError() => _errorMessage;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    this.CloseConnection();
                    _connection.Dispose();
                }
                _disposed = true;
            }
        }

        ~SqlDbHelper()
        {
            Dispose(false);
        }
    }
}
