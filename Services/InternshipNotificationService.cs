using System;
using System.Linq;
using System.Threading.Tasks;
using InternLog.Data;
using Microsoft.EntityFrameworkCore;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace InternLog.Services;

/// <summary>
/// Maintains the Windows reminders associated with the currently signed-in
/// student's internships. Scheduled notifications are removed and rebuilt so
/// changing a reservation or the setting never leaves an outdated reminder.
/// </summary>
public static class InternshipNotificationService
{
    private const string NotificationsSetting = "Notifications";

    public static bool NotificationsEnabled =>
        ApplicationData.Current.LocalSettings.Values[NotificationsSetting] as bool? ?? true;

    public static async Task RefreshForCurrentUserAsync()
    {
        await CompleteExpiredInternshipsAsync();

        ClearScheduledNotifications();

        if (!NotificationsEnabled || SessionService.CurrentUser == null)
            return;

        using var db = new AppDbContext();
        var internships = await db.Internships
            .Include(i => i.Employer)
            .Where(i => i.UserId == SessionService.CurrentUser.Id && i.Status == "Approved")
            .ToListAsync();

        foreach (var internship in internships)
        {
            string employerName = internship.Employer?.Name ?? LocalizationService.Get("Internship");
            ScheduleReminder(
                internship.StartDate.Date.AddDays(-1),
                LocalizationService.Get("InternshipStartsTomorrowTitle"),
                string.Format(LocalizationService.Get("InternshipStartsTomorrowMessage"), employerName));

            if (internship.EndDate.HasValue)
            {
                ScheduleReminder(
                    internship.EndDate.Value.Date.AddDays(-1),
                    LocalizationService.Get("InternshipEndsTomorrowTitle"),
                    string.Format(LocalizationService.Get("InternshipEndsTomorrowMessage"), employerName));
            }
        }
    }

    public static void ClearScheduledNotifications()
    {
        try
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            foreach (var notification in notifier.GetScheduledToastNotifications().ToList())
                notifier.RemoveFromSchedule(notification);
        }
        catch
        {
            // Notification availability must never prevent the application from working.
        }
    }

    public static void ShowTestNotification()
    {
        try
        {
            var xml = new XmlDocument();
            xml.LoadXml($"<toast><visual><binding template=\"ToastGeneric\"><text>{Escape(LocalizationService.Get("TestNotificationTitle"))}</text><text>{Escape(LocalizationService.Get("TestNotificationMessage"))}</text></binding></visual></toast>");
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(xml));
        }
        catch
        {
            // The UI remains usable if Windows notifications are blocked.
        }
    }

    private static async Task CompleteExpiredInternshipsAsync()
    {
        using var db = new AppDbContext();
        var expired = await db.Internships
            .Include(i => i.Employer)
            .Where(i => i.Status == "Approved" && i.EndDate.HasValue && i.EndDate.Value.Date < DateTime.Today)
            .ToListAsync();

        foreach (var internship in expired)
        {
            internship.Status = "Completed";
            if (internship.Employer != null)
                internship.Employer.StudentCapacity++;
        }

        if (expired.Count > 0)
            await db.SaveChangesAsync();
    }

    private static void ScheduleReminder(DateTime date, string title, string message)
    {
        DateTime deliveryTime = date.AddHours(9);
        if (deliveryTime <= DateTime.Now)
            return;

        try
        {
            var xml = new XmlDocument();
            xml.LoadXml($"<toast><visual><binding template=\"ToastGeneric\"><text>{Escape(title)}</text><text>{Escape(message)}</text></binding></visual></toast>");

            var notification = new ScheduledToastNotification(xml, deliveryTime);
            ToastNotificationManager.CreateToastNotifier().AddToSchedule(notification);
        }
        catch
        {
            // The app can still be used on systems where Windows notifications are unavailable.
        }
    }

    private static string Escape(string value) => System.Security.SecurityElement.Escape(value) ?? string.Empty;
}
