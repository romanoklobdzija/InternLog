using InternLog.Pages;
using InternLog.Services;
using Microsoft.UI.Xaml;

namespace InternLog;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ShowAuthentication();
    }

    private void ShowAuthentication()
    {
        ApplicationView.Visibility = Visibility.Collapsed;
        AuthenticationFrame.Visibility = Visibility.Visible;

        AuthenticationFrame.Navigate(typeof(LoginPage));
    }

    public void ShowApplication()
    {
        AuthenticationFrame.Visibility = Visibility.Collapsed;
        ApplicationView.Visibility = Visibility.Visible;

        ContentFrame.Navigate(typeof(HomePage));
    }

    private void HomeButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(HomePage));
    }


    private void EmployersButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(EmployersPage));
    }



    private void DashboardButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(DashboardPage));
    }

    private void DailyLogButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(DailyLogPage));
    }

    private void ProfileButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(ProfilePage));
    }

    private void SettingsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(SettingsPage));
    }

    private void LogoutButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        SessionService.Logout();

        ShowAuthentication();
    }
}