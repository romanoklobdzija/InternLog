using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using InternLog.Services;
using System.Threading.Tasks;
using InternLog;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace InternLog.Pages;


public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        InitializeComponent();
        Loaded += LoginPage_Loaded;
        LocalizationService.LanguageChanged += ApplyLanguage;
    }

    private void LoginPage_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        WelcomeTitleText.Text = LocalizationService.Get("WelcomeBackLogin");
        LoginDescriptionText.Text = LocalizationService.Get("LoginDescription");
        EmailLabelText.Text = LocalizationService.Get("Email");
        EmailTextBox.PlaceholderText = LocalizationService.Get("EnterEmail");
        PasswordLabelText.Text = LocalizationService.Get("Password");
        PasswordBox.PlaceholderText = LocalizationService.Get("EnterPassword");
        LoginButton.Content = LocalizationService.Get("LogIn");
        CreateAccountButton.Content = LocalizationService.Get("CreateAccount");
    }

    private async void LoginButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        string email = EmailTextBox.Text.Trim().ToLower();
        string password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            await ShowMessage(LocalizationService.Get("EnterEmailAndPassword"));
            return;
        }

        var authService = new AuthService();

        var user = authService.Login(email, password);

        if (user == null)
        {
            await ShowMessage(LocalizationService.Get("InvalidCredentials"));
            return;
        }

        SessionService.Login(user);

        var mainWindow = App.MainWindow as MainWindow;

        if (mainWindow == null)
        {
            return;
        }

        mainWindow.ShowApplication();
    }



    private void RegisterButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        Frame.Navigate(typeof(RegisterPage));
    }


    private async Task ShowMessage(string message)
    {
        ContentDialog dialog = new ContentDialog
        {
            Title = "InternLog",
            Content = message,
            CloseButtonText = LocalizationService.Get("OK"),
            XamlRoot = this.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }
}

