using Npgsql;

namespace api_cleany_app.src.Helpers
{
    public class SqlDbHelper : IDisposable
    {
        private readonly NpgsqlConnection _connection;
        private string _connectionString;
        private string _errorMessage = string.Empty;

        public SqlDbHelper(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new NpgsqlConnection(_connectionString);
        }

        public NpgsqlCommand NpgsqlCommand(string pQuery)
        {
            try
            {
                this.OpenConnection();
                return new NpgsqlCommand(pQuery, _connection);
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            return null;
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

        public string getError() => _errorMessage;

        public void Dispose()
        {
            if (_connection != null)
            {
                this.CloseConnection();
                _connection.Dispose();
            }
        }
    }
}