namespace Museum_management.Models
{
    public class ScheduleEntry
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateOnly Date { get; set; }
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
