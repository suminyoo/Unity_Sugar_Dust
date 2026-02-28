using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExploreLevelSelectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int levelNumber;
    [SerializeField] private Button button;
    [SerializeField] private ExploreSelectionUI selectionUI;
    [SerializeField] private Image childIconImage;

    private Color originalColor;
    private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    private Color highlightColor = Color.white;

    private bool isUnlocked = false;
    private bool isSelected = false;

    private void Awake()
    {
        if (childIconImage != null)
        {
            originalColor = childIconImage.color;
        }
    }

    private void OnEnable()
    {
        if (selectionUI != null && selectionUI.exploreConfig != null)
        {
            selectionUI.OnLevelSelectedEvent -= OnLevelSelected;
            selectionUI.OnLevelSelectedEvent += OnLevelSelected;

            if (button != null && GameSaveManager.Instance != null)
            {
                int maxUnlocked = GameSaveManager.Instance.LoadExploreMaxUnlockedLevel();

                isUnlocked = levelNumber <= maxUnlocked;
                button.interactable = isUnlocked;
            }
            isSelected = (selectionUI.selectedLevelNumber == this.levelNumber);

            UpdateColor();
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
        isSelected = (this.levelNumber == selectedLevel);
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (childIconImage == null) return;

        if (!isUnlocked)
        {
            childIconImage.color = disabledColor;
        }
        else if (isSelected)
        {
            childIconImage.color = highlightColor;
        }
        else
        {
            childIconImage.color = originalColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isUnlocked && !isSelected && childIconImage != null)
        {
            childIconImage.color = highlightColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isUnlocked && !isSelected && childIconImage != null)
        {
            childIconImage.color = originalColor;
        }
    }
}