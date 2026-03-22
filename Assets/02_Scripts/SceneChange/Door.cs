using UnityEngine;
using UnityEngine.Localization;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public LocalizedString placeName;
    public SCENE_NAME targetSceneName;
    public SPAWN_ID targetSpawnId;
    public bool isExiting = false;

    [Header("Audio")]
    public SoundData doorOpenSound;
    public SoundData doorLockedSound;

    [Header("Locks")]
    public TutorialLockType tutorialLock = TutorialLockType.None;
    public TimeLockType timeLock = TimeLockType.None;
    public AccessLockType accessLock = AccessLockType.None;

    public string GetInteractPrompt()
    {
        string key = isExiting ? "PROMPT_DOOR_EXIT" : "PROMPT_DOOR_ENTER";

        string nameText = "";

        if (placeName != null && !placeName.IsEmpty)
        {
            try
            {
                nameText = placeName.GetLocalizedString();
            }
            catch (System.Exception)
            {
                nameText = "";
            }
        }
        string finalPrompt = LocalizationHelper.Main(key, nameText);

        return finalPrompt.Trim();
    }

    public void OnInteract()
    {
        if (!InteractionValidator.CanInteract(tutorialLock, timeLock, accessLock, out string rejectMsg))
        {
            if (doorLockedSound.clip != null) SoundManager.Instance.PlaySFX(doorLockedSound, transform.position);
            NotificationUIManager.Instance.ShowNotification(rejectMsg);
            return;
        }

        if (targetSceneName == SCENE_NAME.NONE || targetSpawnId == SPAWN_ID.NONE)
        {
            if (doorLockedSound.clip != null) SoundManager.Instance.PlaySFX(doorLockedSound, transform.position);
            return;
        }

        if (isExiting && MyShopManager.Instance != null && MyShopManager.Instance.IsShopOpen)
        {
            NotificationUIManager.Instance.ShowNotification(LocalizationHelper.Main("NOTI_CANNOT_EXIT_SHOP"));
            return;
        }

        if (doorOpenSound.clip != null) SoundManager.Instance.PlaySFX(doorOpenSound, transform.position);
        SceneController.Instance.AddSceneAndMoveTo(targetSceneName, targetSpawnId, isExiting);
    }
}