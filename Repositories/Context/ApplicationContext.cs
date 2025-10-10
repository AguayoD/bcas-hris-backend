using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Repositories.Context
{
    public class ApplicationContext
    {
        private readonly string _connectionString;

        public ApplicationContext(string connectionString = "DefaultSqlConnection")
        {

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetConnectionString(connectionString);
        }


        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
