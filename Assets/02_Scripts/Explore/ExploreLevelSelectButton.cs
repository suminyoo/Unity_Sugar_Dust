using UnityEngine;
using UnityEngine.UI;

public class ExploreLevelSelectButton : MonoBehaviour
{
    public int levelNumber;
    [SerializeField] private Button button;
    [SerializeField] private ExploreSelectionUI selectionUI;

    private ColorBlock originalColors;

    private void Awake()
    {
        if (button != null)
        {
            originalColors = button.colors;
        }
    }

    private void OnEnable()
    {
        if (selectionUI != null)
        {
            selectionUI.OnLevelSelectedEvent -= OnLevelSelected;
            selectionUI.OnLevelSelectedEvent += OnLevelSelected;
        }

        if (button != null)
        {
            // 잠금 해제 체크
            bool isUnlocked = false;
            if (GameSaveManager.Instance != null)
            {
                int maxUnlocked = GameSaveManager.Instance.LoadExploreMaxUnlockedLevel();
                isUnlocked = (levelNumber <= maxUnlocked);
            }
            button.interactable = isUnlocked;

            // 처음 켜질 때 현재 선택된 레벨표시
            if (selectionUI != null)
            {
                bool isCurrentlySelected = (selectionUI.selectedLevelNumber == this.levelNumber);
                UpdateSelectedVisuals(isCurrentlySelected);
            }
        }
    }

    private void OnDisable()
    {
        if (selectionUI != null)
        {
            selectionUI.OnLevelSelectedEvent -= OnLevelSelected;
        }
    }

    private void Start()
    {
        if (button != null && selectionUI != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => selectionUI.OnSelectLevel(levelNumber));
        }
    }

    private void OnLevelSelected(int selectedLevel)
    {
        bool isCurrentlySelected = (this.levelNumber == selectedLevel);
        UpdateSelectedVisuals(isCurrentlySelected);
    }

    private void UpdateSelectedVisuals(bool isSelected)
    {
        if (button == null) return;

        ColorBlock cb = originalColors;

        if (isSelected)
        {
            // 선택된 상태
            cb.normalColor = originalColors.selectedColor;
            cb.highlightedColor = originalColors.selectedColor;
        }
        else
        {
            // 선택되지 않은 상태
            cb.normalColor = originalColors.normalColor;
            cb.highlightedColor = originalColors.highlightedColor;
        }

        button.colors = cb;
    }
}