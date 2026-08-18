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

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        InitializeComponent();
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
            await ShowMessage("Please enter your email and password.");
            return;
        }

        var authService = new AuthService();

        var user = authService.Login(email, password);

        if (user == null)
        {
            await ShowMessage("Incorrect email or password.");
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
            CloseButtonText = "OK",
            XamlRoot = this.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }
}

