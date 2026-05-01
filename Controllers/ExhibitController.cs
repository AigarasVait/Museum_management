using MySqlConnector;
using Museum_management.Data;
using Museum_management.Models;

namespace Museum_management.Controllers
{
    public class ExhibitsController
    {
        private readonly DBConnection _db;

        public ExhibitsController(DBConnection db)
        {
            _db = db;
        }

        /// <summary>
        /// Gets all exhibits from the database
        /// </summary>
        public List<Exhibit> GetExhibitsList()
        {
            var result = new List<Exhibit>();

            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "SELECT id, name, description, length, width, height FROM exhibit",
                conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new Exhibit
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Description = reader.GetString("description"),
                    Length = reader.GetDecimal("length"),
                    Width = reader.GetDecimal("width"),
                    Height = reader.GetDecimal("height")
                });
            }

            return result;
        }

        /// <summary>
        /// Deletes an exhibit by its ID
        /// Returns: number of affected rows (1 = success, 0 = not found)
        /// </summary>
        public int Delete(int id)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "DELETE FROM exhibit WHERE id = @id",
                conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Creates a new exhibit with the provided data
        /// Returns: the ID of the newly inserted exhibit
        /// </summary>
        public int CreateNewExhibit(string name, string description, decimal length, decimal width, decimal height)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "INSERT INTO exhibit (name, description, length, width, height) VALUES (@name, @description, @length, @width, @height); SELECT LAST_INSERT_ID();",
                conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@description", description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@length", length);
            cmd.Parameters.AddWithValue("@width", width);
            cmd.Parameters.AddWithValue("@height", height);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Updates an existing exhibit with the provided data
        /// Returns: number of affected rows (1 = success, 0 = not found)
        /// </summary>
        public int EditExhibit(int id, string name, string description, decimal length, decimal width, decimal height)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "UPDATE exhibit SET name = @name, description = @description, length = @length, width = @width, height = @height WHERE id = @id",
                conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@description", description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@length", length);
            cmd.Parameters.AddWithValue("@width", width);
            cmd.Parameters.AddWithValue("@height", height);

            return cmd.ExecuteNonQuery();
        }
    }
}