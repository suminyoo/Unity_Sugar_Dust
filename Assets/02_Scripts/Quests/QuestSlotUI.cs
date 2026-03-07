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
        rewardText.text = LocalizationHelper.L(
            "UI_QUEST_REWARD_GOLD",
            questData.rewardGold,
            CustomerPaymentSystem.CURRENCY_SYMBOL
        );

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
            QuestObjective questobjective = questData.objectives[0];
            int current = (activeQuest == null) ? 0 : activeQuest.currentAmounts[0];

            progressText.text = questobjective.GetProgressText(current);
            progressText.color = textColor;
        }

        actionButtonText.text = btnText;
        actionButton.interactable = isInteractable;
        onButtonClicked = onClickAction;

        actionButton.GetComponent<Image>().color = buttonColor;
    }
}