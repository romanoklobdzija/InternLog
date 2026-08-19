using InternLog.Data;
using InternLog.Models;
using InternLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;

namespace InternLog.Pages;

public sealed partial class EmployerDetailsPage : Page
{
    private Employer? _employer;

    public EmployerDetailsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Employer employer)
        {
            _employer = employer;
            LoadEmployerData();
        }
    }

    private void LoadEmployerData()
    {
        if (_employer == null)
            return;

        CompanyNameText.Text = _employer.Name;

        IndustryText.Text = _employer.Industry;
        IndustryCardText.Text = _employer.Industry;

        LocationText.Text = _employer.Location;
        LocationCardText.Text = _employer.Location;

        DescriptionText.Text = _employer.Description;
        TasksText.Text = _employer.StudentTasks;

        PositionsText.Text =
            _employer.StudentCapacity.ToString();

        ContactEmailText.Text =
            string.IsNullOrWhiteSpace(_employer.ContactEmail)
                ? "No contact email available"
                : _employer.ContactEmail;

        ContactPhoneText.Text =
            string.IsNullOrWhiteSpace(_employer.ContactPhone)
                ? "No phone number available"
                : _employer.ContactPhone;

        WebsiteText.Text =
            string.IsNullOrWhiteSpace(_employer.Website)
                ? "No website available"
                : _employer.Website;

        if (!string.IsNullOrWhiteSpace(_employer.Name))
        {
            CompanyInitial.Text =
                _employer.Name.Substring(0, 1).ToUpper();
        }

        UpdateReservationState();
    }

    private async void UpdateReservationState()
    {
        if (_employer == null ||
            SessionService.CurrentUser == null)
        {
            return;
        }

        using var db = new AppDbContext();

        bool alreadyReserved = await db.Internships
            .AnyAsync(i =>
                i.UserId == SessionService.CurrentUser.Id &&
                i.EmployerId == _employer.Id &&
                i.Status != "Cancelled");

        if (alreadyReserved)
        {
            ReserveButton.Content = "Reserved ✓";
            ReserveButton.IsEnabled = false;
        }
        else
        {
            ReserveButton.Content = "Reserve Internship";
            ReserveButton.IsEnabled = _employer.StudentCapacity > 0;
        }
    }

    private async void ReserveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_employer == null)
            return;

        if (SessionService.CurrentUser == null)
        {
            await ShowMessage(
                "You must be logged in to reserve an internship.");

            return;
        }

        using var db = new AppDbContext();

        var employer = await db.Employers
            .FirstOrDefaultAsync(e => e.Id == _employer.Id);

        if (employer == null)
        {
            await ShowMessage(
                "This employer could not be found.");

            return;
        }

        // Provjera samo aktivnih rezervacija
        bool alreadyReserved = await db.Internships
            .AnyAsync(i =>
                i.UserId == SessionService.CurrentUser.Id &&
                i.EmployerId == employer.Id &&
                i.Status != "Cancelled");

        if (alreadyReserved)
        {
            await ShowMessage(
                "You have already reserved an internship with this employer.");

            UpdateReservationButton();

            return;
        }

        if (employer.StudentCapacity <= 0)
        {
            await ShowMessage(
                "There are currently no available internship positions.");

            return;
        }

        // Broje se samo aktivne prakse
        int internshipCount = await db.Internships
            .CountAsync(i =>
                i.UserId == SessionService.CurrentUser.Id &&
                i.Status != "Cancelled");

        if (internshipCount >= 3)
        {
            await ShowMessage(
                "You can have a maximum of 3 active internships at the same time.");

            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Reserve internship?",
            Content =
                $"You are about to reserve an internship at {employer.Name}.",
            PrimaryButtonText = "Confirm reservation",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary)
            return;

        var internship = new Internship
        {
            UserId = SessionService.CurrentUser.Id,
            EmployerId = employer.Id,
            StartDate = DateTime.Today,
            EndDate = null,
            Status = "Approved"
        };

        db.Internships.Add(internship);

        employer.StudentCapacity--;

        await db.SaveChangesAsync();

        _employer.StudentCapacity = employer.StudentCapacity;

        PositionsText.Text =
            employer.StudentCapacity.ToString();

        UpdateReservationButton();

        await ShowMessage(
            $"Your internship at {employer.Name} has been successfully reserved.");
    }

    private void UpdateReservationButton()
    {
        ReserveButton.Content = "Reserved ✓";
        ReserveButton.IsEnabled = false;
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

    private void BackButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}