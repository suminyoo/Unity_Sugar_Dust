using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Shop Data")]
public class ShopData : ScriptableObject
{
    public string shopName;
    public List<ItemData> itemsForSale; // 판매 아이템 목록
}