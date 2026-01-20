using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CustomerBrain : NPCBrain
{
    [Header("Settings")]
    public float thinkingTime = 2.0f;
    [Range(0, 100)] public int baseBuyChance = 50;

    public Transform entrancePoint; // 상점 입구 좌표

    private DisplayStand targetShop;
    private int targetSlotIndex = -1;

    protected override void Start()
    {
        base.Start();
        entrancePoint = GameObject.FindGameObjectWithTag("EntrancePoint").transform;
        GameObject shopObj = GameObject.FindGameObjectWithTag("DisplayStand");
        if (shopObj != null)
        {
            targetShop = shopObj.GetComponent<DisplayStand>();
        }
        else
        {
            Debug.LogError("진열대가 없습니다");
        }

        StartCoroutine(CustomerRoutine());
    }

    // 행동 루틴
    private IEnumerator CustomerRoutine()
    {
        // 배회
        yield return StartCoroutine(WanderPhase());

        // 탐색
        Transform itemPos = null;
        bool itemFound = false;

        if (targetShop != null)
        {
            // 진열대의 랜덤 아이템 받아오기 함수
            itemFound = targetShop.TryGetRandomSellableItem(out targetSlotIndex, out itemPos);
        }

        if (itemFound && itemPos != null)
        {
            // 이동
            yield return StartCoroutine(MoveToItemPhase(itemPos));

            // 결정
            yield return StartCoroutine(ThinkingPhase(itemPos));
        }
        else
        {
            SayToSelf("여긴 살만한게 없네");
            yield return new WaitForSeconds(2f);
        }

        // 퇴장 
        // 입구 좌표로 이동 후 Destroy
        yield return StartCoroutine(ExitPhase());

        // (임시) 다시 루프 돌기
        yield return new WaitForSeconds(5f);

        StartCoroutine(CustomerRoutine());
    }

    #region 행동 단계별 로직 (Phases)

    private IEnumerator WanderPhase()
    {
        // 상점 주변 랜덤 위치 배회 (예시: 상점 반경 5m)
        Vector3 wanderTarget = GetRandomPointAround(targetShop.transform.position, 5f);

        controller.Movement.MoveTo(wanderTarget);

        // 도착할 때까지 대기
        while (!controller.Movement.HasArrived())
        {
            yield return null;
        }

    }

    private IEnumerator MoveToItemPhase(Transform itemPos)
    {
        // 물건 위치로
        controller.Movement.MoveTo(itemPos.position);

        while (!controller.Movement.HasArrived())
        {
            yield return null;
        }
    }

    private IEnumerator ThinkingPhase(Transform itemPos)
    {
        // 물건 바라보기
        controller.Movement.LookAtTarget(itemPos);
        controller.Movement.Stop();

        // 고민 애니메이션
        controller.Animation.PlayEmotion(NPCAnimation.Emotion.LookDown);

        SayToSelf("음... 이걸 살까?");
        yield return new WaitForSeconds(thinkingTime);

        // 가격 정보
        var slot = targetShop.InventorySystem.slots[targetSlotIndex];
        int basePrice = slot.itemData.sellPrice; // 원가
        int sellingPrice = targetShop.GetSlotPrice(targetSlotIndex); // 판매가

        // 구매 로직
        if (DecideToBuy(basePrice, sellingPrice))
        {
            BuyItem(sellingPrice);
        }
        else
        {
            RejectItem(sellingPrice);
        }
    }
    private IEnumerator ExitPhase()
    {
        if (entrancePoint == null)
        {
            Debug.LogError("입구 좌표(EntrancePoint)가 설정되지 않았습니다.");
            yield break;
        }

        // 입구로 이동
        controller.Movement.MoveTo(entrancePoint.position);

        // 도착할 때까지 대기
        while (!controller.Movement.HasArrived())
        {
            yield return null;
        }

        SayToSelf("다음에 또 와야지.");
        controller.Movement.Stop();

        yield return new WaitForSeconds(2f);
    }

    #endregion

    #region 구매 로직

    private bool DecideToBuy(int basePrice, int currentPrice)
    {
        if (basePrice == 0) return true;

        float ratio = (float)currentPrice / (float)basePrice;
        int chance = 0;

        if (ratio <= 0) chance = 100;                 // 공짜
        else if (ratio <= 0.8f) chance = 100; // 혜자
        else if (ratio <= 1.1f) chance = 80;  // 적정
        else if (ratio <= 1.5f) chance = 40;  // 조금 비쌈
        else chance = 5;                               // 매우 비쌈


        int roll = Random.Range(0, 100);
        return roll < chance;
    }

    private void BuyItem(int price)
    {
        bool success = targetShop.TrySellItemToNPC(targetSlotIndex);
        if (success)
        {
            //controller.Animation.PlayEmotion(NPCAnimation.Emotion.Happy);
            SayToSelf($"좋아! {price}골드에 잘 산거 같아!");
        }
        else
        {
            // 고민하는 사이에 누가 사감
            controller.Animation.PlayEmotion(NPCAnimation.Emotion.Disappointed);
            SayToSelf("아니 벌써 팔렸잖아!");
        }
    }

    private void RejectItem(int price)
    {
        // 안 삼
        controller.Animation.PlayEmotion(NPCAnimation.Emotion.Surprised);
        SayToSelf($"{price}골드? 너무 비싸서 안살래.");
    }

    #endregion

    // 랜덤 위치
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
}