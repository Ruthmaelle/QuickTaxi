using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuickTaxi.Models;

namespace QuickTaxi.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<User> Users { get; set; }  // ✅ Explicitly add Users table
        public DbSet<Vehicle> Vehicles { get; set; }
            public DbSet<Ride> Rides { get; set; }
            public DbSet<Rate> Rates { get; set; }
            public DbSet<Payment> Payments { get; set; }
            public DbSet<Notification> Notifications { get; set; }
            public DbSet<BiometricAuthentication> BiometricAuthentications { get; set; }
            public DbSet<Review> Reviews { get; set; }
            public DbSet<Reward> Rewards { get; set; }
            public DbSet<Setting> Settings { get; set; }
            public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("AspNetUsers"); // ✅ S'assurer que User est bien mappé sur AspNetUsers
                entity.HasKey(u => u.Id); // ✅ Définir l'ID comme clé primaire
            });

            // Drivers
            modelBuilder.Entity<Driver>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Driver>()
           .HasOne(d => d.Vehicle)
           .WithOne(v => v.Driver)
           .HasForeignKey<Vehicle>(v => v.DriverId)
           .OnDelete(DeleteBehavior.Cascade);

            // Vehicles
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Driver)
                .WithOne(d => d.Vehicle)
                .HasForeignKey<Vehicle>(v => v.DriverId)
                .OnDelete(DeleteBehavior.Cascade);

            // Rides
            modelBuilder.Entity<Ride>()
                .HasOne(r => r.Passenger)
                .WithMany()
                .HasForeignKey(r => r.PassengerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.Driver)
                .WithMany()
                .HasForeignKey(r => r.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.RateApplied)
                .WithMany()
                .HasForeignKey(r => r.RateId);

            modelBuilder.Entity<Ride>()
                .Property(r => r.RideId)
                .HasColumnType("char(36)");

            modelBuilder.Entity<Ride>()
                .Property(r => r.RateId)
                .HasColumnType("char(36)");

            modelBuilder.Entity<Rate>()
                .Property(r => r.IdTarification)
                .HasColumnType("char(36)");

            // Payments
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Ride)
                .WithOne(r => r.Payment)
                .HasForeignKey<Payment>(p => p.RideId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notifications
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Biometric Authentications
            modelBuilder.Entity<BiometricAuthentication>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Reviews
            modelBuilder.Entity<Review>()
                .HasOne(e => e.Ride)
                .WithMany()
                .HasForeignKey(e => e.RideId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Ride)
                .WithMany(r => r.Reviews)
                .HasForeignKey(r => r.RideId)
                .OnDelete(DeleteBehavior.Cascade);


            // Rewards
            modelBuilder.Entity<Reward>()
                .HasOne(r => r.User)
                .WithMany(u => u.Rewards)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reward>()
                .HasOne(r => r.Ride)
                .WithOne(r => r.Reward)
                .HasForeignKey<Reward>(r => r.RideId)
                .OnDelete(DeleteBehavior.Cascade);

            // Settings
            modelBuilder.Entity<Setting>()
                .HasOne(s => s.User)
                .WithOne(u => u.Settings)
                .HasForeignKey<Setting>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }


    }
    
}
