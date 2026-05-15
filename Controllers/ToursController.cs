using MySqlConnector;
using Museum_management.Data;
using Museum_management.Models;

namespace Museum_management.Services
{
    public class ToursController
    {
        private readonly DBConnection _db;

        public ToursController(DBConnection db)
        {
            _db = db;
        }

        public List<Tour> GetToursList()
        {
            var result = new List<Tour>();

            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "SELECT id, title, description, starts_at, finishes_at, plan FROM tour",
                conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new Tour
                {
                    Id = reader.GetInt32("id"),
                    Title = reader.GetString("title"),
                    Description = reader.GetString("description"),
                    Starts_at = reader.GetDateTime("starts_at"),
                    Finishes_at = reader.GetDateTime("finishes_at"),
                    Plan = reader.GetString("plan")
                });
            }

            return result;
        }
        public Tour GetTourById(int id)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT id, title, description, starts_at, finishes_at, plan  FROM tour WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Tour
                {
                    Id = reader.GetInt32("id"),
                    Title = reader.GetString("title"),
                    Description = reader.GetString("description"),
                    Starts_at = reader.GetDateTime("starts_at"),
                    Finishes_at = reader.GetDateTime("finishes_at"),
                    Plan = reader.GetString("plan")
                };
            }
            return null;
        }

        public bool HasUpcomingTour(int employeeId, DateTime startDate, DateTime endDate)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT COUNT(*)
                FROM tour
                WHERE employee_id = @employee_id
                                    AND DATE(starts_at) <= @end_date
                                    AND DATE(finishes_at) >= @start_date;", conn);

            cmd.Parameters.AddWithValue("@employee_id", employeeId);
            cmd.Parameters.AddWithValue("@start_date", startDate);
            cmd.Parameters.AddWithValue("@end_date", endDate);

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }
}