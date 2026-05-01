using MySqlConnector;
using Museum_management.Data;
using Museum_management.Models;

namespace Museum_management.Controllers
{
    public class ExhibitsController
    {
        private readonly DBConnection _db;

        // Clear return value indicators
        public const int AlreadyExists = -1;
        public const int Success = 1;
        public const int NotFound = 0;

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
        /// Deletes an exhibit by ID
        /// Returns: 1 = deleted, 0 = not found
        /// </summary>
        public int Delete(int id)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand("DELETE FROM exhibit WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Checks if an exhibit with the given name already exists.
        /// Optional excludeId prevents false positives when editing the same exhibit.
        /// </summary>
        public bool CheckIfExists(string name, int? excludeId = null)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            string query = "SELECT COUNT(*) FROM exhibit WHERE LOWER(TRIM(name)) = LOWER(TRIM(@name))";
            if (excludeId.HasValue)
            {
                query += " AND id != @excludeId";
            }

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", name);
            if (excludeId.HasValue)
            {
                cmd.Parameters.AddWithValue("@excludeId", excludeId.Value);
            }

            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }

        /// <summary>
        /// Creates a new exhibit.
        /// Returns:
        ///   > 0  : New exhibit ID (success)
        ///   -1   : Exhibit with this name already exists
        ///    0   : Insertion failed
        /// </summary>
        public int CreateNewExhibit(string name, string description, decimal length, decimal width, decimal height)
        {
            if (CheckIfExists(name))
            {
                return AlreadyExists;
            }

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

            try
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch
            {
                return NotFound;
            }
        }

        /// <summary>
        /// Updates an existing exhibit.
        /// Returns:
        ///    1   : Successfully updated
        ///   -1   : Another exhibit with this name already exists
        ///    0   : Exhibit not found
        /// </summary>
        public int EditExhibit(int id, string name, string description, decimal length, decimal width, decimal height)
        {
            if (CheckIfExists(name, excludeId: id))
            {
                return AlreadyExists;
            }

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