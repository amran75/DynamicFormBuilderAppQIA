//using DynamicFormBuilderAppQIA.Models;
//using Microsoft.Data.SqlClient;
//using System.Data;

//namespace DynamicFormBuilderAppQIA.Data
//{
//    public class DbContext1
//    {
//        private readonly IConfiguration _config;
//        private readonly string _connectionString;

//        public DbContext1(DbContextConfig config)
//        { 
//            _connectionString = config.ConnectionString;
//        }

//        // Execute non-query commands (INSERT/UPDATE/DELETE)
//        public async Task ExecuteCommand(string sql, SqlParameter[] parameters = null)
//        {
//            using (var connection = new SqlConnection(_connectionString))
//            {
//                using (var command = new SqlCommand(sql, connection))
//                {
//                    if (parameters != null)
//                    {
//                        command.Parameters.AddRange(parameters);
//                    }
//                    await connection.OpenAsync();
//                    await command.ExecuteNonQueryAsync();
//                }
//            }
//        }

//        // Query for single value
//        public async Task<object> ExecuteScalar(string sql, SqlParameter[] parameters = null)
//        {
//            using (var connection = new SqlConnection(_connectionString))
//            {
//                using (var command = new SqlCommand(sql, connection))
//                {
//                    if (parameters != null)
//                    {
//                        command.Parameters.AddRange(parameters);
//                    }
//                    await connection.OpenAsync();
//                    return await command.ExecuteScalarAsync();
//                }
//            }
//        }

//        // Query for data list
//        public async Task<DataTable> Query(string sql, SqlParameter[] parameters = null)
//        {
//            using (var connection = new SqlConnection(_connectionString))
//            {
//                using (var command = new SqlCommand(sql, connection))
//                {
//                    if (parameters != null)
//                    {
//                        command.Parameters.AddRange(parameters);
//                    }
//                    await connection.OpenAsync();
//                    using (var reader = await command.ExecuteReaderAsync())
//                    {
//                        var dataTable = new DataTable();
//                        dataTable.Load(reader);
//                        return dataTable;
//                    }
//                }
//            }
//        }

//        // Insert form and return ID
//        public async Task<int> InsertForm(FormModel form)
//        {
//            const string sql = @"
//            INSERT INTO Forms (Title) 
//            VALUES (@Title);
//            SELECT SCOPE_IDENTITY();";

//            var parameters = new SqlParameter[]
//            {
//            new SqlParameter("@Title", form.Title)
//            };

//            var result = await ExecuteScalar(sql, parameters);
//            return Convert.ToInt32(result);
//        }
//    }
//}
