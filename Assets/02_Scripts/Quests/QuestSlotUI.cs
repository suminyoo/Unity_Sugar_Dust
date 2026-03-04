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
        rewardText.text = $"보상: {questData.rewardGold} G";

        if (activeQuest == null)
        {
            // 아직 미수락 상태
            int max = questData.objectives.Count > 0 ? questData.objectives[0].requiredAmount : 0;
            progressText.text = $"목표 수량: {max}";
        }
        else
        {
            // 진행 중 상태
            if (activeQuest.data.objectives.Count > 0)
            {
                int current = activeQuest.currentAmounts[0];
                int max = activeQuest.data.objectives[0].requiredAmount;
                progressText.text = $"진행도: {current} / {max}";
            }
        }

        actionButtonText.text = btnText;
        actionButton.interactable = isInteractable;
        onButtonClicked = onClickAction;
    }
}