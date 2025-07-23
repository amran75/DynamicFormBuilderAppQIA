using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace DynamicFormBuilderAppQIA.Data
{
    public class DbContext : IDisposable
    {
        private readonly string _connectionString;
        private SqlConnection _connection;
        private bool _disposed = false;

        public SqlConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SqlConnection(_connectionString);
                }
                return _connection;
            }
        }

        public DbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task OpenConnectionAsync()
        {
            if (Connection.State != ConnectionState.Open)
            {
                await Connection.OpenAsync();
            }
        }

        public async Task<DataTable> QueryAsync(string sql, params SqlParameter[] parameters)
        {
            await OpenConnectionAsync();

            using var command = new SqlCommand(sql, Connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            return dataTable;
        }

        public async Task<int> ExecuteCommandAsync(string sql, params SqlParameter[] parameters)
        {
            await OpenConnectionAsync();

            using var command = new SqlCommand(sql, Connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteNonQueryAsync();
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, params SqlParameter[] parameters)
        {
            await OpenConnectionAsync();

            using var command = new SqlCommand(sql, Connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            var result = await command.ExecuteScalarAsync();
            return (T)Convert.ChangeType(result, typeof(T));
        }

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
                    _connection?.Close();
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}