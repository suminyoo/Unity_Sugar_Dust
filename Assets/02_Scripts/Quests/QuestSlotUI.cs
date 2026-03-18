using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class QuestSlotUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI rewardText;

    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    private Action onButtonClicked;

    public Color objectiveTextOriginColor;
    public Color objectiveCompletedColor;
    public Color objectiveIncompleteColor;

    public Color actionButtonOriginColor;
    public Color actionButtonCompletedColor;
    public Color actionButtonImcompletedColor;

    private void Awake()
    {
        actionButton.onClick.AddListener(() => onButtonClicked?.Invoke());
    }

    // 슬롯 세팅
    public void SetupSlot(QuestData questData, Quest activeQuest, string btnText, bool isInteractable, Action onClickAction)
    {
        titleText.text = questData.GetQuestName();
        descriptionText.text = questData.GetDescription();
        string fullRewardText = "";

        // 골드 보상
        if (questData.rewardGold > 0)
        {
            fullRewardText += LocalizationHelper.Main("QUEST_REWARD", questData.rewardGold, CustomerPaymentSystem.CURRENCY_SYMBOL) + "\n";
        }

        // 아이템 보상
        if (questData.rewardItems != null && questData.rewardItems.Count > 0)
        {
            foreach (var itemReward in questData.rewardItems)
            {
                var itemData = ItemDataManager.Instance.GetItemByID(itemReward.itemID.ToString());
                if (itemData != null)
                {
                    fullRewardText += LocalizationHelper.Main("QUEST_REWARD", itemData.GetItemName(), $" x{itemReward.amount}") + "\n";
                }
            }
        }
        rewardText.text = fullRewardText.TrimEnd();

        Color textColor;
        Color buttonColor;

        // 수락 여부에 따른 색상 결정
        if (activeQuest == null)
        {
            textColor = objectiveTextOriginColor;
            buttonColor = actionButtonOriginColor;
        }
        else
        {
            bool isComplete = activeQuest.IsAllObjectivesComplete();
            textColor = isComplete ? objectiveCompletedColor : objectiveIncompleteColor;
            buttonColor = isComplete ? actionButtonCompletedColor : actionButtonImcompletedColor;
        }

        // 목표 텍스트 및 진행도 설정
        if (questData.objectives.Count > 0)
        {
            string fullProgressText = "";

            for (int i = 0; i < questData.objectives.Count; i++)
            {
                QuestObjective obj = questData.objectives[i];
                int current = (activeQuest == null) ? 0 : activeQuest.currentAmounts[i];

                fullProgressText += obj.GetProgressText(current) + "\n";
            }

            progressText.text = fullProgressText.TrimEnd();
            progressText.color = textColor;
        }
        else
        {
            progressText.text = "";
        }

        actionButtonText.text = btnText;
        actionButton.interactable = isInteractable;
        onButtonClicked = onClickAction;

        actionButton.GetComponent<Image>().color = buttonColor;
    }
}