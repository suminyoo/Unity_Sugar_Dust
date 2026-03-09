using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;

public class GameSettingsLoader : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;
        LoadSavedSettings();
    }

    private void LoadSavedSettings()
    {
        int savedModeIndex = PlayerPrefs.GetInt(SettingsConstants.PREF_DISPLAY_MODE, 0);
        FullScreenMode mode = SettingsConstants.GetFullScreenMode(savedModeIndex);

        int savedResIndex = PlayerPrefs.GetInt(SettingsConstants.PREF_RESOLUTION, 0);
        if (savedResIndex < 0 || savedResIndex >= SettingsConstants.Resolutions.Length) savedResIndex = 0;

        Vector2Int res = SettingsConstants.Resolutions[savedResIndex];
        Screen.SetResolution(res.x, res.y, mode);

        int savedLangIndex = PlayerPrefs.GetInt(SettingsConstants.PREF_LANGUAGE, 0);
        var locales = LocalizationSettings.AvailableLocales.Locales;

        if (savedLangIndex >= 0 && savedLangIndex < locales.Count)
        {
            LocalizationSettings.SelectedLocale = locales[savedLangIndex];
        }
    }
}