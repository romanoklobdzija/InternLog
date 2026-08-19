using InternLog.Data;
using InternLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternLog.Pages;

public sealed partial class HomePage : Page
{
    private List<Models.Internship> _internships = new();

    public HomePage()
    {
        InitializeComponent();
        Loaded += HomePage_Loaded;
    }

    private async void HomePage_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadInternships();
    }

    private async Task LoadInternships()
    {
        if (SessionService.CurrentUser == null)
            return;

        using var db = new AppDbContext();

        _internships = await db.Internships
            .Include(i => i.Employer)
            .Where(i => i.UserId == SessionService.CurrentUser.Id)
            .Where(i => i.Status != "Cancelled")
            .ToListAsync();

        InternshipsItemsControl.ItemsSource = _internships;

        EmptyStatePanel.Visibility =
            _internships.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    private void ViewInternshipButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Tag is int internshipId)
        {
            Frame.Navigate(
                typeof(ViewInternshipPage),
                internshipId);
        }
    }
}