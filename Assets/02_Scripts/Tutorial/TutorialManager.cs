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
    public DialogueData parentDialogue_Phase1;
    public DialogueData parentDialogue_AfterGuide;
    public DialogueData curiousDialogue_AfterWeapon;
    public DialogueData weaponDialogue_AfterReward;
    public DialogueData spaceshipDialogue_AfterClear;

    [Header("Scene Objects")]
    public GameObject shopBlockerCollider;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // TODO: 세이브 데이터 추가 해야함 게임 세이브 매니저
        bool isTutorialCompleted = PlayerPrefs.GetInt("IsTutorialCompleted", 0) == 1;

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
            normalNPCGroup.SetActive(false);
            tutorialNPCGroup.SetActive(true);

            StartCoroutine(StartPhase1_WakeUp());
        }
    }

    private void OnEnable()
    {
        GameEvents.OnNPCTalked += HandleNPCTalked;
        GameEvents.OnPointArrived += HandlePointArrived;
        GameEvents.OnQuestCompleted += HandleQuestCompleted;
    }

    private void OnDisable()
    {
        GameEvents.OnNPCTalked -= HandleNPCTalked;
        GameEvents.OnPointArrived -= HandlePointArrived;
        GameEvents.OnQuestCompleted -= HandleQuestCompleted;
    }

    private IEnumerator StartPhase1_WakeUp()
    {
        yield return new WaitForSeconds(1.0f);

        DialogueManager.Instance.StartDialogue(
            parentDialogue_Phase1,
            "부모 외계인",
            () =>
            {
                QuestManager.Instance.AddQuest(tuto02_Guide);
            }
        );
    }

    // NPC와 대화했을 때 연출 (안내원 컷신, 탐사 이동 등)
    private void HandleNPCTalked(NPCID npcID)
    {
        // 안내원에게 말을 걸었고 Tuto_02가 진행 중일 때
        if (npcID == NPCID.Guide && HasActiveQuest("Tuto_02"))
        {
            StartCoroutine(PlayGuideSequence());
        }

        // 우주선 NPC에게 말을 걸었고 Tuto_05가 진행 중일 때
        else if (npcID == NPCID.SpaceshipOwner && HasActiveQuest("Tuto_05"))
        {
            StartCoroutine(MoveToExplorationMap());
        }
    }

    // 퀘스트 완료했을 때 대본 교체
    private void HandleQuestCompleted(QuestID questID)
    {
        switch (questID)
        {
            case QuestID.Tuto_04:
                Debug.Log("무기 장만 퀘스트 완료 무기상인, 호기심 NPC 대본 교체");
                weaponNPC.uniqueDialogue = weaponDialogue_AfterReward;
                curiousNPC.uniqueDialogue = curiousDialogue_AfterWeapon;
                break;

            case QuestID.Tuto_07:
                Debug.Log("수집품 보고 퀘스트 완료 우주선 NPC 대본 교체");
                spaceshipNPC.uniqueDialogue = spaceshipDialogue_AfterClear;
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

        Debug.Log("안내원 대화 종료 -> 카메라 마을 전경 컷신 시작");

        // TODO: 카메라 워킹 연출 (Cinemachine Timeline 등)
        // 안내책자, 단축키 설명 등 화면에 출력
        yield return new WaitForSeconds(4.0f); // 컷신 재생 시간 대기

        Debug.Log("컷신 종료 -> 안내원 퇴장, 대본 교체, 다음 퀘스트 지급");

        // 안내원 퇴장
        if (guideNPC != null) guideNPC.gameObject.SetActive(false);

        // 부모 NPC 대본 교체 (조심히 둘러보고 오렴)
        if (parentNPC != null) parentNPC.uniqueDialogue = parentDialogue_AfterGuide;

        // 컷신 종료 후 자연스럽게 Tuto_03(인간의 가능성) 퀘스트 강제 지급
        QuestManager.Instance.AddQuest(tuto03_Curious);

        InputControlManager.Instance.UnlockInput(); // 조작 잠금 해제
    }

    private IEnumerator MoveToExplorationMap()
    {
        InputControlManager.Instance.LockInput();

        Debug.Log("탐사 맵으로 이동 시작 (화면 암전 등)");
        yield return new WaitForSeconds(1.5f); // 텔레포트 연출 대기

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
}