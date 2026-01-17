using Microsoft.EntityFrameworkCore;

namespace Mess_Management_System.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
    }
}
