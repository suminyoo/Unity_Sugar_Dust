using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ExploreSelectionUI : MonoBehaviour
{
    public List<ExploreStageData> stageProfiles;
    public int levelsPerStageData = 15;
    public int levelsPerEnvironment = 30;
    public float defaultTimeLimit = 300f;


    [Header("Environment Selection")]
    [SerializeField] private Button[] environmentButtons;
    [SerializeField] private GameObject[] mapImages;

    [Header("Info Panel")]
    [SerializeField] private GameObject exploreLevelSelectPanel;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI timeLimitText;
    [SerializeField] private Button startExplorationButton;

    private int selectedLevelNumber = -1;

    public void OpenPanel()
    {
        exploreLevelSelectPanel.SetActive(true);
        RefreshEnvironmentButtons();

        foreach (var map in mapImages) map.SetActive(false);
        infoPanel.SetActive(false);
        startExplorationButton.interactable = false;
        selectedLevelNumber = -1;
    }

    private void RefreshEnvironmentButtons()
    {
        int maxUnlocked = GameSaveManager.Instance.LoadExploreMaxUnlockedLevel();

        for (int i = 0; i < environmentButtons.Length; i++)
        {
            int envStartLevel = (i * levelsPerEnvironment) + 1;
            environmentButtons[i].interactable = maxUnlocked >= envStartLevel;
        }
    }

    public void OnClickEnvironment(int index)
    {
        foreach (var map in mapImages) map.SetActive(false);
        if (index < mapImages.Length)
        {
            mapImages[index].SetActive(true);
        }

        infoPanel.SetActive(false);
        startExplorationButton.interactable = false;
    }

    public void OnSelectLevel(int levelNum)
    {
        int maxUnlocked = GameSaveManager.Instance.LoadExploreMaxUnlockedLevel();

        if (levelNum > maxUnlocked)
        {
            NotificationUIManager.Instance.ShowNotification("아직 갈 수 없는 구역입니다.");
            return;
        }

        selectedLevelNumber = levelNum;
        ExploreStageData data = GetStageDataForLevel(levelNum);

        if (data != null)
        {
            infoPanel.SetActive(true);
            int localLevel = (levelNum - 1) % levelsPerStageData + 1;

            levelNameText.text = $"{data.stageName} {localLevel:00}";
            timeLimitText.text = $"제한 시간: {defaultTimeLimit}초";

            startExplorationButton.interactable = true;
        }
    }
    private ExploreStageData GetStageDataForLevel(int level)
    {
        int index = (level - 1) / levelsPerStageData;

        if (stageProfiles != null && index < stageProfiles.Count)
            return stageProfiles[index];

        if (stageProfiles.Count > 0)
            return stageProfiles[stageProfiles.Count - 1];

        return null;
    }

    public void OnExploreStart()
    {
        if (selectedLevelNumber > 0)
        {
            CommonConfirmPopup.Instance.OpenPopup(
                $"{selectedLevelNumber:00} 구역 탐사를 시작하시겠습니까?",
                () => {
                    GameSaveManager.Instance.SaveSelectedExploreLevel(selectedLevelNumber);
                    GameManager.Instance.StartExploration(selectedLevelNumber);
                }
            );
        }
    }

    public void ClosePanel() => exploreLevelSelectPanel.SetActive(false);
}