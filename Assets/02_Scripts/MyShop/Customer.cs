using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Customer : MonoBehaviour
{
    public enum State { Wander, MoveToItem, Thinking, Leave }

    [Header("Settings")]
    public float moveSpeed = 3.5f;
    public float thinkTime = 2.0f;     // 물건 보고 고민하는 시간
    [Range(0, 100)]
    public int buyChance = 50;         // 구매 확률 (50%)

    [Header("Refs")]
    [SerializeField] private NavMeshAgent agent;
    private DisplayStand targetShop;   // 목표 상점

    // 내부 상태 변수
    private State currentState;
    private int targetSlotIndex = -1;  // 사려고 찜한 슬롯 번호
    private int targetItemPrice = 0;  // 보러 갈 아이템의 가격

    private void Start()
    {
        targetShop = GameObject.FindGameObjectWithTag("DisplayStand").GetComponent<DisplayStand>();

        agent.speed = moveSpeed;
        StartCoroutine(FSM());
    }

    private IEnumerator FSM()
    {
        // Wander
        currentState = State.Wander;
        Vector3 wanderPos = GetRandomPointAround(targetShop.transform.position, 5f);
        agent.SetDestination(wanderPos);

        // 이동하는 동안 대기
        while (agent.pathPending || agent.remainingDistance > 0.5f) yield return null;

        // 물건 탐색 (랜덤고르기)
        yield return new WaitForSeconds(1f); // 잠시 숨 고르기

        Transform itemPos;
        // 진열대에게 물건 위치요청
        if (targetShop != null && targetShop.TryGetRandomSellableItem(out targetSlotIndex, out itemPos))
        {
            // 아이템의 가격 데이터
            var slot = targetShop.InventorySystem.slots[targetSlotIndex];
            int basePrice = slot.itemData.sellPrice;

            // 플레이어가 설정한 가격 데이터
            int sellingPrice = targetShop.GetSlotPrice(targetSlotIndex);

            // 물건이 있다면 이동
            currentState = State.MoveToItem;
            agent.SetDestination(itemPos.position);

            // 이동 대기
            while (agent.pathPending || agent.remainingDistance > 1.0f) yield return null;

            // 고민 
            currentState = State.Thinking;
            transform.LookAt(itemPos); // 물건 바라보기

            // 말풍선 UI 등 띄우는 타이밍
            yield return new WaitForSeconds(thinkTime);

            if (DecideToBuy(basePrice, sellingPrice))
            {
                // 구매 시도
                bool success = targetShop.TrySellItemToNPC(targetSlotIndex);
                if (success)
                {
                    Debug.Log($"NPC: 오! {targetItemPrice}원이면 살만하네. (구매함)");
                    // 샀어 애니메이션
                }
                else
                {
                    Debug.Log("NPC: 아, 누가 채갔나보네. (품절)");
                }
            }
            else
            {
                Debug.Log($"NPC: {targetItemPrice}원? 너무 비싼데.. (안 삼)");
                // 안사 애니메이션
            }
        }
        else
        {
            Debug.Log("NPC: 살 물건이 하나도 없네..");
        }

        // 퇴장 혹은 다시 배회 (우선 배회로 루프)
        // 구현시에는 입구로 나가서 Destroy
        yield return new WaitForSeconds(1f);
        StartCoroutine(FSM());
    }


    private bool DecideToBuy(int basePrice, int currentPrice)
    {
        float ratio = (float)currentPrice / (float)basePrice;

        int buyChance = 0;
        if (ratio <= 0) // 공짜
        {
            buyChance = 100; // 무조건 구매
            Debug.Log("이거 공짜에요?");

        }
        else if (ratio <= 0.8f) // 원가보다 20% 이상 싸다
        {
            buyChance = 100; // 무조건 구매
            Debug.Log("NPC: 와! 득템이다!");
        }
        else if (ratio <= 1.1f) // 적정가: 원가 ~ 10% 비쌈
        {
            buyChance = 80; // 80% 확률로 구매
            Debug.Log("NPC: 음, 합리적인 가격이네.");
        }
        else if (ratio <= 1.5f) // 약간 비쌈: 1.1 ~ 1.5배
        {
            buyChance = 40; // 40% 확률로 구매 (고민 많이 함)
            Debug.Log("NPC: 좀 비싼데... 고민되네.");
        }
        else // 완전 비쌈: 1.5배 이상
        {
            buyChance = 5; // 5% 확률 (호구 NPC만 구매)
            Debug.Log("NPC: 너무 비싸! 날 호구로 아나?");
        }


        // 주사위 굴리기
        int roll = Random.Range(0, 100);
        return roll < buyChance;
    }

    // 주변 랜덤 위치 찾기
    private Vector3 GetRandomPointAround(Vector3 center, float range)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomPoint, out hit, range, NavMesh.AllAreas);
        return hit.position;
    }
}