using InternLog.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Collections.Generic;
using Windows.UI;

namespace InternLog.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        string databasePath = Path.Combine(
            Windows.Storage.ApplicationData.Current.LocalFolder.Path,
            "internlog.db");

        optionsBuilder.UseSqlite($"Data Source={databasePath}");
    }
}