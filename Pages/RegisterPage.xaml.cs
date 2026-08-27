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
using System.Text.RegularExpressions;
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
    private static readonly Regex EmailPattern = new(
        @"^[A-Za-z0-9]+@[A-Za-z0-9]+\.[A-Za-z]{2,}$",
        RegexOptions.Compiled);

    private static readonly Regex PasswordPattern = new(
        @"^[A-Za-z0-9]{8,}$",
        RegexOptions.Compiled);

    public RegisterPage()
    {
        InitializeComponent();
        Loaded += RegisterPage_Loaded;
        LocalizationService.LanguageChanged += ApplyLanguage;
    }

    private void RegisterPage_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        RegisterTitleText.Text = LocalizationService.Get("CreateYourAccount");
        RegisterDescriptionText.Text = LocalizationService.Get("RegisterDescription");
        FirstNameLabelText.Text = LocalizationService.Get("FirstName");
        FirstNameTextBox.PlaceholderText = LocalizationService.Get("EnterFirstName");
        LastNameLabelText.Text = LocalizationService.Get("LastName");
        LastNameTextBox.PlaceholderText = LocalizationService.Get("EnterLastName");
        EmailLabelText.Text = LocalizationService.Get("Email");
        EmailTextBox.PlaceholderText = LocalizationService.Get("EnterEmail");
        PasswordLabelText.Text = LocalizationService.Get("Password");
        PasswordBox.PlaceholderText = LocalizationService.Get("EnterPassword");
        ConfirmPasswordLabelText.Text = LocalizationService.Get("ConfirmPassword");
        ConfirmPasswordBox.PlaceholderText = LocalizationService.Get("ConfirmPasswordPlaceholder");
        CreateAccountButton.Content = LocalizationService.Get("CreateAccount");
        CancelRegistrationButton.Content = LocalizationService.Get("Cancel");
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
            await ShowMessage(LocalizationService.Get("FillAllFields"));
            return;
        }

        if (!EmailPattern.IsMatch(email))
        {
            await ShowMessage(LocalizationService.Get("InvalidEmail"));
            return;
        }

        if (!PasswordPattern.IsMatch(password))
        {
            await ShowMessage(LocalizationService.Get("InvalidPassword"));
            return;
        }

        // Provjera lozinki
        if (password != confirmPassword)
        {
            await ShowMessage(LocalizationService.Get("PasswordsDoNotMatch"));
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
                LocalizationService.Get("AccountAlreadyExists"));

            return;
        }

        await ShowMessage(
            LocalizationService.Get("AccountCreatedSuccessfully"));
        Frame.Navigate(typeof(LoginPage));
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

    private void CancelRegistrationButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(LoginPage));
    }


}
