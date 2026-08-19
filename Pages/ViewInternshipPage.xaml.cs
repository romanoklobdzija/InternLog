using InternLog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InternLog.Pages;

public sealed partial class ViewInternshipPage : Page
{
    private int _internshipId;
    private Models.Internship? _internship;

    public ViewInternshipPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(
        Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is int internshipId)
        {
            _internshipId = internshipId;
            await LoadInternship();
        }
    }

    private async System.Threading.Tasks.Task LoadInternship()
    {
        using var db = new AppDbContext();

        _internship = await db.Internships
            .Include(i => i.Employer)
            .Include(i => i.DailyLogs)
            .FirstOrDefaultAsync(i => i.Id == _internshipId);

        if (_internship == null)
            return;

        EmployerNameText.Text =
            _internship.Employer?.Name ?? "Unknown employer";

        EmployerText.Text =
            _internship.Employer?.Name ?? "Unknown employer";

        LocationText.Text =
            _internship.Employer?.Location ?? "Unknown location";

        StartDateText.Text =
            _internship.StartDate.ToString("dd.MM.yyyy");

        EndDateText.Text =
            _internship.EndDate.HasValue
                ? _internship.EndDate.Value.ToString("dd.MM.yyyy")
                : "Not specified";

        StatusText.Text = _internship.Status;

        JournalStatusText.Text =
            $"Status: {_internship.JournalStatus} • {_internship.DailyLogs.Count} daily logs";

        // ============================================================
        // BUTTON STATES
        // ============================================================

        if (_internship.Status == "Cancelled")
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
                SubmitJournalButton.Content = "Journal Submitted";
            }
            else
            {
                SubmitJournalButton.IsEnabled = true;
                SubmitJournalButton.Content = "Submit Journal for Review";
            }
        }
    }

    private void OpenDailyLogButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_internship == null ||
            _internship.Status == "Cancelled")
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
            _internship.Status == "Cancelled" ||
            _internship.JournalStatus == "Pending" ||
            _internship.JournalStatus == "Approved")
            return;

        if (_internship.DailyLogs.Count < 5)
        {
            await ShowMessage(
                "You must complete at least 5 daily logs before submitting your journal for review.");

            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Submit journal?",
            Content = "Are you sure you want to submit your journal for review?",
            PrimaryButtonText = "Submit",
            CloseButtonText = "Cancel",
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
            internship.Status == "Cancelled" ||
            internship.JournalStatus == "Pending" ||
            internship.JournalStatus == "Approved")
            return;

        if (internship.DailyLogs.Count < 5)
        {
            await ShowMessage(
                "You must complete at least 5 daily logs before submitting your journal for review.");

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
            _internship.Status == "Cancelled")
            return;

        var keepButton = new Button
        {
            Content = "Keep Internship",
            Background = new SolidColorBrush(
                Microsoft.UI.Colors.DodgerBlue),
            Foreground = new SolidColorBrush(
                Microsoft.UI.Colors.White),
            Padding = new Thickness(16, 8, 16, 8),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var cancelButton = new Button
        {
            Content = "Cancel Internship",
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
                Text = "Are you sure you want to cancel this internship?",
                TextWrapping = TextWrapping.Wrap
            });

        dialogContent.Children.Add(cancelButton);
        dialogContent.Children.Add(keepButton);

        var dialog = new ContentDialog
        {
            Title = "Cancel internship?",
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
            internshipToCancel.Status == "Cancelled")
            return;

        // Oslobodi mjesto kod poslodavca
        if (internshipToCancel.Employer != null)
        {
            internshipToCancel.Employer.StudentCapacity++;
        }

        // Oznaèi praksu kao otkazanu
        internshipToCancel.Status = "Cancelled";

        await db.SaveChangesAsync();

        // Vrati se na My Internships
        if (Frame.CanGoBack)
            Frame.GoBack();
    }




    private async Task ShowMessage(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "InternLog",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }
}