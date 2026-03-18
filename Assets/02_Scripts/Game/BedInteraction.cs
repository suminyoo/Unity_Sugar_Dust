using UnityEngine;

public class BedInteraction : MonoBehaviour, IInteractable
{
    public string GetInteractPrompt() => LocalizationHelper.Main("PROMPT_SLEEP");

    public void OnInteract()
    {
        CommonConfirmPopup.Instance.OpenPopup(
            LocalizationHelper.Main("CONFIRM_SLEEP"),
            () => {
                // 잠들기 전에 튜토리얼 퀘스트가 진행 중인지 확인
                Quest tuto10 = QuestManager.Instance.GetActiveQuest(QuestID.Tuto_10);
                if (tuto10 != null)
                {
                    // 튜토리얼 완료 저장
                    // GameSaveManager에 변수 추가하기

                    Debug.Log("튜토리얼 종료");
                }

                // 4. 원래 있던 수면 및 다음 날 이동 로직 실행
                GameManager.Instance.TrySleep();
            }
        );
    }

}