using Npgsql;

namespace api_cleany_app.src.Helpers
{
    public class SqlDbHelper : IDisposable
    {
        private readonly NpgsqlConnection _connection;
        public string _connectionString;

        public SqlDbHelper(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new NpgsqlConnection(_connectionString);
        }

        public NpgsqlCommand NpgsqlCommand(string pQuery)
        {
            return new NpgsqlCommand(pQuery, _connection);
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

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
            }
        }
    }
}