using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuickTaxi.Models;

namespace QuickTaxi.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
            public DbSet<Driver> Drivers { get; set; }
            public DbSet<Vehicle> Vehicles { get; set; }
            public DbSet<Ride> Rides { get; set; }
            public DbSet<Payment> Payments { get; set; }
            public DbSet<Review> Reviews { get; set; }
            public DbSet<Reward> Rewards { get; set; }
            public DbSet<Settings> Settings { get; set; }
            
        
    }
    
}
