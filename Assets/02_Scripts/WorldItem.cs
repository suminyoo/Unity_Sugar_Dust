using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public ItemData itemData; // 이 오브젝트가 어떤 아이템인지 정보
    public int amount = 1;    // 몇 개가 들어있는지

    // 아이템 데이터를 기반으로 외형을 초기화하는 함수 (생성 시 호출)
    public void Initialize(ItemData data, int count)
    {
        itemData = data;
        amount = count;
        // 필요하다면 여기서 3D 모델을 교체하거나 크기를 조정하는 로직 추가 가능
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