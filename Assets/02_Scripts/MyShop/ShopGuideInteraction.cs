using UnityEngine;

public class ShopGuideInteraction : MonoBehaviour, IInteractable
{
    public TutorialDataSO shopTutorialData;

    public string GetInteractPrompt() => LocalizationHelper.Main("PROMPT_READ_GUIDE");

    public void OnInteract()
    {
        TutorialGuideUI survivingUI = FindObjectOfType<TutorialGuideUI>(true);

        if (survivingUI != null && shopTutorialData != null)
        {
            survivingUI.OpenGuideWithData(shopTutorialData, GuideType.Shop);
        }
    }
}