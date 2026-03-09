using UnityEngine;
using System.Collections.Generic;

public enum ItemID //순서나 이름 수정 불가
{
    None,
    Blue_Cube,
    Blue_Liquid,
    Blue_Core,
    Blue_Eye,
    Blue_Heart,
    Blue_Tentacles,
    Green_Cube,
    Green_Liquid,
    Green_Core,
    Star_Cap,
    Star_Shell,
    Star_Tooth,
    Red_Cube,
    Red_Liquid,
    Red_Core,
    Pink_Eye,
    Pink_Pocket,
    Pink_Tear,
    Health_Recover1,
    Health_Recover2,
    Health_Recover3,
    Stamina_Recover1,
    Stamina_Recover2,
    Stamina_Recover3,
    SugarDust1_Purple,
    SugarDust2_Blue,
    SugarDust3_Green,
    SugarDust4_Skyblue,
    SugarDust5_Pink,
    SugarDust6_Red,
    SugarDust7_White,
    SugarDust8_Rainbow,
    Health_LevelUp,
    Stamina_LevelUp


}

public class ItemDataManager : MonoBehaviour
{
    public static ItemDataManager Instance;

    // itemID: Key, ItemData: Value
    private Dictionary<ItemID, ItemData> itemDatabase = new Dictionary<ItemID, ItemData>();

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
        ItemData[] items = Resources.LoadAll<ItemData>("Item");

        foreach (var item in items)
        {
            if(item.itemID == ItemID.None)
            {
                Debug.LogWarning($"{item.name}의 itemID가 None으로 설정되어 있습니다.");
                continue;
            }

            if (!itemDatabase.ContainsKey(item.itemID))
            {
                itemDatabase.Add(item.itemID, item);
            }
            else
            {
                Debug.LogError($"중복된 ID 발견: {item.itemID} (파일: {item.name})");
            }
        }
        //Debug.Log($"총 {itemDatabase.Count}개의 아이템 데이터를 성공적으로 불러왔습니다.");
    }

    public ItemData GetItemByID(ItemID id)
    {
        if (id == ItemID.None) return null;

        if (itemDatabase.TryGetValue(id, out ItemData itemData))
        {
            return itemData;
        }

        Debug.LogError($"'{id}'를 가진 아이템을 찾을 수 없습니다!");
        return null;
    }

    public ItemData GetItemByID(string idString)
    {
        if (string.IsNullOrEmpty(idString)) return null;

        if (System.Enum.TryParse(idString, out ItemID id))
        {
            return GetItemByID(id);
        }

        Debug.LogError($"'{idString}'은 유효한 ItemID 형식이 아닙니다.");
        return null;
    }
}