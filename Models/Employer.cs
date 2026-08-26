using System.Collections.Generic;

using InternLog.Services;

namespace InternLog.Models;

public class Employer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;

    public string Website { get; set; } = string.Empty;

    public string Industry { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int StudentCapacity { get; set; }

    public string StudentTasks { get; set; } = string.Empty;

    public string AvailablePositionsText =>
        string.Format(
            LocalizationService.Get("AvailablePositionsCount"),
            StudentCapacity);

    public ICollection<Internship> Internships { get; set; }
        = new List<Internship>();
}
