using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public struct QuestDialogue
{
    public QuestID triggerQuestID;

    [Tooltip("True: 퀘스트 완료 상태일 때 / False: 퀘스트 진행 중(Active)일 때")]
    public bool checkCompleted;

    public DialogueData dialogue; //교체될 대사
}

//NPCBrain은 NPCController에게만 말하고, NPCController가 Animation을 관리하는 구조
public class NPCController : MonoBehaviour
{
    public NPCData npcData;
    public NPCPatrolPath assignedPath;

    [Header("Instance Data")]

    [SerializeField] private LocalizedString npcName;
    [SerializeField] private LocalizedString greetingMessage;
    [SerializeField] private LocalizedString goodByeMessage;

    public DialogueData uniqueDialogue;
    public List<QuestDialogue> questBasedDialogues;

    public string GetNpcName() => npcName.GetLocalizedString();
    public string GetGreetingMessage() => GetSafeLocalizedString(greetingMessage);
    public string GetGoodByeMessage() => GetSafeLocalizedString(goodByeMessage);

    // 참조
    [HideInInspector] public NPCMovement Movement { get; private set; }
    [HideInInspector] public NPCBrain Brain { get; private set; }
    [HideInInspector] public NPCAnimation Animation { get; private set; }
    [HideInInspector] public SpeechBubble Bubble { get; private set; }

    private void Awake()
    {
        Movement = GetComponent<NPCMovement>();
        Brain = GetComponent<NPCBrain>();
        Animation = GetComponent<NPCAnimation>(); // 추가됨
        Bubble = GetComponentInChildren<SpeechBubble>();

    }

    private void Start()
    {
        // 애니메이션 컴포넌트 초기화 (Movement를 넘겨줌)
        Animation.Init(Movement);
        // 데이터 적용 init
        Movement.SetSpeed(npcData.moveSpeed);

    }
    public void UpdateDialogueContext()
    {
        foreach (var qd in questBasedDialogues)
        {
            bool isConditionMet = false;

            if (qd.checkCompleted)
            {
                // 퀘스트 완료 목록에 있는지 확인
                isConditionMet = QuestManager.Instance.completedQuestIDs.Contains(qd.triggerQuestID);
            }
            else
            {
                // 진행 중인 퀘스트 목록에 있는지 확인
                isConditionMet = QuestManager.Instance.GetActiveQuest(qd.triggerQuestID) != null;
            }

            if (isConditionMet)
            {
                uniqueDialogue = qd.dialogue;
                Debug.Log($"{npcData.npcID} 대사 교체됨. {qd.triggerQuestID}");
                return; 
            }
        }
    }

    public void OnInteract()
    {
        Brain.HandleInteraction();
    }
    public void OnQuestInteract()
    {
        Brain.HandleQuestInteraction();
    }

    private string GetSafeLocalizedString(LocalizedString localizedString)
    {
        if (localizedString == null || localizedString.IsEmpty)
        {
            return string.Empty;
        }

        return localizedString.GetLocalizedString();
    }
}