using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace Museum_management.Data
{
    public class DBConnection
    {
        private readonly IConfiguration _config;

        public DBConnection(IConfiguration config)
        {
            _config = config;
        }

        public MySqlConnection CreateConnection()
        {
            var connString = _config["ConnectionStrings:Default"];
            return new MySqlConnection(connString);
        }
    }
}
