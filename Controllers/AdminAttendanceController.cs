using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Mess_Management_System.Models;
using System;
using System.Linq;

namespace Mess_Management_System.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AdminAttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminAttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

    
        public IActionResult Index(DateTime? clientDate)
        {

            // Determine date to use for "today" — prefer client-provided date when available
            DateTime dateToUse;
            if (clientDate.HasValue)
            {
                dateToUse = clientDate.Value.Date;
            }
            else
            {
                // try reading client date from cookie (set by browser JS)
                if (Request.Cookies.TryGetValue("clientDate", out var cd) && DateTime.TryParse(cd, out var parsed))
                {
                    dateToUse = parsed.Date;
                }
                else
                {
                    dateToUse = DateTime.Now.Date;
                }
            }

            var todayMenu = _context.Menus
                .Where(m => m.Date.Date == dateToUse)
                .OrderBy(m => m.IsFood)
                .ToList();


            var users = _context.Users
                .OrderBy(u => u.FullName)
                .ToList();

    
            var todayAttendances = _context.Attendances
                .Include(a => a.Menu)
                .Where(a => a.Menu.Date.Date == dateToUse)
                .ToList();


            var model = new AdminAttendanceViewModel
            {
                Users = users,
                TodayMenu = todayMenu,
                Attendances = todayAttendances
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult MarkAttendance(int userId, int menuId, bool attended)
        {
            try
            {
             
                var attendance = _context.Attendances
                    .FirstOrDefault(a => a.UserId == userId && a.MenuId == menuId);

                if (attendance == null)
                {
                  
                    attendance = new Attendance
                    {
                        UserId = userId,
                        MenuId = menuId,
                        Attended = attended
                    };
                    _context.Attendances.Add(attendance);
                }
                else
                {
           
                    attendance.Attended = attended;
                    _context.Attendances.Update(attendance);
                }

                _context.SaveChanges();

                return Json(new { success = true, message = "Attendance updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating attendance: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AutoMarkDrinks(DateTime? clientDate)
        {
            try
            {
                DateTime dateToUse;
                if (clientDate.HasValue)
                {
                    // Use client-provided local date (browser)
                    dateToUse = clientDate.Value.Date;
                }
                else if (Request.Cookies.TryGetValue("clientDate", out var cd) && DateTime.TryParse(cd, out var parsed))
                {
                    dateToUse = parsed.Date;
                }
                else
                {
                    // fallback to server local date
                    dateToUse = DateTime.Now.Date;
                }

                var todayDrinks = _context.Menus
                    .Where(m => m.Date.Date == dateToUse && !m.IsFood)
                    .ToList();

                // If still no menus found, fallback to UTC date (server) for robustness
                if (!todayDrinks.Any())
                {
                    var todayUtc = DateTime.UtcNow.Date;
                    todayDrinks = _context.Menus
                        .Where(m => m.Date.Date == todayUtc && !m.IsFood)
                        .ToList();

                    if (todayDrinks.Any())
                    {
                        TempData["WarningMessage"] = "Auto-mark used UTC date fallback to find drink menus.";
                    }
                }

                if (!todayDrinks.Any())
                {
                    TempData["SuccessMessage"] = "No drink menus found for today to auto-mark.";
                    return RedirectToAction("Index");
                }

                var users = _context.Users.ToList();

                int markedCount = 0;

                foreach (var user in users)
                {
                    foreach (var drink in todayDrinks)
                    {
                        var existingAttendance = _context.Attendances
                            .FirstOrDefault(a => a.UserId == user.Id && a.MenuId == drink.Id);

                        if (existingAttendance == null)
                        {
                            var attendance = new Attendance
                            {
                                UserId = user.Id,
                                MenuId = drink.Id,
                                Attended = true
                            };
                            _context.Attendances.Add(attendance);
                            markedCount++;
                        }
                        else if (!existingAttendance.Attended)
                        {
                            existingAttendance.Attended = true;
                            _context.Attendances.Update(existingAttendance);
                            markedCount++;
                        }
                    }
                }

                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Successfully auto-marked {markedCount} drink attendances!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error auto-marking drinks: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public IActionResult ExportAttendance()
        {
            var today = DateTime.Now.Date;

            var attendanceData = _context.Attendances
                .Include(a => a.User)
                .Include(a => a.Menu)
                .Where(a => a.Menu.Date.Date == today && a.Attended)
                .Select(a => new
                {
                    UserName = a.User.FullName,
                    MenuItem = a.Menu.Name,
                    Price = a.Menu.Price,
                    Type = a.Menu.IsFood ? "Food" : "Drink"
                })
                .ToList();

            ViewBag.AttendanceData = attendanceData;
            ViewBag.Date = today.ToString("yyyy-MM-dd");

            return View();
        }
    }
}