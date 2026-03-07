using UnityEngine;
using UnityEngine.Localization;

public class Door : MonoBehaviour, IInteractable
{
    public LocalizedString placeName;
    public SCENE_NAME targetSceneName;
    public SPAWN_ID targetSpawnId;
    public bool isExiting = false;

    public SoundData doorOpenSound;
    public SoundData doorLockedSound;

    public string GetPlaceName()
    {
        return placeName.GetLocalizedString();
    }


    public string GetInteractPrompt()
    {
        string key = isExiting ? "PROMPT_DOOR_EXIT" : "PROMPT_DOOR_ENTER";
        return LocalizationHelper.L(key, GetPlaceName());
    }

    public void OnInteract()
    {
        if(targetSceneName == SCENE_NAME.NONE || targetSpawnId == SPAWN_ID.NONE)
        {
            if(doorLockedSound.clip != null) SoundManager.Instance.PlaySFX(doorLockedSound, transform.position); 
            return;
        }

        if (isExiting && MyShopManager.Instance != null && MyShopManager.Instance.IsShopOpen)
        {
            NotificationUIManager.Instance.ShowNotification(LocalizationHelper.L("NOTI_CANNOT_EXIT_SHOP"));
            return;
        }

        if(doorOpenSound.clip != null) SoundManager.Instance.PlaySFX(doorOpenSound, transform.position);
        SceneController.Instance.AddSceneAndMoveTo(targetSceneName, targetSpawnId, isExiting);
    }
}

