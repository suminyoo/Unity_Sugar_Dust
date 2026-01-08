using UnityEngine;

public class NPCAnimation : MonoBehaviour
{
    private Animator animator;
    private NPCMovement movement;

    // 열거형 정의 
    public enum SitState { Ground, Chair } // 앉기 종류
    public enum TalkState { Normal, Explain, Argument } // 대화 종류
    public enum Emotion { Surprised, Disappointed, LookDown } // 감정 종류
    // 애니메이터 파라미터 이름을 미리 해싱
    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashIsSitting = Animator.StringToHash("IsSitting");
    private readonly int hashSitType = Animator.StringToHash("SitType");
    private readonly int hashDoTalk = Animator.StringToHash("DoTalk");
    private readonly int hashTalkType = Animator.StringToHash("TalkType");
    private readonly int hashDoEmotion = Animator.StringToHash("DoEmotion");
    private readonly int hashEmotionType = Animator.StringToHash("EmotionType");


    public void Init(NPCMovement moveComponent)
    {
        animator = GetComponentInChildren<Animator>(); // 모델이 자식 오브젝트
        movement = moveComponent;
    }

    private void Update()
    {
        if (movement != null && animator != null)
        {
            // 현재 이동 속도를 가져옴
            float currentSpeed = movement.GetCurrentSpeed();

            // 애니메이터의 Speed 값에 적용 (dampTime 0.1)
            animator.SetFloat(hashSpeed, currentSpeed, 0.1f, Time.deltaTime);
        }
    }

    // 대화 애니메이션 선택
    public void PlayTalk(TalkState type)
    {
        if (animator == null) return;
        animator.SetInteger(hashTalkType, (int)type);
        animator.SetTrigger(hashDoTalk);
    }

    // 랜덤 대화 애니메이션
    public void PlayTalkRandom()
    {
        if (animator == null) return;
        int randomType = Random.Range(0, 3); // 0, 1, 2 중 랜덤
        animator.SetInteger(hashTalkType, randomType);
        animator.SetTrigger(hashDoTalk);
    }

    // 감정 표현
    public void PlayEmotion(Emotion type)
    {
        if (animator == null) return;
        animator.SetInteger(hashEmotionType, (int)type);
        animator.SetTrigger(hashDoEmotion);
    }

    // 앉기 / 일어서기 
    public void SetSit(bool isSitting, SitState type = SitState.Ground)
    {
        if (animator == null) return;

        // 앉는 종류 먼저 설정 후
        if (isSitting) animator.SetInteger(hashSitType, (int)type);

        // 상태 전환
        animator.SetBool(hashIsSitting, isSitting);
    }

}