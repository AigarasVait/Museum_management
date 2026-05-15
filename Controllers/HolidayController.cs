using Museum_management.Data;
using Museum_management.Models;
using Museum_management.Services;
using MySqlConnector;

namespace Museum_management.Controllers
{
    public class HolidayController
    {
        private readonly DBConnection _db;
        private readonly ToursController _toursController;

        public HolidayController(DBConnection db, ToursController toursController)
        {
            _db = db;
            _toursController = toursController;
        }

        public List<Holiday> GetHolidaysByUserId(int employeeId)
        {
            var result = new List<Holiday>();

            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT id, employee_id, start, end, status
                FROM holiday
                WHERE employee_id = @employee_id
                ORDER BY id DESC;", conn);
            cmd.Parameters.AddWithValue("@employee_id", employeeId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Holiday
                {
                    Id = reader.GetInt32("id"),
                    EmployeeId = reader.GetInt32("employee_id"),
                    Start = DateOnly.FromDateTime(reader.GetDateTime("start")),
                    End = DateOnly.FromDateTime(reader.GetDateTime("end")),
                    Status = reader.GetString("status")
                });
            }

            return result;
        }

        public bool CreateHoliday(int employeeId, DateTime startDate, DateTime endDate)
        {
            var requestStart = startDate.Date;
            var requestEnd = endDate.Date;

            if (_toursController.HasUpcomingTour(employeeId, requestStart, requestEnd))
            {
                return false;
            }

            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                INSERT INTO holiday (employee_id, start, end, status)
                VALUES (@employee_id, @start, @end, @status);", conn);

            cmd.Parameters.AddWithValue("@employee_id", employeeId);
            cmd.Parameters.AddWithValue("@start", requestStart);
            cmd.Parameters.AddWithValue("@end", requestEnd);
            cmd.Parameters.AddWithValue("@status", "Pateikta");

            cmd.ExecuteNonQuery();
            return true;
        }

        public int ChangeStatus(int holidayId, int employeeId, string status)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                UPDATE holiday
                SET status = @status
                WHERE id = @id AND employee_id = @employee_id;", conn);
            cmd.Parameters.AddWithValue("@id", holidayId);
            cmd.Parameters.AddWithValue("@employee_id", employeeId);
            cmd.Parameters.AddWithValue("@status", status);

            return cmd.ExecuteNonQuery();
        }

        public List<Holiday> GetHolidayRequests()
        {
            var result = new List<Holiday>();

            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT id, employee_id, start, end, status
                FROM holiday
                WHERE status = @status
                ORDER BY id DESC;", conn);
            cmd.Parameters.AddWithValue("@status", "Pateikta");
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Holiday
                {
                    Id = reader.GetInt32("id"),
                    EmployeeId = reader.GetInt32("employee_id"),
                    Start = DateOnly.FromDateTime(reader.GetDateTime("start")),
                    End = DateOnly.FromDateTime(reader.GetDateTime("end")),
                    Status = reader.GetString("status")
                });
            }

            return result;
        }
    }
}
