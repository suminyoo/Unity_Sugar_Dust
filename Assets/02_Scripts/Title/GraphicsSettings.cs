using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;

public class GraphicsSettings : MonoBehaviour
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
        if (displayModeDropdown.options.Count < displayModeKeys.Length) return;

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

        int savedMode = PlayerPrefs.GetInt(SettingsConstants.PREF_DISPLAY_MODE, 0);
        displayModeDropdown.SetValueWithoutNotify(savedMode);
        displayModeDropdown.RefreshShownValue();
    }

    private void InitResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < SettingsConstants.Resolutions.Length; i++)
        {
            options.Add($"{SettingsConstants.Resolutions[i].x} x {SettingsConstants.Resolutions[i].y}");
        }
        resolutionDropdown.AddOptions(options);

        int savedRes = PlayerPrefs.GetInt(SettingsConstants.PREF_RESOLUTION, 0);
        if (savedRes < 0 || savedRes >= SettingsConstants.Resolutions.Length) savedRes = 0;

        resolutionDropdown.SetValueWithoutNotify(savedRes);
        resolutionDropdown.RefreshShownValue();
    }

    private void InitLanguageDropdown()
    {
        languageDropdown.ClearOptions();

        var options = new List<string>();
        var locales = LocalizationSettings.AvailableLocales.Locales;

        for (int i = 0; i < locales.Count; i++)
        {
            options.Add(locales[i].Identifier.CultureInfo != null ? locales[i].Identifier.CultureInfo.NativeName : locales[i].name);
        }
        languageDropdown.AddOptions(options);

        int savedLang = PlayerPrefs.GetInt(SettingsConstants.PREF_LANGUAGE, 0);
        if (savedLang < 0 || savedLang >= locales.Count) savedLang = 0;

        languageDropdown.SetValueWithoutNotify(savedLang);
        languageDropdown.RefreshShownValue();
    }

    public void SetLanguage(int index)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        PlayerPrefs.SetInt(SettingsConstants.PREF_LANGUAGE, index);
        PlayerPrefs.Save();
    }

    public void SetDisplayMode(int index)
    {
        Screen.fullScreenMode = SettingsConstants.GetFullScreenMode(index);
        PlayerPrefs.SetInt(SettingsConstants.PREF_DISPLAY_MODE, index);
        PlayerPrefs.Save();
    }

    public void SetResolution(int index)
    {
        Vector2Int res = SettingsConstants.Resolutions[index];
        Screen.SetResolution(res.x, res.y, Screen.fullScreenMode);
        PlayerPrefs.SetInt(SettingsConstants.PREF_RESOLUTION, index);
        PlayerPrefs.Save();
    }
}