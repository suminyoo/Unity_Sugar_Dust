using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialGuideUI : MonoBehaviour
{
    [Header("Tutorial Data")]
    public TutorialDataSO tutorialData;

    [Header("UI")]
    [SerializeField] private GameObject guideRootCanvas;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image contentImage;
    [SerializeField] private TextMeshProUGUI pageIndicatorText; 

    [Header("Navigation Buttons")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    [Header("Input Settings")]
    public KeyCode toggleKey = KeyCode.T;

    private int _currentPageIndex = 0;

    private void Start()
    {
        if (GameSaveManager.Instance != null && GameSaveManager.Instance.savedData != null)
        {
            if (!GameSaveManager.Instance.savedData.isTutorialCompleted)
            {
                OpenGuide();
            }
            else
            {
                if (guideRootCanvas != null) guideRootCanvas.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleGuide();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && guideRootCanvas != null && guideRootCanvas.activeSelf)
        {
            OnClickClose();
        }
    }

    public void ToggleGuide()
    {
        if (guideRootCanvas == null) return;

        if (guideRootCanvas.activeSelf)
        {
            OnClickClose();
        }
        else
        {
            OpenGuide();
        }
    }

    public void OpenGuide()
    {
        // 데이터가 없으면 열리지 않도록
        if (guideRootCanvas == null || tutorialData == null || tutorialData.pages == null || tutorialData.pages.Length == 0) return;

        guideRootCanvas.SetActive(true);
        _currentPageIndex = 0;
        UpdateUI();

    }

    public void OnClickNext()
    {
        if (tutorialData != null && _currentPageIndex < tutorialData.pages.Length - 1)
        {
            _currentPageIndex++;
            UpdateUI();
        }
    }

    public void OnClickPrev()
    {
        if (_currentPageIndex > 0)
        {
            _currentPageIndex--;
            UpdateUI();
        }
    }

    public void OnClickClose()
    {
        if (guideRootCanvas != null) guideRootCanvas.SetActive(false);

    }

    private void UpdateUI()
    {
        if (tutorialData == null || tutorialData.pages == null || tutorialData.pages.Length == 0) return;

        TutorialPage currentPage = tutorialData.pages[_currentPageIndex];

        if (titleText != null) titleText.text = currentPage.GetTitle();
        if (descriptionText != null) descriptionText.text = currentPage.GetDescription();
        if (contentImage != null) contentImage.sprite = currentPage.image;

        if (pageIndicatorText != null)
        {
            pageIndicatorText.text = $"{_currentPageIndex + 1} / {tutorialData.pages.Length}";
        }

        if (prevButton != null) prevButton.gameObject.SetActive(_currentPageIndex > 0);
        if (nextButton != null) nextButton.gameObject.SetActive(_currentPageIndex < tutorialData.pages.Length - 1);
    }
}