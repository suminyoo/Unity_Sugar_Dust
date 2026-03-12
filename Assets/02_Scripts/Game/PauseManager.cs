using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static int openPopupCount = 0;

    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject saveLoadPanel;
    public GameObject optionPanel;

    private bool isPaused = false;

    public SaveLoadUIManager saveLoadManager;

    private void Start()
    {
        CloseAllPanels();
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == SCENE_NAME.TITLE.ToString()) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                if (openPopupCount > 0) return;
                PauseGame();
            }
            else
            {
                if (saveLoadPanel.activeSelf || optionPanel.activeSelf)
                {
                    BackToPauseMenu();
                }
                else if (pausePanel.activeSelf)
                {
                    ResumeGame();
                }
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        GameManager.Instance.PauseGameTime();

        CloseAllPanels();
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        GameManager.Instance.ResumeGameTime();

        CloseAllPanels();
    }

    public void OpenLoadUI()
    {
        pausePanel.SetActive(false);
        saveLoadPanel.SetActive(true);

        if (saveLoadManager != null)
        {
            saveLoadManager.RefreshSlots();
        }
    }

    public void OpenOption()
    {
        pausePanel.SetActive(false);
        optionPanel.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        CloseAllPanels();
        pausePanel.SetActive(true);
    }

    public void GoToTitle()
    {
        CommonConfirmPopup.Instance.OpenPopup(
            $"{LocalizationHelper.Main("CONFIRM_UNSAVED_WARNING")}\n{LocalizationHelper.Main("CONFIRM_GO_TITLE")}",
            () => {
                GameManager.Instance.ResumeGameTime();

                GameSaveManager.Instance.SetTimerActive(false);
                SceneManager.LoadScene(SCENE_NAME.TITLE.ToString());
            }
        );

    }

    public void ExitGame()
    {
        CommonConfirmPopup.Instance.OpenPopup(
            $"{LocalizationHelper.Main("CONFIRM_UNSAVED_WARNING")}\n{LocalizationHelper.Main("CONFIRM_EXIT_GAME")}",
            () => {
                Application.Quit();
            }
        );
    }

    private void CloseAllPanels()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (saveLoadPanel != null) saveLoadPanel.SetActive(false);
        if (optionPanel != null) optionPanel.SetActive(false);
    }
}