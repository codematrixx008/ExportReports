using API1.Interface;
using System.Data;
using System.Data.SqlClient;

namespace API1.DapperDbConnection
{
    public class APIDevelopmentUsingDapper : IDapperDbConnection
    {
            public readonly string _connectionString;

            public APIDevelopmentUsingDapper(IConfiguration configuration)
            {
                _connectionString = configuration.GetConnectionString("DefaultConnection");
            }

            public IDbConnection CreateConnection()
            {
                return new SqlConnection(_connectionString);
            }
    }
}
