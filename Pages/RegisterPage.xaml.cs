using InternLog.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace InternLog.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class RegisterPage : Page
{
    public RegisterPage()
    {
        InitializeComponent();
    }


    private async void CreateAccountButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        string firstName = FirstNameTextBox.Text.Trim();
        string lastName = LastNameTextBox.Text.Trim();
        string email = EmailTextBox.Text.Trim().ToLower();
        string password = PasswordBox.Password;
        string confirmPassword = ConfirmPasswordBox.Password;

        // Provjera praznih polja
        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            await ShowMessage("Please fill in all fields.");
            return;
        }

        // Provjera lozinki
        if (password != confirmPassword)
        {
            await ShowMessage("Passwords do not match.");
            return;
        }

        // Registracija
        var authService = new AuthService();

        bool success = authService.Register(
            firstName,
            lastName,
            email,
            password);

        if (!success)
        {
            await ShowMessage(
                "An account with this email already exists.");

            return;
        }

        await ShowMessage(
            "Your account has been successfully created!");
        Frame.Navigate(typeof(LoginPage));
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
