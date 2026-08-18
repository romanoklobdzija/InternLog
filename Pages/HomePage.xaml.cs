using InternLog.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InternLog.Pages;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();

        if (SessionService.CurrentUser != null)
        {
            WelcomeTextBlock.Text =
                $"Welcome back, {SessionService.CurrentUser.FirstName}!";
        }
    }
}