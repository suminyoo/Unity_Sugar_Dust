using System.Collections;
using System.Linq;
using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    protected NPCController controller;
    protected Transform playerTransform;

    protected bool isInteracting = false;
    public bool IsInteracting => isInteracting;

    private int currentPathIndex = 0;

    private bool isPlayerInRange = false;

    public GameObject questAlertMark;

    protected virtual void Awake()
    {
        controller = GetComponent<NPCController>();
    }

    protected virtual void OnEnable()
    {
        if (GameEvents.OnQuestProgressUpdated != null)
        {
            GameEvents.OnQuestProgressUpdated += UpdateQuestIndicator;
        }
    }
    protected virtual void OnDisable()
    {
        if (GameEvents.OnQuestProgressUpdated != null)
        {
            GameEvents.OnQuestProgressUpdated -= UpdateQuestIndicator;
        }
    }
    protected virtual void Start()
    {

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        UpdateQuestIndicator();

        // 경로가 있으면 패트롤, 없으면 가만히
        if (controller.assignedPath != null)
            StartCoroutine(PatrolRoutine());
    }

    protected virtual void Update()
    {
        if (isInteracting) return;

        DetectPlayer();


    }
    private void DetectPlayer()
    {
        if (playerTransform == null || controller.npcData == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        float detectionRange = controller.npcData.detectRange;

        // 플레이어가 감지범위 들어옴
        if (distance <= detectionRange && !isPlayerInRange)
        {
            isPlayerInRange = true;

            // 인사
            string msg = controller.GetGreetingMessage(); 
            if (!string.IsNullOrEmpty(msg))
            {
                controller.Bubble.ShowBubble(msg);
            }
        }
        // 범위 밖에 나감. 나가는 반경 1.2배
        else if (distance > detectionRange * 1.2f && isPlayerInRange)
        {
            isPlayerInRange = false;
        }
    }

    #region Interaction
    public virtual void HandleInteraction()
    {
        // 이미 대화 중이면 중복 실행 방지
        if (isInteracting) return;

        controller.UpdateDialogueContext();
        GameEvents.OnNPCInteractionStarted?.Invoke(controller);

        StartCoroutine(DefaultInteractionProcess());
    }

    // 기본 NPC 행동 패턴
    protected virtual IEnumerator DefaultInteractionProcess()
    {
        // 대화 준비 (멈추고, 쳐다보고, 애니메이션)
        PrepareInteraction();

        // 대화 진행 (대화 끝나는거 대기)
        yield return StartCoroutine(DialogueProcess());

        GameEvents.OnNPCTalkedFinished?.Invoke(controller.npcData.npcID);

        FinishInteraction();
    }
    protected void PrepareInteraction()
    {
        isInteracting = true;
        controller.Movement.Stop();
        if (playerTransform != null) controller.Movement.LookAtTarget(playerTransform);
        if (controller.Animation != null) controller.Animation.PlayTalkRandom();
    }
    protected IEnumerator DialogueProcess(bool showAutoGoodbye = true)
    {
        DialogueData dialogueToPlay = controller.uniqueDialogue;

        if (dialogueToPlay != null)
        {
            bool dialogueFinished = false;

            // 매니저에게 대화프로세스 요청, 끝나면 dialogueFinished =true 
            DialogueManager.Instance.StartDialogue(
                dialogueToPlay,
                controller.GetNpcName(),
                () => { dialogueFinished = true; }
            );
            //대화 끝날때까지 대기
            yield return new WaitUntil(() => dialogueFinished);

        }
        if (showAutoGoodbye)
        {
            ShowGoodbyeMessage();
        }
    }

    //상호작용 종료
    protected void FinishInteraction()
    {
        controller.Movement.Resume();
        isInteracting = false;
    }


    #endregion

    #region Quest Interaction
    public void UpdateQuestIndicator()
    {
        if (questAlertMark == null || controller.npcData == null) return;

        bool shouldShow = false;

        if (controller.npcData.questsToGive != null)
        {
            foreach (var questData in controller.npcData.questsToGive)
            {
                Quest activeQuest = QuestManager.Instance.GetActiveQuest(questData.questID);

                // 이미 받은 퀘스트인 경우
                if (activeQuest != null)
                {
                    // 완료는 했지만 보상을 아직 안 받았다면 느낌표 표시
                    if (activeQuest.IsAllObjectivesComplete() && !activeQuest.isRewardClaimed)
                    {
                        shouldShow = true;
                        break;
                    }
                    // 진행 중인 상태 느낌표 안 보임
                    continue;
                }

                // 아직 안 받은 퀘스트인 경우
                if (!QuestManager.Instance.completedQuestIDs.Contains(questData.questID))
                {
                    // 선행 퀘스트 조건 확인
                    bool canAccept = true;
                    if (questData.requiredQuestID != QuestID.None)
                    {
                        if (!QuestManager.Instance.IsQuestAchieved(questData.requiredQuestID))
                        {
                            canAccept = false;
                        }
                    }

                    if (canAccept)
                    {
                        shouldShow = true;
                        break;
                    }
                }
            }
        }

        questAlertMark.SetActive(shouldShow);
    }

    public bool HasAvailableQuest()
    {
        if (controller.npcData.questsToGive == null || controller.npcData.questsToGive.Count == 0)
            return false;

        foreach (var questData in controller.npcData.questsToGive)
        {
            if (!QuestManager.Instance.completedQuestIDs.Contains(questData.questID))
            {
                return true;
            }
        }
        return false;
    }

    public void HandleQuestInteraction()
    {
        if (isInteracting) return;
        PrepareInteraction();

        NPCQuestUIManager.Instance.OpenInteractionUI(controller.npcData.questsToGive, OnQuestUIClosed);
    }

    private void OnQuestUIClosed()
    {
        FinishInteraction();
        UpdateQuestIndicator();
    }


    #endregion

    // 굿바이 인사
    protected void ShowGoodbyeMessage()
    {
        if (controller.npcData == null) return;

        string msg = controller.GetGoodByeMessage();
        if (!string.IsNullOrEmpty(msg))
        {
            controller.Bubble.ShowBubble(msg);
        }
    }

    // 혼잣말 함수
    public void SayToSelf(string text)
    {
        if (controller.Bubble != null)
            controller.Bubble.ShowBubble(text);
    }


    private IEnumerator PatrolRoutine()
    {
        PatrolPoint currentPoint = null;

        while (true)
        {
            yield return null;

            if (isInteracting || controller.assignedPath == null) continue;

            // 목표없으면 새로 가져오기
            if (currentPoint == null)
            {
                currentPoint = controller.assignedPath.GetWaypoint(currentPathIndex);
            }

            if (currentPoint != null)
            {
                // 이동
                controller.Movement.MoveTo(currentPoint.transform.position);

                while (!controller.Movement.HasArrived())
                {
                    if (isInteracting) { yield return null; continue; }
                    yield return null; 
                }

                float rotTimer = 0f;
                float rotDuration = 0.5f;
                Quaternion startRot = transform.rotation;
                while (rotTimer < rotDuration)
                {
                    if (!isInteracting)
                    {
                        rotTimer += Time.deltaTime;
                        transform.rotation = Quaternion.Slerp(startRot, currentPoint.transform.rotation, rotTimer / rotDuration);
                    }
                    yield return null;
                }

                // 지점별 대기시간
                float waitTimer = 0f;
                while (waitTimer < currentPoint.waitTime)
                {
                    if (!isInteracting) waitTimer += Time.deltaTime;
                    yield return null;
                }
            }

            currentPathIndex++;
            currentPoint = null;
        }
    }
}





//NPCAnimation 사용 예시
//// 대화할 때 랜덤 제스처
//controller.Animation.PlayTalkRandom();

//// 특정 대화 제스처 (설명하는 손짓
//controller.Animation.PlayTalk(NPCAnimation.TalkState.Explain);

//// 깜짝 놀라기
//controller.Animation.PlayEmotion(NPCAnimation.Emotion.Surprised);

//// 의자에 앉기
//controller.Movement.Stop(); // 일단 멈추고
//controller.Animation.SetSit(true, NPCAnimation.SitState.Chair);

//// 다시 일으키기
//controller.Animation.SetSit(false);
//controller.Movement.Resume();