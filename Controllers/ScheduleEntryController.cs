using Museum_management.Data;
using Museum_management.Models;
using System.Linq;
using MySqlConnector;

namespace Museum_management.Controllers
{
    public class ScheduleEntryController
    {
        private readonly DBConnection _db;
        private readonly EmployeeController _employeeController;
        private readonly HolidayController _holidayController;

        private Dictionary<int, HashSet<DateOnly>> _currentHolidaysByEmployee = new();
        private Dictionary<int, HashSet<DateOnly>> _currentWorkedDaysByEmployee = new();
        private Dictionary<(int empId, DateOnly date), int> _currentWorkedShiftCountsByEmployeeDate = new();
        private Dictionary<int, int> _currentHoursByEmployee = new();
        private Dictionary<int, int> _currentEmployeeFteHours = new();
        private Dictionary<(DateOnly date, int startHour, int endHour), HashSet<int>> _currentAssignedEmployeesBySlot = new();

        public ScheduleEntryController(DBConnection db, EmployeeController employeeController, HolidayController holidayController)
        {
            _db = db;
            _employeeController = employeeController;
            _holidayController = holidayController;
        }

        private bool Solve(List<ScheduleEntry> slots, List<Employee> employees, int pos, int rotation)
        {
            if (pos >= slots.Count)
                return true;

            var slot = slots[pos];

            for (int i = 0; i < employees.Count; i++)
            {
                var idx = (rotation + i) % employees.Count;
                var employee = employees[idx];

                if (!IsValidEmployee(employee, slot))
                    continue;

                var slotKey = (slot.Date, slot.StartHour, slot.EndHour);
                if (!_currentAssignedEmployeesBySlot.TryGetValue(slotKey, out var assignedEmployees))
                {
                    assignedEmployees = new HashSet<int>();
                    _currentAssignedEmployeesBySlot[slotKey] = assignedEmployees;
                }

                _currentHoursByEmployee.TryGetValue(employee.Id, out var currentHours);
                var workedDayKey = (employee.Id, slot.Date);
                _currentWorkedShiftCountsByEmployeeDate.TryGetValue(workedDayKey, out var shiftsOnDay);

                slot.EmployeeId = employee.Id;
                assignedEmployees.Add(employee.Id);
                _currentHoursByEmployee[employee.Id] = currentHours + (slot.EndHour - slot.StartHour);
                _currentWorkedShiftCountsByEmployeeDate[workedDayKey] = shiftsOnDay + 1;
                _currentWorkedDaysByEmployee[employee.Id].Add(slot.Date);

                if (Solve(slots, employees, pos + 1, (idx + 1) % employees.Count))
                    return true;

                Backtrack(employee.Id, slot, assignedEmployees, workedDayKey, currentHours);
            }
            return false;
        }

        private void Backtrack(int employeeId, ScheduleEntry slot, HashSet<int> assignedEmployees, (int empId, DateOnly date) workedDayKey, int previousHours)
        {
            slot.EmployeeId = 0;
            assignedEmployees.Remove(employeeId);
            _currentHoursByEmployee[employeeId] = previousHours;

            if (_currentWorkedShiftCountsByEmployeeDate.TryGetValue(workedDayKey, out var currentDayShiftCount))
            {
                if (currentDayShiftCount <= 1)
                {
                    _currentWorkedShiftCountsByEmployeeDate.Remove(workedDayKey);
                    _currentWorkedDaysByEmployee[employeeId].Remove(workedDayKey.date);
                }
                else
                {
                    _currentWorkedShiftCountsByEmployeeDate[workedDayKey] = currentDayShiftCount - 1;
                }
            }
        }

        private bool IsValidEmployee(Employee employee, ScheduleEntry shift)
        {
            if (employee is null)
                return false;

            if (_currentHolidaysByEmployee.TryGetValue(employee.Id, out var daysOff) && daysOff.Contains(shift.Date))
                return false;

            if (_currentWorkedDaysByEmployee.TryGetValue(employee.Id, out var workedDays) && workedDays.Count > 0)
            {
                var streak = 0;
                for (var check = shift.Date.AddDays(-1); workedDays.Contains(check); check = check.AddDays(-1))
                {
                    streak++;
                    if (streak >= 3)
                        return false;
                }
            }

            var slotKey = (shift.Date, shift.StartHour, shift.EndHour);
            if (_currentAssignedEmployeesBySlot.TryGetValue(slotKey, out var assignedEmployees) && assignedEmployees.Contains(employee.Id))
                return false;

            var hours = shift.EndHour - shift.StartHour;
            _currentHoursByEmployee.TryGetValue(employee.Id, out var workedHours);
            _currentEmployeeFteHours.TryGetValue(employee.Id, out var allowedHours);

            return workedHours + hours <= allowedHours;
        }

        public bool GenerateSchedule(int year, int month)
        {
            var employees = _employeeController.GetAllEmployees();
            if (employees == null || employees.Count == 0)
                return false;

            var employeeFteHours = employees.ToDictionary(e => e.Id, e => (int)Math.Round(160m * e.Fte, MidpointRounding.AwayFromZero));

            var daysInMonth = DateTime.DaysInMonth(year, month);

            var slots = new List<ScheduleEntry>(daysInMonth * 8);
            for (int d = 1; d <= daysInMonth; d++)
            {
                for (var startHour = 8; startHour < 16; startHour += 2)
                {
                    for (var coverage = 0; coverage < 2; coverage++)
                    {
                        slots.Add(new ScheduleEntry
                        {
                            EmployeeId = 0,
                            Date = new DateOnly(year, month, d),
                            StartHour = startHour,
                            EndHour = startHour + 2,
                        });
                    }
                }
            }

            var firstDayOfMonth = new DateOnly(year, month, 1);
            var lastDayOfMonth = new DateOnly(year, month, daysInMonth);

            var overlappingHolidays = _holidayController.GetUpcomingHolidays(firstDayOfMonth, lastDayOfMonth);
            var holidaysByEmployee = employees.ToDictionary(e => e.Id, _ => new HashSet<DateOnly>());

            foreach (var h in overlappingHolidays)
            {
              if (!holidaysByEmployee.TryGetValue(h.EmployeeId, out var set))
                  continue;

              for (var d = h.Start; d <= h.End; d = d.AddDays(1))
                  set.Add(d);
            }

            _currentHolidaysByEmployee = holidaysByEmployee;
            _currentEmployeeFteHours = employeeFteHours;
            _currentAssignedEmployeesBySlot = new Dictionary<(DateOnly date, int startHour, int endHour), HashSet<int>>();
            _currentWorkedShiftCountsByEmployeeDate = new Dictionary<(int empId, DateOnly date), int>();
            _currentHoursByEmployee = employees.ToDictionary(e => e.Id, _ => 0);
            _currentWorkedDaysByEmployee = employees.ToDictionary(e => e.Id, _ => new HashSet<DateOnly>());

            var ok = Solve(slots, employees, 0, 0);
            if (!ok)
                return false;

            using var conn = _db.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                var insertCmd = new MySqlCommand(@"INSERT INTO schedule_entry (employee_id, date, `start`, `end`) VALUES (@employee_id, @date, @start, @end);", conn, tx);
                insertCmd.Parameters.Add(new MySqlParameter("@employee_id", MySqlDbType.Int32));
                insertCmd.Parameters.Add(new MySqlParameter("@date", MySqlDbType.DateTime));
                insertCmd.Parameters.Add(new MySqlParameter("@start", MySqlDbType.Int32));
                insertCmd.Parameters.Add(new MySqlParameter("@end", MySqlDbType.Int32));

                foreach (var e in slots)
                {
                    insertCmd.Parameters["@employee_id"].Value = e.EmployeeId;
                    insertCmd.Parameters["@date"].Value = e.Date.ToDateTime(TimeOnly.MinValue);
                    insertCmd.Parameters["@start"].Value = e.StartHour;
                    insertCmd.Parameters["@end"].Value = e.EndHour;
                    insertCmd.ExecuteNonQuery();
                }

                tx.Commit();
                return true;
            }
            catch
            {
                try { tx.Rollback(); } catch { }
                return false;
            }
        }

        public bool HasSchedule(int year, int month)
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"SELECT COUNT(1) FROM schedule_entry WHERE YEAR(date)=@year AND MONTH(date)=@month;", conn);
            cmd.Parameters.AddWithValue("@year", year);
            cmd.Parameters.AddWithValue("@month", month);
            var result = cmd.ExecuteScalar();
            if (result == null || result == DBNull.Value) return false;
            return Convert.ToInt32(result) > 0;
        }

        public List<ScheduleEntry> GetUserSchedule(int employeeId)
        {
            var entries = new List<ScheduleEntry>();
            using var conn = _db.CreateConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT id, employee_id, date, `start`, `end`
                FROM schedule_entry 
                WHERE employee_id = @employee_id 
                ORDER BY date ASC, `start` ASC;", conn);
            
            cmd.Parameters.AddWithValue("@employee_id", employeeId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                entries.Add(new ScheduleEntry
                {
                    Id = reader.GetInt32("id"),
                    EmployeeId = reader.GetInt32("employee_id"),
                    Date = DateOnly.FromDateTime(reader.GetDateTime("date")),
                    StartHour = reader.GetInt32("start"),
                    EndHour = reader.GetInt32("end")
                });
            }

            return entries;
        }
    }
}
