using InternLog.Pages;
using InternLog.Services;
using Microsoft.UI.Xaml;
using Windows.Storage;

namespace InternLog;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ApplySavedAppearance();

        LocalizationService.LanguageChanged += ApplyLanguage;

        ApplyLanguage();
        ShowAuthentication();
    }

    private void ApplySavedAppearance()
    {
        string appearance =
            ApplicationData.Current.LocalSettings.Values["Appearance"] as string
            ?? "Light";

        if (Content is FrameworkElement root)
        {
            root.RequestedTheme = appearance == "Dark"
                ? ElementTheme.Dark
                : ElementTheme.Light;
        }
    }

    private void ApplyLanguage()
    {
        AppSubtitleText.Text =
            LocalizationService.Get("InternshipTracker");

        HomeButton.Content =
            LocalizationService.Get("Home");

        EmployersButton.Content =
            LocalizationService.Get("Employers");

        DailyLogButton.Content =
            LocalizationService.Get("DailyLog");

        DashboardButton.Content =
            LocalizationService.Get("Dashboard");

        ProfileButton.Content =
            LocalizationService.Get("Profile");

        SettingsButton.Content =
            LocalizationService.Get("Settings");

        LogoutButton.Content =
            LocalizationService.Get("Logout");
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

        ApplyLanguage();

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
