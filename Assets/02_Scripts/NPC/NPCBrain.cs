using System.Collections;
using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    protected NPCController controller;
    protected Transform playerTransform;

    protected bool isInteracting = false;
    public bool IsInteracting => isInteracting;

    private int currentPathIndex = 0;

    private bool isPlayerInRange = false;

    protected virtual void Awake()
    {
        controller = GetComponent<NPCController>();
    }
    protected virtual void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

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
            string msg = controller.npcData.GetGreetingMessage();
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

        // 대화 시작: 코루틴 대신 콜백 방식으로 변경 (더 깔끔함)
        StartCoroutine(DefaultInteractionProcess());
    }

    // 기본 NPC 행동 패턴
    protected virtual IEnumerator DefaultInteractionProcess()
    {
        // 대화 준비 (멈추고, 쳐다보고, 애니메이션)
        PrepareInteraction();

        // 대화 진행 (대화 끝나는거 대기)
        yield return StartCoroutine(DialogueProcess());

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
        //대화 데이터 가져오기 (SO연결 필요)
        DialogueData dialogueToPlay = controller.npcData.defaultDialogue;

        if (dialogueToPlay != null)
        {
            bool dialogueFinished = false;

            // 매니저에게 대화프로세스 요청, 끝나면 dialogueFinished =true 
            DialogueManager.Instance.StartDialogue(
                dialogueToPlay,
                controller.npcData.GetNpcName(),
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
    public bool HasAvailableQuest()
    {
        if (controller.npcData.questsToGive == null || controller.npcData.questsToGive.Count == 0)
            return false;

        foreach (var questData in controller.npcData.questsToGive)
        {
            if (!QuestManager.Instance.completedQuestIDs.Contains(questData.questID.ToString()))
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
    }
    #endregion

    // 굿바이 인사
    protected void ShowGoodbyeMessage()
    {
        if (controller.npcData == null) return;

        string msg = controller.npcData.GetGoodByeMessage();
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