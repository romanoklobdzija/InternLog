using InternLog.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace InternLog.Pages;

public sealed partial class SettingsPage : Page
{
    private readonly ApplicationDataContainer _settings;
    private bool _loading;

    public SettingsPage()
    {
        InitializeComponent();

        _settings = ApplicationData.Current.LocalSettings;

        LoadSettings();

        LocalizationService.LanguageChanged += ApplyLanguage;
    }

    private void LoadSettings()
    {
        _loading = true;

        string appearance =
            _settings.Values["Appearance"] as string ?? "Light";

        if (appearance == "System")
            appearance = "Light";

        bool notifications =
            _settings.Values["Notifications"] as bool? ?? true;

        string language =
            LocalizationService.CurrentLanguage;

        AppearanceComboBox.SelectedIndex = appearance switch
        {
            "Dark" => 1,
            _ => 0
        };

        NotificationsToggle.IsOn = notifications;

        LanguageComboBox.SelectedIndex =
            language == "Hrvatski" ? 1 : 0;

        ApplyAppearance(appearance);
        ApplyLanguage();

        _loading = false;
    }

    private void ApplyLanguage()
    {
        SettingsTitleText.Text =
            LocalizationService.Get("Settings");

        SettingsDescriptionText.Text =
            LocalizationService.Get("SettingsDescription");

        AppearanceTitleText.Text =
            LocalizationService.Get("Appearance");

        AppearanceDescriptionText.Text =
            LocalizationService.Get("AppearanceDescription");

        NotificationsTitleText.Text =
            LocalizationService.Get("Notifications");

        NotificationsDescriptionText.Text =
            LocalizationService.Get("NotificationsDescription");

        NotificationsToggle.Header =
            LocalizationService.Get("EnableNotifications");

        LanguageTitleText.Text =
            LocalizationService.Get("Language");

        LanguageDescriptionText.Text =
            LocalizationService.Get("LanguageDescription");

        AboutTitleText.Text =
            LocalizationService.Get("About");

        AboutDescriptionText.Text =
            LocalizationService.Get("AboutDescription");

        LightItem.Content =
            LocalizationService.Get("Light");

        DarkItem.Content =
            LocalizationService.Get("Dark");

        EnglishItem.Content =
            LocalizationService.Get("English");

        CroatianItem.Content =
            LocalizationService.Get("Croatian");

        SettingsSavedText.Text =
            LocalizationService.Get("SettingsSaved");
    }

    private void AppearanceComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_loading ||
            AppearanceComboBox.SelectedItem is not ComboBoxItem item)
            return;

        string appearance =
            item.Tag?.ToString()
            ?? item.Content?.ToString()
            ?? "Light";

        _settings.Values["Appearance"] = appearance;

        ApplyAppearance(appearance);

        ShowSavedMessage();
    }

    private void NotificationsToggle_Toggled(
        object sender,
        RoutedEventArgs e)
    {
        if (_loading)
            return;

        _settings.Values["Notifications"] =
            NotificationsToggle.IsOn;

        ShowSavedMessage();
    }

    private void LanguageComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_loading ||
            LanguageComboBox.SelectedItem is not ComboBoxItem item)
            return;

        string language =
            item.Tag?.ToString()
            ?? item.Content?.ToString()
            ?? "English";

        LocalizationService.CurrentLanguage =
            language;

        ApplyLanguage();

        ShowSavedMessage();
    }

    private void ApplyAppearance(string appearance)
    {
        if (App.MainWindow?.Content is not FrameworkElement root)
            return;

        root.RequestedTheme = appearance switch
        {
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Light
        };
    }

    private void ShowSavedMessage()
    {
        SettingsSavedText.Visibility =
            Visibility.Visible;
    }
}
