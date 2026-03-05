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

    private void Awake()
    {
        actionButton.onClick.AddListener(() => onButtonClicked?.Invoke());
    }

    // 슬롯 세팅
    public void SetupSlot(QuestData questData, Quest activeQuest, string btnText, bool isInteractable, Action onClickAction)
    {
        titleText.text = questData.questName;
        descriptionText.text = questData.description;
        rewardText.text = $"보상: {questData.rewardGold} {CustomerPaymentSystem.CURRENCY_SYMBOL}";


        if (questData.objectives.Count > 0)
        {
            QuestObjective firstObjective = questData.objectives[0];

            if (activeQuest == null)
            {
                progressText.text = firstObjective.GetProgressText(0);
            }
            else
            {
                int current = activeQuest.currentAmounts[0];
                progressText.text = firstObjective.GetProgressText(current);
            }
        }

        actionButtonText.text = btnText;
        actionButton.interactable = isInteractable;
        onButtonClicked = onClickAction;
    }
}