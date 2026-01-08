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

        // 코루틴 시작
        if (interactionCoroutine != null) StopCoroutine(interactionCoroutine);
        interactionCoroutine = StartCoroutine(InteractionProcess());
    }

    private IEnumerator InteractionProcess()
    {
        isInteracting = true;

        // 1. 이동 멈춤 & 플레이어 바라보기
        controller.Movement.Stop();
        if (playerTransform != null) controller.Movement.LookAtTarget(playerTransform);

        // 2. 애니메이션 (대화 모션)
        if (controller.Animation != null) controller.Animation.PlayTalkRandom();

        // ---------------------------------------------------------
        // [임시 구현] 실제 대화 시스템이 들어갈 자리
        // ---------------------------------------------------------
        Debug.Log($"NPC: 안녕하세요! (마우스 클릭으로 대화 넘기기 시뮬레이션)");

        // 가상의 대화 상태 변수
        bool isDialogueFinished = false;

        // 예시: 대화창을 띄우는 함수 호출
        // DialogueManager.Instance.StartDialogue(npcData, () => { isDialogueFinished = true; });

        // [핵심] 대화가 끝날 때까지 무한 대기 (프레임 단위 체크)
        // 실제 구현 시에는 UI 매니저가 isDialogueFinished를 true로 바꿔줄 때까지 기다림
        while (!isDialogueFinished)
        {
            // --- 임시 테스트용 코드 ---
            // 마우스 왼쪽 클릭을 하면 대화가 끝난 것으로 간주
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("NPC: 즐거운 모험 되세요! (대화 종료)");
                isDialogueFinished = true; // 루프 탈출 조건 달성
            }
            // -----------------------

            yield return null; // 다음 프레임까지 대기
        }
        // ---------------------------------------------------------

        // 3. 잠시 뜸 들이기 (대화창 닫히고 바로 획 돌면 어색하니까)
        yield return new WaitForSeconds(0.5f);

        // 4. 다시 이동 재개
        controller.Movement.Resume();
        isInteracting = false;
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