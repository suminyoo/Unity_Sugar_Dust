using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public struct QuestDialogue
{
    public QuestID triggerQuestID;

    [Tooltip("True: 해당 퀘스트를 '완료'했을 때 출력 (영구 대사)\n" +
                 "False: 해당 퀘스트가 '진행 중'일 때 출력 (임시 대사)")]
    public bool checkCompleted;
    public DialogueData dialogue;
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

    [Tooltip("기본 대사 외에 퀘스트 조건에 따라 우선적으로 출력될 대사 리스트입니다.\n" +
             "우선순위: 1. 진행 중인 퀘스트 대사 / 2. 완료된 퀘스트 대사 (리스트 상단 우선)")]
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

        UpdateDialogueContext();

    }

    /// <summary>
    /// 현재 퀘스트 진행 상태를 확인하여 NPC의 대사를 최신 상태로 업데이트합니다.
    /// 로직 순서:
    /// 1. '진행 중'인 퀘스트 대사가 있다면 즉시 적용 (가장 높은 우선순위)
    /// 2. '진행 중'인 게 없다면, '완료된' 퀘스트 중 리스트 가장 위(Index 0)에 있는 것을 적용
    /// </summary>
    public void UpdateDialogueContext()
    {
        // 진행 중인 퀘스트가 있는지 리스트 전체를 먼저 검사
        foreach (var qd in questBasedDialogues)
        {
            if (!qd.checkCompleted)
            {
                if (QuestManager.Instance.GetActiveQuest(qd.triggerQuestID) != null)
                {
                    uniqueDialogue = qd.dialogue;
                    //Debug.Log($"{npcData.npcID} 대사 교체됨 (진행 중): {qd.triggerQuestID}");
                    return;
                }
            }
        }

        // 진행 중인 조건이 하나도 없었으면
        // 완료된 퀘스트로 바꿀 대사가 있는지 리스트 위에서부터 다시 검사
        foreach (var qd in questBasedDialogues)
        {
            if (qd.checkCompleted)
            {
                if (QuestManager.Instance.completedQuestIDs.Contains(qd.triggerQuestID))
                {
                    uniqueDialogue = qd.dialogue;
                    //Debug.Log($"{npcData.npcID} 대사 교체됨 (완료/영구): {qd.triggerQuestID}");
                    return;
                }
            }
        }

        // 조건에 안맞으면 기본 대사 유지
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