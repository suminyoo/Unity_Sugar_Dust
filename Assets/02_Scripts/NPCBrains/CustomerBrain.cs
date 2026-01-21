using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum CustomerType
{
    Normal,         // 일반 손님
    Haggler,        // 흥정 손님
    Scammer,        // 사기꾼 (위조 지폐)
    Indecisive,     // 카운터와서 안삼
    Beggar,         // 거지 (그냥 주세요)
    Thief,          // 도둑
    Impatient,      // 빨리 계산안하면 감
}

public class CustomerBrain : NPCBrain
{
    [Header("Settings")]
    public float wanderDuration = 5.0f; // 배회 시간
    public float thinkingTime = 1.5f;   // 물건 보고 고민하는 시간

    [Header("Personality")]
    public CustomerType myType; // 내 진상 유형

    // 외부 참조
    private DisplayStand targetShop;
    private CheckoutCounter counter; // 카운터 참조
    private Transform entrancePoint; // 상점 입구 좌표
    private System.Action onDespawnCallback; // 파괴 시 콜백

    // 내부 상태
    private int targetSlotIndex = -1;
    private Vector3 currentQueueTarget;
    private bool isFrontOfLine = false;

    // 카운터가 확인할 프로퍼티
    public bool IsReadyForTransaction { get; private set; } = false;

    protected override void Start()
    {
        // 매니저에 의해 Setup 되므로 Start는 비워둠
    }

    // 매니저에서 호출하는 셋업
    public void Setup(DisplayStand shop, CheckoutCounter checkout, Transform entrance, System.Action onDespawn)
    {
        this.targetShop = shop;
        this.counter = checkout;
        this.entrancePoint = entrance;
        this.onDespawnCallback = onDespawn;

        // 로직 시작
        StartCoroutine(CustomerRoutine());
    }

    // 메인 행동 루틴
    private IEnumerator CustomerRoutine()
    {
        // 1. 배회 (가게 둘러보기)
        yield return StartCoroutine(WanderPhase());

        // 2. 물건 탐색
        Transform itemPos = null;
        bool itemFound = false;

        if (targetShop != null)
        {
            itemFound = targetShop.TryGetRandomSellableItem(out targetSlotIndex, out itemPos);
        }

        if (itemFound && itemPos != null)
        {
            // 3. 물건 위치로 이동
            yield return StartCoroutine(MoveToItemPhase(itemPos));

            // 4. 물건 집기 및 가격 판단 (공짜/비쌈/적당함 분기점)
            // 여기서 결과에 따라 카운터로 갈지, 바로 나갈지 결정됨
            yield return StartCoroutine(PickUpAndCheckPricePhase(itemPos));
        }
        else
        {
            // 살 물건이 없으면 퇴장
            SayToSelf("여긴 살만한게 없네");
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(ExitPhase());
        }
    }

    #region 행동 단계별 로직 (Phases)

    // 배회 로직
    private IEnumerator WanderPhase()
    {
        float timer = 0f;

        // 지정된 시간 동안 랜덤하게 돌아다니기
        while (timer < wanderDuration)
        {
            if (controller.Movement.HasArrived())
            {
                // 상점 근처 랜덤 위치
                Vector3 wanderTarget = GetRandomPointAround(targetShop.transform.position, 4f);
                controller.Movement.MoveTo(wanderTarget);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        controller.Movement.Stop();
    }

    // 아이템 위치로 이동
    private IEnumerator MoveToItemPhase(Transform itemPos)
    {
        controller.Movement.MoveTo(itemPos.position);

        while (!controller.Movement.HasArrived())
        {
            yield return null;
        }
    }

    // [핵심 변경] 물건을 보고 가격을 판단하는 단계
    private IEnumerator PickUpAndCheckPricePhase(Transform itemPos)
    {
        // 물건 바라보기 & 애니메이션
        controller.Movement.LookAtTarget(itemPos);
        controller.Animation.PlayEmotion(NPCAnimation.Emotion.LookDown); // 집는 시늉 혹은 고민

        yield return new WaitForSeconds(thinkingTime);

        // 가격 정보 확인
        var slot = targetShop.InventorySystem.slots[targetSlotIndex];
        int basePrice = slot.itemData.sellPrice;                // 원가
        int sellingPrice = targetShop.GetSlotPrice(targetSlotIndex); // 판매가

        // 비율 계산 (0으로 나누기 방지)
        float ratio = (basePrice == 0) ? 0 : (float)sellingPrice / (float)basePrice;

        // --- 조건 분기 ---

        // Case 1: 공짜 (0원) -> 바로 들고 나감
        if (sellingPrice == 0)
        {
            SayToSelf("와! 이거 공짜네? 득템!");
            //controller.Animation.PlayEmotion(NPCAnimation.Emotion.Happy);

            // 즉시 구매 처리 (돈은 0원이지만 재고 감소)
            targetShop.TrySellItemToNPC(targetSlotIndex);

            yield return new WaitForSeconds(1.5f);

            // 바로 퇴장
            yield return StartCoroutine(ExitPhase());
        }
        // Case 2: 너무 비쌈 (> 2배) -> 안 사고 나감
        else if (ratio > 2.0f)
        {
            SayToSelf($"뭐야? {sellingPrice}골드? 바가지잖아!");
            //controller.Animation.PlayEmotion(NPCAnimation.Emotion.Angry);

            yield return new WaitForSeconds(1.5f);

            // 바로 퇴장
            yield return StartCoroutine(ExitPhase());
        }
        // Case 3: 적당함 (<= 2배) -> 카운터로 가져감
        else
        {
            SayToSelf("음, 가격 괜찮네. 계산하러 가야지.");
            // TODO: 손에 물건 들기 연출 추가 가능 (Visual)

            yield return new WaitForSeconds(1.0f);

            // 카운터 이동 및 줄 서기 시작
            yield return StartCoroutine(QueueAndTransactionPhase());
        }
    }

    // 카운터 줄 서기 및 대기
    // [수정됨] 카운터 줄 서기 및 대기
    private IEnumerator QueueAndTransactionPhase()
    {
        // 1. 줄 서기 시도
        Vector3? targetPos = counter.JoinQueue(this);

        // 자리가 꽉 차서 null을 받았다면?
        if (targetPos == null)
        {
            controller.Animation.PlayEmotion(NPCAnimation.Emotion.Disappointed); // 실망 모션
            SayToSelf("줄이 너무 기네... 다음에 와야겠다.");

            yield return new WaitForSeconds(2.0f);

            // 퇴장 페이즈로 바로 전환
            yield return StartCoroutine(ExitPhase());
            yield break; // 이 코루틴 종료
        }

        // 자리가 있으면 할당받은 위치로 설정
        currentQueueTarget = targetPos.Value;

        // [에러 해결 부분]
        // 내가 맨 앞인지 판단하는 로직: "내 목표 지점이 줄의 0번(맨 앞) 포인트와 같은가?"
        if (counter.queuePoints.Count > 0)
        {
            // Vector3 간의 거리가 아주 가까우면 같은 위치로 간주
            float distToFirstPoint = Vector3.Distance(currentQueueTarget, counter.queuePoints[0].position);
            isFrontOfLine = (distToFirstPoint < 0.1f);
        }
        else
        {
            isFrontOfLine = false;
        }

        IsReadyForTransaction = false;

        // 줄 서기 루프 (거래가 끝날 때까지)
        while (true)
        {
            // 이동 로직
            if (Vector3.Distance(transform.position, currentQueueTarget) > 0.5f)
            {
                controller.Movement.MoveTo(currentQueueTarget);
                IsReadyForTransaction = false;
            }
            else
            {
                // 도착함 -> 멈춤
                controller.Movement.Stop();
                controller.Movement.LookAtTarget(counter.transform);

                // 맨 앞자리라면
                if (isFrontOfLine)
                {
                    // 거래 준비 상태가 아니었다면 준비 상태로 전환
                    if (!IsReadyForTransaction)
                    {
                        IsReadyForTransaction = true;
                        SayToSelf("계산해주세요.");
                    }
                }
            }
            yield return null;
        }
    }

    // 퇴장 로직
    private IEnumerator ExitPhase()
    {
        // 혹시 줄을 서 있었다면 명단에서 제외 (안전장치)
        if (counter != null) counter.LeaveQueue(this);

        // 입구로 이동
        if (entrancePoint != null)
        {
            controller.Movement.MoveTo(entrancePoint.position);

            while (!controller.Movement.HasArrived())
            {
                yield return null;
            }
        }

        SayToSelf("다음에 또 올게요");
        controller.Movement.Stop();

        yield return new WaitForSeconds(0.5f);

        // 파괴
        onDespawnCallback?.Invoke();
    }

    #endregion

    #region 상호작용 및 이벤트

    // 카운터에서 호출: "앞 사람이 줄었으니 이 위치로 오세요"
    public void UpdateQueueTarget(Vector3 newPos, bool amIFront)
    {
        currentQueueTarget = newPos;
        isFrontOfLine = amIFront;
        // 이동을 위해 준비 상태 해제
        IsReadyForTransaction = false;
    }

    // 1. 플레이어가 카운터 클릭 시 호출됨 (진입점)
    public void StartDialogueWithPlayer()
    {
        if (!IsReadyForTransaction) return;

        // 이동 완전 정지
        controller.Movement.Stop();

        Debug.Log($"[Dialogue Triggered] 손님 유형: {myType}");

        // 다이얼로그 매니저 호출
        // TODO: NPC 데이터(ScriptableObject)나 유형에 따른 대사 데이터를 넘겨줘야 함
        // 지금은 null 혹은 임시 데이터를 넣어서 창만 띄움

        DialogueData tempData = null; // 실제 데이터 연결 필요

        DialogueManager.Instance.StartDialogue(
            tempData,
            "손님",
            () => {
                // 대화 끝났을 때의 콜백 (임시)
                // 실제로는 대화 선택지 결과에 따라 OnDialogueFinished(true/false)를 호출해야 함
                // 여기서는 일단 성공했다고 가정하고 테스트
                OnDialogueFinished(true);
            }
        );
    }

    // 2. 대화/거래가 끝났을 때 외부(DialogueManager의 선택지 결과 등)에서 호출해줘야 하는 함수
    // isSuccess: 물건을 샀으면 true, 안 샀으면 false
    public void OnDialogueFinished(bool isSuccess)
    {
        // 줄 서기 루프(QueueAndTransactionPhase)를 강제로 종료시킴
        StopAllCoroutines();

        // 결과 처리
        if (isSuccess)
        {
            SayToSelf("감사합니다. 잘 쓸게요!");
            //controller.Animation.PlayEmotion(NPCAnimation.Emotion.Happy);

            // 실제 아이템 판매 처리 (재고 감소 및 돈 증가)
            targetShop.TrySellItemToNPC(targetSlotIndex);
        }
        else
        {
            SayToSelf("에이, 그럼 안 살래요.");
            //controller.Animation.PlayEmotion(NPCAnimation.Emotion.Sad);
            // 안 샀으니 재고는 그대로 둠 (이미 들고 있었다면 반납 로직 필요할 수 있음)
        }

        // 줄에서 명단 제외
        counter.LeaveQueue(this);

        // 퇴장 시작
        StartCoroutine(ExitPhase());
    }

    #endregion

    #region 유틸리티

    // 랜덤 위치 찾기
    private Vector3 GetRandomPointAround(Vector3 center, float range)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center;
    }

    #endregion
}