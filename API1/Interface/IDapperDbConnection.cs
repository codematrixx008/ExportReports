using System.Data;
namespace API1.Interface
{
    public interface IDapperDbConnection
    {
        public IDbConnection CreateConnection();
    }
}
