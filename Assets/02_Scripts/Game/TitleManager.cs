using UnityEngine;

public class TitleManager : MonoBehaviour
{
    public GameObject loadPanel;
    public GameObject optionsPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (loadPanel.activeSelf || optionsPanel.activeSelf)
            {
                BackToTitle();
            }
        }
    }

    public void OnClickStart() => loadPanel.SetActive(true);
    
    public void OnClickOption() => optionsPanel.SetActive(true);

    public void OnClickExit() => Application.Quit();

    public void BackToTitle()
    {
        loadPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }
}