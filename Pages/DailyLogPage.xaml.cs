using InternLog.Data;
using InternLog.Models;
using InternLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InternLog.Pages;

public sealed partial class DailyLogPage : Page
{
    private Internship? _selectedInternship;
    private List<Internship> _internships = new();
    private int? _editingLogId = null;
    private Button? _selectedInternshipCard;

    public DailyLogPage()
    {
        InitializeComponent();
        Loaded += DailyLogPage_Loaded;
    }

    private async void DailyLogPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadInternships();
    }

    private async System.Threading.Tasks.Task LoadInternships()
    {
        if (SessionService.CurrentUser == null)
            return;

        using var db = new AppDbContext();

        _internships = await db.Internships
            .Include(i => i.Employer)
            .Where(i => i.UserId == SessionService.CurrentUser.Id)
            .ToListAsync();

        CreateInternshipCards();

        if (_internships.Count > 0)
            await SelectInternship(_internships[0]);
        else
        {
            SelectedInternshipText.Text = "No internship selected";
            DayNumberText.Text = "No internship";
            SaveLogButton.IsEnabled = false;
        }
    }

    // ============================================================
    // CREATE INTERNSHIP CARDS
    // ============================================================

    private void CreateInternshipCards()
    {
        InternshipsGrid.Children.Clear();
        _selectedInternshipCard = null;

        for (int i = 0; i < _internships.Count && i < 3; i++)
        {
            var card = CreateInternshipCard(_internships[i]);

            Grid.SetColumn(card, i);
            InternshipsGrid.Children.Add(card);
        }
    }

    private Button CreateInternshipCard(Internship internship)
    {
        string employerName = internship.Employer?.Name ?? "Unknown employer";
        string location = internship.Employer?.Location ?? "Unknown location";

        var button = new Button
        {
            Tag = internship,
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
            BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 14, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch
        };

        var panel = new StackPanel
        {
            Spacing = 6
        };

        var nameText = new TextBlock
        {
            Text = employerName,
            FontSize = 18,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black)
        };

        var locationText = new TextBlock
        {
            Text = location,
            FontSize = 13,
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
        };

        var statusText = new TextBlock
        {
            Text = internship.Status,
            FontSize = 12,
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkGreen),
            Margin = new Thickness(0, 6, 0, 0)
        };

        panel.Children.Add(nameText);
        panel.Children.Add(locationText);
        panel.Children.Add(statusText);

        button.Content = panel;
        button.Click += InternshipCard_Click;

        return button;
    }

    // ============================================================
    // INTERNSHIP CARD CLICK
    // ============================================================

    private async void InternshipCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Internship internship)
        {
            SelectInternshipCard(button);
            await SelectInternship(internship);
        }
    }

    // ============================================================
    // SELECTED CARD VISUAL
    // ============================================================

    private void SelectInternshipCard(Button selectedButton)
    {
        if (_selectedInternshipCard != null)
        {
            _selectedInternshipCard.BorderThickness = new Thickness(1);
            _selectedInternshipCard.BorderBrush =
                new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Transparent);
        }

        selectedButton.BorderThickness = new Thickness(2);
        selectedButton.BorderBrush =
            new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.DodgerBlue);

        _selectedInternshipCard = selectedButton;
    }

    // ============================================================
    // SELECT INTERNSHIP
    // ============================================================

    private async System.Threading.Tasks.Task SelectInternship(Internship internship)
    {
        _selectedInternship = internship;
        _editingLogId = null;

        string employerName = internship.Employer?.Name ?? "Unknown employer";

        SelectedInternshipText.Text = employerName;
        DayNumberText.Text = await GetNextDayText(internship);
        DateText.Text = DateTime.Today.ToString("MMMM dd, yyyy");

        SaveLogButton.Content = "Save daily log";
        SaveLogButton.IsEnabled = true;

        ClearForm();

        await LoadDailyLogs();
    }

    // ============================================================
    // GET NEXT DAY NUMBER
    // ============================================================

    private async System.Threading.Tasks.Task<string> GetNextDayText(Internship internship)
    {
        using var db = new AppDbContext();

        int lastDay = await db.DailyLogs
            .Where(d => d.InternshipId == internship.Id)
            .Select(d => (int?)d.DayNumber)
            .MaxAsync() ?? 0;

        return $"Day {lastDay + 1}";
    }

    // ============================================================
    // LOAD DAILY LOGS
    // ============================================================

    private async System.Threading.Tasks.Task LoadDailyLogs()
    {
        if (_selectedInternship == null)
            return;

        using var db = new AppDbContext();

        var logs = await db.DailyLogs
            .Where(d => d.InternshipId == _selectedInternship.Id)
            .OrderByDescending(d => d.DayNumber)
            .ToListAsync();

        DailyLogsItemsControl.Items.Clear();

        foreach (var log in logs)
        {
            var button = CreateDailyLogButton(log);
            DailyLogsItemsControl.Items.Add(button);
        }
    }

    // ============================================================
    // CREATE DAILY LOG BUTTON
    // ============================================================

    private Button CreateDailyLogButton(DailyLog log)
    {
        var button = new Button
        {
            Tag = log,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(18),
            Margin = new Thickness(0, 0, 0, 10),
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
            BorderThickness = new Thickness(1)
        };

        var panel = new StackPanel
        {
            Spacing = 5
        };

        var headerText = new TextBlock
        {
            Text = $"Day {log.DayNumber} • {log.Date:dd.MM.yyyy}",
            FontSize = 17,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black)
        };

        var descriptionText = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(log.Description) ? "No description" : log.Description,
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkGray)
        };

        var hoursText = new TextBlock
        {
            Text = $"{log.TotalHours:0.##} hours",
            FontSize = 13,
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
        };

        var editText = new TextBlock
        {
            Text = "Click to edit",
            FontSize = 12,
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DodgerBlue),
            Margin = new Thickness(0, 4, 0, 0)
        };

        panel.Children.Add(headerText);
        panel.Children.Add(descriptionText);
        panel.Children.Add(hoursText);
        panel.Children.Add(editText);

        button.Content = panel;
        button.Click += ExistingLog_Click;

        return button;
    }

    // ============================================================
    // EXISTING LOG CLICK
    // ============================================================

    private void ExistingLog_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is DailyLog log)
            LoadLogIntoForm(log);
    }

    // ============================================================
    // LOAD EXISTING LOG INTO FORM
    // ============================================================

    private void LoadLogIntoForm(DailyLog log)
    {
        _editingLogId = log.Id;

        DayNumberText.Text = $"Day {log.DayNumber}";
        DateText.Text = log.Date.ToString("MMMM dd, yyyy");

        DescriptionTextBox.Text = log.Description;
        TasksTextBox.Text = log.Activities;
        LearningTextBox.Text = log.Learned;
        NotesTextBox.Text = log.Notes;

        StartTimePicker.Time = log.StartTime;
        EndTimePicker.Time = log.EndTime;

        SaveLogButton.Content = "Update daily log";
        SaveLogButton.IsEnabled = true;

        ScrollToForm();
    }

    // ============================================================
    // SAVE / UPDATE BUTTON
    // ============================================================

    private async void SaveLogButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedInternship == null || SessionService.CurrentUser == null)
            return;

        using var db = new AppDbContext();

        if (_editingLogId.HasValue)
        {
            var existingLog = await db.DailyLogs
                .FirstOrDefaultAsync(d => d.Id == _editingLogId.Value);

            if (existingLog == null)
                return;

            TimeSpan startTime = StartTimePicker.Time;
            TimeSpan endTime = EndTimePicker.Time;
            double totalHours = (endTime - startTime).TotalHours;

            if (totalHours < 0)
                totalHours = 0;

            existingLog.StartTime = startTime;
            existingLog.EndTime = endTime;
            existingLog.TotalHours = totalHours;
            existingLog.Description = DescriptionTextBox.Text;
            existingLog.Activities = TasksTextBox.Text;
            existingLog.Learned = LearningTextBox.Text;
            existingLog.Notes = NotesTextBox.Text;

            await db.SaveChangesAsync();

            _editingLogId = null;
            SaveLogButton.Content = "Save daily log";

            ClearForm();

            await LoadDailyLogs();

            DayNumberText.Text = await GetNextDayText(_selectedInternship);
            DateText.Text = DateTime.Today.ToString("MMMM dd, yyyy");

            return;
        }

        int nextDayNumber = await db.DailyLogs
            .Where(d => d.InternshipId == _selectedInternship.Id)
            .Select(d => (int?)d.DayNumber)
            .MaxAsync() ?? 0;

        nextDayNumber++;

        TimeSpan newStartTime = StartTimePicker.Time;
        TimeSpan newEndTime = EndTimePicker.Time;
        double newTotalHours = (newEndTime - newStartTime).TotalHours;

        if (newTotalHours < 0)
            newTotalHours = 0;

        var newLog = new DailyLog
        {
            InternshipId = _selectedInternship.Id,
            DayNumber = nextDayNumber,
            Date = DateTime.Today,
            StartTime = newStartTime,
            EndTime = newEndTime,
            TotalHours = newTotalHours,
            Description = DescriptionTextBox.Text,
            Activities = TasksTextBox.Text,
            Learned = LearningTextBox.Text,
            Notes = NotesTextBox.Text,
            CreatedAt = DateTime.UtcNow
        };

        db.DailyLogs.Add(newLog);

        await db.SaveChangesAsync();

        ClearForm();

        SaveLogButton.Content = "Save daily log";

        DayNumberText.Text = await GetNextDayText(_selectedInternship);
        DateText.Text = DateTime.Today.ToString("MMMM dd, yyyy");

        await LoadDailyLogs();
    }

    // ============================================================
    // CLEAR FORM
    // ============================================================

    private void ClearForm()
    {
        DescriptionTextBox.Text = string.Empty;
        TasksTextBox.Text = string.Empty;
        LearningTextBox.Text = string.Empty;
        NotesTextBox.Text = string.Empty;

        StartTimePicker.Time = new TimeSpan(8, 0, 0);
        EndTimePicker.Time = new TimeSpan(16, 0, 0);
    }

    // ============================================================
    // SCROLL TO FORM
    // ============================================================

    private void ScrollToForm()
    {
        MainScrollViewer.ChangeView(null, 0, null);
    }
}