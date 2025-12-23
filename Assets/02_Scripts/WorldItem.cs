using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class WorldItem : MonoBehaviour
{
    public ItemData itemData; // 아이템 SO
    public int amount = 1;    // 들어있는 개수

    private Transform target; //플레이어
    public float moveSpeed = 10f;

    public void Initialize(ItemData data, int count)
    {
        itemData = data;
        amount = count;
    }

    public void StartFollow(Transform player)
    {
        target = player;
    }
    public void StopFollow(Transform player)
    {
        if (target == player)
            target = null;
    }


    private void Update()
    {
        if (target == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어의 인벤토리를 찾아 아이템 추가 시도
            InventorySystem inventory = other.GetComponent<InventorySystem>();

            if (inventory != null)
            {
                bool success = inventory.AddItem(itemData, amount);
                if (success)
                {
                    Debug.Log($"{itemData.itemName} {amount}개 획득!");
                    // 획득 사운드 재생
                    Destroy(gameObject); // 맵에서 삭제
                }
                else
                {
                    Debug.Log("인벤토리가 가득 찼거나 무게가 너무 무겁습니다.");
                }
            }
        }
    }
}