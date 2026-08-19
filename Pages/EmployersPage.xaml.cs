using InternLog.Data;
using InternLog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternLog.Pages;

public sealed partial class EmployersPage : Page
{
    private List<Employer> _employers = new();

    public EmployersPage()
    {
        InitializeComponent();

        Loaded += EmployersPage_Loaded;
    }

    private async void EmployersPage_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadEmployers();
    }

    private async Task LoadEmployers()
    {
        using var db = new AppDbContext();

        _employers = await db.Employers
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .ToListAsync();

        EmployersGridView.ItemsSource = _employers;
    }

    private void Employer_ItemClick(
        object sender,
        ItemClickEventArgs e)
    {
        if (e.ClickedItem is Employer employer)
        {
            Frame.Navigate(
                typeof(EmployerDetailsPage),
                employer);
        }
    }

    private void ViewDetailsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.DataContext is Employer employer)
        {
            Frame.Navigate(
                typeof(EmployerDetailsPage),
                employer);
        }
    }

    private void SearchTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        string searchText = SearchTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            EmployersGridView.ItemsSource = _employers;
            return;
        }

        var filteredEmployers = _employers
            .Where(e =>
                e.Name.Contains(
                    searchText,
                    System.StringComparison.OrdinalIgnoreCase) ||

                e.Industry.Contains(
                    searchText,
                    System.StringComparison.OrdinalIgnoreCase) ||

                e.Location.Contains(
                    searchText,
                    System.StringComparison.OrdinalIgnoreCase))
            .ToList();

        EmployersGridView.ItemsSource = filteredEmployers;
    }
}