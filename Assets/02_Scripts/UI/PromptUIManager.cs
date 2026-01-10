using UnityEngine;
using TMPro;

public class PromptUIManager : MonoBehaviour
{
    public static PromptUIManager Instance;

    public GameObject promptPanel;
    public TextMeshProUGUI promptText;

    // 두 시스템의 요청저장
    private string currentInteractionText = "";
    private string currentActionText = "";

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // InteractionSystem
    public void SetInteractionPrompt(string text)
    {
        currentInteractionText = text;
        UpdatePrompt();
    }
    public void ClearInteractionPrompt()
    {
        currentInteractionText = "";
        UpdatePrompt();
    }

    // ActionSystem
    public void SetActionPrompt(string text)
    {
        currentActionText = text;
        UpdatePrompt();
    }
    public void ClearActionPrompt()
    {
        currentActionText = "";
        UpdatePrompt();
    }

    // 최종 결정 로직
    private void UpdatePrompt()
    {
        // 액션
        if (!string.IsNullOrEmpty(currentActionText))
        {
            ShowUI(currentActionText);
        }
        // 상호작용
        else if (!string.IsNullOrEmpty(currentInteractionText))
        {
            ShowUI(currentInteractionText);
        }
        // 둘 다 없으면 끄기
        else
        {
            HideUI();
        }
    }

    private void ShowUI(string text)
    {
        promptPanel.SetActive(true);
        promptText.text = text;
    }

    private void HideUI()
    {
        promptPanel.SetActive(false);
    }
}