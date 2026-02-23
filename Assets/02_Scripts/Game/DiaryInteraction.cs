using UnityEngine;
using System.Linq;

public class DiaryInteraction : MonoBehaviour, IInteractable
{
    public string GetInteractPrompt() => "[E] 일기쓰기 (저장)";

    public void OnInteract()
    {
        CommonConfirmPopup.Instance.OpenPopup(
            "일기장에 현재까지의 진행 상황을 기록하시겠습니까?",
            () => {
                Debug.Log("저장중..");
                WriteDiary();
            }
        );
    }

    private void WriteDiary()
    {
        var saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
        foreach (var saveable in saveables)
        {
            saveable.SaveData();
        }
        GameSaveManager.Instance.SaveGameAtDiary();
    }
}