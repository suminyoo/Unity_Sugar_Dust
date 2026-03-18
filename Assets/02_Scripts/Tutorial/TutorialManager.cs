using System.Collections;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

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
    public QuestData tuto02_Guide;
    public QuestData tuto03_Curious;

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

    [Header("Tutorial UI")]
    public GameObject tutorialGuideUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // TODO: 세이브 데이터 추가 해야함 게임 세이브 매니저
        bool isTutorialCompleted = false;

        if (isTutorialCompleted)
        {
            tutorialNPCGroup.SetActive(false);
            normalNPCGroup.SetActive(true);
            this.enabled = false; 
            return;
        }
        else
        {
            // 튜토리얼 진행
            //집에 있는 문 콜라이더 키기
            normalNPCGroup.SetActive(false);
            tutorialNPCGroup.SetActive(true);

            StartCoroutine(StartPhaseWakeUp());
        }
    }

    private void OnEnable()
    {
        GameEvents.OnNPCTalkedFinished += HandleNPCTalkedFinished;
        GameEvents.OnPointArrived += HandlePointArrived;
        GameEvents.OnQuestCompleted += HandleQuestCompleted;
        GameEvents.OnNPCInteractionStarted += UpdateNPCDialogueContext;
    }

    private void OnDisable()
    {
        GameEvents.OnNPCTalkedFinished -= HandleNPCTalkedFinished;
        GameEvents.OnPointArrived -= HandlePointArrived;
        GameEvents.OnQuestCompleted -= HandleQuestCompleted;
        GameEvents.OnNPCInteractionStarted -= UpdateNPCDialogueContext;
    }

    private IEnumerator StartPhaseWakeUp()
    {
        yield return new WaitForSeconds(1.0f);

        if (parentNPC == null)
        {
            GameObject obj = GameObject.Find("TutoParent");
            if (obj != null) parentNPC = obj.GetComponent<NPCController>();
        }

        tutorialGuideUI.SetActive(true);
    }

    private void HandleNPCTalkedFinished(NPCID npcID)
    {
        if (npcID == NPCID.Parent)
        {
            if (HasActiveQuest("Tuto_02"))
            {
                if (houseDoorCollider != null) houseDoorCollider.SetActive(false);
                tutorialGuideUI.SetActive(false);
            }
        }
        else if (npcID == NPCID.Guide && HasActiveQuest("Tuto_02"))
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
    }

    // 퀘스트 완료했을 때 대본 교체
    private void HandleQuestCompleted(QuestID questID)
    {
        switch (questID)
        {
            case QuestID.Tuto_04:
                Debug.Log("무기 장만 퀘스트 완료 무기상인, 호기심 NPC 대본 교체");
                weaponNPC.uniqueDialogue = weaponDialogue_Phase2;
                curiousNPC.uniqueDialogue = curiousDialogue_Phase2;
                break;

            case QuestID.Tuto_07:
                Debug.Log("수집품 보고 퀘스트 완료 우주선 NPC 대본 교체");
                spaceshipNPC.uniqueDialogue = spaceshipDialogue_Phase2;
                break;

            case QuestID.Tuto_08:
                shopBlockerCollider.SetActive(false);
                break;
        }
    }

    //특정 장소에 도달했을 때
    private void HandlePointArrived(PointID pointID)
    {
        // 내 상점 내부에 진입했고 Tuto_09가 진행 중일 때
        if (pointID == PointID.MyShop && HasActiveQuest("Tuto_09"))
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

    private bool HasActiveQuest(string questID)
    {
        return QuestManager.Instance.GetActiveQuest(questID) != null;
    }

    private void UpdateNPCDialogueContext(NPCController npc)
    {
        NPCID id = npc.npcData.npcID;

        switch (id)
        {
            case NPCID.Parent:
                if (QuestManager.Instance.completedQuestIDs.Contains("Tuto_09"))
                    npc.uniqueDialogue = parentDialogue_Phase3;
                else if (HasActiveQuest("Tuto_02"))
                    npc.uniqueDialogue = parentDialogue_Phase2;
                break;

            case NPCID.Guide:
                if (QuestManager.Instance.completedQuestIDs.Contains("Tuto_02"))
                {
                    npc.uniqueDialogue = guideDialogue_Phase2;
                }
                break;

            case NPCID.SpaceshipOwner:
                if (QuestManager.Instance.completedQuestIDs.Contains("Tuto_05"))
                    npc.uniqueDialogue = spaceshipDialogue_Phase2;
                break;

            case NPCID.Shopkeeper_Weapon:
                if (QuestManager.Instance.completedQuestIDs.Contains("Tuto_04"))
                    npc.uniqueDialogue = weaponDialogue_Phase2;
                break;
        }
    }
}