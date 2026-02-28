using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ExploreSelectionUI : MonoBehaviour
{
    public event Action<int> OnLevelSelectedEvent;

    public ExploreConfigData exploreConfig;

    [Header("Environment Selection")]
    [SerializeField] private Button[] environmentButtons;
    [SerializeField] private GameObject[] mapImages;

    [Header("Info Panel")]
    [SerializeField] private GameObject exploreLevelSelectPanel;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI timeLimitText;
    [SerializeField] private Button startExplorationButton;

    public int selectedLevelNumber = -1;

    private void Update()
    {
        if (exploreLevelSelectPanel != null && exploreLevelSelectPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePanel();
            }
        }
    }

    public void OpenPanel()
    {
        exploreLevelSelectPanel.SetActive(true);
        RefreshEnvironmentButtons();

        int maxUnlocked = GameSaveManager.Instance.LoadExploreMaxUnlockedLevel();

        if (maxUnlocked < 0) maxUnlocked = 0;

        int mapIndex = maxUnlocked / exploreConfig.levelsPerEnvironment;

        if (mapImages.Length > 0)
        {
            mapIndex = Mathf.Clamp(mapIndex, 0, mapImages.Length - 1);

            OnClickEnvironment(mapIndex);
        }
    }

    private void RefreshEnvironmentButtons()
    {
        int maxUnlocked = GameSaveManager.Instance.LoadExploreMaxUnlockedLevel();

        for (int i = 0; i < environmentButtons.Length; i++)
        {
            int envStartLevel = (i * exploreConfig.levelsPerEnvironment);
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

        int firstLevelOfEnv = (index * exploreConfig.levelsPerEnvironment);
        OnSelectLevel(firstLevelOfEnv);
    }

    public void OnSelectLevel(int levelNum)
    {
        ExploreStageData data = exploreConfig.GetStageData(levelNum);

        if (data != null)
        {
            infoPanel.SetActive(true);
            int localLevel = exploreConfig.GetLocalLevel(levelNum);
            int minutes = (int)data.timeLimit / 60;
            int seconds = (int)data.timeLimit % 60;

            levelNameText.text = $"{data.stageName} {localLevel:00} 구역";
            timeLimitText.text = $"일몰까지 예상시간 {minutes:D2}:{seconds:D2}";

            selectedLevelNumber = levelNum;

            startExplorationButton.interactable = true;

            OnLevelSelectedEvent?.Invoke(levelNum);
        }
    }

    public void OnExploreStart()
    {
        if (selectedLevelNumber >= 0)
        {
            ExploreStageData data = exploreConfig.GetStageData(selectedLevelNumber);
            int localLevel = exploreConfig.GetLocalLevel(selectedLevelNumber);

            CommonConfirmPopup.Instance.OpenPopup(
                $"{data.stageName} {localLevel:00} 구역 탐사를 시작하시겠습니까?",
                () => {
                    GameSaveManager.Instance.SaveSelectedExploreLevel(selectedLevelNumber);
                    GameManager.Instance.StartExploration(selectedLevelNumber);
                }
            );
        }
    }

    public void ClosePanel() => exploreLevelSelectPanel.SetActive(false);
}