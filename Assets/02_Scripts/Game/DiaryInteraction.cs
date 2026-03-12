using UnityEngine;

public class DiaryInteraction : MonoBehaviour, IInteractable
{
    public SoundData diaryOpenSound;
    public string GetInteractPrompt() => LocalizationHelper.Main("PROMPT_WRITE_DIARY");
    public void OnInteract()
    {
        if(diaryOpenSound.clip != null) SoundManager.Instance.PlaySFX(diaryOpenSound, transform.position);

        CommonConfirmPopup.Instance.OpenPopup(
            LocalizationHelper.Main("CONFIRM_SAVE_GAME"),
            () => {
                GameSaveManager.Instance.SaveCurrentGame();
            }
        );
    }

}