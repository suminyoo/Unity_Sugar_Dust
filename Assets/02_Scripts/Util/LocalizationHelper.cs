using UnityEngine.Localization.Settings;

public static class LocalizationHelper
{
    public const string TABLE_MAIN = "GameMainTable";
    public const string TABLE_DIALOGUE = "DialogueTable";
    public const string TABLE_QUEST = "QuestTable";
    public const string TABLE_ITEM = "ItemTable";
    public const string TABLE_TUTORIAL = "TutorialTable";

    private static string Get(string tableName, string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
    }

    private static string Get(string tableName, string key, params object[] args)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key, args);
    }


    // GameMainTable
    public static string Main(string key) => Get(TABLE_MAIN, key);
    public static string Main(string key, params object[] args) => Get(TABLE_MAIN, key, args);

    // DialogueTable
    public static string Talk(string key) => Get(TABLE_DIALOGUE, key);
    public static string Talk(string key, params object[] args) => Get(TABLE_DIALOGUE, key, args);

    // QuestTable
    public static string Quest(string key) => Get(TABLE_QUEST, key);
    public static string Quest(string key, params object[] args) => Get(TABLE_QUEST, key, args);

    //Item Table
    public static string Item(string key) => Get(TABLE_ITEM, key);
    public static string Item(string key, params object[] args) => Get(TABLE_ITEM, key, args);

    //Tutorial Table
    public static string Tuto(string key) => Get(TABLE_TUTORIAL, key);
    public static string Tuto(string key, params object[] args) => Get(TABLE_TUTORIAL, key, args);

    public static string GetGameTimeText(GAME_TIME time)
    {
        return Main($"TIME_{time.ToString().ToUpper()}");
    }

}