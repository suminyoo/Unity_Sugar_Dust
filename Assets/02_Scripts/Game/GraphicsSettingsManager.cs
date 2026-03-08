using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;

public class GraphicsSettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown displayModeDropdown;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown languageDropdown;

    private readonly string[] displayModeKeys = {
        "DISPLAY_MODE_FULLSCREEN",
        "DISPLAY_MODE_BORDERLESS",
        "DISPLAY_MODE_WINDOWED"
    };

    private readonly Vector2Int[] resolutions = {
        new Vector2Int(1920, 1080), new Vector2Int(1600, 900),
        new Vector2Int(1280, 720), new Vector2Int(1920, 1200),
        new Vector2Int(1680, 1050)
    };

    void Start()
    {
        InitLanguageDropdown();
        InitDisplayModeDropdown();
        InitResolutionDropdown();

        displayModeDropdown.onValueChanged.AddListener(SetDisplayMode);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        languageDropdown.onValueChanged.AddListener(SetLanguage);
    }

    void OnEnable() { LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged; }
    void OnDisable() { LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged; }

    private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        UpdateLocalizedTexts();
    }

    private void UpdateLocalizedTexts()
    {
        for (int i = 0; i < displayModeKeys.Length; i++)
        {
            displayModeDropdown.options[i].text = LocalizationHelper.L(displayModeKeys[i]);
        }
        displayModeDropdown.RefreshShownValue();
    }


    private void InitDisplayModeDropdown()
    {
        displayModeDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < displayModeKeys.Length; i++)
        {
            options.Add(LocalizationHelper.L(displayModeKeys[i]));
        }
        displayModeDropdown.AddOptions(options);

        switch (Screen.fullScreenMode)
        {
            case FullScreenMode.ExclusiveFullScreen: displayModeDropdown.value = 0; break;
            case FullScreenMode.FullScreenWindow: displayModeDropdown.value = 1; break;
            case FullScreenMode.Windowed: displayModeDropdown.value = 2; break;
        }
        displayModeDropdown.RefreshShownValue();
    }

    private void InitResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add($"{resolutions[i].x} x {resolutions[i].y}");

            if (Screen.width == resolutions[i].x && Screen.height == resolutions[i].y)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void InitLanguageDropdown()
    {
        languageDropdown.ClearOptions();

        var options = new List<string>();
        int selectedIndex = 0;
        var locales = LocalizationSettings.AvailableLocales.Locales;

        for (int i = 0; i < locales.Count; i++)
        {
            options.Add(locales[i].Identifier.CultureInfo != null ? locales[i].Identifier.CultureInfo.NativeName : locales[i].name);
            if (LocalizationSettings.SelectedLocale == locales[i]) selectedIndex = i;
        }

        languageDropdown.AddOptions(options);
        languageDropdown.value = selectedIndex;
        languageDropdown.RefreshShownValue();
    }

    public void SetLanguage(int index)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }

    public void SetDisplayMode(int index)
    {
        FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;
        if (index == 0) mode = FullScreenMode.ExclusiveFullScreen;
        else if (index == 1) mode = FullScreenMode.FullScreenWindow;
        else if (index == 2) mode = FullScreenMode.Windowed;

        Screen.fullScreenMode = mode;
    }

    public void SetResolution(int index)
    {
        Screen.SetResolution(resolutions[index].x, resolutions[index].y, Screen.fullScreenMode);
    }
}