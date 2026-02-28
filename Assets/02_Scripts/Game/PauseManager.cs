using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject saveLoadPanel;
    public GameObject optionPanel;

    private bool isPaused = false;

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
            "저장되지 않은 정보는 사라집니다.\n타이틀로 돌아가겠습니까?",
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
            "저장되지 않은 정보는 사라집니다.\n게임을 종료하겠습니까?",
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