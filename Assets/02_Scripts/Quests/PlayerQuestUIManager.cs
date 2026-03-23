using System.Collections;
using UnityEngine;

public class PlayerQuestUIManager : MonoBehaviour
{
    public static PlayerQuestUIManager Instance;

    public GameObject playerQuestPanel;
    public Transform contentParent;
    public GameObject questSlotPrefab;
    public GameObject defaultPanel;

    [Header("Quest Notification Alert")]
    public GameObject questAlertIcon;
    public float pulseSpeed = 5f;
    public float pulseAmount = 0.2f;

    private Vector3 originalAlertScale;
    private Coroutine alertCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        playerQuestPanel.SetActive(false);

        // 느낌표 아이콘의 원래 크기를 저장해둡니다.
        if (questAlertIcon != null)
        {
            originalAlertScale = questAlertIcon.transform.localScale;
            questAlertIcon.SetActive(false); // 시작할 때는 꺼둠
        }
    }

    private void Update()
    {
        if (InputControlManager.Instance != null && InputControlManager.Instance.IsInputLocked) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleQuestPanel();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (playerQuestPanel.activeSelf)
            {
                CloseQuestPanel();
            }
        }
    }

    private void ToggleQuestPanel()
    {
        bool isActive = !playerQuestPanel.activeSelf;

        if (isActive)
        {
            playerQuestPanel.SetActive(true);
            QuestManager.Instance.RefreshAllQuestProgress();
            UpdateQuestUI();

            HideQuestAlert();
        }
        else
        {
            CloseQuestPanel();
        }
    }

    public void UpdateQuestUI()
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        var activeQuests = QuestManager.Instance.ActiveQuests;

        if (activeQuests == null || activeQuests.Count == 0)
        {
            defaultPanel.SetActive(true);
            return;
        }
        else
        {
            defaultPanel.SetActive(false);
        }

        foreach (Quest quest in activeQuests)
        {
            GameObject slotObj = Instantiate(questSlotPrefab, contentParent);
            QuestSlotUI slotUI = slotObj.GetComponent<QuestSlotUI>();

            if (quest.IsAllObjectivesComplete())
            {
                slotUI.SetupSlot(quest.data, quest, LocalizationHelper.Main("QUEST_CLAIM"), true, () => ClaimReward(quest));
            }
            else
            {
                slotUI.SetupSlot(quest.data, quest, LocalizationHelper.Main("QUEST_IN_PROGRESS"), false, null);
            }
        }
    }

    private void ClaimReward(Quest quest)
    {
        QuestManager.Instance.ClaimReward(quest);
        UpdateQuestUI();
    }

    public void CloseQuestPanel()
    {
        playerQuestPanel.SetActive(false);
        QuestManager.Instance.RefreshQuestAlertStatus();
        InputControlManager.Instance.UnlockInput();
    }

    // 새 퀘스트를 받거나 보상 조건이 달성되었을 때
    public void ShowQuestAlert()
    {
        if (playerQuestPanel.activeSelf || (questAlertIcon != null && questAlertIcon.activeSelf)) return;

        if (questAlertIcon != null)
        {
            questAlertIcon.SetActive(true);

            if (alertCoroutine != null) StopCoroutine(alertCoroutine);
            alertCoroutine = StartCoroutine(PulseAlertIcon());
        }
    }

    public void HideQuestAlert()
    {
        if (questAlertIcon != null)
        {
            if (alertCoroutine != null) StopCoroutine(alertCoroutine);
            questAlertIcon.transform.localScale = originalAlertScale;
            questAlertIcon.SetActive(false);
        }
    }

    private IEnumerator PulseAlertIcon()
    {
        while (true)
        {
            float scaleModifier = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

            questAlertIcon.transform.localScale = originalAlertScale + new Vector3(scaleModifier, scaleModifier, scaleModifier);

            yield return null;
        }
    }
}