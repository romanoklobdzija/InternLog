using System;
using System.Collections.Generic;

namespace InternLog.Models;

public class Internship
{
    public int Id { get; set; }

    // Student koji je rezervirao praksu
    public int UserId { get; set; }

    public User User { get; set; } = null!;


    // Poslodavac kod kojeg se praksa obavlja
    public int EmployerId { get; set; }

    public Employer Employer { get; set; } = null!;


    // Podaci o praksi
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string Status { get; set; } = "Approved";


    // Dnevni zapisi
    public ICollection<DailyLog> DailyLogs { get; set; }
        = new List<DailyLog>();
}