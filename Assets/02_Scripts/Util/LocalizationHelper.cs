using UnityEngine.Localization.Settings;

public static class LocalizationHelper
{
    public static string LANGUAGE_TABLE_NAME = "GameLanguageTable";


    public static string L(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(
            LANGUAGE_TABLE_NAME,
            key
        );
    }

    public static string L(string key, params object[] args)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(
            LANGUAGE_TABLE_NAME,
            key,
            args
        );
    }

    public static string GetGameTimeText(GAME_TIME time)
    {
        return L($"TIME_{time.ToString().ToUpper()}");
    }
}