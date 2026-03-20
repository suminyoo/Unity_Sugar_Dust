using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewTutorialData", menuName = "Tutorial/Tutorial Data")]
public class TutorialDataSO : ScriptableObject
{
    public TutorialPage[] pages;
}

[System.Serializable]
public class TutorialPage
{
    public LocalizedString title;
    public LocalizedString description;
    public Sprite image;

    public string GetTitle()
    {
        return title != null ? title.GetLocalizedString() : "";
    }

    public string GetDescription()
    {
        return description != null ? description.GetLocalizedString() : "";
    }
}
