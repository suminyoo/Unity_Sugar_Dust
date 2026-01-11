using UnityEngine;

//NPCBrain은 NPCController에게만 말하고, NPCController가 Animation을 관리하는 구조
public class NPCController : MonoBehaviour
{
    public NPCData npcData;
    public PatrolPath assignedPath;


    // 참조
    [HideInInspector] public NPCMovement Movement;
    [HideInInspector] public NPCBrain Brain;
    [HideInInspector] public NPCAnimation Animation;
    [HideInInspector] public SpeechBubble Bubble;

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

    public void OnInteract()
    {
        Brain.HandleInteraction();
    }
}