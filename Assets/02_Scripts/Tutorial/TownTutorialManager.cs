using System.Collections;
using UnityEngine;
using System.Linq;
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
    public QuestData tuto_10;

    [Header("Dialogue Data (교체)")]
    public DialogueData parentDialogue_Phase2;
    public DialogueData parentDialogue_Phase3;

    public DialogueData guideDialogue_Phase2;
   
    public DialogueData curiousDialogue_Phase2;
    public DialogueData weaponDialogue_Phase2;

    public DialogueData spaceshipDialogue_Phase2;
    public DialogueData spaceshipDialogue_Phase3;

    [Header("Spaceship Tuto 06 Dialogues")]
    public DialogueData spaceship_FailNight;
    public DialogueData spaceship_ReadyDay;

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


        SyncEnvironmentState();

        // Tuto_01 퀘스트가 진행 중이거나 이미 완료되었는지 확인
        bool isTuto01Active = HasActiveQuest(QuestID.Tuto_01);
        bool isTuto01Completed = QuestManager.Instance.CompletedQuestIDs.Contains(QuestID.Tuto_01);

        if (!isTuto01Active && !isTuto01Completed)
        {
            StartCoroutine(StartPhaseWakeUp());
        }

        // 06번 퀘스트 관련
        if (QuestManager.Instance.CompletedQuestIDs.Contains(QuestID.Tuto_06))
        {
            if (!HasActiveQuest(QuestID.Tuto_07) && !QuestManager.Instance.CompletedQuestIDs.Contains(QuestID.Tuto_07))
            {
                QuestManager.Instance.AddQuest(tuto_07);
                if (PlayerQuestUIManager.Instance != null)
                    PlayerQuestUIManager.Instance.ShowQuestAlert();
            }
        }

        // 09번 퀘스트 관련
        if (QuestManager.Instance.IsQuestAchieved(QuestID.Tuto_09))
        {
            if (!HasActiveQuest(QuestID.Tuto_10) && !QuestManager.Instance.CompletedQuestIDs.Contains(QuestID.Tuto_10))
            {
                if (tuto_10 != null)
                {
                    QuestManager.Instance.AddQuest(tuto_10);
                    if (PlayerQuestUIManager.Instance != null) PlayerQuestUIManager.Instance.ShowQuestAlert();
                }
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


    private void SyncEnvironmentState()
    {
        // 가이드 NPC 퇴장 동기화 (Tuto_05를 받았거나 완료했다면 끈다)
        if (HasActiveQuest(QuestID.Tuto_05) || QuestManager.Instance.CompletedQuestIDs.Contains(QuestID.Tuto_05))
        {
            if (guideNPC != null) guideNPC.gameObject.SetActive(false);
        }

        // 임대업자 NPC 퇴장 동기화 (Tuto_09를 받았거나 완료했다면 끈다)
        if (HasActiveQuest(QuestID.Tuto_09) || QuestManager.Instance.CompletedQuestIDs.Contains(QuestID.Tuto_09))
        {
            if (landlordNPC != null) landlordNPC.gameObject.SetActive(false);
        }

        // 타운 안내 카메라 연출을 이미 본 상태라면, 가이드 NPC의 대사를 Phase2로
        if (HasActiveQuest(QuestID.Tuto_02))
        {
            if (guideNPC != null) guideNPC.uniqueDialogue = guideDialogue_Phase2;
            isTownGuideFinished = true;
        }

        if (HasActiveQuest(QuestID.Tuto_06))
        {
            if (GameManager.Instance.currentTime == GAME_TIME.Evening || GameManager.Instance.currentTime == GAME_TIME.Night)
            {
                // 저녁에 돌아왔다면 대사로 미리 바꿔두기
                if (spaceshipNPC != null) spaceshipNPC.uniqueDialogue = spaceship_FailNight;
            }
            else
            {
                // 낮이라면 정상(탐사 준비) 대사 유지
                if (spaceshipNPC != null) spaceshipNPC.uniqueDialogue = spaceship_ReadyDay;
            }
        }
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
        if (npcID == NPCID.SpaceshipOwner && HasActiveQuest(QuestID.Tuto_06))
        {
            Quest tuto06 = QuestManager.Instance.GetActiveQuest(QuestID.Tuto_06);

            // 목표량을 이미 다 채운 상태
            if (tuto06 != null && tuto06.IsAllObjectivesComplete())
            {
                // 유저에게 보상을 받으라고 안내
                NotificationUIManager.Instance.ShowNotification(LocalizationHelper.Main("NOTI_CLAIM_QUEST_REWARD"));
                return;
            }

            // 목표를 덜 채웠는데 저녁/밤인 경우
            if (GameManager.Instance.currentTime == GAME_TIME.Evening || GameManager.Instance.currentTime == GAME_TIME.Night)
            {
                StartCoroutine(TimeResetSequence());
            }
        }
    }

    private void HandleQuestAccepted(QuestID questID)
    {
        if (questID == QuestID.Tuto_05)
        {
            if (guideNPC != null) guideNPC.gameObject.SetActive(false);

        }

    }


    private void HandlePointArrived(PointID pointID)
    {
        if (pointID == PointID.MyShop && HasActiveQuest(QuestID.Tuto_09))
        {
            if (landlordNPC != null) landlordNPC.gameObject.SetActive(false);

        }
    }

    private IEnumerator TimeResetSequence()
    {
        InputControlManager.Instance.LockInput();
        yield return FadeUIManager.Instance.FadeOut();

        // 시간만 낮으로
        GameManager.Instance.ChangeTime(GAME_TIME.Morning);

        // 체력 회복
        PlayerCondition playerCondition = FindObjectOfType<PlayerCondition>();
        if (playerCondition != null)
        {
            playerCondition.FullHealthRecovery();
        }

        // 대사 변경
        if (spaceshipNPC != null) spaceshipNPC.uniqueDialogue = spaceship_ReadyDay;

        yield return FadeUIManager.Instance.FadeIn();
        InputControlManager.Instance.UnlockInput();

    }

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

    private bool HasActiveQuest(QuestID questID)
    {
        return QuestManager.Instance.GetActiveQuest(questID) != null;
    }

    private void CheckTuto06Completion()
    {
        if (HasActiveQuest(QuestID.Tuto_07) || QuestManager.Instance.CompletedQuestIDs.Contains(QuestID.Tuto_07))
            return;

        bool isTuto06Achieved = false;

        // 이미 보상까지 다 받았는지
        if (QuestManager.Instance.CompletedQuestIDs.Contains(QuestID.Tuto_06))
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