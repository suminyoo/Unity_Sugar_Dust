using System.Collections;
using System.Linq;
using UnityEngine;

public class TownTutorialManager : MonoBehaviour
{
    public static TownTutorialManager Instance;

    [Header("NPC Groups")]
    public GameObject normalNPCGroup;
    public GameObject tutorialNPCGroup;

    [Header("Tutorial Actors")]
    public NPCController parentNPC;
    public NPCController guideNPC;
    public NPCController curiousNPC;
    public NPCController weaponNPC;
    public NPCController spaceshipNPC;
    public NPCController landlordNPC;

    [Header("Quest Data")]
    public QuestData tuto_01;
    public QuestData tuto_07;


    [Header("Dialogue Data (교체)")]
    public DialogueData parentDialogue_Phase2;
    public DialogueData parentDialogue_Phase3;

    public DialogueData guideDialogue_Phase2;
   
    public DialogueData curiousDialogue_Phase2;
    public DialogueData weaponDialogue_Phase2;

    public DialogueData spaceshipDialogue_Phase2;
    public DialogueData spaceshipDialogue_Phase3;


    [Header("Scene Objects")]
    public GameObject shopBlockerCollider;
    public GameObject houseDoorCollider;

    [Header("Village Introduce")]
    public PathLookAtController townCamController;
    public DialogueData[] townDialogues;

    private bool isTownGuideFinished = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (GameSaveManager.Instance.savedData.isTutorialCompleted)
        {
            // 튜토리얼이 이미 끝났다면 일반 NPC 켜고 매니저 종료
            if (tutorialNPCGroup != null) tutorialNPCGroup.SetActive(false);
            if (normalNPCGroup != null) normalNPCGroup.SetActive(true);
            this.enabled = false;
            return;
        }


        if (normalNPCGroup != null) normalNPCGroup.SetActive(false);
        if (tutorialNPCGroup != null) tutorialNPCGroup.SetActive(true);

        // Tuto_01 퀘스트가 진행 중이거나 이미 완료되었는지 확인
        bool isTuto01Active = HasActiveQuest(QuestID.Tuto_01);
        bool isTuto01Completed = QuestManager.Instance.completedQuestIDs.Contains(QuestID.Tuto_01);

        if (!isTuto01Active && !isTuto01Completed)
        {
            StartCoroutine(StartPhaseWakeUp());
        }

        if (QuestManager.Instance.completedQuestIDs.Contains(QuestID.Tuto_06))
        {
            if (!HasActiveQuest(QuestID.Tuto_07) && !QuestManager.Instance.completedQuestIDs.Contains(QuestID.Tuto_07))
            {
                QuestManager.Instance.AddQuest(tuto_07);
                if (PlayerQuestUIManager.Instance != null)
                    PlayerQuestUIManager.Instance.ShowQuestAlert();
            }
        }
    }

    private void OnEnable()
    {
        GameEvents.OnNPCTalkedFinished += HandleNPCTalkedFinished;
        GameEvents.OnPointArrived += HandlePointArrived;
        GameEvents.OnQuestAccepted += HandleQuestAccepted;
        GameEvents.OnQuestProgressUpdated += CheckTuto06Completion;
    }

    private void OnDisable()
    {
        GameEvents.OnNPCTalkedFinished -= HandleNPCTalkedFinished;
        GameEvents.OnPointArrived -= HandlePointArrived;
        GameEvents.OnQuestAccepted -= HandleQuestAccepted;
        GameEvents.OnQuestProgressUpdated -= CheckTuto06Completion;
    }

    private IEnumerator StartPhaseWakeUp()
    {
        yield return null;
        QuestManager.Instance.AddQuest(tuto_01);

        //if (parentNPC == null)
        //{
        //    GameObject obj = GameObject.Find("TutoParent");
        //    if (obj != null) parentNPC = obj.GetComponent<NPCController>();
        //}

        PlayerQuestUIManager.Instance.ShowQuestAlert();
        

    }

    private void HandleNPCTalkedFinished(NPCID npcID)
    {
        if (npcID == NPCID.Guide && HasActiveQuest(QuestID.Tuto_02) && !isTownGuideFinished)
        {

            StartCoroutine(PlayGuideSequence());

        }
    }

    private void HandleQuestAccepted(QuestID questID)
    {

        if (questID == QuestID.Tuto_02)
        {
            if (houseDoorCollider != null) houseDoorCollider.SetActive(false);
        }
        if (questID == QuestID.Tuto_05)
        {
            if (guideNPC != null) guideNPC.gameObject.SetActive(false);

        }


    }


    //특정 장소에 도달했을 때
    private void HandlePointArrived(PointID pointID)
    {
        // 내 상점 내부에 진입했고 Tuto_09가 진행 중일 때
        if (pointID == PointID.MyShop && HasActiveQuest(QuestID.Tuto_09))
        {
            Debug.Log("상점 진입! 임대업자 NPC 퇴장 및 튜토리얼 UI 오픈");

            if (landlordNPC != null) landlordNPC.gameObject.SetActive(false);

            // TODO: 장사 안내서 UI 팝업 띄우기 
            // ShopTutorialUIManager.Instance.ShowManual();
        }
    }

    // 전용 연출 코루틴 (컷신/이동 등)

    private IEnumerator PlayGuideSequence()
    {
        isTownGuideFinished = true;
        InputControlManager.Instance.LockInput();

        townCamController.gameObject.SetActive(true);
        townCamController.vcam.Priority = 20;
        townCamController.ResetPath();

        for (int i = 0; i < townDialogues.Length; i++)
        {
            yield return StartCoroutine(townCamController.MoveToNextTarget());

            DialogueData currentData = townDialogues[i];

            if (currentData != null)
            {
                bool isDialogueFinished = false;

                DialogueManager.Instance.StartDialogue(
                    currentData,
                    "마을 안내 외계인",
                    () => { isDialogueFinished = true; }
                );
                yield return new WaitUntil(() => isDialogueFinished);
            }
        }

        townCamController.vcam.Priority = 5;
        townCamController.gameObject.SetActive(false);

        guideNPC.uniqueDialogue = guideDialogue_Phase2;

        InputControlManager.Instance.UnlockInput();
    }

    private IEnumerator MoveToExplorationMap()
    {
        InputControlManager.Instance.LockInput();

        yield return new WaitForSeconds(1.5f);

        // TODO: 탐사 맵 씬으로 이동하거나 플레이어를 텔레포트
        // SceneManager.LoadScene("ExplorationMap");

        InputControlManager.Instance.UnlockInput();

        // 탐사 맵 도착 직후 우주선 NPC의 탐사 맵 전용 대사나 시스템 안내를
        // 별도의 콜백으로
    }

    private bool HasActiveQuest(QuestID questID)
    {
        return QuestManager.Instance.GetActiveQuest(questID) != null;
    }

    private void CheckTuto06Completion()
    {
        if (HasActiveQuest(QuestID.Tuto_07) || QuestManager.Instance.completedQuestIDs.Contains(QuestID.Tuto_07))
            return;

        bool isTuto06Achieved = false;

        // 이미 보상까지 다 받았는지
        if (QuestManager.Instance.completedQuestIDs.Contains(QuestID.Tuto_06))
        {
            isTuto06Achieved = true;
        }
        else
        {
            // 진행 중인데 목표량은 다 채웠는지
            Quest tuto6 = QuestManager.Instance.GetActiveQuest(QuestID.Tuto_06);
            if (tuto6 != null && tuto6.IsAllObjectivesComplete())
            {
                isTuto06Achieved = true;
            }
        }

        // Tuto_07 강제 지급
        if (isTuto06Achieved && tuto_07 != null)
        {
            QuestManager.Instance.AddQuest(tuto_07);
            PlayerQuestUIManager.Instance.ShowQuestAlert();
            
        }
    }
}