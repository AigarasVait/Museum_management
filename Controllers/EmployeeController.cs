using Museum_management.Data;
using Museum_management.Models;
using MySqlConnector;

namespace Museum_management.Controllers
{
    public class EmployeeController
    {
        private readonly DBConnection _db;

        public EmployeeController(DBConnection db)
        {
            _db = db;
        }

        public List<Employee> GetAllEmployees()
        {
            var result = new List<Employee>();

            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT id, name, surname, fte, email, password, role
                FROM employee;", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Employee
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.IsDBNull(reader.GetOrdinal("name")) ? string.Empty : reader.GetString("name"),
                    Surname = reader.IsDBNull(reader.GetOrdinal("surname")) ? string.Empty : reader.GetString("surname"),
                    Fte = reader.IsDBNull(reader.GetOrdinal("fte")) ? 1m : reader.GetDecimal("fte"),
                    Email = reader.IsDBNull(reader.GetOrdinal("email")) ? string.Empty : reader.GetString("email"),
                    Password = reader.IsDBNull(reader.GetOrdinal("password")) ? string.Empty : reader.GetString("password"),
                });
            }

            return result;
        }
    }
}
