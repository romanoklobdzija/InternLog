using InternLog.Data;
using InternLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Kernel.Sketches;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternLog.Pages;

public sealed partial class DashboardPage : Page
{
    public ISeries[] InternshipLogSeries { get; private set; } = Array.Empty<ISeries>();
    public IEnumerable<ICartesianAxis> InternshipLogAxes { get; private set; } = Array.Empty<Axis>();
    public ISeries[] JournalStatusSeries { get; private set; } = Array.Empty<ISeries>();
    public ISeries[] HoursTrendSeries { get; private set; } = Array.Empty<ISeries>();
    public IEnumerable<ICartesianAxis> HoursTrendAxes { get; private set; } = Array.Empty<Axis>();

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

        HoursTrendTitleText.Text =
            LocalizationService.Get("HoursTrend");

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
                i.Status == "Approved")
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

        var internshipLabels = internships.Select(i => i.Employer?.Name ?? "").ToArray();
        InternshipLogSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = LocalizationService.Get("DailyLogs"),
                Values = internships.Select(i => i.DailyLogs.Count).ToArray(),
                Fill = new SolidColorPaint(SKColor.Parse("#7C3AED")),
                Stroke = null,
                Rx = 7,
                Ry = 7
            }
        };
        InternshipLogAxes = new[]
        {
            new Axis { Labels = internshipLabels, LabelsRotation = 12, TextSize = 12 }
        };

        var journalGroups = internships.GroupBy(i => i.JournalStatus).ToList();
        var chartColors = new[] { "#7C3AED", "#C084FC", "#F59E0B", "#14B8A6" };
        JournalStatusSeries = journalGroups.Select((group, index) => (ISeries)new PieSeries<int>
        {
            Name = LocalizationService.GetStatus(group.Key),
            Values = new[] { group.Count() },
            Fill = new SolidColorPaint(SKColor.Parse(chartColors[index % chartColors.Length])),
            Stroke = null,
            DataLabelsSize = 13
        }).ToArray();

        var hoursByDate = internships
            .SelectMany(i => i.DailyLogs)
            .GroupBy(log => log.Date.Date)
            .OrderBy(group => group.Key)
            .ToList();
        HoursTrendSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Name = LocalizationService.Get("HoursWorked"),
                Values = hoursByDate.Select(group => group.Sum(log => log.TotalHours)).ToArray(),
                Fill = null,
                Stroke = new SolidColorPaint(SKColor.Parse("#14B8A6"), 3),
                GeometryFill = new SolidColorPaint(SKColor.Parse("#FFFCF7")),
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#14B8A6"), 3),
                GeometrySize = 9
            }
        };
        HoursTrendAxes = new[]
        {
            new Axis { Labels = hoursByDate.Select(group => group.Key.ToString("dd.MM")).ToArray(), TextSize = 12 }
        };

        Bindings.Update();

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
