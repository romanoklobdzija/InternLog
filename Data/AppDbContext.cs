using InternLog.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace InternLog.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<Employer> Employers { get; set; }

    public DbSet<Internship> Internships { get; set; }

    public DbSet<DailyLog> DailyLogs { get; set; }


    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        string databasePath = Path.Combine(
            Windows.Storage.ApplicationData.Current.LocalFolder.Path,
            "internlog.db");

        optionsBuilder.UseSqlite(
            $"Data Source={databasePath}");
    }


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // User → Internship
        modelBuilder.Entity<Internship>()
            .HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);


        // Employer → Internship
        modelBuilder.Entity<Internship>()
            .HasOne(i => i.Employer)
            .WithMany(e => e.Internships)
            .HasForeignKey(i => i.EmployerId)
            .OnDelete(DeleteBehavior.Cascade);


        // Internship → DailyLog
        modelBuilder.Entity<DailyLog>()
            .HasOne(d => d.Internship)
            .WithMany(i => i.DailyLogs)
            .HasForeignKey(d => d.InternshipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}