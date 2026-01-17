using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Mess_Management_System.Models;
using System.Linq;
using System.Security.Claims;

namespace Mess_Management_System.Controllers
{
    [Authorize(Roles = "User")] // ✅ Only regular users can access
    public class UserMenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserMenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Helper methods to get current user info from JWT claims
        private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        private string GetCurrentUserName()
        {
            return User.FindFirst("FullName")?.Value
                   ?? User.FindFirst(ClaimTypes.Name)?.Value
                   ?? User.FindFirst(ClaimTypes.Email)?.Value
                   ?? "User";
        }

        private string GetCurrentUserEmail() => User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        private string GetCurrentUserRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

        // ----------------- Show Today's Menu (READ ONLY) -----------------
        public IActionResult Home()
        {
            int userId = GetCurrentUserId();

            // ✅ Display welcome message if exists
            if (TempData["WelcomeMessage"] != null)
            {
                ViewBag.WelcomeMessage = TempData["WelcomeMessage"].ToString();
            }

            // ✅ Get today's menu
            var todayMenu = _context.Menus
                .Where(m => m.Date.Date == DateTime.Now.Date)
                .OrderBy(m => m.IsFood) // Drinks first, then food
                .ToList();

            // ✅ Get user's attendance for today (marked by admin)
            var attendedIds = _context.Attendances
                .Include(a => a.Menu)
                .Where(a => a.UserId == userId &&
                           a.Attended &&
                           a.Menu.Date.Date == DateTime.Now.Date)
                .Select(a => a.MenuId)
                .ToList();

            ViewBag.Username = GetCurrentUserName();
            ViewBag.Email = GetCurrentUserEmail();
            ViewBag.Role = GetCurrentUserRole();
            ViewBag.UserAttendance = attendedIds;

            // ✅ Get user's bills summary
            var bills = _context.Bills
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.Date)
                .ToList();

            // ✅ Calculate monthly summary
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var currentMonthBills = bills.Where(b => b.Date.Month == currentMonth && b.Date.Year == currentYear).ToList();
            var previousMonthBills = bills.Where(b => b.Date.Month == (currentMonth == 1 ? 12 : currentMonth - 1) &&
                                                      b.Date.Year == (currentMonth == 1 ? currentYear - 1 : currentYear)).ToList();

            ViewBag.CurrentMonthTotal = currentMonthBills.Sum(b => b.Amount);
            ViewBag.CurrentMonthPaid = currentMonthBills.Where(b => b.Paid).Sum(b => b.Amount);
            ViewBag.CurrentMonthUnpaid = currentMonthBills.Where(b => !b.Paid).Sum(b => b.Amount);

            ViewBag.PreviousMonthTotal = previousMonthBills.Sum(b => b.Amount);
            ViewBag.PreviousMonthPaid = previousMonthBills.Where(b => b.Paid).Sum(b => b.Amount);
            ViewBag.PreviousMonthUnpaid = previousMonthBills.Where(b => !b.Paid).Sum(b => b.Amount);

            ViewBag.TotalUnpaid = bills.Where(b => !b.Paid).Sum(b => b.Amount);
            ViewBag.TotalPaid = bills.Where(b => b.Paid).Sum(b => b.Amount);
            ViewBag.GrandTotal = bills.Sum(b => b.Amount);

            ViewBag.RecentBills = bills.Take(5).ToList();

            return View(todayMenu);
        }

        // ----------------- Bill History -----------------
        public IActionResult BillHistory()
        {
            return RedirectToAction("Index", "UserBill");
        }

        // ✅ View Attendance History
        public IActionResult AttendanceHistory()
        {
            int userId = GetCurrentUserId();

            // ✅ Get all attendance records for this user
            var attendances = _context.Attendances
                .Include(a => a.Menu)
                .Where(a => a.UserId == userId && a.Attended)
                .OrderByDescending(a => a.Menu.Date)
                .ToList();

            ViewBag.Username = GetCurrentUserName();
            ViewBag.Email = GetCurrentUserEmail();
            ViewBag.Role = GetCurrentUserRole();

            // ✅ Calculate attendance stats
            var totalDays = attendances.Count;
            var foodDays = attendances.Count(a => a.Menu.IsFood);
            var drinkDays = attendances.Count(a => !a.Menu.IsFood);
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var thisMonthDays = attendances.Count(a => a.Menu.Date.Month == currentMonth &&
                                                       a.Menu.Date.Year == currentYear);

            ViewBag.TotalDays = totalDays;
            ViewBag.FoodDays = foodDays;
            ViewBag.DrinkDays = drinkDays;
            ViewBag.ThisMonthDays = thisMonthDays;

            return View(attendances);
        }
    }
}
