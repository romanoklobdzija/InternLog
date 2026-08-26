using InternLog.Data;
using InternLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using System.Threading.Tasks;

namespace InternLog.Pages;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
        Loaded += DashboardPage_Loaded;
        LocalizationService.LanguageChanged += ApplyLanguage;
    }

    private async void DashboardPage_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        ApplyLanguage();
        await LoadDashboard();
    }

    private void ApplyLanguage()
    {
        DashboardTitleText.Text =
            LocalizationService.Get("Dashboard");

        DashboardDescriptionText.Text =
            LocalizationService.Get("DashboardDescription");

        ActiveInternshipsTitleText.Text =
            LocalizationService.Get("ActiveInternships");

        DailyLogsTitleText.Text =
            LocalizationService.Get("DailyLogs");

        PendingReviewTitleText.Text =
            LocalizationService.Get("PendingReview");

        HoursWorkedTitleText.Text =
            LocalizationService.Get("HoursWorked");

        LogsPerInternshipTitleText.Text =
            LocalizationService.Get("LogsPerInternship");

        JournalStatusTitleText.Text =
            LocalizationService.Get("JournalStatus");

        RecentActivityTitleText.Text =
            LocalizationService.Get("RecentActivity");
    }

    private async Task LoadDashboard()
    {
        if (SessionService.CurrentUser == null)
            return;

        using var db = new AppDbContext();

        var internships = await db.Internships
            .Include(i => i.Employer)
            .Include(i => i.DailyLogs)
            .Where(i =>
                i.UserId == SessionService.CurrentUser.Id &&
                i.Status != "Cancelled")
            .ToListAsync();

        ActiveInternshipsText.Text =
            internships.Count.ToString();

        DailyLogsText.Text =
            internships.Sum(i => i.DailyLogs.Count).ToString();

        PendingReviewText.Text =
            internships.Count(i =>
                i.JournalStatus == "Pending").ToString();

        HoursWorkedText.Text =
            internships
                .SelectMany(i => i.DailyLogs)
                .Sum(l => l.TotalHours)
                .ToString("0.#");

        LogsPerInternshipItemsControl.ItemsSource =
            internships.Select(i => new
            {
                EmployerName = i.Employer?.Name ?? "",
                LogCount = i.DailyLogs.Count
            }).ToList();

        JournalStatusItemsControl.ItemsSource =
            internships
                .GroupBy(i => i.JournalStatus)
                .Select(g => new
                {
                    Status = LocalizationService.GetStatus(g.Key),
                    Count = g.Count()
                })
                .ToList();

        RecentActivityItemsControl.ItemsSource =
            internships
                .SelectMany(i => i.DailyLogs.Select(l => new
                {
                    Title = i.Employer?.Name ?? "",
                    Description = l.Description,
                    Date = l.Date
                }))
                .OrderByDescending(x => x.Date)
                .Take(5)
                .Select(x => new
                {
                    x.Title,
                    x.Description,
                    Date = x.Date.ToString("dd.MM.yyyy")
                })
                .ToList();
    }
}
