

//using Microsoft.AspNetCore.Mvc;
//using Mess_Management_System.Models;
//using System.Security.Cryptography;
//using System.Text;
//using System.Linq;

//namespace Mess_Management_System.Controllers
//{
//    public class RegisterController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public RegisterController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet]
//        public IActionResult Register_Page()
//        {
//            return View();
//        }

//        [HttpPost]
//        public IActionResult Register_Page(string FullName, string Email, string Password, string ConfirmPassword)
//        {
//            // ✅ Server-side validation
//            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) ||
//                string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
//            {
//                ViewBag.Error = "All fields are required.";
//                return View();
//            }

//            // ✅ Email format validation
//            if (!IsValidEmail(Email))
//            {
//                ViewBag.Error = "Invalid email format.";
//                return View();
//            }

//            // ✅ Password length validation
//            if (Password.Length < 6)
//            {
//                ViewBag.Error = "Password must be at least 6 characters long.";
//                return View();
//            }

//            // ✅ Password match validation
//            if (Password != ConfirmPassword)
//            {
//                ViewBag.Error = "Passwords do not match.";
//                return View();
//            }

//            // ✅ Check if email already exists (Email Uniqueness)
//            var existingUser = _context.Users.FirstOrDefault(u => u.Email == Email);
//            if (existingUser != null)
//            {
//                ViewBag.Error = "Email already registered. Please use a different email.";
//                return View();
//            }

//            string hashedPassword = HashPassword(Password);

//            // ✅ First user becomes Admin, rest become User
//            bool isFirstUser = !_context.Users.Any();
//            string userRole = isFirstUser ? "Admin" : "User";

//            var newUser = new User
//            {
//                FullName = FullName,
//                Email = Email,
//                PasswordHash = hashedPassword,
//                Drink = true,
//                Lunch = false,
//                Role = userRole // ✅ Auto-assign role based on first user
//            };

//            _context.Users.Add(newUser);
//            _context.SaveChanges();

//            // ✅ Success message
//            TempData["SuccessMessage"] = isFirstUser
//                ? "Admin account created successfully! Please login."
//                : "Account created successfully! Please login.";

//            return RedirectToAction("Login_Page", "Login");
//        }

//        // ✅ Email validation helper method
//        private bool IsValidEmail(string email)
//        {
//            try
//            {
//                var addr = new System.Net.Mail.MailAddress(email);
//                return addr.Address == email;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        private string HashPassword(string password)
//        {
//            using var sha256 = SHA256.Create();
//            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
//            return string.Concat(bytes.Select(b => b.ToString("x2")));
//        }
//    }
//}


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Mess_Management_System.Models;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace Mess_Management_System.Controllers
{
    [AllowAnonymous] // ✅ Allow anonymous access to registration
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register_Page()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register_Page(string FullName, string Email, string Password, string ConfirmPassword)
        {
            // ✅ Server-side validation
            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) ||
                string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            // ✅ Full name character validation (allow letters, spaces, hyphens, apostrophes)
            if (!IsValidFullName(FullName))
            {
                ViewBag.Error = "Full name contains invalid characters.";
                return View();
            }

            // ✅ Email format validation
            if (!IsValidEmail(Email))
            {
                ViewBag.Error = "Invalid email format.";
                return View();
            }

            // ✅ Password length validation
            if (Password.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters long.";
                return View();
            }

            // ✅ Password match validation
            if (Password != ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            // ✅ Check if email already exists
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == Email);
            if (existingUser != null)
            {
                ViewBag.Error = "Email already registered. Please use a different email.";
                return View();
            }

            string hashedPassword = HashPassword(Password);

            // ✅ First user becomes Admin, rest become User
            // Also ensure we create an Admin if NO admins exist (e.g., users were created but none have Admin role)
            bool isFirstUser = !_context.Users.Any() || !_context.Users.Any(u => u.Role.ToLower() == "admin");
            string userRole = isFirstUser ? "Admin" : "User";

            var newUser = new User
            {
                FullName = FullName,
                Email = Email,
                PasswordHash = hashedPassword,
                Drink = true,
                Lunch = false,
                Role = userRole
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            // ✅ Success message
            TempData["SuccessMessage"] = isFirstUser
                ? "Admin account created successfully! Please login."
                : "Account created successfully! Please login.";

            return RedirectToAction("Login_Page", "Login");
        }

        // ✅ Email validation helper method
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
            // allow letters (unicode), spaces, hyphen and apostrophe
            var regex = new System.Text.RegularExpressions.Regex(@"^[\p{L} .'\-]+$");
            return regex.IsMatch(name);
        }

        // ✅ Password hashing method
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }
    }
}