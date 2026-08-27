using System;

namespace InternLog.Models;

public class DailyLog
{
    public int Id { get; set; }
    public int InternshipId { get; set; }
    public Internship Internship { get; set; } = null!;
    public int DayNumber { get; set; }
    public DateTime Date { get; set; } 
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public double TotalHours { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Activities { get; set; } = string.Empty;
    public string Learned { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


}