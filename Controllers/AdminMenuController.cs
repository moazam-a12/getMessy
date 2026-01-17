using Mess_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Mess_Management_System.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AdminMenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminMenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var menu = _context.Menus.OrderByDescending(m => m.Date).ToList();
            return View(menu);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(Menu menu)
        {
            if (string.IsNullOrWhiteSpace(menu.Name))
            {
                ViewBag.Error = "Menu name is required.";
                return View(menu);
            }

            if (menu.Price <= 0)
            {
                ViewBag.Error = "Price must be greater than 0.";
                return View(menu);
            }

            if (menu.Date.Date < DateTime.Now.Date)
            {
                ViewBag.Error = "Date cannot be in the past.";
                return View(menu);
            }

            var existingMenu = _context.Menus
                .FirstOrDefault(m => m.Name == menu.Name && m.Date.Date == menu.Date.Date);

            if (existingMenu != null)
            {
                ViewBag.Error = $"A menu item '{menu.Name}' already exists for {menu.Date.ToShortDateString()}.";
                return View(menu);
            }

            // ✅ Validate ModelState
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please fill all required fields correctly.";
                return View(menu);
            }

            _context.Menus.Add(menu);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Menu item added successfully!";
            return RedirectToAction("Index");
        }

        // ✅ Edit Menu GET
        public IActionResult Edit(int id)
        {
            var menu = _context.Menus.Find(id);
            if (menu == null)
            {
                TempData["ErrorMessage"] = "Menu item not found.";
                return RedirectToAction("Index");
            }

            return View(menu);
        }

        // ✅ Edit Menu POST with Server-Side Validation
        [HttpPost]
        public IActionResult Edit(Menu menu)
        {
            // ✅ Server-side validation
            if (string.IsNullOrWhiteSpace(menu.Name))
            {
                ViewBag.Error = "Menu name is required.";
                return View(menu);
            }

            if (menu.Price <= 0)
            {
                ViewBag.Error = "Price must be greater than 0.";
                return View(menu);
            }

            if (menu.Date.Date < DateTime.Now.Date)
            {
                ViewBag.Error = "Date cannot be in the past.";
                return View(menu);
            }

            // ✅ Check for duplicate menu item (same name, same date, different ID)
            var existingMenu = _context.Menus
                .FirstOrDefault(m => m.Name == menu.Name && m.Date.Date == menu.Date.Date && m.Id != menu.Id);

            if (existingMenu != null)
            {
                ViewBag.Error = $"A menu item '{menu.Name}' already exists for {menu.Date.ToShortDateString()}.";
                return View(menu);
            }

            // ✅ Validate ModelState
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please fill all required fields correctly.";
                return View(menu);
            }

            _context.Menus.Update(menu);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Menu item updated successfully!";
            return RedirectToAction("Index");
        }

        // ✅ Delete Menu with Confirmation
        public IActionResult Delete(int id)
        {
            var menu = _context.Menus.Find(id);
            if (menu == null)
            {
                TempData["ErrorMessage"] = "Menu item not found.";
                return RedirectToAction("Index");
            }

            // ✅ Check if menu has attendance records
            var hasAttendance = _context.Attendances.Any(a => a.MenuId == id);
            if (hasAttendance)
            {
                TempData["ErrorMessage"] = "Cannot delete menu item. Attendance records exist for this item.";
                return RedirectToAction("Index");
            }

            _context.Menus.Remove(menu);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Menu item deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
