using InternLog.Data;
using InternLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;

namespace InternLog.Pages;

public sealed partial class ViewInternshipPage : Page
{
    private int _internshipId;
    private Models.Internship? _internship;

    public ViewInternshipPage()
    {
        InitializeComponent();
        LocalizationService.LanguageChanged += ApplyLanguage;
    }

    protected override async void OnNavigatedTo(
        Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        ApplyLanguage();

        if (e.Parameter is int internshipId)
        {
            _internshipId = internshipId;
            await LoadInternship();
        }
    }

    private void ApplyLanguage()
    {
        BackButtonText.Text = LocalizationService.Get("BackToInternships");

        InternshipDetailsTitleText.Text =
            LocalizationService.Get("InternshipDetails");

        InternshipInformationTitleText.Text =
            LocalizationService.Get("InternshipInformation");

        EmployerLabelText.Text =
            LocalizationService.Get("Employer");

        LocationLabelText.Text =
            LocalizationService.Get("Location");

        StartDateLabelText.Text =
            LocalizationService.Get("StartDate");

        EndDateLabelText.Text =
            LocalizationService.Get("EndDate");

        InternshipStatusTitleText.Text =
            LocalizationService.Get("InternshipStatus");

        InternshipStatusDescriptionText.Text =
            LocalizationService.Get("InternshipStatusDescription");

        DailyJournalTitleText.Text =
            LocalizationService.Get("DailyJournal");

        OpenDailyLogButton.Content =
            LocalizationService.Get("OpenDailyLog");

        SubmitJournalButton.Content =
            LocalizationService.Get("SubmitJournal");

        CancelInternshipButton.Content =
            LocalizationService.Get("CancelInternship");
    }

    private async Task LoadInternship()
    {
        using var db = new AppDbContext();

        _internship = await db.Internships
            .Include(i => i.Employer)
            .Include(i => i.DailyLogs)
            .FirstOrDefaultAsync(i => i.Id == _internshipId);

        if (_internship == null)
            return;

        EmployerNameText.Text =
            _internship.Employer?.Name ??
            LocalizationService.Get("UnknownEmployer");

        EmployerText.Text =
            _internship.Employer?.Name ??
            LocalizationService.Get("UnknownEmployer");

        LocationText.Text =
            _internship.Employer?.Location ??
            LocalizationService.Get("UnknownLocation");

        StartDateText.Text =
            _internship.StartDate.ToString("dd.MM.yyyy");

        EndDateText.Text =
            _internship.EndDate.HasValue
                ? _internship.EndDate.Value.ToString("dd.MM.yyyy")
                : LocalizationService.Get("NotSpecified");

        StatusText.Text = _internship.DisplayStatus;

        JournalStatusText.Text =
            string.Format(
                LocalizationService.Get("JournalStatusWithLogs"),
                _internship.DisplayJournalStatus,
                _internship.DailyLogs.Count);

        if (_internship.Status != "Approved")
        {
            SubmitJournalButton.IsEnabled = false;
            CancelInternshipButton.IsEnabled = false;
            OpenDailyLogButton.IsEnabled = false;
        }
        else
        {
            CancelInternshipButton.IsEnabled = true;
            OpenDailyLogButton.IsEnabled = true;

            if (_internship.JournalStatus == "Pending" ||
                _internship.JournalStatus == "Approved")
            {
                SubmitJournalButton.IsEnabled = true;
                SubmitJournalButton.Content =
                    LocalizationService.Get("JournalSubmitted");
            }
            else
            {
                SubmitJournalButton.IsEnabled = true;
                SubmitJournalButton.Content =
                    LocalizationService.Get("SubmitJournal");
            }
        }
    }

    private void OpenDailyLogButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_internship == null ||
            _internship.Status != "Approved")
            return;

        Frame.Navigate(
            typeof(DailyLogPage),
            _internshipId);
    }

    private async void SubmitJournalButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_internship == null ||
            _internship.Status != "Approved" ||
            _internship.JournalStatus == "Pending" ||
            _internship.JournalStatus == "Approved")
            return;

        if (_internship.DailyLogs.Count < 5)
        {
            await ShowMessage(
                LocalizationService.Get("MinimumFiveLogs"));

            return;
        }

        var dialog = new ContentDialog
        {
            Title = LocalizationService.Get("SubmitJournalQuestion"),
            Content = LocalizationService.Get("SubmitJournalMessage"),
            PrimaryButtonText = LocalizationService.Get("Submit"),
            CloseButtonText = LocalizationService.Get("Cancel"),
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary)
            return;

        using var db = new AppDbContext();

        var internship = await db.Internships
            .Include(i => i.DailyLogs)
            .FirstOrDefaultAsync(i => i.Id == _internshipId);

        if (internship == null ||
            internship.Status != "Approved" ||
            internship.JournalStatus == "Pending" ||
            internship.JournalStatus == "Approved")
            return;

        if (internship.DailyLogs.Count < 5)
        {
            await ShowMessage(
                LocalizationService.Get("MinimumFiveLogs"));

            return;
        }

        internship.JournalStatus = "Pending";

        await db.SaveChangesAsync();

        await LoadInternship();
    }

    private async void CancelInternshipButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_internship == null ||
            _internship.Status != "Approved")
            return;

        var keepButton = new Button
        {
            Content = LocalizationService.Get("KeepInternship"),
            Background = new SolidColorBrush(
                Microsoft.UI.Colors.DodgerBlue),
            Foreground = new SolidColorBrush(
                Microsoft.UI.Colors.White),
            Padding = new Thickness(16, 8, 16, 8),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var cancelButton = new Button
        {
            Content = LocalizationService.Get("CancelInternship"),
            Background = new SolidColorBrush(
                Microsoft.UI.Colors.Red),
            Foreground = new SolidColorBrush(
                Microsoft.UI.Colors.White),
            Padding = new Thickness(16, 8, 16, 8),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var dialogContent = new StackPanel
        {
            Spacing = 12
        };

        dialogContent.Children.Add(
            new TextBlock
            {
                Text = LocalizationService.Get("CancelInternshipMessage"),
                TextWrapping = TextWrapping.Wrap
            });

        dialogContent.Children.Add(cancelButton);
        dialogContent.Children.Add(keepButton);

        var dialog = new ContentDialog
        {
            Title = LocalizationService.Get("CancelInternshipQuestion"),
            Content = dialogContent,
            XamlRoot = XamlRoot
        };

        bool confirmed = false;

        cancelButton.Click += (_, _) =>
        {
            confirmed = true;
            dialog.Hide();
        };

        keepButton.Click += (_, _) =>
        {
            confirmed = false;
            dialog.Hide();
        };

        await dialog.ShowAsync();

        if (!confirmed)
            return;

        using var db = new AppDbContext();

        var internshipToCancel = await db.Internships
            .Include(i => i.Employer)
            .FirstOrDefaultAsync(i => i.Id == _internshipId);

        if (internshipToCancel == null ||
            internshipToCancel.Status != "Approved")
            return;

        if (internshipToCancel.Employer != null)
            internshipToCancel.Employer.StudentCapacity++;

        internshipToCancel.Status = "Cancelled";

        await db.SaveChangesAsync();
        await InternshipNotificationService.RefreshForCurrentUserAsync();

        if (Frame.CanGoBack)
            Frame.GoBack();
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

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
            Frame.GoBack();
    }
}
