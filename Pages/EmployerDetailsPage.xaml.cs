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
        LocalizationService.LanguageChanged += ApplyLanguage;
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is Employer employer)
        {
            _employer = employer;
            LoadEmployerData();
        }
        ApplyLanguage();
    }
    private void ApplyLanguage()
    {
        BackButtonText.Text = LocalizationService.Get("BackToEmployers");
        InternshipOpenText.Text = LocalizationService.Get("InternshipOpen");
        OverviewTitleText.Text = LocalizationService.Get("InternshipOverview");
        AvailablePositionsTitleText.Text = LocalizationService.Get("AvailablePositions");
        AvailablePositionsDescriptionText.Text = LocalizationService.Get("StudentsCanBeAccepted");
        LocationTitleText.Text = LocalizationService.Get("Location");
        LocationDescriptionText.Text = LocalizationService.Get("InternshipLocation");
        IndustryTitleText.Text = LocalizationService.Get("Industry");
        IndustryDescriptionText.Text = LocalizationService.Get("FieldOfWork");
        AboutEmployerTitleText.Text = LocalizationService.Get("AboutEmployer");
        InternshipExperienceTitleText.Text = LocalizationService.Get("InternshipExperience");
        InternshipExperienceDescriptionText.Text = LocalizationService.Get("WhatToExpect");
        ContactTitleText.Text = LocalizationService.Get("Contact");
        InternshipHighlightsTitleText.Text = LocalizationService.Get("InternshipHighlights");
        InternshipHighlightsDescriptionText.Text = LocalizationService.Get("WhatMakesInteresting");
        RealWorldExperienceText.Text = LocalizationService.Get("RealWorldExperience");
        ModernTechnologiesText.Text = LocalizationService.Get("ModernTechnologies");
        PracticalSkillsText.Text = LocalizationService.Get("PracticalSkills");
        ReserveButton.Content = LocalizationService.Get("ReserveInternship");
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
        PositionsText.Text = _employer.StudentCapacity.ToString();
        ContactEmailText.Text = string.IsNullOrWhiteSpace(_employer.ContactEmail)
            ? LocalizationService.Get("NoContactEmail")
            : _employer.ContactEmail;
        ContactPhoneText.Text = string.IsNullOrWhiteSpace(_employer.ContactPhone)
            ? LocalizationService.Get("NoPhoneNumber")
            : _employer.ContactPhone;
        WebsiteText.Text = string.IsNullOrWhiteSpace(_employer.Website)
            ? LocalizationService.Get("NoWebsite")
            : _employer.Website;
        if (!string.IsNullOrWhiteSpace(_employer.Name))
            CompanyInitial.Text = _employer.Name.Substring(0, 1).ToUpper();
        UpdateReservationState();
    }
    private async void UpdateReservationState()
    {
        if (_employer == null || SessionService.CurrentUser == null)
            return;
        using var db = new AppDbContext();
        bool alreadyReserved = await db.Internships
            .AnyAsync(i =>
                i.UserId == SessionService.CurrentUser.Id &&
                i.EmployerId == _employer.Id &&
                i.Status != "Cancelled");
        if (alreadyReserved)
        {
            ReserveButton.Content = LocalizationService.Get("Reserved");
            ReserveButton.IsEnabled = false;
        }
        else
        {
            ReserveButton.Content = LocalizationService.Get("ReserveInternship");
            ReserveButton.IsEnabled = _employer.StudentCapacity > 0;
        }
    }
    private async void ReserveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_employer == null)
            return;
        if (SessionService.CurrentUser == null)
        {
            await ShowMessage(LocalizationService.Get("MustBeLoggedInToReserve"));
            return;
        }
        using var db = new AppDbContext();
        var employer = await db.Employers
            .FirstOrDefaultAsync(e => e.Id == _employer.Id);
        if (employer == null)
        {
            await ShowMessage(LocalizationService.Get("EmployerNotFound"));
            return;
        }
        bool alreadyReserved = await db.Internships
            .AnyAsync(i =>
                i.UserId == SessionService.CurrentUser.Id &&
                i.EmployerId == employer.Id &&
                i.Status != "Cancelled");
        if (alreadyReserved)
        {
            await ShowMessage(LocalizationService.Get("AlreadyReserved"));
            UpdateReservationButton();
            return;
        }
        if (employer.StudentCapacity <= 0)
        {
            await ShowMessage(LocalizationService.Get("NoAvailablePositions"));
            return;
        }
        int internshipCount = await db.Internships
            .CountAsync(i =>
                i.UserId == SessionService.CurrentUser.Id &&
                i.Status != "Cancelled");
        if (internshipCount >= 3)
        {
            await ShowMessage(LocalizationService.Get("MaximumInternships"));
            return;
        }
        var dialog = new ContentDialog
        {
            Title = LocalizationService.Get("ReserveInternshipQuestion"),
            Content = string.Format(
                LocalizationService.Get("ReserveInternshipMessage"),
                employer.Name),
            PrimaryButtonText = LocalizationService.Get("ConfirmReservation"),
            CloseButtonText = LocalizationService.Get("Cancel"),
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
        PositionsText.Text = employer.StudentCapacity.ToString();
        UpdateReservationButton();
        await ShowMessage(string.Format(
            LocalizationService.Get("ReservationSuccessful"),
            employer.Name));
    }
    private void UpdateReservationButton()
    {
        ReserveButton.Content = LocalizationService.Get("Reserved");
        ReserveButton.IsEnabled = false;
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