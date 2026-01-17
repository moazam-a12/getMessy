using System.Collections.Generic;

namespace Mess_Management_System.Models
{
    public class AdminAttendanceViewModel
    {
        public List<User> Users { get; set; }
        public List<Menu> TodayMenu { get; set; }
        public List<Attendance> Attendances { get; set; } // ✅ Added to track existing attendance
    }
}