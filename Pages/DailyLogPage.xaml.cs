using System;
using System.Linq;
using System.Threading.Tasks;
using InternLog.Data;
using InternLog.Models;
using InternLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace InternLog.Pages;

public sealed partial class DailyLogPage : Page
{
    private int? _selectedInternshipId;
    private int? _editingLogId;
    private Internship? _selectedInternship;

    public DailyLogPage()
    {
        InitializeComponent();
        Loaded += DailyLogPage_Loaded;
        LocalizationService.LanguageChanged += ApplyLanguage;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is int internshipId)
            _selectedInternshipId = internshipId;
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
        ExistingLogsTitleText.Text = LocalizationService.Get("ExistingLogs");
        InternshipComboBox.PlaceholderText = LocalizationService.Get("SelectInternship");
        DescriptionTextBox.PlaceholderText = LocalizationService.Get("WorkDescriptionPlaceholder");
        SaveLogButton.Content = _editingLogId.HasValue ? LocalizationService.Get("SaveChanges") : LocalizationService.Get("SaveLog");
    }

    private async Task LoadInternships()
    {
        if (SessionService.CurrentUser == null) return;
        using var db = new AppDbContext();
        var internships = await db.Internships.Include(i => i.Employer)
            .Where(i => i.UserId == SessionService.CurrentUser.Id && i.Status == "Approved")
            .OrderByDescending(i => i.StartDate).ToListAsync();
        InternshipComboBox.ItemsSource = internships;
        InternshipComboBox.DisplayMemberPath = "Employer.Name";
        if (_selectedInternshipId.HasValue)
            InternshipComboBox.SelectedItem = internships.FirstOrDefault(i => i.Id == _selectedInternshipId.Value);
        else if (internships.Count > 0)
            InternshipComboBox.SelectedIndex = 0;
        await LoadLogs();
    }

    private async void InternshipComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (InternshipComboBox.SelectedItem is Internship internship)
        {
            _selectedInternshipId = internship.Id;
            _editingLogId = null;
            await LoadLogs();
        }
    }

    private async Task LoadLogs()
    {
        if (!_selectedInternshipId.HasValue) { DailyLogsItemsControl.ItemsSource = null; return; }
        using var db = new AppDbContext();
        _selectedInternship = await db.Internships.Include(i => i.DailyLogs)
            .FirstOrDefaultAsync(i => i.Id == _selectedInternshipId.Value);
        if (_selectedInternship == null) return;
        var logs = _selectedInternship.DailyLogs.OrderBy(log => log.Date).ToList();
        DailyLogsItemsControl.ItemsSource = logs.Select((log, index) => new
        {
            log.Id,
            Day = string.Format(LocalizationService.Get("DayAndDate"), log.DayNumber > 0 ? log.DayNumber : index + 1, log.Date.ToString("dd.MM.yyyy")),
            Hours = string.Format(LocalizationService.Get("Hours"), log.TotalHours),
            log.Description
        }).ToList();
        if (!_editingLogId.HasValue) PrepareNewLog(logs.Count);
    }

    private void PrepareNewLog(int completedDays)
    {
        if (_selectedInternship == null) return;
        var nextDate = _selectedInternship.StartDate.Date.AddDays(completedDays);
        var finalDate = _selectedInternship.EndDate?.Date ?? DateTime.Today;
        bool canCreate = nextDate <= DateTime.Today && nextDate <= finalDate;
        DateValueText.Text = canCreate
            ? string.Format(LocalizationService.Get("DayAndDate"), completedDays + 1, nextDate.ToString("dd.MM.yyyy"))
            : LocalizationService.Get("NoNewLogAvailable");
        SaveLogButton.IsEnabled = canCreate;
        HoursNumberBox.Value = 0;
        DescriptionTextBox.Text = string.Empty;
        _editingLogId = null;
        ApplyLanguage();
    }

    private async void SaveLogButton_Click(object sender, RoutedEventArgs e)
    {
        if (SessionService.CurrentUser == null || !_selectedInternshipId.HasValue) return;
        if (HoursNumberBox.Value <= 0) { await ShowMessage(LocalizationService.Get("EnterHours")); return; }
        if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text)) { await ShowMessage(LocalizationService.Get("EnterDescription")); return; }
        using var db = new AppDbContext();
        var internship = await db.Internships.Include(i => i.DailyLogs)
            .FirstOrDefaultAsync(i => i.Id == _selectedInternshipId.Value && i.Status == "Approved");
        if (internship == null) return;
        if (_editingLogId.HasValue)
        {
            var log = internship.DailyLogs.FirstOrDefault(item => item.Id == _editingLogId.Value);
            if (log == null) return;
            log.TotalHours = HoursNumberBox.Value;
            log.Description = DescriptionTextBox.Text.Trim();
        }
        else
        {
            int nextDay = internship.DailyLogs.Count + 1;
            DateTime nextDate = internship.StartDate.Date.AddDays(nextDay - 1);
            DateTime finalDate = internship.EndDate?.Date ?? DateTime.Today;
            if (nextDate > DateTime.Today || nextDate > finalDate)
            {
                await ShowMessage(LocalizationService.Get("NoNewLogAvailable"));
                return;
            }
            db.DailyLogs.Add(new DailyLog
            {
                InternshipId = internship.Id, DayNumber = nextDay, Date = nextDate,
                TotalHours = HoursNumberBox.Value, Description = DescriptionTextBox.Text.Trim()
            });
        }
        await db.SaveChangesAsync();
        _editingLogId = null;
        await LoadLogs();
        await ShowMessage(LocalizationService.Get("LogSaved"));
    }

    private async void EditLogButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int logId } || !_selectedInternshipId.HasValue) return;
        using var db = new AppDbContext();
        var log = await db.DailyLogs.FirstOrDefaultAsync(item => item.Id == logId && item.InternshipId == _selectedInternshipId.Value);
        if (log == null) return;
        _editingLogId = log.Id;
        int displayDay = log.DayNumber > 0 ? log.DayNumber : 1;
        DateValueText.Text = string.Format(LocalizationService.Get("DayAndDate"), displayDay, log.Date.ToString("dd.MM.yyyy"));
        HoursNumberBox.Value = log.TotalHours;
        DescriptionTextBox.Text = log.Description;
        SaveLogButton.IsEnabled = true;
        ApplyLanguage();
    }

    private void EditLogButton_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Button button) button.Content = LocalizationService.Get("Edit");
    }

    private async Task ShowMessage(string message)
    {
        var dialog = new ContentDialog { Title = "InternLog", Content = message, CloseButtonText = LocalizationService.Get("OK"), XamlRoot = XamlRoot };
        await dialog.ShowAsync();
    }
}
