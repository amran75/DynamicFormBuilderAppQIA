namespace DynamicFormBuilderAppQIA.Data
{
    public class DbContextConfig
    {
        public string ConnectionString { get; }
        public DbContextConfig(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
