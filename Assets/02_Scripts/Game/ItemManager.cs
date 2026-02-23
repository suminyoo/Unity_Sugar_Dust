using UnityEngine;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    // itemID: Key, ItemData: Value
    private Dictionary<string, ItemData> itemDatabase = new Dictionary<string, ItemData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadAllItems();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAllItems()
    {
        // Resources/Items 폴더 안에 있는 모든 ItemData
        ItemData[] items = Resources.LoadAll<ItemData>("Items");

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.itemID))
            {
                Debug.LogWarning($"[경고] {item.name}의 itemID가 비어있습니다");
                continue;
            }

            // 등록
            if (!itemDatabase.ContainsKey(item.itemID))
            {
                itemDatabase.Add(item.itemID, item);
            }
        }
        Debug.Log($"총 {itemDatabase.Count}개의 아이템 데이터를 성공적으로 불러왔습니다.");
    }

    public ItemData GetItemByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (itemDatabase.TryGetValue(id, out ItemData itemData))
        {
            return itemData;
        }

        Debug.LogError($"[에러] '{id}'를 가진 아이템을 찾을 수 없습니다!");
        return null;
    }
}