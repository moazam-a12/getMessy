using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Mess_Management_System.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Mess_Management_System.Controllers
{
    [Authorize(Roles = "Admin")] // ✅ Only Admin can access
    public class AdminUserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminUserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ List all users
        public IActionResult Index()
        {
            var users = _context.Users.OrderBy(u => u.FullName).ToList();
            return View(users);
        }

        // ✅ GET: Edit User
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // ✅ POST: Edit User
        [HttpPost]
        public IActionResult Edit(User user, string NewPassword)
        {
            // ✅ Server-side validation
            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                ViewBag.Error = "Full name is required.";
                return View(user);
            }

            // ✅ Full name character validation (allow letters, spaces, hyphens, apostrophes)
            if (!IsValidFullName(user.FullName))
            {
                ViewBag.Error = "Full name contains invalid characters.";
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                ViewBag.Error = "Email is required.";
                return View(user);
            }

            if (!IsValidEmail(user.Email))
            {
                ViewBag.Error = "Invalid email format.";
                return View(user);
            }

            // ✅ Check if email already exists (for different user)
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email && u.Id != user.Id);
            if (existingUser != null)
            {
                ViewBag.Error = "Email already exists. Please use a different email.";
                return View(user);
            }

            // ✅ Get existing user from database
            var dbUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (dbUser == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            // ✅ Update user details
            dbUser.FullName = user.FullName;
            dbUser.Email = user.Email;
            dbUser.Role = user.Role;
            dbUser.Drink = user.Drink;
            dbUser.Lunch = user.Lunch;

            // ✅ Update password only if new password is provided
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                if (NewPassword.Length < 6)
                {
                    ViewBag.Error = "Password must be at least 6 characters long.";
                    return View(user);
                }
                dbUser.PasswordHash = HashPassword(NewPassword);
            }

            _context.Users.Update(dbUser);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction("Index");
        }

        // ✅ Delete user
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            // ✅ Prevent deleting last admin user
            if (user.Role.ToLower() == "admin")
            {
                var adminCount = _context.Users.Count(u => u.Role.ToLower() == "admin");
                if (adminCount <= 1)
                {
                    TempData["ErrorMessage"] = "Cannot delete the last admin user.";
                    return RedirectToAction("Index");
                }
            }

            // ✅ Check if user has bills
            var hasBills = _context.Bills.Any(b => b.UserId == id);
            if (hasBills)
            {
                TempData["ErrorMessage"] = "Cannot delete user. User has billing records.";
                return RedirectToAction("Index");
            }

            // ✅ Check if user has attendance records
            var hasAttendance = _context.Attendances.Any(a => a.UserId == id);
            if (hasAttendance)
            {
                TempData["ErrorMessage"] = "Cannot delete user. User has attendance records.";
                return RedirectToAction("Index");
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "User deleted successfully!";
            return RedirectToAction("Index");
        }

        // ✅ Email validation helper
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // ✅ Full name validation helper (letters, spaces, hyphens, apostrophes)
        private bool IsValidFullName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var regex = new System.Text.RegularExpressions.Regex(@"^[\p{L} .'\-]+$");
            return regex.IsMatch(name);
        }

        // ✅ Password hashing helper
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }
    }
}
