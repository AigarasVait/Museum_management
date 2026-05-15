namespace Museum_management.Models
{
    public class Holiday
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateOnly Start { get; set; }
        public DateOnly End { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
