using Museum_management.Data;
using Museum_management.Models;
using MySqlConnector;

namespace Museum_management.Controllers
{
    public class ExhibitPlaceHoldersController
    {
        private readonly DBConnection _db;

        // Return value indicators
        public const int AlreadyExists = -1;
        public const int Success = 1;
        public const int NotFound = 0;

        public ExhibitPlaceHoldersController(DBConnection db)
        {
            _db = db;
        }

        /// <summary>
        /// Gets all exhibits from the database
        /// </summary>
        public List<ExhibitPlaceHolder> GetExhibitPlaceHolders()
        {
            var result = new List<ExhibitPlaceHolder>();

            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "SELECT id, length, width, height FROM exhibitPlaceHolders",
                conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new ExhibitPlaceHolder
                {
                    Id = reader.GetInt32("id"),
                    Length = reader.GetDecimal("length"),
                    Width = reader.GetDecimal("width"),
                    Height = reader.GetDecimal("height")
                });
            }

            return result;
        }

        /// <summary>
        /// Returns: 1 = deleted, 0 = not found
        /// </summary>
        public int Delete(int id)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand("DELETE FROM exhibitPlaceHolders WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery();
        }


        
        /// <summary>
        /// Returns:
        ///   > 0  : New exhibit ID (success)
        ///    0   : 
        ///   -1   : Exhibit with this name already exists
        /// </summary>
        public int Create(decimal length, decimal width, decimal height)
        {

            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "INSERT INTO exhibitPlaceHolders (length, width, height) VALUES (@length, @width, @height); SELECT LAST_INSERT_ID();",
                conn);
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


        public bool Validate(int id)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            string query = "SELECT EXISTS (SELECT 1 FROM exhibitPlaceHolders WHERE  id = @id)";
        
            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            return Convert.ToBoolean(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Returns:
        ///    1   : Successfully updated
        ///    0   : Exhibit not found
        /// </summary>
        public int Edit(int id, decimal length, decimal width, decimal height)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "UPDATE exhibitPlaceHolders SET length = @length, width = @width, height = @height WHERE id = @id",
                conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@length", length);
            cmd.Parameters.AddWithValue("@width", width);
            cmd.Parameters.AddWithValue("@height", height);

            return cmd.ExecuteNonQuery();
        }

    }
}
