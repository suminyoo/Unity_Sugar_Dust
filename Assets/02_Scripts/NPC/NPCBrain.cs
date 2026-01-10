using System.Collections;
using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    private NPCController controller;
    private int currentPathIndex = 0;
    private bool isInteracting = false;
    private Transform playerTransform;
    private Coroutine interactionCoroutine;

    private void Start()
    {
        controller = GetComponent<NPCController>();

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // 경로가 있으면 패트롤, 없으면 가만히(혹은 랜덤이동)
        if (controller.assignedPath != null)
            StartCoroutine(PatrolRoutine());
    }

    public void HandleInteraction()
    {
        // 이미 대화 중이면 중복 실행 방지
        if (isInteracting) return;

        // 대화 시작: 코루틴 대신 콜백 방식으로 변경 (더 깔끔함)
        StartCoroutine(StartDialogueRoutine());
    }
    private IEnumerator StartDialogueRoutine()
    {
        isInteracting = true;
        controller.Movement.Stop();
        if (playerTransform != null) controller.Movement.LookAtTarget(playerTransform);
        if (controller.Animation != null) controller.Animation.PlayTalkRandom();

        // 1. 대화 끝났는지 확인할 변수
        bool dialogueFinished = false;

        // 2. 대화 데이터 가져오기 (SO가 없으면 기본 대사)
        DialogueData dialogueToPlay = null;
        // 추후 NPCData에 DialogueData 필드를 추가해서 가져오면 됨
        dialogueToPlay = controller.data.defaultDialogue; 

        if (dialogueToPlay != null)
        {
            // 매니저에게 대화 요청 (끝나면 dialogueFinished를 true로 만듦)
            DialogueManager.Instance.StartDialogue(dialogueToPlay, () => {
                dialogueFinished = true;
            });

            // 3. 대화가 끝날 때까지 대기 (WaitUntil)
            yield return new WaitUntil(() => dialogueFinished);
        }
        else
        {
            // 데이터가 없을 경우 테스트용 말풍선 띄우기
            Debug.LogWarning("대화 데이터가 없습니다. 말풍선으로 대체합니다.");
            controller.Bubble.ShowBubble("대화 데이터가 없어요...", 2f);
            yield return new WaitForSeconds(2f);
        }

        // 4. 대화 종료 후 처리
        controller.Movement.Resume();
        isInteracting = false;
    }

    // 혼잣말 테스트용 함수
    public void SayToSelf(string text)
    {
        if (controller.Bubble != null)
            controller.Bubble.ShowBubble(text, 3f);
    }


    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            // [체크 1] 시작부터 대화 중이면 대기
            while (isInteracting) yield return null;

            // 1. 현재 목표 지점 가져오기
            Transform targetPoint = null;
            if (controller.assignedPath != null)
                targetPoint = controller.assignedPath.GetPoint(currentPathIndex);

            // 2. 이동 명령 (딱 한 번만 호출)
            if (targetPoint != null)
            {
                controller.Movement.MoveTo(targetPoint.position);
            }

            // 3. 도착할 때까지 대기 (가장 중요한 부분 수정됨)
            while (true) // 무한 루프로 감싸서 조건을 직접 제어
            {
                // A. 대화 중이면? -> 아무것도 하지 말고 그냥 다음 프레임으로 넘김
                if (isInteracting)
                {
                    yield return null;
                    continue; // 아래의 도착 체크 로직을 실행하지 않고 루프 처음으로
                }

                // B. 도착했는가?
                if (controller.Movement.HasArrived())
                {
                    break; // 도착했으니 이동 루프 탈출
                }

                // C. 아직 이동 중
                yield return null;
            }

            // 4. 지점 도착 후 대기 (멍 때리기)
            float waitTime = controller.data != null ? controller.data.waitTimeAtPoint : 2f;
            float timer = 0;

            // 대기 시간 동안에도 대화가 걸릴 수 있으니 체크
            while (timer < waitTime)
            {
                if (!isInteracting)
                {
                    timer += Time.deltaTime;
                }
                yield return null;
            }

            // 5. 다음 인덱스
            currentPathIndex++;
        }
    }
}





//NPCAnimation 사용 예시
//// 1. 대화할 때 랜덤 제스처
//controller.Animation.PlayTalkRandom();

//// 2. 특정 대화 제스처 (설명하는 손짓)
//controller.Animation.PlayTalk(NPCAnimation.TalkState.Explain);

//// 3. 깜짝 놀라기
//controller.Animation.PlayEmotion(NPCAnimation.Emotion.Surprised);

//// 4. 의자에 앉히기
//controller.Movement.Stop(); // 일단 멈추고
//controller.Animation.SetSit(true, NPCAnimation.SitState.Chair);

//// 5. 다시 일으키기
//controller.Animation.SetSit(false);
//controller.Movement.Resume();