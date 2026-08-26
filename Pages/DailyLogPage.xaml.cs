using InternLog.Data;
using InternLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InternLog.Pages;

public sealed partial class DailyLogPage : Page
{
    private int? _selectedInternshipId;

    public DailyLogPage()
    {
        InitializeComponent();
        Loaded += DailyLogPage_Loaded;
        LocalizationService.LanguageChanged += ApplyLanguage;
    }

    private async void DailyLogPage_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyLanguage();
        await LoadInternships();
    }

    private void ApplyLanguage()
    {
        DailyLogTitleText.Text = LocalizationService.Get("DailyLog");
        DailyLogDescriptionText.Text = LocalizationService.Get("DailyLogDescription");
        InternshipLabelText.Text = LocalizationService.Get("Internship");
        DateLabelText.Text = LocalizationService.Get("Date");
        HoursLabelText.Text = LocalizationService.Get("HoursWorked");
        DescriptionLabelText.Text = LocalizationService.Get("Description");
        SaveLogButton.Content = LocalizationService.Get("SaveLog");
        ExistingLogsTitleText.Text = LocalizationService.Get("ExistingLogs");
        InternshipComboBox.PlaceholderText = LocalizationService.Get("SelectInternship");
        DescriptionTextBox.PlaceholderText = LocalizationService.Get("WorkDescriptionPlaceholder");
    }

    private async Task LoadInternships()
    {
        if (SessionService.CurrentUser == null)
            return;

        using var db = new AppDbContext();

        var internships = await db.Internships
            .Include(i => i.Employer)
            .Where(i =>
                i.UserId == SessionService.CurrentUser.Id &&
                i.Status != "Cancelled")
            .OrderByDescending(i => i.StartDate)
            .ToListAsync();

        InternshipComboBox.ItemsSource = internships;
        InternshipComboBox.DisplayMemberPath = "Employer.Name";

        if (_selectedInternshipId.HasValue)
        {
            var selected = internships.FirstOrDefault(
                i => i.Id == _selectedInternshipId.Value);

            InternshipComboBox.SelectedItem = selected;
        }
        else if (internships.Count > 0)
        {
            InternshipComboBox.SelectedIndex = 0;
        }

        await LoadLogs();
    }

    private async void InternshipComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (InternshipComboBox.SelectedItem is Models.Internship internship)
        {
            _selectedInternshipId = internship.Id;
            await LoadLogs();
        }
    }

    private async Task LoadLogs()
    {
        if (!_selectedInternshipId.HasValue)
        {
            DailyLogsItemsControl.ItemsSource = null;
            return;
        }

        using var db = new AppDbContext();

        var logs = await db.DailyLogs
            .Where(l => l.InternshipId == _selectedInternshipId.Value)
            .OrderByDescending(l => l.Date)
            .ToListAsync();

        DailyLogsItemsControl.ItemsSource = logs;
    }

    private async void SaveLogButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (SessionService.CurrentUser == null)
            return;

        if (InternshipComboBox.SelectedItem is not Models.Internship internship)
        {
            await ShowMessage(
                LocalizationService.Get("SelectInternship"));

            return;
        }

        if (HoursNumberBox.Value <= 0)
        {
            await ShowMessage(
                LocalizationService.Get("EnterHours"));

            return;
        }

        if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
        {
            await ShowMessage(
                LocalizationService.Get("EnterDescription"));

            return;
        }

        using var db = new AppDbContext();

        var log = new Models.DailyLog
        {
            InternshipId = internship.Id,
            Date = DatePicker.Date.DateTime.Date,
            TotalHours = HoursNumberBox.Value,
            Description = DescriptionTextBox.Text.Trim()
        };

        db.DailyLogs.Add(log);

        await db.SaveChangesAsync();

        DescriptionTextBox.Text = "";
        HoursNumberBox.Value = 0;
        DatePicker.Date = DateTimeOffset.Now;

        await LoadLogs();

        await ShowMessage(
            LocalizationService.Get("LogSaved"));
    }

    private async Task ShowMessage(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "InternLog",
            Content = message,
            CloseButtonText = LocalizationService.Get("OK"),
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }
}
