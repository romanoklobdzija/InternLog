using System;

namespace InternLog.Models;

public class DailyLog
{
    public int Id { get; set; }

    public int InternshipId { get; set; }

    public Internship Internship { get; set; } = null!;


    // Osnovni podaci
    public int DayNumber { get; set; }

    public DateTime Date { get; set; }


    // Radno vrijeme
    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public double TotalHours { get; set; }


    // Sadržaj dnevnika
    public string Description { get; set; } = string.Empty;

    public string Activities { get; set; } = string.Empty;

    public string Learned { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;


    // Kada je zapis napravljen
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


}