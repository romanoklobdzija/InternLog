using InternLog.Data;
using InternLog.Models;
using InternLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
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
        LocalizationService.LanguageChanged += ApplyLanguage;
    }

    private async void EmployersPage_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        ApplyLanguage();
        await LoadEmployers();
    }

    private void ApplyLanguage()
    {
        EmployersTitleText.Text =
            LocalizationService.Get("Employers");

        EmployersDescriptionText.Text =
            LocalizationService.Get("EmployersDescription");

        SearchTextBox.PlaceholderText =
            LocalizationService.Get("SearchEmployers");

        UpdateViewDetailsButtonTexts();
    }

    private async Task LoadEmployers()
    {
        using var db = new AppDbContext();

        _employers = await db.Employers
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .ToListAsync();

        EmployersGridView.ItemsSource =
            _employers;

        UpdateViewDetailsButtonTexts();
    }

    private void UpdateViewDetailsButtonTexts()
    {
        if (EmployersGridView == null)
            return;

        foreach (var employer in _employers)
        {
            var container =
                EmployersGridView.ContainerFromItem(employer);

            if (container == null)
                continue;

            var button =
                FindVisualChild<Button>(
                    container,
                    string.Empty);

            if (button != null)
            {
                button.Content =
                    LocalizationService.Get("ViewDetails");
            }
        }
    }

    private void ViewDetailsButton_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            button.Content =
                LocalizationService.Get("ViewDetails");
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
        string searchText =
            SearchTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            EmployersGridView.ItemsSource =
                _employers;

            UpdateViewDetailsButtonTexts();

            return;
        }

        var filteredEmployers = _employers
            .Where(e =>
                e.Name.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase) ||
                e.Industry.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase) ||
                e.Location.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        EmployersGridView.ItemsSource =
            filteredEmployers;

        UpdateViewDetailsButtonTexts();
    }
}
