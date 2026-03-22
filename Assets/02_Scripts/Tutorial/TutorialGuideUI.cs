using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialGuideUI : MonoBehaviour, ISaveable
{
    [Header("Guide Settings")]
    public GuideType guideType = GuideType.None;
    public bool autoOpenOnStart = true;

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

    private int _currentPageIndex = 0;

    private bool _isViewedThisSession = false;

    private void Start()
    {
        if (GameSaveManager.Instance != null)
        {
            if (GameSaveManager.Instance.IsTutorialCompleted())
            {
                if (guideRootCanvas != null) guideRootCanvas.SetActive(false);
                return;
            }

            if (autoOpenOnStart && !HasViewedGuideLocal())
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
        if (Input.GetKeyDown(KeyCode.Escape) && guideRootCanvas != null && guideRootCanvas.activeSelf)
        {
            OnClickClose();
        }
    }

    public void ToggleGuide()
    {
        if (guideRootCanvas == null) return;
        if (guideRootCanvas.activeSelf) OnClickClose();
        else OpenGuide();
    }

    public void OpenGuide()
    {
        if (guideRootCanvas == null || tutorialData == null || tutorialData.pages == null || tutorialData.pages.Length == 0) return;

        guideRootCanvas.SetActive(true);
        _currentPageIndex = 0;
        UpdateUI();

        MarkAsViewed();
    }

    // 외부에서 새로운 튜토리얼 데이터와 타입을 덮어씌우며 강제로 여는 함수
    public void OpenGuideWithData(TutorialDataSO newData, GuideType newGuideType)
    {
        this.tutorialData = newData;
        this.guideType = newGuideType;

        OpenGuide();
    }

    private bool HasViewedGuideLocal()
    {
        if (guideType == GuideType.None) return false;

        return _isViewedThisSession || GameSaveManager.Instance.HasViewedGuide(guideType);
    }

    private void MarkAsViewed()
    {
        if (guideType != GuideType.None && !HasViewedGuideLocal())
        {
            _isViewedThisSession = true;
        }
    }

    public void SaveData()
    {
        if (_isViewedThisSession && guideType != GuideType.None)
        {
            GameSaveManager.Instance.SaveViewedGuide(guideType);
        }
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