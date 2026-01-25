using System;
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
    //Thief,          // 도둑
    //Impatient,      // 빨리 계산안하면 감
}

public class CustomerBrain : NPCBrain
{
    [Header("References")]
    private DisplayStand targetShop;
    private CheckoutCounter counter; // 카운터 참조
    private Transform entrancePoint; // 상점 입구 좌표
    private Action onDespawnCallback; // 파괴 시 콜백

    public GameObject itemCarryPoint;
    public CustomerDialogueDatabase dialogueDatabase;

    [Header("Settings")]
    [SerializeField] private CustomerType myType; // 내 진상 유형
    private Coroutine moveCoroutine;

    private Vector3 currentQueueTargetPosition;
    private Quaternion currentQueueTargetRotation;

    private bool isFrontOfQueue = false;
    private float wanderTime;
    private float thinkingTime = 2.5f;
    private int targetItemSlotIndex;
    private int minItemPickupAmount = 1;
    private int maxItemPickupAmount = 20;

    private ItemData itemToBuy;
    private int itemToBuyAmount; 
    private int itemToBuyPrice;
    public ItemData ItemToBuy => itemToBuy;
    public int ItemToBuyAmount => itemToBuyAmount;
    public int ItemToBuyPrice => itemToBuyPrice;

    public bool IsInQueue => currentQueueTargetPosition != Vector3.zero; // 줄 섰는지
    public bool IsReadyForTransaction = false;

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
    public void Setup(DisplayStand shop, CheckoutCounter checkout, Transform entrance,
                          CustomerType type, float duration, Action onDespawn)
    {
        this.targetShop = shop;
        this.counter = checkout;
        this.entrancePoint = entrance;

        this.myType = type;
        this.wanderTime = duration;

        this.onDespawnCallback = onDespawn;

        IsReadyForTransaction = false;

        StartCoroutine(CustomerInitialRoutine());
    }


    #region 행동 단계별 로직 (Phases)
    private IEnumerator CustomerInitialRoutine()
    {
        yield return StartCoroutine(WanderPhase());

        Transform itemPos = null;
        bool itemFound = false;

        itemFound = targetShop.TryGetRandomSellableItem(out targetItemSlotIndex, out itemPos);

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

    // 배회 로직
    private IEnumerator WanderPhase()
    {
        float timer = 0f;

        // 지정된 시간 동안 랜덤배회
        while (timer < wanderTime)
        {
            if (controller.Movement.HasArrived())
            {
                // 잠깐 멈춤
                controller.Movement.Stop();

                float waitTime = UnityEngine.Random.Range(2f, 4f);
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
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * stoppingDistance;
        Vector3 randomOffset = new Vector3(randomCircle.x, 0, randomCircle.y);

        Vector3 finalTarget = itemPos.position + randomOffset;

        controller.Movement.MoveTo(finalTarget);
        while (!controller.Movement.HasArrived())
        {
            yield return null;
        }
    }

    // 집기 및 가격 판단
    private IEnumerator PickUpItemPhase(Transform itemPos)
    {
        // 고민
        controller.Movement.LookAtTarget(itemPos);
        controller.Animation.PlayEmotion(NPCAnimation.Emotion.LookDown); 

        yield return new WaitForSeconds(thinkingTime);

        // 정보 확인
        var slot = targetShop.InventorySystem.slots[targetItemSlotIndex];
        if (slot.IsEmpty || slot.itemData == null)
        {
            SayToSelf("어? 물건이 그새 없어졌네");
            yield return new WaitForSeconds(1.0f);
            yield return StartCoroutine(WanderPhase());
            yield break;
        }
        ItemData potentialItem = slot.itemData;
        int potentialPrice = targetShop.GetSlotPrice(targetItemSlotIndex);
        float ratio = (potentialItem.sellPrice == 0) ? 0 : (float)potentialPrice / potentialItem.sellPrice;

        // Case A: 너무 비쌈 (> 2배) -> 안 삼
        if (ratio > 2.0f && potentialPrice > 0)
        {
            SayToSelf($"{potentialPrice}골드? 완전 바가지잖아!");

            // 재고를 건드리지 않고 바로 퇴장
            yield return new WaitForSeconds(1.5f);
            yield return StartCoroutine(ExitPhase());
        }
        // Case B: 공짜거나 가격이 적당함
        else
        {
            // 재고 차감 시도
            int desiredAmount = UnityEngine.Random.Range(minItemPickupAmount, maxItemPickupAmount);
            itemToBuyAmount = targetShop.TryTakeItemFromStand(targetItemSlotIndex, desiredAmount);

            if (itemToBuyAmount > 0)
            {
                itemToBuy = potentialItem;
                itemToBuyPrice = potentialPrice;

                if (itemToBuy.dropPrefab != null)
                {
                    GameObject itemObj = Instantiate(itemToBuy.dropPrefab, itemCarryPoint.transform.position, Quaternion.identity, itemCarryPoint.transform);
                    if (itemObj.GetComponent<WorldItem>() != null)
                        itemObj.GetComponent<WorldItem>().enabled = false;
                }

                if (itemToBuyPrice == 0)
                {
                    SayToSelf("와 공짜네? 아싸!");
                    yield return new WaitForSeconds(1.5f);
                    yield return StartCoroutine(ExitPhase());
                }
                else
                {
                    SayToSelf("음, 가격 괜찮네. 사야지.");
                    yield return new WaitForSeconds(1.0f);

                    yield return StartCoroutine(QueueAndTransactionPhase());
                }
            }
            else
            {
                SayToSelf("어? 재고가 없네...");
                yield return new WaitForSeconds(1.0f);
                yield return StartCoroutine(ExitPhase());
            }
        }
    }

    // 카운터 줄 서기 및 대기
    private IEnumerator QueueAndTransactionPhase()
    {
        // 줄 서기 등록
        var queueInfo = counter.JoinQueue(this);

        if (queueInfo == null)
        {
            SayToSelf("줄 너무 긴데... 그냥 가야겠다");
            // TODO: 물건 버리기
            yield return new WaitForSeconds(2.0f);
            yield return StartCoroutine(ExitPhase());
            yield break;
        }

        isFrontOfQueue = (counter.waitingQueue.IndexOf(this) == 0);
        UpdateQueueTarget(queueInfo.Value.position, queueInfo.Value.rotation, isFrontOfQueue);

        // 해당 위치로 이동
        yield return StartCoroutine(MoveToQueuePosRoutine());
    }
    
    // 줄 땡기기
    private IEnumerator MoveToQueuePosRoutine()
    {
        // 이동
        controller.Movement.MoveTo(currentQueueTargetPosition);
        yield return null;

        while (!controller.Movement.HasArrived())
        {
            yield return null;
        }
        controller.Movement.Stop();

        // 회전
        float rotateTimer = 0f;
        while (rotateTimer < 1.0f) //1초
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, currentQueueTargetRotation, Time.deltaTime * 5f);
            rotateTimer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = currentQueueTargetRotation;

        // 맨 앞자리라면 거래 준비 완료
        if (isFrontOfQueue)
        {
            IsReadyForTransaction = true;
            SayToSelf("계산해주세요");
        }
    }

    // 퇴장 로직
    private IEnumerator ExitPhase()
    {
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

        onDespawnCallback?.Invoke();
    }

    #endregion

    #region 상호작용 및 이벤트

    // 카운터에서 호출하는 줄 위치 업데이트 함수
    public void UpdateQueueTarget(Vector3 newPos, Quaternion newRot, bool isFront)
    {
        // 정보 갱신
        currentQueueTargetPosition = newPos;
        currentQueueTargetRotation = newRot;
        isFrontOfQueue = isFront;

        IsReadyForTransaction = false;

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveToQueuePosRoutine());
    }

    public void StartTransactionDialogue()
    {
        if (!IsReadyForTransaction) return;

        controller.Movement.Stop();
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        DialogueData selectedDialogue = null;

        selectedDialogue = dialogueDatabase.GetDialogueByType(myType);

        if (selectedDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(selectedDialogue, controller.npcData.npcName, null);
        }
        else
        {
            Debug.Log("대사가 없습니다");
        }
    }

    public void OnTransactionDialogueFinished(bool isSuccess)
    {
        DialogueManager.Instance.EndDialogue();

        if (isSuccess) SayToSelf("감사합니다.");
        else SayToSelf("쳇 뭐야.");
        
        counter.LeaveQueue(this);

        StartCoroutine(ExitPhase());
    }

    #endregion

    Vector3 GetRandomPointOnNavMesh()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        int index = UnityEngine.Random.Range(0, navMeshData.indices.Length / 3) * 3;

        Vector3 v1 = navMeshData.vertices[navMeshData.indices[index]];
        Vector3 v2 = navMeshData.vertices[navMeshData.indices[index + 1]];
        Vector3 v3 = navMeshData.vertices[navMeshData.indices[index + 2]];

        float r1 = UnityEngine.Random.value;
        float r2 = UnityEngine.Random.value;

        if (r1 + r2 >= 1f)
        {
            r1 = 1 - r1;
            r2 = 1 - r2;
        }

        return v1 + r1 * (v2 - v1) + r2 * (v3 - v1);
    }

}