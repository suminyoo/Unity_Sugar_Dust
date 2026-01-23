using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum CustomerType
{
    Normal,         // 일반 손님
    Haggler,        // 흥정 손님
    Scammer,        // 사기꾼 (위조 지폐..? 이건 좀 너무한듯)
    Indecisive,     // 카운터와서 안삼
    Beggar,         // 거지 (그냥 주세요)
    Thief,          // 도둑
    Impatient,      // 빨리 계산안하면 감
}

public class CustomerBrain : NPCBrain
{
    [Header("References")]
    private DisplayStand targetShop;
    private CheckoutCounter counter; // 카운터 참조
    private Transform entrancePoint; // 상점 입구 좌표
    private System.Action onDespawnCallback; // 파괴 시 콜백

    [Header("Settings")]
    public CustomerType myType; // 내 진상 유형

    public float wanderDuration = 5.0f; // 배회 시간
    public float thinkingTime = 1.5f;   // 물건 보고 고민하는 시간

    private Vector3 currentQueueTarget;
    private Quaternion currentQueueTargetRotation;
    private int targetSlotIndex = -1;
    private bool isFrontOfQueue = false;


    public bool IsInQueue => currentQueueTarget != Vector3.zero; // 줄 섰는지 확인용
                                                                  
    public bool IsReadyForTransaction { get; private set; } = false; // 결제 준비 완료 상태

    public GameObject itemCarryPoint;
    private ItemData itemToBuy;
    private int itemToBuyAmount; 
    private int itemToBuyPrice;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
    }

    // 강제 퇴장
    public void ForceLeave(bool dropItem)
    {
        StopAllCoroutines();

        // 만약 물건을 집은 상태라면 dropItemOnFloor 호출


        // 퇴장 코루틴 시작
        StartCoroutine(ExitPhase());
    }

    private void DropItemOnFloor()
    {
        // 아이템 바닥에 생성 바닥에 놓을때 개수도 포함
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

    // 메인 루틴
    private IEnumerator CustomerRoutine()
    {
        // 배회
        yield return StartCoroutine(WanderPhase());

        // 물건 탐색
        Transform itemPos = null;
        bool itemFound = false;

        itemFound = targetShop.TryGetRandomSellableItem(out targetSlotIndex, out itemPos);

        if (itemFound && itemPos != null)
        {
            // 물건 위치로 이동
            yield return StartCoroutine(MoveToItemPhase(itemPos));

            // 물건 집기 및 가격 판단
            yield return StartCoroutine(PickUpItemPhase(itemPos));
        }
        else
        {
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

        // 지정된 시간 동안 랜덤배회
        while (timer < wanderDuration)
        {
            if (controller.Movement.HasArrived())
            {
                // 잠깐 멈춤
                controller.Movement.Stop();

                float waitTime = Random.Range(1.5f, 4f);
                yield return new WaitForSeconds(waitTime);

                // 새 목적지
                Vector3 wanderTarget = GetRandomPointOnNavMesh();
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
        float stoppingDistance = 1.5f;
        Vector2 randomCircle = Random.insideUnitCircle.normalized * stoppingDistance;
        Vector3 randomOffset = new Vector3(randomCircle.x, 0, randomCircle.y);

        Vector3 finalTarget = itemPos.position + randomOffset;

        controller.Movement.MoveTo(finalTarget);
        while (!controller.Movement.HasArrived())
        {
            yield return null;
        }
    }

    // 집기 및 가격 판단 (공짜/비쌈은 바로 퇴장, 적당함은 카운터로 이동)
    private IEnumerator PickUpItemPhase(Transform itemPos)
    {
        // 고민
        controller.Movement.LookAtTarget(itemPos);
        controller.Animation.PlayEmotion(NPCAnimation.Emotion.LookDown); 

        yield return new WaitForSeconds(thinkingTime);

        // 정보 확인
        var slot = targetShop.InventorySystem.slots[targetSlotIndex];
        if (slot.IsEmpty)
            yield break; // 누가 먼저 사갔을떄 처리 후 종료

        itemToBuy = slot.itemData;
        itemToBuyPrice = targetShop.GetSlotPrice(targetSlotIndex);
        itemToBuyAmount = 1; //TODO: 여러개 살 수 있게 확장

        float ratio = (itemToBuy.sellPrice == 0) ? 0 : (float)itemToBuyPrice / itemToBuy.sellPrice;


        // Case 1: 공짜
        if (itemToBuyPrice == 0)
        {
            SayToSelf("와 공짜네? 아싸!");
            //controller.Animation.PlayEmotion(NPCAnimation.Emotion.Happy);

            targetShop.TryTakeItemFromStand(targetSlotIndex, itemToBuyAmount);

            yield return new WaitForSeconds(1.5f);
            yield return StartCoroutine(ExitPhase());
        }
        // Case 2: 너무 비쌈 (> 2배)
        else if (ratio > 2.0f)
        {
            SayToSelf($"{itemToBuyPrice}골드? 바가지잖아!");
            //controller.Animation.PlayEmotion(NPCAnimation.Emotion.Angry);

            yield return new WaitForSeconds(1.5f);
            yield return StartCoroutine(ExitPhase());
        }
        // Case 3: 적당
        else
        {
            SayToSelf("음, 이거 사야지");
            targetShop.TryTakeItemFromStand(targetSlotIndex, itemToBuyAmount);
            GameObject item = Instantiate(itemToBuy.dropPrefab, itemCarryPoint.transform.position, Quaternion.identity, itemCarryPoint.transform
);
            item.GetComponent<WorldItem>().enabled = false;

            yield return new WaitForSeconds(1.0f);
            yield return StartCoroutine(QueueAndTransactionPhase());
        }
    }

    // 카운터 줄 서기 및 대기
    private IEnumerator QueueAndTransactionPhase()
    {
        // 줄 서기 시도
        Vector3? targetPos = counter.JoinQueue(this);

        // 자리가 꽉참
        if (targetPos == null)
        {
            SayToSelf("줄 너무 긴데... 그냥 가야겠다");
            //TODO: 손에 든 물건 버리기 DropItemOnFloor

            yield return new WaitForSeconds(2.0f);

            // 퇴장
            yield return StartCoroutine(ExitPhase());
            yield break; // 이 코루틴 종료
        }

        // 자리가 있으면 할당받은 위치로 설정
        currentQueueTarget = targetPos.Value;

        controller.Movement.MoveTo(currentQueueTarget);
        while (!controller.Movement.HasArrived())
        {
            yield return null;
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, currentQueueTargetRotation, Time.deltaTime * 5f);

        if (isFrontOfQueue && !IsReadyForTransaction)
        {

            IsReadyForTransaction = true;
            SayToSelf("계산좀");
        }
        
    }

    // 퇴장 로직
    private IEnumerator ExitPhase()
    {
        // 줄을 서 있었다면 명단에서 제외
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

    // 카운터에서 호출하는 줄 위치 업데이트 함수
    public void UpdateQueueTarget(Vector3 newPos, Quaternion newRot, bool isFront)
    {
        currentQueueTarget = newPos;
        currentQueueTargetRotation = newRot;
        isFrontOfQueue = isFront;
    }

    // 플레이어가 카운터 클릭 시 호출
    public void StartDialogueWithPlayer()
    {

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

    Vector3 GetRandomPointOnNavMesh()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        int index = Random.Range(0, navMeshData.indices.Length / 3) * 3;

        Vector3 v1 = navMeshData.vertices[navMeshData.indices[index]];
        Vector3 v2 = navMeshData.vertices[navMeshData.indices[index + 1]];
        Vector3 v3 = navMeshData.vertices[navMeshData.indices[index + 2]];

        float r1 = Random.value;
        float r2 = Random.value;

        if (r1 + r2 >= 1f)
        {
            r1 = 1 - r1;
            r2 = 1 - r2;
        }

        return v1 + r1 * (v2 - v1) + r2 * (v3 - v1);
    }

}