namespace Museum_management.Models
{
    public class Tour
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Starts_at { get; set; }
        public DateTime Finishes_at { get; set; }
        public string Plan { get; set; }
    }
}
