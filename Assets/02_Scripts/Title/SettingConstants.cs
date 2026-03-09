using UnityEngine;

public static class SettingsConstants
{
    // 해상도 배열
    public static readonly Vector2Int[] Resolutions = {
        new Vector2Int(1920, 1080), new Vector2Int(1600, 900),
        new Vector2Int(1280, 720), new Vector2Int(1920, 1200),
        new Vector2Int(1680, 1050)
    };

    // PlayerPrefs 키
    public const string PREF_DISPLAY_MODE = "DisplayModeIndex";
    public const string PREF_RESOLUTION = "ResolutionIndex";
    public const string PREF_LANGUAGE = "LanguageIndex";

    public const string PREF_MASTER_VOL = "MasterVolume";
    public const string PREF_BGM_VOL = "BGMVolume";
    public const string PREF_SFX_VOL = "SFXVolume";

    // 화면 모드 변환기
    public static FullScreenMode GetFullScreenMode(int index)
    {
        if (index == 0) return FullScreenMode.ExclusiveFullScreen;
        if (index == 1) return FullScreenMode.FullScreenWindow;
        return FullScreenMode.Windowed; // 기본값
    }
}