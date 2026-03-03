using UnityEngine;

public class DiaryInteraction : MonoBehaviour, IInteractable
{
    public SoundData diaryOpenSound;
    public string GetInteractPrompt() => "[E] 일기쓰기 (저장)";

    public void OnInteract()
    {
        if(diaryOpenSound.clip != null) SoundManager.Instance.PlaySFX(diaryOpenSound, transform.position);

        CommonConfirmPopup.Instance.OpenPopup(
            "일기장에 현재까지의 진행 상황을 기록하시겠습니까?",
            () => {
                Debug.Log("저장중..");
                GameSaveManager.Instance.SaveCurrentGame();
            }
        );
    }

}