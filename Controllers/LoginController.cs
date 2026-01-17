using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Mess_Management_System.Models;
using Mess_Management_System.Services;

namespace Mess_Management_System.Controllers
{
    [AllowAnonymous] 
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public LoginController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpGet]
        public IActionResult Login_Page()
        {
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }

            if (Request.Cookies.ContainsKey("AuthToken"))
            {
                var token = Request.Cookies["AuthToken"];
                var role = _jwtService.GetRoleFromToken(token);

                if (role != null && !_jwtService.IsTokenExpired(token))
                {
                    if (role.ToLower() == "admin")
                        return RedirectToAction("Dashboard", "Admin");
                    else
                        return RedirectToAction("Home", "UserMenu");
                }
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login_Page(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and Password are required!";
                return View();
            }

            if (!IsValidEmail(email))
            {
                ViewBag.Error = "Please enter a valid email address!";
                return View();
            }

            string hashedPassword = HashPassword(password);


            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hashedPassword);

            if (user != null)
            {
                var token = _jwtService.GenerateToken(user.Id, user.FullName, user.Email, user.Role);

                Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true, 
                    Secure = false, 
                    SameSite = SameSiteMode.Strict, 
                    Expires = DateTimeOffset.UtcNow.AddHours(24) 
                });

   
                Console.WriteLine($"User logged in: {user.Email} - Role: {user.Role}");

           
                if (user.Role.ToLower() == "admin")
                {
                    TempData["WelcomeMessage"] = $"Hi, {user.FullName}";
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    TempData["WelcomeMessage"] = $"Hi, {user.FullName}";
                    return RedirectToAction("Home", "UserMenu");
                }
            }

            ViewBag.Error = "Invalid Email or Password!";
            return View();
        }


        public IActionResult Logout()
        {

            Response.Cookies.Delete("AuthToken");


            TempData["LogoutMessage"] = "You have been logged out successfully!";

            return RedirectToAction("Login_Page");
        }


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


        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }
    }
}

