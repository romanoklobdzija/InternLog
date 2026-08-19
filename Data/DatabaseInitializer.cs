using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace InternLog.Data;

public static class DatabaseInitializer
{
    public static void Initialize(AppDbContext db)
    {
        db.Database.EnsureCreated();

        using var connection = db.Database.GetDbConnection();
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT COUNT(*)
            FROM pragma_table_info('Internships')
            WHERE name = 'JournalStatus';
        ";

        var result = Convert.ToInt32(command.ExecuteScalar());

        if (result == 0)
        {
            command.CommandText = @"
                ALTER TABLE Internships
                ADD COLUMN JournalStatus TEXT NOT NULL DEFAULT 'NotStarted';
            ";

            command.ExecuteNonQuery();
        }
    }
}