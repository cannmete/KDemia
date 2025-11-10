using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KDemia.Models;
namespace KDemia.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { 
        
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
