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
                i.EmployerId == _employer.Id);

        if (alreadyReserved)
        {
            ReserveButton.Content = "Reserved ✓";
            ReserveButton.IsEnabled = false;
        }
    }

    private async void ReserveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_employer == null)
            return;

        // 1. Provjera je li korisnik prijavljen
        if (SessionService.CurrentUser == null)
        {
            await ShowMessage(
                "You must be logged in to reserve an internship.");

            return;
        }

        using var db = new AppDbContext();

        // 2. Učitavanje najnovijih podataka o employeru iz baze
        var employer = await db.Employers
            .FirstOrDefaultAsync(e => e.Id == _employer.Id);

        if (employer == null)
        {
            await ShowMessage(
                "This employer could not be found.");

            return;
        }

        // 3. Provjera je li korisnik već rezervirao ovaj internship
        bool alreadyReserved = await db.Internships
            .AnyAsync(i =>
                i.UserId == SessionService.CurrentUser.Id &&
                i.EmployerId == employer.Id);

        if (alreadyReserved)
        {
            await ShowMessage(
                "You have already reserved an internship with this employer.");

            UpdateReservationButton();

            return;
        }

        // 4. Provjera slobodnih mjesta
        if (employer.StudentCapacity <= 0)
        {
            await ShowMessage(
                "There are currently no available internship positions.");

            return;
        }

        // 5. Provjera maksimalnog broja internshipa
        int internshipCount = await db.Internships
            .CountAsync(i =>
                i.UserId == SessionService.CurrentUser.Id);

        if (internshipCount >= 3)
        {
            await ShowMessage(
                "You can have a maximum of 3 active internships at the same time.");

            return;
        }

        // 6. Confirmation dialog
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

        // 7. Kreiranje internship zapisa
        var internship = new Internship
        {
            UserId = SessionService.CurrentUser.Id,
            EmployerId = employer.Id,
            StartDate = DateTime.Today,
            EndDate = null,
            Status = "Approved"
        };

        db.Internships.Add(internship);

        // 8. Smanjenje broja dostupnih mjesta
        employer.StudentCapacity--;

        await db.SaveChangesAsync();

        // 9. Ažuriranje lokalnog prikaza
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