using InternLog.Data;
using InternLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
        LocalizationService.LanguageChanged += ApplyLanguage;
    }

    private async void HomePage_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        ApplyLanguage();
        await LoadInternships();
    }

    private void ApplyLanguage()
    {
        HomeTitleText.Text =
            LocalizationService.Get("MyInternships");

        HomeDescriptionText.Text =
            LocalizationService.Get("ManageInternships");

        EmptyStateTitleText.Text =
            LocalizationService.Get("NoInternships");

        EmptyStateDescriptionText.Text =
            LocalizationService.Get("NoInternshipsDescription");

        UpdateInternshipButtonTexts();
    }

    private async Task LoadInternships()
    {
        if (SessionService.CurrentUser == null)
            return;

        using var db = new AppDbContext();

        _internships = await db.Internships
            .Include(i => i.Employer)
            .Where(i =>
                i.UserId == SessionService.CurrentUser.Id &&
                i.Status == "Approved")
            .OrderByDescending(i => i.StartDate)
            .ToListAsync();

        InternshipsItemsControl.ItemsSource =
            _internships;

        EmptyStatePanel.Visibility =
            _internships.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;

        UpdateInternshipButtonTexts();
    }

    private void UpdateInternshipButtonTexts()
    {
        if (InternshipsItemsControl == null)
            return;

        foreach (var internship in _internships)
        {
            var container =
                InternshipsItemsControl.ContainerFromItem(internship);

            if (container == null)
                continue;

            var button =
                FindVisualChild<Button>(
                    container,
                    string.Empty);

            if (button != null)
            {
                button.Content =
                    LocalizationService.Get("ViewInternship");
            }
        }
    }

    private void ViewInternshipButton_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            button.Content =
                LocalizationService.Get("ViewInternship");
        }
    }

    private static T? FindVisualChild<T>(
        DependencyObject parent,
        string name)
        where T : FrameworkElement
    {
        int count =
            VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < count; i++)
        {
            var child =
                VisualTreeHelper.GetChild(parent, i);

            if (child is T element &&
                element.Name == name)
            {
                return element;
            }

            var result =
                FindVisualChild<T>(child, name);

            if (result != null)
                return result;
        }

        return null;
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
