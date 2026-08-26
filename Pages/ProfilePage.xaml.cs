using InternLog.Data;
using InternLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace InternLog.Pages;

public sealed partial class ProfilePage : Page
{
    private string _avatarPath = string.Empty;

    public ProfilePage()
    {
        InitializeComponent();

        Loaded += ProfilePage_Loaded;

        LocalizationService.LanguageChanged += ApplyLanguage;
    }

    private async void ProfilePage_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        ApplyLanguage();
        await LoadProfile();
    }

    private void ApplyLanguage()
    {
        ProfileTitleText.Text =
            LocalizationService.Get("Profile");

        ProfileDescriptionText.Text =
            LocalizationService.Get("ProfileDescription");

        ChangeAvatarButton.Content =
            LocalizationService.Get("ChangePhoto");

        FirstNameLabelText.Text =
            LocalizationService.Get("FirstName");

        LastNameLabelText.Text =
            LocalizationService.Get("LastName");

        EmailLabelText.Text =
            LocalizationService.Get("Email");

        AboutMeLabelText.Text =
            LocalizationService.Get("AboutMe");

        SkillsLabelText.Text =
            LocalizationService.Get("SkillsAndInterests");

        TagsDescriptionText.Text =
            LocalizationService.Get("SeparateTagsWithCommas");

        AccountInformationTitleText.Text =
            LocalizationService.Get("AccountInformation");

        SaveButton.Content =
            LocalizationService.Get("SaveChanges");

        if (SessionService.CurrentUser != null)
        {
            CreatedAtText.Text =
                string.Format(
                    LocalizationService.Get("AccountCreated"),
                    SessionService.CurrentUser.CreatedAt.ToString("dd.MM.yyyy"));
        }
    }

    private async Task LoadProfile()
    {
        if (SessionService.CurrentUser == null)
            return;

        using var db = new AppDbContext();

        var user = await db.Users
            .FirstOrDefaultAsync(
                u => u.Id == SessionService.CurrentUser.Id);

        if (user == null)
            return;

        FirstNameTextBox.Text = user.FirstName;
        LastNameTextBox.Text = user.LastName;
        EmailTextBox.Text = user.Email;
        BioTextBox.Text = user.Bio;
        TagsTextBox.Text = user.Tags;

        ProfileNameText.Text =
            $"{user.FirstName} {user.LastName}".Trim();

        ProfileEmailText.Text =
            user.Email;

        CreatedAtText.Text =
            string.Format(
                LocalizationService.Get("AccountCreated"),
                user.CreatedAt.ToString("dd.MM.yyyy"));

        _avatarPath = user.AvatarPath;

        UpdateAvatar(user);
    }

    private void UpdateAvatar(Models.User user)
    {
        if (!string.IsNullOrWhiteSpace(user.AvatarPath) &&
            File.Exists(user.AvatarPath))
        {
            AvatarImage.Source =
                new BitmapImage(
                    new Uri(user.AvatarPath));

            AvatarImage.Visibility =
                Visibility.Visible;

            AvatarInitialsText.Visibility =
                Visibility.Collapsed;

            return;
        }

        AvatarImage.Visibility =
            Visibility.Collapsed;

        AvatarInitialsText.Visibility =
            Visibility.Visible;

        string initials = string.Empty;

        if (!string.IsNullOrWhiteSpace(user.FirstName))
            initials += user.FirstName[0];

        if (!string.IsNullOrWhiteSpace(user.LastName))
            initials += user.LastName[0];

        AvatarInitialsText.Text =
            string.IsNullOrWhiteSpace(initials)
                ? "?"
                : initials.ToUpper();
    }

    private async void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (SessionService.CurrentUser == null)
            return;

        using var db = new AppDbContext();

        var user = await db.Users
            .FirstOrDefaultAsync(
                u => u.Id == SessionService.CurrentUser.Id);

        if (user == null)
            return;

        user.FirstName =
            FirstNameTextBox.Text.Trim();

        user.LastName =
            LastNameTextBox.Text.Trim();

        user.Email =
            EmailTextBox.Text.Trim();

        user.Bio =
            BioTextBox.Text.Trim();

        user.Tags =
            TagsTextBox.Text.Trim();

        user.AvatarPath =
            _avatarPath;

        await db.SaveChangesAsync();

        SessionService.CurrentUser.FirstName =
            user.FirstName;

        SessionService.CurrentUser.LastName =
            user.LastName;

        SessionService.CurrentUser.Email =
            user.Email;

        SessionService.CurrentUser.Bio =
            user.Bio;

        SessionService.CurrentUser.Tags =
            user.Tags;

        SessionService.CurrentUser.AvatarPath =
            user.AvatarPath;

        ProfileNameText.Text =
            $"{user.FirstName} {user.LastName}".Trim();

        ProfileEmailText.Text =
            user.Email;

        UpdateAvatar(user);

        await ShowMessage(
            LocalizationService.Get("ProfileSaved"));
    }

    private async void ChangeAvatarButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (SessionService.CurrentUser == null)
            return;

        var picker = new FileOpenPicker();

        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".webp");

        var hwnd =
            WindowNative.GetWindowHandle(
                App.MainWindow);

        InitializeWithWindow.Initialize(
            picker,
            hwnd);

        var file =
            await picker.PickSingleFileAsync();

        if (file == null)
            return;

        var localFolder =
            ApplicationData.Current.LocalFolder;

        var avatarFolder =
            await localFolder.CreateFolderAsync(
                "Avatars",
                CreationCollisionOption.OpenIfExists);

        await file.CopyAsync(
            avatarFolder,
            $"{SessionService.CurrentUser.Id}_{file.Name}",
            NameCollisionOption.ReplaceExisting);

        _avatarPath =
            Path.Combine(
                avatarFolder.Path,
                $"{SessionService.CurrentUser.Id}_{file.Name}");

        AvatarImage.Source =
            new BitmapImage(
                new Uri(_avatarPath));

        AvatarImage.Visibility =
            Visibility.Visible;

        AvatarInitialsText.Visibility =
            Visibility.Collapsed;
    }

    private async Task ShowMessage(
        string message)
    {
        var dialog = new ContentDialog
        {
            Title = "InternLog",
            Content = message,
            CloseButtonText =
                LocalizationService.Get("OK"),
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }
}